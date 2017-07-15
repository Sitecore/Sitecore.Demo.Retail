//-----------------------------------------------------------------------
// <copyright file="QueryStringCollection.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>QueryStringCollection helper class</summary>
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
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Util
{
    [Serializable]
    public class QueryStringCollection : NameValueCollection
    {
        public QueryStringCollection()
        {
        }

        public QueryStringCollection(string url)
        {
            Uri baseUri;
            var isValid = Uri.TryCreate(url, UriKind.Absolute, out baseUri);

            if (!isValid)
            {
                throw new UriFormatException("Invalid url format");
            }

            if (url.IndexOf("?", StringComparison.OrdinalIgnoreCase) <= -1)
            {
                return;
            }

            foreach (var keyValuePair in ExtractQueryString(url).Split('&'))
            {
                if (string.IsNullOrEmpty(keyValuePair))
                {
                    continue;
                }

                var firstEquals = keyValuePair.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                var key = keyValuePair.Substring(0, firstEquals);
                var value = HttpUtility.UrlDecode(keyValuePair.Substring(firstEquals + 1));
                Add(key, value);
            }
        }

        protected QueryStringCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static QueryStringCollection Current
        {
            get { return new QueryStringCollection().FromCurrent(); }
        }

        public void Parse(string queryString)
        {
            foreach (var keyValuePair in queryString.Split('&'))
            {
                if (string.IsNullOrEmpty(keyValuePair))
                {
                    continue;
                }

                var firstEquals = keyValuePair.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                var key = keyValuePair.Substring(0, firstEquals);
                var value = keyValuePair.Substring(firstEquals + 1);
                Add(key, value);
            }
        }

        public bool Contains(string name)
        {
            var existingValue = base[name];
            return !string.IsNullOrEmpty(existingValue);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < Keys.Count; i++)
            {
                var keyValue = Keys[i];
                if (!string.IsNullOrEmpty(keyValue) && !string.IsNullOrEmpty(base[keyValue]))
                {
                    foreach (var val in base[keyValue].Split(','))
                    {
                        builder.Append(builder.Length == 0 ? "?" : "&");
                        builder.Append(HttpUtility.UrlEncode(keyValue));
                        builder.Append("=");
                        builder.Append(HttpUtility.UrlEncode(val));
                    }
                }
            }

            return builder.ToString();
        }

        public string ExtractQueryString(string fullUrl)
        {
            if (!string.IsNullOrEmpty(fullUrl))
            {
                if (fullUrl.Contains("?"))
                {
                    return fullUrl.Substring(fullUrl.IndexOf("?", StringComparison.OrdinalIgnoreCase) + 1);
                }
            }

            return fullUrl;
        }

        public QueryStringCollection FromCurrent()
        {
            var siteContext = DependencyResolver.Current.GetService<CatalogItemContext>().Current;
            return new QueryStringCollection(HttpContext.Current.Request.Url.ToString());
        }

        public new QueryStringCollection Add(string name, string value)
        {
            return Add(name, value, false);
        }

        public void AddOrSet(string name, string value)
        {
            if (!Contains(name))
            {
                Add(name, value);
            }
            else
            {
                Set(name, value);
            }
        }

        public QueryStringCollection Add(string name, string value, bool isUnique)
        {
            var existingValue = base[name];
            if (string.IsNullOrEmpty(existingValue))
            {
                base.Add(name, value);
            }
            else if (isUnique)
            {
                base[name] = value;
            }
            else
            {
                base[name] += "," + value;
            }

            return this;
        }

        public new QueryStringCollection Remove(string name)
        {
            base.Remove(name);
            return this;
        }

        public QueryStringCollection Reset()
        {
            Clear();
            return this;
        }
    }
}