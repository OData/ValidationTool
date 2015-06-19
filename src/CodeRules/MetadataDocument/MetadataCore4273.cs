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
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4273 : ExtensionRule
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
                return "Metadata.Core.4273";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "All unbound functions with the same function name within a namespace MUST specify the same return type.";
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
                return "12.2.1.1";
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
        /// Verify Metadata.Core.4273
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
            string xPath = "//*[local-name()='Schema']/*[local-name()='Function']";
            var functionElems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            var dic = new Dictionary<string, List<XElement>>();
            foreach (var functionElem in functionElems)
            {
                if (null == functionElem.Attribute("IsBound") || !Convert.ToBoolean(functionElem.GetAttributeValue("IsBound")))
                {
                    if (null == functionElem.Attribute("Name"))
                    {
                        continue;
                    }

                    string funcName = functionElem.GetAttributeValue("Name");
                    if (!dic.ContainsKey(funcName))
                    {
                        dic.Add(funcName, new List<XElement>() { functionElem });
                    }
                    else
                    {
                        var list = dic[funcName];
                        list.Add(functionElem);
                        dic[funcName] = list;
                    }
                }
            }

            foreach (var d in dic)
            {
                // If the count of function elements is greater than or equal to 2, there are some functions have the same name.
                if (d.Value.Count >= 2)
                {
                    for (int i = 0; i < d.Value.Count - 1; i++)
                    {
                        var funcElem1 = d.Value[i];
                        var funcElem2 = d.Value[i + 1];
                        var parentElem1 = funcElem1.Parent;
                        var parentElem2 = funcElem2.Parent;
                        
                        // A function element only has zero or one ReturnType element.
                        xPath = "./*[local-name()='ReturnType']";
                        var returnTypeElem = funcElem1.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                        var returnType1 = null != returnTypeElem.Attribute("Type") ?
                            returnTypeElem.GetAttributeValue("Type") : null;
                        returnTypeElem = funcElem2.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                        var returnType2 = null != returnTypeElem.Attribute("Type") ?
                            returnTypeElem.GetAttributeValue("Type") : null;

                        if (null != parentElem1 && parentElem1 == parentElem2 &&
                            null != returnType1 && returnType1 == returnType2)
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
