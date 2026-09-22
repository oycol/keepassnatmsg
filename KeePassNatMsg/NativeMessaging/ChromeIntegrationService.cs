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
        public static readonly string[] AllowedExtensionOrigins = new[]
        {
            // Keep in sync with KeePassXC upstream NativeMessageInstaller ALLOWED_ORIGINS.
            "chrome-extension://pdffhmdngciaglkoonimfcmckehcpafo/",
            "chrome-extension://oboonakemofpalcgghocfoadofidjkkk/"
        };

        public const string RegistrySubKey = @"Software\Google\Chrome\NativeMessagingHosts\" + NativeHostName;

        public static readonly string[] SupportedRegistryKeys = new[]
        {
            @"Software\Google\Chrome\NativeMessagingHosts\" + NativeHostName,
            @"Software\Microsoft\Edge\NativeMessagingHosts\" + NativeHostName
        };

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
            return Path.Combine(GetConfigDir(), "org.keepassxc.keepassxc_browser.json");
        }

        public virtual bool IsChromeInstalled()
        {
            var p1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe");
            var p2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe");
            var p3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe");
            var e1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\Edge\Application\msedge.exe");
            var e2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe");
            return File.Exists(p1) || File.Exists(p2) || File.Exists(p3) || File.Exists(e1) || File.Exists(e2);
        }

        public static bool IsValidExecutable(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
            try
            {
                var fi = new FileInfo(path);
                if (fi.Length < 1024) return false;

                // Validate MZ header (Windows Portable Executable format)
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length < 2) return false;
                    var b1 = fs.ReadByte();
                    var b2 = fs.ReadByte();
                    return b1 == 'M' && b2 == 'Z';
                }
            }
            catch
            {
                return false;
            }
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
            for (var i = 0; i < AllowedExtensionOrigins.Length; i++)
            {
                var comma = (i < AllowedExtensionOrigins.Length - 1) ? "," : "";
                sb.AppendLine("    \"" + AllowedExtensionOrigins[i] + "\"" + comma);
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static bool ContainsAllAllowedOrigins(string manifestText)
        {
            if (string.IsNullOrEmpty(manifestText)) return false;
            foreach (var origin in AllowedExtensionOrigins)
            {
                if (!manifestText.Contains(origin)) return false;
            }
            return true;
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

            // 1. Verify Proxy (executable check, no brittle fixed hash)
            status.ProxyOk = IsValidExecutable(status.ProxyPath);

            // 2. Verify Manifest
            if (File.Exists(status.ManifestPath))
            {
                try
                {
                    var text = File.ReadAllText(status.ManifestPath);
                    status.ManifestOk = text.Contains(NativeHostName) &&
                                        ContainsAllAllowedOrigins(text) &&
                                        text.Contains(status.ProxyPath.Replace(@"\", @"\\"));
                }
                catch
                {
                    status.ManifestOk = false;
                }
            }

            // 3. Verify Registry (check Chrome and Edge keys)
            var regOkCount = 0;
            foreach (var subKey in SupportedRegistryKeys)
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(subKey, false))
                    {
                        if (key != null)
                        {
                            var val = key.GetValue(null) as string;
                            if (string.Equals(val, status.ManifestPath, StringComparison.OrdinalIgnoreCase))
                            {
                                regOkCount++;
                            }
                        }
                    }
                }
                catch { }
            }
            status.RegistryOk = (regOkCount > 0);

            // Synthesize State
            if (status.ProxyOk && status.ManifestOk && status.RegistryOk)
            {
                status.State = ChromeIntegrationState.Ready;
                status.Message = "Browser integration (Chrome & Edge) is active and verified.";
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
                        using (var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            stream.CopyTo(fileStream);
                        }
                        return true;
                    }
                }
            }

            // Fallback: search relative paths
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

                // 1. Deploy Proxy if missing or not a valid executable
                var proxyPath = GetProxyPath();
                if (!IsValidExecutable(proxyPath))
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

                // 3. Write Registry for both Chrome and Edge (HKCU)
                var registeredAny = false;
                foreach (var subKey in SupportedRegistryKeys)
                {
                    try
                    {
                        using (var key = Registry.CurrentUser.CreateSubKey(subKey))
                        {
                            if (key != null)
                            {
                                key.SetValue(null, manifestPath, RegistryValueKind.String);
                                registeredAny = true;
                            }
                        }
                    }
                    catch { }
                }

                if (!registeredAny)
                {
                    errorMessage = "Failed to write browser NativeMessagingHosts registry keys.";
                    return false;
                }

                // Verify status
                var verified = CheckStatus();
                if (verified.State != ChromeIntegrationState.Ready)
                {
                    errorMessage = "Verification failed after install: " + verified.Message;
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
                // 1. Remove registry keys for Chrome and Edge
                foreach (var subKey in SupportedRegistryKeys)
                {
                    try
                    {
                        Registry.CurrentUser.DeleteSubKeyTree(subKey, false);
                    }
                    catch { }
                }

                // 2. Remove Manifest JSON file
                var manifestPath = GetManifestPath();
                if (File.Exists(manifestPath))
                {
                    try { File.Delete(manifestPath); } catch { }
                }

                // 3. Remove legacy chrome json if present
                var legacyManifest = Path.Combine(GetConfigDir(), "org.keepassxc.keepassxc_browser.chrome.json");
                if (File.Exists(legacyManifest))
                {
                    try { File.Delete(legacyManifest); } catch { }
                }

                // 4. Clean up proxy and directory if possible
                var proxyPath = GetProxyPath();
                if (File.Exists(proxyPath))
                {
                    try { File.Delete(proxyPath); } catch { }
                }

                var configDir = GetConfigDir();
                if (Directory.Exists(configDir))
                {
                    try
                    {
                        if (Directory.GetFiles(configDir).Length == 0 && Directory.GetDirectories(configDir).Length == 0)
                        {
                            Directory.Delete(configDir);
                        }
                    }
                    catch { }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static string GetSha256(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hashBytes = sha256.ComputeHash(stream);
                    var sb = new StringBuilder();
                    foreach (var b in hashBytes)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
        }
    }
}
