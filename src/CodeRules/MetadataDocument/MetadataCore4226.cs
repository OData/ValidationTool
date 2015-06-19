// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4226 : ExtensionRule
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
                return "Metadata.Core.4226";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the IsFlags attribute has a value of true, a non-negative integer value MUST be specified for the Value attribute.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "10.2.2";
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
        /// Gets the flag whether the rule requires metadata document
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
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
        /// Verify Metadata.Core.4226
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

            // Load MetadataDocument into XMLDOM
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.MetadataDocument);

            string xpath = "//*[local-name()='EnumType']";
            XmlNodeList enumTypeNodeList = xmlDoc.SelectNodes(xpath);

            foreach (XmlNode enumType in enumTypeNodeList)
            {
                if (enumType.Attributes["IsFlags"] != null && enumType.Attributes["IsFlags"].Value.Equals("true"))
                {
                    foreach (XmlNode member in enumType.ChildNodes)
                    {
                        if (!member.Name.Equals("Member"))
                        {
                            continue;
                        }
                        else
                        {
                            if (member.Attributes["Value"] != null)
                            {
                                try
                                {
                                    long integerValue = Convert.ToInt64(member.Attributes["Value"].Value);
                                    if (integerValue >= 0)
                                    {
                                        passed = true;
                                    }
                                    else
                                    {
                                        passed = false;
                                        info = new ExtensionRuleViolationInfo(this.ErrorMessage +" One of the Member values is a negative integer.", context.Destination, context.ResponsePayload);
                                        break;
                                    }
                                }
                                catch (System.FormatException e)
                                {
                                    passed = false;
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage + " Member value convert to int excecption:" + e, context.Destination, context.ResponsePayload);
                                    break;
                                }
                                catch (System.OverflowException e)
                                {
                                    passed = false;
                                    info = new ExtensionRuleViolationInfo(this.ErrorMessage + " Member value convert to int excecption:" + e, context.Destination, context.ResponsePayload);
                                    break;
                                }
                            }
                            else
                            {
                                passed = true;
                            }
                        }
                    }

                    if (passed == false)
                    {
                        break;
                    }
                }
            }

            return passed;
        }
    }
}
