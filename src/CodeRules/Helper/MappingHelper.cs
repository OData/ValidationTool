// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The ConvertHelper static class.
    /// </summary>
    public static class MappingHelper
    {
        /// <summary>
        /// Static constructor.
        /// </summary>
        static MappingHelper()
        {
            ParamNullErrorMsgPattern = "The value of the parameter '{0}' cannot be null or empty.";
            ParamNotFoundErrorMsgPattern = "There is no '{0}' has the same {1} with the value of the parameter '{2}' in the service.";
        }

        /// <summary>
        /// Map entity-type full name to its related entity-set name.
        /// </summary>
        /// <param name="entityTypeFullName">An entity-type full name.</param>
        /// <returns>Returns the related entity-set name.</returns>
        public static string MapEntityTypeFullNameToEntitySetName(this string entityTypeFullName)
        {
            if (string.IsNullOrEmpty(entityTypeFullName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entityTypeFullName"));
            }

            if (!IsSpecifiedEntityTypeFullNameExist(entityTypeFullName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-type", "full name", "entityTypeFullName"));
            }

            XElement metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='EntitySet' and @EntityType='{0}']", entityTypeFullName);
            var entitySetElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != entitySetElem && null != entitySetElem.Attribute("Name") ? 
                entitySetElem.GetAttributeValue("Name") : string.Empty;
        }

        /// <summary>
        /// Map entity-set name to its related entity-type full name.
        /// </summary>
        /// <param name="entitySetName">An entity-set name.</param>
        /// <returns>Returns the related entity-set name.</returns>
        public static string MapEntitySetNameToEntityTypeFullName(this string entitySetName)
        {
            if (string.IsNullOrEmpty(entitySetName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entitySetName"));
            }

            if (!IsSpecifiedEntitySetNameExist(entitySetName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-set", "name", "entitySetName"));
            }

            XElement metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='EntitySet' and @Name='{0}']", entitySetName);
            var entitySetElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != entitySetElem && null != entitySetElem.Attribute("EntityType") ? 
                entitySetElem.GetAttributeValue("EntityType") : string.Empty;
        }

        /// <summary>
        /// Map entity-type short name to its related entity-set name.
        /// </summary>
        /// <param name="entityTypeShortName">An entity-type short name.</param>
        /// <returns>Returns the related entity-set name.</returns>
        public static string MapEntityTypeShortNameToEntitySetName(this string entityTypeShortName)
        {
            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entityTypeShortName"));
            }

            if (!IsSpecifiedEntityTypeShortNameExist(entityTypeShortName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-type", "short name", "entityTypeShortName"));
            }

            string entityTypeFullName = entityTypeShortName.AddNamespace(AppliesToType.EntityType, ServiceStatus.GetInstance().MetadataDocument);

            return entityTypeFullName.MapEntityTypeFullNameToEntitySetName();
        }

        /// <summary>
        /// Map entity-set name to its related entity-type short name.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <returns>Returns the related entity-type short name.</returns>
        public static string MapEntitySetNameToEntityTypeShortName(this string entitySetName)
        {
            if (string.IsNullOrEmpty(entitySetName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entitySetName"));
            }

            if (!IsSpecifiedEntitySetNameExist(entitySetName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-set", "name", "entitySetName"));
            }

            return entitySetName.MapEntitySetNameToEntityTypeFullName().GetLastSegment();
        }

        /// <summary>
        /// Map entity-set name to its related entity-set URL.
        /// </summary>
        /// <param name="entitySetName">An entity-set name.</param>
        /// <returns>Returns the related entity-set URL.</returns>
        public static string MapEntitySetNameToEntitySetURL(this string entitySetName)
        {
            if (string.IsNullOrEmpty(entitySetName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entitySetName"));
            }

            if (!IsSpecifiedEntitySetNameExist(entitySetName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-set", "name", "entitySetName"));
            }

            JObject serviceRoot = JObject.Parse(ServiceStatus.GetInstance().ServiceDocument);
            var entries = JsonParserHelper.GetEntries(serviceRoot);

            if (null != entries)
            {
                foreach (var entry in entries)
                {
                    if(null != entry["name"] && entitySetName == entry["name"].Value<string>() &&
                       null != entry["kind"] && "EntitySet" == entry["kind"].Value<string>())
                    {
                        return null != entry["url"] ? entry["url"].Value<string>() : string.Empty;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Map entity-set URL to entity-set name.
        /// </summary>
        /// <param name="entitySetURL">An entity-set URL.</param>
        /// <returns>Returns the related entity-set name.</returns>
        public static string MapEntitySetURLToEntitySetName(this string entitySetURL)
        {
            if (string.IsNullOrEmpty(entitySetURL))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entitySetURL"));
            }

            if (!IsSpecifiedEntitySetNameExist(entitySetURL))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-set", "URL", "entitySetURL"));
            }

            JObject serviceRoot = JObject.Parse(ServiceStatus.GetInstance().ServiceDocument);
            var entries = JsonParserHelper.GetEntries(serviceRoot);

            if (null != entries)
            {
                foreach (var entry in entries)
                {
                    if (null != entry["url"] && entitySetURL == entry["url"].Value<string>() &&
                        null != entry["kind"] && "EntitySet" == entry["kind"].Value<string>())
                    {
                        return null != entry["name"] ? entry["name"].Value<string>() : string.Empty;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Map entity-type full name to entity-set URL.
        /// </summary>
        /// <param name="entityTypeFullName">An entity-type full name.</param>
        /// <returns>Returns the entity-set URL.</returns>
        public static string MapEntityTypeFullNameToEntitySetURL(this string entityTypeFullName)
        {
            if (string.IsNullOrEmpty(entityTypeFullName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entityTypeFullName"));
            }

            if (!IsSpecifiedEntitySetNameExist(entityTypeFullName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-type", "full name", "entityTypeFullName"));
            }

            // Map entity-type full name to entity-set name.
            string entitySetName = entityTypeFullName.MapEntityTypeFullNameToEntitySetName();

            if (!string.IsNullOrEmpty(entitySetName))
            {
                return entitySetName.MapEntitySetNameToEntitySetURL();
            }

            return string.Empty;
        }

        /// <summary>
        /// Map entity-set URL to entity-type full name.
        /// </summary>
        /// <param name="entitySetURL">An entity-set URL.</param>
        /// <returns>Returns entity-type full name.</returns>
        public static string MapEntitySetURLToEntityTypeFullName(this string entitySetURL)
        {
            if (string.IsNullOrEmpty(entitySetURL))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entitySetURL"));
            }

            if (!IsSpecifiedEntitySetNameExist(entitySetURL))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-set", "URL", "entitySetURL"));
            }

            var entitySetName = entitySetURL.MapEntitySetURLToEntitySetName();

            if (!string.IsNullOrEmpty(entitySetName))
            {
                return entitySetName.MapEntitySetNameToEntityTypeFullName();
            }

            return string.Empty;
        }

        /// <summary>
        /// Map entity-type short name to entity-set URL.
        /// </summary>
        /// <param name="entityTypeShortName">An entity-type short name.</param>
        /// <returns>Returns the related entity-set URL.</returns>
        public static string MapEntityTypeShortNameToEntitySetURL(this string entityTypeShortName)
        {
            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entityTypeShortName"));
            }

            if (!IsSpecifiedEntitySetNameExist(entityTypeShortName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-type", "short name", "entityTypeShortName"));
            }

            string entityTypeFullName = entityTypeShortName.AddNamespace(AppliesToType.EntityType, ServiceStatus.GetInstance().MetadataDocument);

            if (!string.IsNullOrEmpty(entityTypeFullName))
            {
                string entitySetName = entityTypeFullName.MapEntityTypeFullNameToEntitySetName();

                if (!string.IsNullOrEmpty(entitySetName))
                {
                    return entitySetName.MapEntitySetNameToEntitySetURL();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Map entity-set URL to entity-type short name.
        /// </summary>
        /// <param name="entitySetURL">An entity-set URL.</param>
        /// <returns>Returns the related entity-type short name.</returns>
        public static string MapEntitySetURLToEntityTypeShortName(this string entitySetURL)
        {
            if (string.IsNullOrEmpty(entitySetURL))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entitySetURL"));
            }

            if (!IsSpecifiedEntitySetNameExist(entitySetURL))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-set", "URL", "entitySetURL"));
            }

            string entitySetName = entitySetURL.MapEntitySetURLToEntitySetName();

            if (!string.IsNullOrEmpty(entitySetName))
            {
                string entityTypeFullName = entitySetName.MapEntitySetNameToEntityTypeFullName();

                if (!string.IsNullOrEmpty(entityTypeFullName))
                {
                    return entityTypeFullName.GetLastSegment();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Map navigation property name to its related entity-set name.
        /// </summary>
        /// <param name="navigationPropertyName">A navigation property name.</param>
        /// <param name="entityTypeShortName">An entity-type short name which contains specifed navigation property name.</param>
        /// <returns>Returns the related entity-set name.</returns>
        public static string MapNavigationPropertyNameToEntitySetName(this string navigationPropertyName, string entityTypeShortName)
        {
            if (string.IsNullOrEmpty(navigationPropertyName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "navigationPropertyName"));
            }

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entityTypeShortName"));
            }

            if (!IsSpecifiedEntityTypeShortNameExist(entityTypeShortName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-type", "short name", "entityTypeShortName"));
            }

            //string entitySetURL = string.Empty;
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty' and @Name='{1}']", entityTypeShortName, navigationPropertyName);
            var navigationPropertyElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            if (null != navigationPropertyElem)
            {
                var navigationPropertyType = null != navigationPropertyElem.Attribute("Type") ?
                    navigationPropertyElem.GetAttributeValue("Type") : string.Empty;

                if (!string.IsNullOrEmpty(navigationPropertyType))
                {
                    return navigationPropertyType

                        // Remove the collection flag from navigation property type, in order to map it to entity-type full name. 
                        .RemoveCollectionFlag()

                        // Map the entity-type full name to entity-set name.
                        .MapEntityTypeFullNameToEntitySetName();
                }
            }
            else
            {
                xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
                navigationPropertyElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                if (null != navigationPropertyElem.Attribute("BaseType"))
                {
                    string baseEntityTypeShortName = navigationPropertyElem.GetAttributeValue("BaseType").GetLastSegment();

                    return MapNavigationPropertyNameToEntitySetName(navigationPropertyName, baseEntityTypeShortName);
                }
                else
                {
                    throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "navigation property", "name", "navigationPropertyName"));
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Map navigation property name to its related entity-set URL.
        /// </summary>
        /// <param name="navigationPropertyName">A navigation property name.</param>
        /// <param name="entityTypeShortName">An entity-type short name which contains specifed navigation property name.</param>
        /// <returns>Returns the related entity-set URL.</returns>
        public static string MapNavigationPropertyNameToEntitySetURL(this string navigationPropertyName, string entityTypeShortName)
        {
            if (string.IsNullOrEmpty(navigationPropertyName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "navigationPropertyName"));
            }

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "entityTypeShortName"));
            }

            if (!IsSpecifiedEntityTypeShortNameExist(entityTypeShortName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "entity-type", "short name", "entityTypeShortName"));
            }

            var entitySetName = MapNavigationPropertyNameToEntitySetName(navigationPropertyName, entityTypeShortName);

            if (!string.IsNullOrEmpty(entitySetName))
            {
                return entitySetName.MapEntitySetNameToEntitySetURL();
            }

            return string.Empty;
        }

        /// <summary>
        /// Map singleton name to its related entity type full name.
        /// </summary>
        /// <param name="singletonName">A singleton name.</param>
        /// <returns>Returns the related entity-type full name.</returns>
        public static string MapSingletonNameToEntityTypeFullName(this string singletonName)
        {
            if (string.IsNullOrEmpty(singletonName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "singletonName"));
            }

            if (!IsSpecifiedEntitySetNameExist(singletonName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "singleton", "name", "singletonName"));
            }

            XElement metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='Singleton' and @Name='{0}']", singletonName);
            var singletonElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != singletonElem && null != singletonElem.Attribute("Type") ?
                singletonElem.GetAttributeValue("Type") : string.Empty;
        }

        /// <summary>
        /// Map singleton name to its related entity type short name.
        /// </summary>
        /// <param name="singletonName">A singleton name.</param>
        /// <returns>Returns the related entity-type short name.</returns>
        public static string MapSingletonNameToEntityTypeShortName(this string singletonName)
        {
            if (string.IsNullOrEmpty(singletonName))
            {
                throw new ArgumentNullException(string.Format(ParamNullErrorMsgPattern, "singletonName"));
            }

            if (!IsSpecifiedEntitySetNameExist(singletonName))
            {
                throw new ArgumentException(string.Format(ParamNotFoundErrorMsgPattern, "singleton", "name", "singletonName"));
            }

            return singletonName.MapSingletonNameToEntityTypeFullName().GetLastSegment();
        }

        /// <summary>
        /// Add namespace prefix for any names of the element in metadata document.
        /// </summary>
        /// <param name="targetName">The target element's name.</param>
        /// <param name="appliesType">The applies type of the element. (Related to the element's local-name.)</param>
        /// <param name="metadataDoc">The metadata document.</param>
        /// <returns>Returns the name with the namespace of the element.</returns>
        public static string AddNamespace(this string targetName, AppliesToType appliesType)
        {
            if (string.IsNullOrEmpty(targetName))
            {
                return string.Empty;
            }

            string target = targetName.GetLastSegment();
            XElement metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
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
        /// Verify whether an entity-type short name is existent in the service.
        /// </summary>
        /// <param name="entityTypeShortName">An entity-type short name. 
        /// Note: This name can be only entity-type short name.</param>
        /// <returns>Returns true if exist, otherwise false.</returns>
        public static bool IsSpecifiedEntityTypeShortNameExist(this string entityTypeShortName)
        {
            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                return false;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", entityTypeShortName);
            var entityTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != entityTypeElem;
        }

        /// <summary>
        /// Verify whether an entity-type full name is existent in the service.
        /// </summary>
        /// <param name="entityTypeFullName">An entity-type full name.</param>
        /// <returns>Returns true if exist, otherwise false.</returns>
        public static bool IsSpecifiedEntityTypeFullNameExist(this string entityTypeFullName)
        {
            if (string.IsNullOrEmpty(entityTypeFullName))
            {
                return false;
            }

            string entityTypeShortName = entityTypeFullName.GetLastSegment();
            string comparedName = entityTypeShortName.AddNamespace(AppliesToType.EntityType, ServiceStatus.GetInstance().MetadataDocument);

            return comparedName == entityTypeFullName;
        }

        /// <summary>
        /// Verify whether a complex type short name is existent in the service.
        /// </summary>
        /// <param name="complexTypeShortName">A complex-type short name.</param>
        /// <returns>Returns true if exist, otherwise false.</returns>
        public static bool IsSpecifiedComplexTypeShortNameExist(this string complexTypeShortName)
        {
            if (string.IsNullOrEmpty(complexTypeShortName))
            {
                return false;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='ComplexType' and @Name='{0}']", complexTypeShortName);
            var complexTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != complexTypeElem;
        }

        /// <summary>
        /// Verify whether a complex type full name is existent in the service.
        /// </summary>
        /// <param name="complexTypeFullName">A complex-type full name.</param>
        /// <returns>Returns true if exist, otherwise false.</returns>
        public static bool IsSpecifiedComplexTypeFullNameExist(this string complexTypeFullName)
        {
            if (string.IsNullOrEmpty(complexTypeFullName))
            {
                return false;
            }

            string complexTypeShortName = complexTypeFullName.GetLastSegment();
            string comparedName = complexTypeShortName.AddNamespace(AppliesToType.ComplexType, ServiceStatus.GetInstance().MetadataDocument);

            return comparedName == complexTypeFullName;
        }

        /// <summary>
        /// Verify whether a contained navigation property is existent in the service.
        /// </summary>
        /// <param name="navigPropName">A navigation property name.</param>
        /// <returns>Returns true if exist, otherwise false.</returns>
        public static bool IsSpecifiedContainedNavigPropExist(this string navigPropName)
        {
            if (string.IsNullOrEmpty(navigPropName))
            {
                return false;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='NavigationProperty' and @Name='{0}' and @ContainsTarget='true']", navigPropName);
            var navigPropElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != navigPropElem;
        }

        /// <summary>
        /// Verify whether an type (entity-type or complex-type) full name is existent in the service.
        /// </summary>
        /// <param name="typeFullName">An entity-type full name.</param>
        /// <returns>Returns true if exist, otherwise false.</returns>
        public static bool IsSpecifiedTypeFullNameExist(this string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName))
            {
                return false;
            }

            string typeShortName = typeFullName.GetLastSegment();
            string comparedName1 = typeShortName.AddNamespace(AppliesToType.EntityType, ServiceStatus.GetInstance().MetadataDocument);
            string comparedName2 = typeShortName.AddNamespace(AppliesToType.ComplexType, ServiceStatus.GetInstance().MetadataDocument);

            return comparedName1 == typeFullName || comparedName2 == typeFullName;
        }

        /// <summary>
        /// Verify whether an entity-set name is existent in the service.
        /// </summary>
        /// <param name="entitySetName">An entity-set name.</param>
        /// <returns>Returns true if exist, otherwise false.</returns>
        public static bool IsSpecifiedEntitySetNameExist(this string entitySetName)
        {
            if (string.IsNullOrEmpty(entitySetName))
            {
                return false;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='EntitySet' and @Name='{0}']", entitySetName);
            var entitySetElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != entitySetElem;
        }

        /// <summary>
        /// Verify whether a singleton is existent in the service.
        /// </summary>
        /// <param name="singletonName">A singleton name.</param>
        /// <returns>Returns true if exist, otherwise false.</returns>
        public static bool IsSpecifiedSingletonNameExist(this string singletonName)
        {
            if (string.IsNullOrEmpty(singletonName))
            {
                return false;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format("//*[local-name()='Singleton' and @Name='{0}']", singletonName);
            var singletonElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);

            return null != singletonElem;
        }

        /// <summary>
        /// Verify whether an entity-set URL is existent in the service.
        /// </summary>
        /// <param name="entitySetURL">An entity-set URL.</param>
        /// <returns>Returns true if exist, otherwise false.</returns>
        public static bool IsSpecifiedEntitySetURLExist(this string entitySetURL)
        {
            if (string.IsNullOrEmpty(entitySetURL))
            {
                return false;
            }

            JObject serviceRoot = JObject.Parse(ServiceStatus.GetInstance().ServiceDocument);
            var entries = JsonParserHelper.GetEntries(serviceRoot);

            if (null == entries || !entries.Any())
            {
                return false;
            }

            var entry = entries
                .Where(e => "EntitySet" == e["kind"].Value<string>() && entitySetURL == e["url"].Value<string>())
                .Select(e => e);

            return null != entry;
        }

        private static readonly string ParamNullErrorMsgPattern;
        private static readonly string ParamNotFoundErrorMsgPattern;
    }
}
