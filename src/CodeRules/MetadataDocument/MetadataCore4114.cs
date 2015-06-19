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
    public class MetadataCore4114 : ExtensionRule
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
                return "Metadata.Core.4114";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the Partner attribute is specified, the value of this attribute MUST be a path from the entity type specified in the Type attribute to a navigation property defined on that type or a derived type.";
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
        /// Verify Metadata.Core.4114
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
            string xPath = "//*[local-name()='EntityType']/*[local-name()='NavigationProperty']";
            var partnerNavigationPropertyElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                .Where(np => null != np.Attribute("Partner") && null != np.Attribute("Type"))
                .Select(np => np);
            foreach (var partnerNavigationPropertyElem in partnerNavigationPropertyElems)
            {
                var partnerVal = partnerNavigationPropertyElem.GetAttributeValue("Partner");
                var typeVal = partnerNavigationPropertyElem.GetAttributeValue("Type");
                string typeShortName = typeVal.RemoveCollectionFlag().GetLastSegment();
                string @namespace = typeVal.RemoveCollectionFlag().RemoveEnd("." + typeShortName);
                var typeShortNames = typeShortName.GetTypeShortNamesFromDerivedType();
                typeShortNames.Add(typeShortName);
                XElement navigPropElem = null;
                foreach (var name in typeShortNames)
                {
                    xPath = string.Format("//*[local-name()='Schema' and (@Namespace='{0}' or @Alias={0})]/*[local-name()='EntityType' and @Name='{1}']/*[local-name()='NavigationProperty' and @Name='{2}']", @namespace, name, partnerVal);
                    navigPropElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    if (null != navigPropElem)
                    {
                        break;
                    }
                }

                if (null != navigPropElem)
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    break;
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
