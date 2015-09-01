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
    /// Class of service implemenation cross join feature.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_CrossJoin : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_CrossJoin";
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
                return this.CategoryInfo.CategoryFullName + ",Cross Join";
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

            List<string> entitySetURLs = MetadataHelper.GetEntitySetURLs();

            Response resp = WebHelper.Get(new Uri(context.ServiceBaseUri + "/$crossjoin(" + entitySetURLs.First<string>() + "," + entitySetURLs.Last<string>() + ")"), 
                Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name, 
                context.ServiceBaseUri + "/$crossjoin(" + entitySetURLs.First<string>() + "," + entitySetURLs.Last<string>() + ")",
                HttpMethod.Get, context.RequestHeaders.ToString());

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

            if (null == resp || HttpStatusCode.OK != resp.StatusCode)
            {
                return false;
            }

            JObject feed;
            resp.ResponsePayload.TryToJObject(out feed);

            if (feed == null || JTokenType.Object != feed.Type)
            {
                return false;
            }

            JArray entities = JsonParserHelper.GetEntries(feed);

            return true;
        }
    }
}
