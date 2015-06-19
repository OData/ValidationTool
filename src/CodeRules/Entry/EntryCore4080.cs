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
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4080
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4080 : ExtensionRule
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
                return "Entry.Core.4080";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "Values of types Edm.Byte, Edm.SByte, Edm.Int16, Edm.Int32, Edm.Int64, Edm.Single, Edm.Double, and Edm.Decimal are represented as JSON numbers.";
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
                return false;
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
            bool IEEE754Compatible = false;

            JObject entry;
            context.ResponsePayload.TryToJObject(out entry);

            string contentType = context.ResponseHttpHeaders.GetHeaderValue("Content-Type");

            if (Regex.Replace(contentType, @"\s+", "").Contains("IEEE754Compatible=true;"))
            {
                IEEE754Compatible = true;
            }

            XElement metadata = XElement.Parse(context.MetadataDocument);

            // Use the XPath query language to access the metadata document and get the node which will be used.
            string xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", context.EntityTypeShortName);
            IEnumerable<XElement> props = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            if (entry != null && entry.Type == JTokenType.Object)
            {
                // Get all the properties in current entry.
                var jProps = entry.Children();

                foreach (JProperty jProp in jProps)
                {
                    string propType = string.Empty;

                    foreach (var prop in props)
                    {
                        if (jProp.Name == prop.Attribute("Name").Value)
                        {
                            propType = prop.Attribute("Type").Value;
                            break;
                        }
                    }

                    if (propType == "Edm.Byte" || propType == "Edm.SByte" || propType == "Edm.Int16" ||
                        propType == "Edm.Int32" || (propType == "Edm.Int64" && !IEEE754Compatible) || propType == "Edm.Single" ||
                        propType == "Edm.Double" || (propType == "Edm.Decimal" && !IEEE754Compatible))
                    {
                        if (jProp.Value.Type == JTokenType.Float || jProp.Value.Type == JTokenType.Integer)
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
