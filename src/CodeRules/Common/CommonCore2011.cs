// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// class of concrete code rule #274 when payload is a feed
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2011_Feed : CommonCore2011_Entry
    {
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Feed; }
        }
    }

    /// <summary>
    /// class of concrete code rule #274 when payload is an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2011_Entry : CommonCore2011
    {
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Entry; }
        }

        /// <summary>
        /// Gets the payload format
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            info = null;
            bool? passed = true;

            XElement entry = XElement.Parse(context.ResponsePayload);
            // for each property as XML element value, make sure it has either custom XML namnespace or the default one - in other word, it is not null or empty
            string xpath = "//m:properties/*";
            var properties = entry.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                var ns = property.Name.Namespace.NamespaceName;
                if (string.IsNullOrEmpty(ns))
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo("", context.Destination, property.ToString());
                    break;
                }
            }

            return passed;
        }
    }

    /// <summary>
    /// class of concrete code rule #274 when payload is a property
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2011_Property : CommonCore2011
    {
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Property; }
        }

        /// <summary>
        /// Gets the payload format
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            info = null;
            bool? passed = true;

            XElement payload = XElement.Parse(context.ResponsePayload);
            // for each property as XML element value, make sure it has either custom XML namnespace or the default one - in other word, it is not null or empty
            string xpath = "/ | //*";
            var properties = payload.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var property in properties)
            {
                var ns = property.Name.Namespace.NamespaceName;
                if (string.IsNullOrEmpty(ns))
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo("", context.Destination, property.ToString());
                    break;
                }
            }

            return passed;
        }
    }

    /// <summary>
    /// Abstract base class of rule #274 
    /// </summary>
    public abstract class CommonCore2011 : ExtensionRule
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
                return "Common.Core.2011";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The namespace URI preceding SHOULD be used if a data service does not wish to use an alternate.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.1";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "2.1.4";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odataatom";
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
                return RequirementLevel.Should;
            }
        }

        /// <summary>
        /// Gets the aspect property.
        /// </summary>
        public override string Aspect
        {
            get
            {
                return "semantic";
            }
        }
    }
}
