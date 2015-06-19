// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Net;
    using ODataValidator.Rule.Helper;
    #endregion

    /// <summary>
    /// Abstract Base Class of extension rule for Entry.Core.2002
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2004 : ExtensionRule
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
                return "Entry.Core.2004";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If the entity represents an AtomPub Media Link Entry, and if such an element identifies a Media Resource with an associated concurrency token, then the element SHOULD include an m:etag attribute with a value equal to the ETag of the Media Resource identified by the <atom:link> element.";
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
                return "9.1.4";
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

        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        public override bool? IsMediaLinkEntry
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

            XElement meta;
            context.MetadataDocument.TryToXElement(out meta);
            string xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}' and @m:HasStream='true']/*[local-name()='Property' and @ConcurrencyMode='Fixed']", 
                context.EntityTypeShortName);
            bool IsConcurrentMle = meta.XPathSelectElement(xpath, ODataNamespaceManager.Instance) != null;

            if (IsConcurrentMle)
            {
                // get ETag header value of the media resource
                XElement entry;
                context.ResponsePayload.TryToXElement(out entry);
                var m = entry.XPathSelectElement("//atom:link[@rel='edit-media' and @href]", ODataNamespaceManager.Instance);
                var targetMedia = context.ServiceBaseUri + m.GetAttributeValue("href");

                var etag = WebResponseHelper.GetETagOfEntry(targetMedia, Constants.AcceptHeaderAtom);
                string rngSchema = string.Format(EntryCore2004.rngSchemaFormat, HttpUtility.HtmlEncode(etag));
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
    xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata""
    xmlns:an=""annotation"">

  <start>
    <ref name=""a_mle""/>
  </start>

  <define name=""a_mle"">
    <element>
      <anyName/>
      <ref name=""anyAttributes""/>
      <mixed>
        <ref name=""a_mle_children"" />
      </mixed>
    </element>
  </define>

  <div an:memo=""[atom:entry > ..."" note=""lax pattern"">
    <define name=""a_mle_children"" combine=""interleave"">
      <zeroOrMore>
		<choice>
        <ref name=""anyElementNotLink""/>
        <ref name=""anyLinkButNotRelOfEditMedia""/>
		</choice>
      </zeroOrMore>
    </define>

    <define name=""a_mle_children"" combine=""interleave"">
      <element>
        <name>atom:link</name>
		<attribute name=""rel"">
			<value>edit-media</value>
		</attribute>
		<attribute name=""m:etag"">
			<value>{0}</value>
		</attribute>
		<text/>
		<zeroOrMore>
			<attribute>
				<anyName>
					<except>
						<name>m:etag</name>
						<name>rel</name>
					</except>
				</anyName>
			</attribute>
		</zeroOrMore>
      </element>
    </define>

    <define name=""anyElementNotLink"">
      <element>
        <anyName>
          <except>
            <name>atom:link</name>
          </except>
        </anyName>
        <ref name=""anyContent"" />
      </element>
    </define>
	
    <define name=""anyLinkButNotRelOfEditMedia"">
      <element name=""atom:link"">
		<ref name=""anyAttributes"" />
		<attribute name=""rel"">
			<data type=""token"">
				<except>
					<value>edit-media</value>
				</except>
			</data>
		</attribute>
		<mixed>
		<ref name=""anyElements""/>
		<text/>
		</mixed>
      </element>
    </define>

	</div>

  <div an:name=""common patterns of any ..."">
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

  </div>
</grammar>";
    }
}

