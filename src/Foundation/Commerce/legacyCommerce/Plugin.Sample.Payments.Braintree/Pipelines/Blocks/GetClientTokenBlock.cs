﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetClientTokenBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using Braintree;
using Braintree.Exceptions;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.Payments.Braintree
{
    /// <summary>
    ///     Defines a block which gets a payment service client tokent.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(PaymentsBraintreeConstants.Pipelines.Blocks.GetClientTokenBlock)]
    public class GetClientTokenBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        /// <summary>
        ///     Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>A client token string</returns>
        public override Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var braintreeClientPolicy = context.GetPolicy<BraintreeClientPolicy>();
            if (braintreeClientPolicy == null)
            {
                context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "InvalidOrMissingPropertyValue", new object[] {"BraintreeClientPolicy"}, $"{Name}. Missing BraintreeClientPolicy");
                return Task.FromResult(arg);
            }

            if (!IsValid(context, braintreeClientPolicy?.Environment, "Environment") || 
                !IsValid(context, braintreeClientPolicy?.MerchantId, "MerchantId") || 
                !IsValid(context, braintreeClientPolicy?.PublicKey, "PublicKey") || 
                !IsValid(context, braintreeClientPolicy?.PrivateKey, "PrivateKey"))
            {
                return Task.FromResult(arg);
            }

            try
            {
                var gateway = new BraintreeGateway(braintreeClientPolicy?.Environment, braintreeClientPolicy?.MerchantId, braintreeClientPolicy?.PublicKey, braintreeClientPolicy?.PrivateKey);
                var clientToken = gateway.ClientToken.generate();
                return Task.FromResult(clientToken);
            }
            catch (BraintreeException ex)
            {
                context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "InvalidClientPolicy", new object[] {"BraintreeClientPolicy"}, $"{Name}. Invalid BraintreeClientPolicy {ex.Message}");
                return Task.FromResult(arg);
            }
        }

        private bool IsValid(CommercePipelineExecutionContext context, string arg, string name)
        {
            if (!string.IsNullOrEmpty(arg) && arg != "0")
                return true;

            context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "InvalidOrMissingPropertyValue", new object[] { "BraintreeClientPolicy" }, $"{Name}. BraintreeClientPolicy.{name} on {context.CommerceContext.Environment.Name} is not valid");
            return false;
        }
    }
}