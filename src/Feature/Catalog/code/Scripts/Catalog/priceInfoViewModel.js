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

var PriceInfoViewModel = function () {
    self = this;

    self.priceBefore = ko.observable();
    self.priceNow = ko.observable();
    self.savingsMessage = ko.observable();

    self.switchInfo = function (priceBefore, priceNow, isOnSale, savingsMessage) {
        self.priceNow(priceNow);
        self.priceBefore(priceBefore);
        self.savingsMessage(savingsMessage);

        if (!isOnSale) {
            $("#priceWithSavings").hide()
            $("#priceOnly").show();
        }
        else {
            $("#priceWithSavings").show()
            $("#priceOnly").hide();
        }
    };
}