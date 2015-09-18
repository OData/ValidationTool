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
    /// Class of service implemenation feature to query an entity with filter Grouping.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_FilterGrouping : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_FilterGrouping";
            }
        }

        /// <summary>
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                return new ServiceImplCategory(ServiceImplCategoryName.LogicalOperators,new ServiceImplCategory(ServiceImplCategoryName.SystemQueryOption,new ServiceImplCategory(ServiceImplCategoryName.RequestingData)));
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",$filter (Grouping)";
            }
        }

        /// <summary>
        /// Gets the service implementation feature specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the service implementation feature level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }


        /// <summary>
        /// Verifies the service implementation feature.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if the service implementation feature passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;

            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);
            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            detail = info.Details[0];

            List<string> supportedPropertyTypes = new List<string>{                
                PrimitiveDataTypes.Int16, PrimitiveDataTypes.Int32, PrimitiveDataTypes.Int64,
                PrimitiveDataTypes.Decimal, PrimitiveDataTypes.Double
            };

            var filterRestrictions = AnnotationsHelper.GetFilterRestrictionsWithoutNavi(context.MetadataDocument, context.VocCapabilities, supportedPropertyTypes);

            if (string.IsNullOrEmpty(filterRestrictions.Item1) ||
                null == filterRestrictions.Item2 || !filterRestrictions.Item2.Any())
            {
                detail.ErrorMessage = "Cannot find an appropriate entity-set which supports Less Than system query options in the service.";

                return passed;
            }

            string entitySet = filterRestrictions.Item1;
            string primitivePropName = filterRestrictions.Item2.First().PropertyName;

            string url = string.Format("{0}/{1}", context.ServiceBaseUri, entitySet);
            var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (null == resp || HttpStatusCode.OK != resp.StatusCode)
            {
                detail.ErrorMessage = JsonParserHelper.GetErrorMessage(resp.ResponsePayload);

                return passed;
            }

            JObject feed;
            resp.ResponsePayload.TryToJObject(out feed);

            if (feed == null || JTokenType.Object != feed.Type)
            {
                detail.ErrorMessage = "The service does not return a valid response for system query option";
                return passed;
            }

            var entities = JsonParserHelper.GetEntries(feed);
            Int64 propVal = entities[0].Value<Int64>(primitivePropName) - 1;

            string pattern = "{0}/{1}?$filter=(1 add 0) mul {2} sub {3} lt 2";
            url = string.Format(pattern, context.ServiceBaseUri, entitySet, primitivePropName, propVal);
            resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            detail.URI = url;
            detail.HTTPMethod = "GET";
            detail.RequestHeaders = StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders);
            detail.ResponseStatusCode = resp != null && resp.StatusCode.HasValue ? resp.StatusCode.Value.ToString() : "";
            detail.ResponseHeaders = string.IsNullOrEmpty(resp.ResponseHeaders) ? "" : resp.ResponseHeaders;
            detail.ResponsePayload = string.IsNullOrEmpty(resp.ResponsePayload) ? "" : resp.ResponsePayload;


            if (resp.StatusCode != HttpStatusCode.OK)
            {
                passed = false;
                detail.ErrorMessage = "Request failed with system query option $filter '()'.";
                return passed;
            }

            JObject feed2;
            resp.ResponsePayload.TryToJObject(out feed2);

            if (feed2 == null || JTokenType.Object != feed2.Type)
            {
                passed = false;
                detail.ErrorMessage = "The service does not return a valid response for system query option $filter '()'.";
                return passed;
            }

            var entities2 = JsonParserHelper.GetEntries(feed2).ToList();
            var temp = entities2.FindAll(en => en.Value<Int64>(primitivePropName) - propVal < 2).Select(en => en);

            if (entities2.Count() == temp.Count())
            {
                passed = true;
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The service does not execute an accurate result with system query option $filter '()'.";
            }

            return passed;
        }
    }
}
