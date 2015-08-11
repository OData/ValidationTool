// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Metadata.Core.4095
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4095 : ExtensionRule
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
                return "Metadata.Core.4095";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The valid values of the SRID attribute and their meanings are as defined by the European Petroleum Survey Group [EPSG].";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "6.2.6";
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
        /// Verify Metadata.Core.4095
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
            string xpath = @"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema']/*[local-name()='EntityType' or local-name()='ComplexType']/*[local-name()='Property']/@SRID";

            XmlNodeList propertyCollection = metadata.SelectNodes(xpath, ODataNamespaceManager.Instance);

            foreach (XmlNode property in propertyCollection)
            {
                if (property.Value.Equals("variable"))
                {
                    continue;
                }

                WebRequest req = WebRequest.Create(new Uri("http://www.epsg-registry.org/query.htm?urn=*:" + property.Value));
                req.Method = "POST";
                Response res = WebHelper.Get(req, 1024 * 1024);

                if (res.StatusCode != HttpStatusCode.OK)
                {
                    continue;
                }

                XElement queryResult;
                try
                {
                    queryResult = XElement.Parse(res.ResponsePayload);
                    if (queryResult.XPathSelectElement(string.Format(@"./div/div[@titleText='Code' and @value='EPSG::{0}']", property.Value)) != null)
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false; break;
                    }
                }
                catch(Exception) { continue; }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }
    }
}

