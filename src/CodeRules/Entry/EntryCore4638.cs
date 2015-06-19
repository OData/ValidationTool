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
    /// Class of entension rule for Entry.Core.4638
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4638 : ExtensionRule
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
                return "Entry.Core.4638";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"For navigation properties declared by a complex type that is used as a single value in an entity type, the URL should be the canonical URL of the source entity, followed by a forward slash and the path to the navigation property.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.1.1.2";
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
        /// Gets the requriement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Should;
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
        /// Verify Entry.Core.4638
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

            string xpath = string.Format(@"//*[local-name()='ComplexType']/*[local-name()='NavigationProperty']/parent::*");
            XElement metadata = XElement.Parse(context.MetadataDocument);
            
            List<string> complexTypeNames = new List<string>();
            IEnumerable<XElement> complexTypes = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var complexType in complexTypes)
            {
                complexTypeNames.Add(complexType.Attribute("Name").Value);
            }

            List<XElement> complexProps = MetadataHelper.GetPropertyElementWithComplexType(context.EntityTypeShortName, context.MetadataDocument, complexTypeNames);
            List<XElement> complexPropsSingleValued = new List<XElement>();
            foreach (XElement el in complexProps)
            {
                if (el.Attribute("Type") != null && !el.Attribute("Type").Value.Contains("Collection("))
                {
                    complexPropsSingleValued.Add(el);
                }
            }

            if (complexPropsSingleValued.Count == 0)
                return null;

            foreach (var ct in complexPropsSingleValued)
            {
                xpath = string.Format(@"//*[local-name()='ComplexType' and @Name='{0}']/*[local-name()='NavigationProperty']", ct.Attribute("Type").Value.GetLastSegment());
                IEnumerable<XElement> navigProperties = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

                List<string> navigPropCanonicalURLs = new List<string>();

                foreach (var np in navigProperties)
                {
                    navigPropCanonicalURLs.Add(context.DestinationBaseLastSegment + @"/" + ct.Attribute("Name").Value + @"/" + np.Attribute(@"Name").Value);
                }

                xpath = string.Format(@"//*[local-name()='{0}']/*[local-name()='link' and (@type='application/atom+xml;type=entry' or @type='application/atom+xml;type=feed')]", ct.Attribute("Name").Value);
                XElement payload = XElement.Parse(context.ResponsePayload);
                var linkElements = payload.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

                foreach (XElement le in linkElements)
                {
                    if (navigPropCanonicalURLs.Contains(le.Attribute("href").Value))
                    {
                        passed = true;
                    }
                    else
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
