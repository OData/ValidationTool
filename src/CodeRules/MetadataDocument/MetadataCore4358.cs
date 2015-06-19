// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Metadata.Core.4358
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4358 : ExtensionRule
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
                return "Metadata.Core.4358";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The edm:Term element MUST include a Type attribute whose value is a TypeName.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "14.1.2";
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
        /// Gets the OData version
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
            }
        }

        /// <summary>
        /// Verify Metadata.Core.4358
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

            string termXpath = @"./*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='Term']";
            XElement metadata = XElement.Parse(context.MetadataDocument);
            IEnumerable<XElement> termCollection = metadata.XPathSelectElements(termXpath);
            foreach (XElement term in termCollection)
            {
                passed = true;
                if (term.Attribute("Type") != null)
                {
                    string ns;
                    string shortName;
                    bool isCollection;

                    if (!parseTypeName(term.Attribute("Type").Value, out isCollection, out ns, out shortName))
                    {
                        passed = false; break;
                    }

                    if ((ns + "." + shortName).IsPrimitiveTypeName()||(ns + "." + shortName).IsBuiltinAbstarctTypeName(true))
                    {
                        continue;
                    }

                    if (context.ContainsExternalSchema)
                    {
                        XElement fullMetadata = XElement.Parse(context.MergedMetadataDocument);
                        string xpath = @"./*[local-name()='DataServices']/*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{0}')]/*[(local-name()='EntityType' or local-name()='ComplexType' or local-name()='EnumType' or local-name()='TypeDefinition') and @Name='{1}']";
                        if (fullMetadata.XPathSelectElement(string.Format(xpath, ns, shortName)) == null)
                        {
                            passed = false; break;
                        }
                    }
                    else
                    {
                        string xpath = @"./*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{0}')]/*[(local-name()='EntityType' or local-name()='ComplexType' or local-name()='EnumType' or local-name()='TypeDefinition') and @Name='{1}']";
                        if (term.Parent.Parent.XPathSelectElement(string.Format(xpath, ns, shortName)) == null)
                        {
                            passed = false; break;
                        }
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }

        private bool parseTypeName(string typeValue, out bool isCollection, out string nsOrAlias, out string shortName)
        {
            isCollection = false;
            nsOrAlias = string.Empty;
            shortName = string.Empty;

            Regex reg = new Regex("^Collection(.*)$");
            Match matchResult = reg.Match(typeValue.Trim());
            if (matchResult.Success)
            {
                isCollection = true;
                typeValue = typeValue.Substring(11, typeValue.Trim().Length - 12);
            }

            int dotIndex = typeValue.LastIndexOf('.');
            if (dotIndex == -1 || dotIndex == 0 || dotIndex == typeValue.Length - 1)
            {
                return false;
            }

             nsOrAlias = typeValue.Substring(0, dotIndex).TrimStart();
             shortName = typeValue.Substring(dotIndex + 1).TrimEnd();
             return true;
        }

    }
}

