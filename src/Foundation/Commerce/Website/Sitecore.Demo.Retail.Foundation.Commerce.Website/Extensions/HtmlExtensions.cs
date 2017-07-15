using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Mvc.Helpers;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions
{
    public static class HtmlExtensions
    {
        public static string Text(this SitecoreHelper sitecoreHelper, string path, string defaultValue = null)
        {
            return DictionaryPhraseRepository.Current?.Get(path, defaultValue) ?? defaultValue;
        }
    }
}