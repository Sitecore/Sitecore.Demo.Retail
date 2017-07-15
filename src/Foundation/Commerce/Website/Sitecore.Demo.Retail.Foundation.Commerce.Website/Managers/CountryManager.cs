using System.Collections.Generic;
using Sitecore.Commerce.Services.Orders;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers
{
    public class CountryManager : IManager
    {
        public CountryManager(OrderServiceProvider orderServiceProvider)
        {
            Assert.ArgumentNotNull(orderServiceProvider, nameof(orderServiceProvider));

            OrderServiceProvider = orderServiceProvider;
        }

        private OrderServiceProvider OrderServiceProvider { get; set; }

        public ManagerResponse<GetAvailableCountriesResult, Dictionary<string, string>> GetAvailableCountries()
        {
            var request = new GetAvailableCountriesRequest();
            var result = OrderServiceProvider.GetAvailableCountries(request);
            result.WriteToSitecoreLog();
            return new ManagerResponse<GetAvailableCountriesResult, Dictionary<string, string>>(result, new Dictionary<string, string>(result.AvailableCountries));
        }

        public ManagerResponse<GetAvailableRegionsResult, Dictionary<string, string>> GetAvailableRegions(string countryCode)
        {
            Assert.ArgumentNotNullOrEmpty(countryCode, nameof(countryCode));

            var request = new GetAvailableRegionsRequest(countryCode);
            var result = OrderServiceProvider.GetAvailableRegions(request);

            result.WriteToSitecoreLog();
            return new ManagerResponse<GetAvailableRegionsResult, Dictionary<string, string>>(result, new Dictionary<string, string>(result.AvailableRegions));
        }

    }
}