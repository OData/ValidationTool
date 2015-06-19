// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Helper class to process atom schema
    /// </summary>
    static class AtomSchemaHelper
    {
        /// <summary>
        /// Whether the property is built-in primitive types
        /// </summary>
        /// <param name="xmlNodeName">The xmlNode Name</param>
        /// <param name="context">The Interop service context</param>
        /// <param name="xmlNodeTypes">The xmlNode Type</param>
        public static bool IsBuiltInPrimitiveTypes(string xmlNodeName, ServiceContext context, out List<string> xmlNodeTypes)
        {
            bool isBuiltInPrimitiveTypes = false;
            xmlNodeTypes = new List<string>();
            List<string> appropriateProperty = new List<string>();
            string primitiveTypeXpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", context.EntityTypeShortName);

            // Use the XPath query language to access the metadata document and get all specified value.
            XElement metadata = XElement.Parse(context.MetadataDocument);
            var properties = metadata.XPathSelectElements(primitiveTypeXpath, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                // Whether the specified property exist.
                if (property.Attribute("Type") != null && property.Attribute("Type").Value.Contains("Edm."))
                {
                    // Get the Type attribute value and convert its value to string.
                    if (xmlNodeName.Equals(property.Attribute("Name").Value))
                    {
                        isBuiltInPrimitiveTypes = true;
                        string specifiedValue = property.Attribute("Type").Value;
                        xmlNodeTypes.Add(specifiedValue);
                    }
                }
            }

            return isBuiltInPrimitiveTypes;
        }

        /// <summary>
        /// Get all names with the Collection Type.
        /// </summary>
        /// <param name="MetadataDocument">The Metadata Document</param>
        /// <param name="entityTypeShortName">The entityType ShortName</param>
        public static List<string> GetAllNameWithCollectionType(string MetadataDocument, string entityTypeShortName)
        {
            List<string> collectionName = new List<string>();

            // Get EntityType and ComplexType. 
            XElement metadata = XElement.Parse(MetadataDocument);
            string xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", entityTypeShortName);
            var properties = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            string pattern = string.Format(Constants.ImmutableCollectionRegexPattern);
            Regex regex = new Regex(pattern);
            foreach (var property in properties)
            {
                if (regex.IsMatch(property.Attribute("Type").Value))
                {
                    collectionName.Add(property.Attribute("Name").Value);
                }
            }

            return collectionName;
        }

        /// <summary>
        /// Get all names with the Primitive Collection Type.
        /// </summary>
        /// <param name="MetadataDocument">The Metadata Document</param>
        /// <param name="entityTypeShortName">The entityType ShortName</param>
        public static List<string> GetAllPrimitiveNameWithCollectionType(string MetadataDocument, string entityTypeShortName)
        {
            List<string> complexTypeNames = new List<string>();
            List<string> collectionPrimitiveName = new List<string>();

            string pattern = string.Format(Constants.ImmutableCollectionRegexPattern);
            Regex regex = new Regex(pattern);

            // Get EntityType and ComplexType. 
            XElement metadata = XElement.Parse(MetadataDocument);
            string xpath = @"//*[local-name()='ComplexType']";
            var properties = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                complexTypeNames.Add(property.Attribute("Name").Value.ToString());
            }

            xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", entityTypeShortName);
            properties = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (var property in properties)
            {
                if (regex.IsMatch(property.Attribute("Type").Value) && !complexTypeNames.Contains(property.Attribute("Name").Value))
                {
                    collectionPrimitiveName.Add(property.Attribute("Name").Value);
                }
            }

            return collectionPrimitiveName;
        }

        /// <summary>
        /// Get all names with the Complex Collection Type.
        /// </summary>
        /// <param name="MetadataDocument">The Metadata Document</param>
        /// <param name="entityTypeShortName">The entityType ShortName</param>
        public static List<string> GetAllComplexNameWithCollectionType(string MetadataDocument, string entityTypeShortName)
        {
            List<string> complexTypeNames = new List<string>();
            List<string> collectionPrimitiveName = new List<string>();

            string pattern = string.Format(Constants.ImmutableCollectionRegexPattern);
            Regex regex = new Regex(pattern);

            // Get EntityType and ComplexType. 
            XElement metadata = XElement.Parse(MetadataDocument);
            string xpath = @"//*[local-name()='ComplexType']";
            var properties = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                complexTypeNames.Add(property.Attribute("Name").Value.ToString());
            }

            xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", entityTypeShortName);
            properties = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (var property in properties)
            {
                if (regex.IsMatch(property.Attribute("Type").Value) && 
                    property.Attribute("Type").Value.ContainsIn(complexTypeNames))
                {
                    collectionPrimitiveName.Add(property.Attribute("Name").Value);
                }
            }

            return collectionPrimitiveName;
        }

        /// <summary>
        /// Verify whether the the namespace-qualified or alias-qualified element type enclosed in parentheses and prefixed with Collection.
        /// </summary>
        /// <param name="typeName">The type name value.</param>
        /// <returns>Returns the result of verification.</returns>
        public static bool IsNamespaceOrAliasInCollection(string typeName)
        {
            bool result = false;

            if (null == typeName || string.Empty == typeName)
            {
                return result;
            }

            string pattern = string.Format(Constants.ImmutableCollectionRegexPattern);
            Regex regex = new Regex(pattern);

            if (regex.IsMatch(typeName))
            {
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}
