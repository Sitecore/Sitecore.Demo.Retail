using Sitecore.Demo.Retail.Feature.Catalog.Website.Factories;
using Sitecore.Feature.Metadata.Infrastructure.Pipelines.GetPageMetadata;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Demo.Retail.Project.Website.Infrastructure.Pipelines
{
    [Service]
    public class SetProductCatalogMetadata
    {
        public SetProductCatalogMetadata(CatalogItemContext catalogItemContext, CategoryViewModelFactory categoryViewModelFactory, ProductViewModelFactory productViewModelFactory)
        {
            CatalogItemContext = catalogItemContext;
            CategoryViewModelFactory = categoryViewModelFactory;
            ProductViewModelFactory = productViewModelFactory;
        }

        public ProductViewModelFactory ProductViewModelFactory { get; }

        public CategoryViewModelFactory CategoryViewModelFactory { get; }

        public CatalogItemContext CatalogItemContext { get; }

        public void Process(GetPageMetadataArgs args)
        {
            if (CatalogItemContext.IsCategory)
            {
                var category = CategoryViewModelFactory.Create(CatalogItemContext.Current.Item);
                args.Metadata.PageTitle = category.Title;
                args.Metadata.Description = StringUtil.RemoveTags(category.Description);
            }
            if (CatalogItemContext.IsProduct)
            {
                var product = ProductViewModelFactory.Create(CatalogItemContext.Current.Item);
                args.Metadata.PageTitle = product.Title;
                args.Metadata.Description = StringUtil.RemoveTags(product.Description);
                foreach (var tag in product.Tags)
                {
                    args.Metadata.KeywordsList.Add(tag);
                }
            }
        }
    }
}