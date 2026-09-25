using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Interfaces;

namespace KeePassNatMsg.Favicon
{
    public sealed class FaviconManager
    {
        private readonly IPluginHost _host;
        private readonly ConfigOpt _config;
        private volatile bool _isRunning;
        private volatile CancellationTokenSource _cts;
        private readonly object _stateLock = new object();
        private readonly List<FaviconDownloader> _activeDownloaders = new List<FaviconDownloader>();

        public FaviconManager(IPluginHost host, ConfigOpt config)
        {
            _host = host;
            _config = config;
        }

        public void Cancel()
        {
            CancellationTokenSource current = _cts;
            if (current == null) return;
            current.Cancel();
            // Network abort and database writers may hold locks; do not wait on
            // either lock on the UI thread.
            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                FaviconDownloader[] downloaders;
                lock (_stateLock)
                {
                    // A previous session must not abort the next session's requests.
                    if (!object.ReferenceEquals(_cts, state)) return;
                    downloaders = _activeDownloaders.ToArray();
                }
                foreach (FaviconDownloader downloader in downloaders)
                    try { downloader.Abort(); } catch { }
            }, current);
        }

        private sealed class DownloadProgress
        {
            public int Success;
            public int NotFound;
            public int Error;
            public int Total;
        }

        public void DownloadFaviconsForEntries(PwEntry[] entries)
        {
            if (_host == null || _host.Database == null || !_host.Database.IsOpen)
            {
                MessageBox.Show("Please open a KeePass database first.", "KeePassNatMsg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (entries == null || entries.Length == 0)
            {
                return;
            }

            if (_isRunning)
            {
                MessageBox.Show("A favicon download is already in progress.", "KeePassNatMsg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _isRunning = true;
            CancellationTokenSource cts = new CancellationTokenSource();
            _cts = cts;
            PwDatabase database = _host.Database;

            Form statusForm;
            IStatusLogger logger = StatusUtil.CreateStatusDialog(
                _host.MainWindow,
                out statusForm,
                "KeePassNatMsg Favicon Downloader",
                "Downloading website favicons...",
                true,
                false
            );

            _host.MainWindow.UIBlockInteraction(true);

            Thread workerThread = new Thread(delegate()
            {
                DownloadProgress progress = new DownloadProgress { Total = entries.Length };
                bool autoPrefix = _config.FaviconPrefixUrls;
                bool useTitle = _config.FaviconUseTitle;
                bool updateModified = _config.FaviconUpdateModified;
                int maxIconSize = _config.FaviconMaxIconSize;
                string providerName = _config.FaviconProvider;
                string customTemplate = _config.FaviconCustomUrl;

                bool isDirect = string.IsNullOrEmpty(providerName) ||
                                providerName.Equals(FaviconProvider.ProviderDirect, StringComparison.OrdinalIgnoreCase);

                string activeTemplate = null;
                if (!isDirect)
                {
                    if (providerName.Equals(FaviconProvider.ProviderCustom, StringComparison.OrdinalIgnoreCase))
                    {
                        activeTemplate = customTemplate;
                    }
                    else
                    {
                        FaviconProvider p = FaviconProvider.FindByName(providerName);
                        activeTemplate = p.UrlTemplate;
                    }
                }

                IWebProxy proxy = WebRequest.DefaultWebProxy;
                CancellationToken ct = cts.Token;

                int done = 0;
                int activeWorkers = 0;

                for (int i = 0; i < entries.Length; i++)
                {
                    if (ct.IsCancellationRequested) break;
                    if (!logger.ContinueWork()) { Cancel(); break; }

                    // Wait for a free worker slot
                    while (Volatile.Read(ref activeWorkers) >= 4)
                    {
                        if (ct.IsCancellationRequested) break;
                        Thread.Sleep(30);
                    }
                    if (ct.IsCancellationRequested) break;

                    PwEntry entry = entries[i];
                    Interlocked.Increment(ref activeWorkers);

                    ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        PwEntry ent = (PwEntry)state;
                        FaviconDownloader fd = new FaviconDownloader(proxy);

                        try
                        {
                            lock (_stateLock)
                            {
                                if (ct.IsCancellationRequested) return;
                                _activeDownloaders.Add(fd);
                            }

                            string url = ent.Strings.ReadSafe(PwDefs.UrlField);
                            if (string.IsNullOrEmpty(url) && useTitle)
                            {
                                url = ent.Strings.ReadSafe(PwDefs.TitleField);
                            }

                            if (string.IsNullOrEmpty(url))
                            {
                                Interlocked.Increment(ref progress.NotFound);
                            }
                            else
                            {
                                byte[] iconBytes = null;
                                try
                                {
                                    if (isDirect)
                                    {
                                        iconBytes = fd.DownloadFaviconDirect(url, autoPrefix, maxIconSize);
                                    }
                                    else
                                    {
                                        iconBytes = fd.DownloadFaviconFromProvider(activeTemplate, url, maxIconSize);
                                    }
                                }
                                catch (FaviconDownloaderException)
                                {
                                    Interlocked.Increment(ref progress.Error);
                                }
                                catch
                                {
                                    Interlocked.Increment(ref progress.Error);
                                }

                                if (!ct.IsCancellationRequested && iconBytes != null && iconBytes.Length > 0)
                                {
                                    byte[] hash = ComputeSha256(iconBytes);
                                    PwUuid uuid = new PwUuid(hash);

                                    lock (_stateLock)
                                    {
                                        if (!ct.IsCancellationRequested && database.IsOpen)
                                        {
                                            lock (database)
                                            {
                                                if (ct.IsCancellationRequested || !database.IsOpen) return;
                                                bool exists = false;
                                                for (int ci = 0; ci < database.CustomIcons.Count; ci++)
                                                {
                                                    if (database.CustomIcons[ci].Uuid.Equals(uuid))
                                                    {
                                                        exists = true;
                                                        break;
                                                    }
                                                }

                                                if (!exists)
                                                {
                                                    PwCustomIcon customIcon = new PwCustomIcon(uuid, iconBytes);
                                                    AttachIconMetadata(customIcon, url);
                                                    database.CustomIcons.Add(customIcon);
                                                }

                                                if (!ent.CustomIconUuid.Equals(uuid))
                                                {
                                                    ent.CustomIconUuid = uuid;
                                                    if (updateModified)
                                                        ent.LastModificationTime = DateTime.UtcNow;
                                                    ent.Touch(true, false);
                                                }
                                                database.Modified = true;
                                            }
                                            Interlocked.Increment(ref progress.Success);
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            lock (_stateLock) { _activeDownloaders.Remove(fd); }
                            try { fd.Dispose(); } catch { }
                            int d = Interlocked.Increment(ref done);
                            uint pct = (uint)Math.Min(100, (int)Math.Round((double)d * 100 / Math.Max(1, progress.Total)));
                            if (!ct.IsCancellationRequested)
                                try { logger.SetProgress(pct); } catch { }
                            Interlocked.Decrement(ref activeWorkers);
                        }
                    }, entry);
                }

                // Wait for all workers, with hard timeout: 60s after cancel, 300s total
                DateTime hardDeadline = DateTime.UtcNow.AddSeconds(300);
                while (Volatile.Read(ref activeWorkers) > 0)
                {
                    if (ct.IsCancellationRequested)
                    {
                        // After cancel, only wait 10 seconds max for workers to unwind
                        DateTime cancelDeadline = DateTime.UtcNow.AddSeconds(10);
                        while (Volatile.Read(ref activeWorkers) > 0 && DateTime.UtcNow < cancelDeadline)
                        {
                            Thread.Sleep(50);
                        }
                        break; // Force-break regardless
                    }
                    if (DateTime.UtcNow > hardDeadline) { Cancel(); break; }
                    Thread.Sleep(50);
                }

                // UI controls belong to the main thread. Never synchronously Invoke
                // from here: the UI may itself be waiting for cancellation.
                try
                {
                    _host.MainWindow.BeginInvoke((MethodInvoker)delegate
                    {
                        try
                        {
                            try { _host.MainWindow.UIBlockInteraction(false); } catch { }
                        }
                        finally
                        {
                            try
                            {
                                if (statusForm != null && !statusForm.IsDisposed) statusForm.Close();
                            }
                            catch { }
                            finally
                            {
                                try { _host.MainWindow.UpdateUI(false, null, true, null, true, null, true); } catch { }
                            }
                        }
                        lock (_stateLock)
                        {
                            if (object.ReferenceEquals(_cts, cts)) _cts = null;
                            _isRunning = false;
                        }
                        if (ct.IsCancellationRequested)
                            MessageBox.Show("Favicon download was cancelled.", "KeePassNatMsg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBox.Show(string.Format(
                                "Favicon Download Complete:\n\nSuccess: {0}\nNot Found: {1}\nErrors: {2}\nTotal: {3}",
                                progress.Success, progress.NotFound, progress.Error, progress.Total),
                                "KeePassNatMsg Favicon Downloader", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
                catch
                {
                    try
                    {
                        if (!_host.MainWindow.IsDisposed && _host.MainWindow.IsHandleCreated)
                            _host.MainWindow.BeginInvoke((MethodInvoker)delegate
                            {
                                _host.MainWindow.UIBlockInteraction(false);
                                if (statusForm != null && !statusForm.IsDisposed) statusForm.Close();
                            });
                    }
                    catch { }
                    lock (_stateLock)
                    {
                        if (object.ReferenceEquals(_cts, cts)) _cts = null;
                        _isRunning = false;
                    }
                }
            });

            workerThread.IsBackground = true;
            workerThread.Start();
        }

        public void DownloadFaviconsForGroup(PwGroup group)
        {
            if (group == null) return;
            List<PwEntry> entries = new List<PwEntry>();
            foreach (PwEntry entry in group.GetEntries(true))
            {
                entries.Add(entry);
            }
            DownloadFaviconsForEntries(entries.ToArray());
        }

        private static byte[] ComputeSha256(byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] full = sha.ComputeHash(data);
                byte[] uuidBytes = new byte[16];
                Array.Copy(full, 0, uuidBytes, 0, 16);
                return uuidBytes;
            }
        }

        private static void AttachIconMetadata(PwCustomIcon icon, string url)
        {
            try
            {
                Type type = icon.GetType();
                PropertyInfo nameProp = type.GetProperty("Name");
                if (nameProp != null)
                {
                    string host = FaviconDownloader.ExtractHostname(url);
                    if (string.IsNullOrEmpty(host)) host = "site";
                    nameProp.SetValue(icon, "natmsg-" + host, null);
                }

                PropertyInfo modProp = type.GetProperty("LastModificationTime");
                if (modProp != null)
                {
                    modProp.SetValue(icon, DateTime.UtcNow, null);
                }
            }
            catch { }
        }
    }
}
