// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for SvcDoc.Core.4003
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class SvcDocCore4003 : ExtensionRule
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
                return "SvcDoc.Core.4003";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The value of the odata.context property MUST NOT contain any fragment part in V4.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "5";
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
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.ServiceDoc;
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
        /// Verify SvcDoc.Core.4003
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out parameter to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            // odata.metadata must not be equal with "none".
            if (context.OdataMetadataType != ODataMetadataType.None)
            {
                JObject jo;
                context.ResponsePayload.TryToJObject(out jo);
                var inner = jo.ReachInnerToken();

                if (inner != null && inner.Type == JTokenType.Object)
                {
                    // Get odata.context.
                    var o = (JObject)inner;
                    var et = (JProperty)o.First;

                    // Verify whether the first property is odata.context.
                    if (et.Name.Equals(Constants.OdataV4JsonIdentity))
                    {
                        // Get the Url of odata.context.
                        string contextValue = et.Value.ToString().StripOffDoubleQuotes();

                        // Verify whether the Url of odata.context is a valid url.
                        bool isValidUrl = Uri.IsWellFormedUriString(contextValue, UriKind.RelativeOrAbsolute);

                        // A fragment is indicated by sign ("#") character.
                        // If the contextValue does not contain "#", it means that the odata.context does not contain fragment part.
                        bool isFragmentPartExist = !contextValue.Contains("#");

                        // The context value is a valid url and does not contain fragment part.
                        passed = isFragmentPartExist && isValidUrl;

                        if (passed == false)
                        {
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                        }
                    }
                    else
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                    }
                }
            }

            return passed;
        }
    }
}