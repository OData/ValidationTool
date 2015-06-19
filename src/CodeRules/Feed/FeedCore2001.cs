// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Globalization;
    using System.Linq;
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
    public class FeedCore2001 : ExtensionRule
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
                return "Feed.Core.2001";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the JSON array represents a partial collection of entities, a nextLinkNVP name value pair MUST be included in the JSON array to indicate it represents a partial collection.";
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
        /// Gets descriptive summary of the rule
        /// </summary>
        public override string Aspect
        {
            get
            {
                return "semantic";
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
        /// Gets the OData version of the response to which the rule applies. 
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V2;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
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

            if (FeedCore2001.IsNextLinkPresent(context))
            {
                passed = true;
            }
            else
            {
                passed = !FeedCore2001.IsPartialColleclection(context);
                if (!passed.Value)
                {
                    info = new ExtensionRuleViolationInfo(Resource.PayloadExpectingNextLink, context.Destination, context.ResponsePayload);
                }
            }

            return passed;
        }

        /// <summary>
        /// Checks whether the context payload is a partial collection of entrities
        /// </summary>
        /// <param name="context">The service context</param>
        /// <returns>True if it is partial collection; false otherwise</returns>
        private static bool IsPartialColleclection(ServiceContext context)
        {
            bool anyNewEntry = false;

            JObject contextResponse = JsonParserHelper.GetResponseObject(context);
            if (contextResponse != null)
            {
                JArray contextEntries = JsonParserHelper.GetEntries(contextResponse);
                if (contextEntries.Count > 0)
                {
                    // if any more entries return, the context response payload was a partial collection
                    Uri prober = FeedCore2001.ConstructProbeUri(context);
                    var resp = WebHelper.Get(prober, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    if (resp == null || string.IsNullOrEmpty(resp.ResponsePayload) || !JsonHelper.IsJsonVerboseFeed(resp.ResponsePayload))
                    {
                        return false;
                    }

                    JObject response = JsonParserHelper.GetResponseObject(resp.ResponsePayload);
                    if (response != null)
                    {
                        JArray entries = JsonParserHelper.GetEntries(response);

                        if (entries.Count > 0)
                        {
                            // some producers do not respect $skipton;
                            // need to look at each entry to see whether there is any new entry.
                            HashSet<string> contextEntryKeys = new HashSet<string>(EqualityComparer<string>.Default);
                            foreach (var e in contextEntries)
                            {
                                if (e.Type == JTokenType.Object)
                                {
                                    var i = JsonParserHelper.GetTokenOfEntry((JObject)e);
                                    contextEntryKeys.Add(i);
                                }
                            }

                            foreach (var e in entries)
                            {
                                if (e.Type == JTokenType.Object)
                                {
                                    var i = JsonParserHelper.GetTokenOfEntry((JObject)e);
                                    if (!contextEntryKeys.Contains(i))
                                    {
                                        anyNewEntry = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return anyNewEntry;
        }

        /// <summary>
        /// Builds the uri for purpose of probing any remaining entries satisfying the request of service context
        /// use the last entry's token in the feed as the marker of skiptoken;
        /// and reduce the $top count if top was in context request URI.
        /// </summary>
        /// <param name="context">The service context object</param>
        /// <returns>Uri object if a meaningful prober can be constructed; null otherwise </returns>
        private static Uri ConstructProbeUri(ServiceContext context)
        {
            Uri result = null;

            JObject response = JsonParserHelper.GetResponseObject(context);
            if (response != null)
            {
                JArray entries = JsonParserHelper.GetEntries(response);
                if (entries != null)
                {
                    JObject lastEntry = JsonParserHelper.GetLastEntry(entries);
                    if (lastEntry != null)
                    {
                        string lastToken = JsonParserHelper.GetTokenOfEntry(lastEntry);
                        string lastTokenOfValues = ResourcePathHelper.GetValuesOfKey(lastToken);

                        var uri = context.Destination;
                        ResourcePathHelper pathHelper = new ResourcePathHelper(uri);

                        // replace top value with the reduced value, if $top was in context request
                        string topValue = pathHelper.GetQueryValue("$top");
                        if (!string.IsNullOrEmpty(topValue))
                        {
                            pathHelper.RemoveQueryOption("$top");

                            int entriesGot = entries.Count;
                            int entriesToGet;
                            if (Int32.TryParse(topValue, out entriesToGet) && entriesToGet > entriesGot)
                            {
                                int entriesLeft = entriesToGet - entriesGot;
                                pathHelper.AddQueryOption("$top", entriesLeft.ToString(CultureInfo.InvariantCulture));
                            }
                        }

                        // set up new skiptoken query
                        pathHelper.RemoveQueryOption("$skiptoken");
                        pathHelper.AddQueryOption("$skiptoken", lastTokenOfValues);

                        result = new Uri(pathHelper.Product);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether __next name-value pair is included in the (json) response payload
        /// </summary>
        /// <param name="context">The service context object</param>
        /// <returns>True if it is found in the response; false otherwise</returns>
        private static bool IsNextLinkPresent(ServiceContext context)
        {
            bool hasNextLink = false;

            JObject feedV2 = JsonParserHelper.GetResponseObject(context);
            if (feedV2 != null && feedV2.Count == 1)
            {
                var d = (JProperty)feedV2.First;
                if (d.Name.Equals("d", StringComparison.Ordinal))
                {
                    var sub = d.Value;
                    if (sub.Type == JTokenType.Object)
                    {
                        // looking for property of name-value pair of "__next:..."
                        JObject resultObject = (JObject)sub;

                        var nextLinks = from p in resultObject.Children<JProperty>()
                                        where p.Name.Equals("__next", StringComparison.Ordinal)
                                        select p;

                        hasNextLink = nextLinks.Any();
                    }
                }
            }

            return hasNextLink;
        }
    }
}
