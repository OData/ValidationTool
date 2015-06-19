// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using ODataValidator.RuleEngine.Common;
    using System;
    #endregion

    /// <summary>
    /// Class of test result detail of extension rule violating information
    /// </summary>
    public sealed class ExtensionRuleResultDetail
    {

        public ExtensionRuleResultDetail()
        {
        }

        public ExtensionRuleResultDetail(string ruleName)
        {
            this.RuleName = ruleName;
        }

        /// <summary>
        /// Creates an instance of ExtensionRuleResultDetail.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="uri"></param>
        /// <param name="httpMethod"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="requestData"></param>
        /// <param name="responseStatusCode"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="responsePayload"></param>
        /// <param name="errorMessage"></param>
        public ExtensionRuleResultDetail(string ruleName, string uri, string httpMethod, string requestHeaders, string responseStatusCode = "", string responseHeaders = "",  string responsePayload = "", string errorMessage = "", string requestData = "")
        {
            this.RuleName = ruleName;
            this.URI = uri;
            this.HTTPMethod = httpMethod;
            this.RequestHeaders = requestHeaders;
            this.RequestData = requestData;
            this.ResponseStatusCode = responseStatusCode;
            this.ResponseHeaders = responseHeaders;
            this.ResponsePayload = responsePayload;
            this.ErrorMessage = errorMessage;
        }

        public ExtensionRuleResultDetail(string ruleName, string uri, string httpMethod, string requestHeaders, Response response, string errorMessage = "", string requestData = "")
        {
            this.RuleName = ruleName;
            this.URI = uri;
            this.HTTPMethod = httpMethod;
            this.RequestHeaders = requestHeaders;
            this.RequestData = requestData;
            this.ResponseStatusCode = response != null && response.StatusCode.HasValue ? response.StatusCode.Value.ToString() : "";
            this.ResponseHeaders = string.IsNullOrEmpty(response.ResponseHeaders) ? "" : response.ResponseHeaders;
            this.ResponsePayload = string.IsNullOrEmpty(response.ResponsePayload) ? "" : response.ResponsePayload;
            this.ErrorMessage = errorMessage;
        }

        public ExtensionRuleResultDetail Clone()
        {
            ExtensionRuleResultDetail desDetail = new ExtensionRuleResultDetail();
            desDetail.RuleName = this.RuleName;
            desDetail.URI = this.URI;
            desDetail.HTTPMethod = this.HTTPMethod;
            desDetail.RequestHeaders = this.RequestHeaders;
            desDetail.RequestData = this.RequestData;
            desDetail.ResponseStatusCode = this.ResponseStatusCode;
            desDetail.ResponseHeaders = this.ResponseHeaders;
            desDetail.ResponsePayload = this.ResponsePayload;
            desDetail.ErrorMessage = this.ErrorMessage;          
            
            return desDetail;
        }

        /// <summary>
        /// Gets the name of the rule.
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// Gets the URI of the request.
        /// </summary>
        public string URI { get; set; }

        /// <summary>
        /// Gets the HTTP method of the request. 
        /// </summary>
        public string HTTPMethod { get; set; }

        /// <summary>
        /// Gets the payload of request. 
        /// </summary>
        public string RequestData{ get; set; }

        /// <summary>
        /// Gets the header of request. 
        /// </summary>
        public string RequestHeaders{ get; set; }

        /// <summary>
        /// Gets the status code of response. 
        /// </summary>
        public string ResponseStatusCode{ get; set; }

        /// <summary>
        /// Gets the headers of response. 
        /// </summary>
        public string ResponseHeaders{ get; set; }

        /// <summary>
        /// Gets the payload of response. 
        /// </summary>
        public string ResponsePayload { get; set; }

        /// <summary>
        /// Gets the error message. 
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
