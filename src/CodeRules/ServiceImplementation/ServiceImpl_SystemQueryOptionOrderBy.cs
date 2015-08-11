// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
    using System;
    using System.ComponentModel.Composition;
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
    public class ServiceImpl_SystemQueryOptionOrderBy : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_SystemQueryOptionOrderBy";
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",$orderby";
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
            string entityTypeShortName;
            var keyProp = MetadataHelper.GetKeyProperty(out entityTypeShortName);
            if (null == keyProp)
            {
                return passed;
            }

            string keyPropName = keyProp.Item1;
            string keyPropType = keyProp.Item2;
            if (string.IsNullOrEmpty(entityTypeShortName) || string.IsNullOrEmpty(keyPropName) || string.IsNullOrEmpty(keyPropType))
            {
                return passed;
            }

            string entitySetUrl = entityTypeShortName.MapEntityTypeShortNameToEntitySetURL();
            string url = string.Format("{0}/{1}?$orderby={2} desc", svcStatus.RootURL.TrimEnd('/'), entitySetUrl, keyPropName);
            var resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            var detail = new ExtensionRuleResultDetail("ServiceImpl_SystemQueryOptionOrderBy", url, HttpMethod.Get, string.Empty);
            info = new ExtensionRuleViolationInfo(new Uri(url), string.Empty, detail);
            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                passed = true;
            }

            var jObj = JObject.Parse(resp.ResponsePayload);
            var jArr = jObj.GetValue("value") as JArray;
            for (int i = 0; i < jArr.Count - 1; i++)
            {
                if (!CompareOperationHelper.Compare(jArr[i][keyPropName], jArr[i + 1][keyPropName], keyPropType, ComparerType.Equal | ComparerType.GreaterThan))
                {
                    passed = false;
                    break;
                }
            }

            return passed;
        }
    }
}
