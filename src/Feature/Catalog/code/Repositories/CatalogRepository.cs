using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Repositories;
using Sitecore.Foundation.Commerce.Util;

namespace Sitecore.Feature.Commerce.Catalog.Repositories
{
    public class CatalogRepository
    {
        public CatalogRepository(SiteContextRepository siteContextRepository, CatalogManager catalogManager)
        {
            SiteContextRepository = siteContextRepository;
            CatalogManager = catalogManager;
        }

        private CatalogManager CatalogManager { get; }
        private SiteContextRepository SiteContextRepository { get; }

        public Category GetCurrentCategoryByUrl()
        {
            Category currentCategory;

            var categoryId = CatalogUrlRepository.ExtractItemIdFromCurrentUrl();

            var virtualCategoryCacheKey = $"VirtualCategory_{categoryId}";

            if (SiteContextRepository.GetCurrent().Items.Contains(virtualCategoryCacheKey))
            {
                currentCategory = SiteContextRepository.GetCurrent().Items[virtualCategoryCacheKey] as Category;
            }
            else
            {
                currentCategory = CatalogManager.GetCategory(categoryId);
                SiteContextRepository.GetCurrent().Items.Add(virtualCategoryCacheKey, currentCategory);
            }

            return currentCategory;
        }

    }
}