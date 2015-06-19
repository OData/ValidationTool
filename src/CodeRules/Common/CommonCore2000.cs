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
    /// Base Class of extension rule to check payload if of supported type. 
    /// </summary>
    public abstract class CommonCore2000: ExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Common.Core.2000";
            }
        }

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
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "Uri for validation must point to a valid OData Service document, Metadata document, Feed, or Entry.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return null;
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
                return "Uri for validation must point to a valid OData Service document, Metadata document, Feed, or Entry.";
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
        /// Gets the flag of rule applying to offline context
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null; // it applies to both live context and offline context, not like most code rules.
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

            bool passed = false;
            info = null;

            switch (context.PayloadType)
            {
                case RuleEngine.PayloadType.ServiceDoc:
                case RuleEngine.PayloadType.Metadata:
                case RuleEngine.PayloadType.Entry:
                case RuleEngine.PayloadType.Feed:
                    passed = true;
                    break;
                default:
                    info = new ExtensionRuleViolationInfo(Resource.NotSupportedPayloadType, context.Destination, context.ResponsePayload);
                    break;
            }

            return passed;
        }
    }

    /// <summary>
    /// Class of extension rule to check payload if of supported type. 
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2000_Other : CommonCore2000
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Other;
            }
        }
    }

    /// <summary>
    /// Class of extension rule to check payload if of supported type. 
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2000_None : CommonCore2000
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.None;
            }
        }
    }

    /// <summary>
    /// Class of extension rule to check payload if of supported type. 
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2000_ServiceDoc : CommonCore2000
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.ServiceDoc;
            }
        }
    }

    /// <summary>
    /// Class of extension rule to check payload if of supported type. 
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2000_Metadata : CommonCore2000
    {
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
    }

    /// <summary>
    /// Class of extension rule to check payload if of supported type. 
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2000_Feed : CommonCore2000
    {
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
    }

    /// <summary>
    /// Class of extension rule to check payload if of supported type. 
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2000_Entry : CommonCore2000
    {
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
    }
}
