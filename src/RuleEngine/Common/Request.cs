// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The members used for generating a odata service request in this class.  
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Gets or sets the request URL.
        /// </summary>
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
            }
        }

        /// <summary>
        /// Gets or sets the http method.
        /// </summary>
        public string HttpMethod
        {
            get
            {
                return httpMethod;
            }
            set
            {
                httpMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the Content-type.
        /// </summary>
        public string ContentType
        {
            get
            {
                return contentType;
            }
            set
            {
                contentType = value;
            }
        }

        /// <summary>
        /// Gets or sets the request payload.
        /// </summary>
        public string RequestPayload
        {
            get
            {
                return requestPayload;
            }
            set
            {
                requestPayload = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum of the request payload size.
        /// </summary>
        public int MaxPayloadSize
        {
            get
            {
                return maxPayloadSize;
            }
            set
            {
                maxPayloadSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the request headers.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> RequestHeaders
        {
            get
            {
                return requestHeaders;
            }
            set
            {
                requestHeaders = value;
            }
        }

        /// <summary>
        /// The URL of the odata service to which this request is sent.
        /// </summary>
        private string url;

        /// <summary>
        /// The method for sending a request to odata service using in HttpRequest
        /// </summary>
        private string httpMethod;

        /// <summary>
        /// The value of the Content-type HTTP header.
        /// </summary>
        private string contentType;

        /// <summary>
        /// The request data which will be posted to odata server.
        /// </summary>
        private string requestPayload;

        /// <summary>
        /// The maximum size of payload in byte.
        /// </summary>
        private int maxPayloadSize;

        /// <summary>
        /// A collection of request header to be sent out.
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> requestHeaders;
    }
}
