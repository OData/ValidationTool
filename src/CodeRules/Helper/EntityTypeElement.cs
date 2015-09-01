// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The entity type class.
    /// </summary>
    public class EntityTypeElement
    {
        public EntityTypeElement()
        {
        }

        /// <summary>
        /// The constructor of the EntityTypeElement class.
        /// </summary>
        /// <param name="entityTypeShortName">The entity-type short name.</param>
        /// <param name="normalProperties">All the properties in the specified entity-type or its base-type.</param>
        /// <param name="propsDict">The properties' dictionary.</param>
        /// <param name="navigationProperties">All the navigation properties in the specified entity-type or its base-type.</param>
        /// <param name="navigPropsDict">The navigation properties' dictionary.</param>
        /// <param name="entityTypeNamespace">The namespace of an entity-type.</param>
        /// <param name="entityTypeAlias">The alias of an entity-type.</param>
        /// <param name="hasStream">The attribute indicates whether the specified entity-type has stream.</param>
        /// <param name="isOpenType">The attribute indicates whether the specified entity-tyep is an open type.</param>
        /// <param name="baseTypeFullName">The base-type full name.</param>
        public EntityTypeElement(
            string entityTypeShortName,
            IEnumerable<NormalProperty> normalProperties,
            Dictionary<string, string> propsDict,
            IEnumerable<NavigProperty> navigationProperties = null,
            Dictionary<string, string> navigPropsDict = null,
            string entityTypeNamespace = null,
            string entityTypeAlias = null,
            bool hasStream = false,
            bool isOpenType = false,
            string baseTypeFullName = null)
        {
            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                throw new ArgumentNullException("entityTypeShortName", "The value of input parameter 'entityTypeShortName' MUST NOT be null or empty.");
            }

            if (normalProperties == null && baseTypeFullName == null)
            {
                throw new ArgumentNullException("baseTypeFullName", "The value of input parameters 'normalProperties' and 'baseTypeFullName' MUST NOT be null at the same time.");
            }

            this.EntityTypeNamespace = entityTypeNamespace;
            this.EntityTypeAlias = entityTypeAlias;
            this.EntityTypeShortName = entityTypeShortName;
            this.NormalProperties = null != normalProperties ? normalProperties.ToList() : null;
            this.PropertiesDict = propsDict;
            this.NavigationProperties = null != navigationProperties ? navigationProperties.ToList() : null;
            this.NavigPropertiesDict = navigPropsDict;
            this.HasStream = hasStream;
            this.IsOpenType = isOpenType;
            this.BaseTypeFullName = baseTypeFullName;
        }

        /// <summary>
        /// Gets or private sets an entity type's namespace.
        /// </summary>
        public string EntityTypeNamespace
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets an entity type's alias.
        /// </summary>
        public string EntityTypeAlias
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets the short name of an entity type.
        /// </summary>
        public string EntityTypeShortName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the full name of an entity type.
        /// </summary>
        public string EntityTypeFullName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.EntityTypeShortName) && !string.IsNullOrEmpty(this.EntityTypeNamespace))
                { 
                    return string.Format("{0}.{1}", this.EntityTypeNamespace, this.EntityTypeShortName);
                }
                else if(!string.IsNullOrEmpty(this.EntityTypeShortName) && !string.IsNullOrEmpty(this.EntityTypeAlias))
                {
                    return string.Format("{0}.{1}", this.EntityTypeAlias, this.EntityTypeShortName);
                }
                else
                {
                    return this.EntityTypeShortName;
                }
            }
        }

        /// <summary>
        /// Gets the the base type short name of an entity type.
        /// </summary>
        public string BaseTypeShortName
        {
            get
            {
                return this.BaseTypeFullName.GetLastSegment();
            }
        }

        /// <summary>
        /// Gets or private sets the base type full name of an entity type.
        /// </summary>
        public string BaseTypeFullName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets the HasStream attribute of an entity type.
        /// </summary>
        public bool HasStream
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets the open type flag of an entity type.
        /// </summary>
        public bool IsOpenType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets the key properties of an entity type.
        /// </summary>
        public List<NormalProperty> KeyProperties
        {
            get
            {
                var keyProperties = this.NormalProperties.Where(p => p.IsKey).Select(p => p);

                return null != keyProperties ? keyProperties.ToList() : null;
            }
        }

        /// <summary>
        /// Gets or private sets the normal properties of an entity type.
        /// </summary>
        public List<NormalProperty> NormalProperties
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets the navigation properties of an entity type.
        /// </summary>
        public List<NavigProperty> NavigationProperties
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the entity-set name of an entity.
        /// </summary>
        public string EntitySetName
        {
            get
            {
                return this.EntityTypeShortName.MapEntityTypeShortNameToEntitySetName();
            }
        }

        /// <summary>
        /// Gets or private sets the properties dictionary.
        /// </summary>
        public Dictionary<string, string> PropertiesDict
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets the navigation dictionary.
        /// </summary>
        public Dictionary<string, string> NavigPropertiesDict
        {
            get;
            private set;
        }

        /// <summary>
        /// Parse a parameter with the type XElement.
        /// </summary>
        /// <param name="entityTypeElement">A parameter with the type XElement.</param>
        /// <returns>Returns the result with the type EntityTypeElement.</returns>
        public static EntityTypeElement Parse(XElement entityTypeElement)
        {
            if (entityTypeElement == null)
            {
                throw new ArgumentNullException("entityTypeElement", "The value of the input parameter 'entityTypeElement' MUST NOT be null.");
            }

            if (!entityTypeElement.Name.LocalName.Equals("EntityType"))
            {
                throw new ArgumentException("The local-name of the input parameter 'entityTypeElement' is incorrect.");
            }

            // Set the alias and namespace of the entity-type element.
            var aliasAndNamespace = entityTypeElement.GetAliasAndNamespace();
            string entityTypeNamespace = aliasAndNamespace.Namespace;
            string entityTypeAlias = aliasAndNamespace.Alias;

            // Set the entity-type short name.
            string entityTypeShortName =
                entityTypeElement.Attribute("Name") != null ?
                entityTypeElement.GetAttributeValue("Name") :
                null;

            // Set the entity-type full name.
            string entityTypeFullName = entityTypeShortName;
            if (!string.IsNullOrEmpty(entityTypeNamespace))
            {
                entityTypeFullName = string.Format("{0}.{1}", entityTypeNamespace, entityTypeShortName);
            }
            else if(!string.IsNullOrEmpty(entityTypeAlias))
            {
                entityTypeFullName = string.Format("{0}.{1}", entityTypeAlias, entityTypeShortName);
            }

            // Set the base-type full name.
            string baseTypeFullName =
                entityTypeElement.Attribute("BaseType") != null ?
                entityTypeElement.GetAttributeValue("BaseType") :
                null;

            // Set the HasStream attribute of the entity-type element.
            bool hasStream =
                entityTypeElement.Attribute("HasStream") != null ?
                Convert.ToBoolean(entityTypeElement.GetAttributeValue("HasStream")) :
                false;

            // Set the OpenType attribute of the entity-type element.
            bool isOpenType =
                entityTypeElement.Attribute("OpenType") != null ?
                Convert.ToBoolean(entityTypeElement.GetAttributeValue("OpenType")) :
                false;

            EntityTypeElement baseTypeElem = null;
            List<NormalProperty> normalProperties = new List<NormalProperty>();
            var propsDict = new Dictionary<string, string>();
            List<NavigProperty> navigProperties = new List<NavigProperty>();
            var navigPropsDict = new Dictionary<string, string>();
            List<string> keyPropNames = new List<string>();
            if (!string.IsNullOrEmpty(baseTypeFullName))
            {
                var baseTypeShortName = baseTypeFullName.GetLastSegment();
                var baseTypeElement = entityTypeElement.Parent != null ?
                    entityTypeElement.Parent.Elements()
                    .Where(et => null != et.Attribute("Name") && et.GetAttributeValue("Name").Equals(baseTypeShortName))
                    .Select(et => et).First() : null;
                baseTypeElem = EntityTypeElement.Parse(baseTypeElement);
                normalProperties.AddRange(baseTypeElem.NormalProperties);
                foreach (var dict in baseTypeElem.PropertiesDict)
                {
                    propsDict.Add(dict.Key, dict.Value);
                }

                navigProperties.AddRange(baseTypeElem.NavigationProperties);
                foreach (var dict in baseTypeElem.NavigPropertiesDict)
                {
                    navigPropsDict.Add(dict.Key, dict.Value);
                }

                keyPropNames.AddRange(baseTypeElem.KeyProperties.Select(kp => kp.PropertyName));
            }

            var allProperties = entityTypeElement.Elements();
            foreach (var prop in allProperties)
            {
                // Records the key properties of the entity-type.
                if (prop.Name.LocalName.Equals("Key"))
                {
                    var keyProps = prop.Elements();
                    if (null == keyProps || !keyProps.Any())
                    {
                        continue;
                    }

                    foreach(var kp in keyProps)
                    {
                        if(null != kp.Attribute("Name"))
                        {
                            var kpName = kp.GetAttributeValue("Name");
                            if (kpName.Contains("/"))
                            {
                                kpName = kpName.Substring(0, kpName.IndexOf('/'));
                            }

                            keyPropNames.Add(kpName);
                        }
                    }
                }
                // Records the properties of the entity-type.
                else if (prop.Name.LocalName.Equals("Property"))
                {
                    string propName = prop.Attribute("Name") != null ? prop.GetAttributeValue("Name") : null;
                    string propType = prop.Attribute("Type") != null ? prop.GetAttributeValue("Type") : null;
                    bool isKey = keyPropNames.Contains(propName) ? true : false;
                    bool isNullable = prop.Attribute("Nullable") != null ? Convert.ToBoolean(prop.GetAttributeValue("Nullable")) : true;
                    bool isValueNull = prop.Value == null;
                    string srid = prop.Attribute("SRID") != null ? prop.GetAttributeValue("SRID") : null;
                    propsDict.Add(propName, entityTypeFullName);
                    normalProperties.Add(new NormalProperty(propName, propType, isKey, isNullable, isValueNull, srid));
                }
                // Records the navigation properties of the entity-type.
                else if (prop.Name.LocalName.Equals("NavigationProperty"))
                {
                    string navigPropName = prop.Attribute("Name") != null ? prop.GetAttributeValue("Name") : null;
                    string navigPropType = prop.Attribute("Type") != null ? prop.GetAttributeValue("Type") : null;
                    string navigPropPartner = prop.Attribute("Partner") != null ? prop.GetAttributeValue("Partner") : null;
                    navigPropsDict.Add(navigPropName, entityTypeFullName);
                    navigProperties.Add(new NavigProperty(navigPropName, navigPropType, navigPropPartner));
                }
            }

            return new EntityTypeElement(entityTypeShortName, normalProperties, propsDict, navigProperties, navigPropsDict, entityTypeNamespace, entityTypeAlias, hasStream, isOpenType, baseTypeFullName);
        }
    }
}
