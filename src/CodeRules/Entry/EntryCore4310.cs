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
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4310
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4310 : ExtensionRule
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
                return "Entry.Core.4310";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The default value of both the edit URL and read URL is the entity's entity-id appended with a cast segment to the type of the entity if its type is derived from the declared type of the entity set.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.5.8";
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
        /// Gets the OData metadata type to which the rule applies.
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
            List<string> entittyTypes = new List<string>();
            string derivedEntityTypeName = string.Empty;
            string editUrl = string.Empty;
            string readUrl = string.Empty;

            string odataeditLink = Constants.V4OdataEditLink;
            string odataTypeName = Constants.V4OdataType;
            string odatareadLink = Constants.V4OdataReadLink;
           
            if (context.Version == ODataVersion.V3)
            {
                odataeditLink = Constants.OdataEditLink;
                odataTypeName = Constants.OdataType;
                odatareadLink = Constants.OdataReadLink;
            }           

            // Use the XPath query language to access the metadata document and get all Namespace and alias values.
            string xpath = string.Format(@"//*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='EntityType' and @Name='{0}'][@BaseType]", context.EntityTypeShortName);
            XElement metadata = XElement.Parse(context.MetadataDocument);
            var derivedElement = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

            if (derivedElement != null)
            {
                derivedEntityTypeName = ((XElement)derivedElement).GetAttributeValue("Name");
            }

            if (!string.IsNullOrEmpty(derivedEntityTypeName))
            {
                if (entry[odataTypeName] != null)
                {
                    string odataType = entry[odataTypeName].Value<string>().StripOffDoubleQuotes().TrimStart('#');
                    string typeWithoutNamespace = odataType.Substring(odataType.LastIndexOf('.') + 1);

                    // Whether the entity is derived entity type.
                    if (derivedEntityTypeName.Equals(typeWithoutNamespace))
                    {
                        if (entry[odataeditLink] != null)
                        {
                            editUrl = entry[odataeditLink].Value<string>().StripOffDoubleQuotes();

                            if (!editUrl.EndsWith(@"/" + odataType))
                            {
                                passed = false;
                                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            }
                        }

                        if (entry[odatareadLink] != null)
                        {
                            readUrl = entry[odatareadLink].Value<string>().StripOffDoubleQuotes();

                            if (!readUrl.EndsWith(@"/" + odataType))
                            {
                                passed = false;
                                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            }
                        }

                        if ((entry[odataeditLink] != null || entry[odatareadLink] != null) && passed == null)
                        {
                            passed = true;
                        }
                    }
                }
            }

            return passed;
        }
    }
}
