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
    public class MetadataCore4120 : ExtensionRule
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
                return "Metadata.Core.4120";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If a partner navigation property is specified, this partner navigation property MUST either specify the current navigation property as its partner to define a bi-directional relationship or it MUST NOT specify a partner attribute.";
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
                return "7.1.4";
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
        /// Verify Metadata.Core.4120
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
            string xPath = "//*[local-name()='NavigationProperty']";
            var navigationPropertyElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                .Where(np => null != np.Attribute("Partner") && null != np.Attribute("Type") && null != np.Attribute("Name"))
                .Select(np => np);
            if (null != navigationPropertyElems && navigationPropertyElems.Any())
            {
                foreach (var navigationPropertyElem in navigationPropertyElems)
                {
                    string navigationPropertyName = navigationPropertyElem.GetAttributeValue("Name");
                    string typeShortName1 = navigationPropertyElem.GetAttributeValue("Type").RemoveCollectionFlag().GetLastSegment();
                    string partnerNavigationPropertyName1 = navigationPropertyElem.GetAttributeValue("Partner");
                    xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{0}']/*[local-name()='NavigationProperty' and @Name='{1}']", typeShortName1, partnerNavigationPropertyName1);
                    var npElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    if (null != npElem.Parent)
                    {
                        var parentElem = npElem.Parent;
                        var derivedList = parentElem.GetAttributeValue("Name").GetEntityTypeShortNamesFromDerivedType();
                        if (!derivedList.Any() && "ComplexType" != parentElem.Name.LocalName)
                        {
                            string partnerNavigationPropertyName2 = null != npElem.Attribute("Partner") ? npElem.GetAttributeValue("Partner") : string.Empty;
                            if (partnerNavigationPropertyName2 == navigationPropertyName)
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
