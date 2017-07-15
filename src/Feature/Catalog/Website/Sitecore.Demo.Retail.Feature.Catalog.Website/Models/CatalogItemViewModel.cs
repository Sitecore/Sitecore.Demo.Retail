using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Resources.Media;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website.Models
{
    public abstract class CatalogItemViewModel
    {
        public Item Item { get; }

        private List<MediaItem> _images;

        protected CatalogItemViewModel(Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Item = item;
        }

        public MediaItem DefaultImage => Images?.FirstOrDefault();

        public string GetThumbnailUrl(int maxWidth)
        {
            if (DefaultImage == null)
                return null;

            var options = new MediaUrlOptions { MaxWidth = maxWidth };
            var url = MediaManager.GetMediaUrl(DefaultImage, options);
            var cleanUrl = StringUtil.EnsurePrefix('/', url);
            var hashedUrl = HashingUtils.ProtectAssetUrl(cleanUrl);

            return hashedUrl;
        }

        public List<MediaItem> Images
        {
            get
            {
                if (_images != null)
                    return _images;

                _images = new List<MediaItem>();

                MultilistField field = Item.Fields[ImagesFieldName];

                if (field == null)
                    return _images;
                foreach (var id in field.TargetIDs)
                {
                    MediaItem mediaItem = Item.Database.GetItem(id);
                    _images.Add(mediaItem);
                }

                return _images;
            }
        }

        public abstract string ImagesFieldName { get; }
        public abstract string DescriptionFieldName { get; }
        public abstract string TitleFieldName { get; }

        public string GetLink()
        {
            return LinkManager.GetDynamicUrl(Item).TrimEnd('/');
        }

        public string Title => Item[TitleFieldName];
        public string Description => Item[DescriptionFieldName];

        public string Name => Item.Name;
    }
}