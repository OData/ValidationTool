// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4086
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4086 : ExtensionRule
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
                return "Entry.Core.4086";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "A complex value is represented as a single JSON object containing one name/value pair for each property that makes up the complex type.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "7.2";
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

            List<string> complexTypeNames = MetadataHelper.GetAllComplexNameFromMetadata(context.MetadataDocument);

            if (complexTypeNames.Count > 0)
            {
                Dictionary<string, string> propertyWithComplexType = MetadataHelper.GetPropertyNameWithComplexTypeFromEntity(context.EntityTypeShortName, context.MetadataDocument, complexTypeNames);

                if (propertyWithComplexType.Count > 0)
                {
                    JObject entry;
                    context.ResponsePayload.TryToJObject(out entry);

                    foreach (JProperty jp in entry.Properties())
                    {
                        if (propertyWithComplexType.Keys.Contains(jp.Name))
                        {
                            if (jp.Value.Type != JTokenType.Array)
                            {
                                passed = false;
                                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                break;
                            }
                            else
                            {
                                // Find the property with complex type.
                                List<string> complexProperties = MetadataHelper.GetAllPropertiesNamesOfComplexType(context.MetadataDocument, propertyWithComplexType[jp.Name]);

                                foreach (var jv in ((JArray)jp.Value))
                                {
                                    if (jv.Type != JTokenType.Object)
                                    {
                                        passed = false;
                                        break;
                                    }
                                    else
                                    {
                                        foreach (JProperty pro in ((JObject)jv).Properties())
                                        {
                                            // Check whether the properties of complex exist in metadata.
                                            if (!JsonSchemaHelper.IsAnnotation(pro.Name))
                                            {
                                                if (complexProperties.Contains(pro.Name))
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

                                if (passed == false)
                                {
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                    break;
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
