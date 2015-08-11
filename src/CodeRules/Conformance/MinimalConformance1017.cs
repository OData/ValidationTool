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
    /// Class of extension rule for Minimal.Conformance.1017
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1017 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1017";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "17. MUST support POST to $ref to add an existing entity to an updatable related collection (section 11.4.6.1)";
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
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail(this.Name);
            ExtensionRuleResultDetail detail2 = new ExtensionRuleResultDetail(this.Name);
            ExtensionRuleResultDetail detail3 = new ExtensionRuleResultDetail(this.Name);
            List<string> keyPropertyTypes = new List<string>() { "Edm.Int32", "Edm.Int16", "Edm.Int64", "Edm.Guid", "Edm.String" };
            List<EntityTypeElement> entityTypeElements = MetadataHelper.GetEntityTypes(serviceStatus.MetadataDocument, 1, keyPropertyTypes, null, NavigationRoughType.CollectionValued).ToList();

            if (entityTypeElements == null || entityTypeElements.Count == 0)
            {
                detail1.ErrorMessage = "To verify this rule it expects an entity type with Int32/Int64/Int16/Guid/String key property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail1);

                return passed;
            }

            foreach (var et in entityTypeElements)
            {
                string navigPropName = null;
                string navigPropRelatedEntitySetUrl = null;
                string navigPropRelatedEntityTypeKeyName = null;

                var matchEntity = et.EntitySetName.GetRestrictions(serviceStatus.MetadataDocument, termDocs.VocCapabilitiesDoc,
                    new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>()
                    {
                        AnnotationsHelper.GetDeleteRestrictions, AnnotationsHelper.GetInsertRestrictions
                    });

                if (string.IsNullOrEmpty(matchEntity.Item1)
                    || matchEntity.Item2 == null || !matchEntity.Item2.Any()
                    || matchEntity.Item3 == null || !matchEntity.Item3.Any())
                {
                    continue;
                }

                foreach (var np in matchEntity.Item3)
                {
                    navigPropName = np.NavigationPropertyName;
                    string navigEntityTypeShortName = np.NavigationPropertyType.RemoveCollectionFlag().GetLastSegment();
                    List<NormalProperty> navigKeyProps = MetadataHelper.GetKeyProperties(serviceStatus.MetadataDocument, navigEntityTypeShortName).ToList();

                    if (navigKeyProps.Count == 1 && keyPropertyTypes.Contains(navigKeyProps[0].PropertyType))
                    {
                        navigPropRelatedEntitySetUrl = navigEntityTypeShortName.MapEntityTypeShortNameToEntitySetURL();
                        navigPropRelatedEntityTypeKeyName = navigKeyProps[0].PropertyName;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(navigPropRelatedEntityTypeKeyName) && !string.IsNullOrEmpty(navigPropRelatedEntitySetUrl))
                {
                    string entitySetUrl = et.EntitySetName.MapEntitySetNameToEntitySetURL();
                    string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
                    var additionalInfos = new List<AdditionalInfo>();
                    var reqData = dFactory.ConstructInsertedEntityData(et.EntitySetName, et.EntityTypeShortName, null, out additionalInfos);
                    string reqDataStr = reqData.ToString();
                    bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
                    var resp = WebHelper.CreateEntity(url, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
                    detail1 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);

                    if (resp.StatusCode.HasValue && HttpStatusCode.Created == resp.StatusCode)
                    {
                        string entityId = additionalInfos.Last().EntityId;
                        url = serviceStatus.RootURL.TrimEnd('/') + @"/" + navigPropRelatedEntitySetUrl;
                        resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, serviceStatus.DefaultHeaders);
                        detail2 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, serviceStatus.DefaultHeaders), resp, string.Empty, reqDataStr);
                        if (null != resp && resp.StatusCode == HttpStatusCode.OK)
                        {
                            JObject feed;
                            resp.ResponsePayload.TryToJObject(out feed);
                            var entities = JsonParserHelper.GetEntries(feed);
                            if (entities.Count > 0)
                            {
                                // Use the HTTP method POST to link the navigation property for an entity.
                                reqDataStr = @"{""" + Constants.V4OdataId + @""" : """ + entities[0][Constants.V4OdataId].ToString() + @"""}";
                                url = string.Format("{0}/{1}/$ref", entityId, navigPropName);
                                resp = WebHelper.CreateEntity(url, reqDataStr);
                                detail3 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);
                                if (null != resp && resp.StatusCode == HttpStatusCode.NoContent)
                                {
                                    passed = true;
                                }
                                else
                                {
                                    passed = false;
                                    detail3.ErrorMessage = "Created the new entity failed for above URI.";
                                }
                            }
                        }
                        else
                        {
                            passed = false;
                            detail2.ErrorMessage = "Get entity failed from above URI.";
                        }

                        // Restore the service.
                        var resps = WebHelper.DeleteEntities(context.RequestHeaders, additionalInfos);
                    }
                    else
                    {
                        passed = false;
                        detail1.ErrorMessage = "Created the new entity failed for above URI.";
                    }
                }

                if (false == passed)
                {
                    break;
                }
            }

            var details = new List<ExtensionRuleResultDetail>() { detail1, detail2, detail3 }.RemoveNullableDetails();
            info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
