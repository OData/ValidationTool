// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4130_Feed : CommonCore4130
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
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4130_Entry : CommonCore4130
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
    /// Class of extension rule for Common.Core.4130
    /// </summary>
    public abstract class CommonCore4130 : ExtensionRule
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
                return "Common.Core.4130";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "This name of each annotation MUST include namespace and name, separated by a period (.), followed by the name of the term.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "18.2";
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
        /// Gets the IsOfflineContext property to which the rule applies.
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

            if (context.PayloadType == RuleEngine.PayloadType.Feed)
            {
                JObject feed;
                context.ResponsePayload.TryToJObject(out feed);
                var entries = JsonParserHelper.GetEntries(feed);

                foreach (JObject entry in entries)
                {
                    if (entry != null && entry.Type == JTokenType.Object)
                    {
                        var jProps = entry.Children();

                        foreach (JProperty jProp in jProps)
                        {
                            if (JsonSchemaHelper.IsJSONArrayOrPrimitiveAnnotation(jProp.Name))
                            {
                                passed = null;
                                string[] splitedStr = jProp.Name.Split('@');

                                JProperty nextProp = jProp.Next as JProperty;
                                JProperty prevProp = jProp.Previous as JProperty;

                                string annotatedPropName = nextProp.Name == splitedStr[0] ? nextProp.Name : prevProp.Name;

                                if (annotatedPropName.Contains(splitedStr[0]))
                                {
                                    if (jProp.Name == annotatedPropName + Constants.V4OdataType)
                                    {
                                        passed = true;
                                        continue;
                                    }
                                    else
                                    {
                                        passed = false;
                                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                        return passed;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (context.PayloadType == RuleEngine.PayloadType.Entry)
            {
                JObject entry;
                context.ResponsePayload.TryToJObject(out entry);

                if (entry != null && entry.Type == JTokenType.Object)
                {
                    var jProps = entry.Children();

                    foreach (JProperty jProp in jProps)
                    {
                        if (JsonSchemaHelper.IsJSONArrayOrPrimitiveAnnotation(jProp.Name))
                        {
                            passed = null;
                            string[] splitedStr = jProp.Name.Split('@');

                            JProperty nextProp = jProp.Next as JProperty;
                            JProperty prevProp = jProp.Previous as JProperty;

                            string annotatedPropName = nextProp.Name == splitedStr[0] ? nextProp.Name : prevProp.Name;

                            if (annotatedPropName.Contains(splitedStr[0]))
                            {
                                if (jProp.Name == annotatedPropName + Constants.V4OdataType)
                                {
                                    passed = true;
                                    continue;
                                }
                                else
                                {
                                    passed = false;
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                    return passed;
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
