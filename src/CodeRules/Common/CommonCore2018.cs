// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Net;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// class of concrete code of rule #249 when payload is an entry
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2018_Entry : CommonCore2018
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
    /// class of concrete code of rule #249 when payload is a feed
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2018_Feed : CommonCore2018
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
    /// class of concrete code of rule #249 when payload is a service document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2018_ServiceDocument : CommonCore2018
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
    /// class of concrete code of rule #249 when payload is a metadata document
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2018_Metadata : CommonCore2018
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
    /// class of concrete code of rule #249 when payload is a property
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2018_Property : CommonCore2018
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
    /// class of concrete code of rule #249 when payload is a Link
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2018_Link : CommonCore2018
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
    /// class of concrete code of rule #249 when payload is a raw value
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2018_RawValue : CommonCore2018
    {
        /// <summary>
        /// Gets the payload type to which the rule shall apply
        /// </summary>
        public override PayloadType? PayloadType
        {

            get
            {
                return RuleEngine.PayloadType.RawValue;
            }
        }
    }

    /// <summary>
    /// Abstract base class of rule #249
    /// </summary>
    public abstract class CommonCore2018 : ExtensionRule
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
                return "Common.Core.2018";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If the server cannot send a response that is acceptable, as indicated in the preceding Accept Request Header to Content-Type Response Header Mapping table and according to the Accept header value, then, as specified in [RFC2616], the server SHOULD return a 4xx response.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.5.1";
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

            string toAccept;
            if (TestingAcceptHeaders.TryGetValue(context.PayloadType, out toAccept))
            {
                var req = WebRequest.Create(context.Destination);
                var reqHttp = req as HttpWebRequest;

                // TODO: make it to method
                // to carry over the request Http headers to be sent to server
                if (context.RequestHeaders != null)
                {
                    foreach (var head in context.RequestHeaders)
                    {
                        reqHttp.Headers[head.Key] = head.Value;
                    }
                }

                var resp = WebResponseHelper.Get(reqHttp, toAccept, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);

                if (resp.StatusCode.HasValue)
                {
                    int statusCode = (int)resp.StatusCode.Value;
                    passed = statusCode >= 400 && statusCode < 500;
                    if (!passed.Value)
                    {
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, resp.StatusCode.Value.ToString());
                    }
                }
                else
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, "no status code returned");
                }
            }


            return passed;
        }

        static Dictionary<PayloadType, string> TestingAcceptHeaders = new Dictionary<PayloadType,string>() {
            {RuleEngine.PayloadType.Entry, "application/atom+xml;type=feed"},
            {RuleEngine.PayloadType.Feed, "application/atom+xml;type=entry"},
            {RuleEngine.PayloadType.ServiceDoc, "application/atom+xml;type=entry"},
            {RuleEngine.PayloadType.Metadata, "application/atom+xml;type=entry"},
            {RuleEngine.PayloadType.Property, "application/atom+xml;type=entry"},
            {RuleEngine.PayloadType.Link, "application/atom+xml;type=entry"},
            {RuleEngine.PayloadType.RawValue, "application/atom+xml;type=entry"},
        };
    }
}
