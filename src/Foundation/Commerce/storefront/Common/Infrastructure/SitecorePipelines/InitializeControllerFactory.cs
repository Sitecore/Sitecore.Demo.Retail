//-----------------------------------------------------------------------
// <copyright file="InitializeControllerFactory.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the InitializeControllerFactory class.</summary>
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

using Sitecore.Mvc.Controllers;
using Sitecore.Pipelines;
using ControllerBuilder = System.Web.Mvc.ControllerBuilder;

namespace Sitecore.Reference.Storefront.Infrastructure.SitecorePipelines
{
    public class InitializeControllerFactory
    {
        public virtual void Process(PipelineArgs args)
        {
            SetControllerFactory(args);
        }

        protected virtual void SetControllerFactory(PipelineArgs args)
        {
            var controllerFactory = new WindsorControllerFactory(WindsorConfig.Container.Kernel);
            var sitecoreFactory = new SitecoreControllerFactory(controllerFactory);

            ControllerBuilder.Current.SetControllerFactory(sitecoreFactory);
        }
    }
}