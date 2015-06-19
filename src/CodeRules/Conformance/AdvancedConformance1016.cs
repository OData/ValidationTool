// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Net;
    using System.ComponentModel.Composition;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Advanced.Conformance.1016
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance1016 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.1016";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "16. SHOULD support a conforming OData service interface over metadata.";
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

            Uri metadataServiceUrl = new Uri(context.Destination.AbsoluteUri.TrimEnd('/') + @"/$metadata" + @"/");
            var req = WebRequest.Create(metadataServiceUrl) as HttpWebRequest;
            Response response = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name, metadataServiceUrl.AbsoluteUri, "GET", string.Empty, response);

            if (response != null && response.StatusCode == HttpStatusCode.OK && response.ResponsePayload.IsMetadata())
            {
                passed = true;
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The response is not the metadata service document. Please refer to section 15 of [OData-CSDL].";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
