// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4447 : ExtensionRule
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
                return "Metadata.Core.4447";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "An edm:LabeledElement expression MUST provide a SimpleIdentifier value for the Name attribute that is unique within the schema containing the expression.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "14.5.8.1";
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
                return ODataVersion.V4;
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
        /// Verify Metadata.Core.4447
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

            XmlNodeList schemaList = xmlDoc.SelectNodes("//*[local-name()='LabeledElement']/ancestor::*[local-name()='Schema']");

            foreach (XmlNode schema in schemaList)
            {
                if (schema.HasChildNodes)
                {
                    // Check if there's any duplicate name for nomimal types and elements within a namespace defined by a Schema.
                    HashSet<string> nominalTypeElementSet = new HashSet<string>(StringComparer.Ordinal);

                    foreach (XmlNode node in schema.ChildNodes)
                    {
                        if (node.Attributes == null) continue; // the comment nodes do not contain Attributes collection.
                        if (node.Attributes["Name"] == null) continue; // The node without a name is out of scope.
                        nominalTypeElementSet.Add(node.Attributes["Name"].Value);
                    }

                    XmlNodeList labeledElementList = schema.SelectNodes("//*[local-name()='LabeledElement']");

                    foreach (XmlNode node in labeledElementList)
                    {
                        if (node.Attributes == null) continue; // the comment nodes do not contain Attributes collection.
                        if (node.Attributes["Name"] == null) continue; // The node without a name is out of scope.
                        if (nominalTypeElementSet.Add(node.Attributes["Name"].Value) && VerificationHelper.IsSimpleIdentifier(node.Attributes["Name"].Value))
                        {

                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                    }
                }

                if (passed == false)
                {
                    break;
                }
            }

            return passed;
        }
    }
}
