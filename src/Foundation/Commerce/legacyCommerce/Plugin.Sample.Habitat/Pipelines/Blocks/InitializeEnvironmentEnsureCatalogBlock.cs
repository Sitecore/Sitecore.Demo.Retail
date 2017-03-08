// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentHabitatEnsureCatalogBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Habitat
{
    using System.Threading.Tasks;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Catalog.Cs;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Ensure Habitat catalog has been loaded.
    /// </summary>
    [PipelineDisplayName("Habitat.InitializeEnvironmentEnsureCatalogBlock")]
    public class InitializeEnvironmentHabitatEnsureCatalogBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly EnsureCatalogCommand _ensureCatalogCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentHabitatEnsureCatalogBlock"/> class.
        /// </summary>
        /// <param name="ensureCatalogCommand">
        /// The EnsureCatalog Command.
        /// </param>
        public InitializeEnvironmentHabitatEnsureCatalogBlock(EnsureCatalogCommand ensureCatalogCommand)
        {
            this._ensureCatalogCommand = ensureCatalogCommand;
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
            await this._ensureCatalogCommand.Process(context.CommerceContext, "Habitat_Master", @"[wwwroot]\data\Catalogs\Habitat_Master.xml", "Habitat_Inventory", @"[wwwroot]\data\Catalogs\Habitat_Inventory.xml");

            await this._ensureCatalogCommand.Process(context.CommerceContext, "Habitat_NextCubeMarketplace", @"[wwwroot]\data\Catalogs\Habitat_NextCubeMarketplace.xml", "Habitat_Inventory", @"[wwwroot]\data\Catalogs\Habitat_Inventory.xml");

            return arg;
        }
    }
}
