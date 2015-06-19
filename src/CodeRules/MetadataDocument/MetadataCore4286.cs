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
    public class MetadataCore4286 : ExtensionRule
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
                return "Metadata.Core.4286";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The first segment of the entity set path MUST be the name of the binding parameter in an edm:Function element.";
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
        /// Verify Metadata.Core.4286
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
            string xPath = "//*[local-name()='Function' and @IsBound='true']";
            var functionElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            if (null == functionElems || !functionElems.Any())
            {
                return passed;
            }

            functionElems = functionElems.Where(actElem => null != actElem.Attribute("EntitySetPath")).Select(actElem => actElem);
            if (null == functionElems || !functionElems.Any())
            {
                return passed;
            }

            foreach (var functionElem in functionElems)
            {
                string entitySetPathAttribVal = functionElem.GetAttributeValue("EntitySetPath");
                int indexOfSlash = entitySetPathAttribVal.IndexOf('/');
                string firstSegment = -1 == indexOfSlash ? entitySetPathAttribVal : entitySetPathAttribVal.Remove(indexOfSlash, entitySetPathAttribVal.Length - indexOfSlash);
                xPath = "./*[local-name()='Parameter']";
                var parameterElem = functionElem.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                if (null != parameterElem && null != parameterElem.Attribute("Name"))
                {
                    if (firstSegment == parameterElem.GetAttributeValue("Name"))
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

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
