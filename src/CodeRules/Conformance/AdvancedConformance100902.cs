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
    /// Class of extension rule for Advanced.Conformance.100902
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance100902 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.100902";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "9.2. MUST support $filter on expanded entities (section 11.2.4.2.1)";
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
            List<string> vocDocs = new List<string>() { context.VocCapabilities, context.VocCore, context.VocMeasures };
            List<string> expectedTypes = new List<string>() { "Edm.String", "Edm.Int32", "Edm.Int16", "Edm.Single", "Edm.Double", "Edm.Boolean", "Edm.DateTimeOffset", "Edm.Guid" };
            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();
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
            string primitivePropertyType = string.Empty;

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
                    var funcs = new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>() { AnnotationsHelper.GetFilterRestrictions };
                    var rest = nEntitySetName.GetRestrictions(context.MetadataDocument, context.VocCapabilities, funcs, expectedTypes);

                    if (string.IsNullOrEmpty(rest.Item1) ||
                        null == rest.Item2 || !rest.Item2.Any() ||
                        null == rest.Item3 || !rest.Item3.Any())
                    {
                        continue;
                    }

                    navigPropName = np.NavigationPropertyName;
                    primitivePropertyName = rest.Item2.First().PropertyName;
                    primitivePropertyType = rest.Item2.First().PropertyType;

                    break;
                }

                entitySet = r.Key;

                break;
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

            if (string.IsNullOrEmpty(primitivePropertyName) || string.IsNullOrEmpty(primitivePropertyType))
            {
                detail.ErrorMessage = "Cannot get an appropriate primitive property from navigation properties.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            Uri uri = new Uri(string.Format("{0}/{1}?$expand={2}", context.ServiceBaseUri, entitySet, navigPropName));
            var response = WebHelper.Get(uri, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (HttpStatusCode.OK != response.StatusCode)
            { 
                passed = false;
                detail.ErrorMessage = JsonParserHelper.GetErrorMessage(response.ResponsePayload);
                info = new ExtensionRuleViolationInfo(uri, response.ResponsePayload, detail);

                return passed;
            }

            JObject feed;
            response.ResponsePayload.TryToJObject(out feed);

            if (feed != null && JTokenType.Object == feed.Type)
            {
                var entities = JsonParserHelper.GetEntries(feed);
                var navigProp = entities[0][navigPropName] as JArray;

                if (navigProp != null && navigProp.Count != 0)
                {
                    string propVal = navigProp[0][primitivePropertyName].ToString();
                    string compareVal = propVal;

                    if (primitivePropertyType.Equals("Edm.String"))
                    {
                        compareVal = "'" + propVal + "'";
                    }

                    string url = string.Format("{0}/{1}?$expand={2}($filter={3} eq {4})", context.ServiceBaseUri, entitySet, navigPropName, primitivePropertyName, compareVal);
                    var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                    detail = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        JObject jObj;
                        resp.ResponsePayload.TryToJObject(out jObj);

                        if (jObj != null && JTokenType.Object == jObj.Type)
                        {
                            var entries = JsonParserHelper.GetEntries(jObj).ToList();

                            foreach (var entry in entries)
                            {
                                if (entry[navigPropName] != null && ((JArray)entry[navigPropName]).Count == 0)
                                {
                                    continue;
                                }
                                else if (entry[navigPropName] != null && ((JArray)entry[navigPropName]).Count > 0)
                                {
                                    var temp = entry[navigPropName].ToList()
                                               .FindAll(en => propVal == en[primitivePropertyName].ToString())
                                               .Select(en => en);

                                    if (entry[navigPropName].ToList().Count == temp.Count())
                                    {
                                        passed = true;
                                    }
                                    else
                                    {
                                        passed = false;
                                        detail.ErrorMessage = string.Format("The service does not execute an accurate result on system query option '$filter' (Actual Value: {0}, Expected Value: {1}).", entry[navigPropName].ToList().Count, temp.Count());
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        passed = false;
                        detail.ErrorMessage = string.Format("The service does not support system query option '$filter' on expanded entities.");
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
