// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentSellableItemsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.AdventureWorks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Availability;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Pricing;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which bootstraps sellable items the AdventureWorks sample environment.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(AwConstants.Pipelines.Blocks.BootstrapAwSellableItemsBlock)]
    public class InitializeEnvironmentSellableItemsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentSellableItemsBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">
        /// The find entity pipeline.
        /// </param>
        public InitializeEnvironmentSellableItemsBlock(IPersistEntityPipeline persistEntityPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
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
        public override Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var artifactSet = "Environment.AdventureWorks.SellableItems-1.0";

            // Check if this environment has subscribed to this Artifact Set
            if (!context.GetPolicy<EnvironmentInitializationPolicy>()
                .InitialArtifactSets.Contains(artifactSet))
            {
                return Task.FromResult(arg);
            }

            context.Logger.LogInformation($"{this.Name}.InitializingArtifactSet: ArtifactSet={artifactSet}");


            // CS PRODUCTS
            this.BootstrapCsGiftCards(context);
            this.BootstrapCsSleepingBags(context);
            this.BootstrapCsBoots(context);
            this.BootstrapCsTents(context);
            this.BootstrapCsParkas(context);
            this.BootstrapCsPants(context);
            this.BootstrapCsHarnesses(context);
            this.BootstrapCsCrampos(context);
            this.BootstrapCsCarabines(context);
            this.BootstrapCsRockshoes(context);
            this.BootstrapCsBackpacks(context);
            this.BootstrapCsShirts(context);
            this.BootstrapCsSupplies(context);

            return Task.FromResult(arg);
        }

        /// <summary>
        /// Bootstraps the cs gift cards.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsGiftCards(CommercePipelineExecutionContext context)
        {
            var giftCard = new SellableItem
            {
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}22565422120",
                Name = "Default GiftCard",
                Policies = new List<Policy>
                {
                    new AvailabilityAlwaysPolicy(),
                    new ListPricingPolicy(new List<Money> { new Money("USD", 0M), new Money("CAD", 0M) })
                },
                Components = new List<Component>
                {
                    new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() } },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                            {
                                new ItemVariationComponent
                                {
                                    Id = "010",
                                    Name = "Gift Card",
                                    Policies = new List<Policy>
                                    {
                                        new AvailabilityAlwaysPolicy(),
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 10M), new Money("CAD", 11M) })
                                    }
                                },
                                new ItemVariationComponent
                                {
                                    Id = "020",
                                    Name = "Gift Card",
                                    Policies = new List<Policy>
                                    {
                                        new AvailabilityAlwaysPolicy(),
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 20M), new Money("CAD", 21M) })
                                    }
                                },
                                new ItemVariationComponent
                                {
                                    Id = "025",
                                    Name = "Gift Card",
                                    Policies = new List<Policy>
                                    {
                                        new AvailabilityAlwaysPolicy(),
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 25M), new Money("CAD", 26M) })
                                    }
                                },
                                new ItemVariationComponent
                                {
                                    Id = "050",
                                    Name = "Gift Card",
                                    Policies = new List<Policy>
                                    {
                                        new AvailabilityAlwaysPolicy(),
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                                    }
                                },
                                new ItemVariationComponent
                                {
                                    Id = "100",
                                    Name = "Gift Card",
                                    Policies = new List<Policy>
                                    {
                                        new AvailabilityAlwaysPolicy(),
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 100M), new Money("CAD", 101M) })
                                    }
                                }
                            }
                     }
                }
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(giftCard), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs sleeping bags.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsSleepingBags(CommercePipelineExecutionContext context)
        {
            var sleepingBag = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                            {
                                new ItemVariationComponent
                                {
                                    Id = "3",
                                    Name = "Big Sur (Blue)",
                                    Policies = new List<Policy>
                                    {
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 220M), new Money("CAD", 221M) })
                                    },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                                }
                            }
                     }
                },
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 220M), new Money("CAD", 221M) }) },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW200-12",
                Name = "Big Sur"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(sleepingBag), context).Wait();

            var sleepingbag2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                            {
                                new ItemVariationComponent
                                {
                                    Id = "4",
                                    Name = "Day Hike (Blue)",
                                    Policies = new List<Policy>
                                    {
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 212M), new Money("CAD", 213M) })
                                    },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                                }
                            }
                    }
                },
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 212M), new Money("CAD", 213M) }) },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW210-12",
                Name = "Day Hike"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(sleepingbag2), context).Wait();

            var sleepingBag3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                            {
                                new ItemVariationComponent
                                {
                                    Id = "2",
                                    Name = "Polar Star (Black)",
                                    Policies = new List<Policy>
                                    {
                                        new ListPricingPolicy(new List<Money> { new Money("USD", 320M), new Money("CAD", 321M) })
                                    },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                                }
                            }
                    }
                },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW310-12",
                Name = "Polar Star",
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 320M), new Money("CAD", 321M) }) }
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(sleepingBag3), context).Wait();

            var sleepingBag4 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "1",
                                Name = "North Face Sunspot (Red)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money>
                                    {
                                        new Money("USD", 402M),
                                        new Money("CAD", 403M)
                                    })
                                },
                                ChildComponents = new List<Component>
                                {
                                    new PhysicalItemComponent()
                                }
                            }
                        }
                    }
                },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW390-12",
                Name = "North Face Sunspot",
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 402M), new Money("CAD", 403M) }) }
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(sleepingBag4), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs boots.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsBoots(CommercePipelineExecutionContext context)
        {
            var boot = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "9",
                                Name = "Dunes (Light green)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money>
                                    {
                                        new Money("USD", 123M),
                                        new Money("CAD", 124M)
                                    })
                                },
                                ChildComponents = new List<Component>
                                {
                                    new PhysicalItemComponent()
                                }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 123M), new Money("CAD", 124M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW074-04",
                Name = "Dunes"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(boot), context).Wait();

            var boot2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "6",
                                Name = "Rockies (Green)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 85M), new Money("CAD", 86M) })
                                },
                                ChildComponents = new List<Component>
                                {
                                    new PhysicalItemComponent()
                                }
                            },
                            new ItemVariationComponent
                            {
                                Id = "7",
                                Name = "Rockies (Gray)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 85M), new Money("CAD", 86M) })
                                },
                                ChildComponents = new List<Component>
                                {
                                    new PhysicalItemComponent()
                                }
                            }
                        }
                    }
                },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW078-04",
                Name = "Rockies",
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 85M), new Money("CAD", 86M) }) }
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(boot2), context).Wait();

            var boot3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "8",
                                Name = "Sierras (Dark brown)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 90M), new Money("CAD", 91M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 90M), new Money("CAD", 91M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW087-04",
                Name = "Sierras"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(boot3), context).Wait();

            var boot4 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "5",
                                Name = "Everglades (Brown)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 105M), new Money("CAD", 106M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW098-04",
                Name = "Everglades",
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 105M), new Money("CAD", 106M) }) }
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(boot4), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs tents.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsTents(CommercePipelineExecutionContext context)
        {
            var tent = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "16",
                                Name = "Scirocco (Black)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 190M), new Money("CAD", 191M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "17",
                                Name = "Scirocco (Grey)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 190M), new Money("CAD", 191M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "18",
                                Name = "Scirocco (Purple)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 190M), new Money("CAD", 191M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                        }
                    }
                },
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 190M), new Money("CAD", 191M) }) },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW190-11",
                Name = "Scirocco"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(tent), context).Wait();

            var tent2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "14",
                                Name = "Aptos (Green)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 340M), new Money("CAD", 341M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "15",
                                Name = "Aptos (Black)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 340M), new Money("CAD", 341M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW335-11",
                Name = "Aptos",
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 340M), new Money("CAD", 341M) }) }
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(tent2), context).Wait();

            var tent3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "10",
                                Name = "Starlight (Black)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 425M), new Money("CAD", 426M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "11",
                                Name = "Starlight (Grey)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 425M), new Money("CAD", 426M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW425-11",
                Name = "Starlight",
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 425M), new Money("CAD", 426M) }) }
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(tent3), context).Wait();

            var tent4 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "12",
                                Name = "Galaxy (Beige)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 535M), new Money("CAD", 536M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "13",
                                Name = "Galaxy (Purple)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 535M), new Money("CAD", 536M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW535-11",
                Name = "Galaxy",
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 535M), new Money("CAD", 536M) }) }
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(tent4), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs parkas.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsParkas(CommercePipelineExecutionContext context)
        {
            var parka = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent(),
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "23",
                                Name = "Sahara (Green)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 120M), new Money("CAD", 121M) })
                                }
                            },
                            new ItemVariationComponent
                            {
                                Id = "24",
                                Name = "Sahara (Purple)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 120M), new Money("CAD", 121M) })
                                }
                            }
                        }
                    }
                },
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 120M), new Money("CAD", 121M) }) },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW114-06",
                Name = "Sahara"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(parka), context).Wait();

            var parka2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent(),
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "25",
                                Name = "Crystal (Purple)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 125M), new Money("CAD", 126M) })
                                }
                            },
                            new ItemVariationComponent
                            {
                                Id = "26",
                                Name = "Crystal (Blue)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 125M), new Money("CAD", 126M) })
                                }
                            }
                        }
                    }
                },
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 125M), new Money("CAD", 126M) }) },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW125-09",
                Name = "Crystal"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(parka2), context).Wait();

            var parka3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent(),
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "19",
                                Name = "Alpine (Red)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 190M), new Money("CAD", 191M) })
                                }
                            },
                            new ItemVariationComponent
                            {
                                Id = "20",
                                Name = "Alpine (Red)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 190M), new Money("CAD", 191M) })
                                }
                            },
                            new ItemVariationComponent
                            {
                                Id = "21",
                                Name = "Alpine (Black)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 190M), new Money("CAD", 191M) })
                                }
                            },
                        }
            }
                },
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 190M), new Money("CAD", 191M) }) },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW188-06",
                Name = "Alpine"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(parka3), context).Wait();

            var parka4 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent(),
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "22",
                                Name = "Campos (Blue)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 250M), new Money("CAD", 251M) })
                                }
                            }
                        }
                    }
                },
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 250M), new Money("CAD", 251M) }) },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW250-06",
                Name = "Campos"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(parka4), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs pants.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsPants(CommercePipelineExecutionContext context)
        {
            var pant = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "27",
                                Name = "Women's 4 pocket pant. (Green, Size 2)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "28",
                                Name = "Women's 4 pocket pant. (Beige, Size 2)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "37",
                                Name = "Women's 4 pocket pant. (Green, Size 16)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "38",
                                Name = "Women's 4 pocket pant. (Beige, Size 16)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW032-01",
                Name = "Women's 4 pocket pant."
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(pant), context).Wait();

            var pant2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "29",
                                Name = "Men's 8-pocket conversion pants. (Gray, Size 28)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 46M), new Money("CAD", 47M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "30",
                                Name = "Men's 8-pocket conversion pants. (Brown, Size 28)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 46M), new Money("CAD", 47M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "39",
                                Name = "Men's 8-pocket conversion pants. (Gray, Size 46)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 46M), new Money("CAD", 47M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "40",
                                Name = "Men's 8-pocket conversion pants. (Brown, Size 46)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 46M), new Money("CAD", 47M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 46M), new Money("CAD", 47M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW046-01",
                Name = "Men's 8-pocket conversion pants."
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(pant2), context).Wait();

            var pant3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "31",
                                Name = "Unisex drawstring pants. (Green, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "32",
                                Name = "Unisex drawstring pants. (Brown, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "41",
                                Name = "Unisex drawstring pants. (Green, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "42",
                                Name = "Unisex drawstring pants. (Brown, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "51",
                                Name = "Unisex drawstring pants. (Green, Size L)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "52",
                                Name = "Unisex drawstring pants. (Brown, Size L)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 50M), new Money("CAD", 51M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW048-01",
                Name = "Unisex drawstring pants."
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(pant3), context).Wait();

            var pant4 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                                {
                                    new ItemVariationComponent
                                    {
                                            Id = "33",
                                            Name = "Unisex hiking pants (Green, Size M)",
                                            Policies = new List<Policy>
                                            {
                                                new ListPricingPolicy(new List<Money> { new Money("USD", 58M), new Money("CAD", 59M) })
                                            },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                                    },
                                    new ItemVariationComponent
                                    {
                                            Id = "34",
                                            Name = "Unisex hiking pants (Beige, Size L)",
                                            Policies = new List<Policy>
                                            {
                                                new ListPricingPolicy(new List<Money> { new Money("USD", 58M), new Money("CAD", 59M) })
                                            },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                                    },
                                    new ItemVariationComponent
                                    {
                                            Id = "43",
                                            Name = "Unisex hiking pants (Green, Size L)",
                                            Policies = new List<Policy>
                                            {
                                                new ListPricingPolicy(new List<Money> { new Money("USD", 58M), new Money("CAD", 59M) })
                                            },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                                    },
                                    new ItemVariationComponent
                                    {
                                            Id = "44",
                                            Name = "Unisex hiking pants (Beige, Size XL)",
                                            Policies = new List<Policy>
                                            {
                                                new ListPricingPolicy(new List<Money> { new Money("USD", 58M), new Money("CAD", 59M) })
                                            },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                                    },
                                    new ItemVariationComponent
                                    {
                                            Id = "53",
                                            Name = "Unisex hiking pants (Green, Size XL)",
                                            Policies = new List<Policy>
                                            {
                                                new ListPricingPolicy(new List<Money> { new Money("USD", 58M), new Money("CAD", 59M) })
                                            },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                                    },
                                    new ItemVariationComponent
                                    {
                                            Id = "54",
                                            Name = "Unisex hiking pants (Beige, Size XXL)",
                                            Policies = new List<Policy>
                                            {
                                                new ListPricingPolicy(new List<Money> { new Money("USD", 58M), new Money("CAD", 59M) })
                                            },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                                    }
                                }
                    }
                },
                Policies = new List<Policy> { new ListPricingPolicy(new List<Money> { new Money("USD", 58M), new Money("CAD", 59M) }) },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW055-01",
                Name = "Unisex hiking pants"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(pant4), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs harnesses.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsHarnesses(CommercePipelineExecutionContext context)
        {
            var harness = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "36",
                                Name = "Black Diamond Alpine Bod (Black)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 33M), new Money("CAD", 34M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 33M), new Money("CAD", 34M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW029-10",
                Name = "Black Diamond Alpine Bod"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(harness), context).Wait();

            var harness2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "35",
                                Name = "Black Diamond Bod (Assorted colors)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 33M), new Money("CAD", 34M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 33M), new Money("CAD", 34M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW032-10",
                Name = "Black Diamond Bod"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(harness2), context).Wait();

            var harness3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "37",
                                Name = "El Capitan (Blue)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 60M), new Money("CAD", 61M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 60M), new Money("CAD", 61M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW053-10",
                Name = "El Capitan"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(harness3), context).Wait();

            var harness4 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "38",
                                Name = "Petzl Mercury (Assorted colors)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 75M), new Money("CAD", 76M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 75M), new Money("CAD", 76M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW072-10",
                Name = "Petzl Mercury"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(harness4), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs crampos.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsCrampos(CommercePipelineExecutionContext context)
        {
            var crampon = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 109M), new Money("CAD", 110M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW109-15",
                Name = "Wafflestomper"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(crampon), context).Wait();

            var crampon2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 150M), new Money("CAD", 151M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW145-15",
                Name = "Edgehugger"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(crampon2), context).Wait();

            var crampon3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 149M), new Money("CAD", 150M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW149-15",
                Name = "Glory Grip"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(crampon3), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs carabines.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsCarabines(CommercePipelineExecutionContext context)
        {
            var carabiner = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 14M), new Money("CAD", 15M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW014-08",
                Name = "Petzl Spirit"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(carabiner), context).Wait();

            var carabiner2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 10M), new Money("CAD", 11M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW007-08",
                Name = "Black Diamond Quicksilver II"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(carabiner2), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs rockshoes.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsRockshoes(CommercePipelineExecutionContext context)
        {
            var rockshoe = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 135M), new Money("CAD", 136M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW125-05",
                Name = "Plymouth"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(rockshoe), context).Wait();

            var rockshoe2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 95M), new Money("CAD", 96M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW082-05",
                Name = "Morro"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(rockshoe2), context).Wait();

            var rockshoe3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 124M), new Money("CAD", 125M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW086-06",
                Name = "Tuscany"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(rockshoe3), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs backpacks.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsBackpacks(CommercePipelineExecutionContext context)
        {
            var backpack = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "41",
                                Name = "University (Blau)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money>
                                    {
                                        new Money("USD", 155M),
                                        new Money("CAD", 156M)
                                    })
                                },
                                ChildComponents = new List<Component>
                                {
                                    new PhysicalItemComponent()
                                }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 155M), new Money("CAD", 156M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW140-13",
                Name = "University"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(backpack), context).Wait();

            var backpack2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "42",
                                Name = "Pacific (Blue)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money>
                                    {
                                        new Money("USD", 151M),
                                        new Money("CAD", 152M)
                                    })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 151M), new Money("CAD", 152M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW151-13",
                Name = "Pacific"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(backpack2), context).Wait();

            var backpack3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "43",
                                Name = "Conestoga (Red)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money>
                                    {
                                        new Money("USD", 184M),
                                        new Money("CAD", 185M)
                                    })
                                },
                                ChildComponents = new List<Component>
                                {
                                    new PhysicalItemComponent()
                                }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 184M), new Money("CAD", 185M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW175-13",
                Name = "Conestoga"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(backpack3), context).Wait();

            var backpack4 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "39",
                                Name = "Aces (Blue)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money>
                                    {
                                        new Money("USD", 325M),
                                        new Money("CAD", 326M)
                                    })
                                },
                                ChildComponents = new List<Component>
                                {
                                    new PhysicalItemComponent()
                                }
                            },
                            new ItemVariationComponent
                            {
                                Id = "40",
                                Name = "Aces (Black)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money>
                                    {
                                        new Money("USD", 325M),
                                        new Money("CAD", 326M)
                                    })
                                },
                                ChildComponents = new List<Component>
                                {
                                    new PhysicalItemComponent()
                                }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 325M), new Money("CAD", 326M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW325-13",
                Name = "Aces"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(backpack4), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs shirts.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsShirts(CommercePipelineExecutionContext context)
        {
            var shirt = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "44",
                                Name = "Women's woven tee (Blue, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "45",
                                Name = "Women's woven tee (Beige, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                                },
                                ChildComponents = new List<Component>
                                {
                                    new PhysicalItemComponent()
                                }
                            },
                            new ItemVariationComponent
                            {
                                Id = "46",
                                Name = "Women's woven tee (Green, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M), new Money("EUR", 33M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "54",
                                Name = "Women's woven tee (Blue, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "55",
                                Name = "Women's woven tee (Beige, Size L)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M), new Money("EUR", 32M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "56",
                                Name = "Women's woven tee (Green, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "64",
                                Name = "Women's woven tee (Blue, Size XL)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 35M), new Money("CAD", 36M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW029-03",
                Name = "Women's woven tee"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(shirt), context).Wait();

            var shirt2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "51",
                                Name = "Men's button-down (Blue, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 48M), new Money("CAD", 49M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "52",
                                Name = "Men's button-down (Red, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 48M), new Money("CAD", 49M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "53",
                                Name = "Men's button-down (Beige, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 48M), new Money("CAD", 49M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "61",
                                Name = "Men's button-down (Blue, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 48M), new Money("CAD", 49M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "62",
                                Name = "Men's button-down (Red ,Size L)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 48M), new Money("CAD", 49M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "63",
                                Name = "Men's button-down (Beige, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 48M), new Money("CAD", 49M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "71",
                                Name = "Men's button-down (Blue, Size L)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 48M), new Money("CAD", 49M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 48M), new Money("CAD", 49M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW042-03",
                Name = "Men's button-down"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(shirt2), context).Wait();

            var shirt3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "47",
                                Name = "Men's loose-weave polo (Red, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "48",
                                Name = "Men's loose-weave polo (Blue, Size L)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "49",
                                Name = "Men's loose-weave polo (White, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "50",
                                Name = "Men's loose-weave polo (Beige, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "57",
                                Name = "Men's loose-weave polo (Red, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "58",
                                Name = "Men's loose-weave polo (Blue, Size XL)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "59",
                                Name = "Men's loose-weave polo (White, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "60",
                                Name = "Men's loose-weave polo (Beige, Size L)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "67",
                                Name = "Men's loose-weave polo (Red, Size XXL)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "68",
                                Name = "Men's loose-weave polo (Blue, Size XXL)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "69",
                                Name = "Men's loose-weave polo (White, Size XL)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "70",
                                Name = "Men's loose-weave polo (Beige, Size XL)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 45M), new Money("CAD", 46M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW045-03",
                Name = "Men's loose-weave polo"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(shirt3), context).Wait();

            var shirt4 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new ItemVariationsComponent
                    {
                        ChildComponents = new List<Component>
                        {
                            new ItemVariationComponent
                            {
                                Id = "54",
                                Name = "Unisex long-sleeve button-down (Green, Size S)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 75M), new Money("CAD", 76M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "55",
                                Name = "Unisex long-sleeve button-down (Beige, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 75M), new Money("CAD", 76M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "64",
                                Name = "Unisex long-sleeve button-down (Green, Size M)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 75M), new Money("CAD", 76M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "65",
                                Name = "Unisex long-sleeve button-down (Beige, Size L)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 75M), new Money("CAD", 75M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "74",
                                Name = "Unisex long-sleeve button-down (Green, Size XL)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 75M), new Money("CAD", 76M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            },
                            new ItemVariationComponent
                            {
                                Id = "75",
                                Name = "Unisex long-sleeve button-down (Beige, Size XL)",
                                Policies = new List<Policy>
                                {
                                    new ListPricingPolicy(new List<Money> { new Money("USD", 75M), new Money("CAD", 76M) })
                                },
                                    ChildComponents = new List<Component>
                                    {
                                        new PhysicalItemComponent()
                                    }
                            }
                        }
                    }
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 75M), new Money("CAD", 76M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW075-03",
                Name = "Unisex long-sleeve button-down"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(shirt4), context).Wait();
        }

        /// <summary>
        /// Bootstraps the cs supplies.
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void BootstrapCsSupplies(CommercePipelineExecutionContext context)
        {
            var supply = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 44.75M), new Money("CAD", 45.75M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW051-14",
                Name = "Wolfgang"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(supply), context).Wait();

            var supply2 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 59M), new Money("CAD", 60M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW475-14",
                Name = "Scoutpride"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(supply2), context).Wait();

            var supply3 = new SellableItem
            {
                Components = new List<Component>
                {
                    new CatalogComponent { Name = "Adventure Works Catalog" },
                    new ListMembershipsComponent
                    {
                        Memberships = new List<string> { CommerceEntity.ListName<SellableItem>() }
                    },
                    new PhysicalItemComponent()
                },
                Policies =
                    new List<Policy>
                    {
                        new ListPricingPolicy(new List<Money> { new Money("USD", 9M), new Money("CAD", 10M) })
                    },
                Id = $"{CommerceEntity.IdPrefix<SellableItem>()}AW425-14",
                Name = "Surelite"
            };
            this._persistEntityPipeline.Run(new PersistEntityArgument(supply3), context).Wait();
        }
    }
}
