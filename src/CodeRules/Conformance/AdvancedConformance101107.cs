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
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Advanced.Conformance.101107
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance101107 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.101107";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "5). If the set of request headers of a Batch request are valid the service MUST return a 200 OK HTTP response code. (section 11.7.4)";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.7.4";
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
            var termDocs = TermDocuments.GetInstance();
            var serviceStatus = ServiceStatus.GetInstance();
            var dFactory = DataFactory.Instance();
            var detail1 = new ExtensionRuleResultDetail(this.Name);
            var detail2 = new ExtensionRuleResultDetail(this.Name);
            List<string> keyPropertyTypes = new List<string>() { "Edm.Int32", "Edm.Int16", "Edm.Int64", "Edm.Guid", "Edm.String" };
            List<EntityTypeElement> entityTypeElements = MetadataHelper.GetEntityTypes(serviceStatus.MetadataDocument, 1, keyPropertyTypes, null, NavigationRoughType.None).ToList();
            if (null == entityTypeElements || 0 == entityTypeElements.Count())
            {
                detail1.ErrorMessage = "To verify this rule it expects an entity type with Int32/Int64/Int16/Guid/String key property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail1);

                return passed;
            }

            EntityTypeElement entityType = null;
            foreach (var ele in entityTypeElements)
            {
                if (ele.EntityTypeShortName.IsMediaType())
                {
                    continue;
                }

                var funcs = new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>()
                {
                    AnnotationsHelper.GetInsertRestrictions, AnnotationsHelper.GetDeleteRestrictions
                };
                var methods = new List<Func<string, string, List<string>, bool?>>()
                {
                    SupportiveFeatureHelper.IsSupportBatchOperation
                };
                var restrictions = ele.EntitySetName.GetRestrictions(serviceStatus.MetadataDocument, termDocs.VocCapabilitiesDoc, funcs, null, NavigationRoughType.None, methods);

                if (!string.IsNullOrEmpty(restrictions.Item1) ||
                    null != restrictions.Item2 || restrictions.Item2.Any() ||
                    null != restrictions.Item3 || restrictions.Item3.Any())
                {
                    entityType = ele;
                    break;
                }
            }

            if (null == entityType || string.IsNullOrEmpty(entityType.EntitySetName))
            {
                detail1.ErrorMessage = "The service does not support batch operation.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail1);

                return passed;
            }

            string entitySetUrl = entityType.EntitySetName.MapEntitySetNameToEntitySetURL();
            if (string.IsNullOrEmpty(entitySetUrl))
            {
                detail1.ErrorMessage = string.Format("Cannot find the entity-set URL which is matched with {0}", entityType.EntityTypeShortName);
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail1);

                return passed;
            }

            string feedUrl = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
            var additionalInfos = new List<AdditionalInfo>();
            var reqData = dFactory.ConstructInsertedEntityData(entityType.EntitySetName, entityType.EntityTypeShortName, null, out additionalInfos);
            string reqDataStr = reqData.ToString();
            var resp = WebHelper.CreateEntity(feedUrl, reqData, false, ref additionalInfos);
            detail1 = new ExtensionRuleResultDetail(this.Name, feedUrl, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);
            if (HttpStatusCode.Created == resp.StatusCode)
            {
                string entityId = additionalInfos.Last().EntityId;
                bool hasEtag = additionalInfos.Last().HasEtag;
                resp = WebHelper.GetEntity(entityId);

                if (HttpStatusCode.OK == resp.StatusCode)
                {
                    string boundary = "batch_4e1a76dc-b738-4aa4-9f93-df661d0a4c9f";
                    string batchReqData = string.Format(
@"
--batch_4e1a76dc-b738-4aa4-9f93-df661d0a4c9f
Content-Type: multipart/mixed; boundary=changeset_77162fcd-b8da-41ac-a9f8-9357efbbd621

--changeset_77162fcd-b8da-41ac-a9f8-9357efbbd621
Content-Type: application/http 
Content-Transfer-Encoding: binary 
Content-ID: 1

DELETE {0} HTTP/1.1
"
+ (hasEtag ? "If-Match: *" : string.Empty) +
@"

--changeset_77162fcd-b8da-41ac-a9f8-9357efbbd621--

--batch_4e1a76dc-b738-4aa4-9f93-df661d0a4c9f--
", entityId);
                    resp = WebHelper.BatchOperation(serviceStatus.RootURL, batchReqData, boundary);
                    detail2 = new ExtensionRuleResultDetail(this.Name, serviceStatus.RootURL + "/$batch", HttpMethod.Post, string.Empty, resp, string.Empty, batchReqData);

                    if (HttpStatusCode.OK == resp.StatusCode)
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        detail2.ErrorMessage = "The OData service does not return a 200 OK HTTP status code.";
                    }
                }
            }
            else
            {
                passed = false;
                detail1.ErrorMessage = string.Format("Created the new entity failed for above URI with entity data {0}. ", reqDataStr);
            }

            var details = new List<ExtensionRuleResultDetail>() { detail1, detail2 }.RemoveNullableDetails();
            info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
