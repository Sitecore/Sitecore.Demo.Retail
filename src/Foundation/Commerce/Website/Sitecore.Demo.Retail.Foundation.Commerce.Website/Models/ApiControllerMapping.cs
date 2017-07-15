using System.Globalization;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models
{
    public class ApiControllerMapping
    {
        public ApiControllerMapping(string name, string controller, string action)
        {
            this.Name = name;
            this.Controller = controller;
            this.Action = action;
        }

        public string Name { get; private set; }

        public string Controller { get; private set; }

        public string Action { get; private set; }

        public string Url => "api/storefront/" + (this.Controller.ToLower(CultureInfo.InvariantCulture) + "/" + this.Action.ToLower(CultureInfo.InvariantCulture));
    }
}