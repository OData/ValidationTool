// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using Newtonsoft.Json.Linq;
    using System.Data.Metadata.Edm;
    #endregion

    /// <summary>
    /// Class of entension rule for Rule #298
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class PropertyCore2004 : ExtensionRule
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
                return "Property.Core.2004";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"A property of type EDMSimpleType MUST be represented as a JSON name/value pair..."
                    +@" The value MUST be formatted, as specified in Common JSON Serialization Rules for All EDM Constructs (section 2.2.6.3.1)";
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
                passed = false;
                string targetType = ((EdmProperty)target).TypeUsage.EdmType.FullName;
                string property = segments.LastOrDefault(s => !s.StartsWith("$"));
                JObject jo = JObject.Parse(context.ResponsePayload);
                var inner = jo.ReachInnerToken();
                if (inner.Type == JTokenType.Object)
                {
                    JObject innerObj = (JObject)inner;
                    if (innerObj.Properties() != null && innerObj.Properties().Count() == 1)
                    {
                        var value = innerObj.Properties().First().Value;
                        string valueLiteral = value.ToString();

                        IEdmType type = EdmTypeManager.GetEdmType(targetType);

                        bool isValidJsonData = type.IsGoodInJsonWith(valueLiteral);
                        if (isValidJsonData)
                        {
                            passed = true;
                        }
                        else
                        {
                            // target type may allow null
                            //TODO: allow null only when the target type is nullable
                            passed = valueLiteral.Equals("null", StringComparison.Ordinal);
                        }
                    }
                }
            }

            if (passed.HasValue && !passed.Value)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, -1);
            }

            return passed;
        }
    }
}