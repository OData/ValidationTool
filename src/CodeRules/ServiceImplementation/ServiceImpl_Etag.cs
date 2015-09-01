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
    /// Class of service implemenation Etag feature.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_Etag : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_Etag";
            }
        }

        /// <summary>
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                return new ServiceImplCategory(ServiceImplCategoryName.ResponseHeaders);
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",Etag";
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

            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();

            List<string> entitySetURLs = MetadataHelper.GetEntitySetURLs();
            foreach (string entitySetUrl in entitySetURLs)
            {
                string entityTypeShortName;
                try
                {
                    entityTypeShortName = entitySetUrl.MapEntitySetNameToEntityTypeShortName();
                }
                catch (ArgumentException)
                { continue; }

                Tuple<string,string> key = MetadataHelper.GetKeyProperty(entityTypeShortName);
                if (key == null)
                { continue; }

                Response resp = WebHelper.Get(new Uri(context.ServiceBaseUri + "/" + entitySetUrl), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                
                if (null == resp || HttpStatusCode.OK != resp.StatusCode)
                {
                    continue;
                }

                JObject feed;
                resp.ResponsePayload.TryToJObject(out feed);

                if (feed == null || JTokenType.Object != feed.Type)
                {
                    continue;
                }

                JArray entities = JsonParserHelper.GetEntries(feed);
                if (entities.Count == 0)
                { continue; }
                string identity = entities[0][key.Item1].ToString();

                string url = context.ServiceBaseUri + "/" + entitySetUrl + "(" + (key.Item2.Equals("Edm.String") ? "\'" + identity + "\'" : identity) + ")";

                resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                details.Add(new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, ""));

                if (resp.ResponseHeaders.Contains("ETag:"))
                {
                    passed = true;
                    break;
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);
            return passed;
        }
    }
}
