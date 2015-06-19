// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Metadata.Core.4182
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4182 : ExtensionRule
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
                return "Metadata.Core.4182";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The properties that compose the key MUST be non-nullable and typed with an enumeration type, one of the following primitive types, or a type definition based on one of these primitive types: Edm.Byte, Edm.Date, Edm.DateTimeOffset, Edm.Decimal, Edm.Duration, Edm.Guid, Edm.Int16, Edm.Int32, Edm.Int64, Edm.SByte, Edm.String, Edm.TimeOfDay.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "8.2";
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
                return RuleEngine.PayloadType.Metadata;
            }
        }

        /// <summary>
        /// Gets the offline context to which the rule applies
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return true;
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
        /// Gets the OData version
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
            }
        }

        /// <summary>
        /// Verify Metadata.Core.4182
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

            XmlDocument metadata = new XmlDocument();

            metadata.LoadXml(context.MetadataDocument);
            string propertyRefXpath = @"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='EntityType']/*[local-name()='Key']/*[local-name()='PropertyRef' and @Name]";

            XmlNodeList propertyRefCollection = metadata.SelectNodes(propertyRefXpath);

            foreach (XmlNode propertyRef in propertyRefCollection)
            {
                XmlNode property;
                if (MetadataHelper.ResolvePropertyRefName(propertyRef.Attributes["Name"].Value, propertyRef.ParentNode.ParentNode, out property))
                {
                    if (property.Attributes["Nullable"] == null || !property.Attributes["Nullable"].Value.Equals("false")|| property.Attributes["Type"]==null)
                    {
                        passed = false; break;
                    }

                    if (MetadataHelper.IsEnumType(context.ContainsExternalSchema ? context.MergedMetadataDocument : context.MetadataDocument,
                        property.Attributes["Type"].Value)
                        || isSpecifiedPrimitiveType(property.Attributes["Type"].Value)
                        || isApplicableTypeDefinition(context.ContainsExternalSchema ? context.MergedMetadataDocument : context.MetadataDocument,
                             property.Attributes["Type"].Value))
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false; break;
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }

        private bool isSpecifiedPrimitiveType(string name)
        {
            switch (name)
            {
                case "Edm.Boolean":
                case "Edm.Byte":
                case "Edm.Date":
                case "Edm.DateTimeOffset":
                case "Edm.Decimal":
                case "Edm.Duration":
                case "Edm.Guid":
                case "Edm.Int16":
                case "Edm.Int32":
                case "Edm.Int64":
                case "Edm.SByte":
                case "Edm.String":
                case "Edm.TimeOfDay":
                    return true;
                default:
                    return false;
            }
        }
        private bool isApplicableTypeDefinition(string metadatadoc, string name)
        {
            XElement metaXml = XElement.Parse(metadatadoc);
            string xpath = string.Format(@"//*[local-name()='TypeDefinition' and @Name='{0}' and @UnderlyingType]", name);
            XElement typeDefinition = metaXml.XPathSelectElement(xpath);
            if (typeDefinition == null)
            {
                return false;
            }
            return isSpecifiedPrimitiveType(typeDefinition.Attribute("UnderlyingType").Value);
        }
    }
}

