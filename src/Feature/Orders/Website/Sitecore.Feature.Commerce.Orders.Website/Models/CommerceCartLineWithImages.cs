//-----------------------------------------------------------------------
// <copyright file="CommerceCartLineWithImages.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the CommerceCartLineWithImages class.</summary>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Data.Items;

namespace Sitecore.Feature.Commerce.Orders.Website.Models
{
#warning Please refactor
    [Serializable]
    public class CommerceCartLineWithImages : CommerceCartLine
    {
        [NonSerialized] private List<MediaItem> _images;

        public CommerceCartLineWithImages()
        {
        }

        public CommerceCartLineWithImages(string productCatalog, string productId, string variantId, uint quantity) : base(productCatalog, productId, variantId, quantity)
        {
        }

        [JsonIgnore]
        [XmlIgnore]
        public MediaItem DefaultImage => Images?.FirstOrDefault();

        [JsonIgnore]
        [XmlIgnore]
        public List<MediaItem> Images
        {
            get
            {
                if (_images != null)
                {
                    return _images;
                }

                _images = new List<MediaItem>();

                var field = Properties["_product_Images"] as string;
                if (field == null)
                {
                    return _images;
                }

                var imageIds = field.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var mediaItem in imageIds.Select(id => Context.Database.GetItem(id)))
                {
                    _images.Add(mediaItem);
                }

                return _images;
            }
        }
    }
}