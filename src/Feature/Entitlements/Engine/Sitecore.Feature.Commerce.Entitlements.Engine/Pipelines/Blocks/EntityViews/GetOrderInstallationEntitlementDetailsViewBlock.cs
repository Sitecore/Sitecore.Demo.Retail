using System;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Entitlements;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Feature.Commerce.Entitlements.Engine.Entities;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Feature.Commerce.Entitlements.Engine.Pipelines.Blocks.EntityViews
{

    [PipelineDisplayName(EntitlementsConstants.Pipelines.Blocks.GetOrderInstallationEntitlementDetailsViewBlock)]
    public class GetOrderInstallationEntitlementDetailsViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly FindEntityCommand _findEntityCommand;

        public GetOrderInstallationEntitlementDetailsViewBlock(FindEntityCommand findEntityCommand)
        {
            this._findEntityCommand = findEntityCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null.");

            var request = context.CommerceContext.Objects.OfType<EntityViewArgument>().FirstOrDefault();
            if (string.IsNullOrEmpty(request?.ViewName)
               || (!request.ViewName.Equals(context.GetPolicy<KnownEntitlementsViewsPolicy>().OrderEntitlements, StringComparison.OrdinalIgnoreCase)
                   && !request.ViewName.Equals(context.GetPolicy<KnownOrderViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase))
               || !(request.Entity is Order))
            {
                return entityView;
            }

            var order = (Order)request.Entity;
            if (!order.HasComponent<EntitlementsComponent>())
            {
                return entityView;
            }

            var entitlementsView = request.ViewName.Equals(context.GetPolicy<KnownOrderViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase)
                   ? entityView.ChildViews.Cast<EntityView>().FirstOrDefault(ev => ev.Name.Equals(context.GetPolicy<KnownEntitlementsViewsPolicy>().OrderEntitlements, StringComparison.OrdinalIgnoreCase))
                   : entityView;
            var entitlementViews = entitlementsView?.ChildViews.Where(cv => cv.Name.Equals(context.GetPolicy<KnownEntitlementsViewsPolicy>().OrderEntitlementDetails, StringComparison.OrdinalIgnoreCase)).Cast<EntityView>().ToList();
            if (entitlementViews == null)
            {
                return entityView;
            }

            foreach (var entitlementView in entitlementViews)
            {
                var entitlement = await this._findEntityCommand.Process(context.CommerceContext, typeof(Entitlement), entitlementView.ItemId);
                if (entitlement == null || entitlement.HasComponent<DeletedEntitlementComponent>())
                {
                    entitlementsView.ChildViews.Remove(entitlementView);
                    continue;
                }

                var installation = entitlement as Installation;
                if (installation == null)
                {
                    continue;
                }

                entitlementView.Properties.Add(new ViewProperty { Name = "Details", IsReadOnly = true, RawValue = "Type=Installation" });
            }

            return entityView;
        }
    }
}
