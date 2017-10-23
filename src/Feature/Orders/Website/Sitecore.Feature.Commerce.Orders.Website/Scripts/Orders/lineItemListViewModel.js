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

function LineItemListViewModel(data) {
    var self = this;

    self.cartLines = ko.observableArray();

    $(data.Lines).each(function () {
        self.cartLines.push(new LineItemDataViewModel(this));
    });

    self.adjustments = ko.observableArray();

    $(data.Adjustments).each(function () {
        self.adjustments.push(new AdjustmentViewModel(this));
    });

    self.promoCodes = ko.observableArray();

    $(data.PromoCodes).each(function () {
        self.promoCodes.push(this);
    });

    self.subTotal = ko.observable(data.Subtotal);
    self.taxTotal = ko.observable(data.TaxTotal);
    self.total = ko.observable(data.Total);
    self.totalAmount = ko.observable(data.TotalAmount);
    self.discount = ko.observable(data.Discount);
    self.discountAmount = ko.observable(data.DiscountAmount);
    self.totalDiscount = ko.observable(data.TotalDiscount);
    self.shippingTotal = ko.observable(data.ShippingTotal);
    self.hasShipping = ko.observable(data.HasShipping);
    self.hasTaxes = ko.observable(data.HasTaxes);

    self.promoCode = ko.observable("");

    self.setAdjustments = function (data) {
        self.adjustments.removeAll();

        $(data.Adjustments).each(function () {
            self.adjustments.push(new AdjustmentViewModel(this));
        });
    }

    self.setSummary = function (data) {
        self.subTotal(data.Subtotal);
        self.taxTotal(data.TaxTotal);
        self.total(data.Total);
        self.totalAmount(data.TotalAmount);
        self.discount(data.Discount);
        self.discountAmount(data.DiscountAmount);
        self.totalDiscount(data.TotalDiscount);
        self.shippingTotal(data.ShippingTotal);
        self.hasShipping(data.HasShipping);
        self.hasTaxes(data.HasTaxes);
    }

    self.setPromoCodes = function (data) {
        self.promoCodes.removeAll();

        $(data.PromoCodes).each(function () {
            self.promoCodes.push(this);
        });
    }

    self.reload = function (data) {
        self.cartLines.removeAll();

        $(data.Lines).each(function () {
            self.cartLines.push(new LineItemDataViewModel(this));
        });

        self.setSummary(data);
        self.setAdjustments(data);
        self.setPromoCodes(data);

        manageCartActions();
    }

    self.hasPromoCode = ko.computed(function () {
        return self.promoCode();
    });
}