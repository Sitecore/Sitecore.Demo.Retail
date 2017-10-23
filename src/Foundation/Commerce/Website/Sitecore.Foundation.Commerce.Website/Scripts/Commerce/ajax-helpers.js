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
