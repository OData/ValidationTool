// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4262 : ExtensionRule
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
                return "Metadata.Core.4262";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "Functions MUST return a single instance or a collection of instances of any type.";
            }
        }

        /// <summary>
        /// Gets the odata version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "12.2";
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
                return RuleEngine.PayloadFormat.Xml;
            }
        }

        /// <summary>
        /// Verify Metadata.Core.4262
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
            var metadata = XElement.Parse(context.MetadataDocument);
            string xPath = "//*[local-name()='Function']/*[local-name()='ReturnType']";
            var returnTypeElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            if (null != returnTypeElems && returnTypeElems.Any())
            {
                foreach (var returnTypeElem in returnTypeElems)
                {
                    if (null != returnTypeElem.Attribute("Type"))
                    {
                        string typeFullName = returnTypeElem.GetAttributeValue("Type").RemoveCollectionFlag();
                        string typeShortName = typeFullName.GetLastSegment();
                        int lastDotIndex = typeFullName.LastIndexOf('.');
                        string aliasOrNamespace = typeFullName.Remove(lastDotIndex, typeFullName.Length - lastDotIndex);
                        if (typeFullName.StartsWith("Edm."))
                        {
                            if (PrimitiveDataTypes.NonQualifiedTypes.Contains(typeShortName))
                            {
                                passed = true;
                                continue;
                            }
                            else
                            {
                                passed = false;
                                break;
                            }
                        }

                        string pattern = "//*[local-name()='Schema' and (@Alias='{0}' or @Namespace='{0}')]/*[(local-name()='EntityType' or local-name()='ComplexType' or local-name()='EnumType') and @Name='{1}']";
                        xPath = string.Format(pattern, aliasOrNamespace, typeShortName);
                        var instance = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                        if (null != instance)
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            break;
                        }
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
