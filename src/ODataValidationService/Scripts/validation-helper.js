// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

var validatorApp = {
    'results': { 'errors': 0, 'warnings': 0, 'successes': 0, 'recommendations': 0, 'aborteds': 0 },
    'currentJob': null,
    'jobsInQ': [],
    'currJobDetail': null,
    'currJobNote': null,
    'panes': 0,
    'LoadResultsTimeoutObj': null
};

var validatorStat = {
    'totalRequestsSent': 0
};
var dataforRerun = {
    "index" : 0,
    "JobDetails" : [ [], [], [] ],
    "JobNotes": [[], [], []],
    "Jobs": [[], [], []],
    "Results" : [ [], [], [] ]
};

var validatorConf = {
    'perRequestDelay': 5000,
    'maxRequests': 24,
    'SpecificationUri': "http://download.microsoft.com/download/9/5/E/95EF66AF-9026-4BB0-A41D-A4F81802D92C/[MS-ODATA].pdf",
    'ODataV3SpecificationUriForJson': "http://docs.oasis-open.org/odata/odata-json-format/v4.0/cs01/odata-json-format-v4.0-cs01.html",
    //http://docs.oasis-open.org/odata/odata-json-format/v4.0/cs01/odata-json-format-v4.0-cs01.html
    'V34JsonURLFragments': {"" : "", "1": "_Toc356575525","2": "_Toc356575529","3": "_Requesting_the_JSON","4": "_Toc356575537","5": "_Representing_the_Service","6": "_Entity","7": "_Structural_Property","8": "_Navigation_Property","9": "_Stream_Property","10": "_Media_Entity","11": "_Toc356575633","12": "_Collection_of_Entities","13": "_Entity_Reference","14": "_Delta_Response","15": "_Bound_Function","16": "_Bound_Action","17": "_Toc356575643","18": "_Instance_Annotations","19": "_Representing_Errors_in","20": "_Payload_Ordering_Constraints","21": "_Toc356910190","22": "_Toc356910191","3.1.1": "_odata.metadata=minimal","3.1.2": "_odata.metadata=full","3.1.3": "_odata.metadata=none","4.5.1": "_Annotation_odata.context","4.5.2": "_Toc356575544","4.5.3": "_Annotation_odata.type","4.5.4": "_Annotation_odata.count","4.5.5": "_Annotation_odata.nextLink","4.5.6": "_Annotation_odata.deltaLink","4.5.7": "_Annotation_odata.id","4.5.8": "_Annotation_odata.editLink_and","4.5.9": "_Toc358116355","4.5.10": "_Annotation_odata.navigationLink_and","4.5.11": "_Annotation_odata.media*","1.1": "_Toc357527697","1.2": "_Ref7502892","1.3": "_Toc356909526","3.1": "_Controlling_the_Amount","3.2": "_Controlling_the_Representation","4.1": "_Header_Content-Type","4.2": "_Toc357527710","4.3": "_Relative_URLs","4.4": "_Payload_Ordering_Constraints_1","4.5": "_Control_Information","7.1": "_Primitive_Value","7.2": "_Complex_Value","7.3": "_Collection_of_Primitive","7.4": "_Collection_of_Complex","8.1": "_Representing_a_Deferred","8.2": "_Toc356575627","8.3": "_Expanded_Navigation_Property","8.4": "_Toc356575629","8.5": "_Toc356575630","14.1": "_Added/Changed_Entity","14.2": "_Deleted_Entity","14.3": "_Added_Link","14.4": "_Deleted_Link","18.1": "_Toc356575645","18.2": "_Toc356575646"} ,

    'ODataV4SpecificationUriForJson': "http://docs.oasis-open.org/odata/odata-json-format/v4.0/cs01/odata-json-format-v4.0-cs01.html",
    'ODataV4SpecificationUriForAtom': "http://docs.oasis-open.org/odata/odata-atom-format/v4.0/cs01/odata-atom-format-v4.0-cs01.html",
    //http://docs.oasis-open.org/odata/odata-atom-format/v4.0/cs01/odata-atom-format-v4.0-cs01.html
    'V4AtomURLFragments': {"" : "", "1": "_Toc230433042","2": "_Toc357168175","3": "_Requesting_the_Atom","4": "_Toc359147088","5": "_Service_Document","6": "_Entity","7": "_Property","8": "_Navigation_Property","9": "_Stream_Property","10": "_Media_Entity","11": "_Individual_Property","12": "_Collection_of_Entities","13": "_Entity_Reference","14": "_Delta_Response","15": "_Function","16": "_Action","17": "_Toc230433178","18": "_Instance_Annotations","19": "_Error_Reponse","20": "_Toc230433208","21": "_Toc230433209","22": "_Toc230433210","2.1.1": "AtomNamespace","2.1.2": "_Toc230433048","2.1.3": "_Atom_Tombstone_Namespace","2.1.4": "_OData_Data","2.1.5": "_OData_Metadata","5.1.1": "_Toc359416682","5.1.2": "_Toc359416683","5.3.1": "_Toc230433066","5.3.2": "_Toc230433067","5.3.3": "_Toc230433068","5.4.1": "_Toc230433070","5.4.2": "_Toc230433071","5.4.3": "_Toc230433072","5.5.1": "_Toc230433074","5.5.2": "_Toc230433075","5.5.3": "_Toc230433076","5.6.1": "_Toc230433078","5.6.2": "_Toc230433079","6.1.1": "_Toc359862344","6.1.2": "_Attribute_metadata:metadata_context","6.1.3": "_Toc230433084","7.3.1": "_Attribute_metadata:type","7.3.2": "_Attribute_metadata:null","7.6.1": "_Toc359511844","7.7.1": "_Toc357168232","8.1.1": "_Element_atom:link","8.2.1": "_Toc230433108","9.1.1": "_Toc230433118","9.1.2": "_Toc230433119","9.1.3": "_Toc230433120","9.1.4": "_Toc230433121","9.1.5": "_Toc230433122","10.1.1": "_Toc230433125","10.1.2": "_Toc230433126","10.2.1": "_Toc230433128","10.2.2": "_Toc230433129","11.1.1": "_Element_metadata:value","11.2.1": "_Toc230433138","12.1.1": "_Attribute_metadata:metadata_1","12.1.2": "_Toc230433143","12.4.1": "_Toc358116704","13.1.1": "_Toc359147191","13.1.2": "_Toc359147192","14.2.1": "_Toc230433154","14.3.1": "_Element_metadata:link","14.4.1": "_Element_metadata:deleted-link","15.1.1": "_Toc230433170","15.1.2": "_Attribute_target","15.1.3": "_Toc230433172","16.1.1": "_Toc230433175","16.1.2": "_Toc359511929","16.1.3": "_Toc230433177","18.1.1": "_Attribute_target_1","18.1.2": "_Toc230433182","18.1.3": "_The_type_attribute","18.1.4": "_Toc230433184","18.2.1": "_Primitive_Values","18.2.2": "_Collection_Value","18.2.3": "_Structure_Annotations","18.3.1": "_Annotating_a_feed","18.3.2": "_Annotating_an_entry","18.3.3": "_Toc359416829","18.3.4": "_Toc359416830","18.3.5": "_Toc230433193","18.3.6": "_Toc230433194","18.3.7": "_Annotating_a_function","18.3.8": "_Error","18.3.9": "_Toc359416835","19.5.1": "_Toc230433203","19.5.2": "_Toc230433204","19.5.3": "_Toc230433205","19.5.4": "_Toc230433206","1.1": "_Toc230433043","1.2": "_Toc230433044","1.3": "_Toc356909526","2.1": "_Toc230433046","2.2": "_Toc230433052","4.1": "_Toc230433055","4.2": "_Toc230433056","4.3": "_Toc230433057","5.1": "_Toc230433059","5.2": "appWorkspace","5.3": "appCollection","5.4": "_Element_metadata:function-import","5.5": "_Element_metadata:entity","5.6": "_Element_metadata:service-document","6.1": "_Toc359871343","6.2": "_Element_atom:id","6.3": "_Toc230433086","6.4": "_Toc230433087","6.5": "_Toc356909999","7.1": "_Primitive_Value","7.2": "metadataProperties","7.3": "_Element_data:[PropertyName]","7.4": "_Primitive_and_Enumeration_2","7.5": "_Complex_Property","7.6": "_Primitive_and_Enumeration_1","7.7": "_Complex_Collection_Property","8.1": "_Navigation_Link","8.2": "_Association_Link","8.3": "_Expanded_Navigation_Property","8.4": "_Toc230433114","8.5": "_Toc230433115","9.1": "atomLink","10.1": "_Toc230433124","10.2": "_Toc230433127","11.1": "_Single_Scalar_Value","11.2": "_Collection_of_Scalar","12.1": "_Element_atom:feed","12.2": "_Toc230433144","12.3": "_Toc356910057","12.4": "_Toc230433146","13.1": "_Element_metadata:ref_1","14.1": "_Changed/Added_Entities_as","14.2": "_Deleted_entities_as_1","14.3": "_Added_Link","14.4": "_Deleted_Link","15.1": "_Element_metadata:function","16.1": "_Element_metadata:action","18.1": "_The_metadata:Annotation_Element","18.2": "_Toc230433185","18.3": "_Instance_Annotation_Targets","19.1": "_The_metadata:error_Element","19.2": "metadataCode","19.3": "metadataMessage","19.4": "_Toc230433201","19.5": "_Toc230433202","19.6": "_Toc230433207"},
    'ODataV4SpecificationUriForProtocol': "http://docs.oasis-open.org/odata/odata/v4.0/cs01/part1-protocol/odata-v4.0-cs01-part1-protocol.html",
    'ODataV4SpecificationUriForURL': "http://docs.oasis-open.org/odata/odata/v4.0/cs01/part2-url-conventions/odata-v4.0-cs01-part2-url-conventions.html",
    'ODataV4SpecificationUriForCSDL': "http://docs.oasis-open.org/odata/odata/v4.0/cs01/part3-csdl/odata-v4.0-cs01-part3-csdl.html",
    'payload_text_help': "paste here the response payload in atompub/xml or json format in full",
    'meta_text_help': "paste here the metadata document content in xml format in full if you have one",
    'info_error_resourse':
        "Attention: This is an OData error payload resource. If you meant to validate the error payload against OData spec, please see the validation results as reported below; otherwise, please correct the Uri input and try again.",
    'info_other_odata_resourse':
        "Attention: This is an arbitrary resource seemed to be generated by an OData producer. If you meant to validate it as an OData resource against OData spec, please see the validation results as reported below; otherwise, please correct the Uri input and try again.",
    'info_other_resourse':
        "Attention: This is an arbitrary resource; seems not a valid OData resource. If you meant to validate it as an OData resource against OData spec, please see the validation results as reported below; otherwise, please correct the Uri input and try again.",
    'offline_info_other_odata_resourse':
        "Attention: This is an arbitrary resource seemed to be generated by an OData producer. If you meant to validate it as an OData resource against OData spec, please see the validation results as reported below; otherwise, please correct the payload input and try again.",
    'offline_info_other_resourse':
        "Attention: This is an arbitrary resource; seems not a valid OData resource. If you meant to validate it as an OData resource against OData spec, please see the validation results as reported below; otherwise, please correct the payload input and try again."            
};

var conformanceResults = {
    'MinimalRulesResults': { 'errors': 0, 'warnings': 0, 'successes': 0, 'recommendations': 0, 'aborteds': 0, 'skip': 0, 'notApplicables': 0 },
    'InterMediateRulesResults': { 'errors': 0, 'warnings': 0, 'successes': 0, 'recommendations': 0, 'aborteds': 0, 'skip': 0, 'notApplicables': 0 },
    'AdvancedRulesResults': { 'errors': 0, 'warnings': 0, 'successes': 0, 'recommendations': 0, 'aborteds': 0, 'skip': 0, 'notApplicables': 0 },
    'MinimalRules': [],
    'InterMediateRules': [],
    'AdvancedRules': [],
    'conformanceJobIDs': [],
    'checkJodIDCount': 0,
    'ResultDetails': [],
    "ResultDetailChecked": -1,
    "rerunloadover":-1
};

var currentDetail;

$(function () {
    resetAll();
    initClickHandlers();
});

function initClickHandlers() {
    function emptyDataForRerun(index) {
        dataforRerun.JobDetails[index] = [];
        dataforRerun.JobNotes[index] = [];
        dataforRerun.Jobs[index] = [];
        dataforRerun.Results[index] = [];
    }
    $("#liveValidateButton").click(function () {
        emptyDataForRerun(0);
        submitValidationJobByUri();
    });
    $("#offlineValidateButton").click(function () {
        emptyDataForRerun(1);
        submitValidationJobByDirectText();
    });
    $("#ConformanceValidation").click(function () {
        emptyDataForRerun(2);
        submitConformanceValidationJob();
    });
    $("#metadataValidateButton").click(function () {
        emptyDataForRerun(3);
        submitMetadataValidationJobByUri();
    });

    $("#meta_icon").click(function (ev) { evh_enableCollapse(ev); });

    resigterUriRerun();
    registerConformanceRerun();
}

// global error handlers
OData.defaultError = function (err) {
    try {
        if (err.response.statusCode == 509) {
            displayStatusText("Payload is too large - please retry with another URI");
        } else if (err.response.statusCode == 403) {
            displayStatusText("Unsupported Uri scheme - please specify a URI using http or https scheme");
        } else {
            displayStatusText("Error retrieving validation results - please retry.");
        }
    }
    catch (e) {
        displayStatusText("Error retrieving validation results - please retry.");
    }

    enableTabSwitch();
}

function updateMasterJobId(id) {
    var activeLiA = $("#tabs_container > ul > li.active > a")[0];
    activeLiA.MasterJobId = id;
}

function resetApp() {
    validatorApp.jobsInQ = [];
    validatorApp.panes = 0;

    resetJob();
    resetConformanceResults();
}

function resetJob() {
    validatorApp.results = { 'errors': 0, 'warnings': 0, 'successes': 0, 'recommendations': 0, 'aborteds': 0, 'notApplicables': 0 };
    validatorApp.currentJob = null;
    validatorApp.currJobDetail = null;
    validatorApp.currJobNote = null;
    if (validatorApp.LoadResultsTimeoutObj) {
        clearTimeout(validatorApp.LoadResultsTimeoutObj);
        validatorApp.LoadResultsTimeoutObj = null
    }

    validatorStat.totalRequestsSent = 0;
}

function resetAll() {
    clearDisplay()
    resetApp();
    $('#infotop #statusInfo').hide();
}

function resetConformanceResults() {
    conformanceResults.MinimalRulesResults = { 'errors': 0, 'warnings': 0, 'successes': 0, 'recommendations': 0, 'aborteds': 0, 'skip': 0, 'notApplicables': 0 };
    conformanceResults.InterMediateRulesResults = { 'errors': 0, 'warnings': 0, 'successes': 0, 'recommendations': 0, 'aborteds': 0, 'skip': 0, 'notApplicables': 0 };
    conformanceResults.AdvancedRulesResults = { 'errors': 0, 'warnings': 0, 'successes': 0, 'recommendations': 0, 'aborteds': 0, 'skip': 0, 'notApplicables': 0 };
    conformanceResults.MinimalRules = [];
    conformanceResults.InterMediateRules = [];
    conformanceResults.AdvancedRules = [];
    conformanceResults.conformanceJobIDs = [];
    conformanceResults.checkJodIDCount = 0;
    conformanceResults.ResultDetails = [];
}

// Justify if there is input text or not for Uri/Payload text for validation
function isNoInput() {
    var index = -1;
    var tabs = $('#tabs_container .tab_contents');
    tabs.each(function (i) {
        if ($(this).hasClass('tab_contents_active')) {
            index = i;
        }
    })
    var input = $('input', tabs[index]).val();
    if (index == 1) {
        input = $('textarea', tabs[index]).val();
    }
    var inputTable = ["enter a url", "paste here the response payload", "enter a url", "enter a url"];
    var msgTable = ['Url should not be empty.', 'Payload input should not be empty.', 'Url should not be empty.', 'Url should not be empty.'];

    if (index != -1 && input.indexOf(inputTable[index]) != -1) {
        $('#statusInfo').empty().show().append("<p class='errorMsg'>" + msgTable[index] + "</p>");
        return true;
    }

    clearDisplay();
    return false;
}

function onJobStart() {
    resetAll();
    $('#infotop #statusInfo').show();
    disableTabSwitch();
}

function onJobEnd() {
    alertJobComplete();
    displayJobSummary();

    if (jobHasAnyIssue()) {
        //color code to highlight
        jQuery('#summary', validatorApp.currJobDetail).addClass("highlit");
    }
    var num = $('#tabs_container > ul > li.active > a').attr('rel').slice(5, 6);
    var index = parseInt(num) - 1;
    dataforRerun.JobDetails[index].push(validatorApp.currJobDetail);
    dataforRerun.JobNotes[index].push(validatorApp.currJobNote);
    dataforRerun.Jobs[index].push(validatorApp.currentJob);
    dataforRerun.Results[index].push(validatorApp.results);

    validatorApp.currJobDetail = null;
    validatorApp.currJobNote = null;
}

function alertSendoutRequest() {
    displayStatusText("Sending validation request...");
}

function alertReceiveResponse() {
    displayStatusText("Retrieving response payload...");
}

function alertTimeout() {
    updateJobStatus("Operation timed out. Please retry.");
}

function alertJobComplete() {
    displayStatusText("Validation complete.");
}

function alertPayloadInProgress() {
    displayStatusText("Loading source payload...");
}

function alertResultsInProgress() {
    displayStatusText("Loading validation results...");
}

function alertDetailsInProgress() {
    displayStatusText("Loading validation resultDetails...");
}

// validation controlling methods
function submitValidationJobByUri() {
    if (isNoInput()) {
        return;
    }
    onJobStart();

    if ($('#crawling').is(':checked')) {
        submitCrawlingValidationJob();
    }
    else {
        submitLiveValidationJob();
    }
}

function submitMetadataValidationJobByUri() {
    if (isNoInput()) {
        return;
    }
    onJobStart();
    submitMetadataValidationJob();
}

function submitValidationJobByDirectText() {
    if (isNoInput()) {
        return;
    }
    onJobStart();

    var request = {
        method: "POST",
        requestUri: "odatavalidator/ExtValidationJobs",
        data: { PayloadText: getPayloadText(), MetadataText: getMetadataText(), ReqHeaders: getReqHeaders() }
    }
    OData.request(request, function (data) {
        alertReceiveResponse();
        updateMasterJobId(data.ID);

        validatorApp.jobsInQ = [];
        validatorApp.currentJob = { 'Uri': data.Uri, 'ResourceType': data.ResourceType, 'Id': data.ID, 'RuleCount': data.RuleCount };
        $('#infotop #statusInfo').empty();
        validatorApp.currJobDetail = offlineValidation(validatorApp.currentJob);
        loadPayload(validatorApp.currentJob.Id);
    });
    alertSendoutRequest();
}

function newValidation(newJob, isSimpleJob) {
    newJob.anchor = "pane" + ++validatorApp.panes;
    var link = createLink(toTypeDesc(newJob.ResourceType), "#" + newJob.anchor);
    var jobStatusInfo = { 'resourceType': link, 'status': "starting ..." };
    var currJobStatus = isSimpleJob ? $('#simpleStatusInfoTmpl').tmpl(jobStatusInfo) : $('#jobStatusInfoTmpl').tmpl(jobStatusInfo);
    $('#infotop #statusInfo').append(currJobStatus);
    validatorApp.currJobNote = jQuery('#status', currJobStatus);

    var t = $('#resultTemplate').tmpl(newJob);
    jQuery('.resultbody', t).attr('id', newJob.anchor);
    t.appendTo('#infobody');

    displayHide_buttonsIninfobody(true);

    jQuery('.sources img', t)[0].onclick = evh_enableCollapse;
    jQuery('.results img', t)[0].onclick = evh_enableCollapse;

    if (isSimpleJob)
        return t;
    //+ customize click evh to expand the target automatically
    jQuery("a[href]", currJobStatus)[0].onclick = function () {
        var h = $(this).attr('href');
        expandView($(h + ' .sources'));
        expandView($(h + ' .results'));
        return true;
    }
    return t;
}

function newRegularValidation(newJob) {
    var t = newValidation(newJob, true);
    jQuery("#summary", t).hide();

    // to display infomational message when type is error payload, or
    // to display infomational message when type is other payload
    if (newJob.ResourceType == "Error") {
        jQuery("#informational", t).html("<p>" + validatorConf.info_error_resourse + "</p>");
    } else if (newJob.ResourceType == "Property" || newJob.ResourceType == "RawValue" || newJob.ResourceType == "Link") {
        jQuery("#informational", t).html("<p>" + validatorConf.info_other_odata_resourse + "</p>");
    } else if (newJob.ResourceType == "Other") {
        jQuery("#informational", t).html("<p>" + validatorConf.info_other_resourse + "</p>");
    }

    return t;
}

function offlineValidation(newJob) {
    var t = newValidation(newJob, true);
    jQuery("#summary", t).hide();

    // to display infomational message when type is error payload, or
    // to display infomational message when type is other payload
    if (newJob.ResourceType == "Property" || newJob.ResourceType == "RawValue" || newJob.ResourceType == "Link") {
        jQuery("#informational", t).html("<p>" + validatorConf.offline_info_other_odata_resourse + "</p>");
    } else if (newJob.ResourceType == "Other") {
        jQuery("#informational", t).html("<p>" + validatorConf.offline_info_other_resourse + "</p>");
    }

    return t;
}

function newCrawlingValidation(newJob) {
    var t = newValidation(newJob, false);
    // for crawling validation job to collapse areas by default
    enableCollapse(jQuery('.sources', t))
    enableCollapse(jQuery('.results', t))
    return t;
}

function submitLiveValidationJob() {
    var request = {
        method: "GET",
        requestUri: "odatavalidator/UriValidationJobs"
            + "?Uri='" + encodeURIComponent(getServiceToValidateUri()).replace(new RegExp("'", "g"), "%25%32%37")
            + "'&Format='" + encodeURIComponent(getSelectedFormat()) + encodeURIComponent(getSelectedMetadata()) + "'"
            + createReqHeaders()
    };
    alertSendoutRequest();
    OData.request(request, function (data) {
        alertReceiveResponse();
        $.each(data.results, function () {
            var job = { 'Uri': this.Uri, 'ResourceType': this.ResourceType, 'Id': this.DerivativeJobId, 'RuleCount': this.RuleCount, 'Issues': this.Issues };
            validatorApp.jobsInQ.push(job);
        });
        updateMasterJobId(data.results[0].MasterJobId);

        $('#infotop #statusInfo').empty();
        startJob(true);
    });
}

function submitMetadataValidationJob() {
    var request = {
        method: "GET",
        requestUri: "odatavalidator/UriValidationJobs"
            + "?Uri='" + encodeURIComponent(getServiceMetadataToValidateUri()).replace(new RegExp("'", "g"), "%25%32%37") + "'"
            + "&Format='atompub'"
            + "&byMetadata='yes'"
            + createMetadataReqHeaders()
    };
    alertSendoutRequest();
    OData.request(request, function (data) {
        alertReceiveResponse();
        $.each(data.results, function () {
            var job = { 'Uri': this.Uri, 'ResourceType': this.ResourceType, 'Id': this.DerivativeJobId, 'RuleCount': this.RuleCount, 'Issues': this.Issues };
            validatorApp.jobsInQ.push(job);
        });
        updateMasterJobId(data.results[0].MasterJobId);

        $('#infotop #statusInfo').empty();
        startJob(true);
    });
}

function submitCrawlingValidationJob() {
    var request = {
        method: "GET",
        requestUri: "odatavalidator/UriValidationJobs"
            + "?Uri='" + encodeURIComponent(getServiceToValidateUri())
            + "'&Format='" + encodeURIComponent(getSelectedFormat()) + encodeURIComponent(getSelectedMetadata()) + "'"
            + "&toCrawl='yes'"
            + createReqHeaders()
    };
    alertSendoutRequest();


    OData.request(request, function (data) {
        alertReceiveResponse();
        $.each(data.results, function () {
            var job = { 'Uri': this.Uri, 'ResourceType': this.ResourceType, 'Id': this.DerivativeJobId, 'RuleCount': this.RuleCount, 'Issues': this.Issues };
            validatorApp.jobsInQ.push(job);
        });
        updateMasterJobId(data.results[0].MasterJobId);

        $('#infotop #statusInfo').empty();
        startJob();
    });
}

function submitConformanceValidationJob() {
    if (isNoInput()) {
        return;
    }
    onJobStart();

    var OriginalURI = "odatavalidator/UriValidationJobs"
            + "?Uri='" + encodeURIComponent($("#svcUrl").val())
            + "'&Format='json;odata.metadata=full'"
            + "&Headers='OData-Version:4.0;'"
            + "&isConformance='" + getSelectedResourceType() + "'"
            + "&levelTypes='" + getSelectedConformanceLevel() + "'";
    conformanceResults.OriginalURI = OriginalURI;
    var request = {
        method: "GET",
        requestUri: conformanceResults.OriginalURI
    };
    alertSendoutRequest();

    OData.request(request, function (data) {
        alertReceiveResponse();

        updateMasterJobId(data.results[0].MasterJobId);

        if (data.results.length == 1 && data.results[0].Issues) {
            $('#infotop #statusInfo').empty();
            var jobStatusInfo = { 'status': data.results[0].Issues };
            $('#jobStatusInfoTmpl').tmpl(jobStatusInfo).appendTo('#infotop #statusInfo');
            enableTabSwitch();
            return;
        }
        $.each(data.results, function () {
            var job = { 'Uri': this.Uri, 'ResourceType': this.ResourceType, 'Id': this.DerivativeJobId, 'RuleCount': this.RuleCount, 'Issues': this.Issues };
            validatorApp.jobsInQ.push(job);
        });

        $('#infotop #statusInfo').empty();
        startJobForConformanceLevel();
    });
}

function startJob(isSimpleJob) {
    while (validatorApp.jobsInQ.length > 0) {
        resetJob();
        validatorApp.currentJob = validatorApp.jobsInQ.shift();
        if (validatorApp.currentJob.Issues) {
            //display status message only, w/o anchor
            var jobStatusInfo = { 'status': validatorApp.currentJob.Issues };
            $('#jobStatusInfoTmpl').tmpl(jobStatusInfo).appendTo('#infotop #statusInfo');
            continue;
        }

        validatorApp.currJobDetail = (isSimpleJob) ? newRegularValidation(validatorApp.currentJob) : newCrawlingValidation(validatorApp.currentJob);
        loadPayload(validatorApp.currentJob.Id);
        return;
    }

    if (validatorApp.currentJob.Issues == null) {
        createRecordLink();
    }
    enableTabSwitch();
}

function resigterUriRerun() {
    var RerunObject = (function () {
        var allSelectedLis;
        var jobid;
        // stores all conformance rule newest results. it will be init at first click, then updated at every rerun.
        var testResults;
        var complete;
        var JobData;
        var resultView;
        var summarychange;
        // invalid judgement 

        function UIResultUpdate(data) {
            //Concurrent conflict
            if (data.results[0].Issues && data.results[0].Issues.length != 0) {
                alert("Error: Job is not complete or somebody else is rerunning this job!");
                enableTabSwitch();
                return;
            }

            JobData = data;
            function waitUtilComplete() {
                var Completequery = "odatavalidator/ValidationJobs(guid'" + jobid + "')/Complete";

                validatorApp.currJobNote.empty().append("Rerunning rules...");
                OData.read(Completequery, function (data) {
                    if (data.Complete == true) {
                        complete = true;
                        UIResultUpdate(JobData);
                    } else {
                        setTimeout(waitUtilComplete, 500);
                    }
                });
            }
            if (complete == false) {
                waitUtilComplete();
                return;
            }

            validatorApp.currJobNote.empty().append("Loading " + allSelectedLis.length + " rerun results...");
            summarychange = false;

            var updateCount = allSelectedLis.length;
            for (var i = 0; i < allSelectedLis.length; i++) {
                var tid = allSelectedLis[i].getAttribute("testresultid");
                var query = "odatavalidator/TestResults(" + tid + ")";
                OData.read(query, function (data) {
                    updateCount--;
                    var li = $('#display > li[testresultid="' + data.ID + '"]', resultView)[0];

                    var resultLine = createTestResultHtml(data, "checked");
                    var classification = li.getAttribute("class").slice(0, -"Result".length)

                    if (classification != data.Classification) {
                        resultLine = resultLine.slice(0, 4) + "style='background-color:#edf5ef;'" + resultLine.slice(4);

                        if (data.Classification == "error") validatorApp.results.errors++;
                        else if (data.Classification == "warning") validatorApp.results.warnings++;
                        else if (data.Classification == "recommendation") validatorApp.results.recommendations++;
                        else if (data.Classification == "success") validatorApp.results.successes++;
                        else if (data.Classification == "notApplicable") { validatorApp.results.notApplicables++; }
                        else if (data.Classification == "aborted") { validatorApp.results.aborteds++; }
                        highlightSourceLine(data.LineNumberInError);  //bug  cancel the old highlight

                        if (classification == "error") validatorApp.results.errors--;
                        else if (classification == "warning") validatorApp.results.warnings--;
                        else if (classification == "recommendation") validatorApp.results.recommendations--;
                        else if (classification == "success") validatorApp.results.successes--;
                        else if (classification == "notApplicable") validatorApp.results.notApplicables--;
                        else if (classification == "aborted") validatorApp.results.aborteds--;
                        summarychange = true;
                    }

                    li.outerHTML = resultLine;

                    if (updateCount == 0) {
                        if (summarychange) {
                            displayJobSummary();

                            if (jobHasAnyIssue()) {
                                //color code to highlight
                                jQuery('#summary', validatorApp.currJobDetail).addClass("highlit");
                            }
                        }
                        validatorApp.currJobNote.empty().append("Rerun complete.");
                        enableTabSwitch();
                        validatorApp.currJobDetail = null;
                        validatorApp.currJobNote = null;
                    }
                });
            }
        }

        return {
            "click": function (e) {
                resultView = $(this).next("div#resultsView")[0];
                allSelectedLis = $('#display > li input[type="checkbox"]', resultView)
                    .filter(function (index) {
                        return this.checked == true;
                    })
                    .parent();

                var number = $(e.target).parent().parent().attr("id").slice(-1);
                number = parseInt(number);
                var num = $('#tabs_container > ul > li.active > a').attr('rel').slice(5, 6);
                var index = parseInt(num) - 1;
                validatorApp.currJobDetail = dataforRerun.JobDetails[index][number - 1];
                validatorApp.currJobNote = dataforRerun.JobNotes[index][number - 1];
                validatorApp.currentJob = dataforRerun.Jobs[index][number - 1];
                validatorApp.results = dataforRerun.Results[index][number - 1];

                complete = false;
                jobid = document.querySelector("#tabs_container > ul > li.active > a").MasterJobId;
                testResultIds = "";

                for (var i = 0; i < allSelectedLis.length ; i++) {
                    var tid = allSelectedLis[i].getAttribute("testresultid");
                    testResultIds += tid + ";";
                }

                var request = {
                    method: "GET",
                    requestUri: "odatavalidator/SimpleRerunJob"
                              + "?jobIdStr='" + jobid + "'"
                              + "&testResultIdsStr='" + testResultIds + "'"
                }
                if (allSelectedLis.length == 0) {
                    return;
                }
                validatorApp.currJobNote.empty().append("Starting rerunning " + allSelectedLis.length + " rules...");
                disableTabSwitch();
                OData.request(request, UIResultUpdate);
            }
        }
    })();

    $("div.results > button").live("click", RerunObject.click);
}

function registerConformanceRerun() {
    var RerunObject = (function () {
        var allSelectedLis;
        var jobid;
        // the select rerun test result ID list string  split by ;
        var testResultIds;
        // stores all conformance rule newest results. it will be init at first click, then updated at every rerun.
        var testResults;
        var complete;
        var JobData;

        function UIResultUpdate(data) {

            JobData = data;
            jobid = data.results[0].DerivativeJobId;
            function waitUtilComplete() {
                var Completequery = "odatavalidator/ValidationJobs(guid'" + jobid + "')/Complete";
                $('#infotop #statusInfo #status').empty().append("Rerunning rules...");
                OData.read(Completequery, function (data) {
                    if (data.Complete == true) {
                        complete = true;
                        UIResultUpdate(JobData);
                    } else {
                        setTimeout(waitUtilComplete, 500);
                    }
                });
            }
            if (complete == false) {
                waitUtilComplete();
                return;
            }

            $('#infotop #statusInfo #status').empty().append("Loading " + allSelectedLis.length + " rerun results...");

            var i = 0, j = 0;
            var updateTestResultsArray = [];
            var updateCount = allSelectedLis.length;
            var update = [false, false, false];
            for (i = 0; i < allSelectedLis.length; i++) {
                var tid = allSelectedLis[i].getAttribute("testresultid");
                var query = "odatavalidator/TestResults(" + tid + ")";
                OData.read(query, function (data) {
                    updateCount--;
                    updateTestResultsArray.push(data);
                    // Get old test result
                    for (j = 0; j < testResults.length; j++) {
                        if (testResults[j].ID == data.ID) {
                            if (testResults[j].Classification != data.Classification) {
                                if (data.RuleName.indexOf("Minimal") != -1) {
                                    updateConformanceRerunResults(conformanceResults.MinimalRulesResults, data.Classification, testResults[j].Classification);
                                } else if (data.RuleName.indexOf("Intermediate") != -1) {
                                    updateConformanceRerunResults(conformanceResults.InterMediateRulesResults, data.Classification, testResults[j].Classification);
                                } else if (data.RuleName.indexOf("Advanced") != -1) {
                                    updateConformanceRerunResults(conformanceResults.AdvancedRulesResults, data.Classification, testResults[j].Classification);
                                }
                                update[getTypeByRuleName(data)] = true;
                            }

                            testResults[j] = data;
                            break;
                        }
                    }

                    if (updateCount == 0) {
                        // Update summary info for every level
                        var confTemplate = $('#conformanceLevelTemplate').tmpl(conformanceResults);
                        jQuery(" > b", confTemplate).append(summarizeConformanceLevel());

                        // Update summary info for test results in every level
                        if (update[0]) jQuery("#minimalRulesCount").text(updateConformanceLevelRules(conformanceResults.MinimalRulesResults));
                        if (update[1]) jQuery("#interRulesCount").text(updateConformanceLevelRules(conformanceResults.InterMediateRulesResults));
                        if (update[2]) jQuery("#advancedRulesCount").text(updateConformanceLevelRules(conformanceResults.AdvancedRulesResults));
                        // Update test result details for every updated test result
                        conformanceResults.ResultDetailChecked = updateTestResultsArray.length;
                        for (i = 0; i < updateTestResultsArray.length; i++) {
                            for (j = 0; j < conformanceResults.ResultDetails.length; j++) {
                                if (conformanceResults.ResultDetails[j].TestResultID == updateTestResultsArray[i].ID) {
                                    conformanceResults.ResultDetails.splice(j,1);
                                    j--;
                                }
                            }
                            var query = "odatavalidator/TestResults(" + updateTestResultsArray[i].ID + ")/ResultDetails?$orderby=ID";
                            OData.read(query, function (data) {
                                $.each(data.results, function () {
                                    conformanceResults.ResultDetails.push(this);
                                });
                                conformanceResults.ResultDetailChecked--;
                                if (conformanceResults.ResultDetailChecked === 0) {
                                    for (i = 0; i < updateTestResultsArray.length; i++) {
                                        if (updateTestResultsArray[i].RuleName.split('.')[2].length == 6) {
                                            continue;
                                        }
                                        var lis = $('#display > li[testresultid=\'' + updateTestResultsArray[i].ID + '\']')[0];
                                        if (!lis) continue;

                                        var arg_pack = { resultLine: "" };
                                        for (j = 0; j < testResults.length; j++) {
                                            if (testResults[j].ID == updateTestResultsArray[i].ID) {
                                                break;
                                            }
                                        }
                                        createConformanceLevelRulesHtml(testResults, j, arg_pack, true);
                                        lis.outerHTML = arg_pack.resultLine;
                                        var lis = $('#display > li[testresultid=\'' + updateTestResultsArray[i].ID + '\']')[0];
                                        $("> div  p > font", lis).each(conformanceDisplayDecorate);
                                    }

                                    for (i = 0; i < updateTestResultsArray.length; i++) {
                                        if (updateTestResultsArray[i].RuleName.split('.')[2].length == 6) {
                                            var selector = "#display > li > div > ul > li[testresultid='" + updateTestResultsArray[i].ID + "'] > input[type='checkbox']";
                                            document.querySelector(selector).checked = true;
                                        }
                                    }

                                    $('#infotop #statusInfo #status').empty().append("Rerun complete.");
                                    enableTabSwitch();
                                }
                            });
                        }
                    }
                });
            }
        }

        return {
            "click": function () {
                allSelectedLis = $('#display > li input[type="checkbox"]')
                    .filter(function (index) { return this.checked == true; })
                    .parent();
                if (allSelectedLis.length == 0)
                    return;

                complete = false;
                jobid = document.querySelector("#tabs_container > ul > li.active > a").MasterJobId;
                testResultIds = "";

                if (typeof testResults == "undefined")
                    testResults = [].concat(conformanceResults.AdvancedRules, conformanceResults.InterMediateRules, conformanceResults.MinimalRules);

                for (var i = 0; i < allSelectedLis.length ; i++) {
                    var tid = allSelectedLis[i].getAttribute("testresultid");
                    testResultIds += tid + ";";
                }

                var request = {
                    method: "GET",
                    requestUri: "odatavalidator/ConformanceRerunJob"
                                + "?jobIdStr='" + jobid + "'"
                                + "&testResultIdsStr='" + testResultIds + "'"
                }

                $('#infotop #statusInfo #status').empty().append("Starting rerunning " + allSelectedLis.length + " rules...");
                disableTabSwitch();  //disable button
                OData.request(request, UIResultUpdate);
            }
        }
    })();

    $("#infobody > div > button").live("click", RerunObject.click);
}

function createRecordLink() {
    $('#recorddiv').empty().append("<p>Generate a <a id='record' style='cursor:pointer;'>Reload Link</a> to share the validation results</p>");
    document.querySelector('#infotop #record').onclick = createShareLink;

    function createShareLink(e) {
        var liaa = document.querySelector('#tabs_container > ul > li.active > a');
        var li1a = document.querySelector('#tabs_container > ul > li > a');
        var li2a = li1a.parentNode.nextElementSibling.firstElementChild;
        var li3a = li2a.parentNode.nextElementSibling.firstElementChild;
        var li4a = li3a.parentNode.nextElementSibling.firstElementChild;
        
        if (!(liaa.MasterJobId)) {
            alert("there is no result of this tab!");
            return;
        }

        var tabnum = $('#tabs_container li.active a')[0].rel.substr(5, 1);
        var pid = window.location.hash;

        var request = {
            method: "POST",
            requestUri: "odatavalidator/Records",
            data: {
                MasterJobId1: tabnum == 1 ? li1a.MasterJobId : null,  //that master jobID
                MasterJobId2: tabnum == 2 ? li2a.MasterJobId : null,
                MasterJobId3: tabnum == 3 ? li3a.MasterJobId : null,
                MasterJobId4: tabnum == 4 ? li4a.MasterJobId : null,
                ActiveTabNum: tabnum
            }
        }

        OData.request(request, function (data) {
            // send the tab data package and choices to service and get a tagid result.
            // delete this button and copy this url to clipboard and redirect to this url
            console.log(data);
            window.open(window.location.origin
                        + window.location.pathname
                        + "#" + data.ID);
        });
    }
}

function startJobForConformanceLevel() {
    while (validatorApp.jobsInQ.length > 0) {
        resetJob();
        validatorApp.currentJob = validatorApp.jobsInQ.shift();
        if (validatorApp.currentJob.Issues) {
            //display status message only, w/o anchor
            var jobStatusInfo = {'status': validatorApp.currentJob.Issues };
            $('#jobStatusInfoTmpl').tmpl(jobStatusInfo).appendTo('#infotop #statusInfo');

            onConformanceLevelJobEnd();
            continue;
        }
        $('#infotop #statusInfo').empty();
        loadPayloadForConformanceLevel(validatorApp.currentJob.Id);
        return;
    }

    enableTabSwitch();
}

function loadPayloadForConformanceLevel(jobID) {
    alertPayloadInProgress();
    var query = "odatavalidator/ValidationJobs(guid'" + jobID + "')/PayloadLines?$orderby=LineNumber";
    OData.read(query, function (data) {
        $.each(data.results, function () {
            //addNewSourceLine(this);
        });
        $('#infotop #statusInfo').empty();
        loadResultsForConformanceLevel(jobID, 0);
    });
}

function loadResultsForConformanceLevel(jobID, totalResultsRetrieved) {
    $('#infotop #statusInfo').empty();
    alertResultsInProgress();   
    var query = "odatavalidator/ValidationJobs(guid'" + jobID + "')/TestResults?$orderby=ID";
    if (totalResultsRetrieved > 0) { query += "&$skip=" + totalResultsRetrieved };

    OData.read(query, function (data) {
        $.each(data.results, function () {
            //addNewResult(this);
            totalResultsRetrieved++;

            var info = "Retrieving " + totalResultsRetrieved + " validation results...";           
            $('#infotop #statusInfo').empty();
            displayStatusText(info);
        });
        loadMoreResultsForConformanceLevel(jobID, totalResultsRetrieved);
    });
}

function loadMoreResultsForConformanceLevel(jobID, totalResultsRetrieved) {
    if (totalResultsRetrieved < validatorApp.currentJob.RuleCount) {
        validatorStat.totalRequestsSent++;
        if (validatorStat.totalRequestsSent < validatorConf.maxRequests + 26) {
            validatorApp.LoadResultsTimeoutObj = setTimeout(function () { loadResultsForConformanceLevel(jobID, totalResultsRetrieved) }, validatorConf.perRequestDelay);
        } else {            
            alertTimeout();
            enableTabSwitch();
        }
    } else {
        onConformanceLevelJobEnd();
        startJobForConformanceLevel();
    }
}

function onConformanceLevelJobEnd() {
    conformanceResults.conformanceJobIDs.push(validatorApp.currentJob.Id);

    if (jobHasAnyIssue()) {
        //color code to highlight
        jQuery('#summary', validatorApp.currJobDetail).addClass("highlit");
    }
    validatorApp.currJobDetail = null;
    validatorApp.currJobNote = null;

    $('#infotop #statusInfo').empty();
    checkNotApplicableRules(validatorApp.currentJob.Id);
}

function checkNotApplicableRules(jobID) {
    var retrievedPending = 0;
    var query;
    conformanceResults.checkJodIDCount = 0;
    query = "odatavalidator/ValidationJobs(guid'" + jobID + "')/TestResults?$orderby=ID";
    OData.read(query, function (data) {
        $.each(data.results, function () {
            if (this.Classification == "pending") {
                retrievedPending++;
            }
        });
        $('#infotop #statusInfo').empty();
        checkMorePendingRules(retrievedPending, jobID);
    });
}

function checkMorePendingRules(retrievedPending, jobId) {
    var storedId;

    if (retrievedPending > 0) {
        validatorStat.totalRequestsSent++;
        if (validatorStat.totalRequestsSent < validatorConf.maxRequests) {
            validatorApp.LoadResultsTimeoutObj = setTimeout(function () { checkNotApplicableRules(jobId) }, validatorConf.perRequestDelay);
        } else {
            alertTimeout();
            enableTabSwitch();
        }
    } else {
        if (storedId != jobId) {
            conformanceResults.checkJodIDCount++;
            storedId = jobId;
        }

        if (conformanceResults.checkJodIDCount == conformanceResults.conformanceJobIDs.length) {
            loadConformanceLevelRules();
            startJob();
        }
    }
}

function loadConformanceLevelRules() {
    alertResultsInProgress(); 
    conformanceResults.checkJodIDCount = 0;
    for (var i = 0; conformanceResults.conformanceJobIDs[i]; i++) {
        var query = "odatavalidator/ValidationJobs(guid'" + conformanceResults.conformanceJobIDs[i] + "')/TestResults?$orderby=ID";
        OData.read(query, function (data) {
            $.each(data.results, function () {
                addConformanceLevelRulesResult(this);
            });

            conformanceResults.checkJodIDCount++;
            $('#infotop #statusInfo').empty();
            if (conformanceResults.checkJodIDCount == conformanceResults.conformanceJobIDs.length) {
                sortConformanceRuleResults();

                var testResults = [].concat(conformanceResults.AdvancedRules, conformanceResults.InterMediateRules, conformanceResults.MinimalRules);
                $.each(testResults, function () {
                    //addConformanceLevelRulesResult(this);
                });
                loadConformanceLevelResultDetails();
            }
        });       
    }
}

function loadConformanceLevelResultDetails() {
    //alertDetailsInProgress();
    conformanceResults.ResultDetails = [];
    conformanceResults.ResultDetailChecked = conformanceResults.MinimalRules.length + conformanceResults.InterMediateRules.length + conformanceResults.AdvancedRules.length;
    for (var i = 0; conformanceResults.MinimalRules[i]; i++) {
        getResultDetailOfTestResult(conformanceResults.MinimalRules[i].ID)
    }

    for (var i = 0; conformanceResults.InterMediateRules[i]; i++) {
        getResultDetailOfTestResult(conformanceResults.InterMediateRules[i].ID);
    }

    for (var i = 0; conformanceResults.AdvancedRules[i]; i++) {
        getResultDetailOfTestResult(conformanceResults.AdvancedRules[i].ID);
    }
}

function getResultDetailOfTestResult(resultId) {
    var query = "odatavalidator/TestResults(" + resultId + ")/ResultDetails";
    OData.read(query, function (data) {
        $.each(data.results, function () {
            conformanceResults.ResultDetails.push(this);
        });
        conformanceResults.ResultDetailChecked--;
        if (conformanceResults.ResultDetailChecked === 0) {
            sortConformanceResultDetails();
            displayConformanceLevelResult();
            enableTabSwitch();
        }
    });
}

function updateConformanceLevelRules(ruleResults) {
    var total = ruleResults.successes + ruleResults.errors + ruleResults.notApplicables + ruleResults.warnings + ruleResults.recommendations + ruleResults.aborteds + ruleResults.skip;
    var executed = total - ruleResults.aborteds;

    var statusLine = "(" + executed + " of " + total  + " executed";
    statusLine += categorizedMessage(false, ruleResults.errors, "error");
    if (ruleResults.notApplicables > 0) { statusLine += ", " + ruleResults.notApplicables + " inapplicable"; }    
    statusLine += categorizedMessage(false, ruleResults.warnings, "warning");
    statusLine += categorizedMessage(false, ruleResults.recommendations, "recommendation"); 
    if (ruleResults.skip > 0) { statusLine += ", " + ruleResults.skip + " skip"; }
    statusLine += ")";

    return statusLine;
}

function summarizeConformanceLevel() {
    var summary;
    if (conformanceResults.MinimalRulesResults.errors > 0) {
        summary = "\<font size='4'\>The service does not appear to meet Minimal conformance level.\</font\>";
    } else if (conformanceResults.InterMediateRulesResults.errors > 0) {
        summary = "Congratulations! The service appears to meet Minimal conformance level."
    } else if (conformanceResults.AdvancedRulesResults.errors > 0) {
        summary = "Congratulations! The service appears to meet InterMediate conformance level."
    } else {
        summary = "Congratulations! The service appears to meet Advanced conformance level."
    }

    return summary;
}

function sortConformanceRuleResults() {    
    var by = function (name, name2) {
        return function (o, p) {
            var a, b;
            if (typeof o === "object" && typeof p === "object" && o && p) {
                a = o[name];
                b = p[name];

                if (typeof a === typeof b) {
                    if(a !== b)
                        return a < b ? -1 : 1;
                    else
                        return  o[name2] > p[name2] ? -1 : 1;
                }
                return typeof a < typeof b ? -1 : 1;
            }
            else {
                throw ("error");
            }
        }
    }

    function deleteDuplate(array) {
        var returnVar = [];
        var k = 0;
        for (var i = 1; i < array.length; i++) {
            if (array[i].RuleName == array[i - 1].RuleName) {
                k++;
                continue;
            }
            array[i - k] = array[i];
        }
        return array.length - k;
    }
    conformanceResults.MinimalRules.sort(by("RuleName", "ID"));
    conformanceResults.InterMediateRules.sort(by("RuleName", "ID"));
    conformanceResults.AdvancedRules.sort(by("RuleName", "ID"));

    conformanceResults.MinimalRules = conformanceResults.MinimalRules.slice(0, deleteDuplate(conformanceResults.MinimalRules));
    conformanceResults.InterMediateRules = conformanceResults.InterMediateRules.slice(0, deleteDuplate(conformanceResults.InterMediateRules));
    conformanceResults.AdvancedRules = conformanceResults.AdvancedRules.slice(0, deleteDuplate(conformanceResults.AdvancedRules));
}

function sortConformanceResultDetails() {
    function ResultDetailsCompare(o, p) {
        if ( !(typeof o === "object" && typeof p === "object" && o && p) )
            throw ("error");

        if (o.TestResultID != p.TestResultID)
            return o.TestResultID - p.TestResultID;
        else return o.ID - p.ID;
    }
    conformanceResults.ResultDetails.sort(ResultDetailsCompare); // TODO
}

function findResultDetails(TestResultID, resultDetailID) {
    for (var i = 0; i < conformanceResults.ResultDetails.length; i++)
        if (conformanceResults.ResultDetails[i].TestResultID == TestResultID)
            if (typeof resultDetailID == "undefined") return i;
            else if (resultDetailID == conformanceResults.ResultDetails[i].ID) return i;
    return conformanceResults.ResultDetails.length;
}

function findResultDetails2(TestResultID, resultDetailID) {
    var k = TestResultID;
    if( typeof resultDetailID === "undefined" )
        k -= 0.5;

    var l = 0, h = conformanceResults.ResultDetails.length - 1, m;
    while (l <= h) {
        m = parseInt( (l + h) / 2 );
        var tvar = conformanceResults.ResultDetails[m];
        if (tvar.TestResultID === TestResultID) {
            if (typeof resultDetailID === "undefined")
                return m;
            if (tvar.ID === resultDetailID) return m;
            else if (tvar.ID > resultDetailID) h = m - 1;
            else l = m + 1;
        } else if (tvar.TestResultID > TestResultID)
            h = m - 1;
        else l = m + 1;
    }
    if (l < conformanceResults.ResultDetails.length)
        if (conformanceResults.ResultDetails[l].TestResultID != TestResultID) {
            return conformanceResults.ResultDetails.length;
        }
    return l;
}

function addConformanceLevelRulesResult(testResult) {
    if (testResult.RuleName.indexOf("Minimal") != -1) {
        updateConformanceResults(testResult.Classification, conformanceResults.MinimalRulesResults);
        conformanceResults.MinimalRules.push(testResult);
    } else if (testResult.RuleName.indexOf("Intermediate") != -1) {
        updateConformanceResults(testResult.Classification, conformanceResults.InterMediateRulesResults);
        conformanceResults.InterMediateRules.push(testResult);
    } else if (testResult.RuleName.indexOf("Advanced") != -1) {
        updateConformanceResults(testResult.Classification, conformanceResults.AdvancedRulesResults);
        conformanceResults.AdvancedRules.push(testResult);
    }
}

function getTypeByRuleName(testResult){
    return "MIA".indexOf(testResult.RuleName[0]);
}

function updateConformanceRerunResults(ruleResult, classification, preClassification) {
    if (classification == preClassification) {
        return false;
    }

    if (classification == "error") ruleResult.errors++;
    else if (classification == "warning") ruleResult.warnings++;
    else if (classification == "recommendation") ruleResult.recommendations++;
    else if (classification == "success") ruleResult.successes++;
    else if (classification == "notApplicable") { ruleResult.notApplicables++; }
    else if (classification == "aborted") { ruleResult.aborteds++; }
    else if (classification == "skip") { ruleResult.skip++; }
    
    if (preClassification == "error") ruleResult.errors--;
    else if (preClassification == "warning") ruleResult.warnings--;
    else if (preClassification == "recommendation") ruleResult.recommendations--;
    else if (preClassification == "success") ruleResult.successes--;
    else if (preClassification == "notApplicable") { ruleResult.notApplicables--; }
    else if (preClassification == "aborted") { ruleResult.aborteds--; }
    else if (preClassification == "skip") { ruleResult.skip--; }

    return true;
}

function updateConformanceResults(classification, ruleResult) {
    if (classification == "error") ruleResult.errors++;
    else if (classification == "warning") ruleResult.warnings++;
    else if (classification == "recommendation") ruleResult.recommendations++;
    else if (classification == "success") ruleResult.successes++;
    else if (classification == "notApplicable") { ruleResult.notApplicables++; }
    else if (classification == "aborted") { ruleResult.aborteds++; }
    else if (classification == "skip") { ruleResult.skip++; }
}

function displayConformanceLevelResult() {
    alertJobComplete();

    $("#infobody").empty();
    
    var confTemplate = $('#conformanceLevelTemplate').tmpl(conformanceResults);
    jQuery(" > b", confTemplate).append(summarizeConformanceLevel());

    confTemplate.appendTo('#infobody');

    displayHide_conformanceResultPart();
    jQuery('.Minimal img', confTemplate)[0].onclick = evh_enableCollapse;
    jQuery('.InterMediate img', confTemplate)[0].onclick = evh_enableCollapse;
    jQuery('.Advanced img', confTemplate)[0].onclick = evh_enableCollapse;

    jQuery("#minimalRulesCount", confTemplate).text(updateConformanceLevelRules(conformanceResults.MinimalRulesResults));
    jQuery("#interRulesCount", confTemplate).text(updateConformanceLevelRules(conformanceResults.InterMediateRulesResults));
    jQuery("#advancedRulesCount", confTemplate).text(updateConformanceLevelRules(conformanceResults.AdvancedRulesResults));
    var arg_pack = { resultLine: "" }

    for (var i = 0; conformanceResults.MinimalRules[i]; i++) {
        i += createConformanceLevelRulesHtml(conformanceResults.MinimalRules, i, arg_pack);
        jQuery(".Minimal #display", confTemplate).append(arg_pack.resultLine);
    }

    for (var i = 0; conformanceResults.InterMediateRules[i]; i++) {
        i += createConformanceLevelRulesHtml(conformanceResults.InterMediateRules, i, arg_pack);
        jQuery(".InterMediate #display", confTemplate).append(arg_pack.resultLine);
    }

    for (var i = 0; conformanceResults.AdvancedRules[i]; i++) {
        i += createConformanceLevelRulesHtml(conformanceResults.AdvancedRules, i, arg_pack);
        jQuery(".Advanced #display", confTemplate).append(arg_pack.resultLine);
    }
    $("#infobody > div > div > button").text("hide details");
    $("#infobody > div > div > button").unbind().click(function () {
        if ($(this).text() == "show details") {
            $(this).text("hide details");
            $(this).next().find("li > button")
                .filter(function(){
                    return $(this).text()[0] == "+";
                })
                .trigger("click");
        } else {
            $(this).text("show details");
            $(this).next().find("li > button")
                .filter(function () {
                    return $(this).text()[0] == "-";
                }).trigger("click");
        }
    });
    $("#display > li > div p > i > b:nth-child(1)").unbind().click(function (e) {
        var resultDetailsID = this.parentNode.getAttribute("resultdetailsid");
        var testResultID = this.parentNode.getAttribute("testresultid");
        var index = findResultDetails(parseInt(testResultID), parseInt(resultDetailsID));
        currentDetail = conformanceResults.ResultDetails[index];
        window.open("showdetail.html?&rid=" + resultDetailsID);
    });

    $("#display > li > div  p > font").each(
        conformanceDisplayDecorate
    );
    
    
    createRecordLink();
}

function conformanceDisplayDecorate() {
    var pnode = document.createElement("p");
    var bigp = this.parentNode;
    var t1 = this, t2 = this;

    if (this.parentNode.childNodes.length <= 5) {
        $(this).prev().prev().children()[1].style.color = "red";
        return;
    }

    do {
        t1 = t1.previousSibling;
    } while (t1.nodeName != "I");

    t1.lastChild.style.color = "red";
    t2 = t1;
    do {
        t1 = t2;
        t2 = t1.nextSibling;
        pnode.appendChild(t1);
    } while (t1.nodeName != "FONT");

    while (t2 && t2.nodeName != "I") {
        t1 = t2.nextSibling;
        bigp.removeChild(t2);
        t2 = t1;
    }
    if (t2) bigp.insertBefore(pnode, t2);
    else bigp.appendChild(pnode);
}

function filterCheck(e) {
    var lists = document.querySelectorAll("#display > li  input[type='checkbox']");
    for (var i = 0; i < lists.length; i++) {
        if (lists[i].parentElement.style.display != "none")
            lists[i].checked = e.checked;
    }
}

function onChangeForCheck(ele) {
    checkChildren(ele);
    checkParent(ele);
}

function checkDiffLevelChildren(ele) {
    var rulename = $(ele).parent().attr('rulename');
    var subboxes = $(" > div input[type='checkbox']", $(ele).parent());
    if (subboxes.length == 0) {
        if (rulename == 'Intermediate.Conformance.1001') {
            subboxes = $("li[rulename^='Minimal.Conformance.'] > input[type='checkbox']");
        }
        else if (rulename == 'Advanced.Conformance.1001') {
            subboxes = $("li[rulename^='Intermediate.Conformance.'] > input[type='checkbox']");
        }
        else return subboxes;

        subboxes = subboxes.filter(function (index) {
            return $(this).parent().attr('rulename').split('.')[2].length == 4;
        });
    }

    return subboxes;
}

function checkChildren(ele) {
    var subboxes = checkDiffLevelChildren(ele);
    var notAccordanceSubBoxes = subboxes.filter(function (index) {
        return this.checked != ele.checked;
    });

    for (var i = 0; i < notAccordanceSubBoxes.length; i++) {
        notAccordanceSubBoxes[i].checked = ele.checked;
        checkChildren(notAccordanceSubBoxes[i]);
    }
}

function checkParent(ele) {
    var rulename = $(ele).parent().attr('rulename');
    var nameseg = rulename.split('.');
    var farnode = null;
    var selectedLevels = getSelectedConformanceLevel();

    if (nameseg[2].length == 6) {
        nameseg[2] = nameseg[2].slice(0, 4);
        farnode = $("li[rulename='" + nameseg.join('.') + "'] > input[type='checkbox']");
    } else if (nameseg[0] == "Minimal" && selectedLevels.indexOf("Intermediate") != -1) {
        farnode = $("li[rulename='Intermediate.Conformance.1001'] > input[type='checkbox']");
    } else if (nameseg[0] == "Intermediate" && selectedLevels.indexOf("Advanced") != -1) {
        farnode = $("li[rulename='Advanced.Conformance.1001'] > input[type='checkbox']");
    } else return;

    if (farnode[0].checked == ele.checked)
        return;

    if (ele.checked == true) {
        farnode[0].checked = ele.checked;
        checkParent(farnode[0]);
        return;
    }

    var subboxes = checkDiffLevelChildren(farnode[0]);
    var oppositeSiblingCheckBoxes = subboxes.filter(function (index) {
        return this.checked != ele.checked;
    });

    if (oppositeSiblingCheckBoxes.length != 0)
        return;

    farnode[0].checked = ele.checked;
    checkParent(farnode[0]);
}

function filterResult(ele) {
    var filterClassName = ele.value;
    var divnode = $(ele);
    
    var hasResult = false;
    var allSelected = true;
    while (true) {
        var divnode = $(divnode).next();
        if (!divnode.length)
            break;

        var lis = $(" ul > li ", divnode);
        for (var i = 0; i < lis.length; i++) {
            var li = lis[i];
            if (filterClassName == "all") {
                li.style.display = "block";
                allSelected = allSelected && $("input[type='checkbox']", li)[0].checked;
                hasResult = true;
                continue;
            }

            if (li.className === filterClassName || filterClassName.search(li.className) >= 0) {
                li.style.display = "block";
                allSelected = allSelected && $("input[type='checkbox']", li)[0].checked;
                hasResult = true;
            } else
                li.style.display = "none";
        }
    }

    ele.nextElementSibling.checked = allSelected && hasResult;
}

function isFatherResult(fatherResult, childredResult) {
    var i = childredResult.RuleName.search(fatherResult.RuleName);
    if (i == 0) {
        return childredResult.RuleName != fatherResult.RuleName;
    }
    return false;
}

function toggle(e) {
    if ($('div', e.parentNode)[0].style.display == "block") {
        $('div', e.parentNode)[0].style.display = "none";
    } else {
        $('div', e.parentNode)[0].style.display = "block";
    }

    $(e).text( $(e).text()[0] == "+"? "-details...":"+details..." );
}

function createConformanceLevelRulesHtml(testRules, i, t, checked) {
    var testResult = testRules[i];
    var children_count = 0;

    var findex = findResultDetails(testResult.ID);
    var hasSon = i + 1 < testRules.length && isFatherResult(testResult, testRules[i + 1]);

    t.resultLine = createTestResultHtmlULli(testResult, checked) + htmlEncode(testResult.Description) + " ";
    t.resultLine += "<button type='button' onclick='toggle(this);'>-details...</button>\
                     <div style='display: block;' class='subresult'>\
                         <p>";
    function myfunction(t, findex) {
        var detailcount = 0;
        var cr = conformanceResults.ResultDetails;
        while (findex + detailcount < cr.length
            && cr[findex + detailcount].TestResultID === testRules[i].ID)
            detailcount++;

        for (var n = 0; n < detailcount; n++) {
            var rdetails = cr[findex];
            if ((rdetails.HTTPMethod == ""
                || rdetails.HTTPMethod == null
                || rdetails.ResponseStatusCode == null
                || rdetails.URI == null
                || rdetails.ResponseStatusCode == "")
                && detailcount == 1) {
                if (t.resultLine.length && t.resultLine[t.resultLine.length - 1] == ">") {
                    t.resultLine = t.resultLine.slice(0, -1) + " style='color:red'>" + rdetails.ErrorMessage;
                }
                else {
                    t.resultLine += "<p> style='color:red'>" + rdetails.ErrorMessage + "</p>";
                }
                findex++;
                continue;
            }

            var I = document.createElement("I");
            I.setAttribute("testresultid", rdetails.TestResultID);
            I.setAttribute("resultdetailsid", rdetails.ID);
            var fb = document.createElement("b");
            var txt = document.createTextNode(" " + rdetails.URI + " ");
            var sb = document.createElement("b");
            if (rdetails.ErrorMessage)
                sb.setAttribute("color", "red");
            fb.textContent = rdetails.HTTPMethod;
            sb.textContent = "(" + rdetails.ResponseStatusCode + ")";
            I.appendChild(fb);
            I.appendChild(txt);
            I.appendChild(sb);
            t.resultLine += I.outerHTML + "<br>";
            if (rdetails.ErrorMessage) {
                var lf = document.createElement("font");
                lf.textContent = rdetails.ErrorMessage;
                lf.setAttribute("color", "red");
                t.resultLine += lf.outerHTML;
            }
            findex++;
        }
    }


    if (hasSon) {
        var SummaryDetail = conformanceResults.ResultDetails[findex];
        t.resultLine += SummaryDetail.ErrorMessage;
    } else {
        myfunction(t, findex);
    }
    t.resultLine += "</p>";

    var tt = { resultLine: "" };

    while (++i < testRules.length) {
        if (!isFatherResult(testResult, testRules[i])) {
            break;
        }
        tt.resultLine += createTestResultHtmlULli(testRules[i]);

        tt.resultLine += htmlEncode(testRules[i].Description) + " ";
        if (testRules[i].SpecificationUri && false) {
            tt.resultLine += "[" + createLinkOfNewWindow("Specification", validatorConf.ODataV4SpecificationUriForProtocol) + "] [Section: " + getSpecificationSection(testRules[i].SpecificationUri) + "]";
        }
        tt.resultLine += "<p class='subrulehttpinfo'>";

        var cindex = findResultDetails(testRules[i].ID);

        myfunction(tt, cindex);
        
        tt.resultLine += "</p></li>";
        children_count += 1;
    }

    if (tt.resultLine) t.resultLine += "<ul>" + tt.resultLine + "</ul>";
    t.resultLine += "</div></li>";
    return children_count;
}

function loadPayload(jobID) {
    alertPayloadInProgress();
    var query = "odatavalidator/ValidationJobs(guid'" + jobID + "')/PayloadLines?$orderby=LineNumber";
    OData.read(query, function (data) {
        $.each(data.results, function () {
            addNewSourceLine(this);
        });
        updateLoadedResultCount(0, true);
        loadResults(jobID, 0);
    });
}

function loadResults(jobID, totalResultsRetrieved) {
    alertResultsInProgress();
    var query = "odatavalidator/ValidationJobs(guid'" + jobID + "')/TestResults?$orderby=ID";
    if (totalResultsRetrieved > 0) { query += "&$skip=" + totalResultsRetrieved };

    OData.read(query, function (data) {
        $.each(data.results, function () {
            addNewResult(this);
            totalResultsRetrieved++;
        });
        updateLoadedResultCount(totalResultsRetrieved);
        loadMoreResults(jobID, totalResultsRetrieved);
    });
}

function loadMoreResults(jobID, totalResultsRetrieved) {
    if (totalResultsRetrieved < validatorApp.currentJob.RuleCount) {
        validatorStat.totalRequestsSent++;
        if (validatorStat.totalRequestsSent < validatorConf.maxRequests) {
            validatorApp.LoadResultsTimeoutObj = setTimeout(function () { loadResults(jobID, totalResultsRetrieved) }, validatorConf.perRequestDelay);
        } else {
            alertTimeout();
            enableTabSwitch();
        }
    } else {
        onJobEnd();
        startJob();
    }
}

//event handlers
function evh_enableCollapse(ev) {
    enableCollapse(ev.currentTarget.parentNode);
}

function enableCollapse(ta) {
    var timg = jQuery('>img', ta)[0];
    timg.src = (timg.src.slice(-14) != "arrow_down.gif") ? "Images/arrow_down.gif" : "Images/arrow_right.gif";
    jQuery('#display', ta).toggle();
    jQuery('>button', ta).toggle();
    jQuery('>input', ta).toggle();
    jQuery('>select', ta).toggle();
}

function expandView(ta) {
    jQuery('img', ta)[0].src = "Images/arrow_down.gif";
    jQuery('#display', ta).show();
    jQuery('>button', ta).show();
}

function collapseView(ta) {
    jQuery('img', ta)[0].src = "Images/arrow_right.gif";
    jQuery('#display', ta).hide();
    jQuery('>button', ta).hide();
}

// display housekeeping methods
function clearDisplay() {
    $('#infobody').empty();
    $('#statusInfo').empty();
    $('#recorddiv').empty();
}

function addNewSourceLine(line) {
    var srclist = jQuery('.sources #display', validatorApp.currJobDetail);
    srclist.append(createNewSourceLineHTML(line));
}

function addNewResult(testResult) {
    (validatorApp.currentJob.Id == testResult.ValidationJobId)
    {
        if (testResult.Classification == "error") validatorApp.results.errors++;
        else if (testResult.Classification == "warning") validatorApp.results.warnings++;
        else if (testResult.Classification == "recommendation") validatorApp.results.recommendations++;
        else if (testResult.Classification == "success") validatorApp.results.successes++;
        else if (testResult.Classification == "notApplicable") { validatorApp.results.notApplicables++; return; }
        else if (testResult.Classification == "aborted") { validatorApp.results.aborteds++; return; }
        else { return;}
        highlightSourceLine(testResult.LineNumberInError);
        jQuery(".results #display", validatorApp.currJobDetail).append(createTestResultHtml(testResult));
    }
}

function categorizedMessage(isFirst, count, nounce) {
    var str = "";
    if (count > 0) {
        str = isFirst ? " " : ", ";
        str += count + " " + nounce;
        if (count != 1) str += "s";
    }
    return str;
}

function getIssusText() {
    var s = categorizedMessage(true, validatorApp.results.errors, "error");
    s += categorizedMessage(!s, validatorApp.results.warnings, "warning");
    s += categorizedMessage(!s, validatorApp.results.recommendations, "recommendation");
    return s;
}

function updateResultCountMessage() {
    var applicableRules = validatorApp.currentJob.RuleCount - validatorApp.results.notApplicables;
    var executedRules = applicableRules - validatorApp.results.aborteds;
    var statusLine = "(" + executedRules + " of " + applicableRules + " rules executed."
    var resultCountMsg = getIssusText();

    if (resultCountMsg) {
        statusLine += " " + resultCountMsg + "."
    }

    statusLine += ")";
    jQuery("#resultsCount", validatorApp.currJobDetail).text(statusLine);
    if (resultCountMsg && validatorApp.currJobNote) {
        if (jQuery("#issueCount", validatorApp.currJobNote.parent())) {
            jQuery("#issueCount", validatorApp.currJobNote.parent()).text(resultCountMsg + ".");
        }
    }
}

function jobHasAnyIssue() {
    return validatorApp.results.errors > 0 || validatorApp.results.warnings > 0 || validatorApp.results.recommendations > 0;
}

function updateLoadedResultCount(resultsLoaded, onFirstLoad) {
    if (onFirstLoad) {
        displayStatusText("Submitting Validation Task...");
    } else {
        displayStatusText("Loaded " + resultsLoaded + " of " + validatorApp.currentJob.RuleCount + " validation results.");
    }
}

function summarize() {
    if (validatorApp.results.aborteds == 0 && validatorApp.results.errors == 0 && validatorApp.results.warnings == 0
        && validatorApp.results.successes == 0 && validatorApp.results.recommendations == 0) {
        // do nothing;
    }
    else {
        if (validatorApp.results.aborteds > 0) return "Some rules were not executed. Issue has been logged and will be investigated. You can see a list of all rules " + createLink("here", "roadmap.htm") + ".";
        if (validatorApp.results.errors == 0 && validatorApp.results.warnings == 0) {
            if (validatorApp.results.recommendations <= 0) {
                return "Congratulations! This is a valid OData endpoint. You can see a list of all rules " + createLink("here", "roadmap.htm#rules") + ".";
            } else {
                return "Congratulations! This is a valid OData endpoint, but interoperability with the largest number of OData clients may be improved by implementing the following recommendations."
                        + "You can see a list of all rules " + createLink("here", "roadmap.htm#rules") + ".";
            }
        }
    }
    return "You can see a list of all rules " + createLink("here", "roadmap.htm#rules") + ".";
}

function displayJobSummary() {
    jQuery("#completionMessage", validatorApp.currJobDetail).empty().append("<p>" + summarize() + "</p>");
    updateResultCountMessage();
}

function updateJobStatus(txt) {
    validatorApp.currJobNote.empty().text(txt);
}

function displayStatusText(txt) {
    validatorApp.currJobNote ? updateJobStatus(txt) : $('#simpleStatusInfoTmpl').tmpl({ 'status': txt }).appendTo('#infotop #statusInfo');
}

function highlightSourceLine(lineNumber) {
    if (lineNumber < 0) return;
    //$("#sourceLine" + lineNumber).css("background-color", "#FFEC8E");
    jQuery("#sourceLine" + lineNumber, validatorApp.currJobDetail).css("background-color", "#FFEC8E");
}

function createNewSourceLineHTML(line) {
    var resultLine = "<li class='sourceLine' id='sourceLine" + line.LineNumber + "'>";
    resultLine += createAnchor("pane" + validatorApp.panes + "line" + line.LineNumber);
    resultLine += htmlEncode(line.LineNumber + ": " + line.LineText);
    resultLine += "</li>";
    return resultLine;
}

function createTestResultHtml(testResult, checked) {
    var resultLine = createTestResultHtmlULli(testResult, checked);
    if (testResult.LineNumberInError && testResult.LineNumberInError >= 0)
        resultLine += "[" + createLink(testResult.LineNumberInError, "#pane" + validatorApp.panes + "line" + testResult.LineNumberInError) + "] ";
    resultLine += htmlEncode(testResult.Description) + " ";
    if (testResult.SpecificationUri) { resultLine += "[" + createLinkOfNewWindow("Specification", getSpecificationLinkUrl(testResult.SpecificationUri)) + "] [Section: " + getSpecificationSection(testResult.SpecificationUri) + "]"; }

    // Display v4 specification with section for v4 atom rules. 
    if ($('#atompub').is(':checked') && ($('#v4Compliance').is(':checked') || $('#v3Compliance').is(':checked'))) {
        var v4Specification = '';
        if (testResult.SpecificationUri.split(";").length == 3) {
            v4Specification = testResult.SpecificationUri.split(";")[2].split(":")[1];
            if (v4Specification != '') {
                resultLine += " | [" + createLinkOfNewWindow("V4Specification", getV4SpecificationLinkUrl(v4Specification)) + "] [Section: " + getV4AtomSpecificationSection(testResult.SpecificationUri) + "]";
            }
        }       
    }

    if (testResult.HelpUri) { resultLine += "[" + createLink("More Info", testResult.SpecificationUri) + "] "; }
       
    resultLine += "</li>";
    return resultLine;
}

function createTestResultHtmlULli(testResult, checked) {
    var className = getResultItemClass(testResult.Classification);
    checked = (typeof checked == "undefined") ? "" : "checked";
    var changestr = "onchange='onChangeForCheck(this);'";
    return "<li  class='" + className +
             "' testResultID='" + testResult.ID +
                "' rulename='" + testResult.RuleName + 
                   "'><span title='" + getHintItem(testResult.Classification) +
        "'></span> <input type='checkbox' " + changestr + " value='rerun'" + checked + ">";
}

function getHintItem(resultType) {
    /* if you want to change the return value, you could reference Site.css in UL LI.****Result_lte content*/
    if (resultType == "error") return "error";
    if (resultType == "warning") return "warning";
    if (resultType == "success") return "success";
    if (resultType == "recommendation") return "recommendation";
    if (resultType == "notApplicable") return "Inapplicable";
    if (resultType == "skip") return "skip";
    if (resultType == "aborted") return "aborted";
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function createReqHeaders() {
    var str = "";
    if ($('#v1_v2Compliance').is(':checked')) {
        str = "&Headers='" + 'DataServiceVersion:1.0;' + "'";
    }
    if ($('#v3Compliance').is(':checked')) {
        str = "&Headers='" + 'DataServiceVersion:3.0;' + "'";
    }
    if ($('#v4Compliance').is(':checked')) {
        str = "&Headers='" + 'OData-Version:4.0;' + "'";
    }

    return str;
}

function createMetadataReqHeaders() {
    var str = "";
    if ($('#v1_v2_Radio').is(':checked')) {
        str = "&Headers='" + 'DataServiceVersion:1.0;' + "'";
    }
    if ($('#v3_Radio').is(':checked')) {
        str = "&Headers='" + 'DataServiceVersion:3.0;' + "'";
    }
    if ($('#v4_Radio').is(':checked')) {
        str = "&Headers='" + 'OData-Version:4.0;' + "'";
    }

    return str;
}

function createAnchor(value) {
    return "<a name='" + value + "'></a>";
}

function createLink(description, uri) {
    return "<a href='" + uri + "'>" + description + "</a>";
}

function createLinkOfNewWindow(description, uri) {
    return "<a href='" + uri + "' target='_blank' >" + description + "</a>";
}

function getSpecificationLinkUrl(SpecificationUri) {
    var sectionUri = SpecificationUri;
    var version = sectionUri.split(";")[0].split(":")[1];
    var fragment = "undefined";
    var chapterTag = sectionUri.split(";")[1];

    function findProperTag(URLFragments, chapterTag) {
        var fragment = "";
        do {
            fragment = URLFragments[chapterTag];
            chapterTag = chapterTag.substr(0, chapterTag.lastIndexOf('.'));
        } while (typeof fragment == "undefined" && chapterTag.length);

        return fragment;
    }

    if ($('#jsonmin').is(':checked') || $('#jsonfull').is(':checked')) {
        if (version == "V3") {
            fragment = findProperTag(validatorConf.V34JsonURLFragments, chapterTag);
            return validatorConf.ODataV3SpecificationUriForJson + "#" + fragment;
        } else if (version == "V4" || version == "V3_V4") {
            fragment = findProperTag(validatorConf.V34JsonURLFragments, chapterTag);
            return validatorConf.ODataV4SpecificationUriForJson + "#" + fragment;
        }
    } else if ($('#v4Compliance').is(':checked')) {
        if (version == "V4" || version == "V3_V4") {
            fragment = findProperTag(validatorConf.V4AtomURLFragments, chapterTag);
            return validatorConf.ODataV4SpecificationUriForAtom + "#" + fragment;
        }
    }

    return validatorConf.SpecificationUri;
}

function getV4SpecificationLinkUrl(V4Specification) {
    var v4SpecificationLinkUrl = '';

    if (V4Specification == "odataprotocol") {
        v4SpecificationLinkUrl = validatorConf.ODataV4SpecificationUriForProtocol;
    } else if (V4Specification == "odataurl") {
        v4SpecificationLinkUrl = validatorConf.ODataV4SpecificationUriForURL;
    } else if (V4Specification == "odatacsdl") {
        v4SpecificationLinkUrl = validatorConf.ODataV4SpecificationUriForCSDL;
    } else if (V4Specification == "odatajson") {
        v4SpecificationLinkUrl = validatorConf.ODataV4SpecificationUriForJson;
    } else if (V4Specification == "odataatom") {
        v4SpecificationLinkUrl = validatorConf.ODataV4SpecificationUriForAtom;
    } else {
        v4SpecificationLinkUrl = '';
    }
        
    return v4SpecificationLinkUrl;
}

function getSpecificationSection(SpecificationUri) {
    var sectionUri = SpecificationUri.split(";")[1];

    if ($.inArray("&", sectionUri) != -1) {
        if ($('#v4Compliance').is(':checked') || $('#v3Compliance').is(':checked')) {
            if ($('#atompub').is(':checked')) {
                return sectionUri.split("&")[1].split(":")[1];
            } else {
                return sectionUri.split("&")[0].split(":")[1];
            }            
        } else {
            return sectionUri.split("&")[1].split(":")[1];
        }
    }

    return sectionUri;
}

function getV4AtomSpecificationSection(SpecificationUri) {
    var sectionUri = SpecificationUri.split(";")[1];
    var v4Section = '';

    if ($.inArray("&", sectionUri) != -1) {
        if (($('#v4Compliance').is(':checked') || $('#v3Compliance').is(':checked')) && $('#atompub').is(':checked')) {
            v4Section = sectionUri.split("&")[0].split(":")[1];
        }
    }

    return v4Section;
}

function getSelectedFormat() {

    // if the string end with substring.
    if (typeof String.prototype.endsWith != 'function') {
        String.prototype.endsWith = function (str){
            return this.slice(-str.length) == str;
        };
    }

    if (getServiceToValidateUri().endsWith("/$metadata")) {
        return "atompub";
    }

    return $('input[name=formatGroup]:checked').val();
}

function getSelectedMetadata() {
    var str = "";

    // if the string end with substring.
    if (typeof String.prototype.endsWith != 'function') {
        String.prototype.endsWith = function (str) {
            return this.slice(-str.length) == str;
        };
    }

    if (getServiceToValidateUri().endsWith("/$metadata")) {
        return "";
    }

    if ($('#v3Compliance').is(':checked')) {
        if ($('#json').is(':checked')) {
            str = ";odata=verbose"
        }
        else if ($('#jsonmin').is(':checked')) {
            str = ";odata=minimalmetadata";
        }
        else if ($('#jsonfull').is(':checked')) {
            str = ";odata=fullmetadata";
        }
    }
    else if ($('#v4Compliance').is(':checked')) {
        if ($('#jsonmin').is(':checked')) {
            str = ";odata.metadata=minimal";
        }
        else if ($('#jsonfull').is(':checked')) {
            str = ";odata.metadata=full";
        }
    }
    else {
        str = "";
    }
    return str;
}

function getSelectedConformanceLevel() {

    return $('input[name=levelGroup]:checked').val();
}

function getSelectedResourceType() {
    if ($('#readWrite').is(':checked')) {
        return "ReadWrite";
    }

    if ($('#readOnly').is(':checked')) {
        return "ReadOnly";
    }
}

function getServiceMetadataToValidateUri() {
    var url = $("#serviceUri").val();

    if (url.indexOf("?$format=") > 0) {
        url = url.slice(0, url.lastIndexOf("?$format="))
    } else if (url.indexOf("&$format=") > 0) {
        url = url.slice(0, url.lastIndexOf("&$format="))
    }

    // if the string end with substring.
    if (typeof String.prototype.endsWith != 'function') {
        String.prototype.endsWith = function (str) {
            return this.slice(-str.length) == str;
        };
    }

    if (url.toString().endsWith("/$metadata") == false) {
        url += "/$metadata";
    }

    return url;
}

function getServiceToValidateUri() {
    var url = $("#odataUri").val();

    if (url.indexOf("?$format=") > 0) {
        url = url.slice(0, url.lastIndexOf("?$format="))
    } else if (url.indexOf("&$format=") > 0) {
        url = url.slice(0, url.lastIndexOf("&$format="))
    }     

    return url;
}

function getPayloadText() {
    if ($("#payload_text").val() == validatorConf.payload_text_help) { return ''; }
    return $("#payload_text").val();
}

function getMetadataText() {
    if ($("#meta_text").val() == validatorConf.meta_text_help) { return ''; }
    return $("#meta_text").val();
}

function getReqHeaders() {
    var header = '';
    
    var version = $('input[name=odataversion]:checked').val();
    if (!version) {
        header = '';
    } else {
        header = 'DataServiceVersion: ' + version + '; \r\n';

        if ($('#offlineMin').is(':checked')) {
            if ($('#offlineV3').is(':checked')) {
                header += "application/json;odata=minimalmetadata"
            } else if ($('#offlineV4').is(':checked')) {
                header += "application/json;odata.metadata=minimal"
            }
        } else if ($('#offlineFull').is(':checked')) {
            if ($('#offlineV3').is(':checked')) {
                header += "application/json;odata=fullmetadata"
            } else if ($('#offlineV4').is(':checked')) {
                header += "application/json;odata.metadata=full"
            }
        }
    }
    
    return header;
}

function getResultItemClass(resultType) {
    if (resultType == "error") return "errorResult";
    if (resultType == "warning") return "warningResult";
    if (resultType == "success") return "successResult";
    if (resultType == "recommendation") return "recommendationResult";
    if (resultType == "notApplicable") return "notApplicableResult";
    if (resultType == "skip") return "skipResult";
    if (resultType == "aborted") return "abortedResult";
}

function toTypeDesc(type) {
    if (type == "None") return "none";
    if (type == "ServiceDoc") return "Service Document";
    if (type == "Metadata") return "Metadata Document";
    if (type == "Feed") return "OData Feed";
    if (type == "Entry") return "OData Entry";
    if (type == "Error") return "Error Payload";
    return "other";
}
