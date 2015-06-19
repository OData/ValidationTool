// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Data.Metadata.Edm;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of entension rule for Property.Core.2001
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class PropertyCore2001 : ExtensionRule
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
                return "Property.Core.2001";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The XML element representing the ComplexType instance as a whole MUST be the root of the XML document"
                    +  @" (for example, not a child element, as described in section Complex Type (section 2.2.6.2.3))";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.5.1";
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
                return RuleEngine.PayloadType.Property;
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
        /// Gets the flag of whether it applies to offline context or not
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag whether this rule requires metadata document or not
        /// </summary>
        public override bool? RequireMetadata
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
        /// <param name="info">out paramater to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            var edmxHelper = new EdmxHelper(XElement.Parse(context.MetadataDocument));
            var segments = ResourcePathHelper.GetPathSegments(context);
            UriType uriType;
            var target = edmxHelper.GetTargetType(segments, out uriType);
            if (uriType == UriType.URI3)
            {
                XElement meta = XElement.Parse(context.MetadataDocument);
                string targetType = ((EdmProperty)target).TypeUsage.EdmType.FullName;

                var complexType = ResourcePathHelper.GetComplexType(targetType, meta);
                if (complexType != null)
                {
                    XElement payload = XElement.Parse(context.ResponsePayload);
                    string schema = GetRngSchema(ResourcePathHelper.GetBaseName(targetType), payload.Name.NamespaceName);
                    RngVerifier verifier = new RngVerifier(schema);
                    TestResult result;
                    passed = verifier.Verify(context, out result);
                    if (passed.HasValue && !passed.Value)
                    {
                        info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload, result.LineNumberInError);
                    }
                }
            }

            return passed;
        }

        private string GetRngSchema(string baseType, string nameNs)
        {
            return string.Format(fmtRng, baseType, nameNs, RngCommonPattern.CommonPatterns);
        }

        const string fmtRng = @"<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">

  <start>
    <ref name=""a_complextype""/>
  </start>

  <define name=""a_complextype"" >
	<element name=""{0}"" ns=""{1}"">
      <ref name=""anyContent""/>
    </element>
  </define>

    {2}
</grammar>";
    }
}