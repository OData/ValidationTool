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
    /// Class of extension rule for Entry.Core.4094
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4094 : ExtensionRule
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
                return "Entry.Core.4094";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The name of navigation link MUST be the name of the property, followed by @odata.navigationLink.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.1";
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

            // Use the XPath query language to access the metadata document and get the node which will be used.
            string xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", context.EntityTypeShortName);
            XElement metadata = XElement.Parse(context.MetadataDocument);
            var propNodesList = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            if (entry != null && entry.Type == JTokenType.Object)
            {
                // Get all the properties in current entry.
                var jProps = entry.Children();
                List<string> jPropNames = new List<string>();

                foreach (JProperty jProp in jProps)
                {
                    if (jProp.Name.EndsWith(Constants.OdataNavigationLinkPropertyNameSuffix) || jProp.Name.EndsWith(Constants.OdataNavigationLinkPropertyNameSuffix + "Url"))
                    {
                        jPropNames.Add(jProp.Name);
                    }
                }

                if (jPropNames.Count > 0)
                {
                    foreach (var propNode in propNodesList)
                    {
                        passed = null;

                        if (jPropNames.Contains(propNode.Attribute("Name").Value + Constants.OdataNavigationLinkPropertyNameSuffix) ||
                            jPropNames.Contains(propNode.Attribute("Name").Value + Constants.OdataNavigationLinkPropertyNameSuffix + "Url"))
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
