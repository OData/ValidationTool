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
    /// Class of extension rule for Intermediate.Conformance.100705
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IntermediateConformance100705 : ConformanceIntermediateExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Intermediate.Conformance.100705";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "7.5. SHOULD support $filter on expanded entities (section 11.2.4.2.1)";
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
            List<string> expectedTypes = new List<string>() { "Edm.String", "Edm.Int32", "Edm.Int16", "Edm.Single", "Edm.Double", "Edm.Boolean", "Edm.DateTimeOffset", "Edm.Guid" };
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail(this.Name);

            var restrictions = AnnotationsHelper.GetRestrictions(
                context.MetadataDocument,
                context.VocCapabilities,
                new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>() { AnnotationsHelper.GetExpandRestrictions, AnnotationsHelper.GetFilterRestrictions },
                null,
                NavigationRoughType.CollectionValued);

            if (string.IsNullOrEmpty(restrictions.Item1) ||
                null == restrictions.Item2 || !restrictions.Item2.Any() ||
                null == restrictions.Item3 || !restrictions.Item3.Any())
            {
                detail1.ErrorMessage = "Cannot find an appropriate entity-set which supports $expand, $filter system query options in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            string entitySet = restrictions.Item1;
            string navigPropName = string.Empty;
            string navigPropType = string.Empty;

            foreach (var np in restrictions.Item3)
            {
                if (NavigationRoughType.CollectionValued == np.NavigationRoughType)
                {
                    navigPropName = np.NavigationPropertyName;
                    navigPropType = np.NavigationPropertyType;
                }
            }

            string entityType = navigPropType.RemoveCollectionFlag().GetLastSegment();
            var tmp = MetadataHelper.GetPropertiesWithSpecifiedTypeFromEntityType(entityType, context.MetadataDocument, expectedTypes);

            if (null == tmp && !tmp.Any())
            {
                detail1.ErrorMessage = "Cannot find an appropriate primitive properties of entity type in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            var str = tmp.First().Split(',');
            string primitivePropertyName = str[0];
            string primitivePropertyType = str[1];

            if (string.IsNullOrEmpty(entitySet))
            {
                detail1.ErrorMessage = "Cannot find an appropriate entity-set which supports $expand system query option.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            if (string.IsNullOrEmpty(navigPropName) || string.IsNullOrEmpty(navigPropType))
            {
                detail1.ErrorMessage = "Cannot get expanded entities because cannot get collection type of navigation property from metadata";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            if (string.IsNullOrEmpty(primitivePropertyName) || string.IsNullOrEmpty(primitivePropertyType))
            {
                detail1.ErrorMessage = "Cannot get an appropriate primitive property from navigation properties.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            Uri uri = new Uri(string.Format("{0}/{1}?$expand={2}", context.ServiceBaseUri, entitySet, navigPropName));
            var response = WebHelper.Get(uri, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (HttpStatusCode.OK != response.StatusCode)
            {
                passed = false;
                detail1.ErrorMessage = JsonParserHelper.GetErrorMessage(response.ResponsePayload);
                info = new ExtensionRuleViolationInfo(uri, response.ResponsePayload, detail1);
   
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
                    detail1 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

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
                                        detail1.ErrorMessage = string.Format("The service does not execute an accurate result on system query option '$filter' (Actual Value: {0}, Expected Value: {1}).", entry[navigPropName].ToList().Count, temp.Count());
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        passed = false;
                        detail1.ErrorMessage = "The service does not support system query option '$filter' on expanded entities.";
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);
            return passed;
        }
    }
}
