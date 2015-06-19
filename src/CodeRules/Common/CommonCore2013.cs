// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// class of concrete code rule when payload is a service document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2013_SvcDoc : CommonCore2013
    {
        /// <summary>
        /// Gets the payload type
        /// </summary>
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.ServiceDoc; }
        }
    }

    /// <summary>
    /// class of concrete code rule when payload is a metadata document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2013_Metadata : CommonCore2013
    {
        /// <summary>
        /// Gets the payload type
        /// </summary>
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Metadata; }
        }
    }

    /// <summary>
    /// class of concrete code rule when payload is a feed
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2013_Feed : CommonCore2013
    {
        /// <summary>
        /// Gets the payload type
        /// </summary>
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Feed; }
        }
    }

    /// <summary>
    /// class of concrete code rule when payload is a link
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2013_Link : CommonCore2013
    {
        /// <summary>
        /// Gets the payload type
        /// </summary>
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Link; }
        }
    }

    /// <summary>
    /// Abstract base class of rule #254
    /// </summary>
    public abstract class CommonCore2013 : ExtensionRule
    {
        /// <summary>
        /// Gets Categpry property
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
                return "Common.Core.2013";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The server MUST NOT include an ETag header in a response to any request performed against a URI that does not identify, as specified in URI Format: Resource Addressing Rules (section 2.2.3), a single entity, properties of an entity, or a Media Resource.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.5.4";
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
                return RequirementLevel.MustNot;
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

        /// <summary>
        /// Gets the flag of context being offline validation
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out paramater to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            info = null;

            //to apply rule to constrained payload types only (which has been determined by derived classes)
            var etagInHeader = context.ResponseHttpHeaders.GetHeaderValue("ETag");
            bool?  passed = etagInHeader == null;
            if (!passed.Value)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, etagInHeader);
            }

            return passed;
        }
    }
}
