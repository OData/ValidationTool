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
    /// Class of service implemenation feature to request all data.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_All : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_All";
            }
        }

        /// <summary>
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                return new ServiceImplCategory(ServiceImplCategoryName.RequestingData);
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",All";
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

            Response resp = WebHelper.Get(new Uri(context.ServiceBaseUri + "/$all"), Constants.AcceptHeaderJson, 
                RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name, context.ServiceBaseUri + "/$all",
                HttpMethod.Get, context.RequestHeaders.ToString());
            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

            if (null == resp || HttpStatusCode.OK != resp.StatusCode)
            {
                return passed = false;
            }

            JObject allFeed;
            resp.ResponsePayload.TryToJObject(out allFeed);

            if (allFeed == null || JTokenType.Object != allFeed.Type)
            {
                
                return passed = false;
            }

            JArray allEntities = JsonParserHelper.GetEntries(allFeed);

            passed = true;

            List<string> entitySetURLs = MetadataHelper.GetEntitySetURLs();
            foreach (string entitySetUrl in entitySetURLs)
            {
                string entityTypeShortName = entitySetUrl.MapEntitySetNameToEntityTypeShortName();
                Tuple<string,string> key = MetadataHelper.GetKeyProperty(entityTypeShortName);

                resp = WebHelper.Get(new Uri(context.ServiceBaseUri + "/" + entitySetUrl), 
                    Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                
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
                foreach (JToken entity in entities)
                {
                    if (!Find(allEntities, key.Item1, entity[key].ToString()))
                        return passed = false;
                }

            }

            return passed;
        }

        private bool Find(JArray array,string key,string value)
        {
            foreach (JToken element in array)
            {
                if (element[key] != null && element[key].ToString().Equals(value))
                    return true;
            }
            return false;
        }
    }
}
