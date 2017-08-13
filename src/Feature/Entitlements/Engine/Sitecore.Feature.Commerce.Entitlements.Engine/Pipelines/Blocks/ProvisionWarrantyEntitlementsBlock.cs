using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Availability;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Entitlements;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Feature.Commerce.Entitlements.Engine.Entities;
using Sitecore.Feature.Commerce.Entitlements.Engine.Policies;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Feature.Commerce.Entitlements.Engine.Pipelines.Blocks
{

    [PipelineDisplayName(EntitlementsConstants.Pipelines.Blocks.ProvisionWarrantyEntitlementsBlock)]
    public class ProvisionWarrantyEntitlementsBlock : PipelineBlock<IEnumerable<Entitlement>, IEnumerable<Entitlement>, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        public ProvisionWarrantyEntitlementsBlock(
            IPersistEntityPipeline persistEntityPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
        }

        public override async Task<IEnumerable<Entitlement>> Run(IEnumerable<Entitlement> arg, CommercePipelineExecutionContext context)
        {
            var entitlements = arg as List<Entitlement> ?? arg.ToList();
            Condition.Requires(entitlements).IsNotNull($"{this.Name}: The entitlements can not be null");

            var argument = context.CommerceContext.GetObjects<ProvisionEntitlementsArgument>().FirstOrDefault();
            if (argument == null)
            {
                return entitlements.AsEnumerable();
            }

            var customer = argument.Customer;
            var order = argument.Order;
            if (order == null)
            {
                return entitlements.AsEnumerable();
            }

            var digitalTags = context.GetPolicy<KnownEntitlementsTags>().WarrantyTags;
            var lineWithDigitalGoods = order.Lines.Where(line => line != null
                && line.GetComponent<CartProductComponent>().HasPolicy<AvailabilityAlwaysPolicy>()
                && line.GetComponent<CartProductComponent>().Tags.Select(t => t.Name).Intersect(digitalTags, StringComparer.OrdinalIgnoreCase).Any()).ToList();
            if (!lineWithDigitalGoods.Any())
            {
                return entitlements.AsEnumerable();
            }

            var hasErrors = false;
            foreach (var line in lineWithDigitalGoods)
            {
                foreach (var index in Enumerable.Range(1, (int)line.Quantity))
                {
                    try
                    {
                        var entitlement = new Warranty();
                        var id = Guid.NewGuid().ToString("N");
                        entitlement.Id = $"{CommerceEntity.IdPrefix<Warranty>()}{id}";
                        entitlement.FriendlyId = id;
                        entitlement.SetComponent(
                            new ListMembershipsComponent
                            {
                                Memberships =
                                        new List<string>
                                            {
                                                $"{CommerceEntity.ListName<Warranty>()}",
                                                $"{CommerceEntity.ListName<Entitlement>()}"
                                            }
                            });
                        entitlement.Order = new EntityReference(order.Id, order.Name);
                        if (customer != null)
                        {
                            entitlement.Customer = new EntityReference(customer.Id, customer.Name);
                        }

                        await this._persistEntityPipeline.Run(new PersistEntityArgument(entitlement), context);
                        entitlements.Add(entitlement);
                        context.Logger.LogInformation(
                            $"Warranty Entitlement Created - Order={order.Id}, LineId={line.Id}, EntitlementId={entitlement.Id}");
                    }
                    catch
                    {
                        hasErrors = true;
                        context.Logger.LogError(
                            $"Warranty Entitlement NOT Created - Order={order.Id}, LineId={line.Id}");
                        break;
                    }
                }

                if (hasErrors)
                {
                    break;
                }
            }

            if (!hasErrors)
            {
                return entitlements.AsEnumerable();
            }

            context.Abort(
                context.CommerceContext.AddMessage(
                    context.GetPolicy<KnownResultCodes>().Error,
                    "ProvisioningEntitlementErrors",
                    new object[] { order.Id },
                    $"Error(s) occurred provisioning entitlements for order '{order.Id}'"),
                context);
            return null;
        }
    }
}
