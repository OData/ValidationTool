// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
    using System;
    using System.ComponentModel.Composition;
    using System.Collections.Generic;
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
    public class MetadataCore4353 : ExtensionRule
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
                return "Metadata.Core.4353";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the return type is not an entity or a collection of entities, a value MUST NOT be defined for the EntitySet attribute.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "13.6.3";
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
        /// Verify Metadata.Core.4353
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
                bool EntitySetAttNoValue = false;
                if (functionImportElem.Attribute("EntitySet") == null ||
                        functionImportElem.Attribute("EntitySet") != null && string.IsNullOrEmpty(functionImportElem.Attribute("EntitySet").Value))
                {
                    EntitySetAttNoValue = true;
                }

                if (null != functionImportElem.Attribute("Function"))
                {
                    XElement functionXElemnt = MetadataHelper.GetTypeDefinitionEleInScope("Function", functionImportElem.Attribute("Function").Value, context);
                    xPath = string.Format("//*[local-name()='Function' and @Name='{0}']/*[local-name()='ReturnType']", functionXElemnt.Attribute("Name").Value);
                    XElement returnTypeXEle = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    if (functionXElemnt != null && returnTypeXEle == null)
                    {
                        passed = EntitySetAttNoValue;
                        if(passed == false)
                        {
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                        continue;
                    }

                    if (functionXElemnt != null && returnTypeXEle != null)
                    {

                        string returnType = returnTypeXEle.Attribute("Type").Value;
                        returnType = returnType.RemoveCollectionFlag();

                        XElement entityType = MetadataHelper.GetTypeDefinitionEleInScope("EntityType", returnType, context);

                        XElement entitySetType = MetadataHelper.GetTypeDefinitionEleInScope("EntitySet", returnType, context);

                        if (entitySetType == null && entityType == null)
                        {
                            passed = EntitySetAttNoValue;
                        }

                    }

                    if(passed == false)
                    {
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                        break;
                    }
                }
            }

            return passed;
        }
    }
}
