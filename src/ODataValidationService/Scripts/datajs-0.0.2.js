// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// odata.js 

(function (window, undefined) {
    if (!window.OData) {
        window.OData = {};
    }

    var odata = window.OData;

    var prepareRequest = function (request, handler, context) {
        /// <summary>Prepares a request object so that it can be sent through the network.</summary>
        /// <param name="request">Object that represents the request to be sent.</param>
        /// <param name="handler">Handler for data serialization</param>
        /// <param name="context">Context used for preparing the request</param>

        // Default to GET if no method has been specified.    
        if (!request.method) {
            request.method = "GET";
        }

        if (!request.headers) {
            request.headers = {};
        }

        if (request.headers.Accept === undefined) {
            request.headers.Accept = handler.accept;
        }

        if (request.data && request.body === undefined) {
            handler.write(request, context);
        }
    };

    var invokeRequest = function (request, success, error, handler, httpClient, context) {
        /// <summary>Sends a request containing OData payload to a server.</summary>
        /// <param name="request">Object that represents the request to be sent..</param>
        /// <param name="success">Callback for a successful read operation.</param>
        /// <param name="error">Callback for handling errors.</param>
        /// <param name="handler">Handler for data serialization.</param>
        /// <param name="httpClient">HTTP client layer.</param>
        /// <param name="context">Context used for processing the request</param>
        httpClient.request(request, function (response) {
            try {
                if (response.data === undefined) {
                    handler.read(response, context);
                }

                success(response.data, response);
            } catch (err) {
                if (err.request === undefined) {
                    err.request = request;
                }
                if (err.response === undefined) {
                    err.response = response;
                }

                error(err);
            }
        }, error);
    };


    // Provides an enumeration of possible kinds of payloads.
    var payloadType = {
        feed: "feed",
        entry: "entry",
        svcDoc: "service document",
        complexType: "complex",
        primitiveType: "primitive",
        batch: "batch",
        unknown: "unknown",
        none: "none"
    };

    // Provides an enumeration of possible kinds of properties.
    var propertyKind = {
        complex: "complex",
        deferred: "deferred",
        inline: "inline",
        primitive: "primitive",
        none: "none"
    };

    var contains = function (arr, item) {
        /// <summary>Checks whether the specified item is in the array.</summary>
        /// <param name="arr" type="Array" optional="false" mayBeNull="false">Array to check in.</param>
        /// <param name="item">Item to look for.</param>
        /// <returns type="Boolean">true if the item is contained, false otherwise.</returns>

        var i, len;
        for (i = 0, len = arr.length; i < len; i++) {
            if (arr[i] === item) {
                return true;
            }
        }

        return false;
    };

    var isDateTimeOffset = function (value) {
        /// <summary>Checks whether a Date object is DateTimeOffset value</summary>
        /// <param name="value" type="Date" mayBeNull="false">Value to check.</param>
        /// <returns type="Boolean">true if the value is a DateTimeOffset, false otherwise.
        return (value.__edmType === "Edm.DateTimeOffset" || (!value.__edmType && value.__offset));
    };

    var formatDateTimeOffset = function (value) {
        /// <summary>Formats a DateTime or DateTimeOffset value a string.</summary>
        /// <param name="value" type="Date" mayBeNull="false">Value to format.</param>
        /// <returns type="String">Formatted text.</returns>

        var hasOffset = isDateTimeOffset(value);
        var offset = getCanonicalTimezone(value.__offset);
        if (hasOffset && offset !== "Z") {
            // We're about to change the value, so make a copy.
            value = new Date(value.valueOf());

            var timezone = parseTimezone(offset);
            var hours = value.getUTCHours() + (timezone.d * timezone.h);
            var minutes = value.getMinutes() + (timezone.d * timezone.m);

            value.setUTCHours(hours, minutes);
        } else if (!hasOffset) {
            // Don't suffix a 'Z' for Edm.DateTime values.
            offset = "";
        }

        var year = value.getUTCFullYear();
        var month = value.getUTCMonth() + 1;
        var sign = "";
        if (year <= 0) {
            year = -(year - 1);
            sign = "-";
        }

        // Avoid generating milliseconds if not necessary.
        var ms = value.getUTCMilliseconds();
        if (ms === 0) {
            ms = "";
        } else {
            ms = "." + formatNumberWidth(ms.toString(), 3);
        }

        return sign +
            formatNumberWidth(year, 4) + "-" +
            formatNumberWidth(month, 2) + "-" +
            formatNumberWidth(value.getUTCDate(), 2) + "T" +
            formatNumberWidth(value.getUTCHours(), 2) + ":" +
            formatNumberWidth(value.getUTCMinutes(), 2) + ":" +
            formatNumberWidth(value.getUTCSeconds(), 2) +
            ms + offset;
    };

    var formatNumberWidth = function (value, width) {
        /// <summary>Formats the specified value to the given width.</summary>
        /// <param name="value" type="Number">Number to format (non-negative).</param>
        /// <param name="width" type="Number">Minimum width for number.</param>
        /// <returns type="String">Text representation.</returns>
        var result = value.toString(10);
        while (result.length < width) {
            result = "0" + result;
        }

        return result;
    };

    var getCanonicalTimezone = function (timezone) {
        /// <summary>Gets the canonical timezone representation.</summary>
        /// <param name="timezone" type="String">Timezone representation.</param>
        /// <returns type="String">An 'Z' string if the timezone is absent or 0; the timezone otherwise.</returns>

        return (!timezone || timezone === "Z" || timezone === "+00:00" || timezone === "-00:00") ? "Z" : timezone;
    };

    var isArray = function (value) {
        /// <summary>Checks whether the specified value is an array object.</summary>
        /// <param name="value">Value to check.</param>
        /// <returns type="Boolean">true if the value is an array object; false otherwise.</returns>

        return Object.prototype.toString.call(value) === "[object Array]";
    };

    var isDate = function (value) {
        /// <summary>Checks whether the specified value is a Date object.</summary>
        /// <param name="value">Value to check.</param>
        /// <returns type="Boolean">true if the value is a Date object; false otherwise.</returns>

        return Object.prototype.toString.call(value) === "[object Date]";
    };

    var lookupTypeInMetadata = function (name, metadata, kind) {
        /// <summary>Looks up a type object by name.</summary>
        /// <param name="name" type="String">Name, possibly null or empty.</param>
        /// <param name="metadata">Metadata store; one of edmx, schema, or an array of any of them.</param>
        /// <param name="kind" type="String">Kind of type to look for; one of 'entityType' or 'complexType'.</param>
        /// <returns>An type description if the name is found; null otherwise.</returns>

        if (!name || !metadata) {
            return null;
        }

        if (isArray(metadata)) {
            var i, len, result;
            for (i = 0, len = metadata.length; i < len; i++) {
                result = lookupTypeInSchema(name, metadata[i], kind);
                if (result) {
                    return result;
                }
            }
        } else {
            return lookupTypeInSchema(name, metadata, kind);
        }
    };

    var lookupComplexType = function (name, metadata) {
        /// <summary>Looks up a complex type object by name.</summary>
        /// <param name="name" type="String">Name, possibly null or empty.</param>
        /// <param name="metadata">Metadata store; one of edmx, schema, or an array of any of them.</param>
        /// <returns>A complex type description if the name is found; null otherwise.</returns>

        return lookupTypeInMetadata(name, metadata, "complexType");
    };

    var lookupEntityType = function (name, metadata) {
        /// <summary>Looks up an entity type object by name.</summary>
        /// <param name="name" type="String">Name, possibly null or empty.</param>
        /// <param name="metadata">Metadata store; one of edmx, schema, or an array of any of them.</param>
        /// <returns>An entity type description if the name is found; null otherwise.</returns>

        return lookupTypeInMetadata(name, metadata, "entityType");
    };

    var lookupTypeInSchema = function (name, metadata, kind) {
        /// <summary>Looks up an entity type object by name.</summary>
        /// <param name="name" type="String">Name (assigned).</param>
        /// <param name="metadata">Metadata store; one of edmx, schema.</param>
        /// <param name="kind" type="String">Kind of type to look for; one of 'entityType' or 'complexType'.</param>
        /// <returns>An entity type description if the name is found; null otherwise.</returns>
        /// <remarks>
        /// metadata is considered an edmx object if it contains a dataServices object.
        /// </remarks>

        if (!metadata) {
            return null;
        }

        var i, len;
        if (metadata.dataServices) {
            var schema, result;
            schema = metadata.dataServices.schema;
            for (i = 0, len = schema.length; i < len; i++) {
                result = lookupTypeInSchema(name, schema[i], kind);
                if (result) {
                    return result;
                }
            }
        } else {
            var ns = metadata["namespace"];

            // The name should be the namespace qualified name in 'ns'.'type' format.
            if (name.indexOf(ns) !== 0) {
                return null;
            }
            if (name.charAt(ns.length) !== ".") {
                return null;
            }

            var nameOnly = name.substr(ns.length + 1);
            var types = metadata[kind];
            if (types) {
                for (i = 0, len = types.length; i < len; i++) {
                    if (types[i].name === nameOnly) {
                        return types[i];
                    }
                }
            }
        }

        return null;
    };

    var parseTimezone = function (timezone) {
        /// <summary>Parses a timezone description in (+|-)nn:nn format.</summary>
        /// <param name="timezone" type="String">Timezone offset.</param>
        /// <returns type="Object">
        /// An object with a (d)irection property of 1 for + and -1 for -,
        /// offset (h)ours and offset (m)inutes.
        /// </returns>

        var direction = timezone.substring(0, 1);
        direction = (direction === "+") ? 1 : -1;

        var offsetHours = parseInt(timezone.substring(1), 10);
        var offsetMinutes = parseInt(timezone.substring(timezone.indexOf(":") + 1), 10);
        return { d: direction, h: offsetHours, m: offsetMinutes };
    };

    var payloadTypeOf = function (data) {
        /// <summary>Determines the kind of payload applicable for the specified value.</summary>
        /// <param name="value">Value to check.</param>
        /// <returns type="String">One of the values declared on the payloadType object.</returns>

        switch (typeof (data)) {
            case "object":
                if (!data) {
                    return payloadType.none;
                }
                if (isArray(data) || isArray(data.results)) {
                    return payloadType.feed;
                }
                if (data.__metadata && data.__metadata.uri !== undefined) {
                    return payloadType.entry;
                }
                if (isArray(data.EntitySets)) {
                    return payloadType.svcDoc;
                }
                if (isArray(data.__batchRequests)) {
                    return payloadType.batch;
                }
                if (isDate(data)) {
                    return payloadType.primitiveType;
                }

                return payloadType.complexType;

            case "string":
            case "number":
            case "boolean":
                return payloadType.primitiveType;

            default:
                return payloadType.unknown;
        }
    };

    var propertyKindOf = function (value) {
        /// <summary>Determines the kind of property for the specified value.</summary>
        /// <param name="value">Value to check.</param>
        /// <returns type="String">One of the values declared on the propertyKind object.</returns>

        switch (payloadTypeOf(value)) {
            case payloadType.complexType:
                if (value.__deferred && value.__deferred.uri) {
                    return propertyKind.deferred;
                }

                return propertyKind.complex;

            case payloadType.feed:
            case payloadType.entry:
                return propertyKind.inline;

            case payloadType.primitiveType:
                return propertyKind.primitive;

            default:
                return propertyKind.none;
        }
    };

    var trimString = function (str) {
        /// <summary>Removes leading and trailing whitespaces from a string.</summary>
        /// <param name="str" type="String" optional="false" mayBeNull="false">String to trim</param>
        /// <returns type="String">The string with no leading or trailing whitespace.</returns>

        if (str.trim) {
            return str.trim();
        }

        return str.replace(/^\s+|\s+$/g, '');
    };


    var ticks = 0;

    var canUseJSONP = function (request) {
        /// <summary>
        /// Checks whether the specified request can be satisfied with a JSONP request.
        /// </summary>
        /// <param name="request">Request object to check.</param>
        /// <returns type="Boolean">true if the request can be satisfied; false otherwise.</returns>

        // Requests that 'degrade' without changing their meaning by going through JSONP
        // are considered usable.
        //
        // We allow data to come in a different format, as the servers SHOULD honor the Accept
        // request but may in practice return content with a different MIME type.
        if (request.method && request.method !== "GET") {
            return false;
        }

        return true;
    };

    var isAbsoluteUrl = function (url) {
        /// <summary>Checks whether the specified URL is an absolute URL.</summary>
        /// <param name="url" type="String">URL to check.</param>
        /// <returns type="Boolean">true if the url is an absolute URL; false otherwise.</returns>

        return url.indexOf("http://") === 0 ||
            url.indexOf("https://") === 0 ||
            url.indexOf("file://") === 0;
    };

    var isLocalUrl = function (url) {
        /// <summary>Checks whether the specified URL is local to the current context.</summary>
        /// <param name="url" type="String">URL to check.</param>
        /// <returns type="Boolean">true if the url is a local URL; false otherwise.</returns>

        if (!isAbsoluteUrl(url)) {
            return true;
        }

        // TODO: need to consider default ports, username and password (but error makes fallback to origin, which is benign)
        // assert: colon > 0, because isAbsoluteUrl is true.
        var locationDomain = window.location.protocol + "//" + window.location.host + "/";
        if (url.indexOf(locationDomain) === 0) {
            return true;
        } else {
            return false;
        }
    };

    var removeCallback = function (tick, name) {
        /// <summary>Removes a callback used for a JSONP request.</summary>
        /// <param name="tick" type="Number">Tick count used on the callback.</param>
        /// <param name="name" type="String">Function name to remove.</param>

        try {
            delete window[name];
        } catch (err) {
            window[name] = undefined;
            if (tick === ticks - 1) {
                ticks -= 1;
            }
        }
    };

    var readResponseHeaders = function (xhr, headers) {
        /// <summary>Reads response headers into array.</summary>
        /// <param name="xhr" type="XMLHttpRequest">HTTP request with response available.</param>
        /// <param name="headers" type="Array">Target array to fill with name/value pairs.</param>

        var responseHeaders = xhr.getAllResponseHeaders().split(/\r?\n/);
        var i, len;
        for (i = 0, len = responseHeaders.length; i < len; i++) {
            if (responseHeaders[i]) {
                var header = responseHeaders[i].split(": ");
                headers[header[0]] = header[1];
            }
        }
    };

    odata.defaultHttpClient = {
        callbackParameterName: "$callback",

        formatQueryString: "$format=json",

        enableJsonpCallback: false,

        request: function (request, success, error) {
            /// <summary>Performs a network request.</summary>
            /// <param name="success" type="Function">Success callback with the response object.</param>
            /// <param name="error" type="Function">Error callback with an error object.</param>
            /// <returns type="Object">Object with an 'abort' method for the operation.</returns>

            var result = {};
            var xhr = null;
            var aborted = false;
            result.abort = function () {
                if (!xhr) {
                    return;
                }

                aborted = true;
                xhr.abort();
                xhr = null;
                error({ message: "Request aborted" });
            };

            var name;
            var url = request.requestUri;
            if (!this.enableJsonpCallback || isLocalUrl(url)) {
                xhr = new XMLHttpRequest();

                xhr.onreadystatechange = function () {
                    if (xhr === null || xhr.readyState !== 4) {
                        return;
                    }

                    // Workaround for XHR behavior on IE.
                    var statusText = xhr.statusText;
                    var statusCode = xhr.status;
                    if (statusCode === 1223) {
                        statusCode = 204;
                        statusText = "No Content";
                    }

                    var headers = [];
                    readResponseHeaders(xhr, headers);

                    var response = { requestUri: url, statusCode: statusCode, statusText: statusText, headers: headers, body: xhr.responseText };

                    xhr = null;
                    if (statusCode >= 200 && statusCode <= 299) {
                        success(response);
                    } else {
                        error({ message: "HTTP request failed", request: request, response: response });
                    }
                };

                xhr.open(request.method || "GET", url, true);

                // Set the name/value pairs.
                if (request.headers) {
                    for (name in request.headers) {
                        xhr.setRequestHeader(name, request.headers[name]);
                    }
                }

                // Set the timeout if available.
                if (request.timeoutMS) {
                    xhr.timeout = request.timeoutMS;
                    xhr.ontimeout = function () {
                        if (xhr) {
                            xhr = null;
                            error({ message: "Request timed out" });
                        }
                    };
                }

                xhr.send(request.body);
            } else {
                if (!canUseJSONP(request)) {
                    throw { message: "Request is not local and cannot be done through JSONP." };
                }

                var tick = ticks;
                ticks += 1;
                var tickText = tick.toString();
                name = "handleJSONP_" + tickText;
                window[name] = function (data) {
                    removeCallback(name, tick);
                    success({ body: data, statusCode: 200, headers: { "Content-Type": "application/json"} });
                };

                var scriptTag = document.createElement("SCRIPT");
                scriptTag.setAttribute("type", "text/javascript");

                var queryStringParams = this.callbackParameterName + "=" + name;
                if (this.formatQueryString) {
                    queryStringParams += "&" + this.formatQueryString;
                }

                var qIndex = url.indexOf("?");
                if (qIndex === -1) {
                    url = url + "?" + queryStringParams;
                } else if (qIndex === url.length - 1) {
                    url = url + queryStringParams;
                } else {
                    url = url + "&" + queryStringParams;
                }

                scriptTag.setAttribute("src", url);

                // Insert SCRIPT element and help the GC in case these are
                // referenced by the abort closure.
                var head = document.getElementsByTagName("HEAD")[0];
                head.appendChild(scriptTag);
                head = scriptTag = null;
            }

            return result;
        }
    };



    var contentType = function (str) {
        /// <summary>Parses a string into an object with media type and properties.</summary>
        /// <param name="str" type="String">String with media type to parse.</param>
        /// <returns>null if the string is empty; an object with 'mediaType' and a 'properties' dictionary otherwise.</returns>

        if (!str) {
            return null;
        }

        var contentTypeParts = str.split(";");
        var properties = {};

        var i, len;
        for (i = 1, len = contentTypeParts.length; i < len; i++) {
            var contentTypeParams = contentTypeParts[i].split("=");
            properties[trimString(contentTypeParams[0])] = contentTypeParams[1];
        }

        return { mediaType: trimString(contentTypeParts[0]), properties: properties };
    };

    var contentTypeToString = function (contentType) {
        /// <summary>Serializes an object with media type and properties dictionary into a string.</summary>
        /// <param name="contentType">Object with media type and properties dictionary to serialize.</param>
        /// <returns>String representation of the media type object; undefined if contentType is null or undefined.</returns>

        if (!contentType) {
            return undefined;
        }

        var result = contentType.mediaType;
        var property;
        for (property in contentType.properties) {
            result += ";" + property + "=" + contentType.properties[property];
        }
        return result;
    };

    var createReadWriteContext = function (contentType, dataServiceVersion, metadata, handler) {
        /// <summary>Creates an object that is going to be used as the context for the handler's parser and serializer.</summary>
        /// <param name="contentType">Object with media type and properties dictionary.</param>
        /// <param name="dataServiceVersion" type="String">String indicating the version of the protocol to use.</param>
        /// <param name="metadata">Conceptual metadata of a request or response.</param>
        /// <param name="handler">Handler object that is processing a resquest or response.</param>
        /// <returns>Context object.</returns>

        return {
            contentType: contentType,
            dataServiceVersion: dataServiceVersion,
            metadata: metadata,
            handler: handler
        };
    };

    var fixRequestHeader = function (request, name, value) {
        /// <summary>Sets a request header's value. If the header has already a value other than undefined, null or empty string, then this method does nothing.</summary>
        /// <param name="request">Request object on which the header will be set.</summary>
        /// <param name="name" type="String">Header name.</param>
        /// <param name="value" type="String">Header value.</param>
        if (!request) {
            return;
        }

        var headers = request.headers;
        if (!headers[name]) {
            headers[name] = value;
        }
    };

    var fixDataServiceVersion = function (context, version) {
        /// <summary>Sets the dataServiceVersion component of the context. If the component has already a value other than undefined, null or empty string, then this method does nothing.</summary>
        /// <param name="context">Context object used for serialization.</summary>
        /// <param name="version" type="String">Version value.</param>

        if (!context.dataServiceVersion) {
            context.dataServiceVersion = version;
        }
    };

    var getRequestOrResponseHeader = function (requestOrResponse, name) {
        /// <summary>Gets the value of a request or response header.</summary>
        /// <param name="requestOrResponse">Object representing a request or a response.</summary>
        /// <param name="name" type="String">Name of the header to retrieve.</param>
        /// <returns type="String">String value of the header; undefined if the header cannot be found.</returns>

        var headers = requestOrResponse.headers;
        return (headers && headers[name]) || undefined;
    };

    var getContentType = function (requestOrResponse) {
        /// <summary>Gets the value of the Content-Type header from a request or response.</summary>
        /// <param name="requestOrResponse">Object representing a request or a response.</summary>
        /// <returns type="Object">Object with 'mediaType' and a 'properties' dictionary; null in case that the header is not found or doesn't have a value.</returns>

        return contentType(getRequestOrResponseHeader(requestOrResponse, "Content-Type"));
    };

    var getDataServiceVersion = function (requestOrResponse) {
        /// <summary>Gets the value of the DataServiceVersion header from a request or response.</summary>
        /// <param name="requestOrResponse">Object representing a request or a response.</summary>
        /// <returns type="String">Data service version; undefined if the header cannot be found.</returns>

        // TODO: add version string parsing and validation. 
        return getRequestOrResponseHeader(requestOrResponse, "DataServiceVersion");
    };

    var handlerAccepts = function (handler, cType) {
        /// <summary>Checks that a handler can process a particular mime type.</summary>
        /// <param name="handler">Handler object that is processing a resquest or response.</summary>
        /// <param name="cType">Object with 'mediaType' and a 'properties' dictionary.</summary>
        /// <returns type="Boolean">True if the handler can process the mime type; false otherwise.</returns>

        // The following check isn't as strict because if cType.mediaType = application/; it will match an accept value of "application/xml";
        // however in practice we don't not expect to see such "suffixed" mimeTypes for the handlers. 
        return handler.accept.indexOf(cType.mediaType) >= 0;
    };

    var handlerRead = function (handler, parseCallback, response, context) {
        /// <summary>Invokes the parser associated with a handler for reading the payload of a HTTP response.</summary>
        /// <param name="handler">Handler object that is processing the response.</summary>
        /// <param name="parseCallback" type="Function">Parser function that will process the response payload.</summary>
        /// <param name="response">HTTP response whose payload is going to be processed.</summary>
        /// <param name="context">Object used as the context for processing the response.</summary>
        /// <returns type="Boolean">True if the handler processed the response payload and the response.data property was set; false otherwise.</returns>

        if (!response || !response.headers) {
            return false;
        }

        var cType = getContentType(response);
        var version = getDataServiceVersion(response) || "";
        var body = response.body;

        if (body === undefined || body === null) {
            return false;
        }

        if (handlerAccepts(handler, cType)) {
            var readContext = createReadWriteContext(cType, version, (context) ? context.metadata : null, handler);
            readContext.response = response;
            response.data = parseCallback(handler, body, readContext);
            return response.data && true;
        }

        return false;
    };

    var handlerWrite = function (handler, serializeCallback, request, context) {
        /// <summary>Invokes the serializer associated with a handler for generating the payload of a HTTP request.</summary>
        /// <param name="handler">Handler object that is processing the request.</summary>
        /// <param name="serializeCallback" type="Function">Serializer function that will generate the request payload.</summary>
        /// <param name="response">HTTP request whose payload is going to be generated.</summary>
        /// <param name="context">Object used as the context for serializing the request.</summary>
        /// <returns type="Boolean">True if the handler serialized the request payload and the request.body property was set; false otherwise.</returns>
        if (!request || !request.headers) {
            return false;
        }

        var cType = getContentType(request);
        var version = getDataServiceVersion(request);

        if (!cType || handlerAccepts(handler, cType)) {
            var writeContext = createReadWriteContext(cType, version, (context) ? context.metadata : null, handler);
            writeContext.request = request;

            request.body = serializeCallback(handler, request.data, writeContext);

            if (request.body) {
                fixRequestHeader(request, "DataServiceVersion", writeContext.dataServiceVersion || "1.0");
                fixRequestHeader(request, "Content-Type", contentTypeToString(writeContext.contentType));
                return true;
            }
        }

        return false;
    };

    var handler = function (parseCallback, serializeCallback, accept, maxDataServiceVersion) {
        /// <summary>Creates a handler object for processing HTTP requests and responses.</summary>
        /// <param name="parseCallback" type="Function">Parser function that will process the response payload.</summary>
        /// <param name="serializeCallback" type="Function">Serializer function that will generate the request payload.</summary>
        /// <param name="accept" type="String">String containing a comma separated list of the mime types that this handler can work with.</summary>
        /// <param name="maxDataServiceVersion" type="String">String indicating the highest version of the protocol that this handler can work with.</summary>
        /// <returns type="Object">Handler object.</returns>
        return {
            accept: accept,
            maxDataServiceVersion: maxDataServiceVersion,

            read: function (response, context) {
                return handlerRead(this, parseCallback, response, context);
            },

            write: function (request, context) {
                return handlerWrite(this, serializeCallback, request, context);
            }
        };
    };



    var xmlMediaType = "application/xml";

    var uriRegEx = /^([^:/?#]+:)?(\/\/[^/?#]*)?([^?#:]+)?(\?[^#]*)?(#.*)?/;

    // URI prefixes to generate smaller code.
    var http = "http://";
    var w3org = http + "www.w3.org/";               // http://www.w3.org/
    var ado = http + "schemas.microsoft.com/ado/";  // http://schemas.microsoft.com/ado/
    var adoDs = ado + "2007/08/dataservices";       // http://schemas.microsoft.com/ado/2007/08/dataservices

    var xmlnsNS = w3org + "2000/xmlns/";            // http://www.w3.org/2000/xmlns/
    var xmlNS = w3org + "XML/1998/namespace";       // http://www.w3.org/XML/1998/namespace
    var edmxNs = ado + "2007/06/edmx";              // http://schemas.microsoft.com/ado/2007/06/edmx
    var edmNs = ado + "2008/09/edm";                // http://schemas.microsoft.com/ado/2008/09/edm
    var edmNs2 = ado + "2006/04/edm";               // http://schemas.microsoft.com/ado/2006/04/edm
    var edmNs3 = ado + "2007/05/edm";               // http://schemas.microsoft.com/ado/2007/05/edm
    var atomXmlNs = w3org + "2005/Atom";            // http://www.w3.org/2005/Atom
    var appXmlNs = w3org + "2007/app";              // http://www.w3.org/2007/app
    var odataXmlNs = adoDs;                         // http://schemas.microsoft.com/ado/2007/08/dataservices
    var odataMetaXmlNs = adoDs + "/metadata";       // http://schemas.microsoft.com/ado/2007/08/dataservices/metadata
    var odataRelatedPrefix = adoDs + "/related/";   // http://schemas.microsoft.com/ado/2007/08/dataservices/related
    var odataScheme = adoDs + "/scheme";            // http://schemas.microsoft.com/ado/2007/08/dataservices/scheme

    var uriPartNames = ["scheme", "authority", "path", "query", "fragment"];

    var getURIInfo = function (uri) {
        /// <summary>Gets information about the components of the specified URI.</summary>
        /// <param name="uri" type="String">URI to get information from.</param>
        /// <returns type="Object">
        /// An object with an isAbsolute flag and part names (scheme, authority, etc.) if available.
        /// </returns>

        var result = { isAbsolute: false };

        if (uri) {
            var matches = uriRegEx.exec(uri);
            if (matches) {
                var i, len;
                for (i = 0, len = uriPartNames.length; i < len; i++) {
                    if (matches[i + 1]) {
                        result[uriPartNames[i]] = matches[i + 1];
                    }
                }
            }
            if (result.scheme) {
                result.isAbsolute = true;
            }
        }

        return result;
    };

    var normalizeURI = function (uri, base) {
        /// <summary>Normalizes a possibly relative URI with a base URI.</summary>
        /// <param name="uri" type="String">URI to normalize, absolute or relative.</param>
        /// <param name="base" type="String" mayBeNull="true">Base URI to compose with.</param>
        /// <returns type="String">The composed URI if relative; the original one if absolute.</returns>

        if (!base) {
            return uri;
        }

        var uriInfo = getURIInfo(uri);
        if (uriInfo.isAbsolute) {
            return uri;
        }

        var baseInfo = getURIInfo(base);
        var normInfo = {};

        if (uriInfo.authority) {
            normInfo.authority = uriInfo.authority;
            normInfo.path = uriInfo.path;
            normInfo.query = uriInfo.query;
        } else {
            if (!uriInfo.path) {
                normInfo.path = baseInfo.path;
                normInfo.query = uriInfo.query || baseInfo.query;
            } else {
                if (uriInfo.path.charAt(0) === '/') {
                    normInfo.path = uriInfo.path;
                } else {
                    normInfo.path = mergeUriPathWithBase(uriInfo, baseInfo);
                }
                
                normInfo.query = uriInfo.query;
            }
            
            normInfo.authority = baseInfo.authority;
        }
        
        normInfo.scheme = baseInfo.scheme;
        normInfo.fragment = uriInfo.fragment;

        return "".concat(
            normInfo.scheme || "",
            normInfo.authority || "",
            normInfo.path || "",
            normInfo.query || "",
            normInfo.fragment || "");
    };

    var mergeUriPathWithBase = function (uriInfo, baseInfo) {
        /// <summary>Merges the path of a relative URI and a base URI.</summary>
        /// <param name="uriInfo">URI component information for the relative URI.</param>
        /// <param name="baseInfo">URI component information for the base URI.</param>
        /// <returns type="String">A string with the merged path.</returns>

        var basePath = "/";
        if (baseInfo.path) {
            var end = baseInfo.path.lastIndexOf("/");
            basePath = baseInfo.path.substring(0, end);

            if (basePath.charAt(basePath.length - 1) !== "/") {
                basePath = basePath + "/";
            }
        }

        return basePath + uriInfo.path;
    };

    var hasLeadingOrTrailingWhitespace = function (text) {
        /// <summary>Checks whether the specified string has leading or trailing spaces.</summary>
        /// <param name="text" type="String">String to check.</param>
        /// <returns type="Boolean">true if text has any leading or trailing whitespace; false otherwise.</returns>

        var re = /(^\s)|(\s$)/;
        return re.test(text);
    };

    var xml = {
        addNamespaceAttribute: function (domNode, name, attributeNamespace) {
            /// <summary>Adds a namespace declaration attribute to the specified element node.</summary>
            /// <param name="domNode">DOM node for the element.</param>
            /// <param name="domNode" type="String">Attribute name, eg: xmlns, xmlns:foo, xmlns:bar.</param>
            /// <param name="attributeNamespace" type="String">Namespace to associate.</param>

            var doc = domNode.ownerDocument;
            var attribute;
            if (doc.createAttributeNS) {
                attribute = doc.createAttributeNS(xmlnsNS, name);
            } else {
                attribute = doc.createNode(2, name, xmlnsNS);
            }

            attribute.nodeValue = attributeNamespace;
            domNode.setAttributeNode(attribute);
        },

        appendAsXml: function (domNode, xmlAsText) {
            /// <summary>Appends an XML text fragment into the specified DOM element node.</summary>
            /// <param name="domNode">DOM node for the parent element.</param>
            /// <param name="xmlAsText" type="String" mayBeNull="false">XML to append as a child of element.</param>

            var value = "<container>" + xmlAsText + "</container>";
            var parsed = xml.parse(value, null);
            var imported = domNode.ownerDocument.importNode(parsed.domNode, true);
            var importedChild = imported.firstChild;
            while (importedChild) {
                domNode.appendChild(importedChild);
                importedChild = importedChild.nextSibling;
            }
        },

        appendPreserving: function (domNode, text) {
            /// <summary>Appends a text node into the specified DOM element node.</summary>
            /// <param name="domNode">DOM node for the element.</param>
            /// <param name="text" type="String" mayBeNull="false">Text to append as a child of element.</param>

            if (hasLeadingOrTrailingWhitespace(text)) {
                domNode.setAttribute("xml:space", "preserve");
            }

            var textNode = domNode.ownerDocument.createTextNode(text);
            domNode.appendChild(textNode);
        },

        attributes: function (element, onAttributeCallback) {
            /// <summary>Iterates through the XML element's attributes and invokes the callback function for each one.</summary>
            /// <param name="element">Wrapped element to iterate over.</param>
            /// <param name="onAttributeCallback" type="Function">Callback function to invoke with wrapped attribute nodes.</param>

            var attribute;
            var domNode = element.domNode;
            var i, len;
            for (i = 0, len = domNode.attributes.length; i < len; i++) {
                attribute = domNode.attributes.item(i);
                onAttributeCallback(this._wrapNode(attribute));
            }
        },

        attribute: function (element, localName, nsURI) {
            /// <summary>Returns the value of an xml element attribute.</summary>
            return this._attribute(element.domNode, localName, nsURI);
        },

        attributeNode: function (domNode, localName, nsURI) {
            /// <summary>Gets an attribute node from an element.</summary>
            /// <param name="domNode">DOM node for the parent element.</param>
            /// <param name="localName" type="String">Local name for the attribute.</param>
            /// <param name="nsURI" type="String">Namespace URI for the attribute.</param>
            /// <returns>The attribute node, null if not found.</returns>

            var attributes = domNode.attributes;
            if (attributes.getNamedItemNS) {
                return attributes.getNamedItemNS(nsURI, localName);
            }

            return attributes.getQualifiedItem(localName, nsURI);
        },

        childElements: function (element, onElementCallback) {
            /// <summary>Iterates through the XML element's child elements and invokes the callback function for each one.</summary>
            /// <param name="element">Wrapped element to iterate over.</param>
            /// <param name="onElementCallback" type="Function">Callback function to invoke with wrapped element nodes.</param>

            var child = element.domNode.firstChild;
            var childBaseURI;
            while (child !== null) {
                if (child.nodeType === 1) {
                    childBaseURI = normalizeURI(this._baseURI(child), element.baseURI);
                    onElementCallback(this._wrapNode(child, childBaseURI));
                }

                child = child.nextSibling;
            }
        },

        firstDomElementChild: function (domNode) {
            /// <summary>Returns the node's first child element, or null if none is found.</summary>
            /// <param name="domNode" optional="false" mayBeNull="false">Parent element.</param>
            /// <returns>First child of element type; null if none found.</returns>

            var child = domNode.firstChild;
            while (child) {
                if (child.nodeType === 1) {
                    break;
                }

                child = child.nextSibling;
            }

            return child;
        },

        firstElement: function (element) {
            /// <summary>Returns the first child element (wrapped) of the specified element.</summary>
            /// <param name="element">Element to get first child for.</param>
            /// <returns>This first child element (wrapped), null if there is none.</returns>

            var child = element.domNode.firstChild;
            var childBaseURI;
            while (child !== null) {
                if (child.nodeType === 1) {
                    childBaseURI = normalizeURI(this._baseURI(child), element.baseURI);
                    return this._wrapNode(child, childBaseURI);
                }

                child = child.nextSibling;
            }

            return null;
        },

        getSingleElementByTagNameNS: function (domNode, namespaceURI, localName) {
            /// <summary>Gets the first element under 'domNode' with the spacified name and namespace.</summary>
            /// <param name="domNode">DOM element node.</param>
            /// <param name="namespaceURI" type="String">The namespace URI of the element to match.</param>
            /// <param name="localName" type="String">The local name of the element to match.</param>
            /// <returns>The first element found, null if none.</returns>
            /// <remarks>namespaceURI should be a specific namespace, otherwise the behavior is unspecified.</remarks>

            var result;
            if (domNode.getElementsByTagNameNS) {
                result = domNode.getElementsByTagNameNS(namespaceURI, localName);
                if (result.length !== 0) {
                    return result[0];
                }
            } else {
                var child = domNode.firstChild;
                while (child) {
                    if (child.nodeType === 1 && 
                        xml.localName(child) === localName && 
                        child.namespaceURI === namespaceURI) {
                        return child;
                    }
                    
                    child = child.nextSibling;
                }
            }

            return null;
        },

        innerText: function (domNode) {
            /// <summary>Returns the text value of an XML element.</summary>
            /// <param name="domNode">DOM element</param>
            /// <returns type="String">
            /// The text content of the node or the concatenated text
            /// representing the node and its descendants; never null.
            /// </returns>

            var result = domNode.text;
            if (result !== undefined) {
                return result;
            }

            result = "";
            
            var cursor = domNode.firstChild;
            if (cursor) {
                do {
                    // Process the node.
                    if (cursor.nodeType === 3 || cursor.nodeType === 4) {
                        result += cursor.nodeValue;
                    }

                    // Advance the node.
                    var next = cursor.firstChild;
                    if (!next) {
                        while (cursor !== domNode) {
                            next = cursor.nextSibling;
                            if (next) {
                                cursor = next;
                                break;
                            } else {
                                cursor = cursor.parentNode;
                            }
                        }
                    } else {
                        cursor = next;
                    }
                } while (cursor !== domNode);
            }

            return result;
        },

        localName: function (node) {
            /// <summary>Returns the localName of a XML node.</summary>
            /// <param name="node">DOM node to get value for.</param>
            /// <returns type="String">localName of node.</returns>

            if (node.localName) {
                return node.localName;
            }
            
            return node.baseName;
        },

        newDocument: function (name, nsURI) {
            /// <summary>Creates a new XML document.</summary>
            /// <param name="name" type="String">Local name of the root element.</param>
            /// <param name="nsURI" type="String">Namespace of the root element.</param>
            /// <returns>The wrapped root element of the document.</returns>

            var dom;
            
            // IE9 beta native dom support is still in development so we fall back to ActiveX instead.  
            if (window.ActiveXObject) {
                dom = new ActiveXObject("Msxml2.DOMDocument.6.0");
                dom.async = false;
                dom.documentElement = dom.createNode(1, name, nsURI);
            } else {
                // Otherwise, this is the regular test for document construction support.
                if (window.document.implementation && window.document.implementation.createDocument) {
                    dom = window.document.implementation.createDocument(nsURI, name, null);
                }
            }

            // Set the processing instructions.
            var pi = dom.createProcessingInstruction("xml", "version=\'1.0\' encoding=\'utf-8\'");
            dom.insertBefore(pi, dom.documentElement);

            return this._wrapNode(dom.documentElement);
        },

        newDomAttribute: function (domNode, localName, nsURI, nsPrefix) {
            /// <summary>Creates a new unwrapped DOM attribute.</summary>
            /// <param name="domNode">Parent element to which new node should be added.</param>
            /// <param name="localName" type="String">Local name for attribute.</param>
            /// <param name="nsURI" type="String">Namespace URI for the attribute.</param>
            /// <param name="nsPrefix" type="String">Namespace prefix for attribute to be created.</param>
            /// <returns>The created DOM attribute.</returns>

            var doc = domNode.ownerDocument;
            var attribute;
            localName = (nsPrefix) ? (nsPrefix + ":" + localName) : localName;
            if (doc.createAttributeNS) {
                attribute = doc.createAttributeNS(nsURI, localName);
                domNode.setAttributeNodeNS(attribute);
            } else {
                attribute = doc.createNode(2, localName, nsURI || undefined);
                domNode.setAttributeNode(attribute);
            }

            return attribute;
        },

        newDomElement: function (domNode, localName, nsURI, nsPrefix) {
            /// <summary>Creates a new unwrapped DOM element.</summary>
            /// <param name="domNode">Parent element to which new node should be added.</param>
            /// <param name="localName" type="String">Local name for element.</param>
            /// <param name="nsURI" type="String">Namespace URI for the element.</param>
            /// <param name="nsPrefix" type="String">Namespace prefix for attribute to be created.</param>
            /// <returns>The created DOM element.</returns>

            var doc = domNode.ownerDocument;
            var element;
            localName = (nsPrefix) ? (nsPrefix + ":" + localName) : localName;
            if (doc.createElementNS) {
                element = doc.createElementNS(nsURI, localName);
            } else {
                element = doc.createNode(1, localName, nsURI || undefined);
            }

            domNode.appendChild(element);
            return element;
        },

        newElement: function (parent, name, nsURI, value) {
            /// <summary>Creates a new wrapped element.</summary>
            /// <param name="parent">Wrapped parent element to which new node should be added.</param>
            /// <param name="name" type="String">Local name for element.</param>
            /// <param name="nsURI" type="String">Namespace URI for the element.</param>
            /// <param name="value" type="String" mayBeNull="true">Text value to append in element, if applicable.</param>
            /// <returns>The created wrapped element.</returns>

            var dom = parent.domNode.ownerDocument;

            var element;
            if (dom.createElementNS) {
                element = dom.createElementNS(nsURI, name);
            } else {
                element = dom.createNode(1, name, nsURI || undefined);
            }

            if (value) {
                this.appendPreserving(element, value);
            }

            parent.domNode.appendChild(element);
            return this._wrapNode(element);
        },

        newAttribute: function (element, name, nsURI, value) {
            /// <summary>Creates a new wrapped attribute.</summary>
            /// <param name="element">Wrapped parent element to which new node should be added.</param>
            /// <param name="name" type="String">Local name for element.</param>
            /// <param name="nsURI" type="String">Namespace URI for the element.</param>
            /// <param name="value" type="String">Node value for the attribute.</param>
            /// <returns>The created wrapped attribute.</returns>

            var dom = element.domNode.ownerDocument;

            var attribute;
            if (dom.createAttributeNS) {
                attribute = dom.createAttributeNS(nsURI, name);
                attribute.value = value;
                element.domNode.setAttributeNodeNS(attribute);
            } else {
                attribute = dom.createNode(2, name, nsURI || undefined);
                attribute.value = value;
                element.domNode.setAttributeNode(attribute);
            }

            return this._wrapNode(attribute);
        },

        qualifyXmlTagName: function (name, prefix) {
            /// <summary>Qualifies an XML tag name with the specified prefix.</summary>
            /// <param name="name" type="String">Element name</param>
            /// <param name="prefix" type="String">Prefix to use, possibly null.</param>
            /// <returns type="String">The qualified name.</returns>
            /// <remarks>This operates at the lexical level - there is no awareness of in-scope prefixes.</remarks>

            if (prefix) {
                return prefix + ":" + name;
            }
            
            return name;
        },

        parse: function (text, baseURI) {
            /// <summary>Returns an XML document from the specified text.</summary>
            /// <param name="text" type="String">Document text.</param>
            /// <param name="baseURI" type="String">Base URI for the document.</param>
            /// <returns>The wrapped root element of the document.</returns>

            var root = this._parse(text);
            var rootBaseURI = normalizeURI(this._baseURI(root), baseURI);
            return this._wrapNode(root, rootBaseURI);
        },

        serialize: function (root) {
            /// <summary>
            /// Returns the text representation of the document to which the specified node belongs.
            /// </summary>
            /// <param name="root">Wrapped element in the document to serialize.</param>
            /// <returns type="String">Serialized document.</returns>

            var dom = root.domNode.ownerDocument;
            return this.serializeNode(dom);
        },

        serializeChildren: function (domNode) {
            /// <summary>Returns the XML representation of the all the descendants of the node.</summary>
            /// <param name="domNode" optional="false" mayBeNull="false">Node to serialize.</param>
            /// <returns type="String">The XML representation of all the descendants of the node.</returns>
            
            var children = domNode.childNodes;
            var i, len = children.length;
            if (len === 0) {
                return "";
            }

            var fragment = domNode.ownerDocument.createDocumentFragment();
            for (i = 0; i < len; i++) {
                fragment.appendChild(children[i].cloneNode(true));
            }

            var result = fragment.xml;
            if (result !== undefined) {
                return result;
            } else {
                var serializer = new window.XMLSerializer();
                return serializer.serializeToString(fragment);
            }
        },

        serializeNode: function (domNode) {
            /// <summary>Returns the XML representation of the node and all its descendants.</summary>
            /// <param name="domNode" optional="false" mayBeNull="false">Node to serialize.</param>
            /// <returns type="String">The XML representation of the node and all its descendants.</returns>
            
            var xml = domNode.xml;
            if (xml !== undefined) {
                return xml;
            } 
            
            if (window.XMLSerializer) {
                var serializer = new window.XMLSerializer();
                return serializer.serializeToString(domNode);
            }
            
            throw { message: "XML serialization unsupported" };
        },

        _attribute: function (domNode, localName, nsURI) {
            /// <summary>Gets the value of a DOM attribute.</summary>
            /// <param name="domNode">DOM element to get attribute for.</param>
            /// <param name="localName" type="String">Name of the attribute to get.</param>
            /// <param name="nsURI" type="String">Namespace of the attribute to get.</param>
            /// <returns type="String">Value of the attribute if found; null otherwise.</returns>

            if (domNode.getAttributeNS) {
                return domNode.getAttributeNS(nsURI || null, localName);
            }
            
            // The method is not supported so we work with the attributes collection. 
            var node = domNode.attributes.getQualifiedItem(localName, nsURI);
            if (node) {
                return node.value;
            }

            return null;
        },

        _baseURI: function (domNode) {
            /// <summary>Gets the value of the xml:base attribute on the specified element.</summary>
            /// <param name="domNode">Element to get attribute value from.</param>
            /// <returns type="String">Value of the xml:base attribute if found; null otherwise.</returns>

            return this._attribute(domNode, "base", xmlNS);
        },

        _parse: function (text) {
            /// <summary>Returns an XML document from the specified text.</summary>
            /// <param name="text" type="String">Document text.</param>
            /// <returns>The root element of the document.</returns>

            var dom;
            // TODO: add DOMParser error handling code. FireFox, IE8, IE9 have different mechanisms to report the parser 
            // errors, IE9 and FireFox overload the same property (DOMParser.parseError);
            // IE9 beta native dom support is still in development so we fall back to ActiveX instead.  
            if (window.ActiveXObject) {
                dom = new ActiveXObject("Msxml2.DOMDocument.6.0");
                dom.async = false;
                dom.loadXML(text);
                if (dom.parseError.errorCode !== 0) {
                    throw { message: dom.parseError.reason, errorXmlText: text, srcText: dom.parseError.srcText };
                }
            } else {
                var parser = new window.DOMParser();
                dom = parser.parseFromString(text, "text/xml");
            } 
            
            return dom.documentElement;
        },

        _wrapNode: function (domNode, baseURI) {
            /// <summary>Creates a wrapped DOM node.</summary>
            /// <param name="domNode">DOM node to wrap.</param>
            /// <param name="baseURI" type="String">Base URI in scope for the node.</param>
            /// <returns type="Object">
            /// An object with the wrapped domNode, information about its in-scope URI, and simple 
            /// access to name and namespace.
            /// </returns>

            return {
                baseURI: baseURI,
                domNode: domNode,
                localName: this.localName(domNode),
                nsURI: domNode.namespaceURI || null
            };
        }
    };

    odata.xml = xml;



    var atomAcceptTypes = ["application/atom+xml", "application/atomsvc+xml", "application/xml"];
    var atomMediaType = atomAcceptTypes[0];

    var inlineTag = xml.qualifyXmlTagName("inline", "m");
    var propertiesTag = xml.qualifyXmlTagName("properties", "m");
    var propertyTypeAttribute = xml.qualifyXmlTagName("type", "m");
    var propertyNullAttribute = xml.qualifyXmlTagName("null", "m");

    // These are the namespaces that are not considered ATOM extension namespaces.
    var nonExtensionNamepaces = [atomXmlNs, appXmlNs, xmlNS, xmlnsNS];

    // These are entity property mapping paths that have well-known paths.
    var knownCustomizationPaths = {
        SyndicationAuthorEmail: "author/email",
        SyndicationAuthorName: "author/name",
        SyndicationAuthorUri: "author/uri",
        SyndicationContributorEmail: "contributor/email",
        SyndicationContributorName: "contributor/name",
        SyndicationContributorUri: "contributor/uri",
        SyndicationPublished: "published",
        SyndicationRights: "rights",
        SyndicationSummary: "summary",
        SyndicationTitle: "title",
        SyndicationUpdated: "updated"
    };

    var isExtensionNs = function (nsURI) {
        /// <summary>Checks whether the specified namespace is an extension namespace to ATOM.</summary>
        /// <param type="String" name="nsURI">Namespace to check.</param>
        /// <returns type="Boolean">true if nsURI is an extension namespace to ATOM; false otherwise.</returns>

        return !(contains(nonExtensionNamepaces, nsURI));
    };

    var propertyTypeDefaultConverter = function (propertyValue) {
        /// <summary>Does a no-op conversion on the specified value.</summary>
        /// <param name="propertyValue">Value to convert.</param>
        /// <returns>Original property value.</returns>

        return propertyValue;
    };

    var parseBool = function (propertyValue) {
        /// <summary>Parses a string into a boolean value.</summary>
        /// <param name="propertyValue">Value to parse.</param>
        /// <returns type="Boolean">true if the property value is 'true'; false otherwise.</returns>

        return propertyValue === "true";
    };

    // The captured indices for this expression are:
    // 0     - complete input
    // 1,2,3 - year with optional minus sign, month, day
    // 4,5,6 - hours, minutes, seconds
    // 7     - optional milliseconds
    // 8     - everything else (presumably offset information)
    var parseDateTimeRE = /^(-?\d{4,})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})(\.\d+)?(.*)$/;

    var parseDateTimeMaybeOffset = function (value, withOffset) {
        /// <summary>Parses a string into a DateTime value.</summary>
        /// <param name="value" type="String">Value to parse.</param>
        /// <param name="withOffset" type="Boolean">Whether offset is expected.</param>
        /// <returns type="Date">The parsed value.</returns>

        // We cannot parse this in cases of failure to match or if offset information is specified.
        var parts = parseDateTimeRE.exec(value);
        var offset = (parts) ? getCanonicalTimezone(parts[8]) : null;

        if (!parts || (!withOffset && offset !== "Z")) {
            throw { message: "Invalid date/time value" };
        }

        // Pre-parse years, account for year '0' being invalid in dateTime.
        var year = parseInt(parts[1], 10);
        if (year <= 0) {
            year++;
        }

        // Pre-parse optional milliseconds, fill in default. Fail if value is too precise.
        var ms = parts[7];
        if (!ms) {
            ms = 0;
        } else {
            if (ms.length > 4) {
                throw { message: "Cannot parse date/time value to given precision." };
            }
            while (ms.length < 4) {
                ms += "0";
            }
            ms = parseInt(ms.substring(1), 10);
        }

        // Pre-parse other time components and offset them if necessary.
        var hours = parseInt(parts[4], 10);
        var minutes = parseInt(parts[5], 10);
        var seconds = parseInt(parts[6], 10);
        if (offset !== "Z") {
            // The offset is reversed to get back the UTC date, which is
            // what the API will eventually have.
            var timezone = parseTimezone(offset);
            var direction = -(timezone.d);
            hours += timezone.h * direction;
            minutes += timezone.m * direction;
        }

        // Set the date and time separately with setFullYear, so years 0-99 aren't biased like in Date.UTC.
        var result = new Date();
        result.setUTCFullYear(
            year,                       // Year.
            parseInt(parts[2], 10) - 1, // Month (zero-based for Date.UTC and setFullYear).
            parseInt(parts[3], 10)      // Date.
            );
        result.setUTCHours(hours, minutes, seconds, ms);

        if (isNaN(result.valueOf())) {
            throw { message: "Invalid date/time value" };
        }

        if (withOffset) {
            result.__edmType = "Edm.DateTimeOffset";
            result.__offset = offset;
        }

        return result;
    };

    var parseDateTime = function (propertyValue) {
        /// <summary>Parses a string into a DateTime value.</summary>
        /// <param name="propertyValue" type="String">Value to parse.</param>
        /// <returns type="Date">The parsed value.</returns>

        return parseDateTimeMaybeOffset(propertyValue, false);
    };

    var parseDateTimeOffset = function (propertyValue) {
        /// <summary>Parses a string into a DateTimeOffset value.</summary>
        /// <param name="propertyValue" type="String">Value to parse.</param>
        /// <returns type="Date">The parsed value.</returns>
        /// <remarks>
        /// The resulting object is annotated with an __edmType property and
        /// an __offset property reflecting the original intended offset of
        /// the value. The time is adjusted for UTC time, as the current
        /// timezone-aware Date APIs will only work with the local timezone.
        /// </remarks>

        return parseDateTimeMaybeOffset(propertyValue, true);
    };

    var parseTime = function () {
        /// <summary>Parses a string into a Time value.</summary>
        /// <param name="propertyValue" type="String">Value to parse.</param>
        /// <returns type="Date">The parsed value.</returns>
        /// <remarks>
        /// The resulting object is annotated with an __edmType proeprty.
        /// </remarks>

        // Edm.Time is conceptually the time component of a datetime, but
        // serialization can also be a duration.
        throw { message: "Edm.Time not supported" };
    };

    // Property type converters are parsers that convert strings into typed values.
    var propertyTypeConverters = {
        "Edm.Boolean": parseBool,
        "Edm.Binary": propertyTypeDefaultConverter,
        "Edm.DateTime": parseDateTime,
        "Edm.DateTimeOffset": parseDateTimeOffset,
        "Edm.Time": parseTime,
        "Edm.Decimal": propertyTypeDefaultConverter,
        "Edm.Guid": propertyTypeDefaultConverter,
        "Edm.String": propertyTypeDefaultConverter,
        "Edm.Byte": parseInt,
        "Edm.Double": parseFloat,
        "Edm.Single": parseFloat,
        "Edm.Int16": parseInt,
        "Edm.Int32": parseInt,
        "Edm.Int64": propertyTypeDefaultConverter,
        "Edm.SByte": parseInt
    };

    var formatToString = function (value) {
        /// <summary>Formats a value by invoking .toString() on it.</summary>
        /// <param name="value" mayBeNull="false">Value to format.</param>
        /// <returns type="String">Formatted text.</returns>

        return value.toString();
    };

    var formatTime = parseTime;

    // Property type formatters are serializers that convert typed values into strings.
    var propertyTypeFormatters = {
        "Edm.Binary": formatToString,
        "Edm.Boolean": formatToString,
        "Edm.Byte": formatToString,
        "Edm.DateTime": formatDateTimeOffset,
        "Edm.DateTimeOffset": formatDateTimeOffset,
        "Edm.Decimal": formatToString,
        "Edm.Double": formatToString,
        "Edm.Guid": formatToString,
        "Edm.Int16": formatToString,
        "Edm.Int32": formatToString,
        "Edm.Int64": formatToString,
        "Edm.SByte": formatToString,
        "Edm.Single": formatToString,
        "Edm.String": formatToString,
        "Edm.Time": formatTime
    };

    var isPrimitiveType = function (typeName) {
        /// <summary>Checks whether the specified type name is a primitive type.</summary>
        /// <param name="typeName" type="String" mayBeNull="true">Name of type to check.</param>
        /// <returns type="Boolean">
        /// true if the type is the name of a primitive type; false otherwise.
        /// </returns>

        return typeName && (propertyTypeConverters[typeName] !== undefined);
    };

    var convertFromAtomPropertyText = function (value, targetType) {
        /// <summary>Converts a text value to the specified target type.</summary>
        /// <param name="value" type="String" mayBeNull="true">Text value to convert.</param>
        /// <param name="targetType" type="String" mayBeNull="true">Name of type to convert from.</param>
        /// <returns>The converted value.</returns>

        if (value !== null && targetType) {
            var converter = propertyTypeConverters[targetType];
            if (converter) {
                value = converter(value);
            }
        }

        return value;
    };

    var convertToAtomPropertyText = function (value, targetType) {
        /// <summary>Converts a typed value from the specified target type into a text value.</summary>
        /// <param name="value">Typed value to convert.</param>
        /// <param name="targetType" type="String" mayBeNull="true">Name of type to convert to. <param>
        /// <returns type="String">The converted value as text.</returns>

        if (value !== null && targetType) {
            if (isDate(value)) {
                targetType = (isDateTimeOffset(value)) ? "Edm.DateTimeOffset" : "Edm.DateTime";
            }

            var converter = propertyTypeFormatters[targetType];
            if (converter) {
                value = converter(value);
            }
        }

        return value;
    };

    var readAtomDocument = function (atomElement, metadata) {
        /// <summary>Reads an ATOM entry, feed or service document, producing an object model in return.</summary>
        /// <param name="atomElement">Top-level element to read.</param>
        /// <param name="metadata">Metadata that describes the conceptual schema.</param>
        /// <returns type="Object">The object model representing the specified element.</returns>

        // Handle feed and entry elements.
        if (atomElement.nsURI === atomXmlNs) {
            switch (atomElement.localName) {
                case "feed":
                    return readAtomFeed(atomElement, metadata);
                case "entry":
                    return readAtomEntry(atomElement, metadata);
            }
        }

        // Handle service documents. 
        if (atomElement.nsURI === appXmlNs && atomElement.localName === "service") {
            return readAtomServiceDocument(atomElement);
        }

        throw { message: "Invalid AtomPub document: root element can only be feed, entry, or service", element: atomElement };
    };

    var readAtomFeed = function (atomFeed, metadata) {
        /// <summary>Deserializes an ATOM feed element.</summary>
        /// <param name="atomFeed">ATOM feed element.</param>
        /// <param name="metadata">Metadata that describes the conceptual schema.</param>
        /// <returns type="Object">A new object representing the feed.</returns>
        var feed = {
            results: [],
            __metadata: {}
        };

        feed.__metadata.feed_extensions = readAtomExtensionAttributes(atomFeed);

        xml.childElements(atomFeed, function (feedChild) {
            switch (feedChild.nsURI) {
                case atomXmlNs:
                    switch (feedChild.localName) {
                        case "id":
                            feed.__metadata.uri = normalizeURI(xml.innerText(feedChild.domNode), feedChild.baseURI);
                            feed.__metadata.uri_extensions = readAtomExtensionAttributes(feedChild);
                            break;
                        case "title":
                            feed.__metadata.title = xml.innerText(feedChild.domNode);
                            feed.__metadata.title_extensions = readAtomExtensionAttributes(feedChild);
                            break;
                        case "entry":
                            var entry = readAtomEntry(feedChild, metadata);
                            feed.results.push(entry);
                            break;
                        case "link":
                            readAtomFeedLink(feedChild, feed);
                            break;
                    }
                    return;
                case odataMetaXmlNs:
                    if (feedChild.localName === "count") {
                        feed.__count = parseInt(xml.innerText(feedChild.domNode));
                        return;
                    }
                    break;
            }

            var extension = readAtomExtensionElement(feedChild);
            feed.__metadata.feed_extensions.push(extension);
        });

        return feed;
    };

    var readAtomFeedLink = function (feedLinkElement, feed) {
        /// <summary>Reads an ATOM link element for a feed.</summary>
        /// <param name="feedLinkElement">Link element to read.</param>
        /// <param name="feed">Feed object to be annotated with information.</param>

        var link = readAtomLink(feedLinkElement);
        switch (link.rel) {
            case "next":
                feed.__next = link.href;
                feed.__metadata.next_extensions = link.extensions;
                break;
            case "self":
                feed.__metadata.self = link.href;
                feed.__metadata.self_extensions = link.extensions;
                break;
        }
    };

    var readAtomLink = function (linkElement) {
        /// <summary>Reads an ATOM link element.</summary>
        /// <param name="linkElement">Link element to read.</param>
        /// <returns type="Object">A link element representation.</returns>

        var link = { extensions: [] };
        var linkExtension;

        xml.attributes(linkElement, function (attribute) {
            if (!attribute.nsURI) {
                switch (attribute.localName) {
                    case "href":
                        link.href = normalizeURI(attribute.domNode.nodeValue, linkElement.baseURI);
                        return;
                    case "type":
                    case "rel":
                        link[attribute.localName] = attribute.domNode.nodeValue;
                        return;
                }
            }

            if (isExtensionNs(attribute.nsURI)) {
                linkExtension = readAtomExtensionAttribute(attribute);
                link.extensions.push(linkExtension);
            }
        });

        if (!link.href) {
            throw { error: "href attribute missing on link element", element: linkElement };
        }

        return link;
    };

    var readAtomExtensionElement = function (atomExtension) {
        /// <summary>Reads an ATOM extension element (an element not in the ATOM namespaces).</summary>
        /// <param name="atomExtension">ATOM extension element.</param>
        /// <returns type="Object">An extension element representation.</returns>

        var extension = {
            name: atomExtension.localName,
            namespaceURI: atomExtension.nsURI,
            attributes: readAtomExtensionAttributes(atomExtension),
            children: []
        };

        xml.childElements(atomExtension, function (child) {
            var childExtension = readAtomExtensionElement(child);
            extension.children.push(childExtension);
        });

        if (extension.children.length === 0) {
            var text = xml.innerText(atomExtension.domNode);
            if (text) {
                extension.value = text;
            }
        }

        return extension;
    };

    var readAtomExtensionAttributes = function (xmlElement) {
        /// <summary>Reads ATOM extension attributes from an element.</summary>
        /// <param name="xmlElement">ATOM element with zero or more extension attributes.</param>
        /// <returns type="Array">An array of extension attribute representations.</returns>

        var extensions = [];
        xml.attributes(xmlElement, function (attribute) {
            if (isExtensionNs(attribute.nsURI)) {
                var extension = readAtomExtensionAttribute(attribute);
                extensions.push(extension);
            }
        });

        return extensions;
    };

    var readAtomExtensionAttribute = function (attribute) {
        /// <summary>Reads an ATOM extension attribute into an object.</summary>
        /// <param name="attribute">ATOM extension attribute.</param>
        /// <returns type="Object">An object with the attribute information.</returns>

        return {
            name: attribute.localName,
            namespaceURI: attribute.nsURI,
            value: attribute.domNode.nodeValue
        };
    };

    var getObjectValueByPath = function (path, item) {
        /// <summary>Gets a slashed path value from the specified item.</summary>
        /// <param name="path" type="String">Property path to read ('/'-separated).</param>
        /// <param name="item" type="Object">Object to get value from.</param>
        /// <returns>The property value, possibly undefined if any path segment is missing.</returns>

        // Fast path.
        if (path.indexOf('/') === -1) {
            return item[path];
        } else {
            var parts = path.split('/');
            var i, len;
            for (i = 0, len = parts.length; i < len; i++) {
                // Avoid traversing a null object.
                if (item === null) {
                    return undefined;
                }

                item = item[parts[i]];
                if (item === undefined) {
                    return item;
                }
            }

            return item;
        }
    };

    var setObjectValueByPath = function (path, target, value, propertyType) {
        /// <summary>Sets a slashed path value on the specified target.</summary>
        /// <param name="path" type="String">Property path to set ('/'-separated).</param>
        /// <param name="target" type="Object">Object to set value on.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="propertyType" type="String" optional="true">Property type to set in metadata.</param>

        // Fast path.
        var propertyName;
        if (path.indexOf('/') === -1) {
            target[path] = value;
            propertyName = path;
        } else {
            var parts = path.split('/');
            var i, len;
            for (i = 0, len = (parts.length - 1); i < len; i++) {
                // We construct each step of the way if the property is missing;
                // if it's already initialized to null, we stop further processing.
                var next = target[parts[i]];
                if (next === undefined) {
                    next = {};
                    target[parts[i]] = next;
                } else if (next === null) {
                    return;
                }

                target = next;
            }

            propertyName = parts[i];
            target[propertyName] = value;
        }

        if (propertyType) {
            var metadata = target.__metadata = target.__metadata || {};
            var properties = metadata.properties = metadata.properties || {};
            var property = properties[propertyName] = properties[propertyName] || {};
            property.type = propertyType;
        }
    };

    var expandCustomizationPath = function (path) {
        /// <summary>Expands a customization path if it's well-known.</summary>
        /// <param name="path" type="String">Path to expand.</param>
        /// <returns type="String">Expanded path or 'path' otherwise.</returns>

        return knownCustomizationPaths[path] || path;
    };

    var getXmlPathValue = function (ns, xmlPath, element, readAsXml) {
        /// <summary>Returns the text value of the element or attribute at xmlPath.</summary>
        /// <param name="ns" type="String">Namespace for elements to match.</param>
        /// <param name="xmlPath" type="String">
        /// '/'-separated list of element names, possibly ending with a '@'-prefixed attribute name.
        /// </param>
        /// <param name="element">Root element.</param>
        /// <param name="readAsXml">Whether the value should be read as XHTML.</param>
        /// <returns type="String">The text value of the node found, null if none.</returns>

        var parts = xmlPath.split('/');
        var i, len;
        for (i = 0, len = parts.length; i < len; i++) {
            if (parts[i][0] === "@") {
                return xml._attribute(element, parts[i].substring(1), ns);
            } else {
                element = xml.getSingleElementByTagNameNS(element, ns, parts[i]);
                if (!element) {
                    return undefined;
                }
            }
        }

        if (readAsXml) {
            // Treat per XHTML in http://tools.ietf.org/html/rfc4287#section-3.1.1, including the DIV
            // in the content.
            return xml.serializeChildren(element);
        } else {
            return xml.innerText(element);
        }
    };

    var setXmlPathValue = function (ns, nsPrefix, xmlPath, element, writeAsXml, value) {
        /// <summary>Sets the text value of the element or attribute at xmlPath.</summary>
        /// <param name="ns" type="String">Namespace for elements to match.</param>
        /// <param name="nsPrefix" type="String">Namespace prefix for elements to be created.</param>
        /// <param name="xmlPath" type="String">
        /// '/'-separated list of element names, possibly ending with a '@'-prefixed attribute name.
        /// </param>
        /// <param name="element">Root element.</param>
        /// <param name="writeAsXml" type="Boolean">Whether the value should be written as XHTML.</param>
        /// <param name="value" type="String">The text value of the node to write.</param>

        var target = element;
        var parts = xmlPath.split('/');
        var i, len;
        for (i = 0, len = parts.length; i < len; i++) {
            var next;
            if (parts[i][0] === "@") {
                next = xml.attributeNode(target, parts[i].substring(1), ns);
                if (!next) {
                    next = xml.newDomAttribute(target, parts[i].substring(1), ns, nsPrefix);
                }
            } else {
                next = xml.getSingleElementByTagNameNS(target, ns, parts[i]);
                if (!next) {
                    next = xml.newDomElement(target, parts[i], ns, nsPrefix);
                }
            }

            target = next;
        }

        // Target can be an attribute (2) or an element (1).
        if (target.nodeType === 2) {
            target.value = value;
        } else {
            // The element should be empty at this point; we won't erase its contents.
            if (writeAsXml) {
                target.setAttribute("type", "xhtml");
                xml.appendAsXml(target, value);
            } else {
                xml.appendPreserving(target, value);
            }
        }
    };

    var isElementEmpty = function (element) {
        /// <summary>Checks whether the specified XML element is empty.</summary>
        /// <param name="element">DOM element node to check.</param>
        /// <returns type="Boolean">true if the element is empty; false otherwise.</returns>
        /// <remarks>
        /// The element is considered empty if it doesn't have any attributes other than
        /// namespace declarations and if it has no child nodes.
        /// </remarks>

        // If there are any child elements or text nodes, it's not empty.
        if (element.childNodes.length) {
            return false;
        }

        // If there are no attributes, then we know it's already empty.
        var attributes = element.attributes;
        var len = attributes.length;
        if (len === 0) {
            return true;
        }

        // Otherwise, we have to search for attributes that aren't namespace declarations.
        for (var i = 0; i < len; i++) {
            var attributeName = attributes[i].nodeName;
            if (attributeName !== "xmlns" && attributeName.indexOf("xmlns:") !== 0) {
                return false;
            }
        }

        return true;
    };
    var removeXmlProperty = function (entryElement, propertyPath) {
        /// <summary>Removes a property from an entry.</summary>
        /// <param name="entryElement">XML element for an ATOM OData entry.</param>
        /// <param name="propertyPath" type="String">Property path to an element.</param>

        // Get the 'properties' node from 'content' or 'properties'.
        var propertiesElement = xml.getSingleElementByTagNameNS(entryElement.domNode, odataMetaXmlNs, "properties");
        if (!propertiesElement) {
            var contentElement = xml.getSingleElementByTagNameNS(entryElement.domNode, atomXmlNs, "content");
            if (contentElement) {
                propertiesElement = xml.getSingleElementByTagNameNS(contentElement, odataMetaXmlNs, "properties");
            }
        }

        if (propertiesElement) {
            // Traverse down to the parent of the property path.
            var propertyParentElement = propertiesElement;
            var parts = propertyPath.split("/");
            var i, len;
            for (i = 0, len = (parts.length - 1); i < len; i++) {
                propertyParentElement = xml.getSingleElementByTagNameNS(propertyParentElement, odataXmlNs, parts[i]);
                if (!propertyParentElement) {
                    return;
                }
            }

            // Remove the child from its parent.
            var propertyElement = xml.getSingleElementByTagNameNS(propertyParentElement, odataXmlNs, parts[i]);
            if (propertyElement) {
                propertyParentElement.removeChild(propertyElement);
            }

            // Remove empty elements up the parent chain.
            var candidate = propertyParentElement;
            while (candidate !== propertiesElement && isElementEmpty(candidate)) {
                var parent = candidate.parentNode;
                parent.removeChild(candidate);
                candidate = parent;
            }
        }
    };

    var applyEntryCustomizationToEntry = function (customization, sourcePath, entryElement, entryObject, propertyType, suffix, context) {
        /// <summary>Applies a specific feed customization item to an entry.</summary>
        /// <param name="customization">Object with customization description.</param>
        /// <param name="sourcePath">Property path to map ('source' in the description).</param>
        /// <param name="entryElement">XML element for the entry that corresponds to the object being written.</param>
        /// <param name="entryObject">Object being written.</param>
        /// <param name="propertyType" type="String">Name of property type to write.</param>
        /// <param name="suffix" type="String">Suffix to feed customization properties.</param>
        /// <param name="context">Context used for serialization.</param>

        var targetPath = customization["FC_TargetPath" + suffix];
        var xmlPath = expandCustomizationPath(targetPath);
        var xmlNamespace = (targetPath !== xmlPath) ? atomXmlNs : customization["FC_NsUri" + suffix];
        var keepInContent = (customization["FC_KeepInContent" + suffix] === "true") ? true : false;
        var writeAsXhtml = (customization["FC_ContentKind" + suffix] === "xhtml");
        var prefix = customization["FC_NsPrefix" + suffix] || null;

        // Get the value to be written.
        var value = getObjectValueByPath(sourcePath, entryObject);

        // Special case: for null values, the 'property' should be left as it was generated.
        // undefined values will appear when metadata describe a property the object doesn't have.
        if (value === null || value === undefined) {
            return;
        }

        // Remove the property if it should not be kept in content.
        if (!keepInContent) {
            fixDataServiceVersion(context, "2.0");
            removeXmlProperty(entryElement, sourcePath);
        }

        // Set/create the subtree for the property path with the appropriate value.
        value = convertToAtomPropertyText(value, propertyType);
        setXmlPathValue(xmlNamespace, prefix, xmlPath, entryElement.domNode, writeAsXhtml, value);
    };

    var applyEntryCustomizationToObject = function (customization, sourcePath, entryElement, entryObject, propertyType, suffix) {
        /// <summary>Applies a specific feed customization item to an object.</summary>
        /// <param name="customization">Object with customization description.</param>
        /// <param name="sourcePath">Property path to set ('source' in the description).</param>
        /// <param name="entryElement">XML element for the entry that corresponds to the object being read.</param>
        /// <param name="entryObject">Object being read.</param>
        /// <param name="propertyType" type="String">Name of property type to set.</param>
        /// <param name="suffix" type="String">Suffix to feed customization properties.</param>

        // If keepInConent equals true then we do nothing as the object has been deserialized at this point.
        if (customization["FC_KeepInContent" + suffix] === "true") {
            return;
        }
        // An existing 'null' means that the property was set to null in the properties,
        // which overrides other items.
        if (getObjectValueByPath(sourcePath, entryObject) === null) {
            return;
        }

        var targetPath = customization["FC_TargetPath" + suffix];
        var xmlPath = expandCustomizationPath(targetPath);
        var xmlNamespace = (targetPath !== xmlPath) ? atomXmlNs : customization["FC_NsUri" + suffix];
        var readAsXhtml = (customization["FC_ContentKind" + suffix] === "xhtml");
        var value = getXmlPathValue(xmlNamespace, xmlPath, entryElement.domNode, readAsXhtml);

        // If the XML tree does not contain the necessary elements to read the value,
        // then it shouldn't be considered null, but rather ignored at all. This prevents
        // the customization from generating the object path down to the property.
        if (value === undefined) {
            return;
        }

        value = convertFromAtomPropertyText(value, propertyType);

        // Set the value on the object.
        setObjectValueByPath(sourcePath, entryObject, value, propertyType);
    };

    var lookupPropertyType = function (metadata, entityType, path) {
        /// <summary>Looks up the type of a property given its path in an entity type.</summary>
        /// <param name="metadata">Metadata in which to search for base and complex types.</param>
        /// <param name="entityType">Entity type to which property belongs.</param>
        /// <param name="path" type="String" mayBeNull="false">Property path to look at.</param>
        /// <returns type="String">The name of the property type; possibly null.</returns>

        var parts = path.split("/");
        var i, len;
        while (entityType) {
            // Keep track of the type being traversed, necessary for complex types.
            var traversedType = entityType;

            for (i = 0, len = parts.length; i < len; i++) {
                // Traverse down the structure as necessary.
                var properties = traversedType.property;
                if (!properties) {
                    break;
                }

                // Find the property by scanning the property list (might be worth pre-processing).
                var j, propLength;
                var found = false;
                for (j = 0, propLength = properties.length; j < propLength; j++) {
                    if (properties[j].name === parts[i]) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    break;
                }

                var propertyType = properties[j].type;

                // We could in theory still be missing types, but that would
                // be caused by a malformed path.
                if (!propertyType || isPrimitiveType(propertyType)) {
                    return propertyType || null;
                }

                traversedType = lookupComplexType(propertyType, metadata);
                if (!traversedType) {
                    return null;
                }
            }

            // Traverse up the inheritance chain.
            entityType = lookupEntityType(entityType.baseType, metadata);
        }

        return null;
    };

    var applyMetadataToObject = function (entryElement, entryObject, metadata) {
        /// <summary>Applies feed customization properties to an object being read.</summary>
        /// <param name="entryElement">XML element for the entry that corresponds to the object being read.</param>
        /// <param name="entryObject">Object being read.</param>
        /// <param name="metadata">Metadata that describes the conceptual schema.</param>

        if (!metadata || metadata.length === 0) {
            return;
        }

        var typeName = entryObject.__metadata.type;
        while (typeName) {
            var entityType = lookupEntityType(typeName, metadata);
            if (!entityType) {
                return;
            }

            // Apply all feed customizations from the entity and each of its properties.
            var propertyType;
            var source = entityType.FC_SourcePath;
            if (source) {
                propertyType = lookupPropertyType(metadata, entityType, source);
                applyEntryCustomizationToObject(entityType, source, entryElement, entryObject, propertyType, "");
            }

            var properties = entityType.property;
            if (properties) {
                var i, len;
                for (i = 0, len = properties.length; i < len; i++) {
                    var property = properties[i];
                    var suffixCounter = 0;
                    var suffix = "";
                    while (property["FC_TargetPath" + suffix]) {
                        source = property.name;
                        propertyType = property.type;

                        var sourcePath = property["FC_SourcePath" + suffix];
                        if (sourcePath) {
                            source += "/" + sourcePath;
                            propertyType = lookupPropertyType(metadata, entityType, source);
                        }

                        applyEntryCustomizationToObject(property, source, entryElement, entryObject, propertyType, suffix);
                        suffixCounter++;
                        suffix = "_" + suffixCounter;
                    }
                }
            }

            // Apply feed customizations from base types.
            typeName = entityType.baseType;
        }
    };

    var readAtomEntry = function (atomEntry, metadata) {
        /// <summary>Reads an ATOM entry in OData format.</summary>
        /// <param name="atomEntry">XML element for the entry.</param>
        /// <param name="metadata">Metadata that describes the conceptual schema.</param>
        /// <returns type="Object">An object in payload representation format.</returns>

        var entry = {
            __metadata: {}
        };

        var etag = xml.attribute(atomEntry, "etag", odataMetaXmlNs);
        if (etag) {
            entry.__metadata.etag = etag;
        }

        xml.childElements(atomEntry, function (entryChild) {
            if (entryChild.nsURI === atomXmlNs) {
                switch (entryChild.localName) {
                    case "id":
                        entry.__metadata.uri = normalizeURI(xml.innerText(entryChild.domNode), entryChild.baseURI);
                        entry.__metadata.uri_extensions = readAtomExtensionAttributes(entryChild);
                        break;
                    case "category":
                        readAtomEntryType(entryChild, entry);
                        break;
                    case "content":
                        readAtomEntryContent(entryChild, entry);
                        break;
                    case "link":
                        readAtomEntryLink(entryChild, entry, metadata);
                        break;
                }
            }

            if (entryChild.nsURI === odataMetaXmlNs && entryChild.localName === "properties") {
                readAtomEntryStructuralObject(entryChild, entry, entry.__metadata);
            }
        });

        // Apply feed customizations if applicable.
        applyMetadataToObject(atomEntry, entry, metadata);

        return entry;
    };

    var readAtomEntryType = function (atomCategory, entry) {
        /// <summary>Reads type information from an ATOM category element.</summary>
        /// <param name="atomCategory">XML category element.</param>
        /// <param name="entry">Entry object to update with information.</param>

        var scheme = xml.attribute(atomCategory, "scheme");
        var term = xml.attribute(atomCategory, "term");

        if (scheme === odataScheme) {
            if (entry.__metadata.type) {
                throw { message: "Invalid AtomPub document: multiple category elements defining the entry type were encounterd withing an entry", element: atomCategory };
            }

            entry.__metadata.type = term;
            entry.__metadata.type_extensions = [];

            var typeExtension;
            xml.attributes(atomCategory, function (attribute) {
                if (!attribute.nsURI) {
                    if (attribute.localName !== "scheme" && attribute.localName !== "term") {
                        typeExtension = readAtomExtensionAttribute(attribute);
                        entry.__metadata.type_extensions.push(typeExtension);
                    }
                } else if (isExtensionNs(attribute.nsURI)) {
                    typeExtension = readAtomExtensionAttribute(attribute);
                    entry.__metadata.type_extensions.push(typeExtension);
                }
            });
        }
    };

    var readAtomEntryContent = function (atomEntryContent, entry) {
        /// <summary>Reads an ATOM content element.</summary>
        /// <param name="atomEntryContent">XML entry element.</param>
        /// <param name="entry">Entry object to update with information.</param>

        var src = xml.attribute(atomEntryContent, "src");
        var type = xml.attribute(atomEntryContent, "type");

        if (src) {
            if (!type) {
                throw { message: "Invalid AtomPub document: content element must specify the type attribute if the src attribute is also specified", element: atomEntryContent };
            }
            entry.__metadata.media_src = normalizeURI(src, atomEntryContent.baseURI);
            entry.__metadata.content_type = type;
        }

        xml.childElements(atomEntryContent, function (contentChild) {
            if (src) {
                throw { message: "Invalid AtomPub document: content element must not have child elements if the src attribute is specified", element: atomEntryContent };
            }

            if (contentChild.nsURI === odataMetaXmlNs && contentChild.localName === "properties") {
                readAtomEntryStructuralObject(contentChild, entry, entry.__metadata);
            }
        });
    };

    var readAtomEntryEditMediaLink = function (link, entry) {
        /// <summary>Reads an ATOM media link element.</summary>
        /// <param name="link">Link representation (not the XML element).</param>
        /// <param name="entry">Entry object to update with information.</param>

        entry.__metadata.edit_media = link.href;
        entry.__metadata.edit_media_extensions = [];

        // Read the link extensions.
        var i, len;
        for (i = 0, len = link.extensions.length; i < len; i++) {
            if (link.extensions[i].namespaceURI === odataMetaXmlNs && link.extensions[i].name === "etag") {
                entry.__metadata.media_etag = link.extensions[i].value;
            } else {
                entry.__metadata.edit_media_extensions.push(link.extensions[i]);
            }
        }
    };

    var readAtomEntryLink = function (atomEntryLink, entry, metadata) {
        /// <summary>Reads a link element on an entry.</summary>
        /// <param name="atomEntryLink">'link' element on the entry.</param>
        /// <param name="entry">An object in payload representation format.</entry>
        /// <param name="metadata">Metadata that describes the conceptual schema.</param>

        var link = readAtomLink(atomEntryLink);
        switch (link.rel) {
            case "self":
                entry.__metadata.self = link.href;
                entry.__metadata.self_link_extensions = link.extensions;
                break;
            case "edit":
                entry.__metadata.edit = link.href;
                entry.__metadata.edit_link_extensions = link.extensions;
                break;
            case "edit-media":
                readAtomEntryEditMediaLink(link, entry);
                break;
            default:
                if (link.rel.indexOf(odataRelatedPrefix) === 0) {
                    readAtomEntryDeferredProperty(atomEntryLink, link, entry, metadata);
                }
        }
    };

    var readAtomEntryDeferredProperty = function (atomEntryLink, link, entry, metadata) {
        /// <summary>Reads a potentially-deferred property on an entry.</summary>
        /// <param name="atomEntryLink">'link' element on the entry.</param>
        /// <param name="link">Parsed link object.</param>
        /// <param name="entry">An object in payload representation format.</entry>
        /// <param name="metadata">Metadata that describes the conceptual schema.</param>

        var propertyName = link.rel.substring(odataRelatedPrefix.length);

        // undefined is used as a flag that inline data was not found (as opposed to
        // found with a value or with null).
        var inlineData = undefined;

        // Get any inline data.
        xml.childElements(atomEntryLink, function (child) {
            if (child.nsURI === odataMetaXmlNs && child.localName === "inline") {
                var inlineRoot = xml.firstElement(child);
                if (inlineRoot) {
                    inlineData = readAtomDocument(inlineRoot, metadata);
                } else {
                    inlineData = null;
                }
            }
        });

        // If the link has no inline content, we consider it deferred.
        if (inlineData === undefined) {
            inlineData = { __deferred: { uri: link.href} };
        }

        // Set the property value on the entry object.
        entry[propertyName] = inlineData;

        // Set the extra property information on the entry object metadata. 
        entry.__metadata.properties = entry.__metadata.properties || {};
        entry.__metadata.properties[propertyName] = {
            extensions: link.extensions
        };
    };

    var readAtomEntryStructuralObject = function (propertiesElement, parent, parentMetadata) {
        /// <summary>Reads an atom entry's property as a structural object and sets its value in the parent and the metadata in the parentMetadata objects.</summary>
        /// <param name="propertiesElement">XML element for the 'properties' node.</param>
        /// <param name="parent">
        /// Object that will contain the property value. It can be either an antom entry or 
        /// an atom complex property object.
        /// </param>
        /// <param name="parentMetadata">Object that will contain the property metadata. It can be either an atom entry metadata or a complex property metadata object</param>

        xml.childElements(propertiesElement, function (property) {
            if (property.nsURI === odataXmlNs) {
                parentMetadata.properties = parentMetadata.properties || {};
                readAtomEntryProperty(property, parent, parentMetadata.properties);
            }
        });
    };

    var readAtomEntryProperty = function (property, parent, metadata) {
        /// <summary>Reads a property on an ATOM OData entry.</summary>
        /// <param name="property">XML element for the property.</param>
        /// <param name="parent">
        /// Object that will contain the property value. It can be either an antom entry or 
        /// an atom complex property object.
        /// </param>
        /// <param name="metadata">Metadata for the object that will contain the property value.</param>

        var propertyNullValue = null;
        var propertyTypeValue = "Edm.String";
        var propertyExtensions = [];

        xml.attributes(property, function (attribute) {
            if (attribute.nsURI === odataMetaXmlNs) {
                switch (attribute.localName) {
                    case "null":
                        propertyNullValue = attribute.domNode.nodeValue;
                        return;
                    case "type":
                        propertyTypeValue = attribute.domNode.nodeValue;
                        return;
                };
            }

            if (isExtensionNs(attribute.nsURI)) {
                var extension = readAtomExtensionAttribute(attribute);
                propertyExtensions.push(extension);
            }
        });

        var propertyValue = null;
        var propertyMetadata = {
            type: propertyTypeValue,
            extensions: propertyExtensions
        };

        if (propertyNullValue !== "true") {
            propertyValue = xml.innerText(property.domNode);
            if (isPrimitiveType(propertyTypeValue)) {
                propertyValue = convertFromAtomPropertyText(propertyValue, propertyTypeValue);
            } else {
                // Probe for a complex type and read it.
                if (xml.firstElement(property)) {
                    propertyValue = { __metadata: { type: propertyTypeValue} };
                    readAtomEntryStructuralObject(property, propertyValue, propertyMetadata);
                }
            }
        }

        parent[property.localName] = propertyValue;
        metadata[property.localName] = propertyMetadata;
    };

    var readAtomServiceDocument = function (atomServiceDoc) {
        /// <summary>Reads an atom service document</summary>
        /// <param name="atomServiceDoc">An element node that represents the root service element of an AtomPub service document</param>
        /// <returns type="Object">An object that contains the properties of the service document</returns>

        // Consider handling Accept and Category elements.

        var serviceDoc = {
            workspaces: [],
            extensions: []
        };

        // Find all the workspace elements.
        xml.childElements(atomServiceDoc, function (child) {
            if (child.nsURI === appXmlNs && child.localName === "workspace") {
                var workspace = readAtomServiceDocumentWorkspace(child);
                serviceDoc.workspaces.push(workspace);
            } else {
                var serviceExtension = readAtomExtensionElement(atomServiceDoc);
                serviceDoc.extensions.push(serviceExtension);
            }
        });

        // AtomPub (RFC 5023 Section 8.3.1) says a service document MUST contain one or 
        // more workspaces. Throw if we don't find any. 
        if (serviceDoc.workspaces.length === 0) {
            throw { message: "Invalid AtomPub service document: No workspace element found.", element: atomServiceDoc };
        }

        return serviceDoc;
    };

    var readAtomServiceDocumentWorkspace = function (atomWorkspace) {
        /// <summary>Reads a single workspace element from an atom service document</summary>
        /// <param name="atomWorkspace">An element node that represents a workspace element of an AtomPub service document</param>
        /// <returns type="Object">An object that contains the properties of the workspace</returns>

        var workspace = {
            collections: [],
            extensions: []
        };

        xml.childElements(atomWorkspace, function (child) {
            if (child.nsURI === atomXmlNs) {
                if (child.localName === "title") {
                    if (atomWorkspace.title) {
                        throw { message: "Invalid AtomPub service document: workspace has more than one child title element", element: child };
                    }

                    workspace.title = xml.innerText(child.domNode);
                }
            } else if (child.nsURI === appXmlNs) {
                if (child.localName === "collection") {
                    var collection = readAtomServiceDocumentCollection(child, workspace);
                    workspace.collections.push(collection);
                }
            } else {
                var extension = readAtomExtensionElement(atomWorkspace);
                workspace.extensions.push(extension);
            }
        });

        workspace.title = workspace.title || "";

        return workspace;
    };

    var readAtomServiceDocumentCollection = function (atomCollection) {
        /// <summary>Reads a service document collection element into an object.</summary>
        /// <param name="atomCollection">An element node that represents a collection element of an AtomPub service document.</param>
        /// <returns type="Object">An object that contains the properties of the collection.</returns>

        var collection = {
            href: xml.attribute(atomCollection, "href"),
            extensions: []
        };

        if (!collection.href) {
            throw { message: "Invalid AtomPub service document: collection has no href attribute", element: atomCollection };
        }

        collection.href = normalizeURI(collection.href, atomCollection.baseURI);

        xml.childElements(atomCollection, function (child) {
            if (child.nsURI === atomXmlNs) {
                if (child.localName === "title") {
                    if (collection.title) {
                        throw { message: "Invalid AtomPub service document: collection has more than one child title element", element: child };
                    }

                    collection.title = xml.innerText(child.domNode);
                }
            } else if (child.nsURI !== appXmlNs) {
                var extension = readAtomExtensionElement(atomCollection);
                collection.extensions.push(extension);
            }
        });

        // AtomPub (RFC 5023 Section 8.3.3) says the collection element MUST contain 
        // a title element. It's likely to be problematic if the service doc doesn't 
        // have one so here we throw. 
        if (!collection.title) {
            throw { message: "Invalid AtomPub service document: collection has no title element", element: atomCollection };
        }

        return collection;
    };

    var writeAtomDocument = function (data, context) {
        /// <summary>Writes the specified data into an OData ATOM document.</summary>
        /// <param name="data">Data to write.</param>
        /// <param name="context">Context used for serialization.</param>
        /// <returns>The root of the DOM tree built.</returns>

        var docRoot;
        var type = payloadTypeOf(data);
        switch (type) {
            case payloadType.feed:
                docRoot = writeAtomFeed(null, data, context);
                break;
            case payloadType.entry:
                // FALLTHROUGH
            case payloadType.complexType:
                docRoot = writeAtomEntry(null, data, context);
                break;
            default:
                throw { message: "Serialization of payloads of type \"" + type + "\" is not supported" };
        }

        return docRoot;
    };

    var writeAtomRoot = function (parent, name) {
        /// <summary>Writes the root of an ATOM document, possibly under an existing element.</summary>
        /// <param name="parent" mayBeNull="true">Element under which to create a new element.</param>
        /// <param name="name">Name for the new element, to be created in the ATOM namespace.</param>
        /// <returns>The created element.</returns>

        if (parent) {
            return xml.newElement(parent, name, atomXmlNs);
        }

        var result = xml.newDocument(name, atomXmlNs);

        // Add commonly used namespaces.
        // ATOM is implied by the just-created element.
        // xml.addNamespaceAttribute(result.domNode, "xmlns", atomXmlNs);
        xml.addNamespaceAttribute(result.domNode, "xmlns:d", odataXmlNs);
        xml.addNamespaceAttribute(result.domNode, "xmlns:m", odataMetaXmlNs);

        return result;
    };

    var writeAtomFeed = function (parent, data, context) {
        /// <summary>Writes the specified feed data into an OData ATOM feed.</summary>
        /// <param name="parent" mayBeNull="true">Parent to append feed tree to.</param>
        /// <param name="data">Feed data to write.</param>
        /// <param name="context">Context used for serialization.</param>
        /// <returns>The feed element of the DOM tree built.</returns>

        var feed = writeAtomRoot(parent, "feed");
        var entries = (isArray(data)) ? data : data.results;
        if (entries) {
            var i, len;
            for (i = 0, len = entries.length; i < len; i++) {
                writeAtomEntry(feed, entries[i], context);
            }
        }

        return feed;
    };

    var writeAtomEntry = function (parent, data, context) {
        /// <summary>Appends an ATOM entry XML payload to the parent node.</summary>
        /// <param name="parent">Parent element.</param>
        /// <param name="data">Data object to write in intermediate format.</param>
        /// <param name="context">Context used for serialization.</param>
        /// <returns>The new entry.</returns>

        var entry = writeAtomRoot(parent, "entry");

        // Set up a default empty author name as required by ATOM.
        var author = xml.newElement(entry, "author", atomXmlNs);
        xml.newElement(author, "name", atomXmlNs);

        // Set up a default empty title as required by ATOM.
        xml.newElement(entry, "title", atomXmlNs);

        var content = xml.newElement(entry, "content", atomXmlNs);
        xml.newAttribute(content, "type", null, "application/xml");

        var properties = xml.newElement(content, propertiesTag, odataMetaXmlNs);

        var propertiesMetadata = (data.__metadata) ? data.__metadata.properties : null;

        writeAtomEntryMetadata(entry, data.__metadata);
        writeAtomEntryProperties(entry, properties, data, propertiesMetadata, context);
        applyMetadataToEntry(entry, data, context);

        return entry;
    };

    var applyMetadataToEntry = function (entry, data, context) {
        /// <summary>Applies feed customizations to the specified entry element.</summary>
        /// <param name="entry">Entry to apply feed customizations to.</param>
        /// <param name="data">Data object associated with the entry.</param>
        /// <param name="context">Context used for serialization.</param>

        if (!data.__metadata) {
            return;
        }

        var metadata = context.metadata;
        var entityType = lookupEntityType(data.__metadata.type, metadata);
        while (entityType) {
            // Apply all feed customizations from the entity and each of its properties.
            var propertyType;
            var source = entityType.FC_SourcePath;
            if (source) {
                propertyType = lookupPropertyType(metadata, entityType, source);
                applyEntryCustomizationToEntry(entityType, source, entry, data, propertyType, "", context);
            }

            var properties = entityType.property;
            if (properties) {
                var i, len;
                for (i = 0, len = properties.length; i < len; i++) {
                    var property = properties[i];
                    var suffixCounter = 0;
                    var suffix = "";
                    while (property["FC_TargetPath" + suffix]) {
                        source = property.name;
                        if (property["FC_SourcePath" + suffix]) {
                            source += "/" + property["FC_SourcePath" + suffix];
                        }

                        applyEntryCustomizationToEntry(property, source, entry, data, property.type, suffix, context);
                        suffixCounter++;
                        suffix = "_" + suffixCounter;
                    }
                }
            }

            // Apply feed customizations from base types.
            entityType = lookupEntityType(entityType.baseType, metadata);
        }
    };

    var writeAtomEntryMetadata = function (entry, metadata) {
        /// <summary>Writes the content of metadata into the specified DOM entry element.</summary>
        /// <param name="entry">DOM entry element.</param>
        /// <param name="metadata" mayBeNull="true">Object __metadata to write.</param>

        if (metadata) {
            // Write the etag if present.
            if (metadata.etag) {
                xml.newAttribute(entry, "etag", odataMetaXmlNs, metadata.etag);
            }

            // Write the ID if present.
            if (metadata.uri) {
                xml.newElement(entry, "id", atomXmlNs, metadata.uri);
            }

            // Write the type name if present.
            if (metadata.type) {
                var category = xml.newElement(entry, "category", atomXmlNs);
                xml.newAttribute(category, "term", null, metadata.type);
                xml.newAttribute(category, "scheme", null, odataScheme);
            }
        }
    };

    var writeAtomEntryLink = function (entry, href, rel) {
        /// <summary>Writes an ATOM link into an entry.</summary>
        /// <param name="entry">DOM entry element to add link to.</param>
        /// <param name="href" type="String">Value for href attribute in link element.</param>
        /// <param name="rel" type="String">Value for rel attribute in link element</param>
        /// <returns>The new link element.</returns>

        var link = xml.newElement(entry, "link", atomXmlNs);
        xml.newAttribute(link, "rel", null, rel);
        xml.newAttribute(link, "href", null, href);
        return link;
    };

    var writeAtomEntryProperties = function (entry, parentElement, data, propertiesMetadata, context) {
        /// <summary>Writes the properties of an entry or complex type.</summary>
        /// <param name="entry" mayBeNull="true">Entry object being written out; null if this is a complex type.</param>
        /// <param name="parentElement">Parent DOM element under which the property should be added.</param>
        /// <param name="data">Data object to write in intermediate format.</param>
        /// <param name="propertiesMetadata" mayBeNull="true">Instance metadata about properties of the 'data' object.</param>
        /// <param name="context">Context used for serialization.</param>

        var name, value, kind, propertyMetadata;
        for (name in data) {
            if (name !== "__metadata") {
                value = data[name];
                kind = propertyKindOf(value);
                switch (kind) {
                    case propertyKind.primitive:
                    case propertyKind.complex:
                        propertyMetadata = (propertiesMetadata) ? propertiesMetadata[name] : null;
                        writeAtomEntryProperty(parentElement, name, kind, value, propertyMetadata, context);
                        break;
                    case propertyKind.deferred:
                    case propertyKind.inline:
                        writeAtomEntryDeferredProperty(entry, kind, name, value, context);
                        break;
                    case propertyKind.none:
                        // This could be a null primitive property or a null link.
                        if (propertyMetadata && propertyMetadata[name] && !isPrimitiveType(propertyMetadata[name])) {
                            // TODO: this is a null - should it be serialized?
                        } else {
                            // This is a a null primitive property.
                            writeAtomEntryProperty(parentElement, name, propertyKind.primitive, value, null, context);
                        }

                        break;
                }
            }
        }
    };

    var writeAtomEntryProperty = function (parentElement, name, kind, value, propertiesMetadata, context) {
        /// <summary>Writes a single property for an entry or complex type.</summary>
        /// <param name="parentElement">Parent DOM element under which the property should be added.</param>
        /// <param name="name" type="String">Property name.</param>
        /// <param name="kind" type="String">Property kind description (from propertyKind values).</param>
        /// <param name="value" mayBeNull="true">Property value.</param>
        /// <param name="propertiesMetadata" mayBeNull="true">Instance metadata about properties of the 'data' object.</param>
        /// <param name="context">Serialization context.</param>

        var propertyTagName = xml.qualifyXmlTagName(name, "d");
        var propertyType = propertiesMetadata && propertiesMetadata.type;
        var property;
        if (kind === propertyKind.complex) {
            property = xml.newElement(parentElement, propertyTagName, odataXmlNs);
            var propertyMetadata;
            if (propertiesMetadata) {
                propertyMetadata = propertiesMetadata.properties;
            }

            writeAtomEntryProperties(null, property, value, propertyMetadata, context);
        } else {
            // Default the conversion to string if no property type has been defined.
            property = xml.newElement(parentElement, propertyTagName, odataXmlNs, convertToAtomPropertyText(value, propertyType || "Edm.String"));
        }

        if (value === null) {
            xml.newAttribute(property, propertyNullAttribute, odataMetaXmlNs, "true");
        }

        if (propertyType) {
            xml.newAttribute(property, propertyTypeAttribute, odataMetaXmlNs, propertyType);
        }
    };

    var writeAtomEntryDeferredProperty = function (entry, kind, name, value, context) {
        /// <summary>Writes a single property for an entry or complex type.</summary>
        /// <param name="entry">Entry object being written out.</param>
        /// <param name="name" type="String">Property name.</param>
        /// <param name="kind" type="String">Property kind description (from propertyKind values).</param>
        /// <param name="value" mayBeNull="true">Property value.</param>
        /// <param name="context">Serialization context.</param>
        /// <remarks>entry cannot be null because that would indicate a complex type, which don't support links.</remarks>

        var href;
        var inlineWriter;
        var inlineType;
        if (kind === propertyKind.inline) {
            href = (value.__metadata) ? value.__metadata.uri : "";
            inlineType = payloadTypeOf(value);
            switch (inlineType) {
                case payloadType.entry:
                    inlineWriter = writeAtomEntry;
                    break;
                case payloadType.feed:
                    inlineWriter = writeAtomFeed;
                    break;
                default:
                    throw { message: "Invalid payload for inline navigation property: " + inlineType };
            }
        } else {
            href = value.__deferred.uri;
        }

        var rel = normalizeURI(name, odataRelatedPrefix);
        var link = writeAtomEntryLink(entry, href, rel);
        if (inlineWriter) {
            var inlineRoot = xml.newElement(link, inlineTag, odataMetaXmlNs);
            xml.newAttribute(link, "type", null, "application/atom+xml;type=" + inlineType);
            inlineWriter(inlineRoot, value, context);
        }
    };

    var atomParser = function (handler, text, context) {
        /// <summary>Parses an ATOM document (feed, entry or service document).</summary>
        /// <param name="handler">This handler.</param>
        /// <param name="text" type="String">Document text.</param>
        /// <param name="context" type="Object">Object with parsing context.</param>
        /// <returns>An object representation of the document; undefined if not applicable.</returns>

        if (text) {
            var atomRoot = xml.parse(text);
            if (atomRoot) {
                return readAtomDocument(atomRoot, context.metadata);
            }
        }
    };

    var atomSerializer = function (handler, data, context) {
        /// <summary>Serializes an ATOM object into a document (feed or entry).</summary>
        /// <param name="handler">This handler.</param>
        /// <param name="data" type="Object">Representation of feed or entry.</param>
        /// <param name="context" type="Object">Object with parsing context.</param>
        /// <returns>An text representation of the data object; undefined if not applicable.</returns>

        var cType = context.contentType = context.contentType || contentType(atomMediaType);
        var result = undefined;
        if (cType && cType.mediaType === atomMediaType) {
            var atomDoc = writeAtomDocument(data, context);
            result = xml.serialize(atomDoc);
        }
        return result;
    };

    odata.atomHandler = handler(atomParser, atomSerializer, atomAcceptTypes.join(","), "2.0");



    // It's assumed that all elements may have Documentation children and Annotation elements.
    // See http://msdn.microsoft.com/en-us/library/bb399292.aspx for a CSDL reference.
    var schema = {
        elements: {
            Association: {
                attributes: ["Name"],
                elements: ["End*", "ReferentialConstraint"]
            },
            AssociationSet: {
                attributes: ["Name", "Association"],
                elements: ["End*"]
            },
            CollectionType: {
                attributes: ["ElementType", "Nullable", "DefaultValue", "MaxLength", "FixedLength", "Precision", "Scale", "Unicode", "Collation"]
            },
            ComplexType: {
                attributes: ["Name", "BaseType", "Abstract"],
                elements: ["Property*"]
            },
            DefiningExpression: {
                text: true
            },
            Dependent: {
                attributes: ["Role"],
                elements: ["PropertyRef*"]
            },
            Documentation: {
                text: true
            },
            End: {
                attributes: ["Type", "Role", "Multiplicity", "EntitySet"],
                elements: ["OnDelete"]
            },
            EntityContainer: {
                attributes: ["Name", "Extends"],
                elements: ["EntitySet*", "AssociationSet*", "FunctionImport*"]
            },
            EntitySet: {
                attributes: ["Name", "EntityType"]
            },
            EntityType: {
                attributes: ["Name", "BaseType", "Abstract", "OpenType"],
                elements: ["Key", "Property*", "NavigationProperty*"]
            },
            Function: {
                attributes: ["Name", "ReturnType"],
                elements: ["Parameter*", "DefiningExpression", "ReturnType"]
            },
            FunctionImport: {
                attributes: ["Name", "ReturnType", "EntitySet"],
                elements: ["Parameter*"]
            },
            Key: {
                elements: ["PropertyRef*"]
            },
            NavigationProperty: {
                attributes: ["Name", "Relationship", "ToRole", "FromRole"]
            },
            OnDelete: {
                attributes: ["Action"]
            },
            Parameter: {
                attributes: ["Name", "Type", "Mode", "MaxLength", "Precision", "Scale"]
            },
            Principal: {
                attributes: ["Role"],
                elements: ["PropertyRef*"]
            },
            Property: {
                attributes: ["Name", "Type", "Nullable", "DefaultValue", "MaxLength", "FixedLength", "Precision", "Scale", "Unicode", "Collation", "ConcurrencyMode"]
            },
            PropertyRef: {
                attributes: ["Name"]
            },
            ReferenceType: {
                attributes: ["Type"]
            },
            ReferentialConstraint: {
                elements: ["Principal", "Dependent"]
            },
            ReturnType: {
                attributes: ["ReturnType"],
                elements: ["CollectionType", "ReferenceType", "RowType"]
            },
            RowType: {
                elements: ["Property*"]
            },
            Schema: {
                attributes: ["Namespace", "Alias"],
                elements: ["Using*", "EntityContainer*", "EntityType*", "Association*", "ComplexType*", "Function*"]
            },
            TypeRef: {
                attributes: ["Type", "Nullable", "DefaultValue", "MaxLength", "FixedLength", "Precision", "Scale", "Unicode", "Collation"]
            },
            Using: {
                attributes: ["Namespace", "Alias"]
            }
        }
    };

    // See http://msdn.microsoft.com/en-us/library/ee373839.aspx for a feed customization reference.
    var customizationAttributes = ["m:FC_ContentKind", "m:FC_KeepInContent", "m:FC_NsPrefix", "m:FC_NsUri", "m:FC_SourcePath", "m:FC_TargetPath"];
    schema.elements.Property.attributes = schema.elements.Property.attributes.concat(customizationAttributes);
    schema.elements.EntityType.attributes = schema.elements.EntityType.attributes.concat(customizationAttributes);

    // See http://msdn.microsoft.com/en-us/library/dd541284(PROT.10).aspx for an EDMX reference.
    schema.elements.Edmx = { attributes: ["Version"], elements: ["DataServices"], ns: edmxNs };
    schema.elements.DataServices = { elements: ["Schema*"], ns: edmxNs };

    // See http://msdn.microsoft.com/en-us/library/dd541233(v=PROT.10) for Conceptual Schema Definition Language Document for Data Services.
    schema.elements.EntityContainer.attributes.push("m:IsDefaultEntityContainer");
    schema.elements.Property.attributes.push("m:MimeType");
    schema.elements.FunctionImport.attributes.push("m:HttpMethod");
    schema.elements.EntityType.attributes.push("m:HasStream");
    schema.elements.DataServices.attributes = ["m:DataServiceVersion"];

    var scriptCase = function (text) {
        /// <summary>Converts a Pascal-case identifier into a camel-case identifier.</summary>
        /// <param name="text" type="String">Text to convert.</param>
        /// <returns type="String">Converted text.</returns>
        /// <remarks>If the text starts with multiple uppercase characters, it is left as-is.</remarks>

        if (!text) {
            return text;
        }

        if (text.length > 1) {
            var firstTwo = text.substr(0, 2);
            if (firstTwo === firstTwo.toUpperCase()) {
                return text;
            }

            return text.charAt(0).toLowerCase() + text.substr(1);
        }

        return text.charAt(0).toLowerCase();
    };

    var getChildSchema = function (parentSchema, candidateName) {
        /// <summary>Gets the schema node for the specified element.</summary>
        /// <param name="parentSchema" type="Object">Schema of the parent XML node of 'element'.</param>
        /// <param name="candidateName">XML element name to consider.</param>
        /// <returns type="Object">The schema that describes the specified element; null if not found.</returns>

        if (candidateName === "Documentation") {
            return { isArray: true, propertyName: "documentation" };
        }

        var elements = parentSchema.elements;
        if (!elements) {
            return null;
        }

        var i, len;
        for (i = 0, len = elements.length; i < len; i++) {
            var elementName = elements[i];
            var multipleElements = false;
            if (elementName.charAt(elementName.length - 1) === "*") {
                multipleElements = true;
                elementName = elementName.substr(0, elementName.length - 1);
            }

            if (candidateName === elementName) {
                var propertyName = scriptCase(elementName);
                return { isArray: multipleElements, propertyName: propertyName };
            }
        }

        return null;
    };

    var parseConceptualModelElement = function (element) {
        /// <summary>Parses a CSDL document.</summary>
        /// <param name="element">DOM element to parse.</param>
        /// <returns type="Object">An object describing the parsed element.</returns>

        if (!element.domNode) {
            element = xml._wrapNode(element, "");
        }

        var localName = element.localName;
        var elementSchema = schema.elements[localName];
        if (!elementSchema) {
            return null;
        }

        if (elementSchema.ns) {
            if (element.nsURI !== elementSchema.ns) {
                return null;
            }
        } else if (element.nsURI !== edmNs && element.nsURI !== edmNs2 && element.nsURI !== edmNs3) {
            return null;
        }

        var item = {};
        var attributes = elementSchema.attributes;
        if (attributes) {
            var i, len;
            for (i = 0, len = attributes.length; i < len; i++) {
                // TODO: support namespaces in attributes
                var propertyName = attributes[i];
                var colonIndex = propertyName.indexOf(":");
                var attributeNs = "";
                if (colonIndex > 0) {
                    // Currently, only m: for metadata is supported as a prefix in the schema.
                    // attributeNs = propertyName.substr(0, colonIndex);
                    attributeNs = odataMetaXmlNs;
                    propertyName = propertyName.substr(colonIndex + 1);
                }

                var attribute = xml.attribute(element, propertyName, attributeNs);
                if (attribute) {
                    item[scriptCase(propertyName)] = attribute;

                    // Feed customizations for complex types have additional
                    // attributes with a suffixed counter starting at '1'.
                    var suffixCounter = 1;
                    while (1) {
                        var suffixedPropertyName = propertyName + "_" + suffixCounter;
                        attribute = xml.attribute(element, suffixedPropertyName, attributeNs);
                        if (!attribute) {
                            break;
                        }

                        item[scriptCase(suffixedPropertyName)] = attribute;
                        suffixCounter++;
                    }
                }
            }
        }

        xml.childElements(element, function (child) {
            var childSchema = getChildSchema(elementSchema, child.localName);
            if (childSchema) {
                if (childSchema.isArray) {
                    var arr = item[childSchema.propertyName];
                    if (!arr) {
                        arr = [];
                        item[childSchema.propertyName] = arr;
                    }
                    arr.push(parseConceptualModelElement(child));
                } else {
                    item[childSchema.propertyName] = parseConceptualModelElement(child);
                }
            }
        });

        if (elementSchema.text) {
            item.text = xml.innerText(element);
        }

        return item;
    };

    var metadataParser = function (handler, text) {
        /// <summary>Parses a metadata document.</summary>
        /// <param name="handler">This handler.</param>
        /// <param name="text" type="String">Metadata text.</param>
        /// <returns>An object representation of the conceptual model.</returns>

        var doc = xml.parse(text);
        return parseConceptualModelElement(doc) || undefined;
    };

    odata.metadataHandler = handler(metadataParser, null, xmlMediaType, "1.0");



    var jsonMediaType = "application/json";

    var normalizeServiceDocument = function (data, baseURI) {
        /// <summary>Normalizes a JSON service document to look like an ATOM service document.</summary>
        /// <param name="data" type="Object">Object representation of service documents as deserialized.</param>
        /// <param name="baseURI" type="String">Base URI to resolve relative URIs.</param>
        /// <returns type="Object">An object representation of the service document.</returns>
        var workspace = { collections: [] };

        var i, len;
        for (i = 0, len = data.EntitySets.length; i < len; i++) {
            var title = data.EntitySets[i];
            var collection = {
                title: title,
                href: normalizeURI(title, baseURI)
            };

            workspace.collections.push(collection);
        }

        return { workspaces: [workspace] };
    };

    // The regular expression corresponds to something like this:
    // /Date(123+60)/
    //
    // This first number is date ticks, the + may be a - and is optional,
    // with the second number indicating a timezone offset in minutes.
    // 
    // On the wire, the leading and trailing forward slashes are
    // escaped without being required to so the chance of collisions is reduced;
    // however, by the time we see the objects, the characters already
    // look like regular forward slashes.
    var jsonDateRE = /^\/Date\((-?\d+)(\+|-)?(\d+)?\)\/$/;

    var minutesToOffset = function (minutes) {
        /// <summary>Formats the given minutes into (+/-)hh:mm format.</summary>
        /// <param name="minutes" type="Number">Number of minutes to format.</param>
        /// <returns type="String">The minutes in (+/-)hh:mm format.</returns>

        var sign;
        if (minutes < 0) {
            sign = "-";
            minutes = -minutes;
        } else {
            sign = "+";
        }

        var hours = Math.floor(minutes / 60);
        minutes = minutes - (60 * hours);

        return sign + formatNumberWidth(hours, 2) + ":" + formatNumberWidth(minutes, 2);
    };

    var parseJsonDateString = function (value) {
        /// <summary>Parses the JSON Date representation into a Date object.</summary>
        /// <param name="value" type="String">String value.</param>
        /// <returns type="Date">A Date object if the value matches one; falsy otherwise.</returns>

        var arr = value && jsonDateRE.exec(value);
        if (arr) {
            // 0 - complete results; 1 - ticks; 2 - sign; 3 - minutes
            var result = new Date(parseInt(arr[1], 10));
            if (arr[2]) {
                var mins = parseInt(arr[3], 10);
                if (arr[2] === "-") {
                    mins = -mins;
                }

                // The offset is reversed to get back the UTC date, which is
                // what the API will eventually have.
                var current = result.getUTCMinutes();
                result.setUTCMinutes(current - mins);
                result.__edmType = "Edm.DateTimeOffset";
                result.__offset = minutesToOffset(mins);
            }
            if (!isNaN(result.valueOf())) {
                return result;
            }
        }

        // Allow undefined to be returned.
    };

    // Some JSON implementations cannot produce the character sequence \/
    // which is needed to format DateTime and DateTimeOffset into the 
    // JSON string representation defined by the OData protocol.
    // See the history of this file for a candidate implementation of
    // a 'formatJsonDateString' function.

    var traverseInternal = function (item, callback) {
        /// <summary>Traverses a tree of objects invoking callback for every value.</summary>
        /// <param name="item" type="Object">Object or array to traverse.</param>
        /// <param name="callback" type="Function">
        /// Callback function with key and value, similar to JSON.parse reviver.
        /// </param>
        /// <returns type="Object">The object with traversed properties.</returns>
        /// <remarks>Unlike the JSON reviver, this won't delete null members.</remarks>

        if (item && typeof item === "object") {
            for (var name in item) {
                var value = item[name];
                var result = traverseInternal(value, callback);
                result = callback(name, result);
                if (result !== value) {
                    if (value === undefined) {
                        delete item[name];
                    } else {
                        item[name] = result;
                    }
                }
            }
        }

        return item;
    };

    var traverse = function (item, callback) {
        /// <summary>Traverses a tree of objects invoking callback for every value.</summary>
        /// <param name="item" type="Object">Object or array to traverse.</param>
        /// <param name="callback" type="Function">
        /// Callback function with key and value, similar to JSON.parse reviver.
        /// </param>
        /// <returns type="Object">The traversed object.</returns>
        /// <remarks>Unlike the JSON reviver, this won't delete null members.</remarks>

        return callback("", traverseInternal(item, callback));
    };

    var jsonParser = function (handler, text, context) {
        /// <summary>Parses a JSON OData payload.</summary>
        /// <param name="handler">This handler.</param>
        /// <param name="text">Payload text (this parser also handles pre-parsed objects).</param>
        /// <param name="context" type="Object">Object with parsing context.</param>
        /// <returns>An object representation of the OData payload.</returns>
        var metadata = context.metadata;

        var json = (typeof text === "string") ? window.JSON.parse(text) : text;
        json = traverse(json, function (key, value) {
            if (value && typeof value === "object") {
                var dataTypeName = value.__metadata && value.__metadata.type;
                var dataType = lookupEntityType(dataTypeName, metadata) || lookupComplexType(dataTypeName, metadata);

                var propertyValue;
                if (dataType) {
                    var properties = dataType.property;
                    if (properties) {
                        var i, len;
                        for (i = 0, len = properties.length; i < len; i++) {
                            var property = properties[i];
                            if (property.type === "Edm.DateTime" || property.type === "Edm.DateTimeOffset") {
                                propertyValue = value[property.name];
                                if (propertyValue) {
                                    propertyValue = parseJsonDateString(propertyValue);
                                    if (!propertyValue) {
                                        throw { message: "Invalid date/time value" };
                                    }

                                    value[property.name] = propertyValue;
                                }
                            }
                        }
                    }
                } else if (handler.recognizeDates) {
                    for (var name in value) {
                        propertyValue = value[name];
                        if (typeof propertyValue === "string") {
                            value[name] = parseJsonDateString(propertyValue) || propertyValue;
                        }
                    }
                }
            }

            return value;
        }).d;

        json = jsonUpdateDataFromVersion(json, context.dataServiceVersion);
        json = jsonNormalizeData(json, context.response.requestUri);
        return json;
    };

    var jsonSerializer = function (handler, data, context) {
        /// <summary>Serializes the data by returning its string representation.</summary>
        /// <param name="handler">This handler.</param>
        /// <param name="data">Data to serialize.</param>
        /// <param name="context" type="Object">Object with serialization context.</param>
        /// <returns type="String">The string representation of data.</returns>

        var result = undefined;
        var cType = context.contentType = context.contentType || contentType(jsonMediaType);
        if (cType && cType.mediaType === jsonMediaType) {
            var version = context.dataServiceVersion;
            var json = data;

            if (version && version !== "1.0") {
                json = { results: [data] };
            }

            // Save the current date.toJSON function
            var dateToJSON = Date.prototype.toJSON;

            try {
                // Set our own date.toJSON function
                Date.prototype.toJSON = function () {
                    return formatDateTimeOffset(this);
                };

                result = window.JSON.stringify(json);
            }
            finally {
                // Restore the original toJSON function
                Date.prototype.toJSON = dateToJSON;
            }
        }
        return result;
    };

    var jsonNormalizeData = function (data, baseURI) {
        /// <summary>
        /// Normalizes the specified data into an intermediate representation.
        /// like the latest supported version.
        /// </summary>
        /// <param name="data" optional="false">Data to update.</param>
        /// <param name="baseURI" optional="false">URI to use as the base for normalizing references.</param>

        if (payloadTypeOf(data) === payloadType.svcDoc) {
            return normalizeServiceDocument(data, baseURI);
        } else {
            return data;
        }
    };

    var jsonUpdateDataFromVersion = function (data, dataVersion) {
        /// <summary>
        /// Updates the specified data in the specified version to look
        /// like the latest supported version.
        /// </summary>
        /// <param name="data" optional="false">Data to update.</param>
        /// <param name="dataVersion" optional="true" type="String">Version the data is in (possibly unknown).</param>

        // Strip the trailing comma if there.
        if (dataVersion && dataVersion.lastIndexOf(";") === dataVersion.length - 1) {
            dataVersion = dataVersion.substr(0, dataVersion.length - 1);
        }

        if (!dataVersion) {
            // Try to detect whether this is an array, in which case it
            // should probably be a feed structure - indicates V1 behavior.
            if (isArray(data)) {
                dataVersion = "1.0";
            }
        }

        // If the data is in the latest version, there is nothing to update.
        if (dataVersion === "2.0") {
            return data;
        }

        if (dataVersion === "1.0") {
            if (isArray(data)) {
                data = { results: data };
            }
        }

        return data;
    };

    odata.jsonHandler = handler(jsonParser, jsonSerializer, jsonMediaType, "2.0");
    odata.jsonHandler.recognizeDates = false;



    var handlers = [odata.jsonHandler, odata.atomHandler];

    var dispatchHandler = function (handlerMethod, requestOrResponse, context) {
        /// <summary>Dispatches an operation to handlers.</summary>
        /// <param name="handlerMethod" type="String">Name of handler method to invoke.</param>
        /// <param name="requestOrResponse" type="Object">request/response argument for delegated call.</param>
        /// <param name="context" type="Object">context argument for delegated call.</param>

        var i, len;
        for (i = 0, len = handlers.length; i < len && !handlers[i][handlerMethod](requestOrResponse, context); i++) {
        }

        if (i === len) {
            throw { message: "no handler for data" };
        }
    };

    odata.defaultSuccess = function (data) {
        /// <summary>Default success handler for OData.</summary>
        /// <param name="data">Data to process.</param>

        window.alert(data.toString());
    };

    odata.defaultError = function (error) {
        /// <summary>Default error handler for OData.</summary>
        /// <param name="error">Error to handle.</param>

        throw error;
    };

    odata.defaultHandler = {
        read: function (response, context) {
            /// <summary>Reads the body of the specified response by delegating to JSON and ATOM handlers.</summary>
            /// <param name="response">Response object.</param>
            /// <param name="context">Operation context.</param>

            if (response && response.body !== null && response.body !== undefined && (response.headers["Content-Type"] || response.headers["content-type"])) {
                dispatchHandler("read", response, context);
            }
        },

        write: function (request, context) {
            /// <summary>Write the body of the specified request by delegating to JSON and ATOM handlers.</summary>
            /// <param name="request">Reques tobject.</param>
            /// <param name="context">Operation context.</param>

            dispatchHandler("write", request, context);
        },

        accept: "application/atomsvc+xml;q=0.8, application/json;q=0.5, */*;q=0.1"
    };

    odata.defaultMetadata = [];

    var batchMediaType = "multipart/mixed";
    var responseStatusRegex = /^HTTP\/1\.\d (\d{3}) (.*)$/i;

    var hex16 = function () {
        /// <summary>
        /// Calculates a random 16 bit number and returns it in hexadecimal format.
        /// </summary>
        /// <returns type="String">A 16-bit number in hex format.</returns>

        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substr(1);
    };

    var createBoundary = function (prefix) {
        /// <summary>
        /// Creates a string that can be used as a multipart request boundary.
        /// </summary>
        /// <param name="prefix" type="String" optional="true">String to use as the start of the boundary string</param>
        /// <returns type="String">Boundary string of the format: <prefix><hex16>-<hex16>-<hex16></returns>

        return prefix + hex16() + "-" + hex16() + "-" + hex16();
    };

    var partHandler = function (context) {
        /// <summary>
        /// Gets the handler for data serialization of individual requests / responses in a batch.
        /// </summary>
        /// <param name="context">Context used for data serialization.</param>
        /// <returns>Handler object.</returns>

        return context.handler.partHandler;
    };

    var currentBoundary = function (context) {
        /// <summary>
        /// Gets the current boundary used for parsing the body of a multipart response.
        /// </summary>
        /// <param name="context">Context used for parsing a multipart response.</param>
        /// <returns type="String">Boundary string.</returns>

        var boundaries = context.boundaries;
        return boundaries[boundaries.length - 1];
    };

    var batchParser = function (handler, text, context) {
        /// <summary>Parses a batch response.</summary>
        /// <param name="handler">This handler.</param>
        /// <param name="text" type="String">Batch text.</param>
        /// <param name="context" type="Object">Object with parsing context.</param>
        /// <returns>An object representation of the batch.</returns>

        var boundary = context.contentType.properties["boundary"];
        return { __batchResponses: readBatch(text, { boundaries: [boundary], handlerContext: context }) };
    };

    var batchSerializer = function (handler, data, context) {
        /// <summary>Serializes a batch object representation into text.</summary>
        /// <param name="handler">This handler.</param>
        /// <param name="data" type="Object">Representation of a batch.</param>
        /// <param name="context" type="Object">Object with parsing context.</param>
        /// <returns>An text representation of the batch object; undefined if not applicable.</returns>

        var cType = context.contentType = context.contentType || contentType(batchMediaType);
        if (cType.mediaType === batchMediaType) {
            return writeBatch(data, context);
        }
    };

    var readBatch = function (text, context) {
        /// <summary>
        /// Parses a multipart/mixed response body from from the position defined by the context. 
        /// </summary>
        /// <param name="text" type="String" optional="false">Body of the multipart/mixed response.</param>
        /// <param name="context">Context used for parsing.</param>
        /// <returns>Array of objects representing the individual responses.</returns>

        var delimiter = "--" + currentBoundary(context);

        // Move beyond the delimiter and read the complete batch
        readTo(text, context, delimiter);

        // Ignore the incoming line
        readLine(text, context);

        // Read the batch parts
        var responses = [];
        var partEnd;

        while (partEnd !== "--" && context.position < text.length) {
            var partHeaders = readHeaders(text, context);
            var partContentType = contentType(partHeaders["Content-Type"]);

            if (partContentType && partContentType.mediaType === batchMediaType) {
                context.boundaries.push(partContentType.properties["boundary"]);
                responses.push({ __changeResponses: readBatch(text, context) });
                context.boundaries.pop();
                readTo(text, context, "--" + currentBoundary(context));
            } else {
                if (!partContentType || partContentType.mediaType !== "application/http") {
                    throw { message: "invalid MIME part type " };
                }

                // Read the response
                var response = readResponse(text, context, delimiter);
                try {
                    if (response.statusCode >= 200 && response.statusCode <= 299) {
                        partHandler(context.handlerContext).read(response, context.handlerContext);
                    } else {
                        // Keep track of failed responses and continue processing the batch.
                        response = { message: "HTTP request failed", response: response };
                    }
                } catch (e) {
                    response = e;
                }

                responses.push(response);
            }

            partEnd = text.substr(context.position, 2);

            // Ignore the incoming line.
            readLine(text, context);
        }
        return responses;
    };

    var readHeaders = function (text, context) {
        /// <summary>
        /// Parses the http headers in the text from the position defined by the context.  
        /// </summary>
        /// <param name="text" type="String" optional="false">Text containing an http response's headers</param>
        /// <param name="context">Context used for parsing.</param>
        /// <returns>Object containing the headers as key value pairs.</returns>
        /// <remarks>
        /// This function doesn't support split headers and it will stop reading when it hits two consecutive line breaks.
        /// </remarks>

        var headers = {};
        var start;
        var line;
        while ((line = readLine(text, context))) {
            start = line.indexOf(":");
            if (start !== -1) {
                headers[trimString(line.substring(0, start))] = trimString(line.substring(start + 1));
            }
        }

        return headers;
    };

    var readResponse = function (text, context, delimiter) {
        /// <summary>
        /// Parses an HTTP response. 
        /// </summary>
        /// <param name="text" type="String" optional="false">Text representing the http response.</param>
        /// <param name="context" optional="false">Context used for parsing.</param>
        /// <param name="delimiter" type="String" optional="false">String used as delimiter of the multipart response parts.</param>
        /// <returns>Object representing the http response.</returns>

        // Read the status line. 
        var match = responseStatusRegex.exec(readLine(text, context));
        if (!match) {
            throw { message: "Invalid HTTP response" };
        }

        // Build the response object.
        return {
            statusCode: match[1],
            statusText: match[2],
            headers: readHeaders(text, context),
            body: readTo(text, context, delimiter)
        };
    };

    var readLine = function (text, context) {
        /// <summary>
        /// Returns a substring from the position defined by the context up to the next line break (CRLF).
        /// </summary>
        /// <param name="text" type="String" optional="false">Input string.</param>
        /// <param name="context" optional="false">Context used for reading the input string.</param>
        /// <returns type="String">Substring to the first ocurrence of a line break or null if none can be found. </returns>

        return readTo(text, context, "\r\n");
    };

    var readTo = function (text, context, str) {
        /// <summary>
        /// Returns a substring from the position given by the context up to value defined by the str parameter and increments the position in the context.
        /// </summary>
        /// <param name="text" optional="false">Input string.</param>
        /// <param name="context" optional="false">Context used for reading the input string.</param>
        /// <returns type="String">Substring to the first ocurrence of str or the end of the input string if str is not specified. Null if the marker is not found.</returns>

        var start = context.position || 0;
        var end = text.length;
        if (str) {
            end = text.indexOf(str, start);
            if (end === -1) {
                return null;
            }

            context.position = end + str.length;
        } else {
            context.position = end;
        }

        return text.substring(start, end);
    };

    var writeBatch = function (data, context) {
        /// <summary>
        /// Serializes a batch request object to a string.
        /// </summary>
        /// <param name="data" optional="false">Batch request object in payload representation format</param>
        /// <param name="context" optional="false">Context used for the serialization</param>
        /// <returns type="String">String representing the batch request</returns>

        var type = payloadTypeOf(data);
        if (type !== payloadType.batch) {
            throw { message: "Serialization of batches of type \"" + type + "\" is not supported" };
        }

        var batchBoundary = createBoundary("batch_");
        var batchParts = data.__batchRequests;
        var batch = "";
        var i, len;
        for (i = 0, len = batchParts.length; i < len; i++) {
            batch += writeBatchPartDelimiter(batchBoundary, false) +
                     writeBatchPart(batchParts[i], context);
        }
        batch += writeBatchPartDelimiter(batchBoundary, true);

        // Register the boundary with the request content type.
        var contentTypeProperties = context.contentType.properties;
        contentTypeProperties.boundary = batchBoundary;

        return batch;
    };

    var writeBatchPartDelimiter = function (boundary, close) {
        /// <summary>
        /// Creates the delimiter that indicates that start or end of an individual request.
        /// </summary>
        /// <param name="boundary" type="String" optional="false">Boundary string used to indicate the start of the request</param>
        /// <param name="close" type="Boolean">Flag indicating that a close delimiter string should be generated</param>
        /// <returns type="String">Delimiter string</returns>

        var result = "\r\n--" + boundary;
        if (close) {
            result += "--";
        }

        return result + "\r\n";
    };

    var writeBatchPart = function (part, context, nested) {
        /// <summary>
        /// Serializes a part of a batch request to a string. A part can be either a GET request or 
        /// a change set grouping several CUD (create, update, delete) requests.
        /// </summary>
        /// <param name="part" optional="false">Request or change set object in payload representation format</param>
        /// <param name="context" optional="false">Object containing context information used for the serialization</param>
        /// <param name="nested" type="boolean" optional="true">Flag indicating that the part is nested inside a change set</param>
        /// <returns type="String">String representing the serialized part</returns>
        /// <remarks>
        /// A change set is an array of request objects and they cannot be nested inside other change sets.
        /// </remarks>

        var changeSet = part.__changeRequests;
        var result;
        if (isArray(changeSet)) {
            if (nested) {
                throw { message: "Not Supported: change set nested in other change set" };
            }

            var changeSetBoundary = createBoundary("changeset_");
            result = "Content-Type: " + batchMediaType + "; boundary=" + changeSetBoundary + "\r\n";
            var i, len;
            for (i = 0, len = changeSet.length; i < len; i++) {
                result += writeBatchPartDelimiter(changeSetBoundary, false) +
                     writeBatchPart(changeSet[i], context, true);
            }

            result += writeBatchPartDelimiter(changeSetBoundary, true);
        } else {
            result = "Content-Type: application/http\r\nContent-Transfer-Encoding: binary\r\n\r\n";
            prepareRequest(part, partHandler(context), { metadata: context.metadata });
            result += writeRequest(part);
        }

        return result;
    };

    var writeRequest = function (request) {
        /// <summary>
        /// Serializes a request object to a string.
        /// </summary>
        /// <param name="request" optional="false">Request object to serialize</param>
        /// <returns type="String">String representing the serialized request</returns>

        var result = (request.method ? request.method : "GET") + " " + request.requestUri + " HTTP/1.1\r\n";
        for (var name in request.headers) {
            if (request.headers[name]) {
                result = result + name + ": " + request.headers[name] + "\r\n";
            }
        }

        result += "\r\n";

        if (request.body) {
            result += request.body;
        }

        return result;
    };

    odata.batchHandler = handler(batchParser, batchSerializer, batchMediaType, "1.0");

    // Configure the batch handler to use the default handler for the batch parts.
    odata.batchHandler.partHandler = odata.defaultHandler;



    odata.read = function (urlOrRequest, success, error, handler, httpClient, metadata) {
        /// <summary>Reads data from the specified URL.</summary>
        /// <param name="url" type="String">URL to read data from.</param>
        /// <param name="success">Callback for a successful read operation.</param>
        /// <param name="error">Callback for handling errors.</param>
        /// <param name="handler">Handler for data response.</param>
        /// <param name="httpClient">HTTP client layer.</param>
        /// <param name="metadata">Conceptual metadata for this request.</param>

        var request;
        if (urlOrRequest instanceof String || typeof urlOrRequest === "string") {
            request = { requestUri: urlOrRequest };
        } else {
            request = urlOrRequest;
        }

        odata.request(request, success, error, handler, httpClient, metadata);
    };

    odata.request = function (request, success, error, handler, httpClient, metadata) {
        /// <summary>Sends a request containing OData payload to a server.</summary>
        /// <param name="request">Object that represents the request to be sent.</param>
        /// <param name="success">Callback for a successful read operation.</param>
        /// <param name="error">Callback for handling errors.</param>
        /// <param name="handler">Handler for data serialization.</param>
        /// <param name="httpClient">HTTP client layer.</param>
        /// <param name="metadata">Conceptual metadata for this request.</param>

        if (!success) {
            success = odata.defaultSuccess;
        }

        if (!error) {
            error = odata.defaultError;
        }

        if (!handler) {
            handler = odata.defaultHandler;
        }

        if (!httpClient) {
            httpClient = odata.defaultHttpClient;
        }

        if (!metadata) {
            metadata = odata.defaultMetadata;
        }

        var context = { metadata: metadata };

        try {
            prepareRequest(request, handler, context);
            invokeRequest(request, success, error, handler, httpClient, context);
        } catch (err) {
            error(err);
            return;
        }
    };

})(this);
