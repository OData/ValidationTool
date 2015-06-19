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
    using System.Xml.Linq;
    #endregion

    /// <summary>
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4406_Feed : CommonCore4406
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
    public class CommonCore4406_Entry : CommonCore4406
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
    /// Class of extension rule for Common.Core.4406
    /// </summary>
    public abstract class CommonCore4406 : ExtensionRule
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
                return "Common.Core.4406";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "For properties that represent a collection of values, the fragment is the namespace-qualified or alias-qualified element type enclosed in parentheses and prefixed with Collection.";
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
            List<string> collectionTypeNavigPropNames = new List<string>();

            // Get all navigation property names with collection type from metadata.
            collectionTypeNavigPropNames = MetadataHelper.GetAppropriateNavigationPropertyNames(context, NavigationPropertyType.SetOfEntities);

            List<XElement> allProps = MetadataHelper.GetAllPropertiesOfEntity(context.MetadataDocument, context.EntityTypeShortName, MatchPropertyType.Normal);
            List<string> propNames = new List<string>();

            // Get all normal property name with collection type from metadata.
            foreach (var prop in allProps)
            {
                if (prop.Attribute("Type").Value.StartsWith("Collection("))
                {
                    collectionTypeNavigPropNames.Add(prop.Attribute("Name").Value);
                }
            }

            if (context.PayloadType == RuleEngine.PayloadType.Entry)
            {
                JObject entry;
                context.ResponsePayload.TryToJObject(out entry);
               
                if (entry != null && entry.Type == JTokenType.Object && collectionTypeNavigPropNames.Count != 0)
                {
                    var jProps = entry.Children();

                    foreach (JProperty jProp in jProps)
                    {
                        if (jProp.Value.Type == JTokenType.Array)
                        {
                            if (collectionTypeNavigPropNames.Contains(jProp.Name))
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
            else if (context.PayloadType == RuleEngine.PayloadType.Feed)
            {
                JObject feed;
                context.ResponsePayload.TryToJObject(out feed);
                var entries = JsonParserHelper.GetEntries(feed);                

                foreach (JObject entry in entries)
                {
                    if (entry != null && entry.Type == JTokenType.Object && collectionTypeNavigPropNames.Count != 0)
                    {
                        var jProps = entry.Children();

                        foreach (JProperty jProp in jProps)
                        {
                            if (jProp.Value.Type == JTokenType.Array)
                            {
                                if (collectionTypeNavigPropNames.Contains(jProp.Name))
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

                        if (passed == false)
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