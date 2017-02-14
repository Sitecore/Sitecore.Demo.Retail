//-----------------------------------------------------------------------
// <copyright file="DependenciesInstaller.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the DependenciesInstaller class.</summary>
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

using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Sitecore.Commerce.Services.Carts;

namespace Sitecore.Reference.Storefront.Infrastructure
{
    public class DependenciesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());

            container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());

            container.Register(Classes.FromThisAssembly().Pick());

            container.Register(Classes.FromAssemblyNamed("Sitecore.Foundation.Commerce").Pick());

            container.Register(Classes.FromAssemblyNamed("Sitecore.Commerce.Connect.CommerceServer").Pick());

            container.Register(Classes.FromAssemblyNamed("Sitecore.Commerce").Pick());

        }
    }
}