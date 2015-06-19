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
    public class MetadataCore4145 : ExtensionRule
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
                return "Metadata.Core.4145";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The value of the ReferencedProperty attribute MUST be a path expression resolving to a primitive property of the principal entity type itself or to a primitive property of a complex property (recursively) of the principal entity type that MUST have the same data type as the property of the dependent entity type.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "7.2.2";
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
        /// Verify Metadata.Core.4145
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
                            string dependentPropName = referentialConstraintElem.Attribute("Property").Value;
                            string includingTypeName = referentialConstraintElem.Parent.Parent.Attribute("Name").Value;

                            XElement dependentProperty = metadata.XPathSelectElement(string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and @Name='{1}']", includingTypeName, dependentPropName));

                            string dependentTypeName = dependentProperty.GetAttributeValue("Type");

                            if (!dependentTypeName.StartsWith("Edm."))
                            {
                                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                return false;
                            }

                            string principalPropPath = referentialConstraintElem.Parent.Attribute("Type").Value;
                            principalPropPath += "/" + referentialConstraintElem.Attribute("ReferencedProperty").Value;

                            XmlNode principalProp;

                            if (MetadataHelper.ResolveToProperty(principalPropPath, xmlDoc, out principalProp))
                            {
                                if (principalProp.Attributes["Type"] != null && principalProp.Attributes["Type"].Value.Equals(dependentTypeName))
                                {
                                    passed = true;
                                }
                                else
                                {
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                    passed = false;
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
