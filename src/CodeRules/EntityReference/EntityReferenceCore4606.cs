// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.EntityReference
{
    #region Namespace.
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Xml;
    #endregion

    /// <summary>
    /// Class of entension rule for EntityReference.Core.4606
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntityReferenceCore4606 : ExtensionRule
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
                return "EntityReference.Core.4606";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"For entities the id attribute of metadata:ref element for Entity Reference MUST be the atom:id of the referenced entity.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.2";
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
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.EntityRef;
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
                return false;
            }
        }

        /// <summary>
        /// Gets the IsOfflineContext property to which the rule applies.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Verify EntityReference.Core.4606
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
            info = null;

            XmlDocument payload = new XmlDocument();

            payload.LoadXml(context.ResponsePayload);
            string xPath = @"//*[local-name()='ref']";

            XmlNodeList elems = payload.SelectNodes(xPath, ODataNamespaceManager.Instance);

            if (null != elems && elems.Count > 0)
            {
                passed = true;

                foreach (XmlNode elem in elems)
                {
                    string idVal = elem.Attributes["id"].Value;

                    if (!Uri.IsWellFormedUriString(idVal, UriKind.Absolute))
                    {
                        if (Uri.IsWellFormedUriString(idVal, UriKind.Relative))
                        {
                            idVal = idVal.TrimStart('#');
                            idVal = context.ServiceBaseUri + idVal;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                    }

                    var resp = WebHelper.Get(new Uri(idVal), Constants.AcceptHeaderAtom, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    if (null != resp && HttpStatusCode.OK == resp.StatusCode)
                    {
                        payload.LoadXml(resp.ResponsePayload);
                        if (null != payload && "entry" == payload.DocumentElement.Name)
                        {
                            xPath = @"/*/*[local-name()='id']";
                            XmlNode idElem = payload.SelectSingleNode(xPath, ODataNamespaceManager.Instance);
                            string idURL = idElem.InnerXml.ToString();

                            if (Uri.IsWellFormedUriString(idURL, UriKind.Relative))
                            {
                                idURL = idURL.TrimStart('#');
                                idURL = context.ServiceBaseUri + idURL;
                            }

                            if (idVal != idURL)
                            {
                                passed = false;
                                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                                break;
                            }
                        }
                    }
                }
            }

            return passed;
        }
    }
}
