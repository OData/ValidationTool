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
using System.Net;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4103 : ExtensionRule
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
                return "Metadata.Core.4103";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The value of the type attribute MUST resolve to an entity type or a collection of an entity type declared in the same document or a document referenced with an edmx:Reference element, or the abstract type Edm.EntityType.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "7.1.2";
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
                return false;
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
        /// Verify Metadata.Core.4103
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

            string xpath = "//*[local-name()='NavigationProperty']";
            XmlNodeList navPropertyNodeList = xmlDoc.SelectNodes(xpath);

            foreach (XmlNode navProp in navPropertyNodeList)
            {
                if (navProp.Attributes["Type"] != null)
                {
                    string navPropTypeName = navProp.Attributes["Type"].Value;
                    if (navPropTypeName.Contains("Collection("))
                    {
                        navPropTypeName = navPropTypeName.Substring(11, navPropTypeName.Length - 12);
                    }

                    // 1. See whether the navigation type is of Edm.EntityType.
                    if (navPropTypeName.Equals("Edm.EntityType"))
                    {
                        passed = true;
                        continue;
                    }

                    // 2. See whether the navigation type can resolve to an entity type defined in the metadata document.
                    string navPropTypeSimpleName = navPropTypeName.GetLastSegment();
                    string navPropTypePrefix = navPropTypeName.Substring(0, navPropTypeName.IndexOf(navPropTypeSimpleName) - 1);

                    bool isTypeDefinedInDoc = IsEntityTypeDefinedInDoc(navPropTypeName, context.MetadataDocument);

                    if (isTypeDefinedInDoc)
                    {
                        passed = true;
                        continue;
                    }

                    // 3. See whether the navigation type can resolve to an entity type defined in one of the referenced data model.
                    string docString = MetadataHelper.GetReferenceDocByDefinedType(navPropTypeName, context);

                    if (!string.IsNullOrEmpty(docString))
                    {
                        isTypeDefinedInDoc = false;
                        isTypeDefinedInDoc = IsEntityTypeDefinedInDoc(navPropTypeName, docString);

                        if (isTypeDefinedInDoc)
                        {
                            passed = true;
                        }
                    }
                }

                // If the navigation type cannot resolve to an in-scope entity type, this navigation property failed in this rule.
                if (passed != true)
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                    break;
                }

            }

            return passed;
        }

        /// <summary>
        /// See whether an entity type is defined in one of the schemas in the metadata document.
        /// </summary>
        /// <param name="typeFullName">The qualified name of the entity type.</param>
        /// <param name="document">The document string.</param>
        /// <returns>True if the entity type is defined in the document, false otherwise.</returns>
        bool IsEntityTypeDefinedInDoc(string typeFullName, string document)
        {
            bool result = false;
            string typeSimpleName = typeFullName.GetLastSegment();
            XElement metaXml = XElement.Parse(document);
            string xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", typeSimpleName);
            IEnumerable<XElement> types = metaXml.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (XElement type in types)
            {
                AliasNamespacePair aliasAndNamespace = MetadataHelper.GetAliasAndNamespace(type);

                if (
                    string.Format("{0}.{1}", aliasAndNamespace.Alias, typeSimpleName) == typeFullName ||
                    string.Format("{0}.{1}", aliasAndNamespace.Namespace, typeSimpleName) == typeFullName
                  )
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}

