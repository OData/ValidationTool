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
    /// Class of extension rule for Entry.Core.3322
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore3322 : ExtensionRule
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
                return "Entry.Core.3322";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The service MAY omit the odata.associationLinkUrl annotation if the association link matches this computed value in V3.";
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
            Dictionary<string, string> associationNamesAndLinks = new Dictionary<string, string>();

            // Get all navigation links from entity object.
            foreach (JProperty jProp in entry.Children())
            {
                if (jProp.Name.EndsWith(Constants.OdataAssociationLinkPropertyNameSuffix + "Url"))
                {
                    string associationName = jProp.Name.Substring(0, jProp.Name.LastIndexOf(Constants.OdataAssociationLinkPropertyNameSuffix));
                    associationNamesAndLinks.Add(associationName, jProp.Value.ToString().StripOffDoubleQuotes());
                }
            }

            // If navigation link exists when minimal metadata, its value should not match the default value 
            // which is the read URL appended with a segment containing the name of the navigation property.
            if (associationNamesAndLinks.Count > 0)
            {
                // Whether the last segment of navigation link is the navigation property name.
                foreach (KeyValuePair<string, string> kvp in associationNamesAndLinks)
                {
                    if (!kvp.Value.EndsWith(kvp.Key))
                    {
                        passed = true;                       
                    }
                }

                if (passed == null)
                {
                    // Send a request with "application/json;odata=fullmetadata" in Accept header.
                    string acceptHeader = Constants.V3AcceptHeaderJsonFullMetadata;
                    Response response = WebHelper.Get(context.Destination, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    JObject fullEntry;
                    response.ResponsePayload.TryToJObject(out fullEntry);

                    if (fullEntry != null)
                    {
                        if (fullEntry[Constants.OdataReadLink] != null)
                        {
                            readUrl = fullEntry[Constants.OdataReadLink].Value<string>().StripOffDoubleQuotes();
                        }
                        else if (fullEntry[Constants.OdataEditLink] != null)
                        {
                            readUrl = fullEntry[Constants.OdataEditLink].Value<string>().StripOffDoubleQuotes();
                        }
                        else if (fullEntry[Constants.OdataId] != null)
                        {
                            string id = fullEntry[Constants.OdataId].Value<string>().StripOffDoubleQuotes();

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
                            foreach (KeyValuePair<string, string> kvp in associationNamesAndLinks)
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
