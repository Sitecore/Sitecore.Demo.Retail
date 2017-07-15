﻿//-----------------------------------------------------------------------
// <copyright file="ResetContact.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Resets the contact identifier when the session has timed out and the user is still 
// authenticated.  This only happens in CMS Only Mode.</summary>
//---------------------------------------------------------------------
// Copyright 2015 Sitecore Corporation A/S
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
// except in compliance with the License. You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the 
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
// either express or implied. See the License for the specific language governing permissions 
// and limitations under the License.
// -------------------------------------------------------------------------------------------

using Sitecore.Analytics.Pipelines.InitializeTracker;
using Sitecore.Foundation.DependencyInjection;
using Sitecore.Pipelines;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Pipelines
{
    [Service]
    public class ResetContact
    {
        public void Process(PipelineArgs args)
        {
            var trackerArgs = (InitializeTrackerArgs) args;

            // Reset the contact identifier when the session has timed out (indentifier null) and the user is authenticated.
            if (Context.User.IsAuthenticated && string.IsNullOrWhiteSpace(trackerArgs.Session.Contact.Identifiers.Identifier) && !string.IsNullOrWhiteSpace(Context.User.Name))
            {
                trackerArgs.Session.Contact.Identifiers.Identifier = Context.User.Name;
            }
        }
    }
}