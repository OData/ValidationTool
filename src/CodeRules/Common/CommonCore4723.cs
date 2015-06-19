// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4723_Entry : CommonCore4723
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
    public class CommonCore4723_Feed : CommonCore4723
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
    /// Class of extension rule for Common.Core.4723
    /// </summary>
    public abstract class CommonCore4723 : ExtensionRule
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
                return "Common.Core.4723";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The title name/value pair in bound action contains the function or action title as a string.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "16";
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
        /// Verify Common.Core.4723
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

            info = null;
            bool? passed = null;
            bool isActionProperty = false;
            string actionName = string.Empty;

            XElement metadata = XElement.Parse(context.MetadataDocument);

            JObject allobject;
            context.ResponsePayload.TryToJObject(out allobject);
            var jProps = allobject.Children();

            // Use the XPath query language to access the metadata document to get all bound Action properties.
            // The bound action has IsBound as true and the first parameter with Type attribute equal to the entity type full name.
            string xpath = string.Format(@"//*[local-name()='Action' and @IsBound='true']/*[local-name()='Parameter' and position()=1 and @Type='{0}']/parent::*", context.EntityTypeFullName);
            List<string> actionImportNames = MetadataHelper.GetPropertyValues(context, xpath, "Name");

            xpath = @"//*[local-name() = 'Schema'][1]";
            XElement metadataSchema = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

            if (actionImportNames.Count > 0)
            {
                foreach (JProperty jProp in jProps)
                {
                    isActionProperty = JsonParserHelper.isFunctionOrActionProperty(actionImportNames, jProp, metadataSchema);

                    // Whether the property is bound action property exist.
                    if (isActionProperty)
                    {
                        if (jProp.Value["title"] != null && jProp.Value["title"].Type == JTokenType.String)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                    }
                }

                if (passed.HasValue && passed.Value == false)
                {
                    return false;
                }

                if (context.PayloadType == RuleEngine.PayloadType.Feed && allobject[Constants.Value] != null)
                {
                    foreach (JObject ob in (JArray)allobject[Constants.Value])
                    {
                        jProps = ob.Children();

                        foreach (JProperty jProp in jProps)
                        {
                            isActionProperty = JsonParserHelper.isFunctionOrActionProperty(actionImportNames, jProp, metadataSchema);

                            // Whether the property is bound action property exist.
                            if (isActionProperty)
                            {
                                if (jProp.Value["title"] != null && jProp.Value["title"].Type == JTokenType.String)
                                {
                                    passed = true;
                                }
                                else
                                {
                                    passed = false;
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                    break;
                                }
                            }
                        }

                        if (passed.HasValue && passed.Value == false)
                        {
                            break;
                        }
                    }
                }
            }
            return passed;
        }
    }
}

