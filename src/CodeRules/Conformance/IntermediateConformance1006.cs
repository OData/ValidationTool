// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.Net;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Intermediate.Conformance.1006
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IntermediateConformance1006 : ConformanceIntermediateExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Intermediate.Conformance.1006";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "6. MUST support /$value on media entities (section 4.10. in [OData-URL]) and individual properties (section 11.2.3.1)";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.2";
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
            info = null;
            var serviceStatus = ServiceStatus.GetInstance();
            var detail = new ExtensionRuleResultDetail(this.Name);
            string[] entitySetUrls = ContextHelper.GetFeeds(serviceStatus.ServiceDocument, this.PayloadFormat.Value).ToArray();
            string entitySetName = entitySetUrls.First().MapEntitySetURLToEntitySetName();
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();
            string url = string.Format("{0}/{1}", serviceStatus.RootURL, entitySetName);
            Response response = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);
            detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, string.Empty, response);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                detail.ErrorMessage = "The service does not have any entity.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            JObject feed;
            response.ResponsePayload.TryToJObject(out feed);
            var entries = JsonParserHelper.GetEntries(feed);
            var entry = entries.First();
            var entryUrl = string.Empty;
            if (null != entry[Constants.V4OdataId])
            {
                entryUrl = entry[Constants.V4OdataId].ToString().TrimEnd('\\');
            }
            else
            {
                detail.ErrorMessage = "Cannot get the entity-id from the current entity.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            // Use the XPath query language to access the metadata document and get all NavigationProperty.
            XElement metadata = XElement.Parse(serviceStatus.MetadataDocument);
            string xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", entityTypeShortName);
            IEnumerable<XElement> props = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            List<string> propNames = new List<string>();
            foreach (var prop in props)
            {
                propNames.Add(prop.Attribute("Name").Value);
            }

            url = string.Format("{0}/{1}/$value", entryUrl, propNames[0]);
            response = WebHelper.Get(WebRequest.Create(url), RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
            detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, string.Empty, response);

            // Get the value of propNames[0] property in entry payload and verify whether this value is equal to /$value payload value.
            if (response.StatusCode == HttpStatusCode.OK && entry[propNames[0]].ToString().Equals(response.ResponsePayload))
            {
                passed = true;
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The service does not support '/$value' segment.";
            }

            info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);
            return passed;
        }
    }
}
