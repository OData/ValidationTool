// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.ComponentModel.Composition;
    using System.Xml;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class of extension rule for Entry.Core.2002
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2002 : ExtensionRule
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
                return "Entry.Core.2002";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If the entity being represented is a Media Link Entry, the value of the ""edit_media"" name/value pair MUST be a URI that is equivalent to the value of the 'href' attribute on an <atom:link rel=""edit-media""> AtomPub element if the entity was to be represented by the AtomPub [RFC5023] format, instead of JSON.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.3.3";
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
                return RuleEngine.PayloadFormat.Json;
            }
        }

        /// <summary>
        /// Gets the flag whether the rule is about an MLE entry
        /// </summary>
        public override bool? IsMediaLinkEntry
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Verify Entry.Core.2002
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

            // Get the payload in AtomPub format
            Response response = WebHelper.Get(context.Destination, Constants.AcceptHeaderAtom, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            // Load the paylaod into XmlDOM and query for src value from content
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(response.ResponsePayload);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            XmlNode xmlNode = xmlDoc.SelectSingleNode("/atom:entry/atom:link[@rel='edit-media']", nsmgr);
            string href = xmlNode.Attributes["href"].Value;

            // Get the edit_media property value
            JObject jsonObject = null;
            string value = null;
            if (context.ResponsePayload.TryToJObject(out jsonObject))
            {
                value = JsonHelper.GetPropertyOfChild(jsonObject, "__metadata", "edit_media");
            }
                        
            href = context.ServiceBaseUri.GetLeftPart(UriPartial.Path) + href;
            if (value.Equals(href, StringComparison.OrdinalIgnoreCase))
            {
                passed = true;
            }
            else
            {
                passed = false;
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }
    }
}

