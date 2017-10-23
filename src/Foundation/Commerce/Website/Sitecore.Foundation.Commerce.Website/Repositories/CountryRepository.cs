using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Commerce.Multishop;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Website.Repositories
{
    [Service]
    public class CountryRepository
    {
        public Dictionary<string, string> GetCountriesAsDictionary()
        {
            return GetCountryItems().ToDictionary(country => country[Sitecore.Commerce.Constants.Templates.CountryRegion.Fields.CountryCode], country => country[Sitecore.Commerce.Constants.Templates.CountryRegion.Fields.Name]);
        }

        public Dictionary<string, string> GetRegionsAsDictionary(string countryCode)
        {
            return GetRegionItems(countryCode).ToDictionary(region => region[Sitecore.Commerce.Constants.Templates.Subdivision.Fields.Code], region => region[Sitecore.Commerce.Constants.Templates.Subdivision.Fields.Name]);
        }

        public IEnumerable<Item> GetCountryItems()
        {
            var countryRegionsConfigurationSettings = ConnectStorefrontContext.Current?.StorefrontConfiguration?.Children.FirstOrDefault(i => i.IsDerived(Sitecore.Commerce.Constants.Templates.CountryRegionConfiguration.ID));
            if (countryRegionsConfigurationSettings == null)
                return Enumerable.Empty<Item>();
            return ((MultilistField) countryRegionsConfigurationSettings.Fields[Sitecore.Commerce.Constants.Templates.CountryRegionConfiguration.Fields.CountriesRegions])?.GetItems().Where(i => i.IsDerived(Sitecore.Commerce.Constants.Templates.CountryRegion.ID));
        }

        public Item GetCountryItem(string countryCode)
        {
            return GetCountryItems().FirstOrDefault(i => string.Equals(i[Sitecore.Commerce.Constants.Templates.CountryRegion.Fields.CountryCode], countryCode, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<Item> GetRegionItems(string countryCode)
        {
            return GetCountryItem(countryCode)?.Children.Where(i => i.IsDerived(Sitecore.Commerce.Constants.Templates.Subdivision.ID));
        }
    }
}