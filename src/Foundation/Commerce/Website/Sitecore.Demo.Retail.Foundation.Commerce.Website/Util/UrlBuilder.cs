//-----------------------------------------------------------------------
// <copyright file="UrlBuilder.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>UrlBuilder helper class</summary>
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
using System.Web;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Util
{
    public class UrlBuilder : UriBuilder
    {
        public UrlBuilder()
        {
        }

        public UrlBuilder(string uri) : base(uri)
        {
            PopulateQueryString(uri);
        }

        public UrlBuilder(Uri uri) : base(uri)
        {
            PopulateQueryString(uri.AbsoluteUri);
        }

        public UrlBuilder(string schemeName, string hostName) : base(schemeName, hostName)
        {
        }

        public UrlBuilder(string scheme, string host, int portNumber) : base(scheme, host, portNumber)
        {
        }

        public UrlBuilder(string scheme, string host, int port, string pathValue) : base(scheme, host, port, pathValue)
        {
        }

        public UrlBuilder(string scheme, string host, int port, string path, string extraValue) : base(scheme, host, port, path, extraValue)
        {
        }

        protected UrlBuilder(HttpRequest request) : base(request.Url.Scheme, request.Url.Host, request.Url.Port, request.Url.LocalPath)
        {
            PopulateQueryString(request);
        }

        public static UrlBuilder CurrentUrl => new UrlBuilder(HttpContext.Current.Request.Url);

        public QueryStringCollection QueryList { get; private set; } = new QueryStringCollection();

        public new string ToString()
        {
            SetQuery();
            return Uri.AbsoluteUri;
        }

        public string ToString(bool relative)
        {
            if (relative)
            {
                SetQuery();
                return Uri.PathAndQuery;
            }
            return ToString();
        }

        private void PopulateQueryString(HttpRequest request)
        {
            QueryList = new QueryStringCollection();

            foreach (var key in request.QueryString.AllKeys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    QueryList.Add(key, request.QueryString[key]);
                }
            }

            SetQuery();
        }

        private void PopulateQueryString(string url)
        {
            QueryList = new QueryStringCollection(url);
            SetQuery();
        }

        private void SetQuery()
        {
            Query = QueryList.ToString().TrimStart('?');
        }
    }
}