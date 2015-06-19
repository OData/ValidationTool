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
    public class CommonCore4028_Entry : CommonCore4028
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
    public class CommonCore4028_Feed : CommonCore4028
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
    /// Class of extension rule for Common.Core.4028
    /// </summary>
    public abstract class CommonCore4028 : ExtensionRule
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
                return "Common.Core.4028";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The odata.type annotation MUST appear in minimal or full metadata if the type that is for a property whose type is not declared in $metadata, cannot be heuristically determined.";
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
        /// Verify Common.Core.4028
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

            JObject minimalResponsePayLoad;
            context.ResponsePayload.TryToJObject(out minimalResponsePayLoad);

            if (context.OdataMetadataType != ODataMetadataType.MinOnly)
            {
                string acceptHeader = Constants.V3AcceptHeaderJsonMinimalMetadata;
                string minimalUrl = context.Destination.AbsoluteUri;

                if (context.Destination.AbsoluteUri.Contains(";odata=fullmetadata"))
                {
                    minimalUrl = context.Destination.AbsoluteUri.Replace(";odata=fullmetadata", "");
                }

                if (context.Version == ODataVersion.V4)
                {
                    acceptHeader = Constants.V4AcceptHeaderJsonMinimalMetadata;

                    if (context.Destination.AbsoluteUri.Contains(";odata.metadata=full"))
                    {
                        minimalUrl = context.Destination.AbsoluteUri.Replace(";odata.metadata=full", "");
                    }
                }

                Response mimimalResponse = WebHelper.Get(new Uri(minimalUrl), acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                mimimalResponse.ResponsePayload.TryToJObject(out minimalResponsePayLoad);
            }

            // Use the XPath query language to access the metadata document and get all Namespace and Alias value.
            string xpath = @"//*[local-name()='DataServices']/*[local-name()='Schema']";
            List<string> appropriateNamespace = MetadataHelper.GetPropertyValues(context, xpath, "Namespace");
            List<string> appropriateAlias = MetadataHelper.GetPropertyValues(context, xpath, "Alias");

            // If PayloadType is Feed, verify every entry.
            if (context.PayloadType.Equals(RuleEngine.PayloadType.Feed))
            {
                var entries = JsonParserHelper.GetEntries(minimalResponsePayLoad);
                foreach (JObject entry in entries)
                {
                    bool? onepassed = this.VerifyOneEntry(entry, context, appropriateNamespace, appropriateAlias);
                    if (onepassed.HasValue)
                    {
                        passed = onepassed;
                        if (!passed.Value)
                        {
                            break;
                        }
                    }
                }
            }
            else if (context.PayloadType.Equals(RuleEngine.PayloadType.Entry))
            {
                passed = this.VerifyOneEntry(minimalResponsePayLoad, context, appropriateNamespace, appropriateAlias);
            }

            if (passed == false)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }

        private bool? VerifyOneEntry(JObject entry, ServiceContext context, List<string> appropriateNamespace, List<string> appropriateAlias)
        {
            bool? passed = null;
            string appropriateType = string.Empty;

            // Whether odata.type value is namespace- or alias-qualified the instance's type
            bool isNamespaceValue = false;
            bool isAliasValue = false;

            var jProps = entry.Children();
            foreach (JProperty jProp in jProps)
            {
                // Whether odata.type exist in response.
                if (JsonSchemaHelper.IsAnnotation(jProp.Name) && jProp.Name.EndsWith("@" + Constants.OdataType) && !jProp.Name.StartsWith("@" + Constants.OdataType))
                {
                    string jPropName = jProp.Name.Remove(jProp.Name.IndexOf("@" + Constants.OdataType));

                    // Compare the value of odata.type with EntityType and ComplexType.
                    if (MetadataHelper.IsPropsExistInMetadata(jPropName, context, out appropriateType))
                    {
                        // Verify the annotation start with namespace.
                        foreach (string currentvalue in appropriateNamespace)
                        {
                            if (appropriateType.Contains(currentvalue))
                            {
                                isNamespaceValue = true;
                                break;
                            }
                            else
                            {
                                isNamespaceValue = false;
                            }
                        }
                        // Verify the annotation start with alias.
                        foreach (string currentvalue in appropriateAlias)
                        {
                            if (appropriateType.Contains(currentvalue))
                            {
                                isAliasValue = true;
                                break;
                            }
                            else
                            {
                                isAliasValue = false;
                            }
                        }

                        if (!appropriateType.StartsWith("Edm.") && !isNamespaceValue && !isAliasValue)
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

