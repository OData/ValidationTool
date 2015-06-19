// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Linq;
    using System.ComponentModel.Composition;
    using System.Web;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Data.Metadata.Edm;
    #endregion

    /// <summary>
    /// Class of entension rule for rule #307
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ValueCore2001 : ExtensionRule
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
                return "Value.Core.2001";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"(Raw value of EDMSimpleType property)MUST be serialized as specified in Common Serialization Rules for XML-based Formats (section 2.2.6.1).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.4.1";
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
                return RuleEngine.PayloadType.RawValue;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Other;
            }
        }

        /// <summary>
        /// Gets the flag of whether it applies to offline context or not
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule requires metadata document or not
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
            if (segments.Length > 0)
            {
                string lastSeg = segments[segments.Length - 1];
                if (lastSeg.Equals("$value", StringComparison.Ordinal))
                {
                    var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
                    UriType uriType;
                    var target = edmxHelper.GetTargetType(segments, out uriType);
                    if (uriType == UriType.URI4 || uriType == UriType.URI5)
                    {
                        string targetType = ((EdmProperty)target).TypeUsage.EdmType.FullName;
                        if (!string.IsNullOrEmpty(targetType))
                        {
                            // do the validation here
                            IEdmType type = EdmTypeManager.GetEdmType(targetType);
                            if (type != null)
                            {
                                passed = type.IsGoodWith(context.ResponsePayload);
                                if (passed.HasValue && !passed.Value)
                                {
                                    info = new ExtensionRuleViolationInfo("pattern not matched", context.Destination, context.ResponsePayload, 1);
                                }
                            }
                            else
                            {
                                // type unknown
                                info = new ExtensionRuleViolationInfo("unrecognized Edm type", context.Destination, targetType, 1);
                                passed = false;
                            }
                        }
                    }
                }
            }

            return passed;
        }
    }
}