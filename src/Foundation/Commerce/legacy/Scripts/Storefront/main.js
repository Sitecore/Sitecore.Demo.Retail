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

// Localized message dictionary
// -
var messageDictionary = new Array();

function AddMessage(key, value) {
    messageDictionary[key] = value;
}

function GetMessage(key) {
    return messageDictionary[key];
}

// AJAX Extensions
// -
AddAntiForgeryToken = function (data) {
    if (data == null) {
        data = {};
    }

    data.__RequestVerificationToken = $('#_CRSFform input[name=__RequestVerificationToken]').val();
    return data;
};

function AJAXGet(url, data, responseFunction, sender) {
    AJAXCall("GET", url, data, responseFunction, sender);
}

function AJAXPost(url, data, responseFunction, sender) {
    AJAXCall("POST", url, data, responseFunction, sender);
}

function AJAXCall(callType, url, data, responseFunction, sender) {
    var token = $('#_CRSFform input[name=__RequestVerificationToken]').val();
    $.ajax({
        type: callType,
        url: url,
        cache: false,
        headers: { "__RequestVerificationToken": token },
        contentType: "application/json; charset=utf-8",
        data: data,
        success: function (data) {
            if (responseFunction != null) {
                responseFunction(data, true, sender);
            }
        },
        error: function (data) {
            if (responseFunction != null) {
                responseFunction(data, false, sender);
            }
        }
    });
}

var toString = Object.prototype.toString;
isString = function (obj) {
    return toString.call(obj) == '[object String]';
}

var queryStringParamerterSort = "s";
var queryStringParamerterSortDirection = "sd";
var queryStringParamerterSortDirectionAsc = "asc";
var queryStringParamerterSortDirectionAscShort = "+";
var queryStringParamerterSortDirectionDesc = "desc";
var queryStringParamerterPage = "pg";
var queryStringParamerterPageSize = "ps";
var queryStringParameterSiteContentPage = "scpg";
var queryStringParameterSiteContentPageSize = "scps";

$(document).ready(function () {
    // This change stops the shopping chart from closing of a user clicks on it.
    $(document).on('click', 'div.dropdown-menu', function (e) {
        e.stopPropagation();
    });
});

function changeClass(e) {
    e.preventDefault();
    var clickedElement = $(this);

    clickedElement.closest("ul").find(".active").removeClass("active");
    clickedElement.closest("li").addClass('active');
};

function states(sender, event) {
    var $btn = $('#' + sender).button(event);
}

function getUrlParameter(url, param) {
    url = url.split('?');
    if (url.length === 1) {
        return null;
    }

    var pattern = new RegExp(param + '=(.*?)(;|&|$)', 'gi');
    return url[1].split(pattern)[1];
}

var mustEqual = function (val, other) {
    return val === other;
};

function printPage() {
    var url = window.location.href;
    var hasParams = url.split('?').length > 1;
    var location = hasParams ? url + "&p=1" : url + "?p=1";
    var w = window.open(location);

    w.onload = function () {
        $(document).ajaxStart().ajaxStop(w.print());
    };
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


