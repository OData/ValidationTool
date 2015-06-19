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
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4130 : ExtensionRule
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
                return "Metadata.Core.4130";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "Containment navigation properties MUST NOT be specified as the last path segment in the Path attribute of a navigation property binding.";
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
                return RequirementLevel.MustNot;
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
        /// Verify Metadata.Core.4130
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
            var navigPropBindingElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                .Where(npb => null != npb.Attribute("Path"))
                .Select(npb => npb);
            if (null != navigPropBindingElems && navigPropBindingElems.Any())
            {
                foreach (var navigPropBindingElem in navigPropBindingElems)
                {
                    var parentElem = navigPropBindingElem.Parent;
                    string typeShortName = string.Empty;
                    if (null != parentElem.Attribute("EntityType") || null != parentElem.Attribute("Type"))
                    {
                        typeShortName = "EntitySet" == parentElem.Name.LocalName ? 
                            parentElem.GetAttributeValue("EntityType") :
                            parentElem.GetAttributeValue("Type");
                        typeShortName = typeShortName.GetLastSegment();
                    }

                    string pathVal = navigPropBindingElem.GetAttributeValue("Path");
                    string navigPropName = string.Empty;
                    string[] separation = pathVal.Split('/');
                    if(separation.Length == 1)
                    {
                        navigPropName = separation[0];
                    }
                    else
                    {
                        typeShortName = separation[separation.Length - 2].GetLastSegment();
                        navigPropName = separation[separation.Length - 1];
                    }

                    xPath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty' and @Name='{1}']", typeShortName, navigPropName);
                    var navigPropElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                    if (null != navigPropElem)
                    {
                        if (null == navigPropElem.Attribute("ContainsTarget"))
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
