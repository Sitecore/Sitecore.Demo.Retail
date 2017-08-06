//-----------------------------------------------------------------------
// <copyright file="RegisterUserInputModel.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Controller parameters required to register a user with the site.</summary>
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

namespace Sitecore.Foundation.Commerce.Website.Models.InputModels
{
    public class RegisterUserInputModel
    {
        [Required]
        [Display(Name = "Email")]
        public string UserName { get; set; }

        [StringLength(25, ErrorMessage = "The {0} must be maximum {1} characters long.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(25, ErrorMessage = "The {0} must be maximum {1} characters long.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail address of existing customer")]
        [Display(Name = "Email Of Existing Customer")]
        public string LinkupEmail { get; set; }

        public string ExternalId { get; set; }

        public string SignupSelection { get; set; }
    }
}