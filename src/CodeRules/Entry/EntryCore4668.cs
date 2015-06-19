// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of entension rule for Entry.Core.4668
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4668 : ExtensionRule
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
                return "Entry.Core.4668";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The href attribute of atom:link element for stream properties MUST be present.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "9.1.2";
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
        /// Verify Entry.Core.4668
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

            var StreamTypeResultList = MetadataHelper.GetAllStreamTypeFromEntry(context.MetadataDocument, context.EntityTypeShortName);

            if (StreamTypeResultList.Count == 0)
            {
                info = null;
                return null;
            }

            XElement xePayload = XElement.Parse(context.ResponsePayload);
            string streamEditref = @"http://docs.oasis-open.org/odata/ns/edit-media/";
            string streamReadref = @"http://docs.oasis-open.org/odata/ns/mediaresource/";

            string editlinkFormat = string.Format("./*[local-name()='link' and @rel='{0}{1}']", streamEditref, @"{0}");
            string readlinkFormat = string.Format("./*[local-name()='link' and @rel='{0}{1}']", streamReadref, @"{0}");
            bool hasLinkNode = false;

            foreach (var xe in StreamTypeResultList)
            {
                string property = xe.GetAttributeValue("Name");

                var xereadlink = xePayload.XPathSelectElement(string.Format(editlinkFormat, property), ODataNamespaceManager.Instance);
                var xeeditlink = xePayload.XPathSelectElement(string.Format(readlinkFormat, property), ODataNamespaceManager.Instance);
                if (xereadlink != null || xeeditlink != null)
                {
                    hasLinkNode = true;
                }
                if (xereadlink != null)
                {
                    if (xereadlink.GetAttributeValue(@"href") == null)
                    {
                        passed = false;
                        break;
                    }
                }

                if (xeeditlink != null)
                {
                    if (xeeditlink.GetAttributeValue(@"href") == null)
                    {
                        passed = false;
                        break;
                    }
                }
            }

            if (hasLinkNode && passed == null)
            {
                passed = true;
            }

            info = new ExtensionRuleViolationInfo(passed == true ? null : this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
