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
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4027_Entry : CommonCore4027
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
    /// Class of extension rule for Common.Core.4027
    /// </summary>
    public abstract class CommonCore4027 : ExtensionRule
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
                return "Common.Core.4027";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The annotation odata.type MUST appear in minimal or full metadata if the type that is derived from the type specified for the (set of) entities or (collection of) complex type instances, cannot be heuristically determined.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.5.3";
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
        /// Gets the RequireMetadata property to which the rule applies.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
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
        /// Verify Common.Core.4027
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

            JObject feed;
            context.ResponsePayload.TryToJObject(out feed);

            string odataTypeName = Constants.V4OdataType;

            if (context.Version == ODataVersion.V3)
            {
                odataTypeName = Constants.OdataType;
            }

            // Use the XPath query language to access the metadata document and get all derived EntityType Name.
            string xpath = string.Format(@"//*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='EntityType' and @Name='{0}'][@BaseType]", context.EntityTypeShortName);
            List<string> derivedEntityTypeName = MetadataHelper.GetPropertyValues(context, xpath, "Name");

            // context.EntityTypeShortName is the value of odata.type in response payload, if context.EntityTypeShortName is the derived property, the derivedEntityTypeName should not be 0; 
            // so if derivedEntityTypeName.Count > 0, the derived property exist in response payload as odata.type annotation's value.
            if (derivedEntityTypeName.Count > 0)
            {
                if (context.PayloadType == RuleEngine.PayloadType.Entry)
                {
                    // Whether odata.type annotation exist.
                    if (feed[odataTypeName] != null)
                    {
                        passed = true;
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

