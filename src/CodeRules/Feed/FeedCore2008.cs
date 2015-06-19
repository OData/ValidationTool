// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Linq;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using ODataValidator.Rule.Helper;

    /// <summary>
    /// Class of extension rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore2008 : ExtensionRule
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
                return "Feed.Core.2008";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The presence of the $expand System Query Option indicates that entities associated with the EntityType instance or EntitySet, identified by the Resource Path section of the URI, MUST be represented inline instead of as Deferred Content (section 2.2.6.2.6).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.3.6.1.3";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.2.4.2";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odataprotocol";
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
        /// Gets the requirement level setting
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RuleEngine.RequirementLevel.Must;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Feed;
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
        /// Verify the code rule
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

            var queries = HttpUtility.ParseQueryString(context.Destination.Query);
            var expand = queries["$expand"];

            if (!string.IsNullOrEmpty(expand))
            {
                XElement root = XElement.Parse(context.ResponsePayload);
                
                var branches = ResourcePathHelper.GetBranchedSegments(expand);
                foreach (var paths in branches)
                {
                    var currNode = root;
                    for(int i = 0; i < paths.Length; i++ )
                    {
                        string toExpand = paths[i];
                        var nodeExpanded = GetNodeWithExpandedLink(currNode, toExpand);
                        //passed = VerifyExpandedLink(feed, toExpand, false) || VerifyExpandedLink(feed, toExpand, true);
                        if (nodeExpanded == null)
                        {
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            return false;
                        }

                        currNode = nodeExpanded;
                    }

                }
            }

            info = null;
            return true;
        }

        private static XElement GetNodeWithExpandedLink(XElement element, string expand)
        {
            string linkFormat = "./*[local-name()='entry']/*[local-name()='link' and @title='{0}'] | ./*/*[local-name()='entry']/*[local-name()='link' and @title='{0}']";
            string xpathAsExpandedLink = "./*[local-name()='inline']/*[local-name()='feed'] | ./*[local-name()='inline']/*[local-name()='entry']";

            string xpathQuery = string.Format(linkFormat, expand);
            XElement node = element.XPathSelectElement(xpathQuery, ODataNamespaceManager.Instance);

            if (node != null)
            {
                XElement expendedEntitySetName = node.XPathSelectElement(xpathAsExpandedLink, ODataNamespaceManager.Instance);

                if (expendedEntitySetName != null)
                {
                    return node;
                }
            }

            return null;
        }
    }
}