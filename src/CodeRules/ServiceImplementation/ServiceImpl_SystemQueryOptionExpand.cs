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
    public class ServiceImpl_SystemQueryOptionExpand : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_SystemQueryOptionExpand";
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",$expand";
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
            var navigPropNames = MetadataHelper.GetNavigationPropertyNames(
                out entityTypeShortName, 
                null, 
                (elem) => {
                    return !(null != elem.Attribute("Nullable") && elem.GetAttributeValue("Nullable") == "false");
                });
            if (null == navigPropNames || !navigPropNames.Any())
            {
                return passed;
            }

            string navigPropName = navigPropNames[0];
            navigPropNames.RemoveAt(0);
            var entitySetUrl = entityTypeShortName.MapEntityTypeShortNameToEntitySetURL();
            string url = string.Format("{0}/{1}?$expand={2}", svcStatus.RootURL.TrimEnd('/'), entitySetUrl, navigPropName);
            var resp = WebHelper.Get(new Uri(url), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, svcStatus.DefaultHeaders);
            var detail = new ExtensionRuleResultDetail("ServiceImpl_SystemQueryOptionExpand", url, HttpMethod.Get, string.Empty);
            info = new ExtensionRuleViolationInfo(new Uri(url), string.Empty, detail);
            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                var jObj = JObject.Parse(resp.ResponsePayload);
                var jArr = jObj.GetValue("value") as JArray;
                foreach (JObject entity in jArr)
                {
                    passed = false;
                    foreach (JProperty prop in entity.Children())
                    {
                        if (prop.Name == navigPropName)
                        {
                            passed = true;
                            break;
                        }
                    }

                    if (passed == true)
                    {
                        break;
                    }
                }
            }

            return passed;
        }
    }
}
