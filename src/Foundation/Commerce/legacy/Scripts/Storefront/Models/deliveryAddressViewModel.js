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

function DeliveryAddressViewModel(address) {
    var self = this;

    var populate = address != null;

    self.externalId = populate ? ko.observable(address.ExternalId) : ko.observable();
    self.partyId = populate ? ko.observable(address.ExternalId) : ko.observable();
    self.name = populate ? ko.validatedObservable(address.Name).extend({ required: true }) : ko.validatedObservable().extend({ required: true });
    self.address1 = populate ? ko.validatedObservable(address.Address1).extend({ required: true }) : ko.validatedObservable().extend({ required: true });
    self.city = populate ? ko.validatedObservable(address.City).extend({ required: true }) : ko.validatedObservable().extend({ required: true });
    self.region = populate ? ko.validatedObservable(address.Region).extend({ required: true }) : ko.validatedObservable().extend({ required: true });
    self.zipPostalCode = populate ? ko.validatedObservable(address.ZipPostalCode).extend({ required: true }) : ko.validatedObservable().extend({ required: true });
    self.country = populate ? ko.validatedObservable(address.Country).extend({ required: true }) : ko.validatedObservable().extend({ required: true });
    self.isPrimary = populate ? ko.observable(address.IsPrimary) : ko.observable();
    self.fullAddress = populate ? ko.observable(address.FullAddress) : ko.observable();
    self.detailsUrl = populate ? ko.observable(address.DetailsUrl) : ko.observable();

    self.regions = ko.observableArray();
    self.country.subscribe(function (countryCode) {
        self.regions.removeAll();
        // self.getRegions(countryCode);
    });

    self.getRegions = function (countryCode) {
        AJAXPost("/api/storefront/checkout/getAvailableRegions", '{ "CountryCode": "' + countryCode + '"}', function (data, success, sender) {
            if (data.Regions != null) {
                $.each(data.Regions, function (code, name) {
                    self.regions.push(new CountryRegionViewModel(name, code));
                });
            }
        });
    }
}