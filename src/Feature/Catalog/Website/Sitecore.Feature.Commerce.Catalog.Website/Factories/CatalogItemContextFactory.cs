using System.Web.Routing;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Caching;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Website.Models;
using Sitecore.Feature.Commerce.Catalog.Website.Services;
using Sitecore.Foundation.Commerce.Website.Managers;
using Sitecore.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Feature.Commerce.Catalog.Website.Factories
{
    [Service]
    public class CatalogItemContextFactory
    {
        public CatalogItemContextFactory(CatalogManager catalogManager, CatalogUrlService catalogUrlRepository)
        {
            CacheProvider = CommerceTypeLoader.GetCacheProvider(CommerceConstants.KnownCacheNames.FriendlyUrlsCache);
            CatalogManager = catalogManager;
            CatalogUrlRepository = catalogUrlRepository;
        }

        private CatalogManager CatalogManager { get; }
        public CatalogUrlService CatalogUrlRepository { get; }

        private ICacheProvider CacheProvider { get; }

        private void AddCatalogItemToCache(string catalogId, string catalogName, Item foundItem)
        {
            CacheProvider.AddData(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.FriendlyUrlsCache, GetCachekey(catalogId, catalogName), foundItem != null ? foundItem.ID : ID.Undefined);
        }

        private ID GetCatalogItemFromCache(string catalogId, string catalogName)
        {
            return CacheProvider.GetData<ID>(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.FriendlyUrlsCache, GetCachekey(catalogId, catalogName));
        }

        private static string GetCachekey(string itemId, string catalogName)
        {
            return "FriendlyUrl-" + itemId + "-" + catalogName;
        }


        public ICatalogItemContext Create(RouteData routeData, Database database)
        {
            Assert.IsNotNull(database, nameof(database));
            Assert.IsNotNull(routeData, nameof(routeData));

            var itemType = RouteConfig.GetItemType(routeData);
            switch (itemType)
            {
                case RouteItemType.CatalogItem:
                    return CreateCatalogContextFromCatalogRoute(routeData);
                case RouteItemType.Product:
                    return CreateCatalogContextFromProductRoute(routeData, database);
                case RouteItemType.Category:
                    return CreateCatalogContextFromCategoryRoute(routeData, database);
                default:
                    return CreateEmptyCatalogContext();
            }
        }

        private ICatalogItemContext Create(Item productCatalogItem)
        {
            Assert.IsNotNull(productCatalogItem, nameof(productCatalogItem));
            Assert.ArgumentCondition(productCatalogItem.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.CatalogItem.Id), nameof(productCatalogItem), "Item must be of type Commerce Catalog Item");

            var data = new CatalogRouteData
            {
                ItemType = productCatalogItem.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.Product.Id) ? CatalogItemType.Product : CatalogItemType.Category,
                Id = productCatalogItem.Name.ToLowerInvariant(),
                Item = productCatalogItem,
                Catalog = productCatalogItem[global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.CatalogItem.Fields.CatalogName],
                CategoryId = GetCategoryIdFromItem(productCatalogItem)
            };

            return data;
        }

        private ICatalogItemContext CreateEmptyCatalogContext()
        {
            if (CatalogManager.CatalogContext == null)
                return null;

            var data = new CatalogRouteData
            {
                Catalog = CatalogManager.CatalogContext.CurrentCatalog.Name
            };

            return data;
        }

        private ICatalogItemContext CreateCatalogContextFromProductRoute(RouteData routeData, Database database)
        {
            return CreateCatalogContextFromCatalogItemRoute(routeData, CatalogItemType.Product, database);
        }

        private ICatalogItemContext CreateCatalogContextFromCategoryRoute(RouteData routeData, Database database)
        {
            return CreateCatalogContextFromCatalogItemRoute(routeData, CatalogItemType.Category, database);
        }

        private ICatalogItemContext CreateCatalogContextFromCatalogItemRoute(RouteData routeData, CatalogItemType itemType, Database database)
        {
            var data = new CatalogRouteData
            {
                ItemType = itemType
            };

            if (routeData.Values.ContainsKey("id"))
            {
                data.Id = CatalogUrlRepository.ExtractItemId(routeData.Values["id"].ToString());
            }

            if (string.IsNullOrWhiteSpace(data.Id))
            {
                return null;
            }

            if (routeData.Values.ContainsKey("catalog"))
            {
                data.Catalog = routeData.Values["catalog"].ToString();
            }
            if (string.IsNullOrEmpty(data.Catalog))
            {
                data.Catalog = CatalogManager.CatalogContext?.CurrentCatalog?.Name;
            }
            if (string.IsNullOrEmpty(data.Catalog))
            {
                return null;
            }

            var item = GetCatalogItem(data, database);
            if (item == null)
            {
                return null;
            }
            data.Item = item;

            if (routeData.Values.ContainsKey("category"))
            {
                data.CategoryId = CatalogUrlRepository.ExtractItemId(routeData.Values["category"].ToString());
            }
            if (string.IsNullOrEmpty(data.CategoryId))
            {
                data.CategoryId = GetCategoryIdFromItem(data.Item);
            }

            return data;
        }

        private Item GetCatalogItem(ICatalogItemContext data, Database database)
        {
            var id = GetCatalogItemFromCache(data.Id, data.Catalog);
            if (!ID.IsNullOrEmpty(id))
            {
                return id == ID.Undefined ? null : database.GetItem(id);
            }

            Item item = null;
            switch (data.ItemType)
            {
                case CatalogItemType.Product:
                    item = CatalogManager.GetProduct(data.Id, data.Catalog);
                    break;
                case CatalogItemType.Category:
                    item = CatalogManager.GetCategoryItem(data.Id, data.Catalog);
                    break;
            }
            if (item != null)
            {
                AddCatalogItemToCache(data.Id, data.Catalog, item);
            }
            return item;
        }

        private ICatalogItemContext CreateCatalogContextFromCatalogRoute(RouteData routeData)
        {
            var catalogPath = routeData.Values["catalogPath"].ToString();
            if (string.IsNullOrEmpty(catalogPath))
            {
                return null;
            }
            var productCatalogItem = CatalogManager.CatalogContext?.CatalogRootItem.Axes.GetItem(catalogPath);
            return productCatalogItem == null ? null : Create(productCatalogItem);
        }

        private string GetCategoryIdFromItem(Item item)
        {
            if (item.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.Category.Id))
            {
                return item.Name.ToLowerInvariant();
            }
            if (item.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.Product.Id))
            {
                return item.Parent.Name.ToLowerInvariant();
            }
            if (item.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.ProductVariant.Id))
            {
                return item.Parent.Parent.Name.ToLowerInvariant();
            }
            return null;
        }
    }
}