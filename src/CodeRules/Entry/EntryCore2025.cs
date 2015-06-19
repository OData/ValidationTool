// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of rule #1140
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2025 : ExtensionRule
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
                return "Entry.Core.2025";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If an EntityType instance included in a Data Service response contains a mapped property that has a value of null, then the element being mapped to can still be present and MUST have empty content.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "3.2.5.2.1";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.1";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odatacsdl";
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
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
            }
        }

        /// <summary>
        /// Verify rule logic
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

            // check each appearance of mapped property which has null value
            string xpath = @"//m:properties/*[@m:null='true']";
            var nullProps = XElement.Parse(context.ResponsePayload).XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            if (nullProps.Any())
            {
                XElement meta = XElement.Parse(context.MetadataDocument);

                foreach (var nullProp in nullProps)
                {
                    var propertyName = nullProp.Name.LocalName;
                    var entry = GetEntryNode(nullProp);
                    var category = entry.XPathSelectElement("./*[local-name()='category' and @term]");
                    if (category != null)
                    {
                        var entityType = category.GetAttributeValue("term");

                        // to check whether the property is mapped and where it is mapped to
                        string targetPath;
                        bool propertyIsMapped = PropertyIsMapped(meta, entityType, propertyName, out targetPath);
                        if (propertyIsMapped)
                        {
                            string mappedTarget;
                            if (AtomTargetMapping.TryGetTarget(targetPath, out mappedTarget))
                            {
                                string xpath2Content = string.Format(@"./{0}", mappedTarget);
                                var mappedContent = entry.XPathSelectElement(xpath2Content, ODataNamespaceManager.Instance);
                                if (mappedContent != null)
                                {
                                    // to check the rule to make sure the element content MUST be empty
                                    passed = mappedContent.IsEmpty;
                                    if (!passed.Value)
                                    {
                                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, mappedContent.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return passed;
        }

        /// <summary>
        /// Gets the enclosing entry node from a property node
        /// </summary>
        /// <param name="nodeProperty">The property node</param>
        /// <returns>The enclosing entry node</returns>
        private static XElement GetEntryNode(XElement nodeProperty)
        {
            XElement result = null;
            result = nodeProperty.XPathSelectElement("./../..");
            if (!string.Equals(result.Name.LocalName, "entry",  StringComparison.Ordinal))
            {
                result = nodeProperty.XPathSelectElement("./../../..");
                if (!string.Equals(result.Name.LocalName, "entry", StringComparison.Ordinal)) 
                {
                    result = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether a property is mapped in the entity type definition
        /// </summary>
        /// <param name="meta">The metadata document</param>
        /// <param name="entityType">The entity type</param>
        /// <param name="propertyName">The property</param>
        /// <param name="mappedTarget">Output parameter of mapped target path</param>
        /// <returns>flag of proerty being mapped</returns>
        private static bool PropertyIsMapped(XElement meta, string entityType, string propertyName, out string mappedTarget)
        {
            const string tmplXPath2EntityType = @"//*[local-name()='EntityType' and @Name = '{0}']";
            const string tmplXPath2Property = @"./*[local-name()='Property' and @Name='{0}']";

            string typeShortName = ResourcePathHelper.GetBaseName(entityType);
            mappedTarget = null;

            string xPath2EntityType = string.Format(tmplXPath2EntityType, typeShortName);
            var nodeEntityType = meta.XPathSelectElement(xPath2EntityType, ODataNamespaceManager.Instance);

            string xpath2prop = string.Format(tmplXPath2Property, propertyName);
            var nodeProperty = nodeEntityType.XPathSelectElement(xpath2prop);
            if (nodeProperty != null)
            {
                string attrFC_KeepInContent = nodeProperty.GetAttributeValue("m:FC_KeepInContent", ODataNamespaceManager.Instance);
                if (!string.IsNullOrEmpty(attrFC_KeepInContent))
                {
                    mappedTarget = nodeProperty.GetAttributeValue("m:FC_TargetPath", ODataNamespaceManager.Instance);
                    return !Convert.ToBoolean(attrFC_KeepInContent);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                string baseType = nodeEntityType.GetAttributeValue("BaseType");
                return PropertyIsMapped(meta, baseType, propertyName, out mappedTarget);
            }
        }
    }
}

