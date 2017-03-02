// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnownEntitlementsTags.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Habitat
{
    using System.Collections.Generic;

    using Sitecore.Commerce.Core;

    /// <summary>
    /// Defines the known Habitat entitlement tags
    /// </summary>
    /// <seealso cref="Sitecore.Commerce.Core.Policy" />
    public class KnownEntitlementsTags : Policy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KnownEntitlementsTags"/> class.
        /// </summary>
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
