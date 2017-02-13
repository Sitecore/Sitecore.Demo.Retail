//-----------------------------------------------------------------------
// <copyright file="PromoRotator.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the PromoRotator class.</summary>
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

using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Mvc.Presentation;
using Sitecore.Web;

namespace Sitecore.Foundation.Commerce.Models
{
    public class PromoRotator : SitecoreItemBase
    {
        private const int DefaultWidth = 100;
        private const int DefaultHeight = 100;

        public PromoRotator(Item item)
        {
            InnerItem = item;
        }

        public List<CommercePromotion> Promotions { get; } = new List<CommercePromotion>();

        public int Width
        {
            get
            {
                var parametersWidth = WebUtil.ParseUrlParameters(RenderingContext.Current.Rendering["Parameters"]);
                if (parametersWidth.AllKeys.All(p => p != "Width"))
                {
                    return DefaultWidth;
                }

                int width;
                return int.TryParse(parametersWidth.Get("Width"), out width) ? width : DefaultWidth;
            }
        }

        public int Height
        {
            get
            {
                var parametersHeight = WebUtil.ParseUrlParameters(RenderingContext.Current.Rendering["Parameters"]);
                if (parametersHeight.AllKeys.All(p => p != "Height"))
                {
                    return DefaultHeight;
                }

                int height;
                return int.TryParse(parametersHeight.Get("Height"), out height) ? height : DefaultHeight;
            }
        }
    }
}