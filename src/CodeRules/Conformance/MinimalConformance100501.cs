// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.Net;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Text.RegularExpressions;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.100501
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance100501 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.100501";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "5.1. Accept (section 8.2.1)";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.1";
            }
        }

        /// <summary>
        /// Verifies the extension rule.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            string url = context.ServiceBaseUri.ToString();

            // Services MUST reject formats that specify unknown or unsupported format parameters.
            var resp = WebHelper.Get(new Uri(url), Constants.UndefinedAcceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.UndefinedAcceptHeader, context.RequestHeaders), resp);

            if (null != resp && HttpStatusCode.UnsupportedMediaType == resp.StatusCode)
            {
                passed = true;
            }
            else
            {
                passed = false;
                detail1.ErrorMessage = "The service does not return an correct result for unknown or unsupported format parameters.";
            }

            // If a media type specified in the Accept header includes a charset format parameter and the request also contains an Accept-Charset header, then the Accept-Charset header MUST be used.
            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            headers.AddRange(context.RequestHeaders);
            headers.Add(new KeyValuePair<string, string>("Accept-Charset", "utf-16"));
            resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson + ";charset=utf-8", RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail2 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson + ";charset=utf-8", context.RequestHeaders), resp);

            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                if (resp.ResponseHeaders.GetHeaderValue("Content-Type").Contains("charset=utf-16"))
                {
                    passed = true && false != passed;
                }
                else
                {
                    passed = false;
                    detail2.ErrorMessage = "If a media type specified in the Accept header includes a charset format parameter and the request also contains an Accept-Charset header, then the Accept-Charset header MUST be used.";
                }
            }
            else
            {
                passed = false;
                detail2.ErrorMessage = string.Format("The service return an unexpected HTTP status code {0}.", resp.StatusCode);
            }

            // If the media type specified in the Accept header does not include a charset format parameter, then the Content-Type header of the response MUST NOT contain a charset format parameter.
            resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail3 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                if (!resp.ResponseHeaders.GetHeaderValue("Content-Type").Contains("charset="))
                {
                    passed = true && false != passed;
                }
                else
                {
                    passed = false;
                    detail3.ErrorMessage = "If the media type specified in the Accept header does not include a charset format parameter, then the Content-Type header of the response MUST NOT contain a charset format parameter.";
                }
            }
            else
            {
                passed = false;
                detail3.ErrorMessage = string.Format("The service return an unexpected HTTP status code {0}.", resp.StatusCode);
            }

            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>() { detail1, detail2, detail3 };
            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);

            return passed;
        }
    }
}
