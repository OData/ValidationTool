// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Feed.Core.4013
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore4013 : ExtensionRule
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
                return "Feed.Core.4013";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "For collections of entities each element is representation of an entity or a representation of an entity reference.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "12";
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
                return RuleEngine.PayloadType.Feed;
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

            XElement metadata = XElement.Parse(context.MetadataDocument);

            // Use the XPath query language to access the metadata document and get the node which will be used.
            string xpath = @"//*[local-name()='EntityType']/*[local-name()='Property' or local-name()='NavigationProperty']";
            IEnumerable<XElement> propNameElements = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            List<string> propNames = new List<string>();

            foreach (var propNameElement in propNameElements)
            {
                propNames.Add(propNameElement.Attribute("Name").Value);
            }

            bool isQueryRefVals = context.Destination.ToString().Contains(@"$ref");

            JObject feed;
            context.ResponsePayload.TryToJObject(out feed);
            var entries = JsonParserHelper.GetEntries(feed);

            foreach (JObject entry in entries)
            {
                if (entry != null && entry.Type == JTokenType.Object)
                {
                    passed = null;

                    // Get all the properties in current entry.
                    var jProps = entry.Children();

                    // If the url contains the "$ref" marker, the program can verify the condition that each element must be represented as an entity reference.
                    // If the url does not contain the "$ref" marker, the program can verify the condition that each element must be represented as an entity.
                    if (isQueryRefVals)
                    {
                        foreach (JProperty jProp in jProps)
                        {
                            if (JsonSchemaHelper.IsAnnotation(jProp.Name) && jProp.Name.Contains(@"id"))
                            {
                                passed = true;
                                break;
                            }
                        }

                        if (passed == null)
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                    }
                    else
                    {
                        foreach (JProperty jProp in jProps)
                        {
                            if (!JsonSchemaHelper.IsAnnotation(jProp.Name) && propNames.Contains(jProp.Name))
                            {
                                passed = true;
                            }
                            else if (!JsonSchemaHelper.IsAnnotation(jProp.Name) && !propNames.Contains(jProp.Name))
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
