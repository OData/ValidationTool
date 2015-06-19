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
    /// Class of extension rule for Intermediate.Conformance.100701
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IntermediateConformance100701 : ConformanceIntermediateExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Intermediate.Conformance.100701";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "7.1. MUST support eq, ne filter operations on properties of entities in the requested entity set (section 11.2.5.1.1)";
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
            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail(this.Name);
            var filterRestrictions = AnnotationsHelper.GetFilterRestrictions(context.MetadataDocument, context.VocCapabilities);

            if (string.IsNullOrEmpty(filterRestrictions.Item1) ||
                null == filterRestrictions.Item2 || !filterRestrictions.Item2.Any())
            {
                detail1.ErrorMessage = "Cannot find an appropriate entity-set which supports $filter system query options in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            string entitySet = filterRestrictions.Item1;
            string primitivePropName = filterRestrictions.Item2.First().PropertyName;
            string primitivePropType = filterRestrictions.Item2.First().PropertyType;

            string url = string.Format("{0}/{1}", context.ServiceBaseUri, entitySet);
            var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (null == resp || HttpStatusCode.OK != resp.StatusCode)
            {
                detail1.ErrorMessage = JsonParserHelper.GetErrorMessage(resp.ResponsePayload);
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return passed;
            }

            JObject feed;
            resp.ResponsePayload.TryToJObject(out feed);

            if (feed != null && JTokenType.Object == feed.Type)
            {
                var entities = JsonParserHelper.GetEntries(feed);
                string propVal = entities[0][primitivePropName].ToString();

                #region Equal operation on filter.
                bool? isEqualOpValidation = null;
                string pattern = "Edm.String" == primitivePropType ? "{0}/{1}?$filter={2} eq '{3}'" : "{0}/{1}?$filter={2} eq {3}";
                url = string.Format(pattern, context.ServiceBaseUri, entitySet, primitivePropName, propVal);
                resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail1 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    JObject feed1;
                    resp.ResponsePayload.TryToJObject(out feed1);

                    if (feed1 != null && JTokenType.Object == feed1.Type)
                    {
                        var entities1 = JsonParserHelper.GetEntries(feed1).ToList();
                        var temp = entities1.FindAll(en => propVal == en[primitivePropName].ToString()).Select(en => en);

                        if (entities1.Count() == temp.Count())
                        {
                            isEqualOpValidation = true;
                        }
                        else
                        {
                            isEqualOpValidation = false;
                            detail1.ErrorMessage = "The service does not execute an accurate result with system query option $filter eq.";
                            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                            return isEqualOpValidation;
                        }
                    }
                }
                else
                {
                    passed = false;
                    detail1.ErrorMessage = "Request failed with system query option $filter eq.";
                    info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                    return passed;
                }
                #endregion

                #region NotEqual operation on filter.
                bool? isNotEqualOpValidation = null;
                pattern = "Edm.String" == primitivePropType ? "{0}/{1}?$filter={2} ne '{3}'" : "{0}/{1}?$filter={2} ne {3}";
                url = string.Format(pattern, context.ServiceBaseUri, entitySet, primitivePropName, propVal);
                resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                ExtensionRuleResultDetail detail2 = new ExtensionRuleResultDetail(this.Name, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    JObject feed2;
                    resp.ResponsePayload.TryToJObject(out feed2);

                    if (feed2 != null && JTokenType.Object == feed2.Type)
                    {
                        var entities2 = JsonParserHelper.GetEntries(feed2).ToList();
                        var temp = entities2.FindAll(en => propVal != en[primitivePropName].ToString()).Select(en => en);

                        if (entities2.Count() == temp.Count())
                        {
                            isNotEqualOpValidation = true;
                        }
                        else
                        {
                            isNotEqualOpValidation = false;
                            detail2.ErrorMessage = "The service does not execute an accurate result with system query option $filter ne.";
                        }
                    }
                }
                else
                {
                    passed = false;
                    detail2.ErrorMessage = "Request failed with system query option $filter ne.";
                }
                #endregion

                if (true == isEqualOpValidation && true == isNotEqualOpValidation)
                {
                    passed = true;
                }

                details.Add(detail2);
            }

            details.Insert(0, detail1);
            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);

            return passed;
        }
    }
}
