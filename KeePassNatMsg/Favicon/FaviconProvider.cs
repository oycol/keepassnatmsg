using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KeePassNatMsg.Favicon
{
    public sealed class FaviconProvider
    {
        public const string ProviderDirect = "Direct Website (Default & Private)";
        public const string ProviderDuckDuckGo = "DuckDuckGo Favicon Service";
        public const string ProviderGoogle = "Google Favicon Service";
        public const string ProviderFaviconKit = "FaviconKit Service";
        public const string ProviderCustom = "Custom URL Provider";

        public string Name { get; private set; }
        public string UrlTemplate { get; set; }

        public FaviconProvider(string name, string urlTemplate)
        {
            Name = name;
            UrlTemplate = urlTemplate;
        }

        public override string ToString()
        {
            return Name;
        }

        private static readonly List<FaviconProvider> PresetProviders = new List<FaviconProvider>();

        static FaviconProvider()
        {
            PresetProviders.Add(new FaviconProvider(ProviderDirect, null));
            PresetProviders.Add(new FaviconProvider(ProviderDuckDuckGo, "https://icons.duckduckgo.com/ip3/{URL:HOST}.ico"));
            PresetProviders.Add(new FaviconProvider(ProviderGoogle, "https://www.google.com/s2/favicons?domain={URL:HOST}&sz={YAFD:ICON_SIZE}"));
            PresetProviders.Add(new FaviconProvider(ProviderFaviconKit, "https://api.faviconkit.com/{URL:HOST}/{YAFD:ICON_SIZE}"));
            PresetProviders.Add(new FaviconProvider(ProviderCustom, ""));
        }

        public static FaviconProvider[] GetProviders()
        {
            return PresetProviders.ToArray();
        }

        public static FaviconProvider FindByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return PresetProviders[0];
            for (int i = 0; i < PresetProviders.Count; i++)
            {
                if (string.Equals(PresetProviders[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return PresetProviders[i];
                }
            }
            return PresetProviders[0];
        }

        public static string BuildProviderUrl(string template, string hostname, int iconSize)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;
            string url = template;
            url = Regex.Replace(url, @"\{URL:HOST\}", hostname ?? string.Empty, RegexOptions.IgnoreCase);
            url = Regex.Replace(url, @"\{YAFD:ICON_SIZE\}", iconSize.ToString(), RegexOptions.IgnoreCase);
            return url;
        }
    }
}
