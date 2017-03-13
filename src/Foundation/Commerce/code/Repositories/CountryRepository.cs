using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Data.Items;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.Commerce.Repositories
{
  public class CountryRepository
  {
    public Dictionary<string, string> GetCountriesAsDictionary()
    {
      return this.GetCountryItems().ToDictionary(country => country[Templates.Commerce.SharedSettings.Country.Fields.CountryCode], country => country[Templates.Commerce.SharedSettings.Country.Fields.Name]);
    }

    public Dictionary<string, string> GetRegionsAsDictionary(string countryCode)
    {
      return this.GetRegionItems(countryCode).ToDictionary(region => region[Templates.Commerce.SharedSettings.Subdivision.Fields.Code], region => region[Templates.Commerce.SharedSettings.Subdivision.Fields.Name]);
    }

    public IEnumerable<Item> GetCountryItems()
    {
      var root = GetCountriesRoot();
      return root.Children.Where(i => i.IsDerived(Templates.Commerce.SharedSettings.Country.Id));
    }

    public Item GetCountryItem(string countryCode)
    {
      var root = GetCountriesRoot();
      return root.Children.FirstOrDefault(i => i.IsDerived(Templates.Commerce.SharedSettings.Country.Id) && String.Equals(i[Templates.Commerce.SharedSettings.Country.Fields.CountryCode], countryCode, StringComparison.InvariantCultureIgnoreCase));
    }

    public IEnumerable<Item> GetRegionItems(string countryCode)
    {
      return GetCountryItem(countryCode)?.Children.Where(i => i.IsDerived(Templates.Commerce.SharedSettings.Subdivision.Id));
    }

    private Item GetCountriesRoot()
    {
      return Sitecore.Context.Database.GetItem(Templates.Commerce.SharedSettings.CountriesRegions);

    }
  }
}