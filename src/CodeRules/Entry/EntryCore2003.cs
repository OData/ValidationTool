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
    using ODataValidator.Rule.Helper;
    #endregion

    /// <summary>
    /// Abstract Base Class of extension rule for Entry.Core.2002
    /// </summary>
    public abstract class EntryCore2003 : ExtensionRule
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
                return "Entry.Core.2003";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"Each child element representing a property MUST be defined in the data service namespace and the element name must be the same as the property it represents.";
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

        // The default Data service Namespace name as defined as dataServiceNs in 2.2.6.2.2, [MS-ODATA].
        private const string dsnsDefault = @"http://schemas.microsoft.com/ado/2007/08/dataservices";
    }

    /// <summary>
    /// Class to validate the rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2003_WithMeta : EntryCore2003
    {
        // RelaxNG pattern of named element.
        private const string rngProperty = @"<element><name>ds:{0}</name><ref name=""anyContent""/></element>
";

        // RelaxNG schema template to enforce this rule.
        private const string rngSchema = @"<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata""
         xmlns:an=""annotation""
         xmlns:ds=""{0}"">
  <start>
    <ref name=""a_entry""/>
  </start>

  <define name=""a_entry"">
    <element>
      <anyName/>
      <ref name=""anyAttributes""/>
      <mixed>
        <ref name=""a_entry_children"" />
      </mixed>
    </element>
  </define>

  <div an:memo=""[atom:entry > ... (atom:content>)? m:properties ..."" note=""lax pattern"">
    <define name=""a_entry_children"" combine=""interleave"">
      <zeroOrMore>
        <ref name=""anyElementNotContentOrMProperties""/>
      </zeroOrMore>
    </define>

    <define name=""a_entry_children"" combine=""interleave"">
        <choice>
            <element>
                <name>atom:content</name>
                <ref name=""anyAttributes"" />
                <ref name=""m_prop""/>
            </element>
            <interleave>
                <element name=""atom:content"">
                    <ref name=""anyAttributes"" />
                </element>
                <ref name=""m_prop""/>
            </interleave>
        </choice>
    </define>

    <define name=""anyElementNotContentOrMProperties"">
      <element>
        <anyName>
          <except>
            <name>atom:content</name>
            <name>m:properties</name>
          </except>
        </anyName>
        <ref name=""anyContent"" />
      </element>
    </define>
  </div>

  <div an:memo=""[m:properties]"">
    <define name=""m_prop"">
      <element>
        <anyName></anyName>
        <ref name=""anyAttributes""/>
        <mixed>
          <ref name=""mpp""/>
        </mixed>
      </element>
    </define>
  </div>

  <div an:memo=""[m:properties] > properties"" an:note=""all the properties"">
    <define name=""mpp"">
      <interleave>
        {1}
      </interleave>
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

</grammar>
";

        // There could exist another version without metadata!!!
        /// <summary>
        /// Gets the flag whether the rule requires metadata document.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the flag whether this applies to offline context. 
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
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

            //1-get the data service namespace name (try 1st property?)
            string dsns = ResourcePathHelper.GetDataServiceNamespace(XElement.Parse(context.ResponsePayload));

            //2-get names of the properties of the involved entity type 
            // ensure they are all unique to protect from repetitive type inheritance declarations in metadata documents 
            string[] propertyNames = XmlHelper.GetProperties(context.MetadataDocument, context.EntityTypeShortName).Distinct().ToArray();

            if (context.Projection)
            {
                List<string> aa = new List<string>();
                var queries = HttpUtility.ParseQueryString(context.Destination.Query);
                var qSelect = queries["$select"];
                var selects = qSelect.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var sel in selects)
                {
                    var propName = propertyNames.FirstOrDefault(x => sel.Equals(x, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(propName))
                    {
                        aa.Add(propName);
                    }
                }
                propertyNames = aa.ToArray();
            }

            //3-craft the rng
            StringBuilder sb = new StringBuilder();
            foreach (var p in propertyNames)
            {
                sb.AppendFormat(rngProperty, p);
            }
            string schema = string.Format(rngSchema, dsns, sb.ToString());

            //4-surrender it under rng validator
            RngVerifier verifier = new RngVerifier(schema);
            TestResult result;
            passed = verifier.Verify(context, out result);

            if (passed.Value)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result.LineNumberInError);
            }

            return passed;
        }
    }

    /// <summary>
    /// Class to validate the rule
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2003_NoMeta : EntryCore2003
    {
        // RelaxNG schema to enforce this rule.
        private const string rngSchema = @"<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata""
         xmlns:an=""annotation"" >
  <start>
    <ref name=""a_entry""/>
  </start>

  <define name=""a_entry"">
    <element>
      <anyName/>
      <ref name=""anyAttributes""/>
      <mixed>
        <ref name=""a_entry_children"" />
      </mixed>
    </element>
  </define>

  <div an:memo=""[atom:entry > ... atom:content?m:properties ..."" note=""lax pattern"">
    <define name=""a_entry_children"" combine=""interleave"">
      <zeroOrMore>
        <ref name=""anyElementNotContentOrMProperties""/>
      </zeroOrMore>
    </define>

    <define name=""a_entry_children"" combine=""interleave"">
      <choice>
        <ref name=""elementContent""/>
        <interleave>
            <element name=""atom:content"">
                <ref name=""anyAttributes"" />
            </element>
            <ref name=""elementMProperties""/>
        </interleave>
      </choice>
    </define>

    <define name=""elementContent"">
      <element>
        <name>atom:content</name>
        <ref name=""anyAttributes"" />
        <ref name=""elementMProperties""/>
      </element>
    </define>

    <define name=""anyElementNotContentOrMProperties"">
      <element>
        <anyName>
          <except>
            <name>atom:content</name>
            <name>m:properties</name>
          </except>
        </anyName>
        <ref name=""anyContent"" />
      </element>
    </define>
  </div>

  <div an:memo=""[m:properties]"">
    <define name=""elementMProperties"">
      <element name=""m:properties"">
        <ref name=""anyAttributes""/>
        <mixed>
          <ref name=""mpp""/>
        </mixed>
      </element>
    </define>
  </div>

  <div an:memo=""[atom:entry > atom:content > m:properties] > properties"" an:note="""">
    <define name=""mpp"">
      <oneOrMore>
        <ref name=""QProperty"" />
      </oneOrMore>
    </define>
  </div>

  <define name=""QProperty"">
    <element>
      <anyName>
        <except>
          <nsName ns=""""/>
          <nsName ns=""http://www.w3.org/2005/Atom""/>
          <nsName ns=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata""/>
        </except>
      </anyName>
      <ref name=""anyContent""/>
    </element>
  </define>

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

        // There could exist another version without metadata!!!
        /// <summary>
        /// Gets the flag whether the rule requires metadata document.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag whether this applies to offline context. 
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V1_V2_V3;
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

            bool passed = false;
            info = null;

            RngVerifier verifier = new RngVerifier(EntryCore2003_NoMeta.rngSchema);
            TestResult result;
            passed = verifier.Verify(context, out result);

            if (!passed)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result.LineNumberInError);
            }

            return passed;
        }
    }
}

