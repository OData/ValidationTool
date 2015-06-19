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
    /// Class of entension rule for Rule #323
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class LinkCore2002 : ExtensionRule
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
                return "Link.Core.2002";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"(the set of links)Such Link information MUST be serialized as an XML document that conforms to the XSD Schema [XMLSCHEMA1] shown in the following XSD Schema for a set of Links Represented using XML listing.";
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

            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            var segments = ResourcePathHelper.GetPathSegments(context);
            UriType uriType;
            var target = edmxHelper.GetTargetType(segments, out uriType);

            if (uriType == UriType.URI7)
            {
                // safe to convert since s must be a RelationshipEndMember
                var targetRelationEndMember = (RelationshipEndMember)target;
                isCheckingTarget = targetRelationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many;
            }

            if (isCheckingTarget)
            {
                string rngSchema = string.Format(rngFormat, RngCommonPattern.CommonPatterns);
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

        private const string rngFormat = @"
<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">

  <start>
    <ref name=""links""/>
  </start>

  <define name=""links"">
	<element name=""links"" ns=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
		<ref name=""anyAttributes""/>
		<mixed><ref name=""myElements""/></mixed>
	</element>
  </define>

    <define name=""myElements"" combine=""interleave"" >
      <zeroOrMore>
      <element>
		<anyName>
				<except>
					<name ns=""http://schemas.microsoft.com/ado/2007/08/dataservices"">uri</name>
				</except>
			</anyName>
			<ref name =""anyContent"" />
	  </element>
      </zeroOrMore>
   </define>

    <define name=""myElements"" combine=""interleave"">
		<ref name=""link"" />
	</define>

  
  <define name=""link"" ns=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
      <zeroOrMore>
		<element name=""uri"" >
			<ref name=""anyAttributes""/>
			<mixed><ref name=""anyElements""/></mixed>
		</element>
      </zeroOrMore>
    </define>
	
    {0}
</grammar>
";
    }
}