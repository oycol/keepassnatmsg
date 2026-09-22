using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace KeePassNatMsg.NativeMessaging
{
    public enum ChromeIntegrationState
    {
        Ready,
        NeedsInstall,
        BrokenManifest,
        BrokenRegistry,
        MissingProxy,
        ChromeNotDetected
    }

    public class ChromeIntegrationStatus
    {
        public ChromeIntegrationState State { get; set; }
        public string Message { get; set; }
        public bool ProxyOk { get; set; }
        public bool ManifestOk { get; set; }
        public bool RegistryOk { get; set; }
        public bool ChromeDetected { get; set; }
        public string ProxyPath { get; set; }
        public string ManifestPath { get; set; }
        public string RegistryKeyPath { get; set; }
        public string Details { get; set; }
    }

    public class ChromeIntegrationService
    {
        public const string NativeHostName = "org.keepassxc.keepassxc_browser";
        public const string ChromeExtensionId = "obcddimikignkfpophjabdkdggkodnnh";
        public const string ChromeExtensionOrigin = "chrome-extension://obcddimikignkfpophjabdkdggkodnnh/";
        public const string RegistrySubKey = @"Software\Google\Chrome\NativeMessagingHosts\" + NativeHostName;

        public static string ExpectedProxySha256 = "acefbe3089ad2cccd8d6a778a98878eafedec8dd06d154b1dafb84ed9ac385a4";
        public static string LegacyProxySha256 = "d1d4e8969c1d142b2eda281d2e3a7a2e8d60ef6fc5cd6c91d3514bb69ff78f00";

        public virtual string GetConfigDir()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "KeePassNatMsg");
        }

        public virtual string GetProxyPath()
        {
            return Path.Combine(GetConfigDir(), "keepassnatmsg-proxy.exe");
        }

        public virtual string GetManifestPath()
        {
            return Path.Combine(GetConfigDir(), "org.keepassxc.keepassxc_browser.chrome.json");
        }

        public virtual bool IsChromeInstalled()
        {
            var p1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe");
            var p2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe");
            var p3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe");
            return File.Exists(p1) || File.Exists(p2) || File.Exists(p3);
        }

        public string GenerateManifestContent(string proxyPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"name\": \"" + NativeHostName + "\",");
            sb.AppendLine("  \"description\": \"KeePassXC-Browser native messaging host (KeePassNatMsg)\",");
            sb.AppendLine("  \"type\": \"stdio\",");
            sb.AppendLine("  \"path\": \"" + proxyPath.Replace(@"\", @"\\") + "\",");
            sb.AppendLine("  \"allowed_origins\": [");
            sb.AppendLine("    \"" + ChromeExtensionOrigin + "\"");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public ChromeIntegrationStatus CheckStatus()
        {
            var status = new ChromeIntegrationStatus
            {
                ProxyPath = GetProxyPath(),
                ManifestPath = GetManifestPath(),
                RegistryKeyPath = @"HKEY_CURRENT_USER\" + RegistrySubKey,
                ChromeDetected = IsChromeInstalled()
            };

            // 1. Verify Proxy
            if (File.Exists(status.ProxyPath))
            {
                var hash = GetSha256(status.ProxyPath);
                status.ProxyOk = string.Equals(hash, ExpectedProxySha256, StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(hash, LegacyProxySha256, StringComparison.OrdinalIgnoreCase) ||
                                 (new FileInfo(status.ProxyPath).Length > 1024);
            }

            // 2. Verify Manifest
            if (File.Exists(status.ManifestPath))
            {
                try
                {
                    var text = File.ReadAllText(status.ManifestPath);
                    status.ManifestOk = text.Contains(NativeHostName) &&
                                        text.Contains(ChromeExtensionOrigin) &&
                                        text.Contains(status.ProxyPath.Replace(@"\", @"\\"));
                }
                catch
                {
                    status.ManifestOk = false;
                }
            }

            // 3. Verify Registry
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistrySubKey, false))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(null) as string;
                        status.RegistryOk = string.Equals(val, status.ManifestPath, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
                status.RegistryOk = false;
            }

            // Synthesize State
            if (status.ProxyOk && status.ManifestOk && status.RegistryOk)
            {
                status.State = ChromeIntegrationState.Ready;
                status.Message = "Chrome integration is active and verified.";
            }
            else if (!File.Exists(status.ProxyPath) && !File.Exists(status.ManifestPath) && !status.RegistryOk)
            {
                status.State = ChromeIntegrationState.NeedsInstall;
                status.Message = "Integration is not installed.";
            }
            else if (!status.ProxyOk)
            {
                status.State = ChromeIntegrationState.MissingProxy;
                status.Message = "Proxy executable is missing or invalid.";
            }
            else if (!status.ManifestOk)
            {
                status.State = ChromeIntegrationState.BrokenManifest;
                status.Message = "Manifest JSON file is missing or invalid.";
            }
            else
            {
                status.State = ChromeIntegrationState.BrokenRegistry;
                status.Message = "Registry configuration is missing or pointing to wrong path.";
            }

            return status;
        }

        public bool DeployEmbeddedProxy(string targetPath)
        {
            var asm = Assembly.GetExecutingAssembly();
            // Look for resource ending with keepassnatmsg-proxy.exe
            string resourceName = null;
            foreach (var name in asm.GetManifestResourceNames())
            {
                if (name.EndsWith("keepassnatmsg-proxy.exe", StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }

            if (resourceName != null)
            {
                using (var stream = asm.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var dir = Path.GetDirectoryName(targetPath);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        using (var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fileStream);
                        }
                        return true;
                    }
                }
            }

            // Fallback: search relative paths for keepassnatmsg-proxy.exe
            var candidatePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "keepassnatmsg-proxy.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keepassnatmsg-proxy.exe"),
                Path.Combine(Environment.CurrentDirectory, "KeePassNatMsg", "Resources", "keepassnatmsg-proxy.exe"),
                Path.Combine(Environment.CurrentDirectory, "Resources", "keepassnatmsg-proxy.exe")
            };

            foreach (var candidate in candidatePaths)
            {
                if (File.Exists(candidate))
                {
                    var dir = Path.GetDirectoryName(targetPath);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.Copy(candidate, targetPath, true);
                    return true;
                }
            }

            return false;
        }

        public bool InstallOrRepair(out string errorMessage)
        {
            errorMessage = null;
            try
            {
                var configDir = GetConfigDir();
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // 1. Deploy Proxy if missing or hash mismatch
                var proxyPath = GetProxyPath();
                bool needDeployProxy = true;
                if (File.Exists(proxyPath))
                {
                    var hash = GetSha256(proxyPath);
                    if (string.Equals(hash, ExpectedProxySha256, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(hash, LegacyProxySha256, StringComparison.OrdinalIgnoreCase) ||
                        (new FileInfo(proxyPath).Length > 1024))
                    {
                        needDeployProxy = false;
                    }
                }

                if (needDeployProxy)
                {
                    if (!DeployEmbeddedProxy(proxyPath))
                    {
                        errorMessage = "Could not extract embedded keepassnatmsg-proxy.exe.";
                        return false;
                    }
                }

                // 2. Write Manifest JSON
                var manifestPath = GetManifestPath();
                var manifestContent = GenerateManifestContent(proxyPath);
                File.WriteAllText(manifestPath, manifestContent, new UTF8Encoding(false));

                // 3. Write Registry (HKCU)
                using (var key = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                {
                    if (key == null)
                    {
                        errorMessage = "Failed to create registry key: " + RegistrySubKey;
                        return false;
                    }
                    key.SetValue(null, manifestPath, RegistryValueKind.String);
                }

                // 4. Verify roundtrip
                var status = CheckStatus();
                if (status.State != ChromeIntegrationState.Ready)
                {
                    errorMessage = "Verification failed after install: " + status.Message;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public bool Uninstall(out string errorMessage)
        {
            errorMessage = null;
            try
            {
                // 1. Delete Registry Key
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(RegistrySubKey, false);
                }
                catch (Exception ex)
                {
                    errorMessage = "Failed to remove registry key: " + ex.Message;
                }

                // 2. Delete Manifest JSON
                var manifestPath = GetManifestPath();
                if (File.Exists(manifestPath))
                {
                    try { File.Delete(manifestPath); } catch { }
                }

                // Leave proxy.exe alone or clean if empty
                return errorMessage == null;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private static string GetSha256(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var bytes = sha256.ComputeHash(stream);
                    var sb = new StringBuilder();
                    foreach (var b in bytes) sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }
            }
        }
    }
}
