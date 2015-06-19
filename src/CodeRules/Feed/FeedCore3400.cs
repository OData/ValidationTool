// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Feed.Core.3400
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore3400 : ExtensionRule
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
                return "Feed.Core.3400";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The odata.metadata annotation MUST also be included for entities whose entity set cannot be determined from the metadata URL of the collection in V3.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.5.1";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V3;
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
                return null;
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
            string entitiesName = string.Empty;

            JObject feed;
            context.ResponsePayload.TryToJObject(out feed);
            var entries = JsonParserHelper.GetEntries(feed);

            var o = (JObject)feed;
            var odatametadata = (JProperty)o.First;

            if (odatametadata.Name.Equals(Constants.OdataV3JsonIdentity))
            {
                // Get entities name.
                string odatametadataValue = odatametadata.Value.ToString().StripOffDoubleQuotes().Split(new string[] { Constants.JsonFeedIdentity }, StringSplitOptions.RemoveEmptyEntries)[1];

                foreach (JObject entry in entries)
                {
                    if (entry != null && entry.Type == JTokenType.Object)
                    {
                        var jProps = entry.Children();

                        foreach (JProperty jProp in jProps)
                        {
                            if (jProp.Name.Equals(Constants.OdataV3JsonIdentity))
                            {
                                string entitysetName = string.Empty;
                                string splitPart = jProp.Value.ToString().StripOffDoubleQuotes().Split(new string[] { Constants.JsonFeedIdentity }, StringSplitOptions.RemoveEmptyEntries)[1];

                                // Get the entity set name.
                                if (splitPart.EndsWith(Constants.V3JsonEntityIdentity))
                                {
                                    entitysetName = splitPart.Remove(splitPart.IndexOf("/"));
                                }

                                if (!odatametadataValue.Equals(entitysetName))
                                {
                                    passed = true;
                                }
                                else
                                {
                                    passed = false;
                                    break;
                                }
                            }
                        }

                        if (passed != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (passed == false)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }
    }
}
