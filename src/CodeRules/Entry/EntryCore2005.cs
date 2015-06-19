// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Web;
    using System.Xml.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Abstract Base Class of extension rule for Entry.Core.2002
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2005 : ExtensionRule
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
                return "Entry.Core.2005";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"When m:etag is included on the <entry> element, it MUST represent the concurrency token associated with the EntityType instance, as defined in ETag.";
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
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2;
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
        /// Gets the flag whether it applies to offline context only or other situations.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Verify the rule
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

            // if atom:entry[@m:etag] is there, ensure its value equals to ETag header value
            XElement entry;
            context.ResponsePayload.TryToXElement(out entry);
            bool etagInPlace = entry.Attribute("{http://schemas.microsoft.com/ado/2007/08/dataservices/metadata}etag") != null;

            if (etagInPlace)
            {
                var etagInHeader = context.ResponseHttpHeaders.GetHeaderValue("ETag");
                string rngSchema = string.Format(EntryCore2005.rngSchemaFormat, HttpUtility.HtmlEncode(etagInHeader));
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
    <ref name=""a_entry""/>
  </start>

  <define name=""a_entry"">
    <element>
      <anyName/>
      <ref name=""myAttributes""/>
      <mixed>
        <ref name=""anyElements"" />
      </mixed>
    </element>
  </define>

    <define name=""myAttributes"" combine=""interleave"">
      <zeroOrMore>
		<attribute>
			<anyName>
				<except>
					<name>m:etag</name>
				</except>
			</anyName>
		</attribute>
      </zeroOrMore>
    </define>

    <define name=""myAttributes"" combine=""interleave"">
      <attribute name=""m:etag"">
		<value>{0}</value>
	  </attribute>
	</define>

    <define name=""anyAttribute"">
      <attribute>
        <anyName />
      </attribute>
    </define>

    <define name=""anyAttributes"">
      <zeroOrMore>
        <ref name=""anyAttribute"" />
      </zeroOrMore>
    </define>

    <define name=""anyContent"">
      <ref name=""anyAttributes"" />
      <mixed>
        <ref name=""anyElements""/>
      </mixed>
    </define>

    <define name=""anyElement"">
      <element>
        <anyName />
        <ref name=""anyAttributes"" />
        <mixed>
          <ref name=""anyElements""/>
        </mixed>
      </element>
    </define>

    <define name=""anyElements"">
      <zeroOrMore>
        <ref name=""anyElement"" />
      </zeroOrMore>
    </define>

</grammar>";
    }
}

