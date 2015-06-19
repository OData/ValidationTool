// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Linq;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using System.Data.Metadata.Edm;
    #endregion

    /// <summary>
    /// Class of entension rule for rule #297
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class PropertyCore2003 : ExtensionRule
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
                return "Property.Core.2003";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"A property of type EDMSimpleType MUST be represented as a JSON name/value pair. The name in the name/value pair MUST be equal to the name of the EDM property...";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.3.8";
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
                return RuleEngine.PayloadFormat.Json;
            }
        }

        /// <summary>
        /// Gets the falg whether it applies to projection query or not
        /// </summary>
        public override bool? Projection
        {
            get
            {
                return false;
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

            var segments = ResourcePathHelper.GetPathSegments(context).ToArray();
            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            UriType uriType;
            var target = edmxHelper.GetTargetType(segments, out uriType);
            if (uriType == UriType.URI4 || uriType == UriType.URI5)
            {
                string property = segments.LastOrDefault(s => !s.StartsWith("$"));
                JObject jo = JObject.Parse(context.ResponsePayload);
                var version = JsonParserHelper.GetPayloadODataVersion(jo);

                string coreSchema = string.Format(fmtCore, property);
                string jSchema = JsonSchemaHelper.WrapJsonSchema(coreSchema, version);
                TestResult testResult;
                passed = JsonParserHelper.ValidateJson(jSchema, context.ResponsePayload, out testResult);
                if (passed.HasValue && !passed.Value)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, testResult != null ? testResult.LineNumberInError : -1);
                }
            }

            return passed;
        }

        private const string fmtCore = @"
{{
			""type"":""object""
			,""properties"" : {{""{0}"" : {{""type"":""any"", ""required"" : true }} }}
}}";
    }
}