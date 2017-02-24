using Sitecore.Data.Items;

namespace Sitecore.Foundation.Commerce.Models
{
    public interface IProductResolver
    {
        Item ResolveProductItem(string productId, string productCatalog);
        Item ResolveCategoryItem(string categoryId, string productCatalog);
    }
}