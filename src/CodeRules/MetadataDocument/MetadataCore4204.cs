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
    public class MetadataCore4204 : ExtensionRule
    {

        private List<AliasNamespacePair> aliasNamespacePairList = new List<AliasNamespacePair>();

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
                return "Metadata.Core.4204";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "A complex type MUST NOT introduce an inheritance cycle via the base type attribute.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "9.1.2";
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
        /// Verify Metadata.Core.4204
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
            string xpath = "//*[local-name()='Reference']";
            XmlNodeList refNodeList = xmlDoc.SelectNodes(xpath);

            // Add all included reference namespace alias pair in aliasNamespacePairList.
            foreach (XmlNode reference in refNodeList)
            {
                foreach (XmlNode child in reference.ChildNodes)
                {
                    if (child.Attributes == null) continue; // the comment nodes do not contain Attributes collection.
                    if (child.Name.Equals("edmx:Include"))
                    {
                        string namespaceString = string.Empty;
                        string aliasString = string.Empty;
                        if (child.Attributes == null) continue; // the comment nodes do not contain Attributes collection.
                        if (child.Attributes["Namespace"] != null)
                        {
                            namespaceString = child.Attributes["Namespace"].Value;
                        }

                        if (child.Attributes["Alias"] != null)
                        {
                            aliasString = child.Attributes["Alias"].Value;
                        }

                        AliasNamespacePair referenceAliasNamespace = new AliasNamespacePair(aliasString, namespaceString);

                        aliasNamespacePairList.Add(referenceAliasNamespace);
                    }
                }
            }

            XElement metaXml = XElement.Parse(context.MetadataDocument);
            xpath = "//*[local-name()='ComplexType']";
            IEnumerable<XElement> complexTypeElements = metaXml.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (XElement complexTypeElement in complexTypeElements)
            {
                passed = true;

                HashSet<string> descendantsSet = new HashSet<string>(StringComparer.Ordinal);

                AliasNamespacePair aliasNameSpace = complexTypeElement.GetAliasAndNamespace();

                if (!string.IsNullOrEmpty(aliasNameSpace.Namespace)) 
                {
                    descendantsSet.Add(aliasNameSpace.Namespace+ "." + complexTypeElement.Attribute("Name").Value);
                }

                if (!string.IsNullOrEmpty(aliasNameSpace.Alias))
                {
                    descendantsSet.Add(aliasNameSpace.Alias + "." + complexTypeElement.Attribute("Name").Value);
                }

                if (!aliasNamespacePairList.Contains(aliasNameSpace))
                {
                    aliasNamespacePairList.Add(aliasNameSpace);
                }

                if (this.IsComplextTypeBaseDeadCycled(complexTypeElement, context, ref descendantsSet))
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

            return passed;
        }

        /// <summary>
        /// See whether there is a complex type base type cycled to one of its desendants.
        /// </summary>
        /// <param name="complexTypeElement">The XElement of a complext type.</param>
        /// <param name="context">The ODataValidation Service context.</param>
        /// <param name="descendantsSet">The set of the qualified names of the </param>
        /// <returns></returns>
        private bool IsComplextTypeBaseDeadCycled(XElement complexTypeElement, ServiceContext context, ref HashSet<string> descendantsSet)
        {
            XElement baseType = null;
            string baseTypeQulifiedName = string.Empty;
            string baseTypeSimpleName = string.Empty;
            string baseTypePrefix = string.Empty;

            if(complexTypeElement.Attribute("BaseType")!=null)
            {
                baseTypeQulifiedName = complexTypeElement.Attribute("BaseType").Value;
                baseTypeSimpleName = baseTypeQulifiedName.GetLastSegment();
                baseTypePrefix = baseTypeQulifiedName.Substring(0, baseTypeQulifiedName.IndexOf(baseTypeSimpleName) - 1);
                
                baseType= MetadataHelper.GetTypeDefinitionEleByDoc("ComplexType", baseTypeQulifiedName, context.MetadataDocument);

                if(baseType == null)
                {
                    string doc = MetadataHelper.GetReferenceDocByDefinedType(baseTypeQulifiedName, context);
                    
                    if(!string.IsNullOrEmpty(doc))
                    {
                        baseType = MetadataHelper.GetTypeDefinitionEleByDoc("ComplexType", baseTypeQulifiedName, doc);
                    }
                }
            }

            if (baseType != null)
            {
                string baseTypeAnotherQualifiedName = string.Empty;

                foreach(AliasNamespacePair aliasNspair in aliasNamespacePairList)
                {
                    if (baseTypePrefix.Equals(aliasNspair.Alias) && !string.IsNullOrEmpty(aliasNspair.Namespace))
                    {
                        baseTypeAnotherQualifiedName = aliasNspair.Namespace + "." + baseTypeSimpleName;
                        break;
                    }
                    else if (baseTypePrefix.Equals(aliasNspair.Namespace) && !string.IsNullOrEmpty(aliasNspair.Alias))
                    {
                        baseTypeAnotherQualifiedName = aliasNspair.Alias + "." + baseTypeSimpleName;
                        break;
                    }
                }

                if (descendantsSet.Add(baseTypeQulifiedName)
                    && (string.IsNullOrEmpty(baseTypeAnotherQualifiedName) ? true : descendantsSet.Add(baseTypeAnotherQualifiedName)))
                {
                    return IsComplextTypeBaseDeadCycled(baseType, context, ref descendantsSet);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
