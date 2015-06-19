// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Globalization;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Extension rule to verify Json ve.0 feed:
    /// If the JSON array represents a partial collection of entities, a nextLinkNVP name value pair MUST be included 
    /// in the JSON array to indicate it represents a partial collection.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore2000 : ExtensionRule
    {
        /// <summary>
        /// Gets Category property
        /// </summary>
        public override string Category
        {
            get
            {
                return "core";
            }
        }

        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Feed.Core.2000";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "An empty EntitySet or collection of entities (one that contains no EntityType instances) MUST be represented as an empty JSON array.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.3.2";
            }
        }

        /// <summary>
        /// Gets location of help information of the rule
        /// </summary>
        public override string HelpLink
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the error message for validation failure
        /// </summary>
        public override string ErrorMessage
        {
            get
            {
                return this.Description;
            }
        }

        /// <summary>
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Feed;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Json;
            }
        }

        /// <summary>
        /// Verifies the extension rule.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            info = null;
            bool? passed = null;

            Uri uriEmptyFeed = FeedCore2000.ConstructProbeUri(context);
            var resp = WebHelper.Get(uriEmptyFeed, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            JObject response = JsonParserHelper.GetResponseObject(resp.ResponsePayload);
            if (response != null)
            {
                JArray entries = JsonParserHelper.GetEntries(response);
                if (entries.Count == 0)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(Resource.NotEmptyFeed, uriEmptyFeed, resp.ResponsePayload);
                }
            }

            return passed;
        }

        /// <summary>
        /// Builds the uri for purpose of probing the empty feed based on the base uri string.
        /// </summary>
        /// <param name="context">The service context object</param>
        /// <returns>Uri object if a meaningful prober can be constructed; null otherwise </returns>
        private static Uri ConstructProbeUri(ServiceContext context)
        {
            int safetyTopping = 20;
            Uri result = null;

            // find out how many entries in the base feed
            Uri uriFeedCount = new Uri(context.DestinationBasePath + "/$count");
            var resp = WebHelper.Get(uriFeedCount, Constants.AcceptHeaderAtom, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            if (resp.StatusCode.HasValue && resp.StatusCode.Value == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(resp.ResponsePayload))
            {
                string payload = resp.ResponsePayload.Trim();
                int count = 0;
                if (Int32.TryParse(payload, out count) && count >= 0)
                {
                    int countToSkip = count + safetyTopping;
                    Uri uriBaseFeed = new Uri(context.DestinationBasePath);
                    ResourcePathHelper pathHelper = new ResourcePathHelper(uriBaseFeed);
                    pathHelper.AddQueryOption("$skip", countToSkip.ToString(CultureInfo.InvariantCulture));

                    result = new Uri(pathHelper.Product);
                }
            }

            return result;
        }
    }
}
