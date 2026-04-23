using System;
using System.Linq;
using System.Windows;

namespace XpsCreator.Localization
{
    public static class LanguageManager
    {
        public static void SetLanguage(string languageCode)
        {
            var dict = new ResourceDictionary();
            switch (languageCode.ToLower())
            {
                case "zh-hans":
                case "cn":
                    dict.Source = new Uri("Localization/Strings.zh-Hans.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("Localization/Strings.en.xaml", UriKind.Relative);
                    break;
            }

            // Find existing language dictionary and replace it
            var oldDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Localization/Strings."));

            if (oldDict != null)
            {
                int index = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                Application.Current.Resources.MergedDictionaries[index] = dict;
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
        }
    }
}
