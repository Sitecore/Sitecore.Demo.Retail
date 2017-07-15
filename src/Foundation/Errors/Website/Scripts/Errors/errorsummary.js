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

var errorSummaryViewModel = null;

function initErrorSummary(sectionId) {
    errorSummaryViewModel = new ErrorSummaryViewModel(sectionId);
    ko.applyBindings(errorSummaryViewModel, document.getElementById(sectionId));
}

function ShowGlobalMessages(data) {
    if (data && data.Url) {
        window.location.href = "/" + data.Url;
    }
    if (errorSummaryViewModel && data && data.Errors && data.Errors.length > 0) {
        errorSummaryViewModel.AddToErrorList(data);
    }
}

function ClearGlobalMessages() {
    if (errorSummaryViewModel) {
        errorSummaryViewModel.ClearMessages();
    }
}