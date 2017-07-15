//-----------------------------------------------------------------------
// <copyright file="BaseJsonResult.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the BaseJsonResult class.</summary>
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
using System.Web.Mvc;
using Sitecore.Commerce.Services;
using Sitecore.Foundation.Dictionary.Repositories;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models
{
    public class BaseApiModel : JsonResult
    {
        public BaseApiModel()
        {
            Success = true;
        }

        public BaseApiModel(ServiceProviderResult result)
        {
            Success = true;

            if (result != null)
            {
                SetErrors(result);
            }
        }

        public BaseApiModel(string url)
        {
            Success = false;

            Url = url;
        }

        public List<string> Errors { get; } = new List<string>();

        public bool HasErrors => Errors != null && Errors.Any();

        public bool Success { get; set; }

        public string Url { get; set; }

        public void SetErrors(ServiceProviderResult result)
        {
            Success = result.Success;
            if (result.SystemMessages.Count <= 0)
            {
                return;
            }

            var errors = result.SystemMessages;
            foreach (var error in errors)
            {
                Errors.Add(DictionaryPhraseRepository.Current.Get($"/Commerce/System Messages/{error.Message}", $"[System Error: {error.Message}]"));
            }
        }

        public void SetErrors(string area, Exception exception)
        {
            Errors.Add($"{area}: {exception.Message}");
            Success = false;
        }

        public void SetErrors(List<string> errors)
        {
            if (!errors.Any())
            {
                return;
            }

            Success = false;
            Errors.AddRange(errors);
        }
    }
}