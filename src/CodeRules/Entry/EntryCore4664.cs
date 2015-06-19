// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of entension rule for Entry.Core.4664
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore4664 : ExtensionRule
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
                return "Entry.Core.4664";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The rel attribute of atom:link element for stream properties MUST be present.";
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
        /// Verify Entry.Core.4664
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

            var StreamTypeResultList = MetadataHelper.GetAllStreamTypeFromEntry(context.MetadataDocument, context.EntityTypeShortName);

            if (StreamTypeResultList.Count == 0)
            {
                info = null;
                return null;
            }

            XElement xePayload = XElement.Parse(context.ResponsePayload);
            string linkFormat = "./*[local-name()='link']";

            var links = xePayload.XPathSelectElements(linkFormat, ODataNamespaceManager.Instance);

            Hashtable refDict = new Hashtable();
            string streamEditref = @"odata/ns/edit-media/";
            string streamReadref = @"odata/ns/mediaresource/";

            using (IEnumerator<XElement> enumerator = links.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var xe = enumerator.Current;
                    string rellink = xe.GetAttributeValue(@"rel");

                    if (null == rellink)
                        continue;

                    int eindex = rellink.IndexOf(streamEditref);
                    int rindex = rellink.IndexOf(streamReadref);
                    if( rindex != -1 || eindex != -1 )
                    {
                        int index = rindex == -1 ? eindex : rindex;
                        int linklength = rindex == -1 ? streamEditref.Length : streamReadref.Length;
                        string property = rellink.Substring(index + linklength);

                        if (refDict.ContainsKey(property))
                        {
                            refDict[property] = (int)refDict[property] + 1;
                        }
                        else
                        {
                            refDict[property] = 1;
                        }
                    }
                }
            }

            foreach (var xe in StreamTypeResultList)
            {
                string property = xe.GetAttributeValue("Name");
                if (refDict.ContainsKey(property))
                {
                    if ((int)refDict[property] > 2)
                    {
                        passed = false;
                        break;
                    }
                    refDict.Remove(property);

                }
                else
                {
                    passed = false;
                    break;
                }
            }

            if (refDict.Count != 0)
            {
                passed = false;
            }

            info = new ExtensionRuleViolationInfo(passed == true ? null : this.ErrorMessage, context.Destination, context.ResponsePayload);

            return passed;
        }
    }
}
