// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Text;
    using System.Web;
    using ODataValidator.Rule.Helper;
    #endregion

    /// <summary>
    /// Class of extension rule for Entry.Core.2011
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2011 : ExtensionRule
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
                return "Entry.Core.2011";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "The presence of the $expand System Query Option indicates that entities associated with the EntityType instance or EntitySet,"
                    + @" identified by the Resource Path section of the URI, MUST be represented inline instead of as Deferred Content (section 2.2.6.2.6)"
                    + @" and Deferred Content (section 2.2.6.3.9).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.3.6.1.3";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.2.4.2";
            }
        }

        /// <summary>
        /// Gets rule specification name in OData Atom
        /// </summary>
        public override string V4Specification
        {
            get
            {
                return "odataprotocol";
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
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Entry;
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

            // get the leftmost navigation property of expand query option
            var qs = HttpUtility.ParseQueryString(context.Destination.Query);
            string qExpand = qs["$expand"];
            if (!string.IsNullOrEmpty(qExpand))
            {
                var branches = ResourcePathHelper.GetBranchedSegments(qExpand);
                foreach (var paths in branches)
                {
                    string leadNavProp = paths.First();

                    // construct the desired srng schema and verify
                    string rngSchema = string.Format(EntryCore2011.formatRng, leadNavProp, RngCommonPattern.CommonPatterns);
                    RngVerifier verifier = new RngVerifier(rngSchema);
                    TestResult result;
                    passed = verifier.Verify(context, out result);
                    if (!passed.Value)
                    {
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result.LineNumberInError);
                        break;
                    }
                }
            }

            return passed;
        }

        private const string formatRng = @"<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">

  <start>
    <ref name=""an_entry""/>
  </start>

  <define name=""an_entry"">
    <element>
      <anyName/>
      <ref name=""anyAttributes""/>
      <mixed>
        <ref name=""myElements"" />
      </mixed>
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
			<ref name=""anyAttributes"" />
			<mixed><ref name=""anyContent""/></mixed>
		</element>
      </zeroOrMore>
    </define>

    <define name=""myElements"" combine=""interleave"">
      <zeroOrMore>
		<element><name>atom:link</name>
			<zeroOrMore>
			<choice>
			<attribute>
				<anyName>
					<except>
						<name>title</name>
					</except>
				</anyName>
			</attribute>
			<attribute name=""title"">
				<data type=""token"">
							<except>
								<value>{0}</value>
							</except>
						</data>
			</attribute>
			</choice>
			</zeroOrMore>
			<mixed><ref name=""anyContent""/></mixed>
		</element>
      </zeroOrMore>
    </define>

    <define name=""myElements"" combine=""interleave"">
		<element><name>atom:link</name>
			<zeroOrMore>
			<choice>
			<attribute>
				<anyName>
					<except>
						<name>title</name>
					</except>
				</anyName>
			</attribute>
			<attribute name=""title"">
				<choice>
					<value>{0}</value>
				</choice>
			</attribute>
			</choice>
			</zeroOrMore>
			<mixed>
			<oneOrMore>
				<ref name=""anyElement""/>
			</oneOrMore>
			</mixed>
		</element>
    </define>

    {1}
</grammar>
";
    }
}

