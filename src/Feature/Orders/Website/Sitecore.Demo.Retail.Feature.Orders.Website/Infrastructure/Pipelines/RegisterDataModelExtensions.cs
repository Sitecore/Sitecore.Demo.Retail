﻿//-----------------------------------------------------------------------
// <copyright file="RegisterDataModelExtensions.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the RegisterDataModelExtensions class.</summary>
//---------------------------------------------------------------------
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
using Sitecore.Analytics.Data.DataAccess.MongoDb;
using Sitecore.Demo.Retail.Feature.Orders.Website.Models;
using Sitecore.Pipelines;

namespace Sitecore.Demo.Retail.Feature.Orders.Website.Infrastructure.Pipelines
{
    [Obsolete("Please refactor")]
    public class RegisterDataModelExtensions
    {
        public void Process(PipelineArgs args)
        {
            MongoDbObjectMapper.Instance.RegisterModelExtension<CommerceCartLineWithImages>();
        }
    }
}
