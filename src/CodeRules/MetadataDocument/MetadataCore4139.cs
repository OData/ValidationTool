// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using ODataValidator.Rule.Helper;
    #endregion

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4139 : ExtensionRule
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
                return "Metadata.Core.4139";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If both the navigation property and the principal property are not nullable, then the dependent property MUST be marked with the Nullable=\"false\" attribute value.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "7.2";
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
                return RuleEngine.PayloadType.Metadata;
            }
        }

        /// <summary>
        /// Gets the offline context to which the rule applies
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
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
        /// Verify Metadata.Core.4139
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

            string mergedMetadata = context.ContainsExternalSchema ? context.MergedMetadataDocument : context.MetadataDocument;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(mergedMetadata);

            var metadata = XElement.Parse(context.MetadataDocument);
            string xPath = "//*[local-name()='ReferentialConstraint']";
            var referentialConstraintElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            if (null != referentialConstraintElems && referentialConstraintElems.Any())
            {
                foreach (var referentialConstraintElem in referentialConstraintElems)
                {
                    if (null != referentialConstraintElem.Attribute("ReferencedProperty"))
                    {
                        string propertyVal = referentialConstraintElem.GetAttributeValue("ReferencedProperty");
                        if (!string.IsNullOrEmpty(propertyVal))
                        {
                            XElement NavigationProp = referentialConstraintElem.Parent;

                            bool navigationNullable = true;
                            string navigationNullableValue = NavigationProp.GetAttributeValue("Nullable");

                            // Since this navigation property's type specifies a single entity type not a collection, its Nullable attribute defaults to true.
                            if (!string.IsNullOrEmpty(navigationNullableValue) && navigationNullableValue.Equals("false"))
                            {
                                navigationNullable = false;
                            }

                            bool principalPropNullable = true;

                            string principalPropPath = NavigationProp.Attribute("Type").Value;
                            principalPropPath += "/" + referentialConstraintElem.Attribute("ReferencedProperty").Value;

                            XmlNode principalProp;

                            if (MetadataHelper.ResolveToProperty(principalPropPath, xmlDoc, out principalProp))
                            {
                                // Since this principal property is of primitive type not a collection, its Nullable attribute defaults to true.
                                if (principalProp.Attributes["Nullable"] != null && principalProp.Attributes["Nullable"].Value.Equals("false"))
                                {
                                    principalPropNullable = false;
                                }
                            }
                            else
                            {
                                continue;
                            }

                            string dependentPropName = referentialConstraintElem.Attribute("Property").Value;
                            string includingTypeName = NavigationProp.Parent.Attribute("Name").Value;
                            XElement dependentProperty = metadata.XPathSelectElement(string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and @Name='{1}']", includingTypeName, dependentPropName));

                            if (dependentProperty != null)
                            {
                                if (!navigationNullable && !principalPropNullable)
                                {
                                    string dependentPropNullableValue = dependentProperty.GetAttributeValue("Nullable");
                                    if (dependentPropNullableValue != null && dependentPropNullableValue.Equals("false"))
                                    {
                                        passed = true;
                                    }
                                    else
                                    {
                                        passed = false;
                                    }
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
