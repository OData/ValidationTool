// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.100611
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance100611 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.100611";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "2). An OData service MUST fail any request that contains actions or functions that it does not understand.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "6.3";
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
            string selectedName = string.Empty;
            string functionactionName = "forFunctionAction";

            // Use the XPath query language to access the metadata document and get all Namespace and Alias value.
            XElement metadata = XElement.Parse(context.MetadataDocument);
            string xpath = @"//*[local-name()='DataServices']/*[local-name()='Schema']";
            List<string> appropriateNamespace = MetadataHelper.GetPropertyValues(context, xpath, "Namespace");

            // Use the XPath query language to access the metadata document and get all Function names and all Action names.
            xpath = @"//*[local-name()='EntityContainer']/*[local-name()='FunctionImport']";
            List<string> functionImportNames = MetadataHelper.GetPropertyValues(context, xpath, "Name");

            xpath = @"//*[local-name()='EntityContainer']/*[local-name()='ActionImport']";
            List<string> actionImportNames = MetadataHelper.GetPropertyValues(context, xpath, "Name");

            if (!functionImportNames.Contains(functionactionName) && !actionImportNames.Contains(functionactionName))
            {
                selectedName = functionactionName;
            }
            else
            {
                selectedName = functionactionName + "New";
            }

            string url = string.Format("{0}/?$select={1}.{2}", context.Destination, appropriateNamespace[0], selectedName);
            var req = WebRequest.Create(url) as HttpWebRequest;
            var response = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name, url, "GET", string.Empty, response);

            if (response != null && response.StatusCode != null)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = "The OData service MUST fail any request that contains actions or functions that it does not understand.";
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
