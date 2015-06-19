// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Net;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.100502
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance100502 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.100502";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "5.2. OData-MaxVersion (section 8.2.7)";
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

            // If specified the service MUST generate a response with an OData-Version less than or equal to the specified OData-MaxVersion.
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("OData-MaxVersion", "4.0");
            var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, headers);
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, headers), resp);

            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                string odataVersion = resp.ResponseHeaders.GetHeaderValue(Constants.ODataVersion).Trim();
                double versionNumber = 0.0;

                try
                {
                    versionNumber = Convert.ToDouble(odataVersion);
                }
                catch (Exception)
                {
                    detail1.ErrorMessage = string.Format("Parse value {0} of OData-Version to a number failed from response header", odataVersion);
                    info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                    return false;
                }

                if (versionNumber <= 4.0)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    detail1.ErrorMessage = string.Format("The OData-Version is {0}, which should be less than or equal to the specified OData-MaxVersion 4.0.", versionNumber);
                }
            }
            else
            {
                passed = false;
                detail1.ErrorMessage = string.Format("Request failed with header 'OData-MaxVersion: 4.0'. The error is {0}.", resp.ResponsePayload.GetErrorMessage());
            }

            headers["OData-MaxVersion"] = "3.0";
            resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, headers);
            ExtensionRuleResultDetail detail2 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, headers), resp);

            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                passed = false;
                detail2.ErrorMessage = "Request should failed because set 'OData-MaxVersion:3.0' in request header and the service is version 4.0.";
            }
            else
            {
                passed = true && false != passed;
            }

            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>() { detail1, detail2 };
            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);

            return passed;
        }
    }
}
