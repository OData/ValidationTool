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
    /// Class of extension rule for Advanced.Conformance.100906
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance100906 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.100906";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "9.6. SHOULD support $top and $skip on expanded properties (section 11.2.4.2.1)";
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

            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail(this.Name);
            ExtensionRuleResultDetail detail2 = new ExtensionRuleResultDetail(this.Name);
            var restrictions = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (!AnnotationsHelper.GetExpandRestrictions(context.MetadataDocument, context.VocCapabilities, ref restrictions))
            {
                detail1.ErrorMessage = "Cannot find any appropriate entity-sets which supports $expand system query options.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            if (!AnnotationsHelper.IsSuitableNavigationProperty(NavigationRoughType.CollectionValued, ref restrictions))
            {
                detail1.ErrorMessage = "Cannot find any collection-valued navigation properties in any entity-sets which supports $expand system query options.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            string entitySet = string.Empty;
            string navigPropName = string.Empty;

            #region Verify the system query $top.

            foreach (var r in restrictions)
            {
                if (string.IsNullOrEmpty(r.Key) ||
                    null == r.Value.Item1 || !r.Value.Item1.Any() ||
                    null == r.Value.Item2 || !r.Value.Item2.Any())
                {
                    continue;
                }

                foreach (var np in r.Value.Item2)
                {
                    string nEntityTypeShortName = np.NavigationPropertyType.RemoveCollectionFlag().GetLastSegment();
                    string nEntitySetName = nEntityTypeShortName.MapEntityTypeShortNameToEntitySetName();

                    if (false == nEntitySetName.IsEntitySetSupportTopQuery(context.MetadataDocument, new List<string>() { context.VocCapabilities }))
                    {
                        continue;
                    }

                    navigPropName = np.NavigationPropertyName;
                    break;
                }

                entitySet = r.Key;
                break;
            }

            if (string.IsNullOrEmpty(entitySet))
            {
                detail1.ErrorMessage = "Cannot find an appropriate entity-set which supports $expand system query option.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            if (string.IsNullOrEmpty(navigPropName))
            {
                detail1.ErrorMessage = "Cannot find any collection-valued navigation properties which support system query options $top.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            bool? isTopQueryValidation = null;
            string url = string.Format("{0}/{1}?$expand={2}($top=1)", context.ServiceBaseUri, entitySet, navigPropName);
            var response = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail1 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), response);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject feed;
                response.ResponsePayload.TryToJObject(out feed);

                if (feed != null && JTokenType.Object == feed.Type)
                {
                    var entities = JsonParserHelper.GetEntries(feed);

                    foreach (var entity in entities)
                    {
                        if (entity[navigPropName] != null)
                        {
                            if (JTokenType.Array == entity[navigPropName].Type && ((JArray)entity[navigPropName]).Count <= 1)
                            {
                                isTopQueryValidation = true;
                            }
                            else
                            {
                                passed = false;
                                detail1.ErrorMessage = "The service does not execute an accurate result on the system query option '$top' for expanded properties.";
                                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                                return passed;
                            }
                        }
                    }
                }
            }
            else
            {
                passed = false;
                detail1.ErrorMessage = "The service does not support the system query option '$top' for expanded properties.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            #endregion

            #region Verify the system query $skip.
            
            foreach (var r in restrictions)
            {
                if (string.IsNullOrEmpty(r.Key) ||
                    null == r.Value.Item1 || !r.Value.Item1.Any() ||
                    null == r.Value.Item2 || !r.Value.Item2.Any())
                {
                    continue;
                }

                foreach (var np in r.Value.Item2)
                {
                    string nEntityTypeShortName = np.NavigationPropertyType.RemoveCollectionFlag().GetLastSegment();
                    string nEntitySetName = nEntityTypeShortName.MapEntityTypeShortNameToEntitySetName();

                    if (false == nEntitySetName.IsEntitySetSupportSkipQuery(context.MetadataDocument, new List<string>() { context.VocCapabilities }))
                    {
                        continue;
                    }

                    navigPropName = np.NavigationPropertyName;
                    break;
                }

                entitySet = r.Key;
                break;
            }

            if (string.IsNullOrEmpty(entitySet))
            {
                detail2.ErrorMessage = "Cannot find an appropriate entity-set which supports $expand system query option.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            if (string.IsNullOrEmpty(navigPropName))
            {
                detail2.ErrorMessage = "Cannot find any collection-valued navigation properties which support system query options $skip.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            bool? isSkipQueryValidation = null;
            url = string.Format("{0}/{1}?$expand={2}($skip=1)", context.ServiceBaseUri, entitySet, navigPropName);
            response = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail2 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), response);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject feed;
                response.ResponsePayload.TryToJObject(out feed);

                if (feed != null && JTokenType.Object == feed.Type)
                {
                    var entities = JsonParserHelper.GetEntries(feed);

                    foreach (var entity in entities)
                    {
                        if (entity[navigPropName] != null)
                        {
                            int actualAmount = 0;
                            JsonParserHelper.GetEntitiesNumFromCollectionValuedNavigProp(context.DestinationBasePath, (JObject)entity, navigPropName, context.RequestHeaders, ref actualAmount);

                            if (JTokenType.Array == entity[navigPropName].Type && ((JArray)entity[navigPropName]).Count <= actualAmount)// TODO: This should be justified by total count
                            {
                                isSkipQueryValidation = true;
                            }
                            else
                            {
                                isSkipQueryValidation = false;
                                detail2.ErrorMessage = "The service does not execute an accurate result on the system query option '$skip' for expanded properties.";
                                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                                return isSkipQueryValidation;
                            }
                        }
                    }
                }
            }
            else
            {
                passed = false; 
                detail2.ErrorMessage = "The service does not support the system query option '$skip' for expanded properties.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            #endregion

            if (isTopQueryValidation == true && isSkipQueryValidation == true)
            {
                passed = true;
            }

            details.Add(detail1);
            details.Add(detail2);
            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);

            return passed;
        }
    }
}
