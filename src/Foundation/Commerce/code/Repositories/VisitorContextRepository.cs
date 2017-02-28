using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Contacts;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Foundation.Commerce.Repositories
{
    public class VisitorContextRepository
    {
        public VisitorContextRepository(SiteContextRepository siteContextRepository, AccountManager accountManager, ContactFactory contactFactory)
        {
            Assert.ArgumentNotNull(siteContextRepository, nameof(siteContextRepository));
            Assert.ArgumentNotNull(accountManager, nameof(accountManager));
            Assert.ArgumentNotNull(contactFactory, nameof(contactFactory));

            SiteContextRepository = siteContextRepository;
            AccountManager = accountManager;
            ContactFactory = contactFactory;
        }

        public VisitorContext GetCurrent()
        {
            // Setup the visitor context only once per HttpRequest.
            var visitorContext = HttpContext.Current.Items["__visitorContext"] as VisitorContext;
            if (visitorContext != null)
            {
                return visitorContext;
            }

            visitorContext = new VisitorContext(ContactFactory);
            if (Context.User.IsAuthenticated && !Context.User.Profile.IsAdministrator)
            {
                visitorContext.SetCommerceUser(AccountManager.ResolveCommerceUser().Result);
            }

            HttpContext.Current.Items["__visitorContext"] = visitorContext;

            return visitorContext;
        }

        public ContactFactory ContactFactory { get; }

        public AccountManager AccountManager { get; }

        public SiteContextRepository SiteContextRepository { get; }
    }
}