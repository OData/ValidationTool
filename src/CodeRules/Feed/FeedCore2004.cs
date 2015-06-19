// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Web;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of extension rule for Feed.Core.2004
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore2004 : ExtensionRule
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
                return "Feed.Core.2004";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "When used in HTTP responses, the URI indicated at href attribute MUST be equal to the associated HTTP request URI.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.2.1";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "6.4";
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

            // get value of attribute of @href of node feed/link[@rel='self']
            var xpath = "/atom:link[@rel='self' and @href]";
            XElement feed;
            context.ResponsePayload.TryToXElement(out feed);
            var selfLink = feed.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
            if (selfLink != null)
            {
                var href = selfLink.GetAttributeValue("href");

                // consider the effect of xml:base in scope
                string xBase = selfLink.GetAttributeValue("xml:base", ODataNamespaceManager.Instance);
                if (string.IsNullOrEmpty(xBase))
                {
                    xBase = feed.GetAttributeValue("xml:base", ODataNamespaceManager.Instance);
                }

                var targetFull = context.DestinationBasePath;
                var targetRelative = (string.IsNullOrEmpty(xBase)) ? targetFull : targetFull.Substring(xBase.Length);
                var rngSchema = string.Format(FeedCore2004.rngSchemaFormat, HttpUtility.HtmlEncode(targetFull), HttpUtility.HtmlEncode(targetRelative), RngCommonPattern.CommonPatterns);
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
					<name>atom:link</name>
				</except>
			</anyName>
			<ref name=""anyAttributes""/>
			<mixed><ref name=""anyElements""/></mixed>
		</element>
      </zeroOrMore>
    </define>

    <define name=""myElements"" combine=""interleave"">
			<element name=""atom:link"">
				<ref name=""anyAttributes""/>
				<attribute name=""rel"">
					<value>self</value>
				</attribute>
				<attribute name=""href"">
                    <choice>
    					<value>{0}</value>
	    				<value>{1}</value>
                    </choice>
				</attribute>
				<mixed>
					<ref name=""anyElements""/>
				</mixed>
			</element>
	</define>
	
    <define name=""myElements"" combine=""interleave"">
		<zeroOrMore>
			<element name=""atom:link"">
				<zeroOrMore>
					<attribute>
						<anyName>
							<except>
								<name>rel</name>
							</except>
						</anyName>
					</attribute>
				</zeroOrMore>
				<mixed>
					<ref name=""anyElements""/>
				</mixed>
			</element>
		</zeroOrMore>
	</define>

	<define name=""myElements"" combine=""interleave"">
		<zeroOrMore>
			<element name=""atom:link"">
					<attribute name=""rel"">
						<data type=""token"">
							<except>
								<value>self</value>
							</except>
						</data>
					</attribute>
				<zeroOrMore>
					<attribute>
						<anyName>
							<except>
								<name>rel</name>
							</except>
						</anyName>
					</attribute>
				</zeroOrMore>
				<mixed>
					<ref name=""anyElements""/>
				</mixed>
			</element>
		</zeroOrMore>
	</define>
	
{2}
</grammar>";
    }
}