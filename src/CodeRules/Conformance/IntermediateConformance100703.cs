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
    /// Class of extension rule for Intermediate.Conformance.100703
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IntermediateConformance100703 : ConformanceIntermediateExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Intermediate.Conformance.100703";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "7.3. SHOULD support additional filter operations (section 11.2.5.1.1) and MUST return 501 Not Implemented for any unsupported filter operations (section 9.3.1)";
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
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);
            var filterRestrictions = AnnotationsHelper.GetFilterRestrictions(context.MetadataDocument, context.VocCapabilities);

            if (string.IsNullOrEmpty(filterRestrictions.Item1) ||
                null == filterRestrictions.Item2 || !filterRestrictions.Item2.Any())
            {
                detail.ErrorMessage = "Cannot find an appropriate entity-set which supports $filter system query options in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            string entitySet = filterRestrictions.Item1;
            string primitivePropName = filterRestrictions.Item2.First().PropertyName;
            string primitivePropType = filterRestrictions.Item2.First().PropertyType;

            string url = string.Format("{0}/{1}", context.ServiceBaseUri, entitySet);
            var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (null == resp || HttpStatusCode.OK != resp.StatusCode)
            {
                detail.ErrorMessage = JsonParserHelper.GetErrorMessage(resp.ResponsePayload);
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return passed;
            }

            JObject feed;
            resp.ResponsePayload.TryToJObject(out feed);

            if (feed != null && JTokenType.Object == feed.Type)
            {
                var entities = JsonParserHelper.GetEntries(feed);
                string propVal = "Edm.String" == primitivePropType ? string.Format("'{0}'", entities[0][primitivePropName].ToString()) : entities[0][primitivePropName].ToString();

                #region Equal operation on filter.
                // The comparison operators does not contain "undefined".
                url = string.Format("{0}?$filter={1} undefined {2}", url, primitivePropName, propVal);
                resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    JObject jObj;
                    resp.ResponsePayload.TryToJObject(out jObj);

                    if (jObj != null && JTokenType.Object == jObj.Type)
                    {
                        var entries = JsonParserHelper.GetEntries(jObj).ToList();
                        var temp = entries.FindAll(en => propVal == en[primitivePropName].ToString()).Select(en => en);

                        if (entries.Count() == temp.Count())
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            detail.ErrorMessage = string.Format("The actual result of filter operation is {0}, and the expected result of filter operation is {1}", entities.Count, temp.Count());
                        }
                    }
                }
                else if (resp.StatusCode == HttpStatusCode.NotImplemented)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = String.Format(JsonParserHelper.GetErrorMessage(resp.ResponsePayload));
                }
                #endregion
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return passed;
        }
    }
}
