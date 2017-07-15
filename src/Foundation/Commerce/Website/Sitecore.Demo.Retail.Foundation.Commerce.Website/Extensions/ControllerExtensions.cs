//-----------------------------------------------------------------------
// <copyright file="BaseController.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the BaseController class.</summary>
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

using System.Linq;
using System.Web.Mvc;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions
{
    public static class ControllerExtensions
    {
        public static BaseApiModel CreateJsonResult(this Controller controller)
        {
            return controller.CreateJsonResult<BaseApiModel>();
        }

        public static T CreateJsonResult<T>(this Controller controller) where T : BaseApiModel, new()
        {
            var result = new T();
            if (controller.ModelState.IsValid)
            {
                return result;
            }
            var errors = controller.ModelState.Values.Where(modelValue => modelValue.Errors.Any()).SelectMany(modelValue => modelValue.Errors, (modelValue, error) => error.ErrorMessage).ToList();
            result.SetErrors(errors);

            return result;
        }

        public static T GetFromCache<T>(this Controller controller, string cacheKey) where T : class
        {
            return controller.HttpContext.Items[cacheKey] as T;
        }

        public static T AddToCache<T>(this Controller controller, string cacheKey, T value)
        {
            if (controller.HttpContext.Items.Contains(cacheKey))
            {
                controller.HttpContext.Items[cacheKey] = value;
            }
            else
            {
                controller.HttpContext.Items.Add(cacheKey, value);
            }
            return value;
        }
    }
}