using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Foundation.Commerce.Repositories
{
    public class SiteContextRepository
    {
        private SiteContext _siteContext;

        public SiteContext GetCurrent()
        {
            return _siteContext ?? (_siteContext = new SiteContext());
        }
    }
}