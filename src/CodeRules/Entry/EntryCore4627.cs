// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
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
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4627
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4627 : ExtensionRule
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
                return "Entry.Core.4627";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The metadata:element element for Complex Property Collection MUST include a metadata:type attribute if the instance is of a type derived from the declared type of the property.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "7.7.1.1";
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
        /// Verify Entry.Core.4627
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
            
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.ResponsePayload);
            string metadataNs = ODataVersion.V4 == context.Version ? Constants.NSMetadata : Constants.V3NSMetadata;
            
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(@"//metadata:properties/*", ODataNamespaceManager.Instance);
            List<string> collectedComplexTypeNames = AtomSchemaHelper.GetAllComplexNameWithCollectionType(context.MetadataDocument, context.EntityTypeShortName);

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (collectedComplexTypeNames.Contains(xmlNode.LocalName) && xmlNode.HasChildNodes)
                {
                    string basedComplexTypeName = 
                        xmlNode.Attributes["type", metadataNs]
                        .Value
                        .Remove(0, 1)
                        .RemoveCollectionFlag();
                        
                    List<string> propNames = GetAllPropertyNamesFromComplexType(context.MetadataDocument, basedComplexTypeName.GetLastSegment());
                    
                    foreach (XmlElement xElem in xmlNode)
                    {
                        if (xElem.LocalName.Equals("element"))
                        {
                            var nodes = xElem.SelectNodes("./*", ODataNamespaceManager.Instance);
                            bool flag = false;

                            foreach (XmlNode node in nodes)
                            {
                                string propName = node.LocalName;

                                if ("link" == node.LocalName)
                                {
                                    if (null == node.Attributes["href"])
                                    {
                                        continue;
                                    }

                                    string hrefAttrib = node.Attributes["href"].Value;
                                    propName = hrefAttrib.Remove(0, hrefAttrib.LastIndexOf("/") + 1);
                                }

                                if (!propNames.Contains(propName))
                                {
                                    flag = true;
                                    break;
                                }
                            }

                            if (flag)
                            {
                                if (xElem.HasAttribute("type", metadataNs))
                                {
                                    string derivedComplexTypeName = xElem.GetAttribute("type", metadataNs).Remove(0, 1);
                                    if (derivedComplexTypeName == basedComplexTypeName)
                                    {
                                        continue;
                                    }

                                    if (IsDerivedComplexType(context, derivedComplexTypeName.GetLastSegment(), basedComplexTypeName))
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
                                else
                                {
                                    passed = false;
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                    break;
                                }
                            }
                        }
                    }

                    if (passed == false)
                    {
                        break;
                    }
                }
            }

            return passed;
        }

        /// <summary>
        /// Get all properties from the specified complex type.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="complexTypeName">The name of the complex type.</param>
        /// <returns>Returns all the names.</returns>
        public static List<string> GetAllPropertyNamesFromComplexType(string metadataDoc, string complexTypeName)
        {
            if (string.IsNullOrEmpty(metadataDoc) || string.IsNullOrEmpty(complexTypeName))
            {
                return null;
            }
            
            Stack<string> complexTypeNames = new Stack<string>();
            complexTypeNames.Push(complexTypeName);
            var metadata = XElement.Parse(metadataDoc);
            string pattern = "//*[local-name()='ComplexType' and @Name='{0}']";
            string xPath = string.Format(pattern, complexTypeName);
            var complexType = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            while (null != complexType && null != complexType.Attribute("BaseType"))
            {
                complexTypeName = complexType.GetAttributeValue("BaseType").GetLastSegment();
                complexTypeNames.Push(complexTypeName);
                xPath = string.Format(pattern, complexTypeName);
                complexType = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            }

            List<string> result = new List<string>();
            pattern = "//*[local-name()='ComplexType' and @Name='{0}']/*[local-name()='Property' or local-name()='NavigationProperty']";

            while (complexTypeNames.Any())
            {
                xPath = string.Format(pattern, complexTypeNames.Pop());
                var props = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                result.AddRange((from prop in props select prop.GetAttributeValue("Name")).ToList());
            }
            
            return result;               
        }

        /// <summary>
        /// whether the type is the derived ComplexType
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="derivedComplexType">The derived ComplexType value</param>
        /// <param name="baseComplexType">The base ComplexType value</param>
        public static bool IsDerivedComplexType(ServiceContext context, string derivedComplexType, string baseComplexType)
        {
            // Get EntityType and ComplexType. 
            XElement metadata = XElement.Parse(context.MetadataDocument);
            string xpath = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}' and @BaseType='{1}']", derivedComplexType, baseComplexType);
            var xElem = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

            return null != xElem;
        }
    }
}
