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

var miniCartViewModel = null;
var miniCartItemListViewModel = null;

$(document).ready(function () {
    // This change stops the shopping chart from closing of a user clicks on it.
    $(document).on('click', 'div.dropdown-menu', function (e) {
        e.stopPropagation();
    });
});

function manageMiniCartActions() {
    $(document).ready(function () {
        $('.minicart-content').on('click', ".minicart-delete", function (event) {
            $(event.currentTarget).find(".glyphicon").removeClass("glyphicon-remove-circle");
            $(event.currentTarget).find(".glyphicon").addClass("glyphicon-refresh");
            $(event.currentTarget).find(".glyphicon").addClass("glyphicon-refresh-animate");
            var lineItem = $(event.currentTarget).parents("[data-ajax-lineitemid]");
            var lineItemId = lineItem.attr("data-ajax-lineitemid");

            ClearGlobalMessages();
            AJAXPost("/api/storefront/cart/DeleteLineItem", "{'ExternalCartLineId':'" + lineItemId + "'}", removeItemResponse, lineItem);
            return false;
        });
    });
}

function removeItemResponse(data, success, sender) {
    if (success && data.Success) {       
        $(sender).slideUp(200);
        miniCartItemListViewModel.reload(data);
    }

    $(sender).find(".glyphicon").removeClass("glyphicon-refresh");
    $(sender).find(".glyphicon").removeClass("glyphicon-refresh-animate");
    $(sender).find(".glyphicon").addClass("glyphicon-remove-circle");
    ShowGlobalMessages(data);
}

function initMiniShoppingCart(sectionId) {
    ClearGlobalMessages();
    AJAXPost("/api/storefront/cart/getcurrentcart", null, function (data, success, sender) {
        if (success && data.Success) {
            miniCartItemListViewModel = new MiniCartItemListViewModel(data);
            ko.applyBindings(miniCartItemListViewModel, document.getElementById(sectionId));
            manageMiniCartActions();
        }

        ShowGlobalMessages(data);
    });
}

function UpdateMiniCart() {
    ClearGlobalMessages();
    AJAXPost("/api/storefront/cart/getcurrentcart", null, function (data, success, sender) {
        if (success && data.Success) {
            miniCartItemListViewModel.reload(data);
        }

        ShowGlobalMessages(data);
    });
}

function initCartAmount(updateAmount) {
    var data = null;
    if (updateAmount != undefined && updateAmount) {
        data = '{ "updateCart" : true}';
    }

    ClearGlobalMessages();
    AJAXPost("/api/storefront/cart/updateminicart", null, function (data) {
        if (success && data.Success) {
            miniCartViewModel = new MiniCartViewModel(data.LineItemCount, data.Total);
            ko.applyBindings(miniCartViewModel, document.getElementById("miniCart"));
        }

        ShowGlobalMessages(data);
    }, null);
}