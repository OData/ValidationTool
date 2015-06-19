// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
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
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4336 : ExtensionRule
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
                return "Metadata.Core.4336";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The path can traverse containment navigation properties or complex properties before ending in a containment navigation property, but there MUST not be any non-containment navigation properties prior to the final segment.";
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
                return "13.4.2";
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
        /// Verify Metadata.Core.4336
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
            string xPath = "//*[local-name()='NavigationPropertyBinding']";
            var navigationPropertyBindingElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            if (null == navigationPropertyBindingElems || !navigationPropertyBindingElems.Any())
            {
                return passed;
            }

            navigationPropertyBindingElems = navigationPropertyBindingElems
                .Where(elem => null != elem.Attribute("Path"))
                .Select(elem => elem);
            if (null == navigationPropertyBindingElems || !navigationPropertyBindingElems.Any())
            {
                return passed;
            }

            foreach (var navigationPropertyBindingElem in navigationPropertyBindingElems)
            {
                string pathAttribVal = navigationPropertyBindingElem.GetAttributeValue("Path");
                int slashIndex = pathAttribVal.LastIndexOf('/');
                string navigationPropertyName = pathAttribVal.Remove(0, slashIndex + 1);
                string nTypeShortName = string.Empty;
                if (slashIndex != -1)
                {
                    nTypeShortName = pathAttribVal.Substring(0, slashIndex).GetLastSegment();
                }

                var parentElement = navigationPropertyBindingElem.Parent;
                if (null == parentElement)
                {
                    return passed;
                }

                string entityTypeShortName = string.Empty;
                if ("EntitySet" == parentElement.Name.LocalName && null != parentElement.Attribute("EntityType"))
                {
                    entityTypeShortName = parentElement.GetAttributeValue("EntityType").GetLastSegment();
                    if (slashIndex == -1)
                    {
                        nTypeShortName = entityTypeShortName;
                    }

                }
                else if ("Singleton" == parentElement.Name.LocalName && null != parentElement.Attribute("Type"))
                {
                    entityTypeShortName = parentElement.GetAttributeValue("Type").GetLastSegment();
                    if (slashIndex == -1)
                    {
                        nTypeShortName = entityTypeShortName;
                    }
                }

                if (!entityTypeShortName.IsSpecifiedEntityTypeShortNameExist())
                {
                    return passed;
                }

                var navigTreeNode = NavigateTreeNode.Parse(entityTypeShortName);
                var node = navigTreeNode.Search(navigationPropertyName, nTypeShortName);
                while (null != node && null != node.Parent)
                {
                    if (node.Parent.Data.ContainsTarget || "root" == node.Parent.Data.Name)
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        break;
                    }

                    node = node.Parent;
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
