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
    /// Class of extension rule for Entry.Core.4102
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4102 : ExtensionRule
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
                return "Entry.Core.4102";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The association link for a navigation property is only represented if the client requests odata.metadata=full in V4.";
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

            List<string> jNavgPropNames = JsonSchemaHelper.GetAllSpecifiedPropertyNameFromEntry(entry, Constants.OdataAssociationLinkPropertyNameSuffix) as List<string>;

            if (jNavgPropNames.Count != 0)
            {
                if (context.OdataMetadataType == ODataMetadataType.FullOnly)
                {
                    // Send a request with "application/json;odata.metadata = minimal" in Accept header.
                    string acceptHeader = Constants.V4AcceptHeaderJsonMinimalMetadata;
                     string minimalUrl = context.Destination.AbsoluteUri;

                    if (context.Destination.AbsoluteUri.Contains(";odata.metadata=full"))
                    {
                        minimalUrl = context.Destination.AbsoluteUri.Replace(";odata.metadata=full", "");
                    }

                    Response response = WebHelper.Get(new Uri(minimalUrl), acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    JObject newEntry;
                    response.ResponsePayload.TryToJObject(out newEntry);
                    List<string> jNewNavgPropNames = JsonSchemaHelper.GetAllSpecifiedPropertyNameFromEntry(newEntry, Constants.OdataAssociationLinkPropertyNameSuffix) as List<string>;

                    if (jNewNavgPropNames.Count == 0)
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                    }
                }
                else
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                }
            }
            else
            {
                passed = null;
            }

            return passed;
        }
    }
}
