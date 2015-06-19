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
    /// Class of extension rule for Advanced.Conformance.100905
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance100905 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.100905";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "9.5. SHOULD support the $count system query option for expanded properties (section 11.2.4.2.1)";
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
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);
            var restrictions = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (!AnnotationsHelper.GetExpandRestrictions(context.MetadataDocument, context.VocCapabilities, ref restrictions))
            {
                detail.ErrorMessage = "Cannot find any appropriate entity-sets which supports $expand system query options.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            if (!AnnotationsHelper.IsSuitableNavigationProperty(NavigationRoughType.CollectionValued, ref restrictions))
            {
                detail.ErrorMessage = "Cannot find any collection-valued navigation properties in any entity-sets which supports $expand system query options.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            string entitySet = string.Empty;
            string navigProp = string.Empty;

            foreach (var r in restrictions)
            {
                if (string.IsNullOrEmpty(r.Key) ||
                    null == r.Value.Item1 || !r.Value.Item1.Any() ||
                    null == r.Value.Item2 || !r.Value.Item2.Any())
                {
                    continue;
                }

                entitySet = r.Key;

                foreach (var np in r.Value.Item2)
                {
                    navigProp = np.NavigationPropertyName;
                    string nEntityTypeShortName = np.NavigationPropertyType.RemoveCollectionFlag().GetLastSegment();
                    var rest = AnnotationsHelper.GetCountRestrictions(context.MetadataDocument, context.VocCapabilities);

                    if (string.IsNullOrEmpty(rest.Item1) ||
                        null == rest.Item2 || !rest.Item2.Any() ||
                        null == rest.Item3 || !rest.Item3.Any())
                    {
                        continue;
                    }

                    break;
                }

                break;
            }

            if (string.IsNullOrEmpty(entitySet))
            {
                detail.ErrorMessage = "Cannot find an appropriate entity-set which supports $expand system query option.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            if (string.IsNullOrEmpty(navigProp))
            {
                detail.ErrorMessage = "Cannot get expanded entities because cannot get collection type of navigation property from metadata";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            string url = string.Format("{0}/{1}?$expand={2}($count=true)", context.ServiceBaseUri, entitySet, navigProp);
            Uri uri = new Uri(url);
            Response resp = WebHelper.Get(uri, Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail(this.Name, uri.AbsoluteUri, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

            if (resp != null && resp.StatusCode == HttpStatusCode.OK)
            {
                JObject feed;
                resp.ResponsePayload.TryToJObject(out feed);

                if (null == feed)
                {
                    detail.ErrorMessage = "Cannot find any entity-sets in the service.\r\n";
                    info = new ExtensionRuleViolationInfo(uri, resp.ResponsePayload, detail);

                    return passed;
                }

                JObject entry = null;
                var entities = JsonParserHelper.GetEntries(feed);

                foreach (var e in entities)
                {
                    if (null != e[navigProp] && JTokenType.Array == e[navigProp].Type)
                    {
                        entry = e as JObject;
                        break;
                    }
                }

                if (null == entry)
                {
                    detail.ErrorMessage = "Cannot find an appropriate entity which contains at least one collection-valued navigation property.\r\n";
                    info = new ExtensionRuleViolationInfo(uri, resp.ResponsePayload, detail);

                    return passed;
                }


                if (null != entry && JTokenType.Object == entry.Type)
                {
                    JArray jArr = entry[navigProp] as JArray;
                    int actualAmount = 0;
                    JsonParserHelper.GetEntitiesNumFromCollectionValuedNavigProp(entry[Constants.V4OdataId].ToString(), entry, navigProp, context.RequestHeaders, ref actualAmount);

                    if (null != entry[navigProp + Constants.V4OdataCount] && actualAmount == (int)entry[navigProp + Constants.V4OdataCount])
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        detail.ErrorMessage = "The service does not execute an accurate result on the system query option '$count' for expanded properties.";
                    }
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The service does not support the system query option '$count' for expanded properties.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
