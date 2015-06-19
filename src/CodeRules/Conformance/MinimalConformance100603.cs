// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.Net;
    using System.ComponentModel.Composition;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.100603
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance100603 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.100603";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "1). Services SHOULD fail any request that contains query options that they not understand.(section 6.1)";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "6.1";
            }
        }

        /// <summary>
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Should;
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

            string url = string.Format("{0}/?$find=*", context.Destination);
            var req = WebRequest.Create(url) as HttpWebRequest;
            Response response = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail(this.Name, url, "GET", string.Empty, response);

            if (response != null && response.StatusCode != null)
            {
                
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    detail1.ErrorMessage = "Services SHOULD fail the above URI because it contains query options 'find' which services does not understand.";
                }
            }
            else
            {
                passed = false;
                detail1.ErrorMessage = "No response returned from above URI.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);
            return passed;
        }
    }
}
