using System;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Contacts;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Diagnostics;
using Sitecore.Foundation.DependencyInjection;
using CommerceUser = Sitecore.Demo.Retail.Foundation.Commerce.Website.Models.CommerceUser;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website
{
    [Service]
    public class CommerceUserContext
    {
        public CommerceUserContext(ContactFactory contactFactory, AccountManager accountManager)
        {
            Assert.ArgumentNotNull(contactFactory, nameof(contactFactory));

            ContactFactory = contactFactory;
            AccountManager = accountManager;
        }

        private AccountManager AccountManager { get; }
        private ContactFactory ContactFactory { get; }

        public IUser Current
        {
            get
            {
                // Setup the visitor context only once per HttpRequest.
                var commerceUser = HttpContext.Current.Items["__commerceUser"] as CommerceUser;
                if (commerceUser != null)
                {
                    return commerceUser;
                }

                commerceUser = new CommerceUser(ContactFactory);
                if (Context.User.IsAuthenticated && !Context.User.Profile.IsAdministrator)
                {
                    var result = AccountManager.ResolveCommerceUser();
                    if (result != null)
                    {
                        SetUser(commerceUser, result);
                    }
                }

                HttpContext.Current.Items["__commerceUser"] = commerceUser;

                return commerceUser;
            }
        }

        private void SetUser(CommerceUser commerceUser, Sitecore.Commerce.Entities.Customers.CommerceUser user)
        {
            commerceUser.User = user;
            Assert.IsNotNull(user.Customers, "The user '{0}' does not contain a Customers collection.", user.UserName);
            commerceUser.UserId = user.Customers.FirstOrDefault();
        }

        public void SetUser(Sitecore.Commerce.Entities.Customers.CommerceUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var commerceUser = Current as CommerceUser;
            if (commerceUser == null)
            {
                return;
            }
            SetUser(commerceUser, user);
        }
    }
}