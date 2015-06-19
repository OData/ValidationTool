// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.ComponentModel.Composition;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.1004
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1004 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1004";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "4. MUST return the appropriate OData-Version header (section 8.1.5)";
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

            var response = WebHelper.Get(context.Destination, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name, context.Destination.AbsoluteUri, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), response);

            if (response != null && response.StatusCode != null)
            {
                // Extracts OData-Version value from ResponseHttpHeaders.
                string oDataVersion = context.ResponseHttpHeaders.GetHeaderValue(Constants.ODataVersion).TrimEnd(';');

                if (!string.IsNullOrEmpty(oDataVersion))
                {
                    float version = float.Parse(oDataVersion);

                    // The minimal OData version of conformance validation is 4.0.
                    if (version >= 4.0)
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        detail.ErrorMessage = string.Format(@"The OData version is {0} which should be not less than minimal version 4.0.", version);
                    }
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = @"Can not get the OData-Version header from response headers.";
                }
            }
            else
            {
                passed = null;
                detail.ErrorMessage = "Cannot get response from above URI.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
