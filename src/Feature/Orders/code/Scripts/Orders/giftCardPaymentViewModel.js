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

function GiftCardPaymentViewModel(card) {
    var self = this;
    var populate = card != null;

    self.isAdded = ko.observable(false);
    self.balance = ko.observable(0);
    self.formattedBalance = ko.observable("");
    self.enableAddCard = ko.observable(false);

    self.giftCardNumber = populate ? ko.validatedObservable(card.PaymentMethodID).extend({ required: true }) : ko.validatedObservable("").extend({ required: true });
    self.giftCardNumber.subscribe(function (cn) {
        self.balance(0);
        self.formattedBalance("");
        self.enableAddCard(cn.length > 0 && self.giftCardAmount().length > 0 && self.isAdded() === false);
    }.bind(this));

    self.giftCardAmount = populate ? ko.validatedObservable(card.Amount).extend({ required: true, number: true }) : ko.validatedObservable(0.00).extend({ required: true, number: true });
    self.giftCardAmount.subscribe(function (ca) {
        self.enableAddCard(ca.length > 0 && self.giftCardNumber().length > 0 && self.isAdded() === false);
        self.revalueAmounts();
    }.bind(this));

    self.reload = function (data) {
        self.giftCardNumber(data.ExternalId);
        self.formattedBalance(data.FormattedBalance);
        self.balance(data.Balance);
        self.enableAddCard(false);
    }

    self.getBalance = function () {
        ClearGlobalMessages();
        $('#giftCardPayment_GetBalance').button('loading');
        var data = {};
        data.GiftCardId = self.giftCardNumber();
        AJAXPost('/api/storefront/catalog/checkgiftcardbalance', JSON.stringify(data), function (data, success, sender) {
            if (success && data.Success) {
                self.reload(data);
            }

            ShowGlobalMessages(data);
            $('#giftCardPayment_GetBalance').button('reset');
        }, this);
    };

    self.applyBalance = function () {
        ClearGlobalMessages();
        var balance = self.balance();
        if (balance <= 0) {
            return;
        }

        var orderTotal = checkoutDataViewModel.cart().totalAmount();
        if (balance > orderTotal) {
            self.giftCardAmount(orderTotal);
        }
        else if (balance <= orderTotal) {
            self.giftCardAmount(balance);
        }

        self.enableAddCard(true);
    };

    self.addCard = function () {
        ClearGlobalMessages();
        self.isAdded(true);
        self.enableAddCard(false);
        self.revalueAmounts();
    };

    self.removeCard = function () {
        ClearGlobalMessages();
        self.isAdded(false);
        self.giftCardNumber("");
        self.formattedBalance("");
        self.giftCardAmount(0.00);
        self.balance(0.00);
    };

    self.revalueAmounts = function () {
        if (!self.isAdded()) {
            return;
        }

        var ccIsAdded = checkoutDataViewModel.creditCardPayment().isAdded();
        if (!ccIsAdded) {
            return;
        }

        var ccAmount = parseFloat(checkoutDataViewModel.creditCardPayment().creditCardAmount());
        var total = parseFloat(checkoutDataViewModel.cart().totalAmount());
        var aTotal = parseFloat(parseFloat(self.giftCardAmount()) + ccAmount);

        if (aTotal > total) {
            var diff = aTotal - total;
            ccAmount = ccAmount - diff;
            checkoutDataViewModel.creditCardPayment().creditCardAmount((ccAmount).toFixed(2));
        }
    };
}