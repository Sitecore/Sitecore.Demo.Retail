﻿//-----------------------------------------------------------------------
// <copyright file="LoginModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the LoginModel class.</summary>
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

namespace Sitecore.Reference.Storefront.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Used to represent a user trying to login
    /// </summary>
    public class LoginModel
    {
        private List<IdentityProviderModel> _providers = new List<IdentityProviderModel>();

        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user's password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the user wants to be remembered for next login
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Gets the Open Id providers.
        /// </summary>        
        public List<IdentityProviderModel> Providers 
        {
            get 
            {
                return this._providers;
            }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>       
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is activation flow.
        /// </summary>        
        public bool IsActivationFlow { get; set; }
    }
}
