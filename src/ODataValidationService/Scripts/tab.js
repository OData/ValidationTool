// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

$(document).ready(function () {
    // Set up a listener so that when anything with a class of 'tab'
    // is clicked, this function is run.
    // Switching between tabs can be disabled/enabled based on the status (of busy class or not)
    $('.tab').click(function () {
        // disable clicking while page is still busy validating
        if ($('#tabs_container').is('#tabs_container.busy'))
            return;
        //save that tab status
        if (validatorApp.currentJob) {
            var that = $('#tabs_container li.active a')[0];
            var content = $('#infobody').children();
            var status = $('#statusInfo').children();
            var createlink = $('#recorddiv').children();
            //that.saveStatus = true;

            that.dataPacket = {
                content: content,
                status: status,
                link:createlink,
                conformanceResults: JSON.stringify(conformanceResults),
                validatorApp: JSON.stringify(validatorApp)
            };
        }

        // Remove the 'active' class from the active tab.
        $('#tabs_container li.active').removeClass('active');

        // Add the 'active' class to the clicked tab.
        $(this).parent().addClass('active');

        // Remove the 'tab_contents_active' class from the visible tab contents.
        $('.tab_contents_container div.tab_contents_active').removeClass('tab_contents_active');

        // Add the 'tab_contents_active' class to the associated tab contents.
        $(this.rel).addClass('tab_contents_active');

        switch (this.rel) {
            case "#tab_1_contents":
            case "#tab_3_contents":
            case "#tab_4_contents":
            case "#tab_5_contents":
                $('#credentialContainer')[0].style.display = "block";
                break;
            case "#tab_2_contents":
                $('#credentialContainer')[0].style.display = "none";
                break;
            default: break;
        }

        // clear the result and resource areas
        resetAll();
        if (this.dataPacket) {
            $('#infobody').append( this.dataPacket.content );
            $('#infotop #statusInfo').append(this.dataPacket.status);
            $('#infotop #statusInfo').show();
            $('#recorddiv').append(this.dataPacket.link);
            conformanceResults = JSON.parse(this.dataPacket.conformanceResults);
            validatorApp = JSON.parse(this.dataPacket.validatorApp);

            return;
        }

        if (this.MasterJobId)// and there is no contents
        {
            this.reload();
        }
    });
});

function disableTabSwitch() {
    $('#tabs_container').addClass('busy');
    // disable all the input elements on active tab
    $("#tabs_container .tab_contents_active [id]").attr("disabled", true);
    disableCredential();
};

function enableTabSwitch() {
    $('#tabs_container').removeClass('busy');
    // enable all the input elements on active tab
    $("#tabs_container .tab_contents_active [id]").attr("disabled", false);
    // enable all rerun buttons on active tab
    displayHide_buttonsIninfobody(false);
    enableCredential();
};

function disableCredential() {
    $('#requiredCredential')[0].disabled = true;
    $('#username')[0].disabled = true;
    $('#password')[0].disabled = true;
}

function enableCredential() {
    $('#requiredCredential')[0].disabled = false;
    $('#username')[0].disabled = false;
    $('#password')[0].disabled = false;
}


