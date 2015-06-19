// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Entry.Core.2018
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2018 : ExtensionRule
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
                return "Entry.Core.2018";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "A ComplexType property on an EntityType MUST be serialized within the <m:properties> element of an <atom:content> element or <atom:entry> element, as specified in Entity Type (as an Atom Entry Element) (section 2.2.6.2.2).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.2.3";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "9.1";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odatacsdl";
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
        /// Gets the version.
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
        /// Gets the offline context to which the rule applies
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule applies to proected response or not
        /// </summary>
        public override bool? Projection
        {
            get
            {
                return false; ;
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
        /// Verify Entry.Core.2018
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
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.MetadataDocument);

            // Get all the properties
            XmlNodeList propertyNodeList = xmlDoc.SelectNodes("//*[local-name()='EntityType' and @Name = '" + context.EntityTypeShortName + "']/*[local-name()='Property']");
            List<string> complexTypeProperties = new List<string>();

            // Get all Complex Type properties
            foreach (XmlNode node in propertyNodeList)
            {
                if (!EdmTypeManager.IsEdmSimpleType(node.Attributes["Type"].Value))
                {
                    complexTypeProperties.Add(node.Attributes["Type"].Value);
                }
            }

            if (complexTypeProperties.Count > 0)
            {
                var complexTypePropertyList = complexTypeProperties.GroupBy(c => c);

                XmlDocument xmlDoc2 = new XmlDocument();
                xmlDoc2.LoadXml(context.ResponsePayload);
                XmlNodeList nodeList;

                // Verify that all Complex Type properties are under m:properties
                foreach (var complexTypeProperty in complexTypePropertyList)
                {
                    nodeList = xmlDoc2.SelectNodes("//*[@*[local-name()='type'] = '" + complexTypeProperty.Key + "']");

                    foreach (XmlNode node in nodeList)
                    {
                        if (node.ParentNode.Name.Equals("m:properties", StringComparison.Ordinal))
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            break;
                        }
                    }

                    if (passed.HasValue && !passed.Value)
                    {
                        break;
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}

