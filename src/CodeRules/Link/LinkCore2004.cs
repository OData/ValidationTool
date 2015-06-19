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
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of entension rule for Rule #325
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class LinkCore2004 : ExtensionRule
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
                return "Link.Core.2004";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"A single Link, which is not part of a set, MUST be serialized as an XML document that conforms to the XSD Schema [XMLSCHEMA1] shown in following XSD Schema for a single Link Represented listing using XML.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.5.5";
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
                return RuleEngine.PayloadType.Link;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
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

            bool passed = true;
            info = null;

            // to check this rule only when request URI if os URI7, and about set of links (not single link)
            bool isCheckingTarget = false;
            bool isNullable = false;

            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            var segments = ResourcePathHelper.GetPathSegments(context);
            UriType uriType;
            var target = edmxHelper.GetTargetType(segments, out uriType);

            if (uriType == UriType.URI7)
            {
                // safe to convert since s must be a RelationshipEndMember
                var targetRelationEndMember = (RelationshipEndMember)target;
                isCheckingTarget = targetRelationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One ||
                    targetRelationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne;
                isNullable = targetRelationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne;
            }

            if (isCheckingTarget)
            {
                string rngSchema = string.Format(isNullable ? rngFormat_nullable : rngFormat_one, RngCommonPattern.CommonPatterns);
                RngVerifier verifier = new RngVerifier(rngSchema);
                TestResult result;
                passed = verifier.Verify(context, out result);
                if (!passed)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result.LineNumberInError);
                }
            }

            return passed;
        }

        private const string rngFormat_one = @"
<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">

  <start>
    <ref name=""link""/>
  </start>

  <define name=""link"" ns=""http://schemas.microsoft.com/ado/2007/08/dataservices"" datatypeLibrary=""http://www.w3.org/2001/XMLSchema-datatypes"">
		<element name=""uri"" >
			<data type=""string""/>
		</element>
    </define>
	
    {0}
</grammar>
";

        private const string rngFormat_nullable = @"
<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">

  <start>
    <ref name=""link""/>
  </start>

  <define name=""link"" ns=""http://schemas.microsoft.com/ado/2007/08/dataservices"" datatypeLibrary=""http://www.w3.org/2001/XMLSchema-datatypes"">
		<element name=""uri"" >
			<optional>
			<data type=""string""/>
			</optional>
		</element>
    </define>
	
    {0}
</grammar>
";
    }
}