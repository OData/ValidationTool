// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// class of concrete code rule #276 when payload is a feed
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2012_Feed : CommonCore2012_Entry
    {
        /// <summary>
        /// Gets the payload type
        /// </summary>
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Feed; }
        }
    }

    /// <summary>
    /// class of concrete code rule #276 when payload is an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2012_Entry : CommonCore2012
    {
        /// <summary>
        /// Gets the payload type
        /// </summary>
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Entry; }
        }

        /// <summary>
        /// Gets the payload format
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
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

            info = null;
            bool? passed = true;

            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));

            XElement payload = XElement.Parse(context.ResponsePayload);

            // for each property as XML element value, make sure it has either custom XML namnespace or the default one - in other word, it is not null or empty
            string xpath = "//m:properties/../..";
            var entries = payload.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var entry in entries)
            {
                if (!VerifyEntry(entry, edmxHelper, context.Destination, out info))
                {
                    passed = false;
                    break;
                }
            }

            return passed;
        }

        private bool VerifyEntry(XElement entry, EdmxHelper edmxHelper, Uri uriTarget, out ExtensionRuleViolationInfo info)
        {
            bool result = true;
            info = null;

            var category = entry.XPathSelectElement("./*[local-name()='category']");
            if (category != null)
            {
                string categoryTerm;
                categoryTerm = category.GetAttributeValue("term");
                if (!string.IsNullOrEmpty(categoryTerm))
                {
                    EntityType entityType;
                    if (edmxHelper.TryGetItem(categoryTerm, out entityType))
                    {
                        var properties = entry.XPathSelectElements("./*/m:properties/*", ODataNamespaceManager.Instance);
                        foreach (var property in properties)
                        {
                            var propertyName = property.Name.LocalName;

                            var metaProperty = from p in entityType.Properties where p.Name == propertyName select p;
                            if (metaProperty != null && metaProperty.Any())
                            {
                                var propertyType = metaProperty.First().TypeUsage.EdmType;
                                if (propertyType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
                                {
                                    if (!VerifyValueByPrimitiveType(property.Value, propertyType.FullName))
                                    {
                                        info = new ExtensionRuleViolationInfo("invalid formatted value for type " + propertyType.FullName, uriTarget, property.Value);
                                        result = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// class of concrete code rule #276 when payload is a property
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2012_Property : CommonCore2012
    {
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Property; }
        }

        /// <summary>
        /// Gets the payload format
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }

        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
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

            info = null;
            bool? passed = null;

            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            var segments = ResourcePathHelper.GetPathSegments(context);
            UriType uriType;
            var target = edmxHelper.GetTargetType(segments, out uriType);

            if (uriType == UriType.URI3)
            {
                ComplexType complexType = (ComplexType)((EdmProperty)target).TypeUsage.EdmType;
                XElement payload = XElement.Parse(context.ResponsePayload);
                passed = VerifyComplexType(payload, complexType, context.Destination, out info);
            }
            else if (uriType == UriType.URI4 || uriType == UriType.URI5)
            {
                PrimitiveType propertyType = (PrimitiveType)((EdmProperty)target).TypeUsage.EdmType;
                XElement property = XElement.Parse(context.ResponsePayload);
                passed = VerifyValueByPrimitiveType(property.Value, propertyType.FullName); 
                if (!passed.Value)
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo("invalid formatted value for type " + propertyType.FullName, context.Destination, property.Value);
                }
            }

            return passed;
        }

        private bool VerifyComplexType(XElement payload, ComplexType complexType, Uri uriTarget, out ExtensionRuleViolationInfo info)
        {
            bool result = true;
            info = null;

            string xpathToProperty = "/*";
            var properties = payload.XPathSelectElements(xpathToProperty, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                var name = property.Name.LocalName;
                var typeOfProperty = from p in complexType.Properties where p.Name == name select p;
                if (typeOfProperty != null && typeOfProperty.Any() )
                {
                    var propType = typeOfProperty.First().TypeUsage.EdmType;
                    if (propType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
                    {
                        bool isWellFormatted = VerifyValueByPrimitiveType(property.Value, propType.FullName);
                        if (!isWellFormatted)
                        {
                            info = new ExtensionRuleViolationInfo("invalid formatted value for type " + propType.FullName, uriTarget, property.Value);
                            result = false;
                            break;
                        }
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Abstract base class of rule #276
    /// </summary>
    public abstract class CommonCore2012 : ExtensionRule
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
                return "Common.Core.2012";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"All EDM Primitive types represented as XML element values MUST be formatted as defined by the rules in the following EDM Primitive Type Formats for Element Values table.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.1";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "7.1";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odataatom";
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
        /// Gets the aspect property.
        /// </summary>
        public override string Aspect
        {
            get
            {
                return "semantic";
            }
        }

        /// <summary>
        /// Checks whether the value is well-formatted based on the type
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="typeName">Name of the type</param>
        /// <returns>bool indicating value being well-formatted</returns>
        protected bool VerifyValueByPrimitiveType(string value, string typeName)
        {
            bool result = true;

            if (!string.IsNullOrEmpty(value))
            {
                IEdmType type = EdmTypeManager.GetEdmType(typeName);
                if (type == null)
                {
                    //unrecognized primitive type
                    result = false;
                }
                else
                {
                    result = type.IsGoodWith(value);
                }
            }

            return result;
        }
    }
}
