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
    public class EntryCore4300 : ExtensionRule
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
                return "Entry.Core.4300";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "Responses MUST include the IEEE754Compatible parameter if Edm.Int64 and Edm.Decimal numbers are represented as strings.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.1";
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
        /// Verify rule logic
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

            JObject entry;
            context.ResponsePayload.TryToJObject(out entry);
            var o = (JObject)entry;
            var jProps = o.Children();

            bool isIntPropertyName = false;
            bool isDecimalPropertyName = false; 

            string xpath = @"//*[local-name()='EntityType']/*[local-name()='Property']";

            // Use the XPath query language to access the metadata document and get all Property Name which type is Edm.Int64.
            List<string> appropriateIntProperty = MetadataHelper.GetPropertyName(context, xpath, "Type", "Edm.Int64");

            // Use the XPath query language to access the metadata document and get all Property Name which type is Edm.Decimal.
            List<string> appropriateDecimalProperty = MetadataHelper.GetPropertyName(context, xpath, "Type", "Edm.Decimal");

            foreach (JProperty jProp in jProps)
            {
                if (appropriateIntProperty.Count != 0)
                {
                    // Verify the annoatation start with namespace.
                    foreach (string currentvalue in appropriateIntProperty)
                    {
                        if (jProp.Name.Contains(currentvalue))
                        {
                            isIntPropertyName = true;
                            break;
                        }
                    }
                }

                if (appropriateDecimalProperty.Count != 0)
                {
                    // Verify the annoatation start with alias.
                    foreach (string currentvalue in appropriateDecimalProperty)
                    {
                        if (jProp.Name.Contains(currentvalue))
                        {
                            isDecimalPropertyName = true;
                            break;
                        }
                    }
                } 

                // Whether the type of Properties with the value of Edm.Int64 or Edm.Decimal is string.
                if (jProp.Value.Type == JTokenType.String && (isDecimalPropertyName || isIntPropertyName))
                {
                    // Whether IEEE754Compatible parameter is inclued.
                    if (context.ResponseHttpHeaders.Contains("IEEE754Compatible"))
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

            return passed;
        }
    }
}

