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

        // Remove the 'active' class from the active tab.
        $('#tabs_container li.active').removeClass('active');

        // Add the 'active' class to the clicked tab.
        $(this).parent().addClass('active');

        // Remove the 'tab_contents_active' class from the visible tab contents.
        $('.tab_contents_container div.tab_contents_active').removeClass('tab_contents_active');

        // Add the 'tab_contents_active' class to the associated tab contents.
        $(this.rel).addClass('tab_contents_active');

        // clear the result and resource areas
    });

});

function disableTabSwitch() {
    $('#tabs_container').addClass('busy');
    $("#liveValidateButton").attr("disabled", true);
    $("#offlineValidateButton").attr("disabled", true);
};

function enableTabSwitch() {
    $('#tabs_container').removeClass('busy');
    $("#liveValidateButton").attr("disabled", false);
    $("#offlineValidateButton").attr("disabled", false);
};


function createShiftArr(step) {

    var space = '    ';

    if (isNaN(parseInt(step))) {  // argument is string
        space = step;
    } else { // argument is integer
        switch (step) {
            case 1: space = ' '; break;
            case 2: space = '  '; break;
            case 3: space = '   '; break;
            case 4: space = '    '; break;
            case 5: space = '     '; break;
            case 6: space = '      '; break;
            case 7: space = '       '; break;
            case 8: space = '        '; break;
            case 9: space = '         '; break;
            case 10: space = '          '; break;
            case 11: space = '           '; break;
            case 12: space = '            '; break;
        }
    }
    var shift = ['\n']; // array of shifts
    for (ix = 0; ix < 100; ix++) {
        shift.push(shift[ix] + space);
    }
    return shift;
}
function formatXML(text, step) {
    var ar = text.replace(/>\s{0,}</g, "><")
                    .replace(/</g, "~::~<")
                    .replace(/xmlns\:/g, "~::~xmlns:")
                    .replace(/xmlns\=/g, "~::~xmlns=")
                    .split('~::~'),
        len = ar.length,
        inComment = false,
        deep = 0,
        str = '',
        ix = 0,
        shift = step ? createShiftArr(step) : this.shift;

    for (ix = 0; ix < len; ix++) {
        // start comment or <![CDATA[...]]> or <!DOCTYPE //
        if (ar[ix].search(/<!/) > -1) {
            str += shift[deep] + ar[ix];
            inComment = true;
            // end comment  or <![CDATA[...]]> //
            if (ar[ix].search(/-->/) > -1 || ar[ix].search(/\]>/) > -1 || ar[ix].search(/!DOCTYPE/) > -1) {
                inComment = false;
            }
        } else
            // end comment  or <![CDATA[...]]> //
            if (ar[ix].search(/-->/) > -1 || ar[ix].search(/\]>/) > -1) {
                str += ar[ix];
                inComment = false;
            } else
                // <elm></elm> //
                if (/^<\w/.exec(ar[ix - 1]) && /^<\/\w/.exec(ar[ix]) &&
                    /^<[\w:\-\.\,]+/.exec(ar[ix - 1]) == /^<\/[\w:\-\.\,]+/.exec(ar[ix])[0].replace('/', '')) {
                    str += ar[ix];
                    if (!inComment) deep--;
                } else
                    // <elm> //
                    if (ar[ix].search(/<\w/) > -1 && ar[ix].search(/<\//) == -1 && ar[ix].search(/\/>/) == -1) {
                        str = !inComment ? str += shift[deep++] + ar[ix] : str += ar[ix];
                    } else
                        // <elm>...</elm> //
                        if (ar[ix].search(/<\w/) > -1 && ar[ix].search(/<\//) > -1) {
                            str = !inComment ? str += shift[deep] + ar[ix] : str += ar[ix];
                        } else
                            // </elm> //
                            if (ar[ix].search(/<\//) > -1) {
                                str = !inComment ? str += shift[--deep] + ar[ix] : str += ar[ix];
                            } else
                                // <elm/> //
                                if (ar[ix].search(/\/>/) > -1) {
                                    str = !inComment ? str += shift[deep] + ar[ix] : str += ar[ix];
                                } else
                                    // <? xml ... ?> //
                                    if (ar[ix].search(/<\?/) > -1) {
                                        str += shift[deep] + ar[ix];
                                    } else
                                        // xmlns //
                                        if (ar[ix].search(/xmlns\:/) > -1 || ar[ix].search(/xmlns\=/) > -1) {
                                            str += shift[deep] + ar[ix];
                                        }

                                        else {
                                            str += ar[ix];
                                        }
    }

    return (str[0] == '\n') ? str.slice(1) : str;
}
//secondpre.firstElementChild.firstChild.data = JSON.stringify(JSON.parse(result.ResponsePayload), null, 4)
function formatPayload(ele) {
    var pre = $(ele).nextAll("pre")[0].firstChild;
    var i = 0;
    while (i < pre.data.length && " \f\n\r\t\v".indexOf( pre.data[i] )!=-1 ) i++;
    if (i == pre.data.length) {
        alert("payload have only space charactor or is empty");
    } else if (pre.data[i] == "<") {
        var text = formatXML(pre.data, 4);
        pre.data = text;
    }
    else {
        pre.parentNode.innerHTML = syntaxHighlight(JSON.parse(pre.data));
    }
    ele.disabled = true;
    //else pre.data = JSON.stringify(JSON.parse(pre.data), null, 3);
}

function syntaxHighlight(json) {
    if (typeof json != 'string') {
        json = JSON.stringify(json, undefined, 2);
    }
    json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
        var cls = 'number';
        if (/^"/.test(match)) {
            if (/:$/.test(match)) {
                cls = 'key';
            } else {
                cls = 'string';
            }
        } else if (/true|false/.test(match)) {
            cls = 'boolean';
        } else if (/null/.test(match)) {
            cls = 'null';
        }
        return '<span class="' + cls + '">' + match + '</span>';
    });
}

function clickToCopy(ele) {
    var doc = document
        , element = $(ele).nextAll("pre")[0]
        , range, selection;
    if (doc.body.createTextRange) {
        range = document.body.createTextRange();
        range.moveToElementText(element);
        range.select();
    } else if (window.getSelection) {
        selection = window.getSelection();
        range = document.createRange();
        range.selectNodeContents(element);
        selection.removeAllRanges();
        selection.addRange(range);
    }

    text = element.textContent;

    if (window.clipboardData && clipboardData.setData) {
        if (text.indexOf("\r\n") == -1 && text.indexOf("\n") != -1) {
            text = text.replace(/\n/g, "\r\n");
        }
        clipboardData.setData("Text", text);
    }
}


$(document).ready(function () {
    /*if (window.clipboardData && clipboardData.setData) {
        clipboardData.setData("Text", text);
    }*/
    if (window.clipboardData && clipboardData.setData) {
        ;//clipboardData.setData("Text", text);
    } else {
        $("#tab_1_contents > fieldset:nth-child(2) > button:nth-child(5)").zclip({
            path: "Scripts/ZeroClipboard.swf",
            copy: function () {
                var text = $(this).nextAll("pre").text();
                if (text.indexOf("\r\n") == -1 && text.indexOf("\n") != -1) {
                    text = text.replace(/\n/g, "\r\n");
                }
                return text;
            }
        });
    }
});