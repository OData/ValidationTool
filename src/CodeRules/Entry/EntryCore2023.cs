// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2023 : ExtensionRule
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
                return "Entry.Core.2023";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "In addition to the rules stated in the table, if the value of a Primitive type is null, then the value of the associated XML element MUST be empty, and an m:null attribute with value set to true MUST be present on the containing element.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.1";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "7.3.2";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odataatom";
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
        /// Gets the version
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Entry;
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
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
            }
        }

        /// <summary>
        /// Verify rule logic
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

            // Load MetadataDocument into XMLDOM
            XmlDocument mdDoc = new XmlDocument();
            mdDoc.LoadXml(context.MetadataDocument);

            // Load payload into XMLDOM and find all properties marked m:null as true          
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.ResponsePayload);
            XmlNodeList nullPropertyList = xmlDoc.SelectNodes("//*[@*[name()='m:null'] = 'true']");

            XmlNode propertyNode;
            foreach (XmlNode node in nullPropertyList)
            {
                if (node.ParentNode.LocalName.Equals("properties", StringComparison.InvariantCultureIgnoreCase))
                {
                    propertyNode = mdDoc.SelectSingleNode("//*[local-name()='EntityType' and @Name = '" + context.EntityTypeShortName + "']/*[local-name()='Property' and @Name = '" + node.LocalName + "']");
                }
                else
                {
                    var tempNode = mdDoc.SelectSingleNode("//*[local-name()='EntityType' and @Name = '" + context.EntityTypeShortName + "']/*[local-name()='Property' and @Name = '" + node.ParentNode.LocalName + "']");
                    propertyNode = mdDoc.SelectSingleNode("//*[local-name()='ComplexType' and @Name = '" + tempNode.Attributes["Type"].Value.Split('.').Last() + "']/*[local-name()='Property' and @Name = '" + node.LocalName + "']");
                }

                if (node.InnerText == "")
                {
                    if (EdmTypeManager.IsEdmSimpleType(propertyNode.Attributes["Type"].Value))
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        break;
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}

