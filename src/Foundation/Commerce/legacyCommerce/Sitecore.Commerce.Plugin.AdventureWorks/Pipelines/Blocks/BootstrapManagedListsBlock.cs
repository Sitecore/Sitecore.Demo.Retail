// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BootstrapManagedListsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.AdventureWorks
{
    using System.Threading.Tasks;

    using ManagedLists;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which bootstraps any ManagedLists for this Plugin.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(AwConstants.Pipelines.Blocks.BootstrapManagedListsBlock)]
    public class BootstrapManagedListsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly CreateManagedListCommand _createManagedListCommand;
        private readonly GetManagedListCommand _getManagedListCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="BootstrapManagedListsBlock"/> class.
        /// </summary>
        /// <param name="createManagedList">The create managed list.</param>
        /// <param name="getManagedListCommand">The get managed list command.</param>
        public BootstrapManagedListsBlock(
            CreateManagedListCommand createManagedList, 
            GetManagedListCommand getManagedListCommand)
        {
            this._createManagedListCommand = createManagedList;
            this._getManagedListCommand = getManagedListCommand;
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <param name="arg">
        /// The argument.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
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
