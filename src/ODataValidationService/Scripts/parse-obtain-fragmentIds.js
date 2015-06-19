// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

/*we run this code in chrome snippet code or console to get the every chapter's fragment */

function getFirstConsecutiveNumbers(str) {
    var s = str.length, e;
    for (var i = 0; i < str.length; i++) {
        if (/^\d$/.test(str[i])) {
            s = i;
            break;
        }
    }
    for (e = 0; s < str.length; e++, i++) {
        if (/^\d$/.test(str[i]) || str[i] == '.');
        else break;
    }
    return str.substr(s, e);
}

var res = {};

var fragmentArray = [];

var selectpatharray = [
      "body > div > h3",
      "body > div > h2",
      "body > div > div > h1"
];

for (var j = 0 ; j < selectpatharray.length ; j++) {
    fragmentArray = document.querySelectorAll(selectpatharray[j]);

    for (var i = 0; i < fragmentArray.length; i++) {
        var chapterIndex = getFirstConsecutiveNumbers(fragmentArray[i].innerText);
        var lasta = fragmentArray[i].querySelector("a[name]:last-of-type");
        if (0 && lasta && lasta.name && lasta.name.substr(0, 4) != "_Toc") {
            console.log(fragmentArray[i]);
            alert(lasta.name);
        }

        if (lasta != null)
            res[chapterIndex] = lasta.name;
    }
}

var resstr = "";

for (var cindex in res) {
    var b = "\"" + cindex + "\": " + "\"" + res[cindex] + "\",";
    console.assert(/(\d+)(.\d+)*/.test(cindex));
    resstr += b;
}
str = "{\"\" : \"\", " + resstr.substr(0, resstr.length - 1) + "}";
console.log("%c%s", "color:red;font-size:10px;", str);
var a = JSON.parse(str);

url = window.location.origin + window.location.pathname + "#";


//check result   //you can see the automatic test proccess
for (var index in a) {
    window.open(url + a[index], "_self");
    alert("arrived chapter : " + index);
}
