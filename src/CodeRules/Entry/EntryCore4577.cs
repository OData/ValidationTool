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
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.4577
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4577 : ExtensionRule
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
                return "Entry.Core.4577";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "A property name for a complex type instance MUST be the name of the property.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "7";
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
            bool isComplexExistInMetadata = false;
            bool isObjecPropInMetadata = false;
            info = null;

            JObject entry;
            context.ResponsePayload.TryToJObject(out entry);
            string appropriatecomplexType = string.Empty;

            // Use the XPath query language to access the metadata document and get all Namespace value.
            string xpath = @"//*[local-name()='DataServices']/*[local-name()='Schema']";
            List<string> appropriateNamespace = MetadataHelper.GetPropertyValues(context, xpath, "Namespace");

            // Get Alias value.
            List<string> appropriateAlias = MetadataHelper.GetPropertyValues(context, xpath, "Alias");

            XElement metadata = XElement.Parse(context.MetadataDocument);

            // Use the XPath query language to access the metadata document and get the node which will be used.
            xpath = string.Format(@"//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", context.EntityTypeShortName);
            IEnumerable<XElement> propsEntity = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            xpath = @"//*[local-name()='Schema']/*[local-name()='ComplexType']";
            List<string> propsComplex = MetadataHelper.GetPropertyValues(context, xpath, "Name");

            if (entry != null && entry.Type == JTokenType.Object)
            {
                // Get all the properties in current entry.
                var o = (JObject)entry;
                var jProps = o.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (!JsonSchemaHelper.IsAnnotation(jProp.Name))
                    {
                        // whether the type of jProp is Object.
                        if (jProp.Value.Type.Equals(JTokenType.Object))
                        {
                            string complexType = string.Empty;

                            // Find the jPro in EntityType Property
                            foreach (var prop in propsEntity)
                            {
                                if (prop.Attribute("Name").Value.Equals(jProp.Name))
                                {
                                    isObjecPropInMetadata = true;
                                    complexType = prop.Attribute("Type").Value;
                                    break;
                                }
                                else
                                {
                                    isObjecPropInMetadata = false;
                                }
                            }

                            // Whether the Property with the Object Type is in metadata.
                            if (isObjecPropInMetadata)
                            {
                                // Split the Object type value to get the appropriate complex type name.
                                if ((appropriateNamespace.Count != 0) || (appropriateAlias.Count != 0))
                                {
                                    appropriatecomplexType = splitAliasOrNamespace(appropriateAlias, appropriateNamespace, complexType);
                                }

                                // Whether the appropriate complex type name can be found.
                                if (appropriatecomplexType != null)
                                {
                                    // Find the according complex type in EntityType Property
                                    if (propsComplex.Contains(appropriatecomplexType))
                                    {
                                        isComplexExistInMetadata = true;
                                    }
                                    else
                                    {
                                        isComplexExistInMetadata = false;
                                    }

                                    // Whether the Complex type is in metadata.
                                    if (isComplexExistInMetadata)
                                    {
                                        // Use the XPath query language to access the metadata document and get the node which will be used.
                                        xpath = string.Format("//*[local-name()='ComplexType' and @Name='{0}']/*[local-name()='Property']", appropriatecomplexType);
                                        IEnumerable<XElement> propsComplexChild = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

                                        // Find the jPro in EntityType Property
                                        foreach (var prop in propsComplexChild)
                                        {
                                            foreach (JProperty jPropChild in jProp.Value.Children())
                                            {
                                                if (!JsonSchemaHelper.IsAnnotation(jPropChild.Name))
                                                {
                                                    if (prop.Attribute("Name").Value.Equals(jPropChild.Name))
                                                    {
                                                        passed = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        passed = false;
                                                    }
                                                }
                                            }

                                            if (passed == false)
                                            {
                                                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return passed;
        }

        /// <summary>
        /// Get the appropriate complexType
        /// </summary>
        /// <param name="appropriateAlias">Alias value in metadata</param>
        /// <param name="appropriateNamespace">Namespace value in metadata</param>
        /// <param name="complexType">The complex type value</param>
        /// <returns>The appropriate complexType</returns>
        private static string splitAliasOrNamespace(List<string> appropriateAlias, List<string> appropriateNamespace, string complexType)
        {
            string appropriatecomplexType = null;

            foreach (string currentvalue in appropriateAlias)
            {
                if (complexType.Contains(currentvalue))
                {
                    appropriatecomplexType = complexType.Substring(currentvalue.Length + 1);
                    break;
                }
            }

            foreach (string currentvalue in appropriateNamespace)
            {
                if (complexType.Contains(currentvalue))
                {
                    appropriatecomplexType = complexType.Substring(currentvalue.Length + 1);
                    break;
                }
            }

            return appropriatecomplexType;
        }
    }
}
