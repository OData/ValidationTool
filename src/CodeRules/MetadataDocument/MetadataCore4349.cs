// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4349 : ExtensionRule 
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
                return "Metadata.Core.4349";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The edm:FunctionImport's Function attribute value MUST be a QualifiedName that resolves to the name of an unbound edm:Function element in scope.";
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
                return "13.6.2";
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
        /// Verify Metadata.Core.4348
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
            string xPath = "//*[local-name()='FunctionImport']";
            var functionImportElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            foreach (var functionImportElem in functionImportElems)
            {
                if (null != functionImportElem.Attribute("Function"))
                {
                    string functionAttribVal = functionImportElem.GetAttributeValue("Function");
                    int lastDotIndex = functionAttribVal.LastIndexOf('.');
                    string aliasOrNamespace = functionAttribVal.Remove(lastDotIndex, functionAttribVal.Length - lastDotIndex);
                    string functionShortName = functionAttribVal.GetLastSegment();
                    xPath = string.Format("//*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{0}')]/*[local-name()='Function' and @Name='{1}']", aliasOrNamespace, functionShortName);
                    var functionElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    if (null != functionElem)
                    {
                        if (null != functionElem.Attribute("IsBound"))
                        {
                            passed = !Convert.ToBoolean(functionElem.GetAttributeValue("IsBound"));
                            if (false == passed)
                            {
                                break;
                            }
                        }
                        else
                        {
                            passed = true;
                        }
                    }
                    else
                    {
                        passed = false;
                        break;
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
