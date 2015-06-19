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
    /// Class of extension rule for Metadata.Core.4191
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4191 : ExtensionRule
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
                return "Metadata.Core.4191";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The value of the Alias attribute MUST be unique within the set of aliases, structural and navigation properties of the containing entity type and any of its base types.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "8.3.2";
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
        /// Verify Metadata.Core.4191
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
            string keyXpath = @"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema' and @Namespace]/*[local-name()='EntityType' and @Name]/*[local-name()='Key']/*[local-name()='PropertyRef' and @Alias]";

            XmlNodeList propertyRefCollection = metadata.SelectNodes(keyXpath);

            string completeMetadata = context.ContainsExternalSchema ? context.MergedMetadataDocument : context.MetadataDocument;

            foreach (XmlNode propertyRef in propertyRefCollection)
            {
                List<string> properties = MetadataHelper.GetAllPropertiesNamesOfStructuredType(completeMetadata,
                      propertyRef.ParentNode.ParentNode.ParentNode.Attributes["Namespace"].Value,
                      propertyRef.ParentNode.ParentNode.Attributes["Name"].Value, MatchPropertyType.All);
                if (properties.Contains(propertyRef.Attributes["Alias"].Value))
                {
                    passed = false; break;
                }
                else
                {
                    passed = true;
                }
            }

            string aliasPath = @"./*[local-name()='Reference']/*[local-name()='Include']";
            aliasPath += @" | ./*[local-name()='DataServices']/*[local-name()='Schema']";
            aliasPath += @" | ./*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='EntityType']/*[local-name()='Key']/*[local-name()='PropertyRef']";
            List<string> aliases =  MetadataHelper.GetPropertyValues(context, aliasPath, "Alias");

            if (propertyRefCollection != null && propertyRefCollection.Count > 0)
            {
                if (aliases.Count == aliases.Distinct().Count<string>())
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }
    }
}

