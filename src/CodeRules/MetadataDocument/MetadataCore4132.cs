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
    public class MetadataCore4132 : ExtensionRule
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
                return "Metadata.Core.4132";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the containment is recursive, the partner navigation property MUST be specify a single entity type.";
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
                return "7.1.5";
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
        /// Verify Metadata.Core.4132
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
            string xPath = "//*[local-name()='EntityType']";
            var entityTypeElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                .Where(et => null != et.Attribute("Name"))
                .Select(et => et);
            if (null != entityTypeElems && entityTypeElems.Any())
            {
                foreach (var entityTypeElem in entityTypeElems)
                {
                    string entityTypeShortName = entityTypeElem.GetAttributeValue("Name");
                    string entityTypeFullName = entityTypeShortName.AddNamespace(AppliesToType.EntityType);
                    xPath = "./*[local-name()='NavigationProperty']";
                    var navigPropElems = entityTypeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                        .Where(np => null != np.Attribute("Type") && null != np.Attribute("Partner") && null != np.Attribute("ContainsTarget"))
                        .Select(np => np);
                    if (null != navigPropElems && navigPropElems.Any())
                    {
                        foreach (var navigPropElem in navigPropElems)
                        {
                            string typeVal = navigPropElem.GetAttributeValue("Type");

                            // Verify the recursive element.
                            if (typeVal.Contains(entityTypeFullName) && 
                                Convert.ToBoolean(navigPropElem.GetAttributeValue("ContainsTarget")))
                            {
                                if (!typeVal.StartsWith("Collection("))
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

                    if (false == passed)
                    {
                        break;
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
