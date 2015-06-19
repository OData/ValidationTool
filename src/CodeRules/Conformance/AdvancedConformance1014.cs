// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Advanced.Conformance.1014
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance1014 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.1014";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "14. SHOULD support Delta change tracking.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.3";
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
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);

            var restrictions = AnnotationsHelper.GetChangeTracking(context.MetadataDocument, context.VocCapabilities);

            if (string.IsNullOrEmpty(restrictions.Item1) ||
                null == restrictions.Item2 || !restrictions.Item2.Any() ||
                null == restrictions.Item3 || !restrictions.Item3.Any())
            {
                detail.ErrorMessage = "Cannot find an entity-container or any entity-sets which support the change tracking function.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            string url = string.Format("{0}/{1}", context.ServiceBaseUri, restrictions.Item1);
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string,string>>();

            foreach (KeyValuePair<string, string> kvp in context.RequestHeaders)
            {
                requestHeaders.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value));
            }

            // Add odata.track-changes preference in request header.
            KeyValuePair<string, string> prefer = new KeyValuePair<string, string>("Prefer", "odata.track-changes");
            requestHeaders.Add(prefer);
            Response response = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, requestHeaders);
            detail = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, requestHeaders), response);

            if (response != null && !string.IsNullOrEmpty(response.ResponseHeaders))
            {
                string preferHeader = response.ResponseHeaders.GetHeaderValue("Preference-Applied");
               
                if (string.IsNullOrEmpty(preferHeader))
                {
                    passed = false;
                    detail.ErrorMessage = "The response header returned by the service does not contain 'odata.track-changes', when request with the header 'Prefer:odata.track-changes'.";
                }
                else if (preferHeader.Contains("odata.track-changes"))
                {
                    passed = true;
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The service does not support Delta change tracking.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
