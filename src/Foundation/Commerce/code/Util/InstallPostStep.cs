//-----------------------------------------------------------------------
// <copyright file="InstallPostStep.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the InstallPostStep class.</summary>
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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Sitecore.Analytics;
using Sitecore.Analytics.Automation.Data.Items;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Install.Framework;

namespace Sitecore.Foundation.Commerce.Util
{
    public class InstallPostStep : IPostStep
    {
        private const string CreateDeployEngagementPlans = "Creating and deploying engagement plans.";
        private const string CreateDeployEngagementPlansComplete = "Done creating and deploying engagement plans.";

        private const string DefaultLocalizationFolder = "Storefront";

        private const string EaPlanFormatString = "{0} {1}";

        private readonly List<EaPlanInfo> _eaPlanInfos = new List<EaPlanInfo>
        {
            new EaPlanInfo
            {
                Name = StorefrontConstants.EngagementPlans.AbandonedCartsEaPlanName,
                ItemId = StorefrontConstants.EngagementPlans.AbandonedCartsEaPlanId,
                EaPlanId = StorefrontConstants.KnownItemIds.AbandonedCartsEaPlanBranchTemplateId
            },
            new EaPlanInfo
            {
                Name = StorefrontConstants.EngagementPlans.NewOrderPlacedEaPlanName,
                ItemId = StorefrontConstants.EngagementPlans.NewOrderPlacedEaPlanId,
                EaPlanId = StorefrontConstants.KnownItemIds.NewOrderPlacedEaPlanBranchTemplateId
            },
            new EaPlanInfo
            {
                Name = StorefrontConstants.EngagementPlans.ProductsBackInStockEaPlanName,
                ItemId = StorefrontConstants.EngagementPlans.ProductsBackInStockEaPlanId,
                EaPlanId = StorefrontConstants.KnownItemIds.ProductsBackInStockEaPlanBranchTemplateId
            }
        };

        public void Run(ITaskOutput output, NameValueCollection metadata)
        {
            var postStep = new Sitecore.Commerce.Connect.CommerceServer.InstallPostStep(DefaultLocalizationFolder);

            postStep.OutputMessage(CreateDeployEngagementPlans);
            CreateEaPlans();
            DeployEaPlans();
            postStep.OutputMessage(CreateDeployEngagementPlansComplete);

            postStep.Run(output, metadata);
        }

        protected virtual void CreateEaPlans()
        {
            var database = Context.ContentDatabase ?? Database.GetDatabase("master");

            foreach (var plan in _eaPlanInfos)
            {
                var item = database.GetItem(ID.Parse(plan.EaPlanId));

                if (item != null)
                {
                    plan.Name = string.Format(CultureInfo.InvariantCulture, EaPlanFormatString, StorefrontConstants.Settings.WebsiteName, item.DisplayName);
                    var result = ItemManager.AddFromTemplate(plan.Name, ID.Parse(plan.EaPlanId), database.GetItem(StorefrontConstants.KnownItemIds.CommerceConnectEaPlanParentId), ID.Parse(plan.ItemId));
                    continue;
                }

                CommerceLog.Current.Error(string.Format(CultureInfo.InvariantCulture, "Error creating engagement plan '{0}'.", plan.Name), this);
            }
        }

        protected virtual void DeployEaPlans()
        {
            foreach (var planInfo in _eaPlanInfos)
            {
                var engagementPlanItem = Tracker.DefinitionDatabase.Automation().EngagementPlans[planInfo.Name];

                if (engagementPlanItem.IsDeployed)
                {
                    continue;
                }

                var result = ((Item) engagementPlanItem).State.GetWorkflow().Execute(StorefrontConstants.KnownItemIds.DeployCommandId, engagementPlanItem, string.Empty, false).Succeeded;

                if (!result)
                {
                    CommerceLog.Current.Error(string.Format(CultureInfo.InvariantCulture, "Error deploying engagement plan '{0}'.", planInfo.Name), this);
                }
            }
        }
    }
}