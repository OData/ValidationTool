// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Error.Core.4605
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ErrorCore4605 : ExtensionRule
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
                return "Error.Core.4605";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The metadata:error element for Error Reponse MUST have at least two child elements: metadata:code and metadata:message.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "19.1";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V3_V4;
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
                return RuleEngine.PayloadType.Error;
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
        /// Gets the flag whether the rule requires metadata document
        /// </summary> 
        public override bool? RequireMetadata
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule applies to offline context
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Verify Error.Core.4605
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
            bool isCodeElementExist = false;
            bool isMessageElementExist = false;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(context.ResponsePayload);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("m", Constants.V3NSMetadata);
            string MeataNS = Constants.V3NSMetadata;

            if (context.Version == ODataVersion.V4)
            {
                nsmgr.AddNamespace("m", Constants.NSMetadata);
                MeataNS = Constants.NSMetadata;
            }

            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(@"//m:error/*", nsmgr);
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (xmlNode.LocalName.Equals("code") && xmlNode.NamespaceURI.Equals(MeataNS))
                {
                    isCodeElementExist = true;
                }
                else if (xmlNode.LocalName.Equals("message") && xmlNode.NamespaceURI.Equals(MeataNS))
                {
                    isMessageElementExist = true;
                }
            }

            if (isCodeElementExist && isMessageElementExist)
            {
                passed = true;
            }
            else
            {
                passed = false;
                info = new ExtensionRuleViolationInfo(ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }
    }
}
