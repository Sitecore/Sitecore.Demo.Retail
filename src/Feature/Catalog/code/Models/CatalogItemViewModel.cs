using System;
using System.Collections.Generic;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;

namespace Sitecore.Feature.Commerce.Catalog.Models
{
    public abstract class CatalogItemViewModel
    {
        public Item Item { get; }

        private List<MediaItem> _images;

        protected CatalogItemViewModel(Item item)
        {
            Item = item;
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