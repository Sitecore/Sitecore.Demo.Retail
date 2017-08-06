//-----------------------------------------------------------------------
// <copyright file="AddressItemModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the AddressItemModel class.</summary>
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

using Sitecore.Commerce.Services;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Orders.Website.Models
{
    public class AddressItemApiModel : BaseApiModel
    {
        public AddressItemApiModel()
        {
        }

        public AddressItemApiModel(ServiceProviderResult result)
            : base(result)
        {
        }

        public string Name { get; set; }

        public string ExternalId { get; set; }

        public string Address1 { get; set; }

        public string ZipPostalCode { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }

        public string FullAddress { get; set; }

        public bool IsPrimary { get; set; }

        public string DetailsUrl { get; set; }

        public void Initialize(IParty party)
        {
            Assert.ArgumentNotNull(party, nameof(party));

            Name = party.Name;
            IsPrimary = party.IsPrimary;
            ExternalId = party.ExternalId;
            Address1 = party.Address1;
            City = party.City;
            Region = party.Region;
            ZipPostalCode = party.ZipPostalCode;
            Country = party.Country;
            FullAddress = string.Concat(party.Address1, ", ", party.City, ", ", party.ZipPostalCode);
#warning Remove hardcoded URL
            DetailsUrl = string.Concat("/accountmanagement/addressbook", "?id=", party.ExternalId);
        }
    }
}