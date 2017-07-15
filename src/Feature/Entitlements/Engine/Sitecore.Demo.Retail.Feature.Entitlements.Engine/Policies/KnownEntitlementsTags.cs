using System.Collections.Generic;
using Sitecore.Commerce.Core;

namespace Sitecore.Demo.Retail.Feature.Entitlements.Engine.Policies
{
    public class KnownEntitlementsTags : Policy
    {
        public KnownEntitlementsTags()
        {
            this.WarrantyTags = new List<string> { "Warranty" };
            this.InstallationTags = new List<string> { "Installation", "Service" };
            this.DigitalProductTags = new List<string> { "OnlineTraining", "OnlineLearning", "Subscription", "DigitalSubscription" };
        }

        /// <summary>
        /// Gets or sets the warranty tags.
        /// </summary>
        /// <value>
        /// The warranty tags.
        /// </value>
        public IList<string> WarrantyTags { get; set; }

        /// <summary>
        /// Gets or sets the digital product tags.
        /// </summary>
        /// <value>
        /// The digital product tags.
        /// </value>
        public IList<string> DigitalProductTags { get; set; }

        /// <summary>
        /// Gets or sets the installation tags.
        /// </summary>
        /// <value>
        /// The installation tags.
        /// </value>
        public IList<string> InstallationTags { get; set; }
    }
}
