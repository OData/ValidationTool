// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
    using System;
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
    public class MetadataCore4327 : ExtensionRule
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
                return "Metadata.Core.4327";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the navigation property is defined on a subtype, the path attribute MUST contain the QualifiedName of the subtype, followed by a forward slash, followed by the navigation property name.";
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
                return "13.4.1";
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
        /// Verify Metadata.Core.4327
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
            foreach (XElement navigationPropertyBindingElem in navigationPropertyBindingElems)
            {
                if (this.IsDefinedOnSubType(navigationPropertyBindingElem))
                {
                    string pathAttribValue = navigationPropertyBindingElem.GetAttributeValue("Path");
                    string[] separations = pathAttribValue.Split('/');
                    xPath = string.Format(
                        "//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty' and @Name='{1}']",
                        separations[0].GetLastSegment(), separations[1]);
                    var navigationPropertyElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    if (null != navigationPropertyElem)
                    {
                        string navigationPropertyName = navigationPropertyElem.GetAttributeValue("Name");
                        string entityTypeShortName = navigationPropertyElem.Parent.GetAttributeValue("Name");
                        var aliasAndNs = navigationPropertyElem.GetAliasAndNamespace();
                        if (pathAttribValue == string.Format("{0}.{1}/{2}", aliasAndNs.Alias, entityTypeShortName, navigationPropertyName) ||
                            pathAttribValue == string.Format("{0}.{1}/{2}", aliasAndNs.Namespace, entityTypeShortName, navigationPropertyName))
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            break;
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

        /// <summary>
        /// Verify whether the navigation property is defined on a sub-type of the current entity-type.
        /// </summary>
        /// <param name="navigationPropertyBinding">The NavigationPropertyBinding element in the EntitySet/Singleton elements.</param>
        /// <returns>Returns the verification result.</returns>
        private bool IsDefinedOnSubType(XElement navigationPropertyBinding)
        {
            if (null == navigationPropertyBinding)
            {
                return false;
            }

            string pathAttribValue =
               null != navigationPropertyBinding.Attribute("Path") ?
               navigationPropertyBinding.GetAttributeValue("Path") : string.Empty;
            if (string.IsNullOrEmpty(pathAttribValue))
            {
                return false;
            }

            string[] separations = pathAttribValue.Split('/');
            if (separations.Length < 2)
            {
                return false;
            }

            string xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']", separations[0].GetLastSegment());
            var entityTypeElem = navigationPropertyBinding.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != entityTypeElem && null != entityTypeElem.Attribute("BaseType"))
            {
                return true;
            }

            return false;
        }
    }
}
