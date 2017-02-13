//-----------------------------------------------------------------------
// <copyright file="SearchNavigation.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the SearchNavigation class.</summary>
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Search;
using Sitecore.Commerce.Connect.CommerceServer.Search.Models;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce;
using Sitecore.Foundation.Commerce.Models.Search;

namespace Sitecore.Reference.Storefront
{
    public static class SearchNavigation
    {
        private static string CurrentLanguageName => Context.Language.Name;

        public static SearchResponse GetNavigationCategories(string navigationDataSource, CommerceSearchOptions searchOptions)
        {
            ID navigationId;
            var searchManager = CommerceTypeLoader.CreateInstance<ICommerceSearchManager>();
            var searchIndex = searchManager.GetIndex();

            if (navigationDataSource.IsGuid())
            {
                navigationId = ID.Parse(navigationDataSource);
            }
            else
            {
                using (var context = searchIndex.CreateSearchContext())
                {
                    var query = LinqHelper.CreateQuery<SitecoreUISearchResultItem>(context, SearchStringModel.ParseDatasourceString(navigationDataSource)).Select(result => result.GetItem().ID);
                    if (query.Any())
                    {
                        navigationId = query.First();
                    }
                    else
                    {
                        return new SearchResponse();
                    }
                }
            }

            using (var context = searchIndex.CreateSearchContext())
            {
                var searchResults = context.GetQueryable<CommerceBaseCatalogSearchResultItem>()
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Category)
                    .Where(item => item.Language == CurrentLanguageName)
                    .Where(item => item.CommerceAncestorIds.Contains(navigationId))
                    .Select(p => new CommerceBaseCatalogSearchResultItem
                    {
                        ItemId = p.ItemId,
                        Uri = p.Uri
                    });

                searchResults = searchManager.AddSearchOptionsToQuery(searchResults, searchOptions);

                var results = searchResults.GetResults();
                var response = SearchResponse.CreateFromSearchResultsItems(searchOptions, results);

                return response;
            }
        }

        public static SearchResponse SearchCatalogItemsByKeyword(string keyword, string catalogName, CommerceSearchOptions searchOptions)
        {
            Assert.ArgumentNotNullOrEmpty(catalogName, "catalogName");
            var searchManager = CommerceTypeLoader.CreateInstance<ICommerceSearchManager>();
            var searchIndex = searchManager.GetIndex(catalogName);

            using (var context = searchIndex.CreateSearchContext())
            {
                var searchResults = context.GetQueryable<CommerceProductSearchResultItem>()
                    .Where(item => item.Name.Equals(keyword) || item["_displayname"].Equals(keyword) || item.Content.Contains(keyword))
                    .Where(item => item.CommerceSearchItemType == CommerceSearchResultItemType.Product || item.CommerceSearchItemType == CommerceSearchResultItemType.Category)
                    .Where(item => item.CatalogName == catalogName)
                    .Where(item => item.Language == CurrentLanguageName)
                    .Select(p => new CommerceProductSearchResultItem
                    {
                        ItemId = p.ItemId,
                        Uri = p.Uri
                    });

                searchResults = searchManager.AddSearchOptionsToQuery(searchResults, searchOptions);

                var results = searchResults.GetResults();
                var response = SearchResponse.CreateFromSearchResultsItems(searchOptions, results);

                return response;
            }
        }

        public static SearchResponse SearchSiteByKeyword(string keyword, CommerceSearchOptions searchOptions)
        {
            const string indexNameFormat = "sitecore_{0}_index";
            var indexName = string.Format(
                CultureInfo.InvariantCulture,
                indexNameFormat,
                Context.Database.Name);

            var searchIndex = ContentSearchManager.GetIndex(indexName);
            using (var context = searchIndex.CreateSearchContext())
            {
                //var rootSearchPath = Sitecore.IO.FileUtil.MakePath(Sitecore.Context.Site.ContentStartPath, "Home", '/');
                var searchResults = context.GetQueryable<SearchResultItem>();
                searchResults = searchResults.Where(item => item.Path.StartsWith(Context.Site.ContentStartPath));
                searchResults = searchResults.Where(item => item[Foundation.Commerce.Constants.CommerceIndex.Fields.SiteContentItem] == "1");
                searchResults = searchResults.Where(item => item.Language == CurrentLanguageName);
                searchResults = searchResults.Where(GetContentExpression(keyword));
                searchResults = searchResults.Page(searchOptions.StartPageIndex, searchOptions.NumberOfItemsToReturn);

                var results = searchResults.GetResults();
                var response = SearchResponse.CreateFromSearchResultsItems(searchOptions, results);

                return response;
            }
        }

        private static Expression<Func<SearchResultItem, bool>> GetContentExpression(string searchPhrase)
        {
            if (string.IsNullOrWhiteSpace(searchPhrase))
            {
                return PredicateBuilder.False<SearchResultItem>();
            }

            Expression<Func<SearchResultItem, bool>> predicate = null;
            var termList = searchPhrase.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var term in termList)
            {
                if (predicate == null)
                {
                    predicate = PredicateBuilder.Create<SearchResultItem>(item => item.Content.Contains(term));
                }
                else
                {
                    predicate = predicate.And(item => item.Content.Contains(term));
                }
            }

            return predicate;
        }
    }
}