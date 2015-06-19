// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4620
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4620 : ExtensionRule
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
                return "Entry.Core.4620";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"For non-built in primitive types, the URI may be an absolute or relative URL of metadata:type for data:[PropertyName] element containing the namespace-qualified or alias-qualified type name as a fragment.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "7.3.1";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V3_V4;
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
                return RuleEngine.PayloadType.Entry;
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
        /// Verify Entry.Core.4620
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
            List<string> xmlNodeTypes = new List<string>();

            // Whether odata.type value is namespace- or alias-qualified the instance's type
            bool isNamespaceValue = false;
            bool isAliasValue = false;

            // Use the XPath query language to access the metadata document and get all Namespace and Alias value.
            string xpath = @"//*[local-name()='DataServices']/*[local-name()='Schema']";
            List<string> appropriateNamespace = MetadataHelper.GetPropertyValues(context, xpath, "Namespace");
            List<string> appropriateAlias = MetadataHelper.GetPropertyValues(context, xpath, "Alias");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.ResponsePayload);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("m", Constants.V3NSMetadata);
            string MeataNS = Constants.V3NSMetadata;
            string DataNs = Constants.V3NSData;

            if (context.Version == ODataVersion.V4)
            {
                nsmgr.AddNamespace("m", Constants.NSMetadata);
                MeataNS = Constants.NSMetadata;
                DataNs = Constants.V4NSData;
            }
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(@"//m:properties/*", nsmgr);

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (xmlNode.NamespaceURI.Equals(DataNs) && xmlNode.Attributes["type", MeataNS] != null)
                {
                    string xmlNodeName = xmlNode.LocalName;

                    if (!AtomSchemaHelper.IsBuiltInPrimitiveTypes(xmlNodeName, context, out xmlNodeTypes))
                    {
                        string metadataTypeValue = xmlNode.Attributes["type", MeataNS].Value;

                        if (appropriateNamespace.Count != 0)
                        {
                            // Verify the annoatation start with namespace.
                            foreach (string currentvalue in appropriateNamespace)
                            {
                                if (metadataTypeValue.Contains(currentvalue))
                                {
                                    isNamespaceValue = true;
                                    break;
                                }
                                else
                                {
                                    isNamespaceValue = false;
                                }
                            }
                        }

                        if (appropriateAlias.Count != 0)
                        {
                            // Verify the annoatation start with alias.
                            foreach (string currentvalue in appropriateAlias)
                            {
                                if (metadataTypeValue.Contains(currentvalue))
                                {
                                    isAliasValue = true;
                                    break;
                                }
                                else
                                {
                                    isAliasValue = false;
                                }
                            }
                        }

                        if (isNamespaceValue || isAliasValue)
                        {
                            passed = true;
                            continue;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                    }
                }
            }

            return passed;
        }
    }
}
