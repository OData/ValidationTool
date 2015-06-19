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
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4100
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4100 : ExtensionRule
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
                return "Entry.Core.4100";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The value of association link is an absolute or relative URL that can be used to retrieve the reference or collection of references to the related entity or entities.";
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
        /// Gets the OData metadata type to which the rule applies.
        /// </summary>
        public override ODataMetadataType? OdataMetadataType
        {
            get
            {
                return RuleEngine.ODataMetadataType.FullOnly;
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

            string odataIdName = Constants.V4OdataId;
            if (context.Version == ODataVersion.V3)
            {
                odataIdName = Constants.OdataId;
            }

            string readUrl = string.Empty;
            List<string> associationLinks = new List<string>();

            // Get all association links from entity object.
            foreach (JProperty jProp in entry.Children())
            {
                if (jProp.Name.EndsWith(Constants.OdataAssociationLinkPropertyNameSuffix) || jProp.Name.EndsWith(Constants.OdataAssociationLinkPropertyNameSuffix + "Url"))
                {
                    associationLinks.Add(jProp.Value.ToString().StripOffDoubleQuotes());
                }
            }

            if (associationLinks.Count > 0 && entry[odataIdName] != null)
            {
                string odataId = entry[odataIdName].Value<string>().StripOffDoubleQuotes();

                // If odata.id is a relative url, get the absolute url through request url.
                if (Uri.IsWellFormedUriString(odataId, UriKind.Relative))
                {
                    if (context.Destination.OriginalString.Contains("?"))
                    {
                        odataId = context.Destination.OriginalString.Remove(context.Destination.OriginalString.IndexOf("?"));
                    }
                    else if (context.Destination.OriginalString.Contains("$"))
                    {
                        odataId = context.Destination.OriginalString.Remove(context.Destination.OriginalString.IndexOf("$"));
                    }
                    else
                    {
                        odataId = context.Destination.OriginalString;
                    }
                }

                string associationUrl = string.Empty;

                foreach (string link in associationLinks)
                {
                    if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
                    {
                        associationUrl = link;
                    }
                    else if (Uri.IsWellFormedUriString(link, UriKind.Relative))
                    {
                        associationUrl = odataId.Remove(odataId.LastIndexOf(@"/") + 1) + link;
                    }

                    if (associationUrl.Contains(@"/$links"))
                    {
                        associationUrl = associationUrl.Replace(@"/$links", "");
                    }
                    else if (associationUrl.Contains(@"/$ref"))
                    {
                        associationUrl = associationUrl.Replace(@"/$ref", "");
                    }

                    // Send the association url of request
                    Response response = WebHelper.Get(new Uri(associationUrl), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var payloadFormat = response.ResponsePayload.GetFormatFromPayload();
                        var payloadType = ContextHelper.GetPayloadType(response.ResponsePayload, payloadFormat, response.ResponseHeaders);

                        if (payloadType == RuleEngine.PayloadType.Entry || payloadType == RuleEngine.PayloadType.Feed)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, string.Empty);
                            break;
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, string.Empty);
                        break;
                    }
                }
            }

            return passed;
        }
    }
}
