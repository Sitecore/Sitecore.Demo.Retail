//-----------------------------------------------------------------------
// <copyright file="SiteContentViewModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the SiteContentViewModel class.</summary>
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

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce;
using Sitecore.Links;

namespace Sitecore.Reference.Storefront.Models
{
    public class SiteContentViewModel
    {
        public const int MaxTitleLength = 100;

        public Item Item { get; set; }

        public string ContentPath { get; set; }

        public string SummaryTitle { get; set; }

        public string SummaryText { get; set; }

        public static SiteContentViewModel Create(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            var model = new SiteContentViewModel
            {
                Item = item,
                ContentPath = LinkManager.GetItemUrl(item),
                SummaryTitle = TrimTextToLength(item[StorefrontConstants.ItemFields.Title], MaxTitleLength),
                SummaryText = item[StorefrontConstants.ItemFields.SummaryText]
            };

            return model;
        }

        private static string TrimTextToLength(string text, int maximumLength)
        {
            if (text != null && text.Length > maximumLength)
            {
                text = text.Substring(0, maximumLength);
                text = text.Substring(0, text.LastIndexOf(' ') + 1);
            }

            return text ?? string.Empty;
        }
    }
}