// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Net;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Intermediate.Conformance.1004
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IntermediateConformance1004 : ConformanceIntermediateExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Intermediate.Conformance.1004";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "4. MUST support casting to a derived type according to [OData-URL] if derived types are present in the model";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.2";
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
            var serviceStatus = ServiceStatus.GetInstance();
            var detail = new ExtensionRuleResultDetail(this.Name);
            List<EntityTypeElement> entityTypes = MetadataHelper.GetEntityTypeInheritance(serviceStatus.MetadataDocument, serviceStatus.ServiceDocument);

            if (!entityTypes.Any())
            {
                detail.ErrorMessage = "Cannot find a derived entity type from metadata.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }
            
            string entityTypeShortName = string.Empty;
            string entitySetUrl = string.Empty;
            string derivedTypeFullName = string.Empty;
            foreach (var entityType in entityTypes)
            {
                entityTypeShortName = entityType.EntityTypeShortName;
                derivedTypeFullName = string.Format("{0}.{1}", entityType.EntityTypeNamespace, entityTypeShortName);
                if (!string.IsNullOrEmpty(entityType.EntitySetName))
                {
                    entitySetUrl = entityType.EntitySetName.MapEntitySetNameToEntitySetURL();
                }
                else
                {
                    continue;
                }

                if (string.IsNullOrEmpty(entityTypeShortName) ||
                    string.IsNullOrEmpty(entitySetUrl) ||
                    string.IsNullOrEmpty(derivedTypeFullName))
                {
                    continue;
                }
            }

            if (string.IsNullOrEmpty(entitySetUrl))
            {
                detail.ErrorMessage = string.Format("Cannot find the entity-set URL which is matched with {0}", entityTypeShortName);
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string url = string.Format("{0}/{1}/{2}", serviceStatus.RootURL, entitySetUrl, derivedTypeFullName);
            var resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, serviceStatus.DefaultHeaders);
            detail = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, serviceStatus.DefaultHeaders), resp);
            if (resp != null && resp.StatusCode == HttpStatusCode.OK)
            {
                JObject feed;
                resp.ResponsePayload.TryToJObject(out feed);

                if (feed != null && JTokenType.Object == feed.Type)
                {
                    var entities = JsonParserHelper.GetEntries(feed).ToList();
                    var appropriateEntities = entities.FindAll(e => e[Constants.V4OdataType].ToString().Contains(derivedTypeFullName)).Select(e => e);

                    if (entities.Count == appropriateEntities.Count())
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;

                        detail.ErrorMessage = "The service does not execute an acculate result on casting to a derived type.";
                    }
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = JsonParserHelper.GetErrorMessage(resp.ResponsePayload);
            }

            info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);
            return passed;
        }
    }
}
