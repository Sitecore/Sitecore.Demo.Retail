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

function AddressListViewModel(data) {
    var self = this;

    self.addresses = ko.observableArray();
    $.each(data.Addresses, function () {
        self.addresses.push(new AddressViewModel(this));
    });

    self.isNotEmpty = ko.observable(self.addresses().length !== 0);
    self.isEmpty = ko.observable(self.addresses().length === 0);
    self.enableDelete = ko.observable(false);
    self.enableSave = ko.observable(true);

    self.countries = ko.observableArray();
    if (data.Countries != null) {
        $.each(data.Countries, function (code, name) {
            self.countries.push(new CountryRegionViewModel(name, code));
        });
    }

    self.address = ko.validatedObservable(new AddressViewModel());
    self.selectedAddress = ko.observable();
    self.selectedAddress.subscribe(function (externalId) {
        var address = ko.utils.arrayFirst(this.addresses(), function (a) {
            if (a.externalId() === externalId) {
                return a;
            }

            return null;
        });

        ClearGlobalMessages();
        if (address != null) {
            self.address(address);
            self.enableDelete(true);
        } else {
            self.address(new AddressViewModel());
            self.enableDelete(false);
        }
    }.bind(this));

    var addressId = getUrlParameter(document.URL, 'id');
    if (addressId != null) {
        self.selectedAddress(addressId);
    }

    self.reload = function (data) {
        if (data.Addresses != null) {
            self.addresses.removeAll();
            $.each(data.Addresses, function () {
                self.addresses.push(new AddressViewModel(this));
            });
        }

        if (data.Countries != null && data.Countries.length > 0) {
            self.countries.removeAll();
            $.each(data.Countries, function (code, name) {
                self.countries.push(new CountryRegionViewModel(name, code));
            });
        }

        self.selectedAddress("");
        self.address(new AddressViewModel());
        self.isNotEmpty(self.addresses().length !== 0);
        self.isEmpty(self.addresses().length === 0);
        self.enableDelete(false);
        self.enableSave(true);
        $("#cancelChanges").removeAttr("disabled");
    }

    self.saveAddress = function () {
        ClearGlobalMessages();
        if (self.address.errors().length === 0) {
            $('#saveAddress').button('loading');
            $("#cancelChanges").attr("disabled", "disabled");
            self.enableDelete(false);
            var address = ko.toJSON(self.address);

            AJAXPost('/api/storefront/customers/addressmodify', address, function (data, success, sender) {
                if (success && data.Success) {
                    self.reload(data);
                }

                ShowGlobalMessages(data);
                $('#saveAddress').button('reset');
            }, this);
        } else {
            $('#addressBook-Name').focus();
            self.address.errors.showAllMessages();
        }
    }

    self.deleteAddress = function () {
        ClearGlobalMessages();
        $("#deleteAddress").prop("disabled", true);
        $("#deleteAddress").html($("#deleteAddress").attr("data-loading-text"));
        self.enableSave(false);
        $("#cancelChanges").attr("disabled", "disabled");

        AJAXPost('/api/storefront/customers/addressdelete', '{ "ExternalId": "' + self.address().externalId() + '"}', function (data, success, sender) {
            if (success && data.Success) {
                self.reload(data);
            }

            ShowGlobalMessages(data);
            $("#deleteAddress").prop("disabled", true);
            $("#deleteAddress").html($("#deleteAddress").attr("data-text"));
        }, this);
    }

    self.showLoader = ko.observable(true);
}

function getUrlParameter(url, param) {
    url = url.split('?');
    if (url.length === 1) {
        return null;
    }

    var pattern = new RegExp(param + '=(.*?)(;|&|$)', 'gi');
    return url[1].split(pattern)[1];
}