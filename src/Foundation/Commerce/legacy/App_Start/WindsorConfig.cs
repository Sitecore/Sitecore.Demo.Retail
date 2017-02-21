//-----------------------------------------------------------------------
// <copyright file="WindsorConfig.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the WindsorConfig class.</summary>
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

using Castle.Windsor;
using Castle.Windsor.Installer;
using Sitecore.Reference.Storefront;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(WindsorConfig), "ConfigureContainer")]
[assembly: ApplicationShutdownMethod(typeof(WindsorConfig), "ReleaseContainer")]

namespace Sitecore.Reference.Storefront
{
    public static class WindsorConfig
    {
        internal static readonly IWindsorContainer Container;

        static WindsorConfig()
        {
            Container = Sitecore.Foundation.Commerce.WindsorConfig.Container.Install(FromAssembly.This());
        }

        public static void ConfigureContainer()
        {
        }

        public static void ReleaseContainer()
        {
        }
    }
}