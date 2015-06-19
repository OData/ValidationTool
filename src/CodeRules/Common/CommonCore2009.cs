// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Web;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// class of concrete code rule implementation
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2009_Feed : CommonCore2009
    {
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Feed; }
        }
    }

    /// <summary>
    /// class of concrete code rule implementation
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2009_Entry : CommonCore2009
    {
        public override PayloadType? PayloadType
        {
            get { return RuleEngine.PayloadType.Entry; }
        }
    }

    /// <summary>
    /// Abstract base class of rule #285 
    /// </summary>
    public abstract class CommonCore2009 : ExtensionRule
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
                return "Common.Core.2009";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"A NavigationProperty that represents an EntityType instance or a group of entities and that is serialized inline MUST be placed within a single <m:inline> element"
                 + @" that is a child element of the <atom:link> element representing the NavigationProperty.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.2.6.1";
            }
        }

        /// <summary>
        /// Gets rule specification section in OData Atom
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "8.1";
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
        /// Gets the aspect property.
        /// </summary>
        public override string Aspect
        {
            get
            {
                return "semantic";
            }
        }

        /// <summary>
        /// Gets the payload format
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Atom;
            }
        }

        /// <summary>
        /// Verifies the semantic rule
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            info = null;
            bool passed = true;

            // if query option of $expand is present, rule shall be verified
            // get the leftmost navigation property of expand query option
            var qs = HttpUtility.ParseQueryString(context.Destination.Query);
            string qExpand = qs["$expand"];

            if (!string.IsNullOrEmpty(qExpand))
            {
                var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
                EntityType et;
                edmxHelper.TryGetItem(context.EntityTypeFullName, out et);

                var segments = ResourcePathHelper.GetPathSegments(context);
                UriType uriType;
                var target = edmxHelper.GetTargetType(segments, out uriType);
                bool isColletcionResource = uriType == UriType.URI1 || uriType == UriType.URI_CollEt;

                var branches = ResourcePathHelper.GetBranchedSegments(qExpand);
                foreach (var paths in branches)
                {
                    var navStack = ODataUriAnalyzer.GetNavigationStack(et, paths).ToArray();
                    bool[] targetIsCollection = (from n in navStack select n.RelationshipMultiplicity == RelationshipMultiplicity.Many).ToArray();

                    string rngCore = @"<ref name=""anyContent"" />";
                    for (int i = paths.Length - 1; i >= 0; i--)
                    {
                        rngCore = targetIsCollection[i] ? GetRngOfInlineFeed(paths[i], rngCore) : GetRngOfInlineEntry(paths[i], rngCore);
                    }

                    // construct the desired srng schema and verify
                    string rngSchema = isColletcionResource 
                        ? string.Format(formatRngOfColl, rngCore, RngCommonPattern.CommonPatterns)
                        : string.Format(formatRngSingle, rngCore, RngCommonPattern.CommonPatterns);

                    RngVerifier verifier = new RngVerifier(rngSchema);
                    TestResult result;
                    passed = verifier.Verify(context, out result);
                    if (!passed)
                    {
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result.LineNumberInError);
                        break;
                    }
                }
            }

            return passed;
        }

        /// <summary>
        /// Gets RelaxNG core schema of inline presentation of single instance of entity type
        /// </summary>
        /// <param name="navProperty">The navigation property name</param>
        /// <param name="innerSchema">The inner definition of navigation property schema</param>
        /// <returns>The generated RelaxNG schema</returns>
        static string GetRngOfInlineEntry(string navProperty, string innerSchema)
        {
            return string.Format(fmtCoreNavToEntry, navProperty, innerSchema);
        }

        /// <summary>
        /// Gets RelaxNG core schema of inline presentation of set of entity type
        /// </summary>
        /// <param name="navProperty">The navigation property name</param>
        /// <param name="innerSchema">The inner definition of navigation property schema</param>
        /// <returns>The generated RelaxNG schema</returns>
        static string GetRngOfInlineFeed(string navProperty, string innerSchema)
        {
            return string.Format(fmtCoreNavToFeed, navProperty, innerSchema);
        }

        /// <summary>
        /// Wrapper of RelaxNG schema for entry payload
        /// </summary>
        const string formatRngSingle = @"<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">

  <start>
    <ref name=""entry"" />
  </start>

    <define name=""entry"">
        {0}
    </define>

    {1}
</grammar>
";
        /// <summary>
        /// Wrapper of RelxNG schema for feed payload
        /// </summary>
        const string formatRngOfColl = @"<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">

  <start>
    <ref name=""feed""/>
  </start>

  <define name=""feed"">
	<element name=""atom:feed"">
		<ref name=""anyAttributes"" />
		<interleave>
			<zeroOrMore>
				<element>
					<anyName>
						<except>
							<name>atom:entry</name>
						</except>
					</anyName>
					<ref name=""anyAttributes"" />
					<mixed><ref name=""anyElements""/></mixed>
				</element>
			</zeroOrMore>
			<zeroOrMore>
				<ref name=""entry""/>
			</zeroOrMore>
		</interleave>
	</element>
  </define>

    <define name=""entry"">
        {0}
    </define>

    {1}
</grammar>
";

        /// <summary>
        /// Template of RelaxNG schema for type in which there is an expanded named navigation property pointing to set of entity type instances
        /// </summary>
        const string fmtCoreNavToFeed = @"<element name=""atom:entry"">
		<ref name=""anyAttributes"" />
		<mixed>
		<interleave>
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
				<empty/>
			</element>
		  </zeroOrMore>

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
				<element name=""m:inline"">
					<choice>
						<empty />
                        <element name=""atom:feed"">
		                    <ref name=""anyAttributes"" />
		                    <interleave>
			                    <zeroOrMore>
				                    <element> 
                    					<anyName>
					                    	<except>
							                    <name>atom:entry</name>
						                    </except>
					                    </anyName>
					                    <ref name=""anyAttributes"" />
					                    <mixed><ref name=""anyElements""/></mixed>
				                    </element>
			                    </zeroOrMore>
			                    <zeroOrMore>
				                    {1}
			                    </zeroOrMore>
		                    </interleave>
	                    </element>						
					</choice>
				</element>
			</element>
		  
		  </interleave>
		</mixed>
	</element>
";

        /// <summary>
        /// Template of RelaxNG schema for type in which there is an expanded named navigation property pointing to single instance of entity type
        /// </summary>
        const string fmtCoreNavToEntry = @"<element name=""atom:entry"">
		<ref name=""anyAttributes"" />
		<mixed>
		<interleave>
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
				<empty/>
			</element>
		  </zeroOrMore>

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
				<element name=""m:inline"">
					<choice>
						<empty />
						{1}
					</choice>
				</element>
			</element>
		  
		  </interleave>
		</mixed>
	</element>
";

    }
}
