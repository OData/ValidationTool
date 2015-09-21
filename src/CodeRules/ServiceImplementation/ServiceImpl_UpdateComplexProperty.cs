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
    /// Class of service implemenation feature to update a complex type property.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_UpdateComplexProperty : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_UpdateComplexProperty";
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
                return new ServiceImplCategory(ServiceImplCategoryName.ManagingValues_Properties, parent);
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",Update a Complex Property";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.4.9.3";
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
            List<EntityTypeElement> entityTypeElements = MetadataHelper.GetEntityTypes(serviceStatus.MetadataDocument, 1, keyPropertyTypes, null, NavigationRoughType.None).ToList();
            if (null == entityTypeElements || 0 == entityTypeElements.Count)
            {
                detail.ErrorMessage = "To verify this rule it expects an entity type with Int32/Int64/Int16/Guid/String key property and a string type normal property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            EntityTypeElement entityType = null;

            Dictionary<string, Dictionary<KeyValuePair<string, string>, List<string>>> entityTypeInfo = new Dictionary<string, Dictionary<KeyValuePair<string, string>, List<string>>>();

            bool foundEntity = MetadataHelper.GetEntityTypesWithComplexProperty(serviceStatus.MetadataDocument, "Edm.String", out entityTypeInfo);

            if (foundEntity)
            {
                foreach (var en in entityTypeElements)
                {
                    if (entityTypeInfo.Keys.Contains(en.EntityTypeShortName) && !string.IsNullOrEmpty(en.EntitySetName))
                    {
                        entityType = en;
                        break;
                    }
                }
            }
            else
            {
                detail.ErrorMessage = "To verify this rule it expects an entity type with complex type normal property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            if(entityType == null)
            {
                detail.ErrorMessage = "To verify this rule it expects an entity type with complex type normal property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            Dictionary<KeyValuePair<string, string>, List<string>> complexProps = entityTypeInfo[entityType.EntityTypeShortName];
            KeyValuePair<KeyValuePair<string, string>, List<string>> complexPop = complexProps.First();
            string complexPropName = complexPop.Key.Key;

            string entitySetUrl = entityType.EntitySetName.MapEntitySetNameToEntitySetURL();
            updateUrl = entitySetUrl;

            string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
            updateUrl = url;
            var additionalInfos = new List<AdditionalInfo>();
            JObject reqData = dFactory.ConstructInsertedEntityData(entityType.EntitySetName, entityType.EntityTypeShortName, null, out additionalInfos);

            if (!dFactory.CheckOrAddTheMissingPropertyData(entityType.EntitySetName, complexPropName, ref reqData))
            {
                detail.ErrorMessage = "The property to update does not exist, and cannot be updated.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }
            
            string reqDataStr = reqData.ToString();
            bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
            var resp = WebHelper.CreateEntity(url, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
            
            detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);

            if (HttpStatusCode.Created == resp.StatusCode)
            {
                string entityId = additionalInfos.Last().EntityId;
                updateUrl = entityId.TrimEnd('/') + "/" + complexPropName;
                bool hasEtag = additionalInfos.Last().HasEtag;
                resp = WebHelper.GetPropertyValue(updateUrl);
                detail = new ExtensionRuleResultDetail(this.Name, entityId, HttpMethod.Get, string.Empty, resp);
                if (HttpStatusCode.OK == resp.StatusCode || HttpStatusCode.NoContent == resp.StatusCode)
                {
                    JProperty complexPropContent = null;

                    foreach (JProperty prop in reqData.Children<JProperty>())
                    {
                        if(prop.Name.Equals(complexPropName))
                        {
                            complexPropContent = prop;
                            break;
                        }
                    }

                    resp = WebHelper.UpdateComplexProperty(updateUrl, context.RequestHeaders, complexPropContent, hasEtag, complexPop.Value);
                    detail = new ExtensionRuleResultDetail(this.Name, updateUrl, HttpMethod.Patch, string.Empty, resp, string.Empty, reqDataStr);
                    if (HttpStatusCode.NoContent == resp.StatusCode)
                    {
                        resp = WebHelper.GetPropertyValue(updateUrl);
                        detail = new ExtensionRuleResultDetail(this.Name, updateUrl, HttpMethod.Get, string.Empty, resp, string.Empty, reqDataStr);

                        if (HttpStatusCode.OK == resp.StatusCode)
                        {
                            JObject value;
                            resp.ResponsePayload.TryToJObject(out value);

                            bool updated = true;

                            if (value != null && value.Type == JTokenType.Object)
                            {
                                foreach (var prop in value.Children<JProperty>())
                                {
                                    if (complexPop.Value.Contains(prop.Name))
                                    {
                                        updated = prop.Value.ToString().Equals(Constants.UpdateData);
                                    }

                                    if (!updated)
                                    {
                                        passed = false;
                                        detail.ErrorMessage = string.Format("The compelex property in request fails in HTTP patch updating.");
                                        break;
                                    }
                                }

                                if(updated)
                                {
                                    passed = true;
                                }
                            }
                        }
                        else
                        {
                            passed = false;
                            detail.ErrorMessage = "Can not get the updated entity.";
                        }
                    }
                    else
                    {
                        passed = false;
                        detail.ErrorMessage = "HTTP Patch the complex property failed.";
                    }
                }
                else
                {
                    detail.ErrorMessage = "Can not get the created entity from above URI.";
                }

                // Restore the service.
                var resps = WebHelper.DeleteEntities(context.RequestHeaders, additionalInfos);
            }
            else
            {
                detail.ErrorMessage = "Created the new entity failed for above URI.";
            }

            var details = new List<ExtensionRuleResultDetail>() { detail };
            info = new ExtensionRuleViolationInfo(new Uri(updateUrl), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
