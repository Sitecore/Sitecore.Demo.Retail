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

$(document).ready(function () {
    var queryStringParamerterSort = "s";
    var queryStringParamerterSortDirection = "sd";
    var queryStringParamerterSortDirectionAsc = "ascending";
    var queryStringParamerterSortDirectionAscShort = "+";
    var queryStringParamerterSortDirectionDesc = "descending";
    var queryStringParamerterPage = "pg";
    var queryStringParamerterPageSize = "ps";

    $(".sortDropdown").change(function () {
        var val = $(this).find("option:selected").attr("value");

        if (val != null && val != "") {
            var fieldName = val.substr(0, val.length - 1);
            var direction = val.charAt(val.length - 1) == queryStringParamerterSortDirectionAscShort ? queryStringParamerterSortDirectionAsc : queryStringParamerterSortDirectionDesc;

            AJAXPost("/api/storefront/catalog/sortorderapplied", "{\"sortField\":\"" + fieldName + "\", \"sortDirection\":\"" + direction + "\"}", function (data, success, sender) {
                var url = new Uri(window.location.href)
                    .deleteQueryParam(queryStringParamerterSort)
                    .deleteQueryParam(queryStringParamerterSortDirection)
                    .addQueryParam(queryStringParamerterSort, fieldName)
                    .addQueryParam(queryStringParamerterSortDirection, direction)
                    .deleteQueryParam(queryStringParamerterPage)
                    .toString();

                window.location.href = url;
            });
        }
        else {
            resetUrl();
        }
    });

    $(".changePageSize").change(function () {
        var val = $(this).find("option:selected").attr("value");

        if (val != null && val != "") {
            var url = new Uri(window.location.href)
                .deleteQueryParam(queryStringParamerterPageSize)
                .addQueryParam(queryStringParamerterPageSize, val)
                .deleteQueryParam(queryStringParamerterPage)
                .toString();

            window.location.href = url;
        }
        else {
            resetUrl();
        }
    });
});

function resetUrl() {
    var url = new Uri(window.location.href)
        .deleteQueryParam(queryStringParamerterSort)
        .deleteQueryParam(queryStringParamerterSortDirection)
        .deleteQueryParam(queryStringParamerterPage)
        .deleteQueryParam(queryStringParamerterPageSize)
        .deleteQueryParam(queryStringParameterSiteContentPage)
        .deleteQueryParam(queryStringParameterSiteContentPageSize)
        .toString();

    window.location.href = url;
}

$(window).on("load", function () {
    setEqualHeight($(".product-list .product-item"));
});

function setEqualHeight(columns) {
    var tallestcolumn = 0;
    columns.each(function () {
        currentHeight = $(this).height();
        if (currentHeight > tallestcolumn) {
            tallestcolumn = currentHeight;
        }
    });
    columns.height(tallestcolumn);
}