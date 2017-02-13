//-----------------------------------------------------------------------
// <copyright file="PartyInputModelItem.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>InputModel item parameter accepting party information.</summary>
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

using System.ComponentModel.DataAnnotations;

namespace Sitecore.Foundation.Commerce.Models.InputModels
{
    public class PartyInputModelItem
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Address1 { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string ZipPostalCode { get; set; }

        public string ExternalId { get; set; }

        public string PartyId { get; set; }

        public bool IsPrimary { get; set; }
    }
}