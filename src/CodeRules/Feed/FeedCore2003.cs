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
    /// Class of extension rule for Feed.Core.2003
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class FeedCore2003 : ExtensionRule
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
                return "Feed.Core.2003";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "When m:etag is included, it MUST be used instead of the ETag HTTP Header defined in ETag (section 2.2.5.4)... to represent a single entity when multiple entities are present in a single payload";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.2.2";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "6.1.1";
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

            // if entry in the feed has @m:etag, check @m:etag value is the same as ETag header value if single entry were represented.
            const string xpath = "//*[local-name()='entry' and @m:etag]";
            XElement feed;
            context.ResponsePayload.TryToXElement(out feed);
            var x = feed.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var e in x)
            {
                var href = e.XPathSelectElement("atom:id", ODataNamespaceManager.Instance);
                var target = href.Value;
                // get the ETag header value for the entry
                var etag = WebResponseHelper.GetETagOfEntry(target, Constants.AcceptHeaderAtom);
                var rngSchema = string.Format(FeedCore2003.rngSchemaFormat, HttpUtility.HtmlEncode(etag), HttpUtility.HtmlEncode(target), RngCommonPattern.CommonPatterns);
                RngVerifier verifier = new RngVerifier(rngSchema);
                TestResult result;
                passed = verifier.Verify(context, out result);
                if (!passed.Value)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result.LineNumberInError);
                    break;
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
					<name>atom:entry</name>
				</except>
			</anyName>
			<ref name=""anyAttributes""/>
			<mixed><ref name=""anyElements""/></mixed>
		</element>
      </zeroOrMore>
    </define>

    <define name=""myElements"" combine=""interleave"">
			<element name=""atom:entry"">
				<ref name=""anyAttributes""/>
				<attribute name=""m:etag"">
					<value>{0}</value>
				</attribute>
				<mixed>
					<interleave>
						<ref name=""anyElements""/>
						<element name=""atom:id"">
							<ref name=""anyAttributes""/>
							<value>{1}</value>
						</element>
					</interleave>
				</mixed>
			</element>
	</define>

    <define name=""myElements"" combine=""interleave"">
		<zeroOrMore>
			<element name=""atom:entry"">
				<ref name=""anyAttributes""/>
				<mixed>
					<interleave>
						<zeroOrMore>
							<element>
								<anyName>
									<except>
										<name>atom:id</name>
									</except>
								</anyName>
								<ref name=""anyAttributes"" />
								<mixed><ref name=""anyElements""/></mixed>
							</element>
						</zeroOrMore>
						<element name=""atom:id"">
							<data type=""token"">
								<except>
									<value>{1}</value>
								</except>
							</data>
						</element>
					</interleave>
				</mixed>
			</element>
		</zeroOrMore>
	</define>

{2}	
</grammar> 
"; 
    }
}