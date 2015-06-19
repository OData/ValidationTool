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
    public class CommonCore4308_Entry : CommonCore4308
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
    public class CommonCore4308_Feed : CommonCore4308
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
    /// Class of extension rule for Common.Core.4308
    /// </summary>
    public abstract class CommonCore4308 : ExtensionRule
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
                return "Common.Core.4308";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If annotates a JSON object, the namespace or alias MUST be defined in the metadata document, see [OData-CSDL].";
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

            // Whether odata.type value is namespace- or alias-qualified the instance's type
            bool isNamespaceValue = false;
            bool isAliasValue = false;

            // Use the XPath query language to access the metadata document and get all Namespace value.
            string xpath = @"//*[local-name()='DataServices']/*[local-name()='Schema']";
            List<string> appropriateNamespace = MetadataHelper.GetPropertyValues(context, xpath, "Namespace");

            // Get Alias value.
            List<string> appropriateAlias = MetadataHelper.GetPropertyValues(context, xpath, "Alias");

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
                        if (JsonSchemaHelper.IsAnnotation(jProp.Name) && jProp.Parent.Type.Equals(JTokenType.Object))
                        {
                            if (appropriateNamespace.Count != 0)
                            {
                                // Verify the annoatation start with namespace.
                                foreach (string currentvalue in appropriateNamespace)
                                {
                                    if (jProp.Name.Contains(currentvalue))
                                    {
                                        isNamespaceValue = true;
                                    }
                                    else
                                    {
                                        isNamespaceValue = false;
                                    }
                                }
                            }

                            if (appropriateAlias.Count != 0)
                            {
                                // Verify the annoatation start with alias.
                                foreach (string currentvalue in appropriateAlias)
                                {
                                    if (jProp.Name.Contains(currentvalue))
                                    {
                                        isAliasValue = true;
                                    }
                                    else
                                    {
                                        isAliasValue = false;
                                    }
                                }
                            }

                            if (!isNamespaceValue && !isAliasValue)
                            {                                    
                                // Whether the annoatation use odata instead of namespace- or alias-qualified name of the annotation.
                                if (jProp.Name.Contains(Constants.OdataNS))
                                {
                                    passed = true;
                                    continue;
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

                    if (passed == false)
                    {
                        break;
                    }
                }
            }
            else
            {
                var jProps = allobject.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (JsonSchemaHelper.IsAnnotation(jProp.Name) && jProp.Parent.Type.Equals(JTokenType.Object))
                    {
                        if (appropriateNamespace.Count != 0)
                        {
                            // Verify the annoatation start with namespace.
                            foreach (string currentvalue in appropriateNamespace)
                            {
                                if (jProp.Name.Contains(currentvalue))
                                {
                                    isNamespaceValue = true;
                                }
                                else
                                {
                                    isNamespaceValue = false;
                                }
                            }
                        }

                        if (appropriateAlias.Count != 0)
                        {
                            // Verify the annoatation start with alias.
                            foreach (string currentvalue in appropriateAlias)
                            {
                                if (jProp.Name.Contains(currentvalue))
                                {
                                    isAliasValue = true;
                                }
                                else
                                {
                                    isAliasValue = false;
                                }
                            }
                        }

                        if (!isNamespaceValue && !isAliasValue)
                        {
                            // Whether the annoatation use odata instead of namespace- or alias-qualified name of the annotation.
                            if (jProp.Name.Contains(Constants.OdataNS))
                            {
                                passed = true;
                                continue;
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

            return passed;
        }
    }
}
