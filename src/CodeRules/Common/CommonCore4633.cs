// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;


    /// <summary>
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IndividualPropertyCore4633 : CommonCore4633
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.IndividualProperty;
            }
        }
    }

    /// <summary>
    /// Class of entension rule for Common.Core.4633
    /// </summary>
    public abstract class CommonCore4633 : ExtensionRule
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
                return "Common.Core.4633";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"When annotating an instance of a complex type, the annotation element MUST be a direct child of the metadata:value element.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "18.3.4";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
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
        /// Gets the requriement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
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
        /// Gets the RequireMetadata property to which the rule applies.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the IsOfflineContext property to which the rule applies.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Verify Common.Core.4633
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out paramater to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;

            // Single primitive individual property is not allowed to have instance annotation.
            if (VerificationHelper.IsIndividualPropertySinglePrimitiveType(context.ResponsePayload, context.PayloadType))
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                return null;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.ResponsePayload);
            XmlElement root = xmlDoc.DocumentElement;
            string type = string.Empty;

            if (root.LocalName.Equals("value") && root.NamespaceURI.Equals(Constants.NSMetadata))
            {
                if (root.Attributes["type", Constants.NSMetadata] != null)
                {
                    type = root.Attributes["type", Constants.NSMetadata].Value;

                    if (type.Contains("Collection("))
                    {
                        type = type.Substring(type.IndexOf('(') + 1, type.Length - 12);
                    }
                    
                    type = type.Substring(type.LastIndexOf('.') + 1);

                    List<string> complexTypeNames = MetadataHelper.GetAllComplexNameFromMetadata(context.MetadataDocument);

                    if (complexTypeNames.Contains(type))
                    {
                        string annotation = @"//*[local-name()='value']/*[local-name()='annotation']";
                        XmlNodeList annoations = xmlDoc.SelectNodes(annotation, ODataNamespaceManager.Instance);
                        if (annoations !=null && annoations.Count > 0)
                        {
                            foreach (XmlNode anno in annoations)
                            {
                                if(anno.Attributes["target"] == null || anno.Attributes["target"].Value.Contains(type))
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
            }

            info = new ExtensionRuleViolationInfo(passed == true ? null : this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
