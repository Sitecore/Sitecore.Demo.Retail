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

var StockInfoListViewModel = function () {
    var self = this;

    self.stockInfos = ko.observableArray();
    self.statuses = ko.observableArray();
    self.hasInfo = ko.observable(false);
    self.selectedStockInfo = ko.observable(new StockInfoViewModel());
    self.load = function () {
        ClearGlobalMessages();
        var data = {};
        data.ProductId = $('#product-id').val();
        AJAXPost("/api/storefront/catalog/GetCurrentProductStockInfo", JSON.stringify(data), function (data, success, sender) {
            if (success && data && data.Success) {
                $.each(data.StockInformations, function () {
                    self.stockInfos.push(new StockInfoViewModel(this));
                });

                self.selectedStockInfo(new StockInfoViewModel(data.StockInformations[0]));
                self.statuses(data.Statuses);
                self.hasInfo(data.StockInformations.length > 0);

                if (self.selectedStockInfo().isOutOfStock()) {
                    $('#AddToCartButton').attr('disabled', 'disabled');
                }
            }

            ShowGlobalMessages(data);
        });
    };

    self.switchInfo = function () {
        ClearGlobalMessages();

        var productId = $("#product-id").val();
        var variantId = $('#VariantId') && $('#VariantId').length > 0 ? $('#VariantId').val() : "";
        var item = ko.utils.arrayFirst(this.stockInfos(), function (si) {
            if (si.productId() === productId && si.variantId() === variantId) {
                return si;
            }

            return null;
        });

        if (item == null) {
            self.selectedStockInfo(self.stockInfos()[0]);
        } else {
            self.selectedStockInfo(item);
        }

        if (self.selectedStockInfo().isOutOfStock()) {
            $('#AddToCartButton').attr('disabled', 'disabled');
        } else {
            $('#AddToCartButton').removeAttr('disabled');
        }
    };
}