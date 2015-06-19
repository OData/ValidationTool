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
    /// Class of extension rule for Advanced.Conformance.100903
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance100903 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.100903";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "9.3. MUST support cast segment in expand with derived types (section 11.2.4.2.1)";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.3";
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
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);
            var entityTypes = MetadataHelper.GetEntityTypeInheritance(context.MetadataDocument, context.ServiceDocument);

            if (null == entityTypes || !entityTypes.Any())
            {
                detail.ErrorMessage = "Cannot get a derived entity type from metadata, so cannot verify this rule.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            string entitySetUrl = string.Empty;
            string navigPropName = string.Empty;
            string derivedType = string.Empty;

            foreach (var et in entityTypes)
            {
                string navigPropType = string.Format("Collection({0})", et.BaseTypeFullName);
                var entityTypeFullNames = MetadataHelper.GetEntityTypeNamesContainsSpecifiedNavigProp(navigPropType, context.MetadataDocument);

                if (null == entityTypeFullNames || !entityTypeFullNames.Any())
                {
                    continue;
                }

                var entityTypeFullName = entityTypeFullNames.First();
                entitySetUrl = entityTypeFullName.MapEntityTypeFullNameToEntitySetURL();

                if (string.IsNullOrEmpty(entitySetUrl))
                {
                    continue;
                }

                var funcs = new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>() { AnnotationsHelper.GetExpandRestrictions };
                var restrictions = entitySetUrl.GetRestrictions(context.MetadataDocument, context.VocCapabilities, funcs);

                if (string.IsNullOrEmpty(restrictions.Item1) ||
                    null == restrictions.Item3 || !restrictions.Item3.Any())
                {
                    continue;
                }

                navigPropName = restrictions.Item3.First().NavigationPropertyName;
                derivedType = string.Format("{0}.{1}", et.EntityTypeNamespace, et.EntityTypeShortName);

                if(!string.IsNullOrEmpty(derivedType))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(entitySetUrl))
            {
                detail.ErrorMessage = "Cannot find any appropriate entity-sets which supports system query options $expand and has at least one navigation property with inherited type in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            if (string.IsNullOrEmpty(navigPropName) || string.IsNullOrEmpty(derivedType))
            {
                detail.ErrorMessage = "Cannot find any appropriate navigation properties with inherited type in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            string url = string.Format("{0}/{1}?$expand={2}/{3}", context.ServiceBaseUri, entitySetUrl, navigPropName, derivedType);
            var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                JObject jObj;
                resp.ResponsePayload.TryToJObject(out jObj);

                if (jObj != null && JTokenType.Object == jObj.Type)
                {
                    var entries = JsonParserHelper.GetEntries(jObj);

                    foreach (var entry in entries)
                    {
                        var entities = entry[navigPropName].ToList();
                        var appropriateEntities = entities
                            .FindAll(e => e[Constants.V4OdataType].ToString().Contains(derivedType))
                            .Select(e => e)
                            .ToList();

                        if (entities.Count == appropriateEntities.Count)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            detail.ErrorMessage = String.Format("The service does not execute an accurate result on cast segment in expand with derived types (Actual Value: {0}, Expected Value: {1}).", entities.Count, appropriateEntities.Count);

                            break;
                        }
                    }
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = String.Format("The service does not support cast segment in expand with derived types.");
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
