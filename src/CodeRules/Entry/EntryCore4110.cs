// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using ODataValidator.Rule.Helper;
    #endregion

    /// <summary>
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4110 : ExtensionRule
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
                return "Entry.Core.4110";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "For expanded navigation property, an empty collection of entities is represented as an empty JSON array.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.3";
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
        /// Gets the IsOfflineContext property to which the rule applies.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Verify Entry.Core.4110 rule logic
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out parameter to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise: // If passed is true mean an empty set of entities is represented as an empty JSON array, passed is false means error, passed is null means an empty JSON array is not be found.</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            List<string> queryOptionVals = ODataUriAnalyzer.GetQueryOptionValsFromUrl(context.Destination.ToString(), "expand");

            if (queryOptionVals.Count != 0)
            {
                // Use the XPath query language to access the metadata document and get the node which will be used.
                XElement metadata = XElement.Parse(context.MetadataDocument);
                string xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", context.EntityTypeShortName);
                IEnumerable<XElement> props = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

                JObject entry;
                context.ResponsePayload.TryToJObject(out entry);
                var o = (JObject)entry;
                var jProps = o.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (!queryOptionVals.Contains(jProp.Name))
                        continue;

                    // Whether the type of entities is array.
                    if (jProp.Value.Type == JTokenType.Array)
                    {
                        JArray jValues = jProp.Value as JArray;

                        // if the count of jProp.Value is 0, it means this entities is an empty set of entities.
                        if (jValues.Count == 0)
                        {
                            foreach (var prop in props)
                            {
                                // Whether expanded navigation property defined in metadata.
                                if (prop.Attribute("Name").Value.Contains(jProp.Name))
                                {
                                    passed = true;
                                    break;
                                }
                                else
                                {
                                    passed = false;
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
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

