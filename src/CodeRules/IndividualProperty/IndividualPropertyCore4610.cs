// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    
    /// <summary>
    /// Class of extension rule for IndividualProperty.Core.4610
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IndividualPropertyCore4610 : ExtensionRule
    {
        /// <summary>
        /// Gets Category property
        /// </summary>
        public override string Category
        {
            get
            {
                return "core";
            }
        }

        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "IndividualProperty.Core.4610";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The metadata:value element MUST have a metadata:context attribute, whose value is the context URL that describes the element.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.2.1.1";
            }
        }

        /// <summary>
        /// Gets location of help information of the rule
        /// </summary>
        public override string HelpLink
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the error message for validation failure
        /// </summary>
        public override string ErrorMessage
        {
            get
            {
                return this.Description;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
            }
        }

        /// <summary>
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.IndividualProperty;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }

        /// <summary>
        /// Gets the IsOfflineContext property to which the rule applies.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the flag whether the rule requires metadata document
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Verify IndividualProperty.Core.4610
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out parameter to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            XmlDocument xmlDoc = new XmlDocument();

            // Test Data
            // 1. Collection of string. Url:http://services.odata.org/V4/OData/OData.svc/Products(0)/Description?$format=xml
            // xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><m:value m:context=\"http://services.odata.org/V4/OData/OData.svc/$metadata#Collection(Edm.String)\" xmlns:d=\"http://docs.oasis-open.org/odata/ns/data\" xmlns:georss=\"http://www.georss.org/georss\" xmlns:gml=\"http://www.opengis.net/gml\" xmlns:m=\"http://docs.oasis-open.org/odata/ns/metadata\"><m:element>Whole grain</m:element> <m:element>bread</m:element></m:value>");
            // 2. Collection of complext type. Url: http://services.odata.org/V4/OData/OData.svc/PersonDetails(0)/Address?$format=xml
            // xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><m:value m:context=\"http://services.odata.org/V4/OData/OData.svc/$metadata#Collection(ODataDemo.Address)\" m:type=\"#Collection(ODataDemo.Address)\" xmlns:d=\"http://docs.oasis-open.org/odata/ns/data\" xmlns:georss=\"http://www.georss.org/georss\" xmlns:gml=\"http://www.opengis.net/gml\" xmlns:m=\"http://docs.oasis-open.org/odata/ns/metadata\"><m:element><d:Street>2817 Milton Dr.</d:Street><d:City>Albuquerque</d:City><d:State>NM</d:State><d:ZipCode>87110</d:ZipCode><d:Country>USA</d:Country></m:element><m:element><d:Street>2817 Milton Dr.</d:Street><d:City>Detorlee</d:City><d:State>UT</d:State><d:ZipCode>124450</d:ZipCode><d:Country>USA</d:Country></m:element></m:value>");
            xmlDoc.LoadXml(context.ResponsePayload);
            XmlElement root = xmlDoc.DocumentElement;

            if (root.LocalName.Equals("value") && root.NamespaceURI.Equals(Constants.NSMetadata))
            {
                if (root.Attributes["context", Constants.NSMetadata] != null)
                {
                    string contextURL = root.Attributes["context", Constants.NSMetadata].Value;

                    string propertyTypeCol = contextURL.Remove(0, contextURL.IndexOf('#') + 1);

                    if (!propertyTypeCol.Contains("Collection("))
                    {
                        return null;
                    }

                    string propertyTypeFullName = propertyTypeCol.Substring(propertyTypeCol.IndexOf('(') + 1, propertyTypeCol.Length - 12);
                    string propertyTypeShortName = propertyTypeFullName.GetLastSegment();

                    // Verify whether the context URL is a valid url.
                    if (!Uri.IsWellFormedUriString(contextURL, UriKind.Absolute))
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                        return passed;
                    }

                    string metadataUrl = contextURL.Remove(contextURL.IndexOf('#'));

                    var resp = WebHelper.Get(new Uri(metadataUrl), Constants.AcceptHeaderAtom, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    if (null != resp && HttpStatusCode.OK == resp.StatusCode)
                    {
                        var metadata = XElement.Parse(resp.ResponsePayload);

                        if (null != metadata && "Edmx" == metadata.Name.LocalName)
                        {
                            passed = false;

                            if (propertyTypeFullName.Contains("Edm.") && EdmTypeManager.IsEdmSimpleType(propertyTypeFullName))
                            {
                                return true;
                            }
                            
                            string xpath = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}']", propertyTypeShortName);
                            XElement complexType = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
                            if (complexType != null)
                            {
                                return true;
                            }
                        }

                        if (passed == false)
                        {
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                        }
                    }
                }
            }

            return passed;
        }
    }
}
