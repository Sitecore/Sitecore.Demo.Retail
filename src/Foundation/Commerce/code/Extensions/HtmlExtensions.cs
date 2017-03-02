using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Foundation.Dictionary.Repositories;
using Sitecore.Mvc.Helpers;

namespace Sitecore.Foundation.Commerce.Extensions
{
    public static class HtmlExtensions
    {
        public static string Text(this SitecoreHelper sitecoreHelper, string path, string defaultValue = null)
        {
            return DictionaryPhraseRepository.Current?.Get(path, defaultValue) ?? defaultValue;
        }
    }
}