// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
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
    /// Class of service implemenation feature to request the delta tracking link and the first page of change content.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_RequestingChanges_delta : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_RequestingChanges_delta";
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",$delta";
            }
        }

        /// <summary>
        /// Gets the service implementation feature specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.3";
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
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                return new ServiceImplCategory(ServiceImplCategoryName.RequestingChanges, null);
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
            var entityTypeElements = MetadataHelper.GetEntityTypes(serviceStatus.MetadataDocument, 1, keyPropertyTypes, null, NavigationRoughType.CollectionValued);

            if (null == entityTypeElements || 0 == entityTypeElements.Count())
            {
                detail.ErrorMessage = "To verify this rule it expects an entity type with Int32/Int64/Int16/Guid/String key property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string entitySet = string.Empty;
            EntityTypeElement entityType = new EntityTypeElement();
            foreach (var en in entityTypeElements)
            {
                entitySet = en.EntitySetName;
                var funcs = new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>() 
                {
                    AnnotationsHelper.GetInsertRestrictions, AnnotationsHelper.GetDeleteRestrictions
                };

                var restrictions = entitySet.GetRestrictions(serviceStatus.MetadataDocument, termDocs.VocCapabilitiesDoc, funcs, null, NavigationRoughType.None);
                if (!string.IsNullOrEmpty(restrictions.Item1)
                    && null != restrictions.Item2 && restrictions.Item2.Any()
                    && null != restrictions.Item3 && restrictions.Item3.Any())
                {
                    entityType = en;
                    break;
                }
            }

            if (string.IsNullOrEmpty(entitySet))
            {
                detail.ErrorMessage = "Cannot find an entity-set URL which can be execute the deep insert operation on it.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string entitySetUrl = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySet;
            string rootUrl = serviceStatus.RootURL.TrimEnd('/') + @"/";
            string url = entitySetUrl;

            Response resp = null;

            bool gotDeltaLink = false;
            string deltaLink = string.Empty;

            while (!gotDeltaLink)
            {
                resp = WebHelper.GetDeltaLink(url, context.RequestHeaders);
                JObject payload;
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    bool hasNextOrDelta = false;
                    resp.ResponsePayload.TryToJObject(out payload);
                    foreach (JProperty child in payload.Children<JProperty>())
                    {
                        if (child.Name.Equals(Constants.V4OdataDeltaLink))
                        {
                            gotDeltaLink = true;
                            hasNextOrDelta = true;
                            deltaLink = child.Value.ToString();

                            if (Uri.IsWellFormedUriString(deltaLink, UriKind.Relative))
                            {
                                deltaLink = rootUrl + deltaLink;
                            }

                            break;
                        }

                        if (child.Name.Equals(Constants.V4OdataNextLink))
                        {
                            url = child.Value.ToString();
                            hasNextOrDelta = true;

                            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
                            {
                                url = rootUrl + deltaLink;
                            }

                            break;
                        }
                    }

                    if (!hasNextOrDelta)
                        break;
                }
            }

            if (!gotDeltaLink)
            {
                detail.ErrorMessage = "The service does not support delta tracking.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);
                passed = false;

                return passed;
            }

            bool isCreated = false;
            var additionalInfos = new List<AdditionalInfo>();
            var reqData = dFactory.ConstructInsertedEntityData(entityType.EntitySetName, entityType.EntityTypeShortName, null, out additionalInfos);
            string reqDataStr = reqData.ToString();
            bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
            resp = WebHelper.CreateEntity(entitySetUrl, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
            detail = new ExtensionRuleResultDetail(this.Name, entitySetUrl, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);
            if (HttpStatusCode.Created == resp.StatusCode)
            {
                var entityId = additionalInfos.Last().EntityId;
                resp = WebHelper.GetEntity(entityId);
                detail = new ExtensionRuleResultDetail(this.Name, entityId, HttpMethod.Get, string.Empty, resp, string.Empty, reqDataStr);

                if (HttpStatusCode.OK == resp.StatusCode)
                {
                    isCreated = true;
                }
            }

            resp = WebHelper.GetEntity(deltaLink);

            if ((isCreated && resp.StatusCode == HttpStatusCode.OK) || resp.StatusCode == HttpStatusCode.NoContent)
            {
                passed = true;
                detail = new ExtensionRuleResultDetail(this.Name, deltaLink, HttpMethod.Get, string.Empty, resp, string.Empty, null);
            }
            else
            {
                passed = false;
                detail = new ExtensionRuleResultDetail(this.Name, deltaLink, HttpMethod.Get, string.Empty, resp, string.Empty, null);
                detail.ErrorMessage = "Cannot get delta changes from the delta link.";
            }

            // Restore the service.
            var resps = WebHelper.DeleteEntities(context.RequestHeaders, additionalInfos);


            var details = new List<ExtensionRuleResultDetail>() { detail };
            info = new ExtensionRuleViolationInfo(new Uri(url), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
