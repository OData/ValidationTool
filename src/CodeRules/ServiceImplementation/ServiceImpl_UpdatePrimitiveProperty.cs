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
    /// Class of service implemenation feature to update a primitive property.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_UpdatePrimitiveProperty : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_UpdatePrimitiveProperty";
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
                return this.CategoryInfo.CategoryFullName + ",Update a Primitive Property";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.4.9.1";
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
            NormalProperty primitiveProp = null;
            foreach (var en in entityTypeElements)
            {
                foreach(NormalProperty np in en.NormalProperties)
                {
                    if (!np.IsKey && np.PropertyType.Equals("Edm.String"))
                    {
                        entityType = en;
                        primitiveProp = np;
                        break;
                    }
                }

                if(primitiveProp != null)
                {
                    break;
                }
            }

            if (entityType == null)
            {
                detail.ErrorMessage = "To verify this rule it expects an Edm.String type property in an entity, but there is no such entity in metadata. So can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string entitySetUrl = entityType.EntitySetName.MapEntitySetNameToEntitySetURL();
            updateUrl = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
            if (string.IsNullOrEmpty(entitySetUrl))
            {
                detail.ErrorMessage = string.Format("Cannot find the entity-set URL which is matched with {0}", entityType.EntityTypeShortName);
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
            var additionalInfos = new List<AdditionalInfo>();
            var reqData = dFactory.ConstructInsertedEntityData(entityType.EntitySetName, entityType.EntityTypeShortName, null, out additionalInfos);

            if (!dFactory.CheckOrAddTheMissingPropertyData(entityType.EntitySetName, primitiveProp.PropertyName, ref reqData))
            {
                detail.ErrorMessage = "The property to update does not exist, and cannot be updated.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string reqDataStr = reqData.ToString();
            if (reqDataStr.Length > 2)
            {
                bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
                var resp = WebHelper.CreateEntity(url, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
                detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);
                if (HttpStatusCode.Created == resp.StatusCode)
                {
                    string entityId = additionalInfos.Last().EntityId;
                    updateUrl = entityId.TrimEnd('/') + "/" + primitiveProp.PropertyName + @"/$value";
                    bool hasEtag = additionalInfos.Last().HasEtag;
                    resp = WebHelper.GetPropertyValue(updateUrl);
                    detail = new ExtensionRuleResultDetail(this.Name, updateUrl, HttpMethod.Get, string.Empty, resp);
                    if (HttpStatusCode.OK == resp.StatusCode || HttpStatusCode.NoContent == resp.StatusCode)
                    {
                        resp = WebHelper.UpdateAStringProperty(updateUrl, context.RequestHeaders, hasEtag);
                        detail = new ExtensionRuleResultDetail(this.Name, updateUrl, HttpMethod.Put, string.Empty, resp, string.Empty, reqDataStr);
                        if (HttpStatusCode.NoContent == resp.StatusCode)
                        {
                            resp = WebHelper.GetPropertyValue(updateUrl);
                            detail = new ExtensionRuleResultDetail(this.Name, updateUrl, HttpMethod.Get, string.Empty, resp, string.Empty, reqDataStr);

                            if (HttpStatusCode.OK == resp.StatusCode)
                            {
                                if (resp.ResponsePayload == Constants.UpdateData)
                                {
                                    passed = true;
                                }
                                else
                                {
                                    passed = false;
                                    detail.ErrorMessage = string.Format("The primitive property in request is not updated.");
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
                            detail.ErrorMessage = "Put the primitive property failed.";
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
            }
            else
            {
                detail.ErrorMessage = "Constructing the testing entity failed.";
            }

            var details = new List<ExtensionRuleResultDetail>() { detail };
            info = new ExtensionRuleViolationInfo(new Uri(updateUrl), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
