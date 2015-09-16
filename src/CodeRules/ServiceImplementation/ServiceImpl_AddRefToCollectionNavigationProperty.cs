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
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of service implemenation feature to add a reference to a collection-valued navigation property.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_AddRefToCollectionNavigationProperty : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_AddRefToCollectionNavigationProperty";
            }
        }

        /// <summary>
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                var parent = new ServiceImplCategory(ServiceImplCategoryName.DataModification);
                return new ServiceImplCategory(ServiceImplCategoryName.ModifyingRelationships, parent);
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",Add a Reference to a Collection-Valued Navigation Property";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.4.6.1";
            }
        }

        /// <summary>
        /// Gets the service implementation feature level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }

        /// <summary>
        /// Verifies the extension rule.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
             if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;
            ServiceStatus serviceStatus = ServiceStatus.GetInstance();
            DataFactory dFactory = DataFactory.Instance();
            var detail = new ExtensionRuleResultDetail(this.Name, serviceStatus.RootURL, HttpMethod.Post, string.Empty);
            string updateUrl = serviceStatus.RootURL;
            List<string> keyPropertyTypes = new List<string>() { "Edm.Int32", "Edm.Int16", "Edm.Int64", "Edm.Guid", "Edm.String" };
            List<string> norPropertyTypes = new List<string>() { "Edm.String" };
            List<EntityTypeElement> entityTypeElements = MetadataHelper.GetEntityTypes(serviceStatus.MetadataDocument, 1, keyPropertyTypes, null, NavigationRoughType.CollectionValued).ToList();
            if (null == entityTypeElements || 0 == entityTypeElements.Count)
            {
                detail.ErrorMessage = "To verify this rule it expects an entity type with Int32/Int64/Int16/Guid/String key property and a string type normal property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            EntityTypeElement entityType = null;

            List<string> navigationPropNames = new List<string>();

            foreach (var en in entityTypeElements)
            {
                if (MetadataHelper.HasEntityNavigationProp(serviceStatus.MetadataDocument, en.EntityTypeShortName, NavigationRoughType.CollectionValued, context, out navigationPropNames))
                {
                    entityType = en;
                    break;
                }
            }

            if (navigationPropNames!=null && !navigationPropNames.Any())
            {
                detail.ErrorMessage = "To verify this rule it is expected that an entity type has collection-valued navigation type property, but there is no such entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string navigationPropName = navigationPropNames.First();

            string entitySetUrl = entityType.EntitySetName.MapEntitySetNameToEntitySetURL();
            updateUrl = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
            if (string.IsNullOrEmpty(entitySetUrl))
            {
                detail.ErrorMessage = string.Format("Cannot find the entity-set URL which is matched with {0}", entityType.EntityTypeShortName);
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
            var additionalInfosAll = new List<AdditionalInfo>();
            var additionalInfos = new List<AdditionalInfo>();
            var reqData = dFactory.ConstructInsertedEntityData(entityType.EntitySetName, entityType.EntityTypeShortName, null, out additionalInfos);
            additionalInfosAll = additionalInfos;
            string reqDataStr = reqData.ToString();
            bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
            var resp = WebHelper.CreateEntity(url, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
            detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);
            if (HttpStatusCode.Created == resp.StatusCode)
            {
                string entityId = additionalInfos.Last().EntityId;
                string nPropUrl = entityId.TrimEnd('/') + "/" + navigationPropName;
                updateUrl = entityId.TrimEnd('/') + "/" + navigationPropName + "/$ref";
                bool hasEtag = additionalInfos.Last().HasEtag;
                resp = WebHelper.GetEntity(updateUrl);
                detail = new ExtensionRuleResultDetail(this.Name, updateUrl, HttpMethod.Get, string.Empty, resp);
                if (HttpStatusCode.OK == resp.StatusCode)
                {
                    string navigPropType = MetadataHelper.GetNavigPropertyTypeFromMetadata(navigationPropName, entityType.EntityTypeShortName, serviceStatus.MetadataDocument);
                    string navigPropTypeShortName = navigPropType.Substring(navigPropType.IndexOf("(")).GetLastSegment().TrimEnd(')');
                    string nEntitySetName = navigationPropName.MapNavigationPropertyNameToEntitySetName(entityType.EntityTypeShortName);
                    string nEntitySetUrl = nEntitySetName.MapEntitySetNameToEntitySetURL();
                    nEntitySetUrl = serviceStatus.RootURL.TrimEnd('/') + @"/" + nEntitySetUrl;

                    reqData = dFactory.ConstructInsertedEntityData(nEntitySetName, navigPropTypeShortName, null, out additionalInfos);

                    foreach(AdditionalInfo a in additionalInfos)
                    {
                        additionalInfosAll.Add(a);
                    }

                    reqDataStr = reqData.ToString();
                    isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
                    resp = WebHelper.CreateEntity(nEntitySetUrl, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
                    detail = new ExtensionRuleResultDetail(this.Name, nEntitySetUrl, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);

                    if (HttpStatusCode.Created == resp.StatusCode)
                    {
                        string refEntityId = additionalInfos.Last().EntityId;

                        resp = WebHelper.GetEntity(refEntityId);
                        detail = new ExtensionRuleResultDetail(this.Name, refEntityId, HttpMethod.Get, string.Empty, resp, string.Empty, reqDataStr);

                        if (HttpStatusCode.OK == resp.StatusCode)
                        {
                            string refEntitySetNameWithEnityID = refEntityId.Split('/').Last();
                            string refEntityValue = serviceStatus.RootURL.TrimEnd('/') + @"/" + refEntitySetNameWithEnityID;
                            string refEntityID = refEntitySetNameWithEnityID.Contains("'") ? refEntitySetNameWithEnityID.Split('\'')[1] : refEntitySetNameWithEnityID.Split('(')[1].TrimEnd(')');

                            JObject navigationRefSet = new JObject();
                            navigationRefSet.Add(Constants.V4OdataId, refEntityValue);

                            resp = WebHelper.CreateEntity(updateUrl, context.RequestHeaders, navigationRefSet, isMediaType, ref additionalInfosAll);
                            detail = new ExtensionRuleResultDetail(this.Name, updateUrl, HttpMethod.Patch, string.Empty, resp, string.Empty, reqDataStr);
                            if (HttpStatusCode.NoContent == resp.StatusCode)
                            {
                                resp = WebHelper.GetEntity(updateUrl);
                                detail = new ExtensionRuleResultDetail(this.Name, updateUrl, HttpMethod.Get, string.Empty, resp, string.Empty, reqDataStr);

                                if (HttpStatusCode.OK == resp.StatusCode)
                                {
                                    JObject refEntitySet;
                                    resp.ResponsePayload.TryToJObject(out refEntitySet);

                                    bool created = false;

                                    var entries = JsonParserHelper.GetEntries(refEntitySet);

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

                                        if (!created)
                                        {
                                            passed = false;
                                            detail.ErrorMessage = string.Format("The HTTP Post to add a reference to a collection-valued navigation property failed.");
                                        }
                                    }
                                }
                                else
                                {
                                    passed = false;
                                    detail.ErrorMessage = "HTTP Get to the navigation property reference failed.";
                                }
                            }
                            else
                            {
                                passed = false;
                                detail.ErrorMessage = "HTTP Post to add the navigation property reference failed.";
                            }
                        }
                        else
                        {
                            detail.ErrorMessage = "Can not get the created navigation entity set entity.";
                        }
                    }
                    else
                    {
                        detail.ErrorMessage = "Creating navigation entity failes from above URI.";
                    }
                }
                else
                {
                    detail.ErrorMessage = "Can not get the created entity's navigation property reference collection set from above URI.";
                }

                // Restore the service.
                var resps = WebHelper.DeleteEntities(context.RequestHeaders, additionalInfosAll);
            }
            else
            {
                detail.ErrorMessage = "Creating the new entity failed for above URI.";
            }

            var details = new List<ExtensionRuleResultDetail>() { detail };
            info = new ExtensionRuleViolationInfo(new Uri(updateUrl), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
