using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public FaviconManager(IPluginHost host, ConfigOpt config)
        {
            _host = host;
            _config = config;
        }

        private sealed class DownloadProgress
        {
            public int Success;
            public int NotFound;
            public int Error;
            public int Total;
            public int Completed;
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

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += delegate(object sender, DoWorkEventArgs e)
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

                Queue<PwEntry> queue = new Queue<PwEntry>(entries);
                object queueLock = new object();
                List<FaviconDownloader> activeDownloaders = new List<FaviconDownloader>();
                object activeLock = new object();

                int activeWorkers = 0;
                const int maxConcurrency = 4;
                bool cancelRequested = false;

                while (true)
                {
                    // Check user cancellation via KeePass Status dialog Cancel button
                    if (!logger.ContinueWork() || worker.CancellationPending || cancelRequested)
                    {
                        cancelRequested = true;
                        e.Cancel = true;

                        lock (queueLock)
                        {
                            queue.Clear();
                        }

                        lock (activeLock)
                        {
                            for (int a = 0; a < activeDownloaders.Count; a++)
                            {
                                try { activeDownloaders[a].Abort(); } catch { }
                            }
                        }
                    }

                    // Dispatch new work items up to concurrency limit
                    while (!cancelRequested)
                    {
                        PwEntry nextEntry = null;
                        lock (queueLock)
                        {
                            if (activeWorkers < maxConcurrency && queue.Count > 0)
                            {
                                nextEntry = queue.Dequeue();
                                Interlocked.Increment(ref activeWorkers);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (nextEntry == null) break;

                        ThreadPool.QueueUserWorkItem(delegate(object state)
                        {
                            PwEntry entry = (PwEntry)state;
                            FaviconDownloader fd = new FaviconDownloader(proxy);

                            lock (activeLock)
                            {
                                if (cancelRequested)
                                {
                                    try { fd.Dispose(); } catch { }
                                    Interlocked.Decrement(ref activeWorkers);
                                    return;
                                }
                                activeDownloaders.Add(fd);
                            }

                            try
                            {
                                if (cancelRequested) return;

                                string url = entry.Strings.ReadSafe(PwDefs.UrlField);
                                if (string.IsNullOrEmpty(url) && useTitle)
                                {
                                    url = entry.Strings.ReadSafe(PwDefs.TitleField);
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
                                    catch (FaviconDownloaderException fex)
                                    {
                                        if (fex.Status == FaviconErrorStatus.NotFound)
                                            Interlocked.Increment(ref progress.NotFound);
                                        else
                                            Interlocked.Increment(ref progress.Error);
                                    }
                                    catch
                                    {
                                        Interlocked.Increment(ref progress.Error);
                                    }

                                    if (!cancelRequested && iconBytes != null && iconBytes.Length > 0)
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

                                            if (!entry.CustomIconUuid.Equals(uuid))
                                            {
                                                entry.CustomIconUuid = uuid;
                                                if (updateModified)
                                                {
                                                    entry.LastModificationTime = DateTime.UtcNow;
                                                }
                                                entry.Touch(true, false);
                                            }
                                        }

                                        Interlocked.Increment(ref progress.Success);
                                    }
                                }
                            }
                            finally
                            {
                                lock (activeLock)
                                {
                                    activeDownloaders.Remove(fd);
                                    try { fd.Dispose(); } catch { }
                                }

                                Interlocked.Decrement(ref activeWorkers);

                                int done = Interlocked.Increment(ref progress.Completed);
                                uint pct = (uint)Math.Min(100, (int)Math.Round((double)done * 100 / progress.Total));
                                logger.SetProgress(pct);
                            }
                        }, nextEntry);
                    }

                    // Check exit condition
                    bool finished = false;
                    lock (queueLock)
                    {
                        if ((queue.Count == 0 && Volatile.Read(ref activeWorkers) == 0) || (cancelRequested && Volatile.Read(ref activeWorkers) == 0))
                        {
                            finished = true;
                        }
                    }

                    if (finished) break;

                    Thread.Sleep(50);
                }

                e.Result = progress;
            };

            worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                try
                {
                    _host.MainWindow.UIBlockInteraction(false);
                    if (statusForm != null && !statusForm.IsDisposed)
                    {
                        statusForm.Close();
                    }

                    _host.MainWindow.UpdateUI(false, null, true, null, true, null, true);

                    if (e.Cancelled)
                    {
                        MessageBox.Show("Favicon download was cancelled.", "KeePassNatMsg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (e.Error != null)
                    {
                        MessageBox.Show("Favicon download encountered an error: " + e.Error.Message, "KeePassNatMsg", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    DownloadProgress p = e.Result as DownloadProgress;
                    if (p != null)
                    {
                        string msg = string.Format(
                            "Favicon Download Complete:\n\nSuccess: {0}\nNot Found: {1}\nErrors: {2}\nTotal: {3}",
                            p.Success, p.NotFound, p.Error, p.Total
                        );
                        MessageBox.Show(msg, "KeePassNatMsg Favicon Downloader", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch { }
            };

            worker.RunWorkerAsync();
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
                // KeePass PwUuid is 16 bytes. Use the first 16 bytes of SHA-256
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
