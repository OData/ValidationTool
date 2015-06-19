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
    /// Class of extension rule for Metadata.Core.4058
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4058 : ExtensionRule
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
                return "Metadata.Core.4058";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The value of the Type attribute MUST be the QualifiedName of a primitive type, complex type, or enumeration type in scope, or a collection of one of these types.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "6.1.2";
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
        /// Verify Metadata.Core.4058
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
            string xpath = @"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='EntityType' or local-name()='ComplexType']/*[local-name()='Property']";

            XmlNodeList propertyCollection = metadata.SelectNodes(xpath, ODataNamespaceManager.Instance);

            foreach (XmlNode property in propertyCollection)
            {

                if(property.Attributes["Type"]==null)
                {
                    continue;
                }

                passed = false;
                
                Regex reg = new Regex(@"^Collection(.*)$");
                Match match = reg.Match(property.Attributes["Type"].Value.Trim());


                string type = property.Attributes["Type"].Value;
                string qulifiedName = match.Success ? type.Substring(11, type.Length - 12) : property.Attributes["Type"].Value;

                if (qulifiedName.IsPrimitiveTypeName())
                { 
                    passed = true; continue; 
                }

                int dotIndex;
                if (string.IsNullOrEmpty(qulifiedName) || (dotIndex = qulifiedName.LastIndexOf('.')) < 0)
                {
                    passed = false; break;
                }

                string ns = qulifiedName.Substring(0, dotIndex);
                string shortName = qulifiedName.Substring(dotIndex + 1);

                string customTypePath = @"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='EnumType' or local-name()='ComplexType']";
                if (context.ContainsExternalSchema)
                {
                    metadata.LoadXml(context.MergedMetadataDocument);
                }
                XmlNodeList customTypes = metadata.SelectNodes(customTypePath, ODataNamespaceManager.Instance);

                foreach (XmlNode customType in customTypes)
                {
                    if (customType.ParentNode.Attributes["Namespace"] != null
                        && (customType.ParentNode.Attributes["Namespace"].Value.Equals(ns)||customType.ParentNode.Attributes["Alias"].Value.Equals(ns))
                        && customType.Attributes["Name"] != null
                        && customType.Attributes["Name"].Value.Equals(shortName))
                    {
                        passed = true; break;
                    }
                }
                
                if (!passed.HasValue || !passed.Value)
                {
                    break;
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }
    }
}

