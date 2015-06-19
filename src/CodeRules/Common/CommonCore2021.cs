// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Concrete class of rule #1057 when payload is a service document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2021_SvcDoc : CommonCore2021
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

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }
    }

    /// <summary>
    /// Concrete class of rule #1057 when payload is a metadata document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2021_Metadata : CommonCore2021
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

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }
    }

    /// <summary>
    /// Concrete class of rule #1057 when payload is a perperty (or properties) of an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2021_Property : CommonCore2021
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Property;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }
    }

    /// <summary>
    /// Concrete class of rule #1057 when payload is navigation link (or links) of an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2021_Link : CommonCore2021
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Link;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }
    }

    /// <summary>
    /// base class of rule #1057
    /// </summary>
    public abstract class CommonCore2021 : ExtensionRule
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
                return "Common.Core.2021";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If an application/atom+xml (section 2.2.5.1.1) response, as specified in AtomPub Format (section 2.2.6.2), is not defined for the target resource,"
                + " then the service SHOULD use application/xml as the response format (see XML Format (section 2.2.6.5)).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "3.1.4.1";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "4.1";
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
        /// Gets the flag whether this rule applies to offline context
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2;
            }
        }

        /// <summary>
        /// Verify rule logic
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

            string value;
            if (!ResourcePathHelper.HasQueryOption(context.Destination.Query, "$format", out value))
            {
                const string AcceptAtomXml = @"application/atom+xml";

                var resp = WebHelper.Get(context.Destination, AcceptAtomXml, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                if (resp.StatusCode.HasValue && resp.StatusCode.Value == System.Net.HttpStatusCode.OK)
                {
                    var contentType = resp.ResponseHeaders.GetContentType();
                    passed = contentType == RuleEngine.PayloadFormat.Xml;
                    if (!passed.Value)
                    {
                        info = new ExtensionRuleViolationInfo(ErrorMessage, context.Destination, resp.ResponseHeaders);
                    }
                }
                else
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(ErrorMessage, context.Destination, resp.StatusCode.ToString());
                }
            }

            return passed;
        }
    }
}

