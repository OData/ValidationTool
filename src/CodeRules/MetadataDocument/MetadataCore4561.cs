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
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4561 : ExtensionRule
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
                return "Metadata.Core.4561";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "When referring to nominal types, the reference MUST use Namespace-qualified name or Alias-qualified name.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "4.1";
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
        /// Verify Metadata.Core.4561
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

            List<string> xPathTypes = new List<string>()
            {
                "//*[@Type]",
                "//*[@BaseType]",
                "//*[@UnderlyingType]",
                "//*[@EntityType]",
            };


            List<string> xPathTypesContainPrefix = new List<string> ()
            {
                "//*[starts-with(@Type,'{0}')]",
                "//*[starts-with(@BaseType,'{0}')]",
                "//*[starts-with(@UnderlyingType,'{0}')]",
                "//*[starts-with(@EntityType,'{0}')]",
            };

            XElement metaXml = XElement.Parse(context.MetadataDocument);
            string xpath = string.Format(@"//*[local-name()='Schema']");
            IEnumerable<XElement> schemaElements = metaXml.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            List<string> qualifiedNamePrefixes = new List<string>() { "Edm.", "Collection(Edm." };

            foreach (XElement element in schemaElements)
            {
                AliasNamespacePair aliasNameSpace = MetadataHelper.GetAliasAndNamespace(element);

                if (!string.IsNullOrEmpty(aliasNameSpace.Alias))
                {
                    qualifiedNamePrefixes.Add(aliasNameSpace.Alias + ".");
                    qualifiedNamePrefixes.Add("Collection(" + aliasNameSpace.Alias + ".");
                }

                if (!string.IsNullOrEmpty(aliasNameSpace.Namespace))
                {
                    qualifiedNamePrefixes.Add(aliasNameSpace.Namespace + ".");
                    qualifiedNamePrefixes.Add("Collection(" + aliasNameSpace.Namespace + ".");
                }
            }

             // Add the namespace and alias of the references.
            xpath = "//*[local-name()='Reference']";
            XmlNodeList refNodeList = xmlDoc.SelectNodes(xpath);

            foreach (XmlNode reference in refNodeList)
            {
                foreach (XmlNode child in reference.ChildNodes)
                {
                    if (child.Name.Equals("edmx:Include"))
                    {
                        if (child.Attributes["Alias"]!=null)
                        {
                            qualifiedNamePrefixes.Add(child.Attributes["Alias"].Value + ".");
                            qualifiedNamePrefixes.Add("Collection(" + child.Attributes["Alias"].Value + ".");
                        }

                        if (child.Attributes["Namespace"]!=null)
                        {
                            qualifiedNamePrefixes.Add(child.Attributes["Namespace"].Value + ".");
                            qualifiedNamePrefixes.Add("Collection(" + child.Attributes["Namespace"].Value + ".");
                        }
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                string xPathForType = xPathTypes[i];
                XmlNodeList typeNodeList = xmlDoc.SelectNodes(xPathForType);
                int typeCount = typeNodeList.Count;
                int sum = 0;

                foreach (string qualifiedNamePrefix in qualifiedNamePrefixes)
                {
                    string xPathfull = string.Format(xPathTypesContainPrefix[i], qualifiedNamePrefix);
                    XmlNodeList containsPrefixNodeList = xmlDoc.SelectNodes(xPathfull);
                    sum += containsPrefixNodeList.Count;
                }

                if (typeCount == sum)
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
    }
}
