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
    public class MetadataCore4326 : ExtensionRule
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
                return "Metadata.Core.4326";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "A navigation property binding MUST name a navigation property of the entity set’s, singleton's, or containment navigation property's entity type or one of its subtypes in the Path attribute.";
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
        /// Verify Metadata.Core.4326
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
            string errMsgPattern = "The '{0}' element does not have the '{1}' attribute in the metadata document. So the rule 'Metadata.Core.4326' cannot be verified.";
            var metadata = XElement.Parse(context.MetadataDocument);
            string xPath = "//*[local-name()='NavigationPropertyBinding']";
            var navigationPropertyBindingElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            if (null != navigationPropertyBindingElems && navigationPropertyBindingElems.Any())
            {
                foreach (XElement navigationPropertyBindingElem in navigationPropertyBindingElems)
                {
                    var parentElem = navigationPropertyBindingElem.Parent;
                    string entityTypeShortName = string.Empty;
                    if ("EntitySet" == parentElem.Name.LocalName)
                    {
                        if (null == parentElem.Attribute("EntityType"))
                        {
                            string errMsg = string.Format(errMsgPattern, "EntitySet", "EntityType");
                            info = new ExtensionRuleViolationInfo(errMsg, context.Destination, context.ResponsePayload);

                            return false;
                        }

                        entityTypeShortName = parentElem.GetAttributeValue("EntityType").GetLastSegment();
                    }
                    else if ("Singleton" == parentElem.Name.LocalName)
                    {
                        if (null == parentElem.Attribute("Type"))
                        {
                            string errMsg = string.Format(errMsgPattern, "Singleton", "Type");
                            info = new ExtensionRuleViolationInfo(errMsg, context.Destination, context.ResponsePayload);

                            return false;
                        }

                        entityTypeShortName = parentElem.GetAttributeValue("Type").GetLastSegment();
                    }

                    if (!string.IsNullOrEmpty(entityTypeShortName))
                    {
                        if (null != navigationPropertyBindingElem.Attribute("Path"))
                        {
                            string combination = navigationPropertyBindingElem.GetAttributeValue("Path").GetLastSegment();
                            string[] separations = combination.Split('/');
                            xPath = string.Format(
                                "//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty' and @Name='{1}']",
                                separations.Length > 1 ? separations[0] : entityTypeShortName,
                                separations.Length > 1 ? separations[1] : separations[0]);
                            var navigationPropertyElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                            if (null != navigationPropertyBindingElem)
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
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
