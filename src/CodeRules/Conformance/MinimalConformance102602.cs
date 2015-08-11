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
    /// Class of extension rule for Minimal.Conformance.102602
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance102602 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.102602";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "2). SHOULD support PATCH to a complex (section 11.4.9.3) property";
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
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Should;
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
            Dictionary<string, Dictionary<KeyValuePair<string, string>, List<string>>> entityTypeInfos;
            if (!MetadataHelper.GetEntityTypesWithComplexProperty(serviceStatus.MetadataDocument, "Edm.String", out entityTypeInfos))
            {
                detail1.ErrorMessage = "To verify this rule it expects complex type containing a property with string type, but there is no this complex type in metadata so cannot verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail1);

                return passed;
            }

            var entityTypeInfo = new KeyValuePair<string, Dictionary<KeyValuePair<string, string>, List<string>>>();
            string entitySetName = string.Empty;
            foreach (var etInfo in entityTypeInfos)
            {
                entitySetName = etInfo.Key.MapEntityTypeShortNameToEntitySetName();
                var funcs = new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>() 
                {
                    AnnotationsHelper.GetInsertRestrictions, AnnotationsHelper.GetUpdateRestrictions, AnnotationsHelper.GetDeleteRestrictions
                };

                var restrictions = entitySetName.GetRestrictions(serviceStatus.MetadataDocument, termDocs.VocCapabilitiesDoc, funcs);
                if (!string.IsNullOrEmpty(restrictions.Item1) ||
                    null != restrictions.Item2 || restrictions.Item2.Any() ||
                    null != restrictions.Item3 || restrictions.Item3.Any())
                {
                    entityTypeInfo = etInfo;

                    break;
                }
            }

            if (string.IsNullOrEmpty(entitySetName) ||
                string.IsNullOrEmpty(entityTypeInfo.Key) ||
                null == entityTypeInfo.Value ||
                !entityTypeInfo.Value.Any())
            {
                detail1.ErrorMessage = "Cannot find the entity-set which support insert, updata, delete restrictions at the same time.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail1);

                return passed;
            }

            string entityTypeShortName = entityTypeInfo.Key;
            string entitySetUrl = entitySetName.MapEntitySetNameToEntitySetURL();
            string complexPropName = entityTypeInfo.Value.Keys.First().Key;
            string complexPropType = entityTypeInfo.Value.Keys.First().Value;
            string propertyNameWithSpecifiedType = entityTypeInfo.Value[new KeyValuePair<string, string>(complexPropName, complexPropType)].First();

            // Create a entity
            string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + entitySetUrl;
            var additionalInfos = new List<AdditionalInfo>();
            var reqData = dFactory.ConstructInsertedEntityData(entitySetName, entityTypeShortName, null, out additionalInfos);
            reqData = dFactory.ReconstructNullableComplexData(reqData, new List<string>() { complexPropName });
            string reqDataStr = reqData.ToString();
            bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
            var resp = WebHelper.CreateEntity(url, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
            detail1 = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);
            if (resp.StatusCode == HttpStatusCode.Created)
            {
                var entityId = additionalInfos.Last().EntityId;
                var hasEtag = additionalInfos.Last().HasEtag;

                // Get a complex property except key property
                string complexProUrl = entityId + @"/" + complexPropName;
                resp = WebHelper.Get(new Uri(complexProUrl), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, serviceStatus.DefaultHeaders);
                detail2 = new ExtensionRuleResultDetail(this.Name, complexProUrl, HttpMethod.Get, StringHelper.MergeHeaders(Constants.AcceptHeaderJson, serviceStatus.DefaultHeaders), resp);

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    JObject jo;
                    resp.ResponsePayload.TryToJObject(out jo);
                    //string newComplexProperty = VerificationHelper.ConstructUpdatedEntityData(jo, new List<string>() { propertyNameWithSpecifiedType }, out hasEtag);
                    string newComplexProperty = dFactory.ConstructUpdatedEntityData(jo, new List<string>() { propertyNameWithSpecifiedType }).ToString();

                    // Update the complex property
                    resp = WebHelper.UpdateEntity(complexProUrl, context.RequestHeaders, newComplexProperty, HttpMethod.Patch, hasEtag);
                    detail3 = new ExtensionRuleResultDetail(this.Name, complexProUrl, HttpMethod.Patch, string.Empty, resp, string.Empty, newComplexProperty);
                    if (resp.StatusCode == HttpStatusCode.NoContent)
                    {
                        // Check whether the complex property is updated to new value
                        if (WebHelper.GetContent(complexProUrl, context.RequestHeaders, out resp))
                        {
                            detail4 = new ExtensionRuleResultDetail(this.Name, complexProUrl, HttpMethod.Get, string.Empty, resp);
                            resp.ResponsePayload.TryToJObject(out jo);

                            if (jo != null && jo[propertyNameWithSpecifiedType] != null && jo[propertyNameWithSpecifiedType].Value<string>().Equals(Constants.UpdateData))
                            {
                                passed = true;
                            }
                            else if (jo == null)
                            {
                                passed = false;
                                detail4.ErrorMessage = "Can not get complex property after Patch it. ";
                            }
                            else if (jo != null && jo[propertyNameWithSpecifiedType] == null)
                            {
                                passed = false;
                                detail4.ErrorMessage = string.Format("Can not get the value of {0} property in complex property {1}. ", propertyNameWithSpecifiedType, complexProUrl);
                            }
                            else if (jo != null && jo[propertyNameWithSpecifiedType] != null && !jo[propertyNameWithSpecifiedType].Value<string>().Equals(Constants.UpdateData))
                            {
                                passed = false;
                                detail4.ErrorMessage = string.Format("The value of {0} property in complex is not updated by {1}. ", propertyNameWithSpecifiedType, complexProUrl);
                            }
                        }
                    }
                    else
                    {
                        passed = false;
                        detail3.ErrorMessage = "Update complex property in the created entity failed. ";
                    }
                }
                else if (resp.StatusCode == HttpStatusCode.NoContent)
                {
                    detail2.ErrorMessage = "The value of property with complex type is null.";
                }
                else
                {
                    passed = false;
                    detail2.ErrorMessage = "Get complex property in the created entity failed. ";
                }
                // Delete the entity
                var resps = WebHelper.DeleteEntities(context.RequestHeaders, additionalInfos);
            }
            else
            {
                passed = false;
                detail1.ErrorMessage = "Created the new entity failed for above URI. ";
            }

            var details = new List<ExtensionRuleResultDetail>() { detail1, detail2, detail3, detail4 }.RemoveNullableDetails();
            info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
