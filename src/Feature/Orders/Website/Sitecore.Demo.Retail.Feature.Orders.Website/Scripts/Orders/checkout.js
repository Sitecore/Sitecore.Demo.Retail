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

// Global Vars
var checkoutDataViewModel = null;
var methodsViewModel = null;
var method = null;
var expirationDates = ko.observableArray();
var expirationYears = ko.observableArray();
var shippingMethodsArray = [];
var addingCountry = false;

function setupCheckoutPage() {
    $("#orderGetShippingMethods").click(function () {
        ClearGlobalMessages();
        if (checkoutDataViewModel && checkoutDataViewModel.shippingAddress() && checkoutDataViewModel.shippingAddress.errors().length === 0) {
            $("#orderGetShippingMethods").button('loading');
            var party = ko.toJS(checkoutDataViewModel.shippingAddress());
            var data = { "ShippingAddress": party, "ShippingPreferenceType": checkoutDataViewModel.selectedShippingOption(), "Lines": null };
            AJAXPost("/api/storefront/checkout/GetShippingMethods", JSON.stringify(data), function (data, success, sender) {
                if (data.Success && success) {
                    var methods = "";
                    checkoutDataViewModel.shippingMethods.removeAll();
                    $.each(data.ShippingMethods, function (i, v) {
                        checkoutDataViewModel.shippingMethods.push(new method(v.Description, v.ExternalId));
                    });
                }

                ShowGlobalMessages(data);
                $("#orderGetShippingMethods").button('reset');
            }, $(this));
        }
        else {
            checkoutDataViewModel.shippingAddress.errors.showAllMessages();
        }
    });

    $('.temp-click').on('click', changeClass);

    $("body").on("click", ".toBilling", function () { switchingCheckoutStep('billing'); });

    $("body").on("click", ".toShipping", function () { switchingCheckoutStep('shipping'); });

    $("body").on('click', '.lineGetShippingMethods', function () {
        ClearGlobalMessages();
        var lineId = $(this).attr('id').replace('lineGetShippingMethods-', '');
        var line = ko.utils.arrayFirst(checkoutDataViewModel.cart().cartLines(), function (l) {
            return l.externalCartLineId === lineId;
        });

        if (line && line.shippingAddress() && line.shippingAddress.errors().length === 0) {
            $("#lineGetShippingMethods-" + lineId).button('loading');
            var party = ko.toJS(line.shippingAddress());
            var lines = [{ "ExternalCartLineId": lineId, "ShippingPreferenceType": line.selectedShippingOption() }];
            var data = { "ShippingAddress": party, "ShippingPreferenceType": checkoutDataViewModel.selectedShippingOption(), "Lines": lines };
            AJAXPost("/api/storefront/checkout/GetShippingMethods", JSON.stringify(data), function (data, success, sender) {
                var lineId = sender.attr('id').replace('lineGetShippingMethods-', '');
                if (data.Success && success && checkoutDataViewModel != null) {
                    var match = ko.utils.arrayFirst(checkoutDataViewModel.cart().cartLines(), function (item) {
                        return item.externalCartLineId === lineId;
                    });

                    match.shippingMethods.removeAll();
                    $.each(data.LineShippingMethods[0].ShippingMethods, function (i, v) {
                        match.shippingMethods.push(new method(v.Description, v.ExternalId));
                    });
                }

                ShowGlobalMessages(data);
                $("#lineGetShippingMethods-" + lineId).button('reset');
            }, $(this));
        } else {
            line.shippingAddress.errors.showAllMessages();
        }
    });

    $('form').submit(function (e) {
        e.preventDefault();
        return false;
    });

    $("#submitOrder").click(function () {
        submitOrder();
    });
}

function changeClass(e) {
    e.preventDefault();
    var clickedElement = $(this);

    clickedElement.closest("ul").find(".active").removeClass("active");
    clickedElement.closest("li").addClass('active');
};

// ----- JSON CALLS ----- //
function GetAvailableRegions(countryCode) {
    var regionsArray = [];
    // Uncomment when the Regions are available
    //
    //AJAXPost("/api/storefront/checkout/getAvailableRegions", '{ "CountryCode": "' + countryCode + '"}', function (data, success, sender){     
    //    if (data.Regions != null) {
    //        $.each(data.UserAddresses, function (index, value) {         
    //            regionsArray.push(new Country(value, index));
    //        });
    //    }  
    //});
    return regionsArray;
}

function UpdateAvailableRegions(countryCode) {
    checkoutDataViewModel.regions(GetAvailableRegions(countryCode));
}

function getCheckoutData() {
    AJAXPost("/api/storefront/checkout/GetCheckoutData", null, function (data, success, sender) {
        if (success && data.Success) {
            checkoutDataViewModel = new CheckoutDataViewModel(data);
            ko.applyBindingsWithValidation(checkoutDataViewModel, document.getElementById("checkoutSection"));
            $('#orderShippingPreference').removeAttr('disabled');

        }

        ShowGlobalMessages(data);
    });
}

function getExpirationDates() {
    for (var i = 0; i < 12; i++) {
        var index = i + 1;
        expirationDates.push({ Name: index, Value: index });
    }
}

function getExpirationYears() {
    for (var i = 0; i < 10; i++) {
        var currentYear = new Date().getFullYear();
        expirationYears.push({ Year: currentYear + i, Value: currentYear + i });
    }
}

function initObservables() {
    method = function (description, id) {
        this.description = description;
        this.id = id;
    }
    MethodsViewModel = function () {
        var self = this;
        self.methods = ko.observableArray();
    }
    methodsViewModel = new MethodsViewModel();
}

var abde = null;

function initCheckoutData() {
    getExpirationDates();
    getExpirationYears();
    getCheckoutData();
}

// ----- SHIPPING ----- //
function InitDeliveryPage() {
    $(document).ready(function () {
        $('#btn-delivery-next').show();
        $('#btn-delivery-prev').show();
        $('#orderShippingPreference').attr('disabled', 'disabled');
        $("#ShipAllItemsInput-ExternalId").val(0);

        $("body").on('click', ".nav li.disabled a", function (e) {
            $(this).parent().removeClass("active");
            e.preventDefault();
            return false;
        });

        $("#deliveryMethodSet").val(false);

        $("#checkoutNavigation2").parent().addClass("disabled");
        $("#checkoutNavigation3").parent().addClass("disabled");

        switchingCheckoutStep("shipping");
        initObservables();
    });
};

function setShippingMethods() {
    ClearGlobalMessages();
    var parties = [];
    var shipping = [];
    var orderShippingPreference = checkoutDataViewModel.selectedShippingOption();
    $("#deliveryMethodSet").val(false);

    $("#ToBillingButton").button('loading');
    $("#BackToBillingButton").button('loading');

    if (orderShippingPreference === 1) {
        var partyId = checkoutDataViewModel.shippingAddress().externalId();
        parties.push({
            "Name": checkoutDataViewModel.shippingAddress().name(),
            "Address1": checkoutDataViewModel.shippingAddress().address1(),
            "Country": checkoutDataViewModel.shippingAddress().country(),
            "City": checkoutDataViewModel.shippingAddress().city(),
            "Region": checkoutDataViewModel.shippingAddress().region(),
            "ZipPostalCode": checkoutDataViewModel.shippingAddress().zipPostalCode(),
            "ExternalId": partyId,
            "PartyId": partyId
        });

        shipping.push({
            "ShippingMethodID": checkoutDataViewModel.shippingMethod().id,
            "ShippingMethodName": checkoutDataViewModel.shippingMethod().description,
            "ShippingPreferenceType": orderShippingPreference,
            "PartyId": partyId
        });
    }
    else if (orderShippingPreference === 4) {
        $.each(checkoutDataViewModel.cart().cartLines(), function () {
            var lineDeliveryPreference = this.selectedShippingOption();
            var lineId = this.externalCartLineId;

            if (lineDeliveryPreference === 1) {
                var partyId = this.shippingAddress().externalId();
                parties.push({
                    "Name": this.shippingAddress().name(),
                    "Address1": this.shippingAddress().address1(),
                    "Country": this.shippingAddress().country(),
                    "City": this.shippingAddress().city(),
                    "Region": this.shippingAddress().region(),
                    "ZipPostalCode": this.shippingAddress().zipPostalCode(),
                    "ExternalId": partyId,
                    "PartyId": partyId
                });

                shipping.push({
                    "ShippingMethodID": this.shippingMethod().id,
                    "ShippingMethodName": this.shippingMethod().description,
                    "ShippingPreferenceType": lineDeliveryPreference,
                    "PartyId": partyId,
                    "LineIDs": [lineId]
                });
            }

            if (lineDeliveryPreference === 3) {
                shipping.push({
                    "ShippingMethodID": checkoutDataViewModel.emailDeliveryMethod().ExternalId,
                    "ShippingMethodName": checkoutDataViewModel.emailDeliveryMethod().Description,
                    "ShippingPreferenceType": lineDeliveryPreference,
                    "ElectronicDeliveryEmail": this.shippingEmail(),
                    "ElectronicDeliveryEmailContent": this.shippingEmailContent(),
                    "LineIDs": [lineId]
                });
            }
        });
    }
    else if (orderShippingPreference === 3) {
        shipping.push({
            "ShippingMethodID": checkoutDataViewModel.emailDeliveryMethod().ExternalId,
            "ShippingMethodName": checkoutDataViewModel.emailDeliveryMethod().Description,
            "ShippingPreferenceType": orderShippingPreference,
            "ElectronicDeliveryEmail": checkoutDataViewModel.shippingEmail(),
            "ElectronicDeliveryEmailContent": checkoutDataViewModel.shippingEmailContent()
        });
    }

    var data = '{"OrderShippingPreferenceType": "' + orderShippingPreference + '", "ShippingMethods":' + JSON.stringify(shipping) + ', "ShippingAddresses":' + JSON.stringify(parties) + '}';
    AJAXPost("/api/storefront/checkout/SetShippingMethods", data, setShippingMethodsResponse, $(this));
    return false;
}

function setShippingMethodsResponse(data, success, sender) {
    if (success && data.Success) {
        if (checkoutDataViewModel != null) {
            checkoutDataViewModel.cart().setSummary(data);
        }

        updatePaymentAllAmount();
        if (checkoutDataViewModel.paymentClientToken() != null) {
            var clientToken = checkoutDataViewModel.paymentClientToken();
            if (clientToken.length > 0) {
                braintree.setup(clientToken, 'dropin', {
                    container: 'dropin-container',
                    paymentMethodNonceReceived: function (event, nonce) {
                        if (nonce.length > 0) {
                            checkoutDataViewModel.cardPaymentResultAccessCode = nonce;
                            checkoutDataViewModel.cardPaymentAcceptCardPrefix = "paypal";
                        }
                    }
                });
            }
        }
        else if (checkoutDataViewModel.cardPaymentAcceptPageUrl().length == 0) {
            getCardPaymentAcceptUrl();
        }
        $("#deliveryMethodSet").val(true);
        $("#billingStep").show();
        $("#reviewStep").hide();
        $("#shippingStep").hide();
        shippingButtons(false);
        billingButtons(true);
        confirmButtons(false);
        $("#checkoutNavigation1").parent().removeClass("active");
        $("#checkoutNavigation2").parent().addClass("active");
        $("#checkoutNavigation3").parent().removeClass("active");
        $("#checkoutNavigation2").parent().removeClass("disabled");
        $("#checkoutNavigation3").parent().removeClass("disabled");
    }

    ShowGlobalMessages(data);
    $("#ToBillingButton").button('reset');
    $("#BackToBillingButton").button('reset');
}

// ----- BILLING ----- //
function initBillingPage() {
    $(document).ready(function () {
        $('.accordion-toggle').on('click', function (event) {
            event.preventDefault();

            // create accordion variables
            var accordion = $(this);
            var accordionContent = accordion.closest('.accordion-container').find('.accordion-content');
            var accordionToggleIcon = $(this).children('.toggle-icon');

            // toggle accordion link open class
            accordion.toggleClass("open");

            // toggle accordion content
            accordionContent.slideToggle(250);

            // change plus/minus icon
            if (accordion.hasClass("open")) {
                accordionToggleIcon.html("<span class='glyphicon glyphicon-minus-sign'></span>");
                if (this.id == "ccpayment") {
                    checkoutDataViewModel.creditCardPayment().isAdded(true);
                    if (checkoutDataViewModel.paymentClientToken() != null) {
                        checkoutDataViewModel.creditCardEnable(true);
                        checkoutDataViewModel.billingAddressEnable(true);
                    }
                }
            } else {
                accordionToggleIcon.html("<span class='glyphicon glyphicon-plus-sign'></span>");
            }
        });
    });
}

function getCardPaymentAcceptUrl() {
    if (checkoutDataViewModel && checkoutDataViewModel.payFederatedPayment && checkoutDataViewModel.shippingAddress()) {
        AJAXPost("/api/storefront/checkout/GetCardPaymentAcceptUrl", null, function (data, success, sender) {
            if (data.Success && success) {
                checkoutDataViewModel.cardPaymentAcceptPageUrl(data.ServiceUrl);
                checkoutDataViewModel.messageOrigin = data.MessageOrigin;

                removeCardPaymentAcceptListener();
                addCardPaymentAcceptListener();
            }
            ShowGlobalMessages(data);
        }, $(this));
    }
}

function updatePaymentAllAmount() {
    var ccIsAdded = checkoutDataViewModel.creditCardPayment().isAdded();
    var gcIsAdded = checkoutDataViewModel.giftCardPayment().isAdded();
    if (!ccIsAdded && !gcIsAdded) {
        return;
    }

    var total = parseFloat(checkoutDataViewModel.cart().totalAmount());
    var gcAmount = parseFloat(checkoutDataViewModel.giftCardPayment().giftCardAmount());
    var ccAmount = parseFloat(checkoutDataViewModel.creditCardPayment().creditCardAmount());
    var aTotal = parseFloat(gcAmount + ccAmount);

    if (aTotal === total) {
        return;
    }

    var count = 0
    if (gcIsAdded) {
        ++count;
    }
    if (ccIsAdded) {
        ++count;
    }

    if (aTotal > total) {
        var diff = (aTotal - total) / count;
        gcAmount = gcIsAdded ? gcAmount - diff : 0;
        ccAmount = ccIsAdded ? ccAmount - diff : 0;
    } else if (aTotal < total) {
        var diff = (total - aTotal) / count;
        gcAmount = gcIsAdded ? gcAmount + diff : 0;
        ccAmount = ccIsAdded ? ccAmount + diff : 0;
    }

    checkoutDataViewModel.giftCardPayment().giftCardAmount((gcAmount).toFixed(2));
    checkoutDataViewModel.creditCardPayment().creditCardAmount((ccAmount).toFixed(2));
}

function submitCardPaymentAcceptPayment() {
    if (checkoutDataViewModel && checkoutDataViewModel.payFederatedPayment && checkoutDataViewModel.creditCardPayment().isAdded() && !checkoutDataViewModel.cardPaymentResultAccessCode) {
        if (checkoutDataViewModel.messageOrigin) {
             //Send a message to the card page to trigger submit
            var cardPaymentAcceptIframe = document.getElementById("cardPaymentAcceptFrame");
            var cardPaymentAcceptMessage = {
                type: "msax-cc-submit",
                value: "true"
            };
            cardPaymentAcceptIframe.contentWindow.postMessage(JSON.stringify(cardPaymentAcceptMessage), checkoutDataViewModel.messageOrigin);
        }
    }
    else {
        setPaymentMethods();
    }
};

function addCardPaymentAcceptListener() {
    window.addEventListener("message", this.cardPaymentAcceptMessageHandler, false);
}


function removeCardPaymentAcceptListener() {
    window.removeEventListener("message", this.cardPaymentAcceptMessageHandler, false);
}

function cardPaymentAcceptMessageHandler(eventInfo) {
    // Validate origin
    if (!eventInfo || !(checkoutDataViewModel.messageOrigin.indexOf(eventInfo.origin) === 0)) {
        return;
    }

    // Parse messages
    var message = eventInfo.data;
    if (typeof (message) === "string" && message.length > 0) {

        // Handle various messages from the card payment accept page
        var messageObject = JSON.parse(message);
        switch (messageObject.type) {
            case checkoutDataViewModel.CARDPAYMENTACCEPTPAGEHEIGHT:
                var cardPaymentAcceptIframe = $("cardPaymentAcceptFrame");
                cardPaymentAcceptIframe.height = messageObject.value;
                break;
            case checkoutDataViewModel.CARDPAYMENTACCEPTCARDPREFIX:
                checkoutDataViewModel.cardPaymentAcceptCardPrefix = messageObject.value;
                break;
            case checkoutDataViewModel.CARDPAYMENTACCEPTPAGEERROR:
                // Handle retrieve card payment accept result failure.
                var paymentErrors = messageObject.value;
                var errors = [];
                for (var i = 0; i < paymentErrors.length; i++) {
                    errors.push(!paymentErrors[i].Message ? paymentErrors[i].Code.toString() : paymentErrors[i].Message);
                }
                var data = { "Errors": errors };
                ShowGlobalMessages(data);
                break;
            case checkoutDataViewModel.CARDPAYMENTACCEPTPAGERESULT:
                checkoutDataViewModel.cardPaymentResultAccessCode = messageObject.value;
                setPaymentMethods();
                break;
            default:
                // Ignore all other messages.
        }
    }
}

function setPaymentMethods() {
    var data = "{";

    if (checkoutDataViewModel.creditCardPayment().isAdded()) {
        var cc = checkoutDataViewModel.creditCardPayment();
        if (checkoutDataViewModel && checkoutDataViewModel.payFederatedPayment) {
            var creditCard = {
                "CardToken": checkoutDataViewModel.cardPaymentResultAccessCode,
                "Amount": cc.creditCardAmount(),
                "CardPaymentAcceptCardPrefix": checkoutDataViewModel.cardPaymentAcceptCardPrefix
            };

            if (data.length > 1) {
                data += ",";
            }

            data += '"FederatedPayment":' + JSON.stringify(creditCard);
            if (checkoutDataViewModel.cardPaymentAcceptCardPrefix === "paypal") {
                var ba = checkoutDataViewModel.billingAddress();
                var billingAddress =
                {
                    "Name": ba.name(),
                    "Address1": ba.address1(),
                    "Country": ba.country(),
                    "City": ba.city(),
                    "Region": ba.region(),
                    "ZipPostalCode": ba.zipPostalCode(),
                    "ExternalId": ba.externalId(),
                    "PartyId": ba.externalId()
                };

                if (data.length > 1) {
                    data += ",";
                }

                data += '"BillingAddress":' + JSON.stringify(billingAddress);
            }
        } else {
            var creditCard = {
                "CreditCardNumber": cc.creditCardNumber(),
                "PaymentMethodID": cc.paymentMethodID(),
                "ValidationCode": cc.validationCode(),
                "ExpirationMonth": cc.expirationMonth(),
                "ExpirationYear": cc.expirationYear(),
                "CustomerNameOnPayment": cc.customerNameOnPayment(),
                "Amount": cc.creditCardAmount(),
                "PartyId": $('#billingAddress-ExternalId').val()
            };

            var ba = checkoutDataViewModel.billingAddress();
            var billingAddress =
            {
                "Name": ba.name(),
                "Address1": ba.address1(),
                "Country": ba.country(),
                "City": ba.city(),
                "Region": ba.region(),
                "ZipPostalCode": ba.zipPostalCode(),
                "ExternalId": ba.externalId(),
                "PartyId": ba.externalId()
            };

            if (data.length > 1) {
                data += ",";
            }

            data += '"CreditCardPayment":' + JSON.stringify(creditCard) + ',"BillingAddress":' + JSON.stringify(billingAddress);
        }
    }

    if (checkoutDataViewModel.giftCardPayment().isAdded()) {
        var giftCard = {
            "PaymentMethodID": checkoutDataViewModel.giftCardPayment().giftCardNumber(),
            "Amount": checkoutDataViewModel.giftCardPayment().giftCardAmount()
        };

        if (data.length > 1) {
            data += ",";
        }

        data += '"GiftCardPayment":' + JSON.stringify(giftCard);
    }

    data += "}";

    $("#ToConfirmButton").button('loading');

    AJAXPost("/api/storefront/checkout/SetPaymentMethods", data, setPaymentMethodsResponse, $(this));
}

function setPaymentMethodsResponse(data, success, sender) {
    if (data.Success && success) {
        if (checkoutDataViewModel != null) {
            checkoutDataViewModel.cart().setSummary(data);
            if (checkoutDataViewModel && checkoutDataViewModel.payFederatedPayment && checkoutDataViewModel.cardPaymentAcceptCardPrefix != "paypal" && checkoutDataViewModel.creditCardPayment().isAdded()) {
                var cc = checkoutDataViewModel.creditCardPayment();
                cc.creditCardNumberMasked(data.Payment[0].CreditCardNumber);
                cc.expirationMonth(data.Payment[0].ExpirationMonth);
                cc.expirationYear(data.Payment[0].ExpirationYear);
                cc.customerNameOnPayment(data.Payment[0].CustomerNameOnPayment);

                var ba = checkoutDataViewModel.billingAddress();
                var address = data.Parties[0];
                addingCountry = checkoutDataViewModel.countries[address.Country] == undefined;
                checkoutDataViewModel.addCountry(address.Country, address.Country);
                ba.address1(address.Address1);
                ba.country(address.Country);
                ba.city(address.City);
                ba.region(address.Region);
                ba.zipPostalCode(address.ZipPostalCode);
            }
        }

        switchingCheckoutStep('confirm');
    }

    ShowGlobalMessages(data);
    $("#ToConfirmButton").button('reset');
    $("#PlaceOrderButton").button('reset');
}

// ----- CONFIRM & SUBMIT ----- //
function submitOrder() {
    ClearGlobalMessages();

    var data = "{";
    data += '"userEmail": "' + checkoutDataViewModel.billingEmail() + '"';

    if (checkoutDataViewModel.creditCardPayment().isAdded()) {
        var cc = checkoutDataViewModel.creditCardPayment();
        if (checkoutDataViewModel && checkoutDataViewModel.payFederatedPayment) {
            var creditCard = {
                "CardToken": checkoutDataViewModel.cardPaymentResultAccessCode,
                "Amount": cc.creditCardAmount(),
                "CardPaymentAcceptCardPrefix": checkoutDataViewModel.cardPaymentAcceptCardPrefix
            };

            if (data.length > 1) {
                data += ",";
            }

            data += '"FederatedPayment":' + JSON.stringify(creditCard);
        } else {
            var creditCard = {
                "CreditCardNumber": cc.creditCardNumber(),
                "PaymentMethodID": cc.paymentMethodID(),
                "ValidationCode": cc.validationCode(),
                "ExpirationMonth": cc.expirationMonth(),
                "ExpirationYear": cc.expirationYear(),
                "CustomerNameOnPayment": cc.customerNameOnPayment(),
                "Amount": cc.creditCardAmount(),
                "PartyId": $('#billingAddress-ExternalId').val()
            };

            var ba = checkoutDataViewModel.billingAddress();
            var billingAddress =
            {
                "Name": ba.name(),
                "Address1": ba.address1(),
                "Country": ba.country(),
                "City": ba.city(),
                "Region": ba.region(),
                "ZipPostalCode": ba.zipPostalCode(),
                "ExternalId": ba.externalId(),
                "PartyId": ba.externalId()
            };

            data += ',"CreditCardPayment":' + JSON.stringify(creditCard) + ',"BillingAddress":' + JSON.stringify(billingAddress);
        }
    }

    if (checkoutDataViewModel.giftCardPayment().isAdded()) {
        var giftCard = {
            "PaymentMethodID": checkoutDataViewModel.giftCardPayment().giftCardNumber(),
            "Amount": checkoutDataViewModel.giftCardPayment().giftCardAmount()
        };

        data += ',"GiftCardPayment":' + JSON.stringify(giftCard);
    }

    data += "}";

    $("#PlaceOrderButton").button('loading');

    AJAXPost("/api/storefront/checkout/SubmitOrder", data, submitOrderResponse, $(this));
}

function submitOrderResponse(data, success, sender) {
    if (data.Success && success) {
        window.location.href = data.ConfirmUrl;
    }

    ShowGlobalMessages(data);
    $("#PlaceOrderButton").button('reset');
}

// ----- LOCALIZED MESSAGE DICTIONARY ----- //
var messageDictionary = new Array();
function AddMessage(key, value) {
    messageDictionary[key] = value;
}

function GetMessage(key) {
    return messageDictionary[key];
}

// ----- CHECKOUT GENERAL ----- //
function switchingCheckoutStep(step) {
    ClearGlobalMessages();

    if (step === "billing") {
        if ($("#deliveryMethodSet").val() === 'false') {
            setShippingMethods();
        } else {
            $("#billingStep").show();
            $("#reviewStep").hide();
            $("#shippingStep").hide();
            shippingButtons(false);
            billingButtons(true);
            confirmButtons(false);
            $("#checkoutNavigation1").parent().removeClass("active");
            $("#checkoutNavigation2").parent().addClass("active");
            $("#checkoutNavigation3").parent().removeClass("active");
            return;
        }
    }

    if (step === "shipping") {
        if (addingCountry) {
            checkoutDataViewModel.countries.pop();
        }
        $("#deliveryMethodSet").val(false);
        $("#billingStep").hide();
        $("#reviewStep").hide();
        $("#shippingStep").show();
        shippingButtons(true);
        billingButtons(false);
        confirmButtons(false);
        $("#checkoutNavigation1").parent().addClass("active");
        $("#checkoutNavigation2").parent().removeClass("active");
        $("#checkoutNavigation3").parent().removeClass("active");
        $("#checkoutNavigation2").parent().addClass("disabled");
        $("#checkoutNavigation3").parent().addClass("disabled");
    }

    if (step === "confirm") {
        if ($("#deliveryMethodSet").val() === 'true') {
            $("#billingStep").hide();
            $("#reviewStep").show();
            $("#shippingStep").hide();
            shippingButtons(false);
            billingButtons(false);
            confirmButtons(true);
            $("#checkoutNavigation1").parent().removeClass("active");
            $("#checkoutNavigation2").parent().removeClass("active");
            $("#checkoutNavigation3").parent().addClass("active");
        } else {
            $("#checkoutNavigation2").parent().addClass("disabled");
            $("#checkoutNavigation3").parent().addClass("disabled");
            return;
        }
    }

    if (step === "placeOrder") {
        $("#billingStep").hide();
        $("#reviewStep").hide();
        $("#shippingStep").hide();
        shippingButtons(false);
        billingButtons(false);
        confirmButtons(false);
        $("#checkoutNavigation1").parent().removeClass("active");
        $("#checkoutNavigation2").parent().removeClass("active");
        $("#checkoutNavigation3").parent().removeClass("active");
    }
}

function setupStepIndicator() {
    $(document).ready(function () {
        $("#checkoutNavigation1").click(function (e) {
            switchingCheckoutStep("shipping");
            $("#checkoutNavigation1").parent().addClass("active");
            $("#checkoutNavigation2").parent().removeClass("active");
            $("#checkoutNavigation3").parent().removeClass("active");
        });
        $("#checkoutNavigation2").click(function (e) {
            if (!$("#ToBillingButton").prop("disabled")) {
                switchingCheckoutStep("billing");
                $("#checkoutNavigation2").parent().addClass("active");
                $("#checkoutNavigation1").parent().removeClass("active");
                $("#checkoutNavigation3").parent().removeClass("active");
            } else {
                $("#checkoutNavigation1").parent().addClass("active");
                $("#checkoutNavigation2").parent().removeClass("active");
                $("#checkoutNavigation3").parent().removeClass("active");
            }
        });
        $("#checkoutNavigation3").click(function (e) {
            if (!$("#ToBillingButton").prop("disabled")) {
                switchingCheckoutStep("confirm");
                $("#checkoutNavigation3").parent().addClass("active");
                $("#checkoutNavigation2").parent().removeClass("active");
                $("#checkoutNavigation1").parent().removeClass("active");
            } else {
                $("#checkoutNavigation1").parent().addClass("active");
                $("#checkoutNavigation2").parent().removeClass("active");
                $("#checkoutNavigation3").parent().removeClass("active");
            }
        });
    });
}

function shippingButtons(show) {
    if (!show) {
        $('#btn-delivery-next').hide();
        $('#btn-delivery-prev').hide();
    }
    else {
        $('#btn-delivery-next').show();
        $('#btn-delivery-prev').show();
    }
}

function billingButtons(show) {
    if (!show) {
        $('#btn-billing-next').hide();
        $('#btn-billing-prev').hide();
    }
    else {
        $('#btn-billing-next').show();
        $('#btn-billing-prev').show();
    }
}

function confirmButtons(show) {
    if (!show) {
        $('#btn-confirm-next').hide();
        $('#btn-confirm-prev').hide();
    }
    else {
        $('#btn-confirm-next').show();
        $('#btn-confirm-prev').show();
    }
}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function formatCurrency(x, precision, seperator, isoCurrencySymbol, groupSeperator) {
    var options = {
        precision: precision || 2,
        seperator: seperator || ',',
        groupSeperator: groupSeperator || " "
    }

    var currencyValue = (x.__ko_proto__ === ko.dependentObservable || x.__ko_proto__ === ko.observable) ? x() : x;

    var formatted = parseFloat(currencyValue, 10).toFixed(options.precision);

    var regex = new RegExp('^(\\d+)[^\\d](\\d{' + options.precision + '})$');
    formatted = formatted.replace(regex, '$1' + options.seperator + '$2');
    formatted = formatted.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1" + options.groupSeperator)

    if (isoCurrencySymbol && isoCurrencySymbol.length > 0) {
        return formatted + " " + isoCurrencySymbol;
    }
    else {
        return formatted;
    }
}
