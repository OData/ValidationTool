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
    /// Class of extension rule for Minimal.Conformance.1016
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1016 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1016";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "16. MUST support POST of new related entities to updatable navigation properties (section 11.4.6.1)";
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

                string navigPropName = null;
                string navigPropRelatedEntityTypeShortName = null;
                string navigPropRelatedEntitySetName = null;
                string navigPropRelatedEntitySetUrl = null;
                NormalProperty navigPropRelatedEntityTypeKey = null;

                foreach (var np in matchEntity.Item3)
                {
                    navigPropName = np.NavigationPropertyName;
                    navigPropRelatedEntityTypeShortName = np.NavigationPropertyType.RemoveCollectionFlag().GetLastSegment();
                    List<NormalProperty> navigKeyProps = MetadataHelper.GetKeyProperties(serviceStatus.MetadataDocument, navigPropRelatedEntityTypeShortName).ToList();

                    if (navigKeyProps.Count == 1 && keyPropertyTypes.Contains(navigKeyProps[0].PropertyType))
                    {
                        navigPropRelatedEntitySetName = navigPropRelatedEntityTypeShortName.MapEntityTypeShortNameToEntitySetName();
                        navigPropRelatedEntitySetUrl = navigPropRelatedEntityTypeShortName.MapEntityTypeShortNameToEntitySetURL();
                        navigPropRelatedEntityTypeKey = navigKeyProps[0];
                        break;
                    }
                }

                if (null != navigPropRelatedEntityTypeKey && !string.IsNullOrEmpty(navigPropRelatedEntitySetUrl))
                {
                    string entitySetUrl = et.EntitySetName.MapEntitySetNameToEntitySetURL();
                    string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
                    var additionalInfos1 = new List<AdditionalInfo>();
                    var reqData = dFactory.ConstructInsertedEntityData(et.EntitySetName, et.EntityTypeShortName, null, out additionalInfos1);
                    string reqDataStr = reqData.ToString();
                    bool isMediaType = !string.IsNullOrEmpty(additionalInfos1.Last().ODataMediaEtag);
                    var resp = WebHelper.CreateEntity(url, reqData, isMediaType, ref additionalInfos1);
                    detail1 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);

                    if (resp.StatusCode.HasValue && HttpStatusCode.Created == resp.StatusCode)
                    {
                        string entityId = additionalInfos1.Last().EntityId;
                        url = serviceStatus.RootURL.TrimEnd('/') + @"/" + navigPropRelatedEntitySetUrl;
                        if (string.IsNullOrEmpty(navigPropRelatedEntityTypeShortName))
                        {
                            continue;
                        }

                        var navigPropEntityType = navigPropRelatedEntityTypeShortName.GetSpecifiedEntityType(serviceStatus.MetadataDocument);
                        if (null == navigPropEntityType)
                        {
                            continue;
                        }

                        url = string.Format("{0}/{1}", entityId, navigPropName);
                        var additionalInfos2 = new List<AdditionalInfo>();
                        reqData = dFactory.ConstructInsertedEntityData(navigPropRelatedEntitySetName, navigPropRelatedEntityTypeShortName, null, out additionalInfos2);
                        reqDataStr = reqData.ToString();
                        isMediaType = !string.IsNullOrEmpty(additionalInfos2.Last().ODataMediaEtag);
                        resp = WebHelper.CreateEntity(url, reqData, isMediaType, ref additionalInfos2);
                        detail2 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);

                        if (resp.StatusCode.HasValue && resp.StatusCode == HttpStatusCode.Created)
                        {
                            passed = true;

                            // Restore the service.
                            var resps2 = WebHelper.DeleteEntities(additionalInfos2);
                        }
                        else
                        {
                            passed = false;
                            detail2.ErrorMessage = "Create entity failed for above URI.";
                        }

                        // Restore the service.
                        var resps1 = WebHelper.DeleteEntities(additionalInfos1);
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

            var details = new List<ExtensionRuleResultDetail>() { detail1, detail2 }.RemoveNullableDetails();
            info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
