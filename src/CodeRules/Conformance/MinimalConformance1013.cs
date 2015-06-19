// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.1013
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1013 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1013";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "13. MAY publish metadata at $metadata according to [OData-CSDL] (section 11.1.2)";
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
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.May;
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

            string url = context.Destination.AbsoluteUri.TrimEnd('/') + @"/$metadata";
            var response = WebHelper.Get(new Uri(url), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(string.Empty, context.RequestHeaders), response);

            if (response != null && response.StatusCode != null)
            {
                // Get EntityContainer from metadata payload.
                string xpath = @"//*[local-name()='EntityContainer']";
                XElement metadata = XElement.Parse(response.ResponsePayload);
                IEnumerable<XElement> entityContainer = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
                List<string> propNames = new List<string>();

                foreach (var prop in entityContainer)
                {
                    propNames.Add(prop.Attribute("Name").Value);
                }

                if (propNames.Count > 0)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = "There is no EntityContainer in metadata document.";
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
