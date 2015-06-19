// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Xml.Linq;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of entension rule for Rule #288
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class PropertyCore2008 : ExtensionRule
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
                return "Property.Core.2008";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"Each declared property defined on the ComplexType MUST be represented as a name/value pair within the JSON object.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.3.4";
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
        /// Gets the flag whether this rule applies to offline context or not
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule applies to projected query or not
        /// </summary>
        public override bool? Projection
        {
            get
            {
                return false;
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

            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            var segments = ResourcePathHelper.GetPathSegments(context);
            UriType uriType;
            var target = edmxHelper.GetTargetType(segments, out uriType);

            // to check this rule only when returned resource is a ComplexType
            if (uriType == UriType.URI3 || uriType == UriType.URI12)
            {
                JObject jo = JObject.Parse(context.ResponsePayload);
                var version = JsonParserHelper.GetPayloadODataVersion(jo);

                ComplexType ct = (uriType == UriType.URI12) ? (ComplexType)target : (ComplexType)((EdmProperty)target).TypeUsage.EdmType;
                var props = from p in ct.Properties select p.Name;
                var jsProps = props.Select(x => string.Format(@"""{0}"" : {{""required"" : true }}", x));
                string jsCore = string.Format(fmtCoreSchema, string.Join(",", jsProps.ToArray()));
                string jSchema = JsonSchemaHelper.WrapJsonSchema(jsCore, version);
                TestResult result;
                passed = JsonParserHelper.ValidateJson(jSchema, context.ResponsePayload, out result);
                if (passed.HasValue && !passed.Value)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result != null ? result.LineNumberInError : -1);
                }
            }

            return passed;
        }

        private const string fmtCoreSchema = @"
		{{
			""type"":""object""
            ,""required"" : true
			,""patternProperties"" : {{
				"".+"" : {{""type"" : ""object""
						,""required"" : true
						,""properties"": {{ {0} }}
                        }}
            }}
		}}";
    }
}