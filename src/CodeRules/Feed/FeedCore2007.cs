// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Text;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of entension rule for the rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore2007 : ExtensionRule
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
                return "Feed.Core.2007";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "When etag is included, it MUST be used instead of the ETag HTTP Header defined in ETag (section 2.2.5.4), "
                + "which, as specified in [RFC2616], is used to represent a single entity when multiple entities are present in a single payload.";
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

            // for each entry within the collection (feed)
            // to check its __metadata.etag value (if existent) is the same as ETag header in the single entry response that would have been requested
            JObject feed;
            context.ResponsePayload.TryToJObject(out feed);
            ODataVersion version = JsonParserHelper.GetPayloadODataVersion(feed);
            var entries = JsonParserHelper.GetEntries(feed);
            int indexEntry = 0;
            int count = entries.Count;
            foreach (JObject e in entries)
            {
                var etag = e.GetPropertyOfElement("__metadata", "etag");
                bool etagInPlace = etag != null;
                if (etagInPlace)
                {
                    var targetSingleEntry = e.GetPropertyOfElement("__metadata", "uri");
                    // get the ETag header value for the entry
                    var etagInSingleEntry = WebResponseHelper.GetETagOfEntry(targetSingleEntry, Constants.AcceptHeaderAtom);

                    JSconSchemaBuilder builder;
                    if (version == ODataVersion.V2)
                    {
                        builder = new JSconSchemaBuilder(jschemaV2Header, jschemaV2Tail);
                    }
                    else
                    {
                        builder = new JSconSchemaBuilder(jschemaV1Header, jschemaV1Tail);
                    }

                    for (int i = 0; i < indexEntry; i++)
                    {
                        builder.AddItem("{},");
                    }

                    var etagLiteral = StringHelper.ToLiteral(etagInSingleEntry);
                    string entryCore = string.Format(entryCoreFormat, etagLiteral);
                    builder.AddItem(entryCore);

                    for (int i = indexEntry + 1; i < count; i++)
                    {
                        builder.AddItem(",{}");
                    }

                    var jSchema = builder.GetProduct();
                    RuleEngine.TestResult result = null;
                    passed = JsonParserHelper.ValidateJson(jSchema, context.ResponsePayload, out result);
                    if (!passed.Value)
                    {
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result != null ? result.LineNumberInError : -1);
                        break;
                    }
                }

                indexEntry++;
            }

            return passed;
        }

        private class JSconSchemaBuilder
        {
            private string tail;
            private StringBuilder sb;

            public JSconSchemaBuilder(string header, string tail)
            {
                this.tail = tail;
                this.sb = new StringBuilder(header);
            }

            public void AddItem(string item)
            {
                this.sb.AppendLine(item);
            }

            public string GetProduct()
            {
                this.sb.AppendLine(this.tail);
                return this.sb.ToString();
            }
        }

        private const string jschemaV1Header = @"{""type"" : ""object"", ""patternProperties"" : { 
            "".*"": { ""type"" : ""array"", ""items"" : [";
        private const string jschemaV1Tail = @",{ } ]}}}";

        private const string jschemaV2Header = @"{""type"" : ""object"", ""patternProperties"":	{
            "".*"": { ""type"" : ""object"", ""patternProperties"" : {
            "".*"": { ""type"" : ""array"",	""items"" : [";
        private const string jschemaV2Tail = @",{ } ]}}}}}";

        private const string entryCoreFormat = @"{{
                    ""type"" : ""object"",
                    ""properties"" : {{
                        ""__metadata"" : {{ 
                            ""type"" : ""object"" ,
                            ""properties"" : {{
                                ""etag"" : {{ ""type"" : ""string"", ""enum"" : [{0}], ""required"" : true }}	
                            }}
                        }}
                    }}
                }}";
    }
}