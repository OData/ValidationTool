// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Web;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of extension rule for Feed.Core.2005
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore2005 : ExtensionRule
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
                return "Feed.Core.2005";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "<m:count> element MUST be a direct child element of the <feed> element and MUST occur before any <atom:entry> elements in the feed.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.2.1.1";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "12.3";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odataatom";
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
        /// Gets the offline context to which the rule applies
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
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
                return RuleEngine.PayloadFormat.Atom;
            }
        }

        /// <summary>
        /// Verify the code rule
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

            // run the rng rule if $inlinecount=allpages is part of query string of target uri
            var q = context.Destination.Query;
            var qs = HttpUtility.ParseQueryString(q);
            var v = qs["$inlinecount"];
            if (v != null && v.Equals("allpages", StringComparison.Ordinal))
            {
                var rngSchema = string.Format(FeedCore2005.rngSchemaFormat, RngCommonPattern.CommonPatterns);
                RngVerifier verifier = new RngVerifier(rngSchema);
                TestResult result;
                passed = verifier.Verify(context, out result);
                if (!passed.Value)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result.LineNumberInError);
                }
            }

            return passed;
        }

        private const string rngSchemaFormat = @"<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">

  <start>
    <ref name=""a_feed""/>
  </start>

  <define name=""a_feed"">
	<element>
		<anyName/>
		<ref name=""anyAttributes""/>
		<mixed><ref name=""myElements""/></mixed>
	</element>
  </define>

  <define name=""myElements"" combine=""interleave"">
      <zeroOrMore>
		<element>
			<anyName>
				<except>
					<name>m:count</name>
					<name>atom:entry</name>
				</except>
			</anyName>
			<ref name=""anyAttributes""/>
			<mixed><ref name=""anyElements""/></mixed>
		</element>
      </zeroOrMore>
    </define>

    <define name=""myElements"" combine=""interleave"">
		<element name=""m:count"">
			<ref name=""anyAttributes""/>
			<mixed><ref name=""anyElements""/></mixed>
		</element>
		<zeroOrMore>
		<element name=""atom:entry"">
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