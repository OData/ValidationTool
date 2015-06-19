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
    using System.Net;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4211 : ExtensionRule
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
                return "Metadata.Core.4211";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "A complex type derived from an open complex type MUST NOT provide a value of false for the OpenType attribute.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "9.1.4";
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
        /// Verify Metadata.Core.4211
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

            XElement metaXml = XElement.Parse(context.MetadataDocument);
            string xpath = "//*[local-name()='ComplexType']";
            IEnumerable<XElement> complexTypeElements = metaXml.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (XElement complexTypeElement in complexTypeElements)
            {
                if (complexTypeElement.Attribute("BaseType") != null)
                {
                    string baseTypeQulifiedName = complexTypeElement.Attribute("BaseType").Value;

                    XElement baseType = MetadataHelper.GetTypeDefinitionEleByDoc("ComplexType", baseTypeQulifiedName, context.MetadataDocument);

                    if (baseType == null)
                    {
                        string doc = MetadataHelper.GetReferenceDocByDefinedType(baseTypeQulifiedName, context);

                        if (!string.IsNullOrEmpty(doc))
                        {
                            baseType = MetadataHelper.GetTypeDefinitionEleByDoc("ComplexType", baseTypeQulifiedName, doc);
                        }
                    }

                    if(baseType != null)
                    {
                        if (baseType.Attribute("OpenType") != null && baseType.Attribute("OpenType").Value.Equals("true"))
                        {
                            if(complexTypeElement.Attribute("OpenType")==null ||
                               complexTypeElement.Attribute("OpenType")!=null && complexTypeElement.Attribute("OpenType").Value.Equals("true"))
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
