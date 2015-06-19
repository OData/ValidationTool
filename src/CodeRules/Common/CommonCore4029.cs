// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
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
    public class CommonCore4029_Entry : CommonCore4029
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
    public class CommonCore4029_Feed : CommonCore4029
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
    /// Class of extension rule for Common.Core.4029
    /// </summary>
    public abstract class CommonCore4029 : ExtensionRule
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
                return "Common.Core.4029";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"For the primitive type of a dynamic property in the absence of the odata.type annotation: The floating-point values NaN, INF, and -INF are serialized as strings and MUST have an odata.type annotation to specify the numeric type.";
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
        /// Gets the odata metadata type to which the rule applies.
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
                return null;
            }
        }

        /// <summary>
        /// Verify Common.Core.4029
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

            // If Payload type is Feed.
            if (context.PayloadType.Equals(RuleEngine.PayloadType.Feed))
            {
                var entries = JsonParserHelper.GetEntries(allobject);
                foreach (JObject entry in entries)
                {
                    bool? onepassed = this.VerifyOneEntry(entry, context);
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
            else if (context.PayloadType.Equals(RuleEngine.PayloadType.Entry))
            {
                passed = this.VerifyOneEntry(allobject, context);
            }

            if (passed == false)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }

        private bool? VerifyOneEntry(JObject entry, ServiceContext context)
        {
            bool? passed = null;
            // Record the name of @odata.type annotation.
            string nameRecord = string.Empty;
            // Record the value of @odata.type annotation.
            JToken valueRecord = null;

            var jProps = entry.Children();
            foreach (JProperty jProp in jProps)
            {
                // Find @odata.type annotation.
                if (jProp.Name.EndsWith(Constants.V4OdataType) && !jProp.Name.Equals(Constants.V4OdataType))
                {
                    // Record the Name and value of @odata.type annotation.
                    nameRecord = jProp.Name;
                    valueRecord = jProp.Value;
                }
                else
                {
                    if (nameRecord == string.Empty)
                    {
                        continue;
                    }
                    // Prase the annotation name to two parts.
                    string[] splitedStr = nameRecord.Split('@');

                    // If the string before the sign "@" are the same with property name, this rule will passed.
                    if (jProp.Name == splitedStr[0])
                    {
                        if ("NaN" == jProp.Value.ToString() || "INF" == jProp.Value.ToString() || "-INF" == jProp.Value.ToString())
                        {
                            // Whether the value NaN, INF, and -INF are serialized as strings. And the type of @odata.type annotation is numeric type.
                            if (jProp.Value.Type == JTokenType.String && PrimitiveDataTypes.NumbericTypesDefinedInOdataTypeValue.Contains(valueRecord.ToString()))
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
                }
            }

            return passed;
        }
    }
}

