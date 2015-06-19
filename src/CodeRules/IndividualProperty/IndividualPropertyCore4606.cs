// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for IndividualProperty.Core.4606
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IndividualPropertyCore4606 : ExtensionRule
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
                return "IndividualProperty.Core.4606";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If the type of the scalar value being specified is anything other than Edm.String the metadata:type attribute in metadata:value element for Single Scalar Value MUST be present and specify the namespace - or alias - qualified type of the value.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.1.1.3";
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
                return RuleEngine.PayloadType.IndividualProperty;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
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
        /// Gets the flag whether the rule requires metadata document
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Verify IndividualProperty.Core.4606
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

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.ResponsePayload);
            XmlElement root = xmlDoc.DocumentElement;

            XElement metadata = XElement.Parse(context.MetadataDocument);
            string xpath = string.Empty;
            string individualPropType = string.Empty;
            string entityTypeShortName = string.Empty;

            if (root.LocalName.Equals("value") && root.NamespaceURI.Equals(Constants.NSMetadata))
            {
                if (root.Attributes["context", Constants.NSMetadata] != null)
                {
                    string contextURI = root.Attributes["context", Constants.NSMetadata].Value;
                    string propertyType = contextURI.Remove(0, contextURI.IndexOf('#') + 1);

                    if (propertyType.Contains("Collection("))
                    {
                        return null;
                    }

                    string[] types = propertyType.Split('/');
                    if (types.Length == 2)
                    {
                        if (types[0].Contains("("))
                        {
                            string entitySetName = types[0].Remove(types[0].IndexOf('('));
                            entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();
                        }
                        else
                        {
                            xpath = string.Format(@"//*[local-name()='Singleton' and @Name='{0}']", types[0]);
                            XElement singleton = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
                            entityTypeShortName = singleton.GetAttributeValue("Type").GetLastSegment();
                        }

                        individualPropType = MetadataHelper.GetPropertyTypeFromMetadata(types[1], entityTypeShortName, context.MetadataDocument);
                    }
                    else if (types.Length < 2)
                    {
                        individualPropType = propertyType;
                    }

                    if (!individualPropType.Equals("Edm.String"))
                    {
                        xpath = @"//*[local-name()='DataServices']/*[local-name()='Schema']";
                        List<string> qualitifiedNamespaces = MetadataHelper.GetPropertyValues(context, xpath, "Namespace");
                        List<string> qualitifiedAliases = MetadataHelper.GetPropertyValues(context, xpath, "Alias");
                        if (root.Attributes["type", Constants.NSMetadata] != null)
                        {
                            string typeName = root.Attributes["type", Constants.NSMetadata].Value;
                            if (!typeName.Contains("."))
                            {
                                typeName = "Edm." + typeName;
                            }

                            if ((this.IsContainsSpecialStrings(qualitifiedNamespaces, typeName) || this.IsContainsSpecialStrings(qualitifiedAliases, typeName))
                            || EdmTypeManager.IsEdmSimpleType(typeName))
                            {
                                passed = true;
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
    }
}
