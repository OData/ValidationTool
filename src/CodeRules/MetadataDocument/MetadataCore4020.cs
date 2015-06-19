// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Metadata.Core.4020
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4020 : ExtensionRule
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
                return "Metadata.Core.4020";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The value MUST match the namespace of a schema defined in the referenced CSDL document.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "3.4.1";
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
                return false;
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
        /// Gets the OData version
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
            }
        }

        /// <summary>
        /// Verify Metadata.Core.4020
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

            XmlDocument metadata = new XmlDocument();

            metadata.LoadXml(context.MetadataDocument);
            string xpath = @"/*[local-name()='Edmx']/*[local-name()='Reference' and @Uri]/*[local-name()='Include' and @Namespace]";
            
            XmlNodeList includeElements = metadata.SelectNodes(xpath);

            foreach (XmlNode include in includeElements)
            {
                XmlDocument referencedCSDL = new XmlDocument();
                try
                {
                    WebRequest req = WebRequest.Create(include.ParentNode.Attributes["Uri"].Value);
                    Response resp = WebResponseHelper.Get(req as HttpWebRequest, null, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
                    referencedCSDL.LoadXml(resp.ResponsePayload);
                }
                catch (Exception e)
                {
                    if (!ExceptionHelper.IsCatchableExceptionType(e))
                    { throw; }
                }

                // Set to false now, will change to true if find a match.
                passed = false;
                string schemaXPath = @"/edmx:Edmx/edmx:DataServices/edm:Schema";
                XmlNodeList schemaElements = referencedCSDL.SelectNodes(schemaXPath, ODataNamespaceManager.Instance);
                if (schemaElements.Count == 0) { break; }

                foreach (XmlNode referencedSchema in schemaElements)
                {
                    if (referencedSchema.Attributes["Namespace"] == null)
                    {
                        break;
                    }
                    if (include.Attributes["Namespace"].Value == referencedSchema.Attributes["Namespace"].Value)
                    {
                        passed = true;
                    }
                }

                if (passed == false)
                {
                    break;
                }
            }
            
            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }
    }
}

