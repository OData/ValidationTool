// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion   

    /// <summary>
    /// Class of extension rule for Common.Core.4006
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4006 : ExtensionRule
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
                return "Common.Core.4006";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "URLs present in a payload (whether request or response) MAY be represented as relative URLs.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.3";
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
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return null;
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

            JObject allobject;
            context.ResponsePayload.TryToJObject(out allobject);

            passed = this.IsUrlsRelative((JObject)allobject);

            if (passed == false)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }

        /// <summary>
        /// Whether the property is OData V4 link property.
        /// </summary>
        /// <param name="propName">The name of property.</param>
        /// <returns>true: the property value is a link property; false:otherwise.</returns>
        private bool IsOdataV4LinkProp(string propName)
        {
            if (propName.Equals(Constants.OdataV4JsonIdentity) || propName.Equals(Constants.V4OdataId) || propName.Equals(Constants.OdataNextLink) || propName.Equals(Constants.V4OdataDeltaLink)
                    || propName.Equals(Constants.V4OdataEditLink) || propName.Equals(Constants.OdataReadLink) || propName.Equals(Constants.V4OdataMediaEditLink)
                    || propName.Equals(Constants.V4OdataMediaReadLink) || propName.EndsWith(Constants.OdataNavigationLinkPropertyNameSuffix) || propName.EndsWith(Constants.OdataAssociationLinkPropertyNameSuffix))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Whether the property is OData V3 link property.
        /// </summary>
        /// <param name="propName">The name of property.</param>
        /// <returns>true: the property value is a link property; false:otherwise.</returns>
        private bool IsOdataV3LinkProp(string propName)
        {
            if (propName.Equals(Constants.OdataV3JsonIdentity) || propName.Equals(Constants.OdataId) || propName.Equals(Constants.OdataNextLink) || propName.Equals(Constants.OdataDeltaLink)
                    || propName.Equals(Constants.OdataEditLink) || propName.Equals(Constants.OdataReadLink) || propName.Equals(Constants.OdataMediaEditLink)
                    || propName.Equals(Constants.OdataMediaReadLink) || propName.EndsWith(Constants.OdataNavigationLinkPropertyNameSuffix + "Url") || propName.EndsWith(Constants.OdataAssociationLinkPropertyNameSuffix + "Url"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Whether all urls are relative in JSON object payload.
        /// </summary>
        /// <param name="jp">JSON object payload </param>
        /// <returns>true: relative url exists; false: otherwise.</returns>
        private bool IsUrlsRelative(JObject jp)
        {
            bool result = true;

            foreach (var jProp in (JObject)jp)
            {
                if (jProp.Value.Type == JTokenType.Object)
                {
                    result = IsUrlsRelative((JObject)jProp.Value);
                }
                else if (jProp.Value.Type == JTokenType.Array)
                {
                    result = IsUrlsRelative((JArray)jProp.Value);
                }
                else
                {
                    if ((this.IsOdataV4LinkProp(jProp.Key) || this.IsOdataV3LinkProp(jProp.Key)) 
                        && !Uri.IsWellFormedUriString(jProp.Value.ToString().StripOffDoubleQuotes(), UriKind.Relative))
                    {
                        result = false;
                    }
                }

                if (result == false)
                {
                    break;
                }
            }

            return result;
        }

         /// <summary>
        /// Whether all urls are relative in JSON array payload.
        /// </summary>
        /// <param name="jp">JSON array payload </param>
        /// <returns>true: relative url exists; false: otherwise.</returns>
        private bool IsUrlsRelative(JArray ja)
        {
            bool result = true;

            foreach (var jv in (JArray)ja)
            {
                if (jv.Type == JTokenType.Object)
                {
                    result = IsUrlsRelative((JObject)jv);
                }
                else if (jv.Type == JTokenType.Array)
                {
                    result = IsUrlsRelative((JArray)jv);
                }

                if (result == false)
                {
                    break;
                }
            }

            return result;
        }
    }
}
