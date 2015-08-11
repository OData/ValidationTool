// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of service implemenation feature to verify .
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_SystemQueryOptionSkipToken : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_SystemQueryOptionSkipToken";
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",$skiptoken";
            }
        }

        /// <summary>
        /// Gets the service implementation feature specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "";
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
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                var parent = new ServiceImplCategory(ServiceImplCategoryName.RequestingData);

                return new ServiceImplCategory(ServiceImplCategoryName.SystemQueryOption, parent);
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
            info = null;
            var svcStatus = ServiceStatus.GetInstance();
            var pagingLimitEntitySetUrls = MetadataHelper.GetPagingLimitEntitySetURLs();
            if (null == pagingLimitEntitySetUrls || !pagingLimitEntitySetUrls.Any())
            {
                return passed;
            }

            var pagingLimitEntitySetUrl = pagingLimitEntitySetUrls.First();
            string url = svcStatus.RootURL.TrimEnd('/') + "/" + pagingLimitEntitySetUrl;
            int actualNum = JsonParserHelper.GetEntitiesCountFromFeed(url);
            var resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            var detail = new ExtensionRuleResultDetail("ServiceImpl_SystemQueryOptionSkipToken", url, HttpMethod.Get, string.Empty);
            info = new ExtensionRuleViolationInfo(new Uri(url), string.Empty, detail);
            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                var jObj = JObject.Parse(resp.ResponsePayload);
                var jArr = jObj.GetValue("value") as JArray;
                passed = jArr.Count < actualNum;
            }
            else
            {
                passed = false;
            }

            return passed;
        }
    }
}
