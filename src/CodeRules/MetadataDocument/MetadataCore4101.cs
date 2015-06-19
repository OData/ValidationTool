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
    public class MetadataCore4101 : ExtensionRule
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
                return "Metadata.Core.4101";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The name of the navigation property MUST be unique within the set of structural and navigation properties of the containing structured type and any of its base types.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "7.1.1";
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
        /// Verify Metadata.Core.4101
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
            string xpath = "//*[local-name()='NavigationProperty']/parent::*";
            IEnumerable<XElement> navPropertyParentElements = metaXml.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            foreach (XElement navPropParent in navPropertyParentElements)
            {
                passed = true;

                HashSet<string> childrenSet = new HashSet<string>(StringComparer.Ordinal);

                if (!MetadataHelper.IsNameUniqueInChildren(navPropParent, ref childrenSet))
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                    break;
                }                

                if (navPropParent.Attribute("BaseType") != null)
                {
                    string baseTypeName = navPropParent.Attribute("BaseType").Value;

                    XElement baseTypeElement = MetadataHelper.GetTypeDefinitionEleByDoc(navPropParent.Name.LocalName, baseTypeName, context.MetadataDocument);

                    if(baseTypeElement!=null)
                    {
                        if (!MetadataHelper.IsNameUniqueInChildren(baseTypeElement, ref childrenSet))
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }

                    }
                    else
                    {
                        string docString = MetadataHelper.GetReferenceDocByDefinedType(baseTypeName, context);

                        if(!string.IsNullOrEmpty(docString))
                        {
                            baseTypeElement = MetadataHelper.GetTypeDefinitionEleByDoc(navPropParent.Name.LocalName, baseTypeName, docString);

                            if(baseTypeElement!=null)
                            {
                                if (!MetadataHelper.IsNameUniqueInChildren(baseTypeElement, ref childrenSet))
                                {
                                    passed = false;
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                    break;
                                }

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
