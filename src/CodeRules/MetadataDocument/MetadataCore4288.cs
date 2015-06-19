// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
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
    public class MetadataCore4288 : ExtensionRule
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
                return "Metadata.Core.4288";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "A navigation segment of the EntitySetPath attribute's value names the SimpleIdentifier of the navigation property in an edm:Function element.";
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
                return "12.2.4";
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
        /// Verify Metadata.Core.4288
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
            string xPath = "//*[local-name()='Schema']/*[local-name()='Function' and @IsBound='true']";
            var functionElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            foreach (var functionElem in functionElems)
            {
                xPath = "./*[local-name()='Parameter']";
                var parameterElem = functionElem.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                if (null == parameterElem || null == parameterElem.Attribute("Type"))
                {
                    continue;
                }

                string entityTypeShortName = parameterElem.GetAttributeValue("Type").RemoveCollectionFlag().GetLastSegment();
                if (null != functionElem.Attribute("EntitySetPath") && !string.IsNullOrEmpty(entityTypeShortName))
                {
                    string entitySetPathVal = functionElem.GetAttributeValue("EntitySetPath");
                    string[] separations = entitySetPathVal.Split('/');
                    var navigTree = NavigateTreeNode.Parse(entityTypeShortName);
                    for (int i = 1; i < separations.Length; i++)
                    {
                        if (i > 1 && !separations[i - 1].IsTypeCastSegment())
                        {
                            if (null != navigTree.Search(separations[i - 1]))
                            {
                                entityTypeShortName = navigTree.Search(separations[i - 1]).Data.TypeShortName;
                            }
                        }
                        else if (i > 1 && separations[i - 1].IsTypeCastSegment())
                        {
                            entityTypeShortName = separations[i - 1].GetLastSegment();
                        }

                        if (null != navigTree.Search(separations[i], entityTypeShortName))
                        {
                            if (separations[i].IsSimpleIdentifier())
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
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
