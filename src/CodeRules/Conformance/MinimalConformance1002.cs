// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.1002
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1002 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1002";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "2. MUST return data according to at least one of the OData defined formats (section 7)";
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
            var response = WebHelper.Get(context.Destination, string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name, context.Destination.AbsoluteUri, "GET", StringHelper.MergeHeaders(string.Empty, context.RequestHeaders), response);

            if (response != null && response.StatusCode != null)
            {
                if (context.PayloadFormat == RuleEngine.PayloadFormat.JsonLight)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = "Get above URI with accept header 'application/json', the response is not JSON format.";
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = String.Format("No response returned from above URI with accept header 'application/json'.");
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
