//-----------------------------------------------------------------------
// <copyright file="NavigationSearchController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the NavigationSearchController class.</summary>
//-----------------------------------------------------------------------
// Copyright 2016 Sitecore Corporation A/S
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
// except in compliance with the License. You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the 
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
// either express or implied. See the License for the specific language governing permissions 
// and limitations under the License.
// -------------------------------------------------------------------------------------------

using System.Linq;
using System.Web.Mvc;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.Commerce.Contacts;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Mvc.Presentation;
using Sitecore.Reference.Storefront.Utils;

namespace Sitecore.Reference.Storefront.Controllers
{
    public class NavigationSearchController : BaseController
    {
        public NavigationSearchController([NotNull] ContactFactory contactFactory)
            : base(contactFactory)
        {
        }

        public string IndexName { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public override ActionResult Index()
        {
            var dataSourceQuery = RenderingContext.Current.Rendering.DataSource;

            if (string.IsNullOrWhiteSpace(dataSourceQuery))
            {
                return View(Enumerable.Empty<Category>());
            }
            var response = SearchNavigation.GetNavigationCategories(dataSourceQuery, new CommerceSearchOptions());
            var navigationResults = response.ResponseItems;

            return View(navigationResults.Select(result => new Category(result)));
        }
    }
}