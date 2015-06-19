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
    public class CommonCore4712_Entry : CommonCore4712
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
    public class CommonCore4712_Feed : CommonCore4712
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
    /// Class of extension rule for Common.Core.4712
    /// </summary>
    public abstract class CommonCore4712 : ExtensionRule
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
                return "Common.Core.4712";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If odata.metadata=full is requested, each value object for bound function MUST have at least the two name/value pairs title and target.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "15";
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
        /// Verify Common.Core.4712
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

            XElement metadata = XElement.Parse(context.MetadataDocument);

            JObject jo;
            context.ResponsePayload.TryToJObject(out jo);

            // Use the XPath query language to access the metadata document to get all Bound Function properties.
            // The bound function has IsBound as true and the first parameter with Type attribute equal to the entity type full name.
            string xpath = string.Format(@"//*[local-name()='Function' and @IsBound='true']/*[local-name()='Parameter' and position()=1 and @Type='{0}']/parent::*", context.EntityTypeFullName);            
            
            List<string> BoundfunctionNames = MetadataHelper.GetPropertyValues(context, xpath, "Name");

            xpath = @"//*[local-name() = 'Schema'][1]";
            XElement metadataSchema =  metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
                
            if (BoundfunctionNames.Count > 0 && context.Destination.ToString().Contains("odata.metadata=full"))
            {
                passed = HasBoundFunctionTitleTarget(context.PayloadType, BoundfunctionNames, jo, metadataSchema);

                if (passed.HasValue && passed.Value == false)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                }
            }

            return passed;
        }

        /// <summary>
        /// Get whether the bound function properties have title and target objects.
        /// </summary>
        /// <param name="payloadType">Feed or Entry as the payload type.</param>
        /// <param name="boundfunctionNames">The bound function names with the current entity.</param>
        /// <param name="jo">The json object of response.</param>
        /// <param name="metadata">The Schema XML element of the metadata.</param>
        /// <returns>true: rule pass; false: rule fail; null: not applicable.</returns>
        public bool? HasBoundFunctionTitleTarget(RuleEngine.PayloadType payloadType, List<string> boundfunctionNames, JObject jo, XElement metadataSchema)
        {
            if (payloadType == RuleEngine.PayloadType.Feed)
            {
                bool? result = HasBoundFunctionTitleTarget(RuleEngine.PayloadType.Entry, boundfunctionNames, jo, metadataSchema);

                if (result.HasValue && result.Value == false)
                {
                    return result;
                }

                foreach (JObject ob in (JArray)jo[Constants.Value])
                {
                    result = HasBoundFunctionTitleTarget(RuleEngine.PayloadType.Entry, boundfunctionNames, ob, metadataSchema);

                    if (result.HasValue && result.Value == false)
                    {
                        break;
                    }
                }
                return result;
            }
            else if (payloadType == RuleEngine.PayloadType.Entry)
            {
                bool? result = null;

                var jProps = jo.Children();

                foreach (JProperty jProp in jProps)
                {
                    bool isFunctionProperty = JsonParserHelper.isFunctionOrActionProperty(boundfunctionNames, jProp, metadataSchema);

                    // Whether the property is bound function property.
                    if (isFunctionProperty)
                    {
                        if (jProp.Value["title"] != null && jProp.Value["target"] != null)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                            break;
                        }
                    }
                }
                return result;
            }
            return null;
        }
    }
}

