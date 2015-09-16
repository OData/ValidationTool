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
    using ODataValidator.Rule.Helper.Geo;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of service implemenation feature to verify .
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_SystemQueryOptionFilter_GeoDistance : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_SystemQueryOptionFilter_GeoDistance";
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",$filter(geo.distance)";
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
                parent = new ServiceImplCategory(ServiceImplCategoryName.SystemQueryOption, parent);

                return new ServiceImplCategory(ServiceImplCategoryName.ArithmeticOperators, parent);
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
            var props = MetadataHelper.GetProperties("Edm.GeographyPoint", 1, out entityTypeShortName);
            if (null == props || !props.Any())
            {
                return passed;
            }

            string propName = props[0].PropertyName;
            string srid = props[0].SRID;
            var entitySetUrl = entityTypeShortName.GetAccessEntitySetURL();
            if (string.IsNullOrEmpty(entitySetUrl))
            {
                return passed;
            }

            string url = svcStatus.RootURL.TrimEnd('/') + "/" + entitySetUrl;
            var resp = WebHelper.Get(new Uri(url), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, svcStatus.DefaultHeaders);
            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                JObject jObj = JObject.Parse(resp.ResponsePayload);
                JArray jArr = jObj.GetValue(Constants.Value) as JArray;
                var entity = jArr.First as JObject;
                var propVal = entity[propName]["coordinates"] as JArray;
                var pt1 = new Point(Convert.ToDouble(propVal[0]), Convert.ToDouble(propVal[1]));
                var pt2 = new Point(0.0, 0.0);
                var distance = Point.GetDistance(pt1, pt2);
                url = string.Format("{0}?$filter=geo.distance({1}, geography'POINT(0.0 0.0)') ge {2}", url, propName, distance);
                resp = WebHelper.Get(new Uri(url), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, svcStatus.DefaultHeaders);
                var detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, string.Empty);
                info = new ExtensionRuleViolationInfo(new Uri(url), string.Empty, detail);
                if (null != resp && HttpStatusCode.OK == resp.StatusCode)
                {
                    jObj = JObject.Parse(resp.ResponsePayload);
                    jArr = jObj.GetValue(Constants.Value) as JArray;
                    foreach (JObject et in jArr)
                    {
                        propVal = et[propName]["coordinates"] as JArray;
                        pt1 = new Point(Convert.ToDouble(propVal[0]), Convert.ToDouble(propVal[1]));
                        pt2 = new Point(0.0, 0.0);
                        var dis = Point.GetDistance(pt1, pt2);
                        passed = dis >= distance;
                    }
                }
                else
                {
                    passed = false;
                }
            }

            return passed;
        }
    }
}
