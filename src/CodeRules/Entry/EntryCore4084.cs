// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using Newtonsoft.Json.Linq;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4084
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4084 : ExtensionRule
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
                return "Entry.Core.4084";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "Values of type Edm.Binary, Edm.Date, Edm.DateTimeOffset, Edm.Duration,  Edm.Guid, and Edm.TimeOfDay as well as enumeration values are represented as JSON strings.";
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
        /// Verify Entry.Core.4084
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
            List<string> types = new List<string>()
            {
                @"Edm.Binary", @"Edm.Date", @"Edm.DateTimeOffset", 
                @"Edm.Duration", @"Edm.Guid", @"Edm.TimeOfDay"
            };

            var props = MetadataHelper.GetAllPropertiesOfEntity(context.MetadataDocument, context.EntityTypeShortName, MatchPropertyType.Normal);
            Dictionary<string, string> nameTypePairs = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                nameTypePairs.Add(prop.Attribute(@"Name").Value, prop.Attribute(@"Type").Value);
            }

            var entry = JObject.Parse(context.ResponsePayload);

            if (entry != null && entry.Type == JTokenType.Object)
            {
                var jProps = entry.Children<JProperty>();

                foreach (var jP in jProps)
                {
                    if (nameTypePairs.ContainsKey(jP.Name) && types.Contains(nameTypePairs[jP.Name]))
                    {
                        string[] temp = StringHelper.Split(jP.ToString(), ':');
                        string jPropName = temp[0].Trim('"');
                        string jPropVal = temp[1].Trim();

                        if (jPropName == jP.Name && (jPropVal != jPropVal.Trim('"') || jPropVal == "null"))
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

            return passed;
        }
    }
}
