// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// class of concrete code of rule #250 when payload is an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2017_Entry : CommonCore2017
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.Entry;
            }
        }
    }

    /// <summary>
    /// class of concrete code of rule #250 when payload is a feed
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2017_Feed : CommonCore2017
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
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
    /// class of concrete code of rule #250 when payload is an error
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2017_Error : CommonCore2017
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.Error;
            }
        }
    }


    /// <summary>
    /// class of concrete code of rule #250 when payload is a service document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2017_ServiceDocument : CommonCore2017
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
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
    /// class of concrete code of rule #250 when payload is a metadata document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2017_Metadata : CommonCore2017
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
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
    /// class of concrete code of rule #250 when payload is a property
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2017_Property : CommonCore2017
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.Property;
            }
        }
    }

    /// <summary>
    /// class of concrete code of rule #250 when payload is a Link
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2017_Link : CommonCore2017
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.Link;
            }
        }
    }

    /// <summary>
    /// Abstract base class of rule #250
    /// </summary>
    public abstract class CommonCore2017 : ExtensionRule
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
                return "Common.Core.2017";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"(a data service)server SHOULD only use HTTP messages with a Content-Type header value as shown in the ABNF grammar that follows and is specified in [RFC5234]...";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.5.2";
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
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2_V3;
            }
        }

        /// <summary>
        /// Gets the flag of metadata document availability
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return null;
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
            bool? passed = false;

            string contentType = context.ResponseHttpHeaders.GetHeaderValue("Content-Type");
            if (!string.IsNullOrEmpty(contentType))
            {
                string mediaType = null;
                string subType = null;

                string[] parts = contentType.Split(';');
                if (parts.Length >= 1)
                {
                    mediaType = parts[0].Trim().ToLowerInvariant();

                    for (int i = 1; i < parts.Length; i++)
                    {
                        string[] pair = parts[i].Split(new char[]{'='}, 2);
                        if (pair.Length == 2)
                        {
                            if (pair[0].Equals("type", StringComparison.OrdinalIgnoreCase))
                            {
                                subType = pair[1].ToLowerInvariant();
                                break;
                            }
                        }
                    }
                }

                passed = ContentTypes.Any(x => mediaType.Equals(x, StringComparison.Ordinal));
                if (passed.Value)
                {
                    if (!string.IsNullOrEmpty(subType) && mediaType.Equals("application/atom+xml", StringComparison.Ordinal))
                    {
                        passed = ODataSubtypes.Any(x => subType.Equals(x, StringComparison.Ordinal));
                    }
                }
            }

            if (!passed.Value)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, contentType);
            }

            return passed;
        }

        static string[] ContentTypes = {"application/atom+xml", 
                                       "application/json",
                                       "application/xml",
                                       "text/plain",
                                       "text/xml",
                                       "octet/stream"};

        static string[] ODataSubtypes = { "entry", "feed" };

    }
}
