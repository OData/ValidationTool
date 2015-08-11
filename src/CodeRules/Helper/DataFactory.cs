// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine.Common;
    #endregion

    public class DataFactory
    {
        static DataFactory()
        {
            KeyPropertyTypes = new List<string>()
            {
                "Edm.Int16", "Edm.Int32", "Edm.Int64", "Edm.Guid", "Edm.String"
            };
            IntKeyPropertyTypes = new List<string>() 
            { 
                "Edm.Int16", "Edm.Int32", "Edm.Int64"
            };
            RefID = "ODataValidationToolTestID-{0}";
        }

        /// <summary>
        /// Get a data factory instance.
        /// </summary>
        /// <param name="rootURL">The service root URL.</param>
        /// <returns>Return an data factory instance to construct the data.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static DataFactory Instance()
        {
            // Initialize the increment of the property's key to 1.
            keyIncrement = 1;
            if (null == factory || ServiceStatus.GetInstance().RootURL != factory.rootURL)
            {
                factory = new DataFactory();
            }

            return factory;
        }

        /// <summary>
        /// Construct an entity property's data.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="actualEntityTypeShortName">The actual entity-type short name.</param>
        /// <param name="PropertyName">The property name.</param>
        /// <returns>Returns the custom entity data.</returns>
        public JProperty ConstructPropertyData(
            string entitySetName,
            string actualEntityTypeShortName,
            string PropertyName)
        {
            JObject entity = new JObject();

            if (!entitySetName.IsSpecifiedEntitySetNameExist())
            {
                return null;
            }

            // Map the entity-set name to entity-type short name.
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();
            string targetShortName = string.IsNullOrEmpty(actualEntityTypeShortName) ? entityTypeShortName : actualEntityTypeShortName;
            var normalPropNames = MetadataHelper.GetNormalPropertiesNames(this.metadataDoc, targetShortName);
            var template = this.GetEntityDataTemplate(entitySetName, actualEntityTypeShortName);

            if (null != template)
            {
                var properties = template.Children<JProperty>();

                foreach (var prop in properties)
                {
                    if (PropertyName.Equals(prop.Name))
                    {
                        return prop;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Construct an inserted entity data.
        /// </summary>
        /// <param name="entitySetName">The entity-set name.</param>
        /// <param name="actualEntityTypeShortName">The actual entity-type short name.</param>
        /// <param name="deepInsertedPropNames">The deep inserted property names.</param>
        /// <param name="entityIdsAndRelatedEtags">The dictionary of the entity-ids and its related etags.</param>
        /// <returns>Returns the custom entity data.</returns>
        public JObject ConstructInsertedEntityData(
            string entitySetName,
            string actualEntityTypeShortName,
            List<string> deepInsertedPropNames,
            out List<AdditionalInfo> additionalInfos)
        {
            // Initialize the output parameter.
            additionalInfos = new List<AdditionalInfo>();
            JObject entity = new JObject();

            if (!entitySetName.IsSpecifiedEntitySetNameExist())
            {
                return null;
            }

            // Map the entity-set name to entity-type short name.
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();
            string targetShortName = string.IsNullOrEmpty(actualEntityTypeShortName) ? entityTypeShortName : actualEntityTypeShortName;
            var normalPropNames = MetadataHelper.GetNormalPropertiesNames(this.metadataDoc, targetShortName);
            var template = this.GetEntityDataTemplate(entitySetName, actualEntityTypeShortName);

            if (null != template)
            {
                var properties = template.Children<JProperty>();
                string odataIdVal = string.Empty;
                string odataEtag = string.Empty;
                string odataMedia = string.Empty;

                foreach (var prop in properties)
                {
                    if (normalPropNames.Contains(prop.Name))
                    {
                        entity.Add(prop.Name, prop.Value);
                    }
                    else
                    {
                        if (Constants.V4OdataType == prop.Name)
                        {
                            entity.Add(prop.Name, prop.Value);
                        }
                        else if (Constants.V4OdataId == prop.Name)
                        {
                            odataIdVal = prop.Value.ToString();
                        }
                        else if (Constants.V4OdataEtag == prop.Name)
                        {
                            odataEtag = prop.Value.ToString();
                        }
                        else if (prop.Name.StartsWith(Constants.V4OdataMedia))
                        {
                            odataMedia = prop.Value.ToString();
                        }
                        else if (prop.Name.Contains(Constants.OdataNavigationLinkPropertyNameSuffix) &&
                            null != deepInsertedPropNames && deepInsertedPropNames.Any())
                        {
                            string navigPropName = prop.Name.RemoveEnd(Constants.OdataNavigationLinkPropertyNameSuffix);
                            string navigPropType = MetadataHelper.GetNavigPropertyTypeFromMetadata(navigPropName, targetShortName, this.metadataDoc);
                            string nEntitySetName = navigPropName.MapNavigationPropertyNameToEntitySetName(targetShortName);

                            if (deepInsertedPropNames.Contains(navigPropName) && nEntitySetName.IsSpecifiedEntitySetNameExist())
                            {
                                var nAdditionalInfos = new List<AdditionalInfo>();
                                var nEntity = this.ConstructInsertedEntityData(nEntitySetName, null, null, out nAdditionalInfos);

                                foreach (var nAdditionalInfo in nAdditionalInfos)
                                {
                                    additionalInfos.Add(new AdditionalInfo(nAdditionalInfo.EntityId, nAdditionalInfo.ODataEtag, nAdditionalInfo.ODataMediaEtag));
                                }

                                if (navigPropType.Contains("Collection("))
                                {
                                    entity.Add(navigPropName, new JArray(nEntity));
                                }
                                else
                                {
                                    entity.Add(navigPropName, new JObject(nEntity));
                                }
                            }
                        }
                    }
                }

                additionalInfos.Add(new AdditionalInfo(odataIdVal, odataEtag, odataMedia));
            }

            return entity;
        }

        /// <summary>
        /// Construct an updated entity data basing on the inputted entity.
        /// </summary>
        /// <param name="entity">The inputted entity.</param>
        /// <param name="updatedNormalPropNames">The normal properties which will be updated.</param>
        /// <returns>Returns the updated entity data.</returns>
        public JObject ConstructUpdatedEntityData(JObject entity, IEnumerable<string> updatedNormalPropNames)
        {
            JObject updatedEntity = new JObject();
            var props = entity.Children<JProperty>();

            foreach (var p in props)
            {
                if (updatedNormalPropNames.Contains(p.Name))
                {
                    updatedEntity.Add(p.Name, new JValue(Constants.UpdateData));
                }
                else if (Constants.V4OdataType == p.Name)
                {
                    updatedEntity.Add(p.Name, p.Value);
                }
            }

            return updatedEntity;
        }

        /// <summary>
        /// Re-construct the nullable complex-type properties in an entity.
        /// </summary>
        /// <param name="entity">The target entity.</param>
        /// <param name="complexTypePropNames">The complex type property names.</param>
        /// <returns>Returns the entity with no nullable complex type property.</returns>
        public JObject ReconstructNullableComplexData(JObject entity, List<string> complexTypePropNames)
        {
            if (null == entity || null == complexTypePropNames || !complexTypePropNames.Any())
            {
                return entity;
            }

            string entityTypeShortName = entity[Constants.V4OdataType].ToString().GetLastSegment();
            string pattern = "//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and @Name='{1}']";
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);

            foreach (var complexTypePropName in complexTypePropNames)
            {
                if (null == entity[complexTypePropName] || string.IsNullOrEmpty(entity[complexTypePropName].ToString()))
                {
                    string xPath = string.Format(pattern, entityTypeShortName, complexTypePropName);
                    XElement prop = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    string complexTypeName = prop.GetAttributeValue("Type").GetLastSegment();
                    entity[complexTypePropName] = ConstructAComplexTypeValue(complexTypeName);
                }
            }

            return entity;
        }

        /// <summary>
        /// Construct a complex type value according to its metadata definition.
        /// </summary>
        /// <param name="complexName">The name of the complex type.</param>
        /// <returns>The JSON object value of the complex type.</returns>
        public JObject ConstructAComplexTypeValue(string complexName)
        {
            string metadataDoc = ServiceStatus.GetInstance().MetadataDocument;
            JObject result = new JObject();
            List<string> complextTypeNames = MetadataHelper.GetAllComplexNameFromMetadata(metadataDoc);
            Dictionary<string, string> properties = MetadataHelper.GetAllPropertiesOfComplexType(metadataDoc, complexName);

            foreach (string prop in properties.Keys)
            {
                string type = properties[prop];
                if (EdmTypeManager.IsEdmSimpleType(type))
                {
                    IEdmType edmType = EdmTypeManager.GetEdmType(type);
                    result[prop] = edmType.GetJsonValueTemplate();
                }
                else if (type.Contains("Collection"))
                {
                    string itemType = type.Substring(type.IndexOf('(') + 1, type.Length - 12).GetLastSegment();

                    result[prop] = ConstructCollectionTypeValue(itemType);
                }
                else if (MetadataHelper.IsEnumType(metadataDoc, type))
                {
                    result[prop] = ConstructEnumTypeValue(type);
                }
                else if (complextTypeNames != null && complextTypeNames.Contains(type))
                {
                    result[prop] = ConstructAComplexTypeValue(type);
                }
            }

            return result;
        }

        /// <summary>
        /// Provide value template for a primitive type.
        /// </summary>
        /// <param name="type">The primitive type name.</param>
        /// <returns>The template value of the primitive type.</returns>
        public string ConstructPrimitiveTypeValue(string type)
        {
            if (EdmTypeManager.IsEdmSimpleType(type))
            {
                IEdmType edmType = EdmTypeManager.GetEdmType(type);
                return edmType.GetJsonValueTemplate();
            }

            return null;
        }

        /// <summary>
        /// Get one enumeration value of the enumeration type.
        /// </summary>
        /// <param name="type">The name of the enumeration type.</param>
        /// <returns>A value of the enum type.</returns>
        public string ConstructEnumTypeValue(string type)
        {
            string metadataDoc = ServiceStatus.GetInstance().MetadataDocument;
            if (MetadataHelper.IsEnumType(metadataDoc, type))
            {
                List<string> enumValues = MetadataHelper.GetValuesOfAnEnum(metadataDoc, type);
                Random rnd = new Random();
                int rndNumber = rnd.Next(0, enumValues.Count - 1);
                return enumValues[rndNumber];
            }

            return null;
        }

        /// <summary>
        /// Provide the value tempalte of a collection.
        /// </summary>
        /// <param name="type">The name of the collection content type.</param>
        /// <returns>The JSON array of the collection.</returns>
        public JArray ConstructCollectionTypeValue(string type)
        {
            string metadataDoc = ServiceStatus.GetInstance().MetadataDocument;
            JArray array = new JArray();
            List<string> complextTypeNames = MetadataHelper.GetAllComplexNameFromMetadata(metadataDoc);

            if (EdmTypeManager.IsEdmSimpleType(type))
            {
                string value = ConstructPrimitiveTypeValue(type);
                array.Add(value);
                array.Add(value);
            }
            else if (complextTypeNames != null && complextTypeNames.Count > 0 && complextTypeNames.Contains(type))
            {
                JObject objValue = ConstructAComplexTypeValue(type);
                array.Add(objValue);
                array.Add(objValue);
            }
            else if (MetadataHelper.IsEnumType(metadataDoc, type))
            {
                array.Add(ConstructEnumTypeValue(type));
                array.Add(ConstructEnumTypeValue(type));
            }

            return array;
        }

        /// <summary>
        /// Reinitialize the key increment.
        /// </summary>
        public void ClearKeyIncrement()
        {
            // Set the static parameter 'keyIncrement' to initail value.
            keyIncrement = 1;
        }

        /// <summary>
        /// Initialize the data factory.
        /// </summary>
        private DataFactory()
        {
            ServiceStatus serviceStatus = ServiceStatus.GetInstance();
            this.rootURL = serviceStatus.RootURL;
            this.serviceDoc = serviceStatus.ServiceDocument;
            this.metadataDoc = serviceStatus.MetadataDocument;
        }

        /// <summary>
        /// Get the template of an specified entity.
        /// </summary>
        /// <param name="entitySetName">The entity-type short name.</param>
        /// <param name="actualEntityTypeShortName">The actual entity-type short name.</param>
        /// <returns>Returns the entity template.</returns>
        private JObject GetEntityDataTemplate(string entitySetName, string actualEntityTypeShortName = null)
        {
            if (string.IsNullOrEmpty(entitySetName) && !entitySetName.IsSpecifiedEntitySetNameExist())
            {
                return null;
            }

            // Map 'entity-set name' to 'entity-type short name'.
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();
            string targetShortName = string.IsNullOrEmpty(actualEntityTypeShortName) ? entityTypeShortName : actualEntityTypeShortName;

            if (string.IsNullOrEmpty(entityTypeShortName))
            {
                throw new Exception("Failed to convert entity-set name to entity-type short name.");
            }

            var keyProperties = MetadataHelper.GetKeyProperties(this.metadataDoc, entityTypeShortName);
            JObject entity = null;

            // The multiple-key entities are very complex to construct, so the program only filter the single-key entities.
            if (null != keyProperties && 1 == keyProperties.Count())
            {
                var keyProperty = keyProperties.First();

                if (!KeyPropertyTypes.Contains(keyProperty.PropertyType))
                {
                    return entity;
                }

                // Convert 'entity-set name' to 'entity-set URL'.
                string entitySetURL = entitySetName.MapEntitySetNameToEntitySetURL();

                if (string.IsNullOrEmpty(entitySetURL))
                {
                    throw new Exception("Failed to convert to entity-set name to entity-set URL.");
                }

                string url = string.Format("{0}/{1}", this.rootURL, entitySetURL);
                var resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);

                if (null != resp && HttpStatusCode.OK == resp.StatusCode)
                {
                    JObject feed;
                    resp.ResponsePayload.TryToJObject(out feed);
                    if (null != feed)
                    {
                        var entries = JsonParserHelper.GetEntries(feed);

                        if (null != entries && entries.Any())
                        {
                            // If the current entity-type is a derived type, the program will get the entity with derived type from the feed.
                            // Otherwise, it will get the first entity from the feed.
                            entity = !string.IsNullOrEmpty(actualEntityTypeShortName) && actualEntityTypeShortName.IsSpecifiedEntityTypeShortNameExist() ?
                            this.GetDerivedEntity(entries, actualEntityTypeShortName) : entries.First as JObject;

                            // Set the new key value for the selected entity.
                            object keyValTemp = IntKeyPropertyTypes.Contains(keyProperty.PropertyType)
                                ? this.GetMaxEntityKey(entries, keyProperty.PropertyName)
                                : entity[keyProperty.PropertyName];
                            object keyVal = this.GenerateEntityID(keyProperty.PropertyType, keyValTemp);
                            entity[keyProperty.PropertyName] = new JValue(keyVal);
                            string pattern = "Edm.String" == keyProperty.PropertyType ? "{0}('{1}')" : "{0}({1})";
                            entity[Constants.V4OdataId] = new JValue(string.Format(pattern, url, keyVal.ToString()));

                            string serviceNamespace = this.GetNamespace(targetShortName, "EntityType");
                            entity[Constants.V4OdataType] = new JValue(string.Format("#{0}.{1}", serviceNamespace, targetShortName));
                        }
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Generate a new ID for an new created entity.
        /// </summary>
        /// <param name="keyPropertyType">The type of key property.</param>
        /// <param name="refID">The reference ID. 
        /// Note: Generally, this ID is the largest one in the entity-set.</param>
        /// <returns>Return a new ID.</returns>
        private object GenerateEntityID(string keyPropertyType, object refID = null)
        {
            if (!KeyPropertyTypes.Contains(keyPropertyType))
            {
                return null;
            }

            if ("Edm.Guid" != keyPropertyType && null == refID)
            {
                return null;
            }

            object result = null;

            switch (keyPropertyType)
            {
                case "Edm.Int16":
                    result = Convert.ToInt16(refID) + keyIncrement;
                    break;
                case "Edm.Int32":
                    result = Convert.ToInt32(refID) + keyIncrement;
                    break;
                case "Edm.Int64":
                    result = Convert.ToInt64(refID) + keyIncrement;
                    break;
                case "Edm.Guid":
                    result = Guid.NewGuid();
                    break;
                case "Edm.String":
                    result = string.Format(RefID, Guid.NewGuid().ToString());
                    break;
                default:
                    break;
            }

            keyIncrement++;

            return result;
        }

        /// <summary>
        /// Get the max value of the key property.
        /// </summary>
        /// <param name="entries">The feed. (The collection/set of the entities.)</param>
        /// <param name="keyPropertyName">The name of the key property.</param>
        /// <returns>Returns the max value of the key property.</returns>
        private object GetMaxEntityKey(JArray entries, string keyPropertyName)
        {
            object value = null;

            if (null != entries && entries.Any())
            {
                value = entries.First[keyPropertyName];

                foreach (var entry in entries)
                {
                    if (Convert.ToInt64(value) < (Int64)entry[keyPropertyName])
                    {
                        value = entry[keyPropertyName];
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Get the derived entity from the feed.
        /// </summary>
        /// <param name="entries">The feed. (The collection/set of the entities.)</param>
        /// <param name="actualEntityTypeShortName">The actual entity-type short name.</param>
        /// <returns>Returns the entity with the derived type.</returns>
        private JObject GetDerivedEntity(JArray entries, string actualEntityTypeShortName)
        {
            JObject entity = null;

            if (null != entries)
            {
                foreach (var entry in entries)
                {
                    string odataType = entry[Constants.V4OdataType].ToString();
                    if (odataType.EndsWith(actualEntityTypeShortName))
                    {
                        entity = entry as JObject;
                        break;
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Get namespace of the target element from metadata.
        /// </summary>
        /// <param name="targetShortName">The target element's short name.</param>
        /// <param name="targetType">The target type.</param>
        /// <returns>Returns the namespace of the target element.</returns>
        private string GetNamespace(string targetShortName, string targetType)
        {
            string xPath = string.Format("//*[local-name()='{0}' and @Name='{1}']", targetType, targetShortName);
            var metadata = XElement.Parse(this.metadataDoc);
            var elem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            var aliasAndNs = MetadataHelper.GetAliasAndNamespace(elem);

            return aliasAndNs.Namespace;
        }

        private static readonly List<string> KeyPropertyTypes;
        private static readonly List<string> IntKeyPropertyTypes;
        private static readonly string RefID;
        private static int keyIncrement;
        private static DataFactory factory;
        private string rootURL;
        private string serviceDoc;
        private string metadataDoc;
    }
}
