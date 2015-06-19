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
    public class MetadataCore4117 : ExtensionRule
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
                return "Metadata.Core.4117";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The type of the partner navigation property MUST be the containing entity type of the current navigation property or one of its parent entity types.";
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
        /// Verify Metadata.Core.4117
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
            var partnerNavigationPropertyElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                .Where(np => null != np.Attribute("Partner"))
                .Select(np => np);
            foreach (var partnerNavigationPropertyElem in partnerNavigationPropertyElems)
            {
                var partnerVal = partnerNavigationPropertyElem.GetAttributeValue("Partner");
                if (null != partnerNavigationPropertyElem.Attribute("Type"))
                {
                    var typeVal = partnerNavigationPropertyElem.GetAttributeValue("Type");
                    string entityTypeShortName = typeVal.RemoveCollectionFlag().GetLastSegment();
                    string @namespace = typeVal.RemoveCollectionFlag().RemoveEnd("." + entityTypeShortName);
                    var entityTypeShortNames = entityTypeShortName.GetEntityTypeShortNamesFromDerivedType();
                    entityTypeShortNames.Add(entityTypeShortName);
                    XElement navigPropElem = null;
                    foreach (var name in entityTypeShortNames)
                    {
                        xPath = string.Format("//*[local-name()='Schema' and @Namespace='{0}']/*[local-name()='EntityType' and @Name='{1}']/*[local-name()='NavigationProperty' and @Name='{2}']", @namespace, name, partnerVal);
                        navigPropElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                        if (null != navigPropElem)
                        {
                            break;
                        }
                    }

                    if(null == navigPropElem || null == navigPropElem.Attribute("Type"))
                    {
                        continue;
                    }

                    string partnerNavigPropTypeShortName = navigPropElem.GetAttributeValue("Type").RemoveCollectionFlag().GetLastSegment();
                    var parentElem = partnerNavigationPropertyElem.Parent;
                    if(null != parentElem && "EntityType" == parentElem.Name.LocalName && null != parentElem.Attribute("Name"))
                    {
                        string pEntityTypeShortName = parentElem.GetAttributeValue("Name");
                        var pEntityTypeShortNames = pEntityTypeShortName.GetEntityTypeShortNamesFromBaseType();
                        pEntityTypeShortNames.Add(pEntityTypeShortName);
                        if (pEntityTypeShortNames.Contains(partnerNavigPropTypeShortName))
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
