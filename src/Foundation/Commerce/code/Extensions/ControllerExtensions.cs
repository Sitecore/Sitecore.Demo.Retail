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
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Foundation.Commerce.Extensions
{
    public static class ControllerExtensions
    {
        public static BaseJsonResult CreateJsonResult(this Controller controller)
        {
            return controller.CreateJsonResult<BaseJsonResult>();
        }

        public static T CreateJsonResult<T>(this Controller controller) where T : BaseJsonResult, new()
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
    }
}