// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2006_None : CommonCore2006
    {
        /// <summary>
        /// Gets the payload type this rule applies to
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
    /// Class of code rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2006_Other : CommonCore2006
    {
        /// <summary>
        /// Gets the payload type this rule applies to
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
    /// Class of code rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2006_Error : CommonCore2006
    {
        /// <summary>
        /// Gets the payload type this rule applies to
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
    /// Class of code rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2006_Entry : CommonCore2006
    {
        /// <summary>
        /// Gets the payload type this rule applies to
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
    /// Class of code rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2006_Feed : CommonCore2006
    {
        /// <summary>
        /// Gets the payload type this rule applies to
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
    /// Abstract base class of code rule to check semantic expectation. 
    /// </summary>
    public abstract class CommonCore2006 : ExtensionRule
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
                return "Common.Core.2006";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "URI2, if no entity identified by the keyPredicate exists in the EntitySet specified, "
                    + "then this URI (and any URI created by appending additional path segments) MUST represent a resource that does not exist in the data model.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.3.5";
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
        /// Gets the flag whether it requires metadata document or not
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
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

            bool passed = true;
            info = null;

            // get the first segment after the service root URI
            char[] delimiters = new char[] { '/'};
            string path = context.DestinationBasePath.Substring(context.ServiceBaseUri.AbsoluteUri.Length).Trim('/');
            string firstSegment = path.IndexOfAny(delimiters) >= 0 ? path.Substring(0, path.IndexOfAny(delimiters)) : path;
            firstSegment = firstSegment.Trim('/');

            // check whether the first segment is URI-2
            if (RegexInUri.IsURI2(firstSegment, XElement.Parse(context.MetadataDocument)))
            {
                if (!firstSegment.Equals(path.Trim('/')))
                {
                    Uri resource = new Uri(context.ServiceBaseUri, firstSegment);
                    var resp = WebResponseHelper.GetWithHeaders(resource,
                        context.PayloadFormat == RuleEngine.PayloadFormat.Json,
                        null,
                        RuleEngineSetting.Instance().DefaultMaximumPayloadSize,
                        context);
                    if (resp.StatusCode.HasValue)
                    {
                        int statusCode = (int)resp.StatusCode.Value;
                        if (statusCode < 100 || statusCode >= 400)
                        {
                            if (!(context.HttpStatusCode.Value == System.Net.HttpStatusCode.NotFound || context.HttpStatusCode.Value == System.Net.HttpStatusCode.Gone))
                            {
                                passed = false;
                                info = new ExtensionRuleViolationInfo("unexpected status code " + context.HttpStatusCode.Value.ToString(), context.Destination, context.HttpStatusCode.Value.ToString());
                            }
                            else if (context.PayloadType != RuleEngine.PayloadType.Error)
                            {
                                passed = false;
                                info = new ExtensionRuleViolationInfo("error payload expected", context.Destination, context.PayloadType.ToString());
                            }
                        }
                    }
                }
                else
                {
                    if (context.PayloadType == RuleEngine.PayloadType.Error)
                    {
                        int statusCode = (int)context.HttpStatusCode.Value;
                        passed = statusCode >= 400 && statusCode < 500;
                        if (!passed)
                        {
                            info = new ExtensionRuleViolationInfo("unexpected HTTP status code", context.Destination, context.HttpStatusCode.Value.ToString());
                        }
                    }
                }
            }

            return passed;
        }
    }
}
