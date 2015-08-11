// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Net;
    #endregion

    /// <summary>
    /// Helper class to encapsulate metadata parser
    /// </summary>
    public static class MetadataHelper
    {
        /// <summary>
        /// Extension method to convert a string to JSON JObject object
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="specifiedPropretyName"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static List<string> GetPropertyValues(ServiceContext context, string xpath, string specifiedPropretyName)
        {
            List<string> appropriateValues = new List<string>();

            // Use the XPath query language to access the metadata document and get all specified value.
            XElement metadata = XElement.Parse(context.MetadataDocument);
            var properties = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                // Whether the specified property exist.
                if (property.Attribute(specifiedPropretyName) != null)
                {
                    // Get the Type attribute value and convert its value to string.
                    string specifiedVlaue = property.Attribute(specifiedPropretyName).Value;
                    appropriateValues.Add(specifiedVlaue);
                }
            }

            return appropriateValues;
        }

        /// <summary>
        /// Get Property Values and Types
        /// </summary>
        /// <param name="PropName">The xmlNode Name</param>
        /// <param name="context">The Interop service context</param>
        /// <param name="PropsType">The Prop Type</param>
        public static bool IsPropsExistInMetadata(string PropName, ServiceContext context, out string PropsType)
        {
            bool isJPropsExistInMetadata = false;
            PropsType = string.Empty;
            List<string> appropriateProperty = new List<string>();
            string primitiveTypeXpath = string.Format(@"//*[local-name()='EntityType' or local-name()='ComplexType'and @Name='{0}']/*[local-name()='Property']", context.EntityTypeShortName);

            // Use the XPath query language to access the metadata document and get all specified value.
            XElement metadata = XElement.Parse(context.MetadataDocument);
            var properties = metadata.XPathSelectElements(primitiveTypeXpath, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                // Whether the specified property exist.
                if (property.Attribute("Type") != null)
                {
                    // Get the Type attribute value and convert its value to string.
                    if (PropName.Equals(property.Attribute("Name").Value))
                    {
                        isJPropsExistInMetadata = true;
                        string specifiedValue = property.Attribute("Type").Value.ToString();
                        PropsType = specifiedValue;
                    }
                }
            }

            return isJPropsExistInMetadata;
        }

        /// <summary>
        /// Get specified Property Name from metadata. 
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="xpath">The xpath</param>
        /// <param name="specifiedAttributeName">The specified attribute name</param>
        /// <param name="specifiedAttributeValue">The specified attribute value</param>
        /// <returns></returns>
        public static List<string> GetPropertyName(ServiceContext context, string xpath, string specifiedAttributeName, string specifiedAttributeValue)
        {
            List<string> appropriateValues = new List<string>();

            // Use the XPath query language to access the metadata document and get all specified value.
            XElement metadata = XElement.Parse(context.MetadataDocument);
            var properties = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                // Whether the specified property exist.
                if (property.Attribute(specifiedAttributeName) != null && property.Attribute(specifiedAttributeName).Value.Equals(specifiedAttributeValue))
                {
                    // Get the Type attribute value and convert its value to string.
                    string specifiedVlaue = property.Attribute("Name").Value;
                    appropriateValues.Add(specifiedVlaue);
                }
            }

            return appropriateValues;
        }

        /// <summary>
        /// Gets all the complex name from metadata.
        /// </summary>
        /// <param name="metadata">Store the metadata information.</param>
        /// <returns>Returns all the complex names from the metadata.</returns>
        public static List<string> GetAllComplexNameFromMetadata(string metadata)
        {
            List<string> result = new List<string>();

            if (metadata == null || metadata == string.Empty)
            {
                return result;
            }

            XElement metadataRoot = XElement.Parse(metadata);
            string xPath = @"//*[local-name()='ComplexType']";
            IEnumerable<XElement> complexTypes = metadataRoot.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

            foreach (var complexType in complexTypes)
            {
                result.Add(complexType.Attribute("Name").Value);
            }

            return result;
        }

        /// <summary>
        /// Get specified properties with complex type.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="entityTypeShortName">The short name of entity type.</param>
        /// <param name="isCollection">Indicates whether the type is a collection or not. 
        /// Collection: true.
        /// Non collection: false.
        /// Both: null</param>
        /// <returns>Returns all the appropriate properties with complex type.</returns>
        public static IEnumerable<XElement> GetComplexTypeProperties(string metadataDoc, string entityTypeShortName, bool? isCollection)
        {
            if (!metadataDoc.IsXmlPayload() || string.IsNullOrEmpty(entityTypeShortName))
            {
                return new List<XElement>();
            }

            List<XElement> result = new List<XElement>();
            var complexTypes = MetadataHelper.GetAllComplexNameFromMetadata(metadataDoc);

            XElement metadata = XElement.Parse(metadataDoc);
            string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", entityTypeShortName);
            var props = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

            if (null != props)
            {
                foreach (var p in props)
                {
                    if (null != p.Attribute("Type"))
                    {
                        var originalPropType = p.Attribute("Type").Value;
                        var propType = originalPropType.RemoveCollectionFlag().GetLastSegment();

                        if (complexTypes.Contains(propType))
                        {
                            bool? flag = null;
                            if (originalPropType.StartsWith("Collection("))
                            {
                                flag = true;
                            }
                            else if (!originalPropType.Contains("Collection"))
                            {
                                flag = false;
                            }

                            if (flag == isCollection)
                            {
                                result.Add(p);
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Judge whether the entity is media type
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="entityTypeShortName">The name of entity type.</param>
        /// <returns>True, if the entity is media type; false, otherwise. </returns>
        public static bool IsMediaEntity(string metadata, string entityTypeShortName)
        {
            string xpath1 = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            XElement md = XElement.Parse(metadata);
            XElement entity = md.XPathSelectElement(xpath1, ODataNamespaceManager.Instance);
            string HasStream = entity.GetAttributeValue(@"HasStream");

            if (HasStream != null && HasStream.ToLower() == "true")
            {
                return true;
            }                
            
            return false;            
        }

        /// <summary>
        /// Judge whether the entity is media type by thoroughly checking all its base ancestor entity types.
        /// </summary>
        /// <param name="metadata">The string of metadata document.</param>
        /// <param name="entityTypeShortName">The name of entity type.</param>
        /// <param name="context">The service context.</param>
        /// <returns> True, if the entity is media type; false, otherwise. </returns>
        public static bool IsMediaEntity(string metadata, string entityTypeShortName, ServiceContext context)
        {
            string xpath1 = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            XElement md = XElement.Parse(metadata);
            XElement entity = md.XPathSelectElement(xpath1, ODataNamespaceManager.Instance);
            string HasStream = entity.GetAttributeValue(@"HasStream");

            if (HasStream != null)
            {
                if (HasStream.ToLower() == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            while (null != entity.Attribute("BaseType") && null == entity.GetAttributeValue(@"HasStream"))
            {
                string baseEntityQualifiedName = entity.GetAttributeValue("BaseType");
                entity = GetTypeDefinitionEleInScope("EntityType", baseEntityQualifiedName, context);
                HasStream = entity.GetAttributeValue(@"HasStream");
                if (HasStream != null)
                {
                    if (HasStream.ToLower() == "true")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Find all the entity types that has a property which is of Edm.Stream. Note: Not condider base type, the scenairo that Entity's Edm.Stream property is in its base type.
        /// </summary>
        /// <param name="metadata">The string of metadata document.</param>
        /// <returns>A dictionary that has enity type short name as key, and relative property path as value.</returns>
        public static Dictionary<string, string> StreamPropertyEntities(string metadata)
        {
            Dictionary<string, string> entityAndpaths = new Dictionary<string, string>();
            string relPath = string.Empty;
            FindEntityTypeHavingStreamProp(metadata, "Edm.Stream", ref entityAndpaths, ref relPath);
            return entityAndpaths;
        }

        /// <summary>
        /// Find all the entity types that has a property which is of Edm.Stream. Recursively sorting the complex types.
        /// Note: Not condider base type, the scenairo that Entity's Edm.Stream property is in its base type.
        /// </summary>
        /// <param name="metadata">The string of metadata document.</param>
        /// <param name="type">The type value of Edm.Stream or the complex type short name begining with a dot.</param>
        /// <param name="entityAndpaths">The dictionary of entity type short name and property relative path.</param>
        /// <param name="relPath">The temporary relative path.</param>
        private static void FindEntityTypeHavingStreamProp(string metadata, string type, ref Dictionary<string, string> entityAndpaths, ref string relPath)
        {
            Dictionary<string, string> entityPath = new Dictionary<string, string>();
            string xpath = string.Format(@"//*[local-name()='Property' and contains(@Type,'{0}')]", type);
            XElement md = XElement.Parse(metadata);
            IEnumerable<XElement> propElems = md.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            string complexTypeName = string.Empty;

            foreach (XElement propEle in propElems)
            {
                relPath = propEle.Attribute("Name").Value + "/" + relPath;

                if (propEle.Parent.Name.LocalName.Equals("EntityType"))
                {
                    if (!entityAndpaths.Keys.Contains(propEle.Parent.Attribute("Name").Value)) 
                    {
                        /// Ignore the Entity that has two or more Edm.Stream properties. 
                        /// The test code here only pick the first stream property to sample test.
                        entityAndpaths.Add(propEle.Parent.Attribute("Name").Value, relPath);
                    }
                }
                else if (propEle.Parent.Name.LocalName.Equals("ComplexType"))
                {
                    complexTypeName = propEle.Parent.Attribute("Name").Value;

                    FindEntityTypeHavingStreamProp(metadata, "." + complexTypeName, ref entityAndpaths, ref relPath);
                }

                int length = relPath.Length;
                int propNameLenth = (propEle.Attribute("Name").Value + "/").Length;
                if (length >= propNameLenth)
                {
                    relPath = relPath.Substring(propNameLenth);
                }
            }

            return;
        }

        /// <summary>
        /// get all streamtype element from an Entry and ComplexType metadata. BaseClass will be searched
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="entityTypeShortName">The name of entity type.</param>
        /// <returns> List of result </returns>
        public static List<XElement> GetAllStreamTypeFromEntry(string metadata, string entityTypeShortName)
        {
            var complexTypeNames = MetadataHelper.GetAllComplexNameFromMetadata(metadata);

            Queue<XElement> complexTypeEnumQueue = new Queue<XElement>();
            List<XElement> streamTypeResultList = new List<XElement>();

            string xpath1 = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            XElement md = XElement.Parse(metadata);
            XElement entity = md.XPathSelectElement(xpath1, ODataNamespaceManager.Instance);
            complexTypeEnumQueue.Enqueue(entity);

            while (complexTypeEnumQueue.Count != 0)
            {
                XElement complexTypeOpen = complexTypeEnumQueue.Dequeue();

                string baseTypeFullName = entity.Attribute(@"BaseType") != null ? entity.Attribute(@"BaseType").Value : string.Empty;

                if (baseTypeFullName != string.Empty)
                {
                    string ns = entity.Parent.Attribute("Namespace").Value;
                    string baseTypeShortName =
                        baseTypeFullName.Contains(@".") ?
                        baseTypeFullName.Remove(0, ns.Length + 1) :
                        baseTypeFullName;

                    xpath1 = string.Format(@"//*[local-name()='{0}' and @Name='{1}']", complexTypeOpen.Name.LocalName, baseTypeFullName);

                    var baseXElement = md.XPathSelectElement(xpath1, ODataNamespaceManager.Instance);
                    complexTypeEnumQueue.Enqueue(baseXElement);
                }

                IEnumerable<XElement> openedResult = null;

                if (complexTypeOpen.Name.LocalName == "EntityType")
                {
                    openedResult = MetadataHelper.GetAllPropertiesOfEntity(metadata, complexTypeOpen.GetAttributeValue("Name"), MatchPropertyType.Normal);
                }
                else
                {
                    string xpath2 = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}']/*[local-name()='Property']", complexTypeOpen.GetAttributeValue("Name"));
                    openedResult = complexTypeOpen.XPathSelectElements(xpath2, ODataNamespaceManager.Instance);
                }

                foreach (XElement xe in openedResult)
                {
                    string type = xe.GetAttributeValue("Type");
                    if (type == "Edm.Stream")
                    {
                        streamTypeResultList.Add(xe);
                        continue;
                    }

                    if (type != null && complexTypeNames.Contains(type.GetLastSegment().Trim(')')))
                    {
                        string xpath2 = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}']", xe.GetAttributeValue("Name"));
                        var complexTypeElement = complexTypeOpen.XPathSelectElement(xpath2, ODataNamespaceManager.Instance);
                        complexTypeEnumQueue.Enqueue(complexTypeElement);
                    }
                }
            }

            return streamTypeResultList;
        }

        /// <summary>
        /// Get the names of complex type of normal property in specified entity.
        /// </summary>
        /// <param name="entityTypeShortName">The name of entity type.</param>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="complexTypeNames">The specified complex types.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPropertyNameWithComplexTypeFromEntity(string entityTypeShortName, string metadata, List<string> complexTypeNames)
        {
            Dictionary<string, string> propertiesNameWithComplexType = new Dictionary<string, string>();

            List<XElement> propertiesWithComplexType = GetPropertyElementWithComplexType(entityTypeShortName, metadata, complexTypeNames);

            foreach (XElement xe in propertiesWithComplexType)
            {
                propertiesNameWithComplexType.Add(xe.GetAttributeValue("Name"), xe.GetAttributeValue("Type").GetLastSegment().Trim(')'));
            }

            return propertiesNameWithComplexType;
        }

        /// <summary>
        /// Get complex type of normal property element in specified entity.
        /// </summary>
        /// <param name="entityTypeShortName">The name of entity type.</param>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="complexTypeNames">The specified complex types.</param>
        /// <returns></returns>
        public static List<XElement> GetPropertyElementWithComplexType(string entityTypeShortName, string metadata, List<string> complexTypeNames)
        {
            List<XElement> propertiesWithComplexType = new List<XElement>();

            List<XElement> normalProperties = GetAllPropertiesOfEntity(metadata, entityTypeShortName, MatchPropertyType.Normal);

            foreach (XElement xe in normalProperties)
            {
                if (xe.GetAttributeValue("Type") != null && complexTypeNames.Contains(xe.GetAttributeValue("Type").GetLastSegment().Trim(')')))
                {
                    propertiesWithComplexType.Add(xe);
                }
            }

            return propertiesWithComplexType;
        }

        /// <summary>
        /// Gets the type of property's value from the metadata.
        /// </summary>
        /// <param name="propName">Indicate the property's name.</param>
        /// <param name="props">Indicate the properties.</param>
        /// <returns>Returns the value of property's type.</returns>
        public static string GetPropertyValueTypeFromMetadata(string propName, IEnumerable<XElement> props)
        {
            if (propName == string.Empty || propName == null || props == null)
            {
                return null;
            }

            string result = string.Empty;

            foreach (var prop in props)
            {
                if (propName == prop.Attribute("Name").Value)
                {
                    result = prop.Attribute("Type").Value;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets property type from metadata.
        /// </summary>
        /// <param name="propName">Indicate the property's name.</param>
        /// <param name="entityTypeShortName">Indicate the entity type's short name.</param>
        /// <param name="metadata">Store the metadata information.</param>
        /// <returns>Returns the property type.</returns>
        public static string GetPropertyTypeFromMetadata(string propName, string entityTypeShortName, string metadata)
        {
            string propType = null;

            if (string.IsNullOrEmpty(propName) || string.IsNullOrEmpty(entityTypeShortName) || !metadata.IsXmlPayload())
            {
                return propType;
            }

            IEnumerable<XElement> props = GetAllPropertiesOfEntity(metadata, entityTypeShortName, MatchPropertyType.Normal);

            foreach (var prop in props)
            {
                if (propName == prop.Attribute("Name").Value)
                {
                    propType = prop.Attribute("Type").Value;
                    break;
                }
            }

            return propType;
        }

        /// <summary>
        /// Gets navigation property type from metadata.
        /// </summary>
        /// <param name="navigPropName">Indicate the navigation property's name.</param>
        /// <param name="entityTypeShortName">Indicate the entity type's short name.</param>
        /// <param name="metadata">Store the metadata information.</param>
        /// <returns>Returns the navigation property type.</returns>
        public static string GetNavigPropertyTypeFromMetadata(string navigPropName, string entityTypeShortName, string metadata)
        {
            string navigPropType = null;

            if (string.IsNullOrEmpty(navigPropName) || string.IsNullOrEmpty(entityTypeShortName) || !metadata.IsXmlPayload())
            {
                return navigPropType;
            }

            IEnumerable<XElement> props = GetAllPropertiesOfEntity(metadata, entityTypeShortName, MatchPropertyType.Navigations);

            foreach (var prop in props)
            {
                if (navigPropName == prop.Attribute("Name").Value)
                {
                    navigPropType = prop.Attribute("Type").Value;
                    break;
                }
            }

            return navigPropType;
        }

        /// <summary>
        /// Get the names of appropriate navigation properties from metadata.
        /// </summary>
        /// <param name="sc">The service context.</param>
        /// <param name="type">The navigation property type.</param>
        /// <returns>Returns the list of navigation property names.</returns>
        public static List<string> GetAppropriateNavigationPropertyNames(ServiceContext sc, NavigationPropertyType type)
        {
            if (sc == null)
            {
                return null;
            }

            List<string> result = new List<string>();
            string xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", sc.EntityTypeShortName);
            XElement metadata = XElement.Parse(sc.MetadataDocument);
            var navigProps = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            if (ODataVersion.V4 == sc.Version)
            {
                var collectionTypeNavigProps = NavigationPropertyType.SetOfEntities == type ?
                    navigProps.Where(np => np.Attribute(@"Type").Value.Contains(@"Collection")) :
                    navigProps.Where(np => !np.Attribute(@"Type").Value.Contains(@"Collection"));

                foreach (var ctnp in collectionTypeNavigProps)
                {
                    result.Add(ctnp.Attribute(@"Name").Value);
                }
            }
            else
            {
                string typeFlag = NavigationPropertyType.SetOfEntities == type ? @"*" : @"0..1";
                xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", sc.EntityTypeShortName);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sc.MetadataDocument);
                XmlNode entityTypeNode = xmlDoc.SelectSingleNode(xpath, ODataNamespaceManager.Instance);
                string odataNamespace = string.Empty;

                if (null != entityTypeNode.ParentNode.Attributes[@"Namespace"])
                {
                    odataNamespace = entityTypeNode.ParentNode.Attributes[@"Namespace"].Value;
                }

                string associNameFilter = @"1!=1";
                string toRoleNameFilter = @"1!=1";

                foreach (var np in navigProps)
                {
                    string associName = np.Attribute(@"Relationship").Value.Remove(0, (odataNamespace + @".").Length);
                    associNameFilter += @" or " + string.Format(@"@Name='{0}'", associName);

                    string toRoleName = np.Attribute(@"ToRole").Value;
                    toRoleNameFilter += @" or " + string.Format(@"@Role='{0}'", toRoleName);
                }

                xpath = string.Format(@"//*[local-name()='Association'][{0}]/*[local-name()='End'][@Multiplicity='{1}' and ({2})]", associNameFilter, typeFlag, toRoleNameFilter);
                XmlNodeList endNodes = xmlDoc.SelectNodes(xpath, ODataNamespaceManager.Instance);
                List<string> toRoleNames = new List<string>();

                foreach (XmlNode en in endNodes)
                {
                    toRoleNames.Add(en.Attributes[@"Role"].Value);
                }

                foreach (var np in navigProps)
                {
                    if (toRoleNames.Contains(np.Attribute(@"ToRole").Value))
                    {
                        result.Add(np.Attribute(@"Name").Value);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the names of appropriate navigation properties from metadata.
        /// </summary>
        /// <param name="entityTypeShortName">The entity type short name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="type">The navigation property rough type.</param>
        /// <returns>Returns the list of navigation property names.</returns>
        public static List<string> GetAppropriateNavigationPropertyNames(string entityTypeShortName, string metadataDoc, NavigationRoughType type)
        {
            if (string.IsNullOrEmpty(entityTypeShortName) || string.IsNullOrEmpty(metadataDoc))
            {
                return null;
            }

            List<string> result = new List<string>();
            string xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", entityTypeShortName);
            XElement metadata = XElement.Parse(metadataDoc);
            var navigProps = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            if (null == navigProps || !navigProps.Any())
            {
                return null;
            }

            foreach (var np in navigProps)
            {
                if (null == np.Attribute("Name") || null == np.Attribute("Type"))
                {
                    break;
                }

                if (NavigationRoughType.None == type)
                {
                    result.Add(np.GetAttributeValue("Name"));
                }
                else if (NavigationRoughType.SingleValued == type)
                {
                    if (!np.GetAttributeValue("Type").StartsWith("Collection("))
                    {
                        result.Add(np.GetAttributeValue("Name"));
                    }
                }
                else
                {
                    if (np.GetAttributeValue("Type").StartsWith("Collection("))
                    {
                        result.Add(np.GetAttributeValue("Name"));
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, string> GetAppropriateNavigationPropertyNameAndType(string entityTypeName, string metadataDocument, NavigationPropertyType type)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", entityTypeName);
            XElement metadata = XElement.Parse(metadataDocument);
            var navigProps = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            var collectionTypeNavigProps = NavigationPropertyType.SetOfEntities == type ?
                navigProps.Where(np => np.Attribute(@"Type").Value.Contains(@"Collection")) :
                navigProps.Where(np => !np.Attribute(@"Type").Value.Contains(@"Collection"));

            foreach (var ctnp in collectionTypeNavigProps)
            {
                result.Add(ctnp.GetAttributeValue("Name"), ctnp.GetAttributeValue("Type"));
            }

            return result;
        }

        /// <summary>
        /// Get first group of navigation property name recursing by system query $levels value. 
        /// (Note: Only support V4 service)
        /// </summary>
        /// <param name="entityTypeShortName">The entity type short name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="levels">The value of system query $levels.</param>
        /// <returns></returns>
        public static string[] GetNavigPropNamesRecurseByLevels(this string entityTypeShortName, string metadataDoc, int levels)
        {
            string[] result = new string[levels];

            if (string.IsNullOrEmpty(entityTypeShortName) || string.IsNullOrEmpty(metadataDoc))
            {
                return result;
            }

            for (int i = 0; i < levels; i++)
            {
                string xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", entityTypeShortName);
                XElement metadata = XElement.Parse(metadataDoc);
                var navigProp = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

                if (null == navigProp || null == navigProp.Attribute("Name"))
                {
                    return result;
                }

                result[i] = navigProp.Attribute(@"Name").Value;
                string navigPropType = navigProp.GetAttributeValue("Type");
                entityTypeShortName = navigPropType.Contains(@"Collection") ?
                    navigPropType.RemoveCollectionFlag().GetLastSegment() :
                    navigPropType.GetLastSegment();
            }

            return result;
        }

        /// <summary>
        /// Get all the properties from EntityType element in metadata.
        /// </summary>
        /// <param name="metadata">The whole metadata data.</param>
        /// <param name="entityTypeShortName">The short name of entity type.</param>
        /// <param name="mpType">The matching property type.</param>
        /// <returns>Returns all the properties of an entity.</returns>
        public static List<XElement> GetAllPropertiesOfEntity(string metadata, string entityTypeShortName, MatchPropertyType mpType)
        {
            if (metadata == null || metadata == string.Empty || entityTypeShortName == null || entityTypeShortName == string.Empty)
            {
                return null;
            }

            string xpath1 = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            string xpath2 = string.Empty;
            XElement md = XElement.Parse(metadata);
            var entity = md.XPathSelectElement(xpath1, ODataNamespaceManager.Instance);

            if (entity != null)
            {
                string baseTypeFullName = entity.Attribute(@"BaseType") != null ? entity.Attribute(@"BaseType").Value : string.Empty;
                string ns = entity.Parent.Attribute("Namespace").Value;
                string baseTypeShortName =
                    baseTypeFullName.Contains(@".") ?
                    baseTypeFullName.Remove(0, ns.Length + 1) :
                    baseTypeFullName;

                switch (mpType)
                {
                    default:
                    case MatchPropertyType.Normal:
                        xpath1 = @"./*[local-name()='Property']";
                        xpath2 = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", baseTypeShortName);
                        break;
                    case MatchPropertyType.Navigations:
                        xpath1 = @"./*[local-name()='NavigationProperty']";
                        xpath2 = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", baseTypeShortName);
                        break;
                    case MatchPropertyType.All:
                        xpath1 = @"./*[local-name()='Property' or local-name()='NavigationProperty']";
                        xpath2 = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' or local-name()='NavigationProperty']", baseTypeShortName);
                        break;
                }

                var currentEntityProps = entity.XPathSelectElements(xpath1, ODataNamespaceManager.Instance);
                var baseEntityProps = baseTypeFullName != string.Empty ? md.XPathSelectElements(xpath2, ODataNamespaceManager.Instance) : null;

                return baseEntityProps != null ? baseEntityProps.Concat(currentEntityProps).ToList() : currentEntityProps.ToList();
            }
            else
            {
                return new List<XElement>();
            }
        }

        /// <summary>
        /// Get property names of complex type.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="complexName">The name of complex type.</param>
        /// <returns>The list of property names.</returns>
        public static List<string> GetAllPropertiesNamesOfComplexType(string metadata, string complexName)
        {
            if (string.IsNullOrEmpty(metadata) || string.IsNullOrEmpty(complexName))
            {
                return null;
            }

            List<string> propertiesNames = new List<string>();
            string xpath = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}']", complexName);
            //string xpath = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}']/*[local-name()='Property']", complexName);
            XElement md = XElement.Parse(metadata);
            var complexType = md.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

            if (complexType != null)
            {
                string baseTypeName = complexType.GetAttributeValue(@"BaseType") != null ? complexType.GetAttributeValue(@"BaseType").GetLastSegment() : string.Empty;
                string xpath1 = @"./*[local-name()='Property']";
                string xpath2 = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}']/*[local-name()='Property']", baseTypeName);

                var currentComplexTypeProperties = complexType.XPathSelectElements(xpath1, ODataNamespaceManager.Instance);
                var baseComplexTypeProperties = !string.IsNullOrEmpty(baseTypeName) ? md.XPathSelectElements(xpath2, ODataNamespaceManager.Instance) : null;

                var total = baseComplexTypeProperties != null ? currentComplexTypeProperties.Concat(baseComplexTypeProperties) : currentComplexTypeProperties;
                foreach (XElement xe in total)
                {
                    propertiesNames.Add(xe.GetAttributeValue("Name"));
                }
            }

            return propertiesNames;
        }

        /// <summary>
        /// Get property names and types of a complex type.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="complexName">The name of complex type.</param>
        /// <returns>The dictionay of property name and its type.</returns>
        public static Dictionary<string, string> GetAllPropertiesOfComplexType(string metadata, string complexName)
        {
            if (string.IsNullOrEmpty(metadata) || string.IsNullOrEmpty(complexName))
            {
                return null;
            }

            Dictionary<string, string> properties = new Dictionary<string, string>();
            string xpath = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}']", complexName);
            XElement md = XElement.Parse(metadata);
            var complexType = md.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

            if (complexType != null)
            {
                string baseTypeName = complexType.GetAttributeValue(@"BaseType") != null ? complexType.GetAttributeValue(@"BaseType").GetLastSegment() : string.Empty;
                string xpath1 = @"./*[local-name()='Property']";
                string xpath2 = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}']/*[local-name()='Property']", baseTypeName);

                var currentComplexTypeProperties = complexType.XPathSelectElements(xpath1, ODataNamespaceManager.Instance);
                var baseComplexTypeProperties = !string.IsNullOrEmpty(baseTypeName) ? md.XPathSelectElements(xpath2, ODataNamespaceManager.Instance) : null;

                var total = baseComplexTypeProperties != null ? currentComplexTypeProperties.Concat(baseComplexTypeProperties) : currentComplexTypeProperties;
                foreach (XElement xe in total)
                {
                    properties.Add(xe.GetAttributeValue("Name"), xe.GetAttributeValue("Type"));
                }
            }

            return properties;
        }

        /// <summary>
        /// Get property names of a EntityType or ComplexType.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="@namespace">The namespace of the StructuredType.</param>
        /// <param name="complexName">The short name of the StructuredType.</param>
        /// <param name="mpType">The property type to get.</param>
        /// <returns>The dictionay of property name and its type.</returns>
        public static List<string> GetAllPropertiesNamesOfStructuredType(string metadata, string @namespace, string complexName, MatchPropertyType mpType)
        {
            if (string.IsNullOrEmpty(metadata) || string.IsNullOrEmpty(complexName) || string.IsNullOrEmpty(@namespace))
            {
                return null;
            }

            string basePath = @"./*[local-name()='DataServices']/*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{1}')]";

            string xpath = string.Format(basePath, @namespace, @namespace) + string.Format(@"/*[@Name='{0}']", complexName);

            XElement md = XElement.Parse(metadata);
            XElement structuredType = md.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
            List<string> propertiesNames = new List<string>();

            // Get base type name and namespace.
            if (structuredType != null)
            {
                string baseTypeName = string.Empty;
                string baseTypeNs = string.Empty;
                if (!string.IsNullOrEmpty(structuredType.GetAttributeValue(@"BaseType")))
                {
                    string fullname = structuredType.GetAttributeValue(@"BaseType");
                    int dotIndex = structuredType.GetAttributeValue(@"BaseType").LastIndexOf('.');
                    if (dotIndex != -1 && dotIndex != 0 && dotIndex != fullname.Length - 1)
                    {
                        baseTypeNs = fullname.Substring(0, dotIndex);
                        baseTypeName = fullname.Substring(dotIndex + 1);
                    }
                }

                // Get the XPath for properties.
                string currentTypePath;
                string baseTypePath = string.Empty;
                switch (mpType)
                {
                    case MatchPropertyType.All:
                        currentTypePath = @"./*[local-name()='Property' or local-name()='NavigationProperty']";
                        if (string.IsNullOrEmpty(baseTypeName)) { break; }
                        baseTypePath = string.Format(basePath, baseTypeNs, baseTypeNs)
                            + string.Format(@"/*[local-name()='{0}' and @Name='{1}']/*[local-name()='Property' or local-name()='NavigationProperty']", structuredType.Name, baseTypeName);
                        break;
                    case MatchPropertyType.Navigations:
                        currentTypePath = @"./*[local-name()='NavigationProperty']";
                        if (string.IsNullOrEmpty(baseTypeName)) { break; }
                        baseTypePath = string.Format(basePath, baseTypeNs, baseTypeNs)
                            + string.Format(@"/*[local-name()='{0}' and @Name='{1}']/*[local-name()='NavigationProperty']", structuredType.Name, baseTypeName);
                        break;
                    case MatchPropertyType.Normal:
                        currentTypePath = @"./*[local-name()='Property']";
                        if (string.IsNullOrEmpty(baseTypeName)) { break; }
                        baseTypePath = string.Format(basePath, baseTypeNs, baseTypeNs)
                            + string.Format(@"/*[local-name()='{0}' and @Name='{1}']/*[local-name()='Property']", structuredType.Name, baseTypeName);
                        break;
                    default:
                        return null;
                }

                IEnumerable<XElement> currentComplexTypeProperties = structuredType.XPathSelectElements(currentTypePath, ODataNamespaceManager.Instance);
                IEnumerable<XElement> baseComplexTypeProperties = string.IsNullOrEmpty(baseTypeName) ? null : md.XPathSelectElements(baseTypePath, ODataNamespaceManager.Instance);
                IEnumerable<XElement> total = baseComplexTypeProperties != null ? currentComplexTypeProperties.Concat(baseComplexTypeProperties) : currentComplexTypeProperties;


                foreach (XElement xe in total)
                {
                    propertiesNames.Add(xe.GetAttributeValue("Name"));
                }
            }

            return propertiesNames;
        }

        /// <summary>
        /// Get the sorted properties from EntityType elements in metadata.
        /// </summary>
        /// <param name="metadata">The whole metadata data.</param>
        /// <param name="entityTypeShortName">The short name of entity type.</param>
        /// <returns>Returns properties' name and type.</returns>
        public static List<string> GetSortedPropertiesOfEntity(string metadata, string entityTypeShortName)
        {
            if (string.IsNullOrEmpty(metadata) && string.IsNullOrEmpty(entityTypeShortName))
            {
                return null;
            }

            List<string> result = new List<string>();
            var props = GetAllPropertiesOfEntity(metadata, entityTypeShortName, MatchPropertyType.Normal);
            props.FindAll(p => IsSortedPrimitiveDataType(p.Attribute("Type").Value)).ForEach(p => { result.Add(string.Format(@"{0},{1}", p.Attribute("Name").Value, p.Attribute("Type").Value)); });
            return result;
        }

        /// <summary>
        /// Get the normal properties' names from EntityType elements in metadata.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="entityTypeShortName">The entity-type short name.</param>
        /// <returns>Returns the properties' name.</returns>
        public static List<string> GetNormalPropertiesNames(string metadata, string entityTypeShortName)
        {
            List<string> result = new List<string>();
            var props = GetAllPropertiesOfEntity(metadata, entityTypeShortName, MatchPropertyType.Normal);

            if (props.Any())
            {
                foreach (XElement xe in props)
                {
                    result.Add(xe.Attribute("Name").Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Get the navigation properties' names from EntityType elements in metadata.
        /// </summary>
        /// <param name="metadata">The metadata docment.</param>
        /// <param name="entityTypeShortName">The entity-type short name.</param>
        /// <returns>Returns the navigation properties' name.</returns>
        public static List<string> GetNavigationPropertiesNames(string metadata, string entityTypeShortName)
        {
            List<string> result = new List<string>();
            var props = GetAllPropertiesOfEntity(metadata, entityTypeShortName, MatchPropertyType.Navigations);

            if (props.Any())
            {
                foreach (XElement xe in props)
                {
                    result.Add(xe.Attribute("Name").Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Judge the primitive data type whether can be sorted or not.
        /// </summary>
        /// <param name="primitiveDataType">The primitive data type name.</param>
        /// <returns>Returns the result.</returns>
        public static bool IsSortedPrimitiveDataType(string primitiveDataType)
        {
            return PrimitiveDataTypes.Binary == primitiveDataType ||
                PrimitiveDataTypes.Boolean == primitiveDataType ||
                PrimitiveDataTypes.Byte == primitiveDataType ||
                PrimitiveDataTypes.Decimal == primitiveDataType ||
                PrimitiveDataTypes.Double == primitiveDataType ||
                PrimitiveDataTypes.Guid == primitiveDataType ||
                PrimitiveDataTypes.Int16 == primitiveDataType ||
                PrimitiveDataTypes.Int32 == primitiveDataType ||
                PrimitiveDataTypes.Int64 == primitiveDataType ||
                PrimitiveDataTypes.SByte == primitiveDataType ||
                PrimitiveDataTypes.Single == primitiveDataType ||
                PrimitiveDataTypes.String == primitiveDataType ?
                true : false;
        }

        /// <summary>
        /// Gets the name of the first entity set which contains entities with at least one navigation- and collection-valued property from metadata.
        /// </summary>
        /// <param name="metadataDoc">Metadata document.</param>
        /// <param name="navigationCollectionValuedPropertyName">Outputs the name of navigation- and collection-valued property.</param>
        /// <returns>Returns the appropriate entity set's name.</returns>
        public static string GetNameOfFirstEntitySetContainsNavigationCollectionValuedProperty(string metadataDoc, out string navigationCollectionValuedPropertyName, out string navigationCollectionValuedPropertyEntityType)
        {
            string result = string.Empty;
            string odataNamespace = string.Empty;
            string entityTypeName = string.Empty;
            string collectionEntityType = string.Empty;
            navigationCollectionValuedPropertyName = string.Empty;
            navigationCollectionValuedPropertyEntityType = string.Empty;

            if (string.IsNullOrEmpty(metadataDoc))
            {
                return string.Empty;
            }

            string xpath = @"//*[local-name()='EntityType']";
            XElement metadata = XElement.Parse(metadataDoc);
            var entityTypes = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (var entityType in entityTypes)
            {
                xpath = @"./*[local-name()='NavigationProperty']";
                var navigProps = entityType.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

                foreach (var navigProp in navigProps)
                {
                    if (navigProp.Attribute("Type").Value.Contains("Collection"))
                    {
                        navigationCollectionValuedPropertyName = navigProp.Attribute("Name").Value;
                        string type = navigProp.GetAttributeValue("Type");
                        collectionEntityType = type.Substring(type.IndexOf('(') + 1, type.LastIndexOf(')') - type.IndexOf('(') - 1);
                        break;
                    }
                }

                if (string.Empty != navigationCollectionValuedPropertyName)
                {
                    entityTypeName = entityType.Attribute("Name").Value;
                    odataNamespace = entityType.Parent.Attribute("Namespace").Value;

                    if (collectionEntityType.Contains(odataNamespace))
                    {
                        navigationCollectionValuedPropertyEntityType = collectionEntityType.Remove(0, odataNamespace.Length + 1);
                    }
                    else
                    {
                        navigationCollectionValuedPropertyEntityType = collectionEntityType;
                    }

                    break;
                }
            }

            if (string.Empty != entityTypeName && string.Empty != odataNamespace && string.Empty != navigationCollectionValuedPropertyName)
            {
                xpath = string.Format(@"//*[local-name()='EntityContainer']/*[local-name()='EntitySet' and @EntityType='{0}']", string.Format("{0}.{1}", odataNamespace, entityTypeName));
                result = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance).Attribute("Name").Value;
            }

            return result;
        }

        /// <summary>
        /// Gets all the non-navigation properties with special type from an entity type element in metadata document.
        /// </summary>
        /// <param name="entityTypeName">The name of an entity type.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="primitiveTypes">The specified type to limit the output properties.</param>
        /// <returns>Returns all the properties with specified type.</returns>
        public static List<string> GetPropertiesWithSpecifiedTypeFromEntityType(string entityTypeName, string metadataDoc, List<string> primitiveTypes)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(entityTypeName) || string.IsNullOrEmpty(metadataDoc) || primitiveTypes.Count == 0)
            {
                return result;
            }

            string xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", entityTypeName);
            XElement metadata = XElement.Parse(metadataDoc);
            var props = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance).ToList();
            result = props.Where(p => primitiveTypes.Contains(p.Attribute("Type").Value)).Select(p => string.Format(@"{0},{1}", p.Attribute("Name").Value, p.Attribute("Type").Value)).ToList();

            return result;
        }

        /// <summary>
        /// Get the URLs of entity sets which have a specified navigation property.
        /// </summary>
        /// <param name="sc">The service context.</param>
        /// <param name="navigPropType">The type of navigation property.</param>
        /// <returns>Returns a list contatins all the appropriate entity set URLs.</returns>
        public static List<string> GetEntitySetUrlsContainsCurrentEntityTypeName(ServiceContext sc, NavigationPropertyType navigPropType)
        {
            if (sc == null)
            {
                return new List<string>();
            }

            string entityTypeNs = sc.EntityTypeFullName.RemoveEnd(sc.EntityTypeShortName);
            string target = NavigationPropertyType.SetOfEntities == navigPropType ?
                            string.Format("Collection({0})", sc.EntityTypeFullName) :
                            sc.EntityTypeFullName;
            string xpath = string.Format("//*[local-name()='EntityType']/*[local-name()='NavigationProperty' and @Type='{0}']", target);
            XElement metadata = XElement.Parse(sc.MetadataDocument);
            var navigProps = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance).ToList();

            if (navigProps == null)
            {
                return new List<string>();
            }

            var entityTypeElementNames = new List<string>();
            navigProps.ForEach(n => entityTypeElementNames.Add(entityTypeNs + n.Parent.Attribute("Name").Value));

            xpath = @"//*[local-name()='EntityContainer']/*[local-name()='EntitySet']";
            var allEntitySetElements = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance).ToList();

            if (allEntitySetElements == null)
            {
                return new List<string>();
            }

            var entitySetElementNames = allEntitySetElements
                .FindAll(entitySet => entityTypeElementNames.Contains(entitySet.Attribute("EntityType").Value))
                .Select(entitySet => entitySet.Attribute("Name").Value);

            JObject service = JObject.Parse(sc.ServiceDocument);
            var entities = JsonParserHelper.GetEntries(service).ToList();

            if (entities == null)
            {
                return new List<string>();
            }

            return entities.Where(e => entitySetElementNames.Contains(e["name"].ToString())).Select(e => e["url"].ToString()).ToList();
        }

        /// <summary>
        /// Gets inheritance of entity types from metadata.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns a list of entity type with inheritance.</returns>
        public static List<EntityTypeElement> GetEntityTypeInheritance(string metadataDoc, string serviceDoc)
        {
            List<EntityTypeElement> result = new List<EntityTypeElement>();

            if (string.IsNullOrEmpty(metadataDoc) || string.IsNullOrEmpty(serviceDoc))
            {
                return result;
            }

            XElement metadata = XElement.Parse(metadataDoc);
            string xpath = @"//*[local-name()='Schema']/*[local-name()='EntityType']";
            metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance)
                .ToList()
                .ForEach(r =>
                {
                    if (null != r.Attribute("BaseType"))
                    {
                        result.Add(EntityTypeElement.Parse(r));
                    }
                });

            return result;
        }

        /// <summary>
        /// Gets the name of entity type which contains specified navigation property.
        /// </summary>
        /// <param name="navigPropType">The navigation property's name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns a list of entity type full name.</returns>
        public static List<string> GetEntityTypeNamesContainsSpecifiedNavigProp(string navigPropType, string metadataDoc)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(navigPropType) || !metadataDoc.IsXmlPayload())
            {
                return result;
            }

            XElement metadata = XElement.Parse(metadataDoc);
            string xpath = string.Format(@"//*[local-name()='EntityType']/*[local-name()='NavigationProperty' and @Type='{0}']", navigPropType);
            metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance)
                .ToList()
                .ForEach(r =>
                {
                    if (r.Parent != null && r.Parent.Attribute("Name") != null)
                    {
                        string ns = r.Parent.Parent != null && r.Parent.Parent.Attribute("Namespace") != null ?
                            r.Parent.Parent.Attribute("Namespace").Value :
                            string.Empty;
                        result.Add(string.Format("{0}.{1}", ns, r.Parent.Attribute("Name").Value));
                    }
                });

            return result;
        }

        /// <summary>
        /// Add namespace prefix for any names of the element in metadata document.
        /// </summary>
        /// <param name="targetName">The target element's name.</param>
        /// <param name="appliesType">The applies type of the element. (Related to the element's local-name.)</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the name with the namespace of the element.</returns>
        public static string AddNamespace(this string targetName, AppliesToType appliesType, string metadataDoc)
        {
            if (string.IsNullOrEmpty(targetName) || !metadataDoc.IsXmlPayload())
            {
                return string.Empty;
            }

            string target = targetName.GetLastSegment();
            XElement metadata = XElement.Parse(metadataDoc);
            string xpath = string.Format(@"//*[local-name()='{0}' and @Name='{1}']", appliesType, target);
            var element = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

            if (null == element)
            {
                return string.Empty;
            }

            AliasNamespacePair aliasNamespace = element.GetAliasAndNamespace();

            if (string.IsNullOrEmpty(aliasNamespace.Namespace))
            {
                return string.Empty;
            }

            return string.Format("{0}.{1}", aliasNamespace.Namespace, target);
        }

        /// <summary>
        /// Judge whether the input name is an entity-set name.
        /// </summary>
        /// <param name="name">An input name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns a decision outcome.</returns>
        public static bool IsEntitySetName(this string name, string metadataDoc)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(metadataDoc))
            {
                return false;
            }

            XElement metadata = XElement.Parse(metadataDoc);
            string xpath = @"//*[local-name()='Schema']/*[local-name()='EntityContainer']/*[local-name()='EntitySet']";
            var elements = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            var entitySetNames = elements.Where(ele => name == ele.Attribute("Name").Value).Select(ele => ele.Attribute("Name").Value);

            return 0 < entitySetNames.Count() ? true : false;
        }

        /// <summary>
        /// Gets all entity types which were satisfied specified conditions from metadata document. 
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="entityTypePredicate">Assert whether an entity-type matches specified conditions.</param>
        /// <param name="containedKeyPropSum">The sum of the key properties in current entity-type.</param>
        /// <param name="containedKeyPropTypes">The contained key property types.</param>
        /// <param name="containedNorPropTypes">The contained normal property types.</param>
        /// <param name="containedNavigRoughType">The contained navigation properties' rough-type at least one was satisfied the input value.</param>
        /// <returns></returns>
        public static IEnumerable<EntityTypeElement> GetEntityTypes(
            string metadataDoc,
            uint? containedKeyPropSum = 1,
            IEnumerable<string> containedKeyPropTypes = null,
            IEnumerable<string> containedNorPropTypes = null,
            NavigationRoughType containedNavigRoughType = NavigationRoughType.None)
        {
            if (string.IsNullOrEmpty(metadataDoc))
            {
                return null;
            }

            List<EntityTypeElement> result = new List<EntityTypeElement>();
            XElement metadata = XElement.Parse(metadataDoc);
            string xpath = @"//*[local-name()='Schema']/*[local-name()='EntityType']";
            var elements = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (var ele in elements)
            {
                EntityTypeElement entityType = EntityTypeElement.Parse(ele);

                if (PredicateHelper.EntityTypeAnyPropertiesMeetsSpecifiedConditions(entityType, containedKeyPropSum, containedKeyPropTypes, containedNorPropTypes, containedNavigRoughType))
                {
                    result.Add(entityType);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a specified entity-type and parses it to the EntityTypeElement class type.
        /// </summary>
        /// <param name="entityTypeName">The entity-type name.</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the instance with the EntityTypeElement class type.</returns>
        public static EntityTypeElement GetSpecifiedEntityType(this string entityTypeName, string metadataDoc)
        {
            if (string.IsNullOrEmpty(entityTypeName) || !metadataDoc.IsXmlPayload())
            {
                return null;
            }

            string entityTypeShortName = entityTypeName.GetLastSegment();
            var metadata = XElement.Parse(metadataDoc);

            if (null == metadata)
            {
                return null;
            }

            string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            var entityTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != entityTypeElem ? EntityTypeElement.Parse(entityTypeElem) : null;
        }

        /// <summary>
        /// Get key properties.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="entityTypeShortName">The short name of an entity type.</param>
        /// <returns>Returns all the key properties.</returns>
        public static IEnumerable<NormalProperty> GetKeyProperties(string metadataDoc, string entityTypeShortName)
        {
            if (string.IsNullOrEmpty(metadataDoc) || string.IsNullOrEmpty(entityTypeShortName))
            {
                return null;
            }

            XElement metadata = XElement.Parse(metadataDoc);
            string xpath = string.Format(@"//*[local-name()='Schema']/*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            var element = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
            var entityType = EntityTypeElement.Parse(element);

            return entityType.NormalProperties.Where(np => np.IsKey).Select(np => np);
        }

        /// <summary>
        /// Get normal properties.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="entityTypeShortName">The short name of an entity type.</param>
        /// <returns>Returns all the normal properties.</returns>
        public static IEnumerable<NormalProperty> GetNormalProperties(string metadataDoc, string entityTypeShortName)
        {
            if (string.IsNullOrEmpty(metadataDoc) || string.IsNullOrEmpty(entityTypeShortName))
            {
                return null;
            }

            XElement metadata = XElement.Parse(metadataDoc);
            string xpath = string.Format(@"//*[local-name()='Schema']/*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            var element = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
            var entityType = EntityTypeElement.Parse(element);

            return entityType.NormalProperties;
        }

        /// <summary>
        /// Get navigation properties.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="entityTypeShortName">The short name of an entity type.</param>
        /// <returns>Return all the navigation properties.</returns>
        public static IEnumerable<NavigProperty> GetNavigationProperties(string metadataDoc, string entityTypeShortName)
        {
            if (string.IsNullOrEmpty(metadataDoc) || string.IsNullOrEmpty(entityTypeShortName))
            {
                return null;
            }

            XElement metadata = XElement.Parse(metadataDoc);
            string xpath = string.Format(@"//*[local-name()='Schema']/*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            var element = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
            var entityType = EntityTypeElement.Parse(element);

            return entityType.NavigationProperties;
        }

        /// <summary>
        /// Get a concurrency entity type from metadata document.
        /// </summary>
        /// <param name="metadataDoc">The OData metadata document.</param>
        /// <returns>Returns an entity-type.</returns>
        public static EntityTypeElement GetConcurrencyEntityType(string metadataDoc)
        {
            if (string.IsNullOrEmpty(metadataDoc))
            {
                return null;
            }

            XElement metadata = XElement.Parse(metadataDoc);
            string xpath = @"//*[local-name()='Schema']/*[local-name()='EntityType']/*[local-name()='Property' and @ConcurrencyMode='Fixed']";
            var element = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

            if (null == element || null == element.Parent)
            {
                return null;
            }

            var entityType = EntityTypeElement.Parse(element.Parent);

            if (1 == entityType.KeyProperties.Count() && "Edm.Int32" == entityType.KeyProperties.First().PropertyType)
            {
                return entityType;
            }

            return null;
        }

        /// <summary>
        /// Get entity containing complex type with specified property type.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <param name="specifiedType">The specified type of property in complex type.</param>
        /// <param name="entityTypeInfo">Output: The information of the entity type that has the complex type property containing specified type property.
        /// The dictionary structures are like follows
        /// (entity type short name, ((complex type property name, complex type name), list of property names that are the specified type)</param>
        /// <returns>true: get the specified entity; false: otherwise.</returns>
        public static bool GetEntityTypesWithComplexProperty(
            string metadataDoc,
            string specifiedType,
            out Dictionary<string, Dictionary<KeyValuePair<string, string>, List<string>>> entityTypeInfo)
        {
            entityTypeInfo = new Dictionary<string, Dictionary<KeyValuePair<string, string>, List<string>>>();
            var complexTypeInfo = new Dictionary<string, List<string>>();

            // Find all complex property containing property with specified type.
            XElement metadata = XElement.Parse(metadataDoc);
            string xPath = @"//*[local-name()='Schema']/*[local-name()='ComplexType']";
            var complexTypeElem = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

            if (null == complexTypeElem || !complexTypeElem.Any())
            {
                return false;
            }

            foreach (var elem in complexTypeElem)
            {
                XElement complexElem = elem;
                bool continueSearch = true;
                while (continueSearch)
                {
                    var propNames = from prop in complexElem.Elements()
                                    where prop.Name.LocalName.Equals("Property") && prop.Attribute("Type").Value.Equals(specifiedType)
                                    select prop.GetAttributeValue("Name");

                    if (propNames.Any())
                    {
                        complexTypeInfo.Add(elem.GetAttributeValue("Name"), propNames.ToList());
                        continueSearch = false;
                    }
                    else
                    {
                        if(elem.Attribute("BaseType")!=null && ! string.IsNullOrEmpty(elem.Attribute("BaseType").Value))
                        {
                            complexElem = GetTypeDefinitionEleByDoc("ComplexType", elem.Attribute("BaseType").Value, metadataDoc);
                        }
                    }
                }
            }

            if (null == complexTypeInfo || !complexTypeInfo.Any())
            {
                return false;
            }

            // Find all entity types containing got complex type
            xPath = @"//*[local-name()='Schema']/*[local-name()='EntityType']";
            var entityTypeElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

            if (null == entityTypeElems || !entityTypeElems.Any())
            {
                return false;
            }

            foreach (XElement elem in entityTypeElems)
            {
                var complexTypes = new Dictionary<KeyValuePair<string, string>, List<string>>();

                foreach (var info in complexTypeInfo)
                {
                    string complexTypeName = info.Key;
                    var complexProperties =
                        from prop in elem.Elements()
                        where prop.Name.LocalName.Equals("Property") && prop.GetAttributeValue("Type").EndsWith(string.Format(".{0}", complexTypeName))
                        select prop;

                    if (null == complexProperties || !complexProperties.Any())
                    {
                        continue;
                    }

                    foreach (var cp in complexProperties)
                    {
                        if (info.Value.Any())
                        {
                            complexTypes.Add(new KeyValuePair<string, string>(cp.GetAttributeValue("Name"), cp.GetAttributeValue("Type")), info.Value);
                        }
                    }
                }

                if (complexTypes.Any())
                {
                    entityTypeInfo.Add(elem.GetAttributeValue("Name"), complexTypes);
                }
            }

            return null != entityTypeInfo && entityTypeInfo.Any();
        }

        /// <summary>
        /// Judge whether the entity has Collection type properties by thoroughly checking all its base ancestor entity types.
        /// </summary>
        /// <param name="metadata">The string of metadata document.</param>
        /// <param name="entityTypeShortName">The name of entity type.</param>
        /// <param name="context">The service context.</param>
        /// <param name="collectionPropNames">Output the list of collection properties' names.</param>
        /// <returns> True, if the entity is media type; false, otherwise. </returns>
        public static bool HasEntityCollectionProp(string metadata, string entityTypeShortName, ServiceContext context, out List<string> collectionPropNames)
        {
            bool hasBaseType = true;
            collectionPropNames = new List<string>();

            while (hasBaseType)
            {
                string xpath1 = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and contains(@Type, 'Collection(')]", entityTypeShortName);
                XElement md = XElement.Parse(metadata);
                IEnumerable<XElement> collectionProps = md.XPathSelectElements(xpath1, ODataNamespaceManager.Instance);

                if (collectionProps != null)
                {
                    foreach (XElement prop in collectionProps)
                    {
                        collectionPropNames.Add(prop.Attribute("Name").Value);
                    }
                }

                xpath1 = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
                XElement entity = md.XPathSelectElement(xpath1, ODataNamespaceManager.Instance);

                if (null != entity.Attribute("BaseType") && !string.IsNullOrEmpty(entity.Attribute("BaseType").Value))
                {
                    string baseEntityQualifiedName = entity.GetAttributeValue("BaseType");
                    entity = GetTypeDefinitionEleInScope("EntityType", baseEntityQualifiedName, context);
                    entityTypeShortName = entity.Attribute("Name").Value;
                }
                else
                {
                    hasBaseType = false;
                }
            }

            return collectionPropNames != null && collectionPropNames.Any();
        }

        /// <summary>
        /// Generate a individual property URL.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="svcDoc">The service document.</param>
        /// <param name="propertyTypes">The specified property types.</param>
        /// <returns>The individual property URL.</returns>
        public static string GenerateIndividualPropertyURL(string metadata, string svcDoc, string svcDocUrl, List<string> propertyTypes)
        {
            string individualPropertyUrl = string.Empty;
            var payloadFormat = svcDoc.GetFormatFromPayload();
            string[] feeds = ContextHelper.GetFeeds(svcDoc, payloadFormat).ToArray();

            for (int i = 0; i < feeds.Length; i++)
            {
                string entitySetUrl = feeds[i];
                string entityTypeShortName = entitySetUrl.MapEntitySetURLToEntityTypeShortName();
                List<string> properties = GetPropertiesWithSpecifiedTypeFromEntityType(entityTypeShortName, metadata, propertyTypes);

                if (properties.Count > 0)
                {
                    string feedUrl = svcDocUrl.TrimEnd('/') + @"/" + entitySetUrl + @"/?$top=1";
                    var response = WebHelper.Get(new Uri(feedUrl), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);

                    if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                    {
                        var entityIDs = ContextHelper.GetEntries(response.ResponsePayload, PayloadFormat.JsonLight);

                        if (entityIDs.Any())
                        {
                            string entityUrl = entityIDs.First();
                            individualPropertyUrl = entityUrl + @"/" + properties.First().Split(',')[0];
                            break;
                        }
                    }
                }
            }

            return individualPropertyUrl;
        }

        /// <summary>
        /// Generate URL of derived type.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="svcDoc">The service document.</param>
        /// <returns>The URL of derived type.</returns>
        public static string GenerateDerivedTypeURL(string metadata, string svcDoc)
        {
            string derivedTypeUrl = string.Empty;
            //var payloadFormat = svcDoc.GetFormatFromPayload();
            //string[] feeds = ContextHelper.GetFeeds(svcDoc, payloadFormat).ToArray();

            var derivedElements = GetEntityTypeInheritance(metadata, svcDoc);

            if (derivedElements.Any())
            {
                EntityTypeElement ele = derivedElements.First();
                //string baseTypeShortName = ele.BaseTypeFullName.Contains('.') ? ele.BaseTypeFullName.Split('.')[1] : ele.BaseTypeFullName;
                string derivedTypeFullName = ele.EntityTypeFullName;
                string entitySetUrl = ele.BaseTypeFullName.MapEntityTypeFullNameToEntitySetURL();
                derivedTypeUrl = entitySetUrl + @"/" + derivedTypeFullName;
            }

            return derivedTypeUrl;
        }

        /// <summary>
        /// Generate URL of media stream property.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="svcDoc">The service document.</param>
        /// <param name="svcDocUrl">The service document URL.</param>
        /// <returns>The URL of media stream property.</returns>
        public static string GenerateMediaStreamURL(string metadata, string svcDoc, string svcDocUrl)
        {
            string mediaStreamUrl = string.Empty;
            var payloadFormat = svcDoc.GetFormatFromPayload();
            var feeds = ContextHelper.GetFeeds(svcDoc, payloadFormat);

            foreach (string feed in feeds)
            {
                string entityTypeShortName = feed.MapEntitySetURLToEntityTypeShortName();
                List<string> properties = GetPropertiesWithSpecifiedTypeFromEntityType(entityTypeShortName, metadata, new List<string>() { PrimitiveDataTypes.Stream });

                if (properties.Count > 0)
                {
                    string entityUrl = string.Empty;
                    string feedUrl = svcDocUrl.TrimEnd('/') + @"/" + feed + @"/?$top=1";
                    var response = WebHelper.Get(new Uri(feedUrl), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);

                    if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                    {
                        var entityIDs = ContextHelper.GetEntries(response.ResponsePayload, PayloadFormat.JsonLight);

                        if (entityIDs.Any())
                        {
                            entityUrl = entityIDs.First();
                            mediaStreamUrl = entityUrl + @"/" + properties.First().Split(',')[0];
                            break;
                        }
                    }
                }
            }

            return mediaStreamUrl;
        }

        /// <summary>
        /// Generate a reference URL according to metadata and service document.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <param name="svcDoc">The service document.</param>
        /// <param name="svcDocUrl">The service document URL.</param>
        /// <returns></returns>
        public static string GenerateReferenceURL(string metadata, string svcDoc, string svcDocUrl)
        {
            string referenceURL = string.Empty;
            var payloadFormat = svcDoc.GetFormatFromPayload();
            var feeds = ContextHelper.GetFeeds(svcDoc, payloadFormat);

            foreach (string feed in feeds)
            {
                string feedUrl = svcDocUrl.TrimEnd('/') + @"/" + feed + @"/?$top=1";
                var response = WebHelper.Get(new Uri(feedUrl), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);

                if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                {
                    JObject jo;
                    response.ResponsePayload.TryToJObject(out jo);
                    var entities = JsonParserHelper.GetEntries(jo);

                    if (entities != null && entities.Count > 0)
                    {
                        foreach (JProperty jp in entities.First())
                        {
                            if (jp.Name.EndsWith(Constants.OdataAssociationLinkPropertyNameSuffix)
                                && jp.Value.ToString().StripOffDoubleQuotes().EndsWith("$ref"))
                            {
                                referenceURL = Uri.IsWellFormedUriString(jp.Value.ToString().StripOffDoubleQuotes(), UriKind.Relative) ?
                                    svcDocUrl.TrimEnd('/') + @"/" + jp.Value.ToString().StripOffDoubleQuotes() :
                                    jp.Value.ToString().StripOffDoubleQuotes();
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(referenceURL))
                {
                    break;
                }
            }

            return referenceURL;
        }

        /// <summary>
        /// Get all entity set names from entity container
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <returns>The entity set names.</returns>
        public static List<string> GetAllEntitySetNames(string metadata)
        {
            List<string> entitySetNames = new List<string>();

            XElement metaXel = XElement.Parse(metadata);
            string xpath = @"//*[local-name()='EntityContainer']/*[local-name()='EntitySet']";
            var elements = metaXel.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            if (elements.Any())
            {
                foreach (XElement element in elements)
                {
                    entitySetNames.Add(element.GetAttributeValue("Name"));
                }
            }

            return entitySetNames;
        }

        /// <summary>
        /// Get EntityContainer names of specified metadata document.
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <returns>The names of EntityContainer.</returns>
        public static List<string> GetEntityContainerNames(string metadata)
        {
            List<string> entityContainerNames = new List<string>();

            XElement metaXel = XElement.Parse(metadata);
            string xpath = @"//*[local-name()='EntityContainer']";
            var elements = metaXel.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            if (elements.Any())
            {
                foreach (XElement element in elements)
                {
                    entityContainerNames.Add(element.GetAttributeValue("Name"));
                }
            }

            return entityContainerNames;
        }

        /// <summary>
        /// Get all entity set names from entity container
        /// </summary>
        /// <param name="metadata">The metadata document.</param>
        /// <returns>The entity set names.</returns>
        public static List<string> GetEntityTypeNamesOfAllEntityset(string metadata)
        {
            List<string> entityTypeNames = new List<string>();

            XElement metaXel = XElement.Parse(metadata);
            string xpath = @"//*[local-name()='EntityContainer']/*[local-name()='EntitySet']";
            var elements = metaXel.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            if (elements.Any())
            {
                foreach (XElement element in elements)
                {
                    entityTypeNames.Add(element.GetAttributeValue("EntityType").GetLastSegment());
                }
            }

            return entityTypeNames;
        }

        /// <summary>
        /// Get all the feeds from the metadata document.
        /// </summary>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns all the feeds.</returns>
        public static IEnumerable<string> GetFeeds(string metadataDoc)
        {
            if (string.IsNullOrEmpty(metadataDoc) || !metadataDoc.IsXmlPayload())
            {
                return new List<string>();
            }

            List<string> feeds = new List<string>();
            var metadata = XElement.Parse(metadataDoc);
            string xPath = "//*[local-name()='EntitySet']";
            var eles = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

            if (null == eles || !eles.Any())
            {
                return new List<string>();
            }

            foreach (var ele in eles)
            {
                if (null == ele.Attribute("Name"))
                {
                    continue;
                }

                feeds.Add(ele.GetAttributeValue("Name"));
            }

            return feeds;
        }

        /// <summary>
        /// Gets the alias and namespace of an element in metadata.
        /// </summary>
        /// <param name="element">The target element which will be got the alias and namespace.</param>
        /// <returns>Returns the alias and namespace information and store them in an parameter with AliasNamespacePair type.</returns>
        public static AliasNamespacePair GetAliasAndNamespace(this XElement element)
        {
            if (null == element)
            {
                throw new ArgumentNullException("The input parameter 'element' cannot be null");
            }

            string alias = string.Empty, nspace = string.Empty;
            string ns = element.Name.NamespaceName;

            while (null != element.Parent && ns == element.Parent.Name.NamespaceName)
            {
                element = element.Parent;
            }

            alias = null != element.Attribute("Alias") ? element.Attribute("Alias").Value : string.Empty;
            nspace = null != element.Attribute("Namespace") ? element.Attribute("Namespace").Value : string.Empty;

            return new AliasNamespacePair(alias, nspace);
        }

        /// <summary>
        /// Get the string list of the enumeration member names.
        /// </summary>
        /// <param name="metadatadoc">The string of the metadata document.</param>
        /// <param name="enumType">The name of the enumeration type.</param>
        /// <returns>The string list of the enumeration memeber names.</returns>
        public static List<string> GetValuesOfAnEnum(string metadatadoc, string enumType)
        {
            List<string> result = new List<string>();
            XElement metaXml = XElement.Parse(metadatadoc);
            string xpath = string.Format(@"./*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='EnumType' and @Name='{0}']", enumType);

            // Check whether the enumType is a qulified name.
            int dotIndex = enumType.LastIndexOf('.');
            if (dotIndex == 0 || dotIndex == enumType.Length - 1)
            {
                return result;
            }
            if (dotIndex != -1)
            {
                string ns = enumType.Substring(0, dotIndex);
                string shortName = enumType.Substring(dotIndex + 1);
                xpath = string.Format(@"./*[local-name()='DataServices']/*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{0}')]/*[local-name()='EnumType' and @Name='{1}']", ns, shortName);
            }

            XElement element = metaXml.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
            if (element != null)
            {
                foreach (XElement member in element.XPathSelectElements(@"./*[local-name()='Member']"))
                {
                    result.Add(member.Attribute("Name").Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Tells whehter a type is an enumeration type.
        /// </summary>
        /// <param name="metadatadoc">The string of the metadata document.</param>
        /// <param name="enumType">The name of the enumeratin type.</param>
        /// <returns>True, if it is an enum; false, otherwise.</returns>
        public static bool IsEnumType(string metadatadoc, string enumType)
        {
            XElement metaXml = XElement.Parse(metadatadoc);
            string xpath = string.Format(@"//*[local-name()='EnumType' and @Name='{0}']", enumType);
            XElement element = metaXml.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

            if (element != null && element.Attribute("Name").Value.Equals(enumType))
                return true;

            return false;
        }

        /// <summary>
        /// Verify whether the entity is a media-type entity or not.
        /// </summary>
        /// <param name="entityTypeShortName">The entity-type short name.</param>
        /// <returns>Returns a boolean value: true or false.</returns>
        public static bool IsMediaType(this string entityTypeShortName)
        {
            if (!entityTypeShortName.IsSpecifiedEntityTypeShortNameExist())
            {
                return false;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            var xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}' and @HasStream='true']/*[local-name()='Annotation' and @Term='Org.OData.Core.V1.AcceptableMediaTypes']", entityTypeShortName);
            var xElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != xElem;
        }

        /// <summary>
        /// Get the list of names of the TypeDefinition given its UnerlyingType.
        /// </summary>
        /// <param name="metadatadoc">The string of the Metadata document.</param>
        /// <param name="underlyingType">The name of the underlying type.</param>
        /// <returns>The list of the names of the TypeDefinitions having the same UnderlyingType.</returns>
        public static List<string> GetNamesOfTypeDefinitionByUnderlyingType(string metadatadoc, string underlyingType)
        {
            List<string> result = new List<string>();
            XElement metaXml = XElement.Parse(metadatadoc);
            string xpath = string.Format(@"//*[local-name()='TypeDefinition' and @UnderlyingType='{0}']", underlyingType);
            IEnumerable<XElement> elements = metaXml.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            if (elements != null)
            {
                foreach (XElement member in elements)
                {
                    result.Add(member.Attribute("Name").Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Get the string of the reference document that defines the namespace of the targeted element.
        /// </summary>
        /// <param name="elementFullName">The qualified name of the targeted element.</param>
        /// <param name="context">The context of the ODataValidation service.</param>
        /// <returns>The string of the referenece document; or empty string, if the reference is not found or fail in loading the reference document content from the internet.</returns>
        public static string GetReferenceDocByDefinedType(string elementFullName, ServiceContext context)
        {
            // Load MetadataDocument into XMLDOM
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.MetadataDocument);

            string elementSimpleName = elementFullName.GetLastSegment();
            string elementPrefix = elementFullName.Substring(0, elementFullName.Length - elementSimpleName.Length - 1);

            // See whether the navigation type can resolve to an entity type defined in one of the referenced data model.
            string xpath = "//*[local-name()='Reference']";
            XmlNodeList refNodeList = xmlDoc.SelectNodes(xpath);

            bool referenceFound = false;

            foreach (XmlNode reference in refNodeList)
            {
                foreach (XmlNode child in reference.ChildNodes)
                {
                    if (child.Name.Equals("edmx:Include"))
                    {
                        if (child.Attributes["Namespace"].Value.Equals(elementPrefix) ||
                            (child.Attributes["Alias"] != null && child.Attributes["Alias"].Value.Equals(elementPrefix)))
                        {
                            referenceFound = true;
                            break;
                        }
                    }
                    else if (child.Name.Equals("edmx:IncludeAnnotations"))
                    {
                        if (child.Attributes["TermNamespace"].Value.Equals(elementPrefix))
                        {
                            referenceFound = true;
                            break;
                        }
                    }
                    else 
                    {
                        // Risk: if there is other service shema uses the same alias as one of the three vocabularies, the reference result can be a false return. 
                        switch (elementPrefix)
                        {
                            case "Org.OData.Capabilities.V1":
                            case "Capabilities":
                                return context.VocCapabilities;
                            case "Org.OData.Core.V1":
                            case "Core":
                                return context.VocCore;
                            case "Org.OData.Measures.V1":
                            case "Measures":
                                return context.VocMeasures;
                            default:
                                break;
                        }
                    }
                }

                if (referenceFound)
                {
                    Uri referenceUri = new Uri(reference.Attributes["Uri"].Value, UriKind.RelativeOrAbsolute);
                    var payload = XElement.Parse(context.MetadataDocument);
                    string baseUriString = payload.GetAttributeValue("xml:base", ODataNamespaceManager.Instance);
                    if (!string.IsNullOrEmpty(baseUriString) && Uri.IsWellFormedUriString(reference.Attributes["Uri"].Value, UriKind.Relative))
                    {
                        Uri referenceUriRelative = new Uri(reference.Attributes["Uri"].Value, UriKind.Relative);
                        Uri baseUri = new Uri(baseUriString, UriKind.Absolute);
                        referenceUri = new Uri(baseUri, referenceUriRelative);
                    }

                    Response response = WebHelper.Get(referenceUri, string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                    {
                        return response.ResponsePayload;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the definition element of the type.
        /// </summary>
        /// <param name="primeTypeName">The prime type name such as ComplexType, EntityType etc.</param>
        /// <param name="typeFullName">The qualified name of the type.</param>
        /// <param name="schemaDocument">The schema document to look for the type definition.</param>
        /// <returns>The XElement of the type definition element; null, if the type definition is not found.</returns>
        public static XElement GetTypeDefinitionEleByDoc(string primeTypeName, string typeFullName, string schemaDocument)
        {
            string typeSimpleName = typeFullName.GetLastSegment();

            string typeNamePrefix = typeFullName.Substring(0, typeFullName.Length - typeSimpleName.Length - 1);

            XElement metaXml = XElement.Parse(schemaDocument);
            string xpath = string.Format(@"//*[local-name()='{0}' and @Name='{1}']", primeTypeName, typeSimpleName);
            IEnumerable<XElement> baseTypeElements = metaXml.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (XElement baseTypeElement in baseTypeElements)
            {
                AliasNamespacePair aliasNameSpace = baseTypeElement.GetAliasAndNamespace();

                if (typeNamePrefix == aliasNameSpace.Namespace || typeNamePrefix == aliasNameSpace.Alias)
                {
                    return baseTypeElement;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the in-scope type definition element by its qualified name.
        /// </summary>
        /// <param name="primeTypeName">The primary type name such as EntityType, ComplexType, Term, etc. </param>
        /// <param name="typeFullName">The qualified type name of the type.</param>
        /// <param name="context">The service context.</param>
        /// <returns>The XElement of the type definition element; null if not found.</returns>
        public static XElement GetTypeDefinitionEleInScope(string primeTypeName, string typeFullName, ServiceContext context)
        {
            XElement typeEle = MetadataHelper.GetTypeDefinitionEleByDoc(primeTypeName, typeFullName, context.MetadataDocument);
            if (typeEle != null)
            {
                return typeEle;
            }
            else
            {
                string refDoc = MetadataHelper.GetReferenceDocByDefinedType(typeFullName, context);
                if(!string.IsNullOrEmpty(refDoc))
                {
                    return MetadataHelper.GetTypeDefinitionEleByDoc(primeTypeName, typeFullName, refDoc);
                }
            }

            return null;
        }

        /// <summary>
        /// Whether the name is a TypeName.
        /// </summary>
        /// <param name="typeFullName">The qualified name of the type.</param>
        /// <param name="context">The service context.</param>
        /// <returns>True if the type is in scope, false otherwise.</returns>
        public static bool IsTypeName(string typeFullName, ServiceContext context)
        {
            string typeName = typeFullName.RemoveCollectionFlag();

            List<string> typePrimeNames = new List<string>() {"TypeDefinition", "ComplexType", "EntityType", "EnumType"};

            if (typeFullName.StartsWith("Edm."))
            {
                return true;
            }
            else
            {
                foreach (string typePrimeName in typePrimeNames)
                {
                    XElement result = GetTypeDefinitionEleInScope(typePrimeName, typeFullName, context);
                    if (result != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Judge whether the parent's children have unique names with each other in the children set.
        /// </summary>
        /// <param name="parent">The parent XElement.</param>
        /// <param name="childrenSet">The children name set.</param>
        /// <returns>True if the names of all children are unique in the children name set; False otherwise.</returns>
        public static bool IsNameUniqueInChildren(XElement parent, ref HashSet<string> childrenSet)
        {
            foreach (XNode child in parent.Nodes())
            {
                if (child.NodeType != XmlNodeType.Element) continue; // pass the comment node.
                if (((XElement)child).Attribute("Name") == null) continue;
                if (!(childrenSet.Add(((XElement)child).Attribute("Name").Value)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Whether the type contains a child Key element.
        /// </summary>
        /// <param name="entityTypeName">The qualified type name.</param>
        /// <param name="metadataDoc">The metadata document which contains the type.</param>
        /// <returns></returns>
        public static bool IsTypeContainsKey(string entityTypeName, string metadataDoc)
        {
            string ns = string.Empty;
            string shortName = string.Empty;
            int dotIndex = entityTypeName.LastIndexOf('.');
            if (dotIndex != -1 && dotIndex != 0 && dotIndex != entityTypeName.Length - 1)
            {
                ns = entityTypeName.Substring(0, dotIndex);
                shortName = entityTypeName.Substring(dotIndex + 1);
            }
            else
            {
                return false;
            }

            string XPath = @"./*[local-name()='DataServices']/*[local-name()='Schema' and (@Namespace='{0}' or @Alias= '{0}')]/*[local-name()='EntityType' and @Name= '{1}']";
            XElement metadata = XElement.Parse(metadataDoc);
            XElement entityType = metadata.XPathSelectElement(string.Format(XPath, ns, shortName));
            if (entityType != null)
            {
                if (entityType.XPathSelectElement(@"./*[local-name()='Key']") != null)
                { return true; }
                else
                { return false; }
            }
            else
            { return false; }
        }

        /// <summary>
        /// Get the navigation properties from the current entity type and its derived types.
        /// </summary>
        /// <param name="typeShortName">The short name of the current entity type.</param>
        /// <param name="pathStr">The path string.</param>
        /// <returns>Returns the navigation property elements and their parent-elements.</returns>
        public static List<Tuple<XElement, string>> GetNavigProperties(string typeShortName, string pathStr)
        {
            var result = new List<Tuple<XElement, string>>();
            if (!typeShortName.IsSpecifiedEntityTypeShortNameExist())
            {
                return result;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{0}']", typeShortName);
            var typeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null == typeElem)
            {
                return result;
            }

            xPath = "./*[local-name()='NavigationProperty']";
            var npElems = typeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            if (null != npElems && npElems.Any())
            {
                var tuples = new List<Tuple<XElement, string>>();
                npElems.ToList().ForEach(npElem =>
                {
                    // The first level of the path.
                    tuples.Add(new Tuple<XElement, string>(npElem, pathStr));
                });
                result.AddRange(tuples);
            }

            xPath = "./*[local-name()='Property']";
            var pElems = typeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                .Where(p => null != p.Attribute("Type"))
                .Select(p => p);
            if (null != pElems && pElems.Any())
            {
                result.AddRange(MetadataHelper.GetNavigPropertiesInComplexType(pElems, pathStr));
            }

            string typeFullName = "EntityType" == typeElem.Name.LocalName ?
                typeShortName.AddNamespace(AppliesToType.EntityType) :
                typeShortName.AddNamespace(AppliesToType.ComplexType);
            if (typeFullName.IsSpecifiedTypeFullNameExist())
            {
                result.AddRange(GetNavigPropertiesInDerivedType(typeFullName, pathStr));
                result.AddRange(GetNavigPropertiesInBasedType(typeFullName.GetLastSegment(), pathStr));
            }

            return result;
        }

        /// <summary>
        /// Get the navigation properties from the derived type of the current entity type.
        /// </summary>
        /// <param name="typeFullName">The full name of the currrent entity type or the complex type.</param>
        /// <param name="pathStr">The path string.</param>
        /// <returns>Return the navigation property elements.</returns>
        public static List<Tuple<XElement, string>> GetNavigPropertiesInDerivedType(string typeFullName, string pathStr)
        {
            var result = new List<Tuple<XElement, string>>();
            if (!typeFullName.IsSpecifiedEntityTypeFullNameExist())
            {
                return result;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and @BaseType='{0}']", typeFullName);
            var derivedTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != derivedTypeElem && null != derivedTypeElem.Attribute("Name"))
            {
                var derivedTypeFullName = "EntityType" == derivedTypeElem.Name.LocalName ?
                        derivedTypeElem.GetAttributeValue("Name").AddNamespace(AppliesToType.EntityType) :
                        derivedTypeElem.GetAttributeValue("Name").AddNamespace(AppliesToType.ComplexType);
                xPath = "./*[local-name()='NavigationProperty']";
                var navigationPropertyElems = derivedTypeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                if (null != navigationPropertyElems && navigationPropertyElems.Any())
                {
                    var tuple = new List<Tuple<XElement, string>>();
                    pathStr += "/" + derivedTypeFullName;
                    foreach (var npElem in navigationPropertyElems)
                    {
                        tuple.Add(new Tuple<XElement, string>(npElem, pathStr.TrimStart('/')));
                    }

                    result.AddRange(tuple);
                }

                xPath = "./*[local-name()='Property']";
                var pElems = derivedTypeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                    .Where(p => null != p.Attribute("Type"))
                    .Select(p => p);
                if (null != pElems && pElems.Any())
                {
                    result.AddRange(MetadataHelper.GetNavigPropertiesInComplexType(pElems, string.Empty));
                }

                if (null != derivedTypeElem.Attribute("Name"))
                {
                    typeFullName = "EntityType" == derivedTypeElem.Name.LocalName ?
                        derivedTypeElem.GetAttributeValue("Name").AddNamespace(AppliesToType.EntityType) :
                        derivedTypeElem.GetAttributeValue("Name").AddNamespace(AppliesToType.ComplexType);

                    result.AddRange(GetNavigPropertiesInDerivedType(typeFullName, pathStr.TrimStart('/')));
                }
            }

            return result;
        }

        /// <summary>
        /// Get the navigation properties from the based type of the current entity type.
        /// </summary>
        /// <param name="typeShortName">The short name of the currrent entity type or the complex type.</param>
        /// <param name="pathStr">The path string.</param>
        /// <returns>Return the navigation property elements.</returns>
        public static List<Tuple<XElement, string>> GetNavigPropertiesInBasedType(string typeShortName, string pathStr)
        {
            var result = new List<Tuple<XElement, string>>();
            if (!typeShortName.IsSpecifiedEntityTypeShortNameExist())
            {
                return result;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{0}']", typeShortName);
            var typeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != typeElem && null != typeElem.Attribute("BaseType"))
            {
                var basedTypeFullName = typeElem.Attribute("BaseType").Value;
                xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{0}']", basedTypeFullName.GetLastSegment());
                var basedTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                if (null != basedTypeElem)
                {
                    xPath = "./*[local-name()='NavigationProperty']";
                    var navigationPropertyElems = basedTypeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                    if (null != navigationPropertyElems && navigationPropertyElems.Any())
                    {
                        var tuple = new List<Tuple<XElement, string>>();
                        pathStr += "/" + basedTypeFullName;
                        foreach (var npElem in navigationPropertyElems)
                        {
                            tuple.Add(new Tuple<XElement, string>(npElem, pathStr.TrimStart('/')));
                        }

                        result.AddRange(tuple);
                    }

                    xPath = "./*[local-name()='Property']";
                    var pElems = basedTypeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                        .Where(p => null != p.Attribute("Type"))
                        .Select(p => p);
                    if (null != pElems && pElems.Any())
                    {
                        result.AddRange(MetadataHelper.GetNavigPropertiesInComplexType(pElems, string.Empty));
                    }

                    if (null != typeElem.Attribute("Name"))
                    {
                        var typeFullName = "EntityType" == basedTypeElem.Name.LocalName ?
                            basedTypeElem.GetAttributeValue("Name").AddNamespace(AppliesToType.EntityType) :
                            basedTypeElem.GetAttributeValue("Name").AddNamespace(AppliesToType.ComplexType);

                        result.AddRange(GetNavigPropertiesInBasedType(typeFullName, pathStr.TrimStart('/')));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the navigation properties from the complex types of the entity type's properties.
        /// </summary>
        /// <param name="props">All the property elements in the entity type.</param>
        /// <param name="pathStr">The path string.</param>
        /// <returns>Return the navigation property elements.</returns>
        public static List<Tuple<XElement, string>> GetNavigPropertiesInComplexType(IEnumerable<XElement> props, string pathStr)
        {
            var result = new List<Tuple<XElement, string>>();
            if (null == props || !props.Any())
            {
                return result;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            props = props.Where(prop => null != prop.Attribute("Type") && null != prop.Attribute("Name")).Select(prop => prop);
            if (null == props || !props.Any())
            {
                return result;
            }

            foreach (var prop in props)
            {
                var typeFullName = prop.GetAttributeValue("Type");
                var complexPropName = prop.GetAttributeValue("Name");

                // Verify the primitive type.
                if (typeFullName.StartsWith("Edm."))
                {
                    continue;
                }

                // Verify the complex type.
                pathStr += complexPropName + "/" + typeFullName + "/";
                var complexTypeShortName = typeFullName.GetLastSegment();
                string xPath = string.Format("//*[local-name()='ComplexType' and @Name='{0}']", complexTypeShortName);
                var complexTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                if (null == complexTypeElem)
                {
                    continue;
                }

                xPath = "./*[local-name()='NavigationProperty']";
                var npElems = complexTypeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                if (null != npElems && npElems.Any())
                {
                    var tuples = new List<Tuple<XElement, string>>();
                    foreach (var npElem in npElems)
                    {
                        tuples.Add(new Tuple<XElement, string>(npElem, pathStr.Trim('/')));
                    }

                    result.AddRange(tuples);
                }

                xPath = "./*[local-name()='Property']";
                var pElems = complexTypeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                if (null != pElems && pElems.Any())
                {
                    result.AddRange(MetadataHelper.GetNavigPropertiesInComplexType(pElems, pathStr));
                }
            }

            return result;
        }

        /// <summary>
        /// Get entity type short name from all the generations.
        /// </summary>
        /// <param name="entityTypeShortName">The short name of an entity type.</param>
        /// <returns>Returns all the entity-type short name from its related generations.</returns>
        public static List<string> GetEntityTypeShortNamesInEveryGeneration(this string entityTypeShortName)
        {
            if (!entityTypeShortName.IsSpecifiedEntityTypeShortNameExist())
            {
                return new List<string>();
            }

            var result = new List<string>();
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            var entityTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != entityTypeElem)
            {
                result.Add(entityTypeShortName);

                // Get all the base-type short name from the current entity-type element.
                result.AddRange(entityTypeShortName.GetEntityTypeShortNamesFromBaseType());

                // Get all the derived-type short name from the current entity-type element.
                result.AddRange(entityTypeShortName.GetEntityTypeShortNamesFromDerivedType());
            }

            return result;
        }

        /// <summary>
        /// Get entity-type short name from its base type.
        /// </summary>
        /// <param name="entityTypeShortName">The short name of an entity type.</param>
        /// <returns>Returns all the entity-type short name inherited from itself.</returns>
        public static List<string> GetEntityTypeShortNamesFromBaseType(this string entityTypeShortName)
        {
            if (!entityTypeShortName.IsSpecifiedEntityTypeShortNameExist())
            {
                return null;
            }

            var result = new List<string>();
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            var entityTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != entityTypeElem)
            {
                while (null != entityTypeElem.Attribute("BaseType"))
                {
                    entityTypeShortName = entityTypeElem.GetAttributeValue("BaseType").GetLastSegment();
                    xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
                    entityTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    result.Add(entityTypeElem.GetAttributeValue("Name"));
                }
            }

            return result;
        }

        /// <summary>
        /// Get entity-type short name from its derived type.
        /// </summary>
        /// <param name="entityTypeShortName">The short name of an entity type.</param>
        /// <returns>Returns all the entity-type short name derived from itself.</returns>
        public static List<string> GetEntityTypeShortNamesFromDerivedType(this string entityTypeShortName)
        {
            if (!entityTypeShortName.IsSpecifiedEntityTypeShortNameExist())
            {
                return null;
            }

            var result = new List<string>();
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            var entityTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != entityTypeElem)
            {
                while (true)
                {
                    xPath = string.Format("//*[local-name()='EntityType' and substring-after(@BaseType, '.')='{0}']", entityTypeShortName);
                    entityTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    if (null != entityTypeElem)
                    {
                        entityTypeShortName = entityTypeElem.GetAttributeValue("Name");
                        result.Add(entityTypeShortName);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the unstructural property's XML node, given its path begining with the including entity type's qualified name.
        /// </summary>
        /// <param name="path">The property path begining with an entity type qualified name.</param>
        /// <param name="metadata">The XmlNode of a the merged metadata.</param>
        /// <param name="result">The property XML node.</param>
        /// <returns>True if succeeds, false otherwise.</returns>
        public static bool ResolveToProperty(string path, XmlNode metadata, out XmlNode result)
        {
            result = null;
            string[] pathes = path.Split('/');

            if (!GetTypeNode(pathes[0], metadata, "'EntityType'", out result))
            {
                return false;
            }

            int i = 1;
            while (i < pathes.Length - 1)
            {
                if (!GetPropertyNode(pathes[i], metadata, ref result))
                {
                    return false;
                }

                if (!GetTypeNode(result.Attributes["Type"].Value, metadata, "('ComplexType' or 'EntityType')", out result))
                {
                    return false;
                }

                i++;
            }

            if (i == pathes.Length - 1)
            {
                if (!GetPropertyNode(pathes[i], metadata, ref result))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the property's XML node, given the propert name, the property including type Node, and metadata.
        /// </summary>
        /// <param name="propertyName">The local name of the property.</param>
        /// <param name="metadata">The whole in-scope metadata XML node defining the type and its property.</param>
        /// <param name="result">Input as the including type node; Output as the property node.</param>
        /// <returns>True, if succeeds; false, otherwise.</returns>
        public static bool GetPropertyNode(string propertyName, XmlNode metadata, ref XmlNode result)
        {
            XmlNode property = result.SelectSingleNode(string.Format(@"./*[@Name='{0}']", propertyName));

            // If not found inline, the property may be in the base type definition.
            while (property == null)
            {
                if (result.Attributes["BaseType"] != null)
                {
                    if (!GetTypeNode(result.Attributes["BaseType"].Value, metadata, "'" + result.LocalName + "'", out result))
                    {
                        return false;
                    }

                    property = result.SelectSingleNode(string.Format(@"./*[@Name='{0}']", propertyName));
                }
                else
                {
                    return false;
                }
            }

            result = property;

            return true;
        }

        /// <summary>
        /// Get the type's XML node, given its qualified name, metadata, and primitive type name.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the type.</param>
        /// <param name="metadata">The whole in-scope metadata XML node that defines the type.</param>
        /// <param name="primitiveTypeXPathSegment">The possible primitive type names XPath segment, such as ('EntityType' or 'ComplexType').</param>
        /// <param name="result">The out put of the type XML node, null if failed to found.</param>
        /// <returns>True, if succeeds; false, otherwise.</returns>
        public static bool GetTypeNode(string qualifiedName, XmlNode metadata, string primitiveTypeXPathSegment, out XmlNode result)
        {
            int dotIndex = qualifiedName.LastIndexOf('.');
            if (dotIndex == 0 || dotIndex == -1 || dotIndex == qualifiedName.Length - 1)
            {
                result = null;
                return false;
            }

            string ns = qualifiedName.Substring(0, dotIndex);
            string name = qualifiedName.Substring(dotIndex + 1);
            result = metadata.SelectSingleNode(string.Format(@"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{0}')]/*[local-name()={1} and @Name='{2}']", ns, primitiveTypeXPathSegment, name));
            if (result == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the XML node for the type in the merged service metadata, given its qualified name.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the type.</param>
        /// <param name="metadata">The XML node of the merged serivce metadata.</param>
        /// <param name="result">The XML node of the type.</param>
        /// <returns>True, if succeeds; false, otherwise.</returns>
        public static bool GetTypeNode(string qualifiedName, XmlNode metadata, out XmlNode result)
        {
            qualifiedName = qualifiedName.RemoveCollectionFlag();
            int dotIndex = qualifiedName.LastIndexOf('.');
            if (dotIndex == 0 || dotIndex == -1 || dotIndex == qualifiedName.Length - 1)
            {
                result = null;
                return false;
            }

            string ns = qualifiedName.Substring(0, dotIndex);
            string name = qualifiedName.Substring(dotIndex + 1);
            result = metadata.SelectSingleNode(string.Format(@"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{0}')]/*[@Name='{1}']", ns, name));
            if (result == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the element in metadata the path points to, given the path's element's parent or tageting XML node.
        /// </summary>
        /// <param name="path">The string of the forward slashes (/) splited path.</param>
        /// <param name="metadata">The XmlNode of the merged service metadata.</param>
        /// <param name="pathNode">The path's element's parent or tageting XML node.</param>
        /// <returns>True, if succeeds; false, otherwise.</returns>
        public static bool Path(string path, XmlNode metadata, ServiceContext context, ref XmlNode pathNode)
        {
            if(pathNode == null)
            {
                return false;
            }

            if(string.IsNullOrEmpty(path))
            {
                return true;
            }

            string[] pathes = path.Split('/');

            for (int i = 0; i < pathes.Length; i++ )
            {
                if(pathes[i].Equals("$count"))
                {
                    //Returns the type of the collection of the second to last path segment.
                    return true;
                }

                if (!pathes[i].StartsWith("@"))
                {
                    if (pathes[i].Contains("."))
                    {
                        if (!GetTypeNode(pathes[0], metadata, out pathNode))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (pathNode.LocalName.Equals("EntitySet"))
                        {
                            if (!GetTypeNode(pathNode.Attributes["EntityType"].Value, metadata, out pathNode))
                            {
                                return false;
                            }
                        }
                        else if (pathNode.LocalName.Equals("Singleton")||
                            pathNode.LocalName.Equals("Property") ||
                            pathNode.LocalName.Equals("NavigationProperty"))
                        {
                            if (!GetTypeNode(pathNode.Attributes["Type"].Value, metadata, out pathNode))
                            {
                                return false;
                            }
                        }

                        if (!GetPropertyNode(pathes[i], metadata, ref pathNode))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    int numSignIndex = pathes[i].IndexOf('#');
                    string termQualifiedName = (numSignIndex < 0) ? pathes[i].Substring(1) : pathes[i].Substring(1, numSignIndex - 1);

                    // Look for term definition in the merged metadata first.
                    if(GetTypeNode(termQualifiedName, metadata, "'Term'", out pathNode)) 
                    {
                        return true; 
                    }

                    // Look for term definition in the referenced vocabularies.
                    string termXmlDoc = GetTermXMLDefDoc(termQualifiedName, context);
                    if(string.IsNullOrEmpty(termXmlDoc))
                    {
                        pathNode = null;
                        return false;
                    }
                    XmlDocument termMetadataNode = new XmlDocument();
                    termMetadataNode.LoadXml(termXmlDoc);

                    if (!GetTypeNode(termQualifiedName, termMetadataNode, "'Term'", out pathNode))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Get the XML definition document of the term, given the term's qualified name.
        /// </summary>
        /// <param name="qualifiedName">The string of the qualified name of the term.</param>
        /// <param name="context">The service context.</param>
        /// <returns>The string of the whole XML document that defines the term.</returns>
        public static string GetTermXMLDefDoc(string qualifiedName, ServiceContext context)
        {
            int dotIndex = qualifiedName.LastIndexOf('.');
            if (dotIndex == 0 || dotIndex == -1 || dotIndex == qualifiedName.Length - 1)
            {
                return null;
            }

            string ns = qualifiedName.Substring(0, dotIndex);

            switch(ns)
            {
                case "Org.OData.Capabilities.V1":
                case "Capabilities":
                    return context.VocCapabilities;
                case "Org.OData.Core.V1": 
                case "Core":
                    return context.VocCore;
                case "Org.OData.Measures.V1":
                case "Measures":
                    return context.VocMeasures;
                default: 
                    break;
            }

            return GetReferenceDocByDefinedType(qualifiedName, context);
        }

        public static List<string> GetTypeShortNamesFromDerivedType(this string typeShortName)
        {
            if (!typeShortName.IsSpecifiedEntityTypeShortNameExist() &&
                !typeShortName.IsSpecifiedComplexTypeShortNameExist())
            {
                return null;
            }

            var result = new List<string>();
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{0}']", typeShortName);
            var typeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != typeElem)
            {
                while (true)
                {
                    xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and substring-after(@BaseType, '.')='{0}']", typeShortName);
                    typeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    if (null != typeElem)
                    {
                        typeShortName = typeElem.GetAttributeValue("Name");
                        result.Add(typeShortName);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public static List<string> GetTypeShortNamesFromBaseType(this string typeShortName)
        {
            if (!typeShortName.IsSpecifiedEntityTypeShortNameExist())
            {
                return null;
            }

            var result = new List<string>();
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{0}']", typeShortName);
            var typeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != typeElem)
            {
                while (null != typeElem.Attribute("BaseType"))
                {
                    typeShortName = typeElem.GetAttributeValue("BaseType").GetLastSegment();
                    xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{0}']", typeShortName);
                    typeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    result.Add(typeElem.GetAttributeValue("Name"));
                }
            }

            return result;
        }

        public static bool ResolvePropertyRefName(string name, XmlNode entityType, out XmlNode property)
        {
            if (string.IsNullOrEmpty(name) || entityType == null)
            {
                property = null;
                return false;
            }

            property = entityType;
            string[] nameParts = name.Split('/');
            property = property.SelectSingleNode(string.Format(@"./*[@Name='{0}']", nameParts[0]));
            if (property == null)
            {
                return false;
            }

            if (nameParts.Length < 2)
            {
                return true;
            }

            for (int i = 1; i < nameParts.Length; i++)
            {
                if (property.Attributes["Type"] == null)
                {
                    return false;
                }
                string type = property.Attributes["Type"].Value;
                int dotIndex = type.LastIndexOf('.');
                if (dotIndex == -1 || dotIndex == 0 || dotIndex == type.Length - 1)
                {
                    return false;
                }

                string ns = type.Substring(0, dotIndex);
                string shortName = type.Substring(dotIndex + 1);
                property = entityType.ParentNode.ParentNode.SelectSingleNode(string.Format(@"./*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{0}')]/*[@Name='{1}']", ns, shortName));

                if (property == null)
                {
                    return false;
                }

                property = property.SelectSingleNode(string.Format(@"./*[@Name='{0}']", nameParts[i]));
                if (property == null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the key property name from a specified service.
        /// </summary>
        /// <param name="entityTypeShortName">Output the entity-type short name.</param>
        /// <returns>Returns the key property name and type.</returns>
        public static Tuple<string, string> GetKeyProperty(out string entityTypeShortName)
        {
            entityTypeShortName = string.Empty;
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = "//*[local-name()='EntityType']";
            var etElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            foreach (var etElem in etElems)
            {
                if (null == etElem.Attribute("Name"))
                {
                    continue;
                }

                entityTypeShortName = etElem.GetAttributeValue("Name");
                xPath = "./*[local-name()='Key']/*[local-name()='PropertyRef']";
                var propRefElems = etElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                if (null == propRefElems || propRefElems.Count() > 1 || null == propRefElems.First().Attribute("Name"))
                {
                    continue;
                }

                string keyPropName = propRefElems.First().GetAttributeValue("Name");
                xPath = "./*[local-name()='Property']";
                var propElems = etElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                foreach (var propElem in propElems)
                {
                    if (null == propElem.Attribute("Name") || null == propElem.Attribute("Type"))
                    {
                        continue;
                    }

                    if (MetadataHelper.IsKeyPropertyType(propElem.GetAttributeValue("Type")) &&
                        keyPropName == propElem.GetAttributeValue("Name"))
                    {
                        return new Tuple<string, string>(propElem.GetAttributeValue("Name"), propElem.GetAttributeValue("Type"));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the key property name from a specified service.
        /// </summary>
        /// <param name="entityTypeShortName">The entity-type short name.</param>
        /// <returns>Returns the key property name and type.</returns>
        public static Tuple<string, string> GetKeyProperty(string entityTypeShortName)
        {
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            var etElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            xPath = "./*[local-name()='Key']/*[local-name()='PropertyRef']";
            var propRefElems = etElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            if (propRefElems.Count() > 1)
            {
                return null;
            }

            string keyPropName = propRefElems.First().GetAttributeValue("Name");
            xPath = string.Format("./*[local-name()='Property' and @Name='{0}']", keyPropName);
            var propElem = etElem.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != propElem.Attribute("Name") &&
                null != propElem.Attribute("Type") &&
                MetadataHelper.IsKeyPropertyType(propElem.GetAttributeValue("Type")))
            {
                return new Tuple<string, string>(propElem.GetAttributeValue("Name"), propElem.GetAttributeValue("Type"));
            }

            return null;
        }

        /// <summary>
        /// Get all the entity-set URLs from the service.
        /// </summary>
        /// <returns>Returns the entity-set URLs' list.</returns>
        public static List<string> GetEntitySetURLs()
        {
            var jObj = JObject.Parse(ServiceStatus.GetInstance().ServiceDocument);
            var jArr = jObj.GetValue("value") as JArray;
            var result = new List<string>();
            foreach (JObject elem in jArr)
            {
                if (null != elem["url"])
                {
                    result.Add(elem["url"].ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// Get all the paging limit entity-set URLs from the service.
        /// </summary>
        /// <returns>Returns the entity-set URLs' list.</returns>
        public static List<string> GetPagingLimitEntitySetURLs()
        {
            var result = new List<string>();
            var entitySetUrls = MetadataHelper.GetEntitySetURLs();
            foreach (var entitySetUrl in entitySetUrls)
            {
                string url = ServiceStatus.GetInstance().RootURL.TrimEnd('/') + "/" + entitySetUrl;
                var resp = WebHelper.Get(new Uri(url), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);
                if (null != resp && HttpStatusCode.OK == resp.StatusCode)
                {
                    var jObj = JObject.Parse(resp.ResponsePayload);
                    if (null != jObj[Constants.V4OdataNextLink])
                    {
                        result.Add(entitySetUrl);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the property name with the specified type.
        /// </summary>
        /// <param name="propertyType">The specified type of a property.</param>
        /// <param name="entityTypeShortName">The entity-type short name.</param>
        /// <returns>Returns all the eligible property names in an entity-type.</returns>
        public static List<string> GetPropertyNames(string propertyType, out string entityTypeShortName)
        {
            var propertyTypes = new string[1] { propertyType };

            return MetadataHelper.GetPropertyNames(propertyTypes, out entityTypeShortName);
        }

        /// <summary>
        /// Get the property name with the specified types.
        /// </summary>
        /// <param name="propertyTypes">The specified types of properties.</param>
        /// <param name="entityTypeShortName">The entity-type short name.</param>
        /// <returns>Returns all the eligible property names in an entity-type.</returns>
        public static List<string> GetPropertyNames(IEnumerable<string> propertyTypes, out string entityTypeShortName)
        {
            var result = new List<string>();
            entityTypeShortName = string.Empty;
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = "//*[local-name()='EntityType']";
            var etElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            foreach (var etElem in etElems)
            {
                if (null == etElem.Attribute("Name"))
                {
                    continue;
                }

                if (null != etElem.Attribute("HasStream"))
                {
                    continue;
                }

                entityTypeShortName = etElem.GetAttributeValue("Name");
                xPath = "./*[local-name()='Property']";
                var propElems = etElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                if (null != propElems && propElems.Any())
                {
                    foreach (var propElem in propElems)
                    {
                        if (null != propElem.Attribute("Type"))
                        {
                            string type = propElem.GetAttributeValue("Type");
                            if (propertyTypes.Contains(type))
                            {
                                result.Add(propElem.GetAttributeValue("Name"));
                            }
                        }
                    }

                    if (result.Any())
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get property names from an entity-type.
        /// </summary>
        /// <param name="entityTypeShortName">Output the entity-type short name.</param>
        /// <returns>Returns all the property names in the entity-type.</returns>
        public static List<string> GetPropertyNames(out string entityTypeShortName)
        {
            var result = new List<string>();
            entityTypeShortName = string.Empty;
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = "//*[local-name()='EntityType']";
            var etElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            foreach (var etElem in etElems)
            {
                if (null == etElem.Attribute("Name"))
                {
                    continue;
                }

                entityTypeShortName = etElem.GetAttributeValue("Name");
                xPath = "./*[local-name()='Property']";
                var propElems = etElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                if (null != propElems && propElems.Any())
                {
                    foreach (var propElem in propElems)
                    {
                        if (null != propElem.Attribute("Name"))
                        {
                            result.Add(propElem.GetAttributeValue("Name"));
                        }
                    }

                    if (result.Any())
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get numeric property name from an entity-type.
        /// </summary>
        /// <param name="entityTypeShortName">Output the entity-type short name.</param>
        /// <returns>Returns all the numeric property names in the entity-type.</returns>
        public static List<string> GetNumericPropertyNames(out string entityTypeShortName)
        {
            var result = new List<string>();
            entityTypeShortName = string.Empty;
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = "//*[local-name()='EntityType']";
            var etElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            foreach (var etElem in etElems)
            {
                if (null == etElem.Attribute("Name"))
                {
                    continue;
                }

                entityTypeShortName = etElem.GetAttributeValue("Name");
                xPath = "./*[local-name()='Property']";
                var propElems = etElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                if (null != propElems && propElems.Any())
                {
                    foreach (var propElem in propElems)
                    {
                        if (null != propElem.Attribute("Name") &&
                            null != propElem.Attribute("Type") &&
                            MetadataHelper.IsNumericPropertyType(propElem.GetAttributeValue("Type")))
                        {
                            result.Add(propElem.GetAttributeValue("Name"));
                        }
                    }

                    if (result.Any())
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the navigation property names from an entity-type.
        /// </summary>
        /// <param name="entityTypeShortName">Output the entity-type short name.</param>
        /// <returns>Returns all the navigation property name in the entity-type.</returns>
        public static List<string> GetNavigationPropertyNames(out string entityTypeShortName, IEnumerable<string> entityTypeShortNames = null)
        {
            var result = new List<string>();
            entityTypeShortName = string.Empty;
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = "//*[local-name()='EntityType']";
            var etElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            foreach (var etElem in etElems)
            {
                if (null == etElem.Attribute("Name"))
                {
                    continue;
                }

                if (null != entityTypeShortNames &&
                    entityTypeShortNames.Any() &&
                    entityTypeShortNames.Contains(etElem.GetAttributeValue("Name")))
                {
                    continue;
                }

                entityTypeShortName = etElem.GetAttributeValue("Name");
                xPath = "./*[local-name()='NavigationProperty']";
                var propElems = etElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
                if (null != propElems && propElems.Any())
                {
                    foreach (var propElem in propElems)
                    {
                        result.Add(propElem.GetAttributeValue("Name"));
                    }

                    if (result.Any())
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Verify the specified key property type.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>Returns the verification result.</returns>
        private static bool IsKeyPropertyType(string propertyType)
        {
            if ("Edm.Int16" == propertyType || "Edm.Int32" == propertyType ||
                "Edm.Int64" == propertyType || "Edm.String" == propertyType)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Verify the specified property is a numeric property.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>Returns the verification result.</returns>
        private static bool IsNumericPropertyType(string propertyType)
        {
            if ("Edm.Int16" == propertyType || "Edm.Int32" == propertyType ||
                "Edm.Int64" == propertyType || "Edm.Decimal" == propertyType ||
                "Edm.Double" == propertyType || "Edm.Single" == propertyType)
            {
                return true;
            }

            return false;
        }
    }
}
