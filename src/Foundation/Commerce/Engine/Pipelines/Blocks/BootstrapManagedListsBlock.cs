using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines;
using Sitecore.Commerce.Plugin.ManagedLists;

namespace Sitecore.Foundation.Commerce.Engine.Pipelines.Blocks
{
    [PipelineDisplayName(CommerceEngineConstants.Pipelines.Blocks.BootstrapManagedListsBlock)]
    public class BootstrapManagedListsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly CreateManagedListCommand _createManagedListCommand;
        private readonly GetManagedListCommand _getManagedListCommand;

        public BootstrapManagedListsBlock(
            CreateManagedListCommand createManagedList, 
            GetManagedListCommand getManagedListCommand)
        {
            this._createManagedListCommand = createManagedList;
            this._getManagedListCommand = getManagedListCommand;
        }

        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var pendingOrdersListName = context.GetPolicy<KnownOrderListsPolicy>().PendingOrders;
            var managedList = await this._getManagedListCommand.Process(context.CommerceContext, pendingOrdersListName);
            if (managedList != null)
            {
                return arg;
            }

            await this._createManagedListCommand.Process(context.CommerceContext, pendingOrdersListName);
            return arg;
        }
    }
}
