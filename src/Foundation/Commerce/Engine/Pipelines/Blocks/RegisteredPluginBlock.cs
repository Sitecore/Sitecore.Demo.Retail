using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Foundation.Commerce.Engine.Pipelines.Blocks
{
    [PipelineDisplayName(CommerceEngineConstants.Pipelines.Blocks.RegisteredPluginBlock)]
    public class RegisteredPluginBlock : PipelineBlock<IEnumerable<RegisteredPluginModel>, IEnumerable<RegisteredPluginModel>, CommercePipelineExecutionContext>
    {
        public override Task<IEnumerable<RegisteredPluginModel>> Run(IEnumerable<RegisteredPluginModel> arg, CommercePipelineExecutionContext context)
        {
            if (arg == null)
            {
                return Task.FromResult(arg);
            }

            var plugins = arg.ToList();
            PluginHelper.RegisterPlugin(this, plugins);

            return Task.FromResult(plugins.AsEnumerable());
        }
    }
}
