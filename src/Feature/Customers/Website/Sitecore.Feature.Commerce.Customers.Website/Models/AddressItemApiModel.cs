//-----------------------------------------------------------------------
// <copyright file="AddressItemJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the AddressItemJsonResult class.</summary>
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

namespace Sitecore.Feature.Commerce.Customers.Website.Models
{
    public class AddressItemApiModel : BaseApiModel, IParty
    {
        public AddressItemApiModel()
        {
        }

        public AddressItemApiModel(ServiceProviderResult result) : base(result)
        {
        }

        public string Name { get; set; }

        public string ExternalId { get; set; }

        public string PartyId { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }

        public string ZipPostalCode { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }

        public string FullAddress { get; set; }

        public bool IsPrimary { get; set; }

        public string DetailsUrl { get; set; }

        public virtual void Initialize(IParty address)
        {
            Assert.ArgumentNotNull(address, nameof(address));

            Name = address.Name;
            IsPrimary = address.IsPrimary;
            ExternalId = address.ExternalId;
            Address1 = address.Address1;
            City = address.City;
            Region = address.Region;
            ZipPostalCode = address.ZipPostalCode;
            Country = address.Country;
            FullAddress = string.Concat(address.Address1, ", ", address.City, ", ", address.ZipPostalCode);
            DetailsUrl = string.Concat("/accountmanagement/addressbook", "?id=", address.ExternalId);
        }

    }
}