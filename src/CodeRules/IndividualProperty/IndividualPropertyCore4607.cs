// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for IndividualProperty.Core.4607
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IndividualPropertyCore4607 : ExtensionRule
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
                return "IndividualProperty.Core.4607";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"A valid OData payload MAY consist of a collection of primitive or complex properties.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.2";
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
                return RequirementLevel.May;
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
                return true;
            }
        }

        /// <summary>
        /// Verify IndividualProperty.Core.4607
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
            // Metadata: modify PersonDatail 's Address Type to Collection(ODataDemo.Address)
            xmlDoc.LoadXml(context.ResponsePayload);
            XmlElement root = xmlDoc.DocumentElement;

            if (root.LocalName.Equals("value") && root.NamespaceURI.Equals(Constants.NSMetadata))
            {
                if (root.Attributes["context", Constants.NSMetadata] != null)
                {
                    string contextURI = root.Attributes["context", Constants.NSMetadata].Value;
                    string propertyTypeCol = contextURI.Remove(0, contextURI.IndexOf('#') + 1);

                    if (!propertyTypeCol.Contains("Collection("))
                    {
                        return null;
                    }

                    string propertyType = propertyTypeCol.Substring(propertyTypeCol.IndexOf('(') + 1, propertyTypeCol.Length - 12);
                    string propertyTypeShortName = propertyType.GetLastSegment();
                    var metadata = XElement.Parse(context.MetadataDocument);

                    // Built-in primitive types.
                    if (propertyType.Contains("Edm.") && EdmTypeManager.IsEdmSimpleType(propertyType))
                    {
                        return true;
                    }

                    // Complex type.
                    List<string> complextTypes = MetadataHelper.GetAllComplexNameFromMetadata(context.MetadataDocument);
                    if (complextTypes.Contains(propertyTypeShortName))
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                    }
                }
            }

            return passed;
        }
    }
}
