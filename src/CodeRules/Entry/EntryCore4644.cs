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
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using ODataValidator.Rule.Helper;

    /// <summary>
    /// Class of entension rule for Entry.Core.4644
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4644 : ExtensionRule
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
                return "Entry.Core.4644";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If the URL is the navigation link with /$ref appended, the association link MAY be omitted.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.2";
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
                return RequirementLevel.May;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Entry;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
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
        /// Verify Entry.Core.4644
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

            List<string> navigPropNames = MetadataHelper.GetNavigationPropertiesNames(context.MetadataDocument, context.EntityTypeShortName);

            if (navigPropNames == null)
                return null;

            XElement payload = XElement.Parse(context.ResponsePayload);

            foreach (string np in navigPropNames)
            {
                string xpath = string.Format(@"//*[local-name()='link' and @rel='http://docs.oasis-open.org/odata/ns/related/{0}']", np);

                XElement NaviglinkElement = payload.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
                string navigHref = NaviglinkElement.Attribute("href").Value;

                xpath = string.Format(@"//*[local-name()='link' and @rel='http://docs.oasis-open.org/odata/ns/relatedlinks/{0}']", np);

                XElement AssoclinkElement = payload.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

                if (AssoclinkElement == null)
                {
                    passed = true;
                }
                else
                {
                    if (AssoclinkElement.Attribute("href").Value.Equals(navigHref + @"/$ref"))
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                        break;
                    }
                }
            }

            return passed;
        }
    }
}
