// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Linq;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Data.Metadata.Edm;
    #endregion

    /// <summary>
    /// Concrete class of code rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ValueCore2000_NoMeta : ValueCore2000
    {
        /// <summary>
        /// Get the flag whether this rule requires metadata document or not
        /// </summary>
        public override bool? RequireMetadata
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

            var path = context.DestinationBasePath.Substring(context.ServiceBaseUri.AbsoluteUri.Length);
            var segments = ResourcePathHelper.GetPathSegments(path).ToArray();

            if (segments.Length > 0)
            {
                string lastSeg = segments[segments.Length - 1];
                if (lastSeg.Equals("$value", StringComparison.Ordinal))
                {
                    var ct = context.ResponseHttpHeaders.GetContentTypeValue();
                    HttpHeaderRegexVerifier verifier = new HttpHeaderRegexVerifier("Content-Type", "\\s*text/plain\\b.*");
                    TestResult tr;
                    passed = verifier.Verify(context, out tr);
                    if (passed.HasValue && !passed.Value)
                    {
                        info = new ExtensionRuleViolationInfo("unexpected Content-Type header value", context.Destination, ct);
                    }
                }
            }

            return passed;
        }
    }

    /// <summary>
    /// Concrete class of code rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ValueCore2000_Meta : ValueCore2000
    {
        /// <summary>
        /// Get the flag whether this rule requires metadata document or not
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

            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            var segments = ResourcePathHelper.GetPathSegments(context).ToArray();
            UriType uriType;
            var target = edmxHelper.GetTargetType(segments, out uriType);

            if (segments.Length > 0)
            {
                string lastSeg = segments[segments.Length - 1];
                if (lastSeg.Equals("$value", StringComparison.Ordinal) && uriType != UriType.URI17)
                {
                    string targetType = ((EdmProperty)target).TypeUsage.EdmType.FullName;

                    // to ignore the case of Edm.Binary
                    if (!targetType.Equals("Edm.Binary", StringComparison.Ordinal))
                    {
                        var ct = context.ResponseHttpHeaders.GetContentTypeValue();
                        HttpHeaderRegexVerifier verifier = new HttpHeaderRegexVerifier("Content-Type", "\\s*text/plain\\b.*");
                        TestResult tr;
                        passed = verifier.Verify(context, out tr);
                        if (passed.HasValue && !passed.Value)
                        {
                            info = new ExtensionRuleViolationInfo("unexpected Content-Type header value", context.Destination, ct);
                        }
                    }
                }
            }

            return passed;
        }
    }

    /// <summary>
    /// Abstract class of entension rule for Value.Core.2000
    /// </summary>
    public abstract class ValueCore2000 : ExtensionRule
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
                return "Value.Core.2000";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"By default, the raw value (identified via URIs with Resource Paths ending in ""$value"") of any EDMSimpleType property"
                    +@" (except those of type Edm.Binary) SHOULD be represented using the text/plain media type.";
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
                return RequirementLevel.Should;
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


        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }
    }
}