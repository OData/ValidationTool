// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
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
    /// Class of service implemenation feature to verify .
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_SystemQueryOptionRef : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_SystemQueryOptionRef";
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",$ref";
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
            string navigPropName = string.Empty;
            string entityTypeShortName;
            var entityTypeShortNames = new List<string>();
            Tuple<string, string> keyProp = null;
            do
            {
                var navigPropNames = MetadataHelper.GetNavigationPropertyNames(out entityTypeShortName, entityTypeShortNames);
                if (null == navigPropNames || !navigPropNames.Any())
                {
                    return passed;
                }

                navigPropName = navigPropNames[0];
                entityTypeShortNames.Add(entityTypeShortName);
                keyProp = MetadataHelper.GetKeyProperty(entityTypeShortName);
            }
            while (null == keyProp);

            string keyPropName = keyProp.Item1;
            string keyPropType = keyProp.Item2;
            var entitySetUrl = entityTypeShortName.GetAccessEntitySetURL();
            if (string.IsNullOrEmpty(entitySetUrl))
            {
                return passed;
            }

            string url = svcStatus.RootURL.TrimEnd('/') + "/" + entitySetUrl;
            var resp = WebHelper.Get(new Uri(url), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, svcStatus.DefaultHeaders);
            if (null == resp || HttpStatusCode.OK != resp.StatusCode)
            {
                return passed;
            }

            var entities = JsonParserHelper.GetEntities(resp.ResponsePayload);
            if (!entities.Any())
            {
                return passed;
            }

            var entity = entities.First();
            string keyPropVal = entity[keyPropName].ToString();
            string pattern = "Edm.String" == keyPropType ? "{0}('{1}')/{2}/$ref" : "{0}({1})/{2}/$ref";
            url = string.Format(pattern, url, keyPropVal, navigPropName);
            resp = WebHelper.Get(new Uri(url), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, svcStatus.DefaultHeaders);
            var detail = new ExtensionRuleResultDetail("ServiceImpl_SystemQueryOptionRef", url, HttpMethod.Get, string.Empty);
            info = new ExtensionRuleViolationInfo(new Uri(url), string.Empty, detail);
            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                entities = JsonParserHelper.GetEntities(resp.ResponsePayload);
                if (!entities.Any())
                {
                    return false;
                }

                entity = entities.First();
                var odataId = entity[Constants.V4OdataId].ToString();
                resp = WebHelper.Get(new Uri(url), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, svcStatus.DefaultHeaders);
                passed = null != resp && HttpStatusCode.OK == resp.StatusCode;
            }
            else
            {
                passed = false;
            }

            return passed;
        }
    }
}
