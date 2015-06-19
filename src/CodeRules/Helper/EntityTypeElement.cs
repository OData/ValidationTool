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
        /// Initialize the EntityTypeElement class.
        /// </summary>
        /// <param name="entityTypeShortName"></param>
        /// <param name="normalProperties"></param>
        public EntityTypeElement(
            string entityTypeShortName,
            IEnumerable<NormalProperty> normalProperties,
            string entityTypeNamespace = null,
            string entityTypeAlias = null,
            string baseTypeFullName = null,
            bool isOpenType = false,
            IEnumerable<NavigProperty> navigationProperties = null,
            string entitySetName = null)
        {
            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                throw new ArgumentNullException("entityTypeShortName", "The value of input parameter 'entityTypeShortName' MUST NOT be null or empty.");
            }

            if (normalProperties == null && baseTypeFullName == null)
            {
                throw new ArgumentNullException("baseTypeFullName", "The value of input parameters 'normalProperties' and 'baseTypeFullName' MUST NOT be null at the same time.");
            }

            this.entityTypeNamespace = entityTypeNamespace;
            this.entityTypeAlias = entityTypeAlias;
            this.entityTypeShortName = entityTypeShortName;
            this.entityTypeFullName =
                entityTypeNamespace != null ?
                string.Format("{0}.{1}", entityTypeNamespace, entityTypeShortName) :
                entityTypeShortName;
            this.baseTypeFullName = baseTypeFullName;
            this.isOpenType = isOpenType;
            this.normalProperties = normalProperties;
            this.keyProperties = normalProperties.Where(np => np.IsKey).Select(np => np);

            if (this.keyProperties == null || this.keyProperties.Count() == 0)
            {
                throw new KeyNotFoundException("Not found any key properties.");
            }

            this.navigationProperties = navigationProperties;
            this.entitySetName = entitySetName;
        }

        /// <summary>
        /// Gets or sets an entity type's namespace.
        /// </summary>
        public string EntityTypeNamespace
        {
            get
            {
                return entityTypeNamespace;
            }
            set
            {
                entityTypeNamespace = value;
            }
        }

        /// <summary>
        /// Gets or sets an entity type's alias.
        /// </summary>
        public string EntityTypeAlias
        {
            get
            {
                return entityTypeAlias;
            }
            set
            {
                entityTypeAlias = value;
            }
        }

        /// <summary>
        /// Gets or sets the short name of an entity type.
        /// </summary>
        public string EntityTypeShortName
        {
            get
            {
                return entityTypeShortName;
            }
            set
            {
                entityTypeShortName = value;
            }
        }

        /// <summary>
        /// Gets or sets the full name of an entity type.
        /// </summary>
        public string EntityTypeFullName
        {
            get
            {
                return entityTypeFullName;
            }
            set
            {
                entityTypeFullName = value;
            }
        }

        /// <summary>
        /// Gets or sets the base type full name of an entity type.
        /// </summary>
        public string BaseTypeFullName
        {
            get
            {
                return baseTypeFullName;
            }
            set
            {
                baseTypeFullName = value;
            }
        }

        /// <summary>
        /// Gets or sets the open type flag of an entity type.
        /// </summary>
        public bool IsOpenType
        {
            get
            {
                return isOpenType;
            }
            set
            {
                isOpenType = value;
            }
        }

        /// <summary>
        /// Gets or sets the key properties of an entity type.
        /// </summary>
        public IEnumerable<NormalProperty> KeyProperties
        {
            get
            {
                return keyProperties;
            }
            set
            {
                keyProperties = value;
            }
        }

        /// <summary>
        /// Gets or sets the normal properties of an entity type.
        /// </summary>
        public IEnumerable<NormalProperty> NormalProperties
        {
            get
            {
                return normalProperties;
            }
            set
            {
                normalProperties = value;
            }
        }

        /// <summary>
        /// Gets or sets the navigation properties of an entity type.
        /// </summary>
        public IEnumerable<NavigProperty> NavigationProperties
        {
            get
            {
                return navigationProperties;
            }
            set
            {
                navigationProperties = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity-set name of an entity.
        /// </summary>
        public string EntitySetName
        {
            get
            {
                return entitySetName;
            }
            set
            {
                entitySetName = value;
            }
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

            if (!entityTypeElement.Name.LocalName.Equals(@"EntityType"))
            {
                throw new ArgumentException("The local-name of the input parameter 'entityTypeElement' is incorrect.");
            }

            XElement rootElement = entityTypeElement;

            while (null != rootElement.Parent)
            {
                rootElement = rootElement.Parent;
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

            // Set the base-type full name.
            string baseTypeFullName =
                entityTypeElement.Attribute("BaseType") != null ?
                entityTypeElement.GetAttributeValue("BaseType") :
                null;

            // Set the OpenType attribute of the entity-type element.
            bool isOpenType =
                entityTypeElement.Attribute("OpenType") != null ?
                Convert.ToBoolean(entityTypeElement.GetAttributeValue("OpenType")) :
                false;

            EntityTypeElement basedEntityTypeElem = null;
            List<NormalProperty> normalProperties = new List<NormalProperty>();
            List<NavigProperty> navigProperties = new List<NavigProperty>();
            List<string> keyPropNames = new List<string>();

            if (baseTypeFullName != null)
            {
                var baseTypeShortName = baseTypeFullName.GetLastSegment();
                var basedEntityTypeElement = entityTypeElement.Parent != null ?
                    entityTypeElement.Parent.Elements()
                    .Where(et => null != et.Attribute("Name") && et.GetAttributeValue("Name").Equals(baseTypeShortName))
                    .Select(et => et).First() : null;
                basedEntityTypeElem = EntityTypeElement.Parse(basedEntityTypeElement);
                normalProperties.AddRange(basedEntityTypeElem.NormalProperties);
                navigProperties.AddRange(basedEntityTypeElem.NavigationProperties);
                keyPropNames.AddRange(basedEntityTypeElem.KeyProperties.Select(kp => kp.PropertyName));
            }

            var allProperties = entityTypeElement.Elements();
            foreach (var prop in allProperties)
            {
                if (prop.Name.LocalName.Equals("Key"))
                {
                    var keyProps = prop.Elements();
                    if (null == keyProps || !keyProps.Any())
                    {
                        continue;
                    }

                    keyProps = keyProps.Where(kp => null != kp.Attribute("Name")).Select(kp => kp);
                    if (null == keyProps || !keyProps.Any())
                    {
                        continue;
                    }

                    foreach (var kp in keyProps)
                    {
                        var kpName = kp.GetAttributeValue("Name");
                        if (kpName.Contains("/"))
                        {
                            kpName = kpName.Substring(0, kpName.IndexOf('/'));
                        }

                        keyPropNames.Add(kpName);
                    }
                }
                else if (prop.Name.LocalName.Equals("Property"))
                {
                    string propName = prop.Attribute("Name") != null ? prop.GetAttributeValue("Name") : null;
                    string propType = prop.Attribute("Type") != null ? prop.GetAttributeValue("Type") : null;
                    bool isKey = keyPropNames.Contains(propName) ? true : false;
                    bool isNullable = prop.Attribute("Nullable") != null ? Convert.ToBoolean(prop.GetAttributeValue("Nullable")) : true;
                    normalProperties.Add(new NormalProperty(propName, propType, isKey, isNullable));
                }
                else if (prop.Name.LocalName.Equals("NavigationProperty"))
                {
                    string navigPropName = prop.Attribute("Name") != null ? prop.GetAttributeValue("Name") : null;
                    string navigPropType = prop.Attribute("Type") != null ? prop.GetAttributeValue("Type") : null;
                    string navigPropPartner = prop.Attribute("Partner") != null ? prop.GetAttributeValue("Partner") : null;
                    navigProperties.Add(new NavigProperty(navigPropName, navigPropType, navigPropPartner));
                }
            }

            // Map entity-type short name to entity-set name.
            string entitySetName = GetEntitySetName(entityTypeElement);

            return new EntityTypeElement(entityTypeShortName, normalProperties, entityTypeNamespace, entityTypeAlias, baseTypeFullName, isOpenType, navigProperties, entitySetName);
        }

        /// <summary>
        /// Map the entity-type to its entity-set name.
        /// </summary>
        /// <param name="entityTypeElement">The entity-type element.</param>
        /// <returns>Returns the entity-set name.</returns>
        private static string GetEntitySetName(XElement entityTypeElement)
        {
            if (null == entityTypeElement)
            {
                return string.Empty;
            }

            var serviceStatus = ServiceStatus.GetInstance();
            string entityTypeShortName = null != entityTypeElement.Attribute("Name") ? entityTypeElement.GetAttributeValue("Name") : string.Empty;
            if (!entityTypeShortName.IsSpecifiedEntityTypeShortNameExist())
            {
                return string.Empty;
            }

            string entitySetName =
                !string.IsNullOrEmpty(entityTypeShortName) ?
                entityTypeShortName.MapEntityTypeShortNameToEntitySetName() :
                string.Empty;
            string baseTypeShortName = null != entityTypeElement.Attribute("BaseType") ? entityTypeElement.GetAttributeValue("BaseType").GetLastSegment() : string.Empty;
            if (string.IsNullOrEmpty(entitySetName) && !string.IsNullOrEmpty(baseTypeShortName))
            {
                var metadata = XElement.Parse(serviceStatus.MetadataDocument);
                while (!string.IsNullOrEmpty(baseTypeShortName))
                {
                    entitySetName = baseTypeShortName.MapEntityTypeShortNameToEntitySetName();
                    if (!string.IsNullOrEmpty(entitySetName))
                    {
                        break;
                    }

                    string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", baseTypeShortName);
                    var xElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    baseTypeShortName = (null != xElem && null != xElem.Attribute("BaseType")) ? 
                        xElem.GetAttributeValue("BaseType").GetLastSegment() : string.Empty;
                }
            }

            return entitySetName;
        }

        /// <summary>
        /// The entity type's namespace.
        /// </summary>
        private string entityTypeNamespace;

        /// <summary>
        /// The entity type's alias.
        /// </summary>
        private string entityTypeAlias;

        /// <summary>
        /// The short name of an entity type.
        /// </summary>
        private string entityTypeShortName;

        /// <summary>
        /// The full name (contains entity type namespace) of an entity type.
        /// </summary>
        private string entityTypeFullName;

        /// <summary>
        /// The base type full name of an entity type.
        /// </summary>
        private string baseTypeFullName;

        /// <summary>
        /// The open type flag of an entity type.
        /// </summary>
        private bool isOpenType;

        /// <summary>
        /// The key properties of an entity type.
        /// </summary>
        private IEnumerable<NormalProperty> keyProperties;

        /// <summary>
        /// The normal properties of an entity type.
        /// </summary>
        private IEnumerable<NormalProperty> normalProperties;

        /// <summary>
        /// The navigation properties of an entity type.
        /// </summary>
        private IEnumerable<NavigProperty> navigationProperties;

        /// <summary>
        /// The entity-set name of an entity.
        /// </summary>
        private string entitySetName;
    }
}
