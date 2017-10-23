//-----------------------------------------------------------------------
// <copyright file="ProfileBaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Emits the Json result of a profile details request.</summary>
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
using Sitecore.Commerce.Services.Customers;
using Sitecore.Foundation.Commerce.Website.Models;

namespace Sitecore.Feature.Commerce.Customers.Website.Models
{
    public class ProfileApiModel : BaseApiModel
    {
        public ProfileApiModel()
        {
        }

        public ProfileApiModel(UpdateUserResult result)
            : base(result)
        {
        }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Telephone")]
        public string TelephoneNumber { get; set; }

        public void Initialize(UpdateUserResult result)
        {
            if (result.CommerceUser != null)
            {
                Email = result.CommerceUser.Email;
                FirstName = result.CommerceUser.FirstName;
                LastName = result.CommerceUser.LastName;
                TelephoneNumber = result.Properties["Phone"] as string;
            }

            SetErrors(result);
        }
    }
}