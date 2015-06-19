// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4329
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4329 : ExtensionRule
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
                return "Entry.Core.4329";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The association link for a navigation property is represented if the association link cannot be computed by appending /$ref to the navigation link.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.2";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
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
                return RuleEngine.PayloadType.Entry;
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

            JObject entry;
            context.ResponsePayload.TryToJObject(out entry);

            if (entry != null && entry.Type == JTokenType.Object && context.OdataMetadataType == ODataMetadataType.MinOnly)
            {
                JsonParserHelper.ClearProperyValsList();
                List<JToken> odataMetadataVals = JsonParserHelper.GetSpecifiedPropertyValsFromEntryPayload(entry, Constants.OdataV4JsonIdentity, MatchType.Equal);

                JsonParserHelper.ClearProperyValsList();
                List<JToken> associationLinkVals = JsonParserHelper.GetSpecifiedPropertyValsFromEntryPayload(entry, Constants.OdataAssociationLinkPropertyNameSuffix, MatchType.Contained);

                string url = string.Empty;

                if (odataMetadataVals.Count == 1)
                {
                    string odataMetadataValStr = odataMetadataVals[0].ToString().Trim('"');
                    int index = odataMetadataValStr.IndexOf(Constants.Metadata);
                    url = odataMetadataValStr.Remove(index, odataMetadataValStr.Length - index);

                    foreach (var associationLinkVal in associationLinkVals)
                    {
                        if (Uri.IsWellFormedUriString(associationLinkVal.ToString().Trim('"'), UriKind.Relative))
                        {
                            url += associationLinkVal.ToString().Trim('"') + @"/$ref";
                        }
                        else
                        {
                            url = associationLinkVal.ToString().Trim('"');
                        }

                        Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);

                        // Send a request with "application/json;odata=minimalmetadata" in Accept header.
                        string acceptHeader = Constants.V4AcceptHeaderJsonMinimalMetadata;
                        Response response = WebHelper.Get(uri, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                        // If response's status code is not equal to 200, it will indicate the url cannot be computed.
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                    }
                }
            }

            return passed;
        }
    }
}
