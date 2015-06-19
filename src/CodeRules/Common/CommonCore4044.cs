// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4044_Entry : CommonCore4044
    {
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
    }

    /// <summary>
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4044_Feed : CommonCore4044
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
    /// Class of code rule applying to DeltaResponse payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4044_DeltaResponse : CommonCore4044
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Delta;
            }
        }
    }

    /// <summary>
    /// Class of code rule applying to other payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4044_Other : CommonCore4044
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Other;
            }
        }
    }

    /// <summary>
    /// Class of extension rule for Common.Core.4044
    /// </summary>
    public abstract class CommonCore4044 : ExtensionRule
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
                return "Common.Core.4044";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The odata.editLink annotation is written if the edit URL differs from the default value of the edit URL.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.5.8";
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
                return ODataVersion.V3_V4;
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
        /// Gets the odata metadata type to which the rule applies.
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
                return null;
            }
        }

        /// <summary>
        /// Verify Common.Core.4044
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
            JObject allobject;
            context.ResponsePayload.TryToJObject(out allobject);

            // If PayloadType is Feed, verify as below, else is Entry.
            if (context.PayloadType.Equals(RuleEngine.PayloadType.Feed))
            {
                var entries = JsonParserHelper.GetEntries(allobject);
                foreach (JObject entry in entries)
                {
                    bool? onepassed = this.VerifyOneEntry(entry);
                    if (onepassed.HasValue)
                    {
                        passed = onepassed;
                        if (!passed.Value)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                passed = this.VerifyOneEntry(allobject);
            }

            if (passed.HasValue && !passed.Value)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }

        /// <summary>
        /// Verify if one entry passed this rule
        /// </summary>
        /// <param name="entry">One entry object</param>
        /// <returns>true if rule passes; false otherwise</returns>
        private bool? VerifyOneEntry(JObject entry)
        {
            if (entry == null || entry.Type != JTokenType.Object)
            {
                return null;
            }

            bool? passed = null;
            string editLinkValue = string.Empty;
            string idValue = string.Empty;

            var jProps = entry.Children();
            foreach (JProperty jProp in jProps)
            {
                if (!string.IsNullOrEmpty(editLinkValue) && !string.IsNullOrEmpty(idValue))
                {
                    break;
                }
                if (jProp.Name.Equals(Constants.V4OdataEditLink))
                {
                    // Get the Url of odata.editlink.
                    editLinkValue = jProp.Value.ToString().StripOffDoubleQuotes();
                    continue;
                }
                if (jProp.Name.Equals(Constants.V4OdataId))
                {
                    // Get the corresponding Url odata.id for odata.editLinkValue.
                    idValue = jProp.Value.ToString().StripOffDoubleQuotes();
                    continue;
                }
            }

            // If odata.editLinkValue exists in minimalmetadata, its value must not be equal to corresponding odata.id
            if (!string.IsNullOrEmpty(editLinkValue))
            {
                if (!idValue.Equals(editLinkValue))
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                }
            }

            return passed;
        }
    }
}

