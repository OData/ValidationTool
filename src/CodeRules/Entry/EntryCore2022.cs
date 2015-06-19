// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of extension rule #148
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2022 : ExtensionRule
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
                return "Entry.Core.2022";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "Each element MUST contain an atom:rel attribute with the value defined by the relNavigationlLinkURI rule.";
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
                return "8.1.1.1";
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
        /// Gets the flag whether the rule requires metadata document
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
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
        /// Verify rule logic
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

            // get the list of navigation properties of the interesting entity type
            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            EntityType et;
            edmxHelper.TryGetItem(context.EntityTypeFullName, out et);
            var navProps = et.NavigationProperties.Select(x => x.Name);

            // find out the data service namespace; falling back the implcit one if none exists
            string dsns = ResourcePathHelper.GetDataServiceNamespace(XElement.Parse(context.ResponsePayload));

            // get the relaxNG schema and verify the payload
            var rngNavLinks = navProps.Select(x => string.Format(tmplRngNavLink, dsns, x));
            var rng = string.Format(tmplRngSchema, string.Join(string.Empty, rngNavLinks), RngCommonPattern.CommonPatterns);

            RngVerifier verifier = new RngVerifier(rng);
            TestResult result;
            passed = verifier.Verify(context, out result);

            if (!passed.Value)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result.LineNumberInError);
            }

            return passed;
        }

        const string tmplRngNavLink = @"<define name=""entryProps"" combine=""interleave"">
      <element name=""atom:link"">
        <attribute name=""rel"">
            <choice>
                <value>http://schemas.microsoft.com/ado/2007/08/dataservices/related/{1}</value>
                <value>{0}/related/{1}</value>
            </choice>
        </attribute>
        <zeroOrMore><attribute><anyName><except><name>rel</name></except></anyName></attribute></zeroOrMore>
        <ref name=""anyElements"" />
      </element>
  </define>";

        const string tmplRngSchema = @"<?xml version=""1.0""?>
<grammar xmlns=""http://relaxng.org/ns/structure/1.0"" xmlns:atom=""http://www.w3.org/2005/Atom"">
  <start>
    <ref name=""entry"" />
  </start>
  
  <define name=""entry"">
    <element name=""atom:entry"">
        <ref name=""anyAttributes"" />
        <mixed><ref name=""entryProps"" /></mixed>
    </element>
  </define>

  <define name=""entryProps"" combine=""interleave"">
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

  <define name=""entryProps"" combine=""interleave"">
    <zeroOrMore>
      <element name=""atom:link"">
        <zeroOrMore>
            <choice>
                <attribute name=""rel"">
                    <choice>
                        <value>self</value>
                        <value>edit</value>
                        <value>edit-media</value>
                    </choice>
                </attribute>
                <attribute><anyName><except><name>rel</name></except></anyName></attribute>
            </choice>
        </zeroOrMore>
        <ref name=""anyElements"" />
      </element>
    </zeroOrMore>
  </define>

  {0}

  {1}
  
</grammar>
";
    }
}

