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
    /// Class of extension rule for Minimal.Conformance.1022
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1022 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1022";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "22. MUST support if-match header in update/delete of any resources returned with an ETag (section 11.4.1.1)";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.1";
            }
        }

        /// <summary>
        /// Gets the resource type to which the rule applies.
        /// </summary>
        public override ConformanceServiceType? ResourceType
        {
            get
            {
                return ConformanceServiceType.ReadWrite;
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
            TermDocuments termDocs = TermDocuments.GetInstance();
            DataFactory dFactory = DataFactory.Instance();
            var detail1 = new ExtensionRuleResultDetail(this.Name);
            var detail2 = new ExtensionRuleResultDetail(this.Name);
            var detail3 = new ExtensionRuleResultDetail(this.Name);
            var detail4 = new ExtensionRuleResultDetail(this.Name);
            var detail5 = new ExtensionRuleResultDetail(this.Name);
            var detail6 = new ExtensionRuleResultDetail(this.Name);
            var entityType = MetadataHelper.GetConcurrencyEntityType(serviceStatus.MetadataDocument);
            if (null == entityType)
            {
                detail1.ErrorMessage = "Cannot find an entity which contains a concurrency property in the service.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail1);

                return passed;
            }

            var normalPropNames = entityType.NormalProperties.Where(np => "Edm.String" == np.PropertyType && !np.IsKey).Select(np => np.PropertyName);
            if (normalPropNames.Any())
            {
                string entitySetUrl = entityType.EntitySetName.MapEntitySetNameToEntitySetURL();
                string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
                var additionalInfos = new List<AdditionalInfo>();
                var reqData = dFactory.ConstructInsertedEntityData(entityType.EntitySetName, entityType.EntityTypeShortName, null, out additionalInfos);
                string reqDataStr = reqData.ToString();
                bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
                var resp = WebHelper.CreateEntity(url, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
                detail1 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);

                if (HttpStatusCode.Created == resp.StatusCode)
                {
                    var entityId = additionalInfos.Last().EntityId;
                    var hasEtag = additionalInfos.Last().HasEtag;
                    if (!hasEtag)
                    {
                        detail1.ErrorMessage = "The new inserted entity does not contain an @odata.etag annotation.";
                        info = new ExtensionRuleViolationInfo(new Uri(entityId), resp.ResponsePayload, detail2);

                        return passed;
                    }

                    resp = WebHelper.GetEntity(entityId, serviceStatus.DefaultHeaders);
                    detail2 = new ExtensionRuleResultDetail(this.Name, entityId, HttpMethod.Get, StringHelper.MergeHeaders(string.Empty, serviceStatus.DefaultHeaders), resp);
                    if (HttpStatusCode.OK == resp.StatusCode)
                    {
                        reqDataStr = dFactory.ConstructUpdatedEntityData(reqData, normalPropNames).ToString();
                        var header = new KeyValuePair<string, string>("If-Match", additionalInfos.Last().ODataEtag);
                        var headers = new List<KeyValuePair<string, string>>() { header };
                        resp = WebHelper.UpdateEntity(entityId, reqDataStr, HttpMethod.Patch, headers);
                        detail3 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Patch, StringHelper.MergeHeaders(string.Empty, headers), resp, string.Empty, reqDataStr);
                        if (HttpStatusCode.NoContent == resp.StatusCode)
                        {
                            resp = WebHelper.GetEntity(entityId, serviceStatus.DefaultHeaders);
                            detail4 = new ExtensionRuleResultDetail(this.Name, entityId, HttpMethod.Get, StringHelper.MergeHeaders(string.Empty, serviceStatus.DefaultHeaders), resp);
                            if (HttpStatusCode.OK == resp.StatusCode)
                            {
                                resp = WebHelper.DeleteEntity(entityId, context.RequestHeaders, hasEtag);
                                detail5 = new ExtensionRuleResultDetail(this.Name, entityId, HttpMethod.Delete, StringHelper.MergeHeaders(string.Empty, new List<KeyValuePair<string, string>>() { header }), resp);
                                if (HttpStatusCode.NoContent == resp.StatusCode)
                                {
                                    resp = WebHelper.GetEntity(entityId);
                                    detail6 = new ExtensionRuleResultDetail(this.Name, entityId, HttpMethod.Get, string.Empty, resp);
                                    if (HttpStatusCode.NotFound == resp.StatusCode)
                                    {
                                        passed = true;
                                    }
                                    else
                                    {
                                        passed = false;
                                        detail6.ErrorMessage = "It still can get the deleted entity from above URI.";
                                    }
                                }
                                else
                                {
                                    passed = false;
                                    detail5.ErrorMessage = "Delete entity failed.";
                                }
                            }
                            else
                            {
                                passed = false;
                                detail4.ErrorMessage = "Can not get the created entity from above URI.";
                            }
                        }
                        else
                        {
                            passed = false;
                            detail3.ErrorMessage = "Update entity failed.";
                        }
                    }
                }
                else
                {
                    passed = false;
                    detail1.ErrorMessage = "Created the new entity failed for above URI.";
                }
            }

            var details = new List<ExtensionRuleResultDetail>() 
            {
                detail1, detail2, detail3, detail4, detail5, detail6
            }.RemoveNullableDetails();
            info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
