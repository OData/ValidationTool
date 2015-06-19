// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4132_Feed : CommonCore4132
    {
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
    }

    /// <summary>
    /// Class of code rule applying to EntityRef payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4132_EntityRef : CommonCore4132
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.EntityRef;
            }
        }
    }

    /// <summary>
    /// Class of extension rule for Common.Core.4132
    /// </summary>
    public abstract class CommonCore4132 : ExtensionRule
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
                return "Common.Core.4132";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The odata.count annotation contains the count of a collection of entities or a collection of entity references.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.5.4";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V3_V4;
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
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.JsonLight;
            }
        }

        /// <summary>
        /// Gets the RequireMetadata property to which the rule applies.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule applies to offline context
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
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

            bool? passed = null;
            info = null;

            int totalCountOfEntities = 0;
            int odataCount = 0;

            string feedUrl = context.DestinationBasePath + @"?$count=true";

            if (context.Version == ODataVersion.V3)
            {
                feedUrl = context.DestinationBasePath + @"?$inlinecount=allpages";
            }

            Response response = WebHelper.Get(new Uri(feedUrl), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (response.StatusCode.HasValue && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject jo;
                response.ResponsePayload.TryToJObject(out jo);

                GetEntitiesCountFromFeed(new Uri(feedUrl), jo, context.Version, context.RequestHeaders, ref totalCountOfEntities, out odataCount);

                if (totalCountOfEntities == odataCount)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                }
            }

            return passed;
        }

        /// <summary>
        /// Get total count of entities in feed response.
        /// </summary>
        /// <param name="url">The input url.</param>
        /// <param name="feed">The json object of feed response.</param>
        /// <param name="version">The version of the service.</param>
        /// <param name="RequestHeaders">The request headers.</param>
        /// <param name="totalCount">The amount of the entities.</param>
        /// <param name="odataCount">The odata.count value.</param>        
        private void GetEntitiesCountFromFeed(Uri url, JObject feed, ODataVersion version, IEnumerable<KeyValuePair<string, string>> RequestHeaders, ref int totalCount, out int odataCount)
        {
            int skiptoken = 0;
            odataCount = 0;
            string OdataNextLinkName = version.Equals(ODataVersion.V4) ? Constants.V4OdataNextLink : Constants.OdataNextLink;

            foreach (var r in feed.Children<JProperty>())
            {
                if (r.Name.Equals(Constants.Value, StringComparison.Ordinal) && r.Value.Type == JTokenType.Array)
                {
                    totalCount += ((JArray)r.Value).Count;
                }

                if (r.Name.Equals(version == ODataVersion.V4 ? Constants.V4OdataCount : Constants.OdataCount, StringComparison.Ordinal))
                {
                    odataCount = Int32.Parse(r.Value.ToString().StripOffDoubleQuotes());
                }

                // When entities are more than one page.
                if (r.Name.Equals(OdataNextLinkName, StringComparison.Ordinal))
                {
                    string[] skiptokenValues = r.Value.ToString().StripOffDoubleQuotes().Split(new string[] { "skiptoken=" }, StringSplitOptions.None);
                    skiptoken = Int32.Parse(skiptokenValues[1]);
                    string nextLinkUrl = url + @"&$skiptoken=" + skiptoken.ToString();
                    Response response = WebHelper.Get(new Uri(nextLinkUrl), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, RequestHeaders);

                    JObject jo;
                    int tempCount = 0;
                    response.ResponsePayload.TryToJObject(out jo);

                    GetEntitiesCountFromFeed(url, jo, version, RequestHeaders, ref totalCount, out tempCount);
                }
            }
        }
    }
}
