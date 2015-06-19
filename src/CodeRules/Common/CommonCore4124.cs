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
    public class CommonCore4124_Entry : CommonCore4124
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
    public class CommonCore4124_Feed : CommonCore4124
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
    /// Class of extension rule for Common.Core.4124
    /// </summary>
    public abstract class CommonCore4124 : ExtensionRule
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
                return "Common.Core.4124";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"When annotating a JSON object, the annotation name always starts with the ""at"" sign(@), followed by the namespace- or alias-qualified name of the annotation.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "18.1";
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
                return true;
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

            JObject allobject;
            context.ResponsePayload.TryToJObject(out allobject);
            string namespaceValue = string.Empty;
            string alias = string.Empty;
           
            // Whether odata.type value is namespace- or alias-qualified the instance's type
            bool isNamespaceValue = false;
            bool isAliasValue = false;
            bool isStartwithodata = false;

            // Use the XPath query language to access the metadata document and get all Namespace value.
            string xpath = @"//*[local-name()='DataServices']/*[local-name()='Schema']";
            List<string> appropriateNamespace = MetadataHelper.GetPropertyValues(context, xpath, "Namespace");
            namespaceValue = appropriateNamespace[0];

            // Get Alias value.
            List<string> appropriateAlias = MetadataHelper.GetPropertyValues(context, xpath, "Alias");

            if (appropriateAlias.Count > 0)
            {
                alias = appropriateAlias[0];
            }

            // If PayloadType is Feed, verify as below, eles is Entry.
            if (context.PayloadType.Equals(RuleEngine.PayloadType.Feed))
            {
                var entries = JsonParserHelper.GetEntries(allobject);

                foreach (JObject entry in entries)
                {
                    var o = (JObject)entry;
                    var jProps = o.Children();

                    foreach (JProperty jProp in jProps)
                    {
                        if (JsonSchemaHelper.IsAnnotation(jProp.Name) && jProp.Name.StartsWith("@"))
                        {
                            // odata is the namespace element, if contains it the whole element must be annotation.
                            isStartwithodata = jProp.Name.StartsWith(@"@odata");
                            isNamespaceValue = jProp.Name.StartsWith("@" + namespaceValue);

                            if (!string.IsNullOrEmpty(alias))
                            {
                                isAliasValue = jProp.Name.StartsWith("@" + alias);
                            }

                            if (isStartwithodata || isNamespaceValue || isAliasValue)
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

                    if (passed == false)
                    {
                        break;
                    }
                }
            }
            else
            {
                var o = (JObject)allobject;
                var jProps = o.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (JsonSchemaHelper.IsAnnotation(jProp.Name) && jProp.Name.StartsWith("@"))
                    {
                        // odata is the namespace element, if contains it the whole element must be annotation.
                        isStartwithodata = jProp.Name.StartsWith(@"@odata");
                        isNamespaceValue = jProp.Name.StartsWith("@" + namespaceValue);

                        if (!string.IsNullOrEmpty(alias))
                        {
                            isAliasValue = jProp.Name.StartsWith("@" + alias);
                        }

                        if (isStartwithodata || isNamespaceValue || isAliasValue)
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

            if (passed == false)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }
    }
}
