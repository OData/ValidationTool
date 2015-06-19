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
    /// Class of extension rule for Advanced.Conformance.100907
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance100907 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.100907";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "9.7. SHOULD support $search on expanded properties (section 11.2.4.2.1)";
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
            string navigPropName = string.Empty;
            string primitivePropertyName = string.Empty;

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
                    string nEntityType = np.NavigationPropertyType.RemoveCollectionFlag().GetLastSegment();
                    var funcs = new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>() { AnnotationsHelper.GetFilterRestrictions };
                    var expectedTypes = new List<string>() { "Edm.String" };
                    var props = MetadataHelper.GetNormalProperties(context.MetadataDocument, nEntityType);

                    if (null == props || !props.Any())
                    {
                        continue;
                    }

                    var targetProps = props.Where(p => expectedTypes.Contains(p.PropertyType)).Select(p => p);

                    if (!targetProps.Any())
                    {
                        continue;
                    }

                    navigPropName = np.NavigationPropertyName;
                    primitivePropertyName = targetProps.First().PropertyName;

                    break;
                }

                entitySet = r.Key;

                if (!string.IsNullOrEmpty(navigPropName) && !string.IsNullOrEmpty(primitivePropertyName))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(entitySet))
            {
                detail.ErrorMessage = "Cannot find an appropriate entity-set which supports $expand system query option.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            if (string.IsNullOrEmpty(navigPropName))
            {
                detail.ErrorMessage = "Cannot get expanded entities because cannot get collection type of navigation property from metadata";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            if (string.IsNullOrEmpty(primitivePropertyName))
            {
                detail.ErrorMessage = "Cannot get an appropriate primitive property from navigation properties.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            string url = string.Format("{0}/{1}?$expand={2}", context.ServiceBaseUri, entitySet, navigPropName);
            var resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (null == resp || HttpStatusCode.OK != resp.StatusCode)
            {
                detail.ErrorMessage = JsonParserHelper.GetErrorMessage(resp.ResponsePayload);
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            JObject feed;
            resp.ResponsePayload.TryToJObject(out feed);
            var entities = JsonParserHelper.GetEntries(feed);

            if (null == entities || !entities.Any())
            {
                detail.ErrorMessage = string.Format("Cannot find any entities from the entity-set '{0}'", entitySet);
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            string searchVal = string.Empty;

            foreach (var en in entities)
            {
                if (null != en[navigPropName] || JTokenType.Array == en[navigPropName].Type || en[navigPropName].Any())
                {
                    if (JTokenType.Object != en[navigPropName].First.Type)
                    {
                        break;
                    }

                    var nEntity = en[navigPropName].First as JObject;
                    searchVal = nEntity[primitivePropertyName].ToString().Contains(" ") ?
                        string.Format("\"{0}\"", nEntity[primitivePropertyName].ToString()) :
                        nEntity[primitivePropertyName].ToString();

                    if (!string.IsNullOrEmpty(searchVal))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(searchVal))
            {
                detail.ErrorMessage = "Cannot find any appropriate search values in the expanded entities.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }
            
            url = string.Format("{0}/{1}?$expand={2}($search={3})", context.ServiceBaseUri, entitySet, navigPropName, searchVal);
            resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail(this.Name, url, "GET", String.Empty, resp);

            if (null != resp && resp.StatusCode == HttpStatusCode.OK)
            {
                resp.ResponsePayload.TryToJObject(out feed);

                if (null != feed && JTokenType.Object == feed.Type)
                {
                    entities = JsonParserHelper.GetEntries(feed);

                    foreach (var e in entities)
                    {
                        var navigEntities = e[navigPropName].ToList();

                        if (null == navigEntities || !navigEntities.Any())
                        {
                            continue;
                        }

                        foreach (var ne in navigEntities)
                        {
                            passed = null;

                            if (searchVal.StripOffDoubleQuotes() == ne[primitivePropertyName].ToString())
                            {
                                passed = true;
                            }

                            if (passed == null)
                            {
                                passed = false;
                                detail.ErrorMessage = "The service does not execute an accurate result on the system query option '$search' for expanded properties.";
                            }
                        }

                        if (passed == false)
                        {
                            break;
                        }
                    }

                    if (null == passed)
                    {
                        detail.ErrorMessage = "Cannot find any appropriate data to verify this rule.";
                    }
                }
                else
                {
                    detail.ErrorMessage = "The service does not return an correct response payload.";
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The service does not support the system query option '$search' for expanded properties.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
