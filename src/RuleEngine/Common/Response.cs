// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region namespace

    using System.Net;

    #endregion

    /// <summary>
    /// Helper class to encapulate response information from OData servcie endpoint
    /// </summary>
    public class Response
    {
        /// <summary>
        /// constructing response object
        /// </summary>
        /// <param name="statusCode">http status code of response</param>
        /// <param name="headers">header block of response</param>
        /// <param name="payload">payload content of response</param>
        public Response(HttpStatusCode? statusCode, string headers, string payload)
        {
            this.StatusCode = statusCode;
            this.ResponseHeaders = headers;
            this.ResponsePayload = payload;
        }

        /// <summary>
        /// Http status code from server (when Http or https scheme is used)
        /// </summary>
        public HttpStatusCode? StatusCode { get; private set; }

        /// <summary>
        /// response headers from server
        /// </summary>
        public string ResponseHeaders { get; private set; }

        /// <summary>
        /// respone payload from server
        /// </summary>
        public string ResponsePayload { get; private set; }

        /// <summary>
        /// method to indicate whether a service document has just be found from server
        /// </summary>
        /// <returns>true/false</returns>
        public bool IsServiceDocument()
        {
            return this.ResponsePayload.IsAtomServiceDocument() 
                || this.ResponsePayload.IsJsonVerboseSvcDoc() 
                || this.ResponsePayload.IsJsonLightSvcDoc();
        }
    }
}
