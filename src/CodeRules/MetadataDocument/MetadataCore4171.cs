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
    /// Class of extension rule for Metadata.Core.4171
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4171 : ExtensionRule
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
                return "Metadata.Core.4171";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"An entity type derived from an open entity type MUST NOT provide a value of false for the OpenType attribute.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "8.1.4";
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
        /// Verify Metadata.Core.4171
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
            string xpath = @"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='EntityType' and @BaseType]";
            XmlNodeList entityTypeCollection = metadata.SelectNodes(xpath);

            if (context.ContainsExternalSchema)
            {
                metadata.LoadXml(context.MergedMetadataDocument);
            }

            string openTypePath = @"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema' and @Namespace]/*[local-name()='EntityType' and @Name and @OpenType='true']";
            XmlNodeList openTypeCollection = metadata.SelectNodes(openTypePath);

            foreach (XmlNode entityType in entityTypeCollection)
            {
                foreach (XmlNode openType in openTypeCollection)
                {
                    if (entityType.Attributes["BaseType"].Value.Equals(openType.ParentNode.Attributes["Namespace"].Value+"."+openType.Attributes["Name"].Value)
                        || (openType.ParentNode.Attributes["Alias"]!=null 
                            && entityType.Attributes["BaseType"].Value.Equals(openType.ParentNode.Attributes["Alias"].Value + "." + openType.Attributes["Name"].Value)))
                    {
                        if (entityType.Attributes["OpenType"] == null || entityType.Attributes["OpenType"].Value != "true")
                        {
                            passed = false;
                        }
                        else
                        {
                            passed = true; 
                        }
                        break;
                    }
                }

                if (passed.HasValue && passed == false)
                { break; }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }
    }
}

