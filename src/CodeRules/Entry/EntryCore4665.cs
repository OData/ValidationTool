// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of entension rule for Entry.Core.4665
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4665 : ExtensionRule
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
                return "Entry.Core.4665";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The rel attribute of atom:link element for stream properties MUST be made up of the string http://docs.oasis-open.org/odata/ns/mediaresource/ or http://docs.oasis-open.org/odata/ns/edit-media/, followed by the name of the stream property on the entity, and the full name must be used; the use of relative URLs in the rel attribute is not allowed.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "9.1.1";
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
        /// Verify Entry.Core.4665
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

            bool? passed = true;
            info = null;

            var streamTypes = MetadataHelper.GetAllStreamTypeFromEntry(context.MetadataDocument, context.EntityTypeShortName);
            var streamTypeNames = from st in streamTypes
                                  where null != st.Attribute("Name")
                                  select st.GetAttributeValue("Name");
            XElement xePayload = XElement.Parse(context.ResponsePayload);
            string streamReadRef = @"http://docs.oasis-open.org/odata/ns/mediaresource/";
            string streamRWRef = @"http://docs.oasis-open.org/odata/ns/edit-media/";
            string xPath = string.Format("./*[local-name()='link' and starts-with(@rel, '{0}') or starts-with(@rel, '{1}')]", streamReadRef, streamRWRef);
            var linkElems = xePayload.XPathSelectElements(xPath, ODataNamespaceManager.Instance);

            foreach (var lelem in linkElems)
            {
                string streamPropName = lelem.GetAttributeValue("rel").StartsWith(streamReadRef) ?
                    lelem.GetAttributeValue("rel").Remove(0, streamReadRef.Length) :
                    lelem.GetAttributeValue("rel").Remove(0, streamRWRef.Length);

                if (streamTypeNames.Contains(streamPropName))
                {
                    passed = true;
                }
                else
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                }
            }

            return passed;
        }
    }
}
