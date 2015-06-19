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
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4405_Feed : CommonCore4405
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
    public class CommonCore4405_Entry : CommonCore4405
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
    /// Class of extension rule for Common.Core.4405
    /// </summary>
    public abstract class CommonCore4405 : ExtensionRule
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
                return "Common.Core.4405";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "For non-built in primitive types, the URI contains the namespace-qualified or alias-qualified type, specified as a URI fragment.";
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

            string xpath = @"//*[local-name()='DataServices']/*[local-name()='Schema']";
            List<string> qualitifiedNamespace = MetadataHelper.GetPropertyValues(context, xpath, "Namespace");
            List<string> qualitifiedAlias = MetadataHelper.GetPropertyValues(context, xpath, "Alias");

            var props = MetadataHelper.GetAllPropertiesOfEntity(context.MetadataDocument, context.EntityTypeShortName, MatchPropertyType.Normal);
            HashSet<string> builtInTypes = new HashSet<string>();

            foreach (var prop in props)
            {
                builtInTypes.Add(prop.Attribute("Type").Value);
            }

            JObject allobject;
            context.ResponsePayload.TryToJObject(out allobject);

            if (context.PayloadType == RuleEngine.PayloadType.Entry)
            {
                passed = this.VerifyOneEntry(allobject, builtInTypes, qualitifiedNamespace, qualitifiedAlias, context.Version);
            }
            else if (context.PayloadType == RuleEngine.PayloadType.Feed)
            {
                var entries = JsonParserHelper.GetEntries(allobject);
                foreach (JObject entry in entries)
                {
                    passed = this.VerifyOneEntry(entry, builtInTypes, qualitifiedNamespace, qualitifiedAlias, context.Version);
                    if (passed.HasValue && !passed.Value)
                    {
                        break;
                    }
                }
            }

            if (passed.HasValue && !passed.Value)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }

        /// <summary>
        /// Decide whether the one of special strings is a segment of target string.
        /// </summary>
        /// <param name="specialStrings">The special strings.</param>
        /// <param name="target">The target string.</param>
        /// <returns>Return the result.</returns>
        private bool IsContainsSpecialStrings(List<string> specialStrings, string target)
        {
            bool result = false;

            if (null == specialStrings || null == target || string.Empty == target)
            {
                return result;
            }

            foreach (var s in specialStrings)
            {
                if (target.Contains(s))
                {
                    result = true;
                    break;
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Verify whether the annotated property is a primitive type.
        /// </summary>
        /// <param name="jProp">The annotation property.</param>
        /// <returns>Return the result of verification.</returns>
        private bool IsPrimitiveType(JProperty jProp)
        {
            if (null == jProp || !jProp.Name.Contains(Constants.V4OdataType))
            {
                return false;
            }

            string annotatedPropName = null;

            if (null != jProp.Parent && jProp.Parent.Type == JTokenType.Object)
            {
                var parent = jProp.Parent as JObject;
                var children = parent.Children<JProperty>();

                foreach (JProperty c in children)
                {
                    if (c.Name == jProp.Name.Remove(jProp.Name.IndexOf('@'), jProp.Name.Length - jProp.Name.IndexOf('@')) &&
                        c.Value.Type != JTokenType.Array)
                    {
                        annotatedPropName = c.Name;
                        break;
                    }
                }
            }

            return null != annotatedPropName && (jProp.Next as JProperty).Name == annotatedPropName ? true : false;
        }

        /// <summary>
        /// Verify if one entry passed this rule
        /// </summary>
        /// <param name="entry">One entry object</param>
        /// <returns>true if rule passes; false otherwise</returns>
        private bool? VerifyOneEntry(JObject entry, HashSet<string> builtInTypes, List<string> qualitifiedAlias, List<string> qualitifiedNamespace, ODataVersion version = ODataVersion.V4)
        {
            if (entry == null || entry.Type != JTokenType.Object)
            {
                return null;
            }

            bool? passed = null;
            string jPropValue = string.Empty;

            var jProps = entry.Children();
            foreach (JProperty jProp in jProps)
            {
                if (jProp.Name.Contains(Constants.V4OdataType) && this.IsPrimitiveType(jProp))
                {
                    jPropValue = version ==
                        ODataVersion.V3 ?
                        jProp.Value.ToString().StripOffDoubleQuotes() :
                        Constants.EdmDotPrefix + jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#');

                    if (!builtInTypes.Contains(jPropValue))
                    {
                        passed = null;
                        if ((this.IsContainsSpecialStrings(qualitifiedAlias, jProp.Value.ToString().StripOffDoubleQuotes()) ||
                            this.IsContainsSpecialStrings(qualitifiedNamespace, jProp.Value.ToString().StripOffDoubleQuotes()) &&
                            Uri.IsWellFormedUriString(jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#'), UriKind.RelativeOrAbsolute)))
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

            return passed;
        }
    }
}
