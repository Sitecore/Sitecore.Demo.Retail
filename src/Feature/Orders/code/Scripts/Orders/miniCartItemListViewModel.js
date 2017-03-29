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

function MiniCartItemListViewModel(data) {
    if (data != null) {
        var self = this;

        self.miniCartItems = ko.observableArray();

        $(data.Lines).each(function () {
            self.miniCartItems.push(new MiniCartItemViewModel(this.Image, this.title, this.Quantity, this.LinePrice, this.ProductUrl, this.ExternalCartLineId));
        });

        self.lineitemcount = ko.observable(data.Lines.length);
        self.total = ko.observable(data.Subtotal);

        self.reload = function (data) {
            self.miniCartItems.removeAll();

            $(data.Lines).each(function () {
                self.miniCartItems.push(new MiniCartItemViewModel(this.Image, this.Title, this.Quantity, this.LinePrice, this.ProductUrl, this.ExternalCartLineId));
            });
            self.lineitemcount(data.Lines.length);
            self.total(data.Subtotal);
        }
    }
}