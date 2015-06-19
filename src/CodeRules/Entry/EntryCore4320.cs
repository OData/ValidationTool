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
    /// Class of extension rule for Entry.Core.4320
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4320 : ExtensionRule
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
                return "Entry.Core.4320";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The service MAY omit the odata.navigationLink annotation if odata.metadata=minimal has been specified on the request and the navigation link matches this computed value in V4";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.5.10";
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
                return RequirementLevel.May;
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
                return RuleEngine.ODataMetadataType.MinOnly;
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

            string readUrl = string.Empty;
            Dictionary<string, string> navigationNamesAndLinks = new Dictionary<string, string>();

            // Get all navigation links from entity object.
            foreach (JProperty jProp in entry.Children())
            {
                if (jProp.Name.EndsWith(Constants.OdataNavigationLinkPropertyNameSuffix))
                {
                    string navigationName = jProp.Name.Substring(0, jProp.Name.LastIndexOf(Constants.OdataNavigationLinkPropertyNameSuffix));
                    navigationNamesAndLinks.Add(navigationName, jProp.Value.ToString().StripOffDoubleQuotes());
                }
            }

            // If navigation link exists when minimal metadata, its value should not match the default value 
            // which is the read URL appended with a segment containing the name of the navigation property.
            if (navigationNamesAndLinks.Count > 0)
            {
                // Whether the last segment of navigation link is the navigation property name.
                foreach (KeyValuePair<string, string> kvp in navigationNamesAndLinks)
                {
                    if (!kvp.Value.EndsWith(kvp.Key))
                    {
                        passed = true;
                    }
                }

                if (passed == null)
                {
                    // Send a request with "application/json;odata.metadata=full" in Accept header.
                    string acceptHeader = Constants.V4AcceptHeaderJsonFullMetadata;
                    Response response = WebHelper.Get(context.Destination, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    JObject fullEntry;
                    response.ResponsePayload.TryToJObject(out fullEntry);

                    if (fullEntry != null)
                    {
                        if (fullEntry[Constants.V4OdataReadLink] != null)
                        {
                            readUrl = fullEntry[Constants.V4OdataReadLink].Value<string>().StripOffDoubleQuotes();
                        }
                        else if (fullEntry[Constants.V4OdataEditLink] != null)
                        {
                            readUrl = fullEntry[Constants.V4OdataEditLink].Value<string>().StripOffDoubleQuotes();
                        }
                        else if (fullEntry[Constants.V4OdataId] != null)
                        {
                            string id = fullEntry[Constants.V4OdataId].Value<string>().StripOffDoubleQuotes();

                            if (id.LastIndexOf(@"/") > 0)
                            {
                                // if odata.id is absolute path, get the relatived path.
                                readUrl = id.Substring(id.LastIndexOf(@"/") + 1);
                            }
                            else
                            {
                                readUrl = id;
                            }
                        }

                        if (!String.IsNullOrEmpty(readUrl))
                        {
                            foreach (KeyValuePair<string, string> kvp in navigationNamesAndLinks)
                            {
                                if (Uri.IsWellFormedUriString(readUrl, UriKind.Relative))
                                {
                                    passed = !kvp.Value.StartsWith(readUrl);
                                }
                                else
                                {
                                    passed = !(readUrl + @"/" + kvp.Key).Contains(kvp.Value);
                                }

                                if (passed == false)
                                {
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, string.Empty);
                                    break;
                                }                               
                            }                           
                        }
                    }
                }
            }

            return passed;
        }
    }
}
