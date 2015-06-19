// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Web;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of entension rule for the rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore2006 : ExtensionRule
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
                return "Feed.Core.2006";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If inlinecount query string object is present, the response MUST include the countNVP name/value pair with the value of the name/value pair "
                    + "equal to the count of the total number of entities addressed by the request URI.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.3.2.1";
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
                return RuleEngine.PayloadFormat.Json;
            }
        }

        /// <summary>
        /// Verify the code rule
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

            bool? passed = null;
            info = null;

            // if no inlinecount is the target uri, fetch the one with this query option
            // and checks its structural
            RuleEngine.TestResult result;
            var queries = HttpUtility.ParseQueryString(context.Destination.Query);
            var count = queries["$inlinecount"];
            if (count != null && count.Equals("allpages", StringComparison.Ordinal))
            {
                passed = JsonParserHelper.ValidateJson(jschema, context.ResponsePayload, out result);
                if (!passed.Value && result != null)
                {
                    info = new ExtensionRuleViolationInfo(result.ErrorDetail, context.Destination, context.ResponsePayload, result.LineNumberInError);
                }
            }
            else
            {
                queries["$inlinecount"] = "allpages";
                var qset = from z in queries.Cast<string>()
                         where !string.IsNullOrEmpty(queries[z])
                         select z + "=" + queries[z];
                var qstr = string.Join("&", qset);
                var target = context.Destination.GetLeftPart(UriPartial.Query) + "?" + qstr;
                Uri uriTarget = new Uri(target);
                var resp = WebResponseHelper.GetWithHeaders(uriTarget, true, null, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context);
                passed = JsonParserHelper.ValidateJson(jschema, resp.ResponsePayload, out result);
                if (!passed.Value && result != null)
                {
                    info = new ExtensionRuleViolationInfo(result.ErrorDetail, uriTarget, resp.ResponsePayload);
                }
            }

            return passed;
        }

        private const string jschema = @"
{""type"" : ""object"",
	""patternProperties"" : {
		"".*"": {
			""type"":[""object""],
			""properties"": {
				""__count"" : { ""type"": ""string"", ""pattern"" : ""^[0-9]+$"" }
			}
		}
	}
}";

    }
}