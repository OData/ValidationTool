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
    using ODataValidator.Rule.Helper.Geo;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of service implemenation feature to verify .
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_SystemQueryOptionFilter_GeoLength : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_SystemQueryOptionFilter_GeoLength";
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",$filter(geo.length)";
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
            var propTypes = new string[1] { "Edm.GeographyLineString" };
            var propNames = MetadataHelper.GetPropertyNames(propTypes, out entityTypeShortName);
            if (null == propNames || !propNames.Any())
            {
                return passed;
            }

            string propName = propNames[0];
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
                var length = this.GetLength(propName, entity);
                url = string.Format("{0}?$filter=geo.length({1}) eq {2}", url, propName, length);
                resp = WebHelper.Get(new Uri(url), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, svcStatus.DefaultHeaders);
                var detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, string.Empty);
                info = new ExtensionRuleViolationInfo(new Uri(url), string.Empty, detail);
                if (null != resp && HttpStatusCode.OK == resp.StatusCode)
                {
                    jObj = JObject.Parse(resp.ResponsePayload);
                    jArr = jObj.GetValue(Constants.Value) as JArray;
                    foreach (JObject et in jArr)
                    {
                        var len = this.GetLength(propName, et);
                        passed = len == length;
                    }
                }
                else
                {
                    passed = false;
                }
            }

            return passed;
        }

        /// <summary>
        /// Get the length of the line.
        /// </summary>
        /// <param name="propName">The property name indicates the line.</param>
        /// <param name="entity">An entity in the service.</param>
        /// <returns>Return the length of the line.</returns>
        private double GetLength(string propName, JObject entity)
        {
            var propVal = entity[propName]["coordinates"] as JArray;
            var pts = new List<Point>();
            for (int i = 0; i < propVal.Count - 1; i += 2)
            {
                var pt = new Point(Convert.ToDouble(propVal[i]), Convert.ToDouble(propVal[i + 1]));
                pts.Add(pt);
            }

            var segs = new List<SegmentEquation>();
            for (int i = 0; i < pts.Count - 1; i++)
            {
                var seg = new SegmentEquation(pts[i], pts[i + 1]);
                segs.Add(seg);
            }

            double len = 0.0;
            foreach (var seg in segs)
            {
                len += seg.Length;
            }

            return len;
        }
    }
}
