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
    /// Abstract Base Class of extension rule for Entry.Core.2009
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2009 : ExtensionRule
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
                return "Entry.Core.2009";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"When etag is included, it MUST represent the concurrency token associated with the EntityType instance ETag (section 2.2.5.4).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.3.3";
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
                return RuleEngine.PayloadFormat.Json;
            }
        }

        /// <summary>
        /// Gets the flag whether it applies to offline context.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Verify the rule
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

            // if __metadata.etag property is there, ensure its value equals to ETag header value
            var entry = JsonParserHelper.GetResponseObject(context);
            var etag = entry.GetPropertyOfChild("__metadata", "etag");
            bool etagInPlace = etag != null;
            
            if (etagInPlace)
            {
                var etagInHeader = context.ResponseHttpHeaders.GetHeaderValue("ETag").Trim();
                var etagLiteral = StringHelper.ToLiteral(etagInHeader);
                RuleEngine.TestResult result = null;

                ODataVersion version = JsonParserHelper.GetPayloadODataVersion(entry);
                switch (version)
                {
                    case ODataVersion.V1:
                        string schemaV1 = string.Format(EntryCore2009.schemaFormat_v1, etagLiteral);
                        passed = JsonParserHelper.ValidateJson(schemaV1, context.ResponsePayload, out result);
                        break;
                    case ODataVersion.V2:
                        string schemaV2 = string.Format(EntryCore2009.schemaFormat_v2, etagLiteral);
                        passed = JsonParserHelper.ValidateJson(schemaV2, context.ResponsePayload, out result);
                        break;
                    default:
                        passed = false;
                        break;
                }

                if (!passed.Value)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result != null ? result.LineNumberInError : -1);
                }
            }

            return passed;
        }

        private const string schemaFormat_v1 = @"{{
  ""type"" : ""object"",
	""patternProperties"" : {{
		"".*"": {{
			""type"":[""object""],
			""properties"" : {{
				""__metadata"" : {{ 
					""type"" : ""object"" ,
					""properties"" : {{ ""etag"" : {{ ""type"" : ""string"", ""enum"" : [{0}] }}	}}
				}},
			}}
		}}
	}}
}}";

        private const string schemaFormat_v2 = @"{{
  ""type"": ""object"",
    ""properties"": {{
        ""d"": {{
            ""type"": ""object"",
            ""properties"": {{
                ""result"": {{
                    ""type"": ""object"",
                    ""properties"": {{
                        ""__metadata"": {{
                            ""type"": ""object"",
                            ""properties"": {{
                                ""etag"": {{ ""type"": ""string"", ""enum"": [{0}] }}
                            }}
                        }}
                    }}
                }}
            }}
        }}
    }}
}}";
    }
}

