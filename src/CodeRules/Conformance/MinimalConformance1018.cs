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
    /// Class of extension rule for Minimal.Conformance.1018
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1018 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1018";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "18. MUST support PUT to $ref to set an existing single updatable related entity (section 11.4.6.3)";
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
            List<string> keyPropertyTypes = new List<string>() { "Edm.Int32", "Edm.Int16", "Edm.Int64", "Edm.Guid", "Edm.String" };
            List<EntityTypeElement> entityTypeElements = MetadataHelper.GetEntityTypes(serviceStatus.MetadataDocument, 1, keyPropertyTypes, null, NavigationRoughType.SingleValued).ToList();

            if (null == entityTypeElements || 0 == entityTypeElements.Count())
            {
                detail1.ErrorMessage = "To verify this rule it expects an entity type with Int32/Int64/Int16/Guid/String key property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail1);

                return passed;
            }

            string entityTypeShortName = string.Empty;
            string entitySetName = string.Empty;
            string entitySetUrl = string.Empty;
            string navigPropName = string.Empty;

            foreach (var en in entityTypeElements)
            {
                var matchEntity = en.EntitySetName.GetRestrictions(serviceStatus.MetadataDocument, termDocs.VocCapabilitiesDoc,
                    new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>()
                    {
                        AnnotationsHelper.GetExpandRestrictions, AnnotationsHelper.GetInsertRestrictions
                        , AnnotationsHelper.GetDeleteRestrictions, AnnotationsHelper.GetUpdateRestrictions
                    });

                if (!string.IsNullOrEmpty(matchEntity.Item1)
                     && matchEntity.Item2 != null && matchEntity.Item2.Any()
                     && matchEntity.Item3 != null && matchEntity.Item3.Any())
                {
                    entityTypeShortName = en.EntityTypeShortName;
                    entitySetName = matchEntity.Item1;
                    entitySetUrl = entitySetName.MapEntitySetNameToEntitySetURL();
                    foreach (NavigProperty np in matchEntity.Item3)
                    {
                        string nEntityTypeShortName = np.NavigationPropertyType.RemoveCollectionFlag().GetLastSegment();
                        if (np.NavigationRoughType == NavigationRoughType.SingleValued && !nEntityTypeShortName.IsMediaType())
                        {
                            navigPropName = np.NavigationPropertyName;
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(entitySetName) && !string.IsNullOrEmpty(navigPropName))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(entitySetName) || string.IsNullOrEmpty(navigPropName))
            {
                detail1.ErrorMessage = "Cannot find the appropriate entity-set URL to verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail1);

                return passed;
            }

            string navigPropEntitySet = navigPropName.MapNavigationPropertyNameToEntitySetURL(entityTypeShortName);
            string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
            var additionalInfos = new List<AdditionalInfo>();
            var reqData = dFactory.ConstructInsertedEntityData(entitySetName, entityTypeShortName, new List<string>() { navigPropName }, out additionalInfos);
            string reqDataStr = reqData.ToString();
            bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
            var resp = WebHelper.CreateEntity(url, reqData, isMediaType, ref additionalInfos);
            detail1 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);
            if (HttpStatusCode.Created == resp.StatusCode)
            {
                string entityId = additionalInfos.Last().EntityId;
                url = serviceStatus.RootURL.TrimEnd('/') + @"/" + navigPropEntitySet;
                resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, serviceStatus.DefaultHeaders);
                detail2 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, serviceStatus.DefaultHeaders), resp);
                if (resp.StatusCode.HasValue && HttpStatusCode.OK == resp.StatusCode)
                {
                    JObject feed;
                    resp.ResponsePayload.TryToJObject(out feed);
                    var entities = JsonParserHelper.GetEntries(feed);
                    if (entities.Any())
                    {
                        string v4OdataIdVal = string.Empty;
                        if (entities.First[Constants.V4OdataId] != null)
                        {
                            v4OdataIdVal = entities.First[Constants.V4OdataId].ToString();
                        }

                        url = string.Format("{0}/{1}/$ref", entityId, navigPropName);
                        reqDataStr =
                        @"{
                                """ + Constants.V4OdataId + @""" : """ + v4OdataIdVal + @"""
                        }";
                        bool hasEtag = additionalInfos.Last().HasEtag;
                        resp = WebHelper.UpdateEntity(url, reqDataStr, HttpMethod.Put, hasEtag);
                        detail3 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Put, string.Empty, resp, string.Empty, reqDataStr);

                        if (null != resp && HttpStatusCode.NoContent == resp.StatusCode)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            detail3.ErrorMessage = "PUT to $ref failed by above URI";
                        }
                    }
                }
                else
                {
                    passed = false;
                    detail2.ErrorMessage = "Get entity failed from above URI.";
                }

                // Restore the service.
                var resps = WebHelper.DeleteEntities(additionalInfos);
            }
            else
            {
                passed = false;
                detail1.ErrorMessage = "Created the new entity failed for above URI.";
            }

            var details = new List<ExtensionRuleResultDetail>() { detail1, detail2, detail3 }.RemoveNullableDetails();
            info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
