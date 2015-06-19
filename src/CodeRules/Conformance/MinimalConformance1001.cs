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
    /// Class of extension rule for Minimal.Conformance.1001
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1001 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1001";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "1. MUST publish a service document at the service root (section 11.1.1)";
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
                var payloadFormat = response.ResponsePayload.GetFormatFromPayload();
                var payloadType = response.ResponsePayload.GetTypeFromPayload(payloadFormat);

                if (payloadType == RuleEngine.PayloadType.ServiceDoc)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = "The response is not service document.";
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = String.Format("No response returned from above URI.");
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
