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
    /// Extension rule to verify v3 and v4 Json light individual property:
    /// A property that is of complex type are represented as a complex value.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IndividualPropertyCore4204 : ExtensionRule
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
                return "IndividualProperty.Core.4204";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"In individual property, a property that is of complex type are represented as a complex value.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11";
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
            string propertyType = string.Empty;
            string formatLiteral = string.Empty;
            List<string> complexTypeValues = new List<string>();

            JObject jo;
            context.ResponsePayload.TryToJObject(out jo);
            string contextUrl = string.Empty;

            if (jo[Constants.OdataV4JsonIdentity] == null)
            {
                passed = false;
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

                return passed;
            }
            else
            {
                // Use the XPath query language to access the metadata document and get all Namespace and alias values.
                string xpath = @"//*[local-name()='DataServices']/*[local-name()='Schema']";
                XElement metadata = XElement.Parse(context.MetadataDocument);
                var schemaElement = metadata.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
                string nameSpace = schemaElement.GetAttributeValue("Namespace");
                string alias = schemaElement.GetAttributeValue("Alias");

                complexTypeValues = MetadataHelper.GetAllComplexNameFromMetadata(context.MetadataDocument);

                contextUrl = jo[Constants.OdataV4JsonIdentity].Value<string>().StripOffDoubleQuotes();
                string[] feedSegment = contextUrl.Remove(0, contextUrl.IndexOf(Constants.JsonFeedIdentity) + Constants.JsonFeedIdentity.Length).Split('/');
                string entitySetName = feedSegment[0].Split('(')[0];
                string entityTypeName = XmlHelper.GetEntityTypeShortName(entitySetName, context.MetadataDocument);
                var normalProperties = MetadataHelper.GetAllPropertiesOfEntity(context.MetadataDocument, entityTypeName, MatchPropertyType.Normal);

                Dictionary<string, string> propertyNameAndComplexTypeName = new Dictionary<string, string>();

                foreach (XElement xe in normalProperties)
                {
                    if ((!string.IsNullOrEmpty(nameSpace) && xe.GetAttributeValue("Type").StartsWith(nameSpace))
                        || (!string.IsNullOrEmpty(alias) && xe.GetAttributeValue("Type").StartsWith(alias)))
                    {
                        string typeShortName = xe.GetAttributeValue("Type").Remove(0, xe.GetAttributeValue("Type").LastIndexOf('.') + 1);

                        if (complexTypeValues.Contains(typeShortName))
                        {
                            propertyNameAndComplexTypeName.Add(xe.GetAttributeValue("Name"), typeShortName);
                        }
                    }
                }

                 string propertyName = feedSegment[1];

                 if (propertyNameAndComplexTypeName.ContainsKey(propertyName))
                 {
                     List<string> propertiesOfComplexType = MetadataHelper.GetAllPropertiesNamesOfComplexType(context.MetadataDocument, propertyNameAndComplexTypeName[propertyName]);

                     foreach (JProperty jp in jo.Children())
                     {
                         if (!JsonSchemaHelper.IsAnnotation(jp.Name))
                         {
                             if (propertiesOfComplexType.Contains(jp.Name))
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
            }

            return passed;
        }
    }
}
