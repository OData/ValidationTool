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
    /// Class of extension rule for Entry.Core.4325
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4325 : ExtensionRule
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
                return "Entry.Core.4325";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "In geography and geometry values, if the optional CRS object is present, it MUST be of type name, where the value of the name member of the contained properties object is an EPSG SRID legacy identifier.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "7.1";
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

            JObject entry;
            context.ResponsePayload.TryToJObject(out entry);

            if (entry != null && entry.Type == JTokenType.Object)
            {
                JsonParserHelper.ClearPropertiesList();
                List<JProperty> props = JsonParserHelper.GetSpecifiedPropertiesFromEntryPayload(entry, @"crs");

                foreach (var prop in props)
                {
                    if (prop.Value.Type == JTokenType.Object)
                    {
                        JProperty typeProp = null;
                        JProperty propertiesProp = null;

                        var crsProps = prop.Value.Children();

                        foreach (JProperty crsProp in crsProps)
                        {
                            if (@"type" == crsProp.Name)
                            {
                                typeProp = crsProp;
                            }

                            if (@"properties" == crsProp.Name)
                            {
                                propertiesProp = crsProp;
                            }

                            if (typeProp != null && propertiesProp != null)
                            {
                                JProperty nameProp = null;

                                if (propertiesProp.Value.Type == JTokenType.Object)
                                {
                                    var ps = propertiesProp.Value.Children();

                                    foreach (JProperty p in ps)
                                    {
                                        if (@"name" == p.Name)
                                        {
                                            nameProp = p;
                                        }
                                    }
                                }

                                if (nameProp != null &&
                                    @"name" == typeProp.Value.ToString().Trim('"') &&
                                    nameProp.Value.ToString().Trim('"').Contains(@"EPSG"))
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
                    }
                }
            }

            return passed;
        }
    }
}
