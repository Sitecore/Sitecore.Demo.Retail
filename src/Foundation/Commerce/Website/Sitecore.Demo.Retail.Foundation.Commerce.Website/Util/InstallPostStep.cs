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
using Sitecore.Analytics;
using Sitecore.Analytics.Automation.Data.Items;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Install.Framework;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Util
{
    public class InstallPostStep : IPostStep
    {
        private const string CreateDeployEngagementPlans = "Creating and deploying engagement plans.";
        private const string CreateDeployEngagementPlansComplete = "Done creating and deploying engagement plans.";

        private const string DefaultLocalizationFolder = "Storefront";

        private const string AbandonedCartsEaPlanId = "{7138ACC1-329C-4070-86DD-6A53D6F57AC5}";
        private const string AbandonedCartsEaPlanName = "Abandoned Carts";
        private const string NewOrderPlacedEaPlanId = "{7CA697EA-5CCA-4B59-85A3-D048B285E6B4}";
        private const string NewOrderPlacedEaPlanName = "New Order Placed";
        private const string ProductsBackInStockEaPlanId = "{36B4083E-F7F7-4E60-A747-75DDBEC6BB4B}";
        private const string ProductsBackInStockEaPlanName = "Products Back In Stock";
        private const string AbandonedCartsEaPlanBranchTemplateId = "{8C90E12F-4E2E-4E3D-9137-B2D5F5DD40C0}";
        private const string NewOrderPlacedEaPlanBranchTemplateId = "{6F6A861F-78CF-4859-8AD8-7A2D5CCDBEB6}";
        private const string ProductsBackInStockEaPlanBranchTemplateId = "{534EE43B-00B1-49D0-92A7-E78B9C127B00}";
        private const string DeployCommandId = "{4044A9C4-B583-4B57-B5FF-2791CB0351DF}";
        private const string CommerceConnectEaPlanParentId = "{03402BEE-21E9-458A-B3F4-D004CC4F21FA}";

        private readonly List<EaPlanInfo> _eaPlanInfos = new List<EaPlanInfo>
        {
            new EaPlanInfo
            {
                Name = AbandonedCartsEaPlanName,
                ItemId = AbandonedCartsEaPlanId,
                EaPlanId = AbandonedCartsEaPlanBranchTemplateId
            },
            new EaPlanInfo
            {
                Name = NewOrderPlacedEaPlanName,
                ItemId = NewOrderPlacedEaPlanId,
                EaPlanId = NewOrderPlacedEaPlanBranchTemplateId
            },
            new EaPlanInfo
            {
                Name = ProductsBackInStockEaPlanName,
                ItemId = ProductsBackInStockEaPlanId,
                EaPlanId = ProductsBackInStockEaPlanBranchTemplateId
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

        private void CreateEaPlans()
        {
            var database = Context.ContentDatabase ?? Database.GetDatabase("master");

            foreach (var plan in _eaPlanInfos)
            {
                var item = database.GetItem(ID.Parse(plan.EaPlanId));

                if (item != null)
                {
                    plan.Name = $"Storefront {item.DisplayName}";
                    ItemManager.AddFromTemplate(plan.Name, ID.Parse(plan.EaPlanId), database.GetItem(CommerceConnectEaPlanParentId), ID.Parse(plan.ItemId));
                    continue;
                }

                Log.Error($"Error creating engagement plan '{plan.Name}'.", this);
            }
        }

        private void DeployEaPlans()
        {
            foreach (var planInfo in _eaPlanInfos)
            {
                var engagementPlanItem = Tracker.DefinitionDatabase.Automation().EngagementPlans[planInfo.Name];

                if (engagementPlanItem.IsDeployed)
                {
                    continue;
                }

                var result = ((Item) engagementPlanItem).State.GetWorkflow().Execute(DeployCommandId, engagementPlanItem, string.Empty, false).Succeeded;

                if (!result)
                {
                    Log.Error($"Error deploying engagement plan '{planInfo.Name}'.", this);
                }
            }
        }
    }
}