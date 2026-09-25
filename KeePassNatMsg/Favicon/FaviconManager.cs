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
        private CancellationTokenSource _cts;

        public FaviconManager(IPluginHost host, ConfigOpt config)
        {
            _host = host;
            _config = config;
        }

        public void Cancel()
        {
            if (_cts != null)
            {
                try { _cts.Cancel(); } catch { }
            }
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
            _cts = new CancellationTokenSource();

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
                CancellationToken ct = _cts.Token;

                int done = 0;
                int activeWorkers = 0;

                for (int i = 0; i < entries.Length; i++)
                {
                    if (ct.IsCancellationRequested) break;
                    if (!logger.ContinueWork()) { _cts.Cancel(); break; }

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
                            if (ct.IsCancellationRequested) return;

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

                                    lock (_host.Database)
                                    {
                                        bool exists = false;
                                        for (int ci = 0; ci < _host.Database.CustomIcons.Count; ci++)
                                        {
                                            if (_host.Database.CustomIcons[ci].Uuid.Equals(uuid))
                                            {
                                                exists = true;
                                                break;
                                            }
                                        }

                                        if (!exists)
                                        {
                                            PwCustomIcon customIcon = new PwCustomIcon(uuid, iconBytes);
                                            AttachIconMetadata(customIcon, url);
                                            _host.Database.CustomIcons.Add(customIcon);
                                        }

                                        if (!ent.CustomIconUuid.Equals(uuid))
                                        {
                                            ent.CustomIconUuid = uuid;
                                            if (updateModified)
                                            {
                                                ent.LastModificationTime = DateTime.UtcNow;
                                            }
                                            ent.Touch(true, false);
                                        }
                                    }

                                    Interlocked.Increment(ref progress.Success);
                                }
                            }
                        }
                        finally
                        {
                            try { fd.Dispose(); } catch { }
                            Interlocked.Decrement(ref activeWorkers);
                            int d = Interlocked.Increment(ref done);
                            uint pct = (uint)Math.Min(100, (int)Math.Round((double)d * 100 / Math.Max(1, progress.Total)));
                            try { logger.SetProgress(pct); } catch { }
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
                    if (DateTime.UtcNow > hardDeadline) break;
                    Thread.Sleep(50);
                }

                // === UI cleanup — ALWAYS runs, even if workers are stuck ===
                try
                {
                    _host.MainWindow.UIBlockInteraction(false);
                }
                catch { }

                try
                {
                    if (statusForm != null && !statusForm.IsDisposed)
                    {
                        statusForm.Invoke((MethodInvoker)delegate { statusForm.Close(); });
                    }
                }
                catch { }

                try
                {
                    if (_host != null && _host.Database != null && _host.Database.IsOpen)
                    {
                        _host.Database.Modified = true;
                    }
                }
                catch { }

                try
                {
                    _host.MainWindow.UpdateUI(false, null, true, null, true, null, true);
                }
                catch { }

                // Show result dialog via Invoke to avoid cross-thread issues
                try
                {
                    _host.MainWindow.Invoke((MethodInvoker)delegate
                    {
                        if (ct.IsCancellationRequested)
                        {
                            MessageBox.Show("Favicon download was cancelled.", "KeePassNatMsg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string msg = string.Format(
                                "Favicon Download Complete:\n\nSuccess: {0}\nNot Found: {1}\nErrors: {2}\nTotal: {3}",
                                progress.Success, progress.NotFound, progress.Error, progress.Total
                            );
                            MessageBox.Show(msg, "KeePassNatMsg Favicon Downloader", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    });
                }
                catch { }

                _isRunning = false;
                try { _cts.Dispose(); } catch { }
                _cts = null;
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
