//-----------------------------------------------------------------------
// <copyright file="ExcludeUrlTracking.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>startTracking pipeline processor to stop tracker from registering tracking 
// information on configured urls.</summary>
//-----------------------------------------------------------------------
// Copyright 2016 Sitecore Corporation A/S
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
// except in compliance with the License. You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the 
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
// either express or implied. See the License for the specific language governing permissions 
// and limitations under the License.
// -------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml;
using Sitecore.Analytics.Pipelines.StartTracking;
using Sitecore.Configuration;
using Sitecore.Pipelines;

namespace Sitecore.Foundation.Commerce.Infrastructure.SitecorePipelines
{
    public class ExcludeUrlTracking
    {
        private static List<string> _urlsToExclude;
        private static readonly object _urlToExcludeLock = new object();

        public virtual void Process(PipelineArgs args)
        {
            var arguments = (StartTrackingArgs) args;

            LoadExclusionList();

            var url = arguments.HttpContext.Request.Url;

            var foundUrl = _urlsToExclude.Find(u => url.AbsolutePath.IndexOf(u, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(foundUrl))
            {
                args.AbortPipeline();
            }
        }

        protected virtual void LoadExclusionList()
        {
            if (_urlsToExclude == null)
            {
                lock (_urlToExcludeLock)
                {
                    if (_urlsToExclude == null)
                    {
                        var exclusionList = new List<string>();

                        var configNodes = Factory.GetConfigNodes("excludeUrlTracking/url");
                        if (configNodes != null)
                        {
                            foreach (XmlElement node in configNodes)
                            {
                                exclusionList.Add(node.InnerText);
                            }
                        }

                        _urlsToExclude = exclusionList;
                    }
                }
            }
        }
    }
}