// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using System.Xml;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of service implemenation feature to create an entity with deep inserting.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_CreateEntityDeepInsert : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_CreateEntityDeepInsert";
            }
        }

        /// <summary>
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                return new ServiceImplCategory(ServiceImplCategoryName.DataModification);
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",Create an Entity (Deep Insert)";
            }
        }

        /// <summary>
        /// Gets the service implementation feature specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.4.2.2";
            }
        }

        /// <summary>
        /// Gets the service implementation feature level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Should;
            }
        }

        /// <summary>
        /// Verifies the service implementation feature.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if the service implementation feature passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;
            ServiceStatus serviceStatus = ServiceStatus.GetInstance();
            TermDocuments termDocs = TermDocuments.GetInstance();
            DataFactory dFactory = DataFactory.Instance();
            var detail = new ExtensionRuleResultDetail(this.Name, serviceStatus.RootURL, HttpMethod.Post, string.Empty);
            List<string> keyPropertyTypes = new List<string>() { "Edm.Int32", "Edm.Int16", "Edm.Int64", "Edm.Guid", "Edm.String" };
            var entityTypeElements = MetadataHelper.GetEntityTypes(serviceStatus.MetadataDocument, 1, keyPropertyTypes, null, NavigationRoughType.None);

            if (null == entityTypeElements || 0 == entityTypeElements.Count())
            {
                detail.ErrorMessage = "To verify this rule it expects an entity type with Int32/Int64/Int16/Guid/String key property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string entitySet = string.Empty;
            string navigProp = string.Empty;
            NavigationRoughType navigRoughType = NavigationRoughType.None;
            EntityTypeElement entityType = new EntityTypeElement();
            foreach (var en in entityTypeElements)
            {
                if(!string.IsNullOrEmpty(en.EntitySetName) && en.NavigationProperties.Count > 0)
                {
                    foreach (NavigProperty np in en.NavigationProperties)
                    {
                        string npTypeFullName = np.NavigationPropertyType.RemoveCollectionFlag();
                        
                        XmlDocument metadata = new XmlDocument();
                        metadata.LoadXml(context.MetadataDocument);
                        if (context.ContainsExternalSchema)
                        { 
                            metadata.LoadXml(context.MergedMetadataDocument); 
                        }
                        XmlNode npXmlNode;
                        MetadataHelper.GetTypeNode(npTypeFullName, metadata, out npXmlNode);
                        if(!(npXmlNode.Attributes["HasStream"]!=null ? npXmlNode.Attributes["HasStream"].Value.Equals("true") : false))
                        {
                            navigProp = np.NavigationPropertyName;
                            navigRoughType = np.NavigationRoughType;
                            entityType = en;
                            entitySet = en.EntitySetName;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(entitySet))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(entitySet) || string.IsNullOrEmpty(navigProp))
            {
                detail.ErrorMessage = "Cannot find an entity-set URL which can be execute the deep insert operation on it.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySet;
            var additionalInfos = new List<AdditionalInfo>();
            var reqData = dFactory.ConstructInsertedEntityData(entityType.EntitySetName, entityType.EntityTypeShortName, new List<string>() { navigProp }, out additionalInfos);
            string reqDataStr = reqData.ToString();
            bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
            var resp = WebHelper.CreateEntity(url, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
            detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);
            if (HttpStatusCode.Created == resp.StatusCode)
            {
                var mainEntityID = additionalInfos.Last().EntityId;
                string entityId = additionalInfos.First().EntityId;
                
                string refUrl = mainEntityID.TrimEnd('/') + "/" + navigProp + "/$ref";
                string refEntitySetNameWithEnityID = entityId.Split('/').Last();
                string refEntityValue = serviceStatus.RootURL.TrimEnd('/') + @"/" + refEntitySetNameWithEnityID;
                string refEntityID = refEntitySetNameWithEnityID.Contains("'") ? refEntitySetNameWithEnityID.Split('\'')[1] : refEntitySetNameWithEnityID.Split('(')[1].TrimEnd(')');

                resp = WebHelper.GetEntity(mainEntityID);
                detail = new ExtensionRuleResultDetail(this.Name, mainEntityID, HttpMethod.Get, string.Empty, resp, string.Empty, reqDataStr);

                if (HttpStatusCode.OK == resp.StatusCode && additionalInfos.Count > 1)
                {
                    resp = WebHelper.GetEntity(refUrl);
                    detail = new ExtensionRuleResultDetail(this.Name, refUrl, HttpMethod.Get, string.Empty, resp, string.Empty, reqDataStr);

                    if (HttpStatusCode.OK == resp.StatusCode)
                    {
                        JObject refPayload;
                        resp.ResponsePayload.TryToJObject(out refPayload);
                        bool created = false;

                        if (navigRoughType == NavigationRoughType.CollectionValued)
                        {
                            var entries = JsonParserHelper.GetEntries(refPayload);

                            if (entries != null)
                            {
                                foreach (JObject entry in entries)
                                {
                                    foreach (JProperty prop in entry.Children())
                                    {
                                        if (prop.Name.Equals(Constants.V4OdataId))
                                        {
                                            created = prop.Value.ToString().Contains(refEntityID);
                                            if (created)
                                            {
                                                passed = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (JProperty prop in refPayload.Children())
                            {
                                if (prop.Name.Equals(Constants.V4OdataId))
                                {
                                    created = prop.Value.ToString().Contains(refEntityID);
                                    if (created)
                                    {
                                        passed = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!created)
                        {
                            passed = false;
                            detail.ErrorMessage = string.Format("The HTTP deep insert failed to add a reference to a collection-valued navigation property failed.");
                        }
                    }
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = "Can not get the created entity. ";
                }

                // Restore the service.
                var resps = WebHelper.DeleteEntities(context.RequestHeaders, additionalInfos);
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "Created the new entity failed for above URI. ";
            }

            var details = new List<ExtensionRuleResultDetail>() { detail };
            info = new ExtensionRuleViolationInfo(new Uri(url), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
