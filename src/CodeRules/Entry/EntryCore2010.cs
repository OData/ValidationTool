// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of entension rule for Entry.Core.2010
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2010 : ExtensionRule
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
                return "Entry.Core.2010";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the property of an Entity Type instance in a Data Service response includes Customizable Feed annotations in"
                    + @" the data services metadata document and has a value of null, then the element or attribute being mapped to"
                    + @" MAY be present and MUST be empty.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.6.2.2.1";
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
        /// Get the flag whether this rule requires metadata document or not
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

            XElement meta = XElement.Parse(context.MetadataDocument);
            XElement entry = XElement.Parse(context.ResponsePayload);

            string xpath = string.Format(".//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and @Name and @m:FC_TargetPath and m:null='true']", context.EntityTypeShortName);
            var mappedProperties = meta.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var mp in mappedProperties)
            {
                var custFeedProperty = new EntryCore2010CFP(mp, entry);
                
                //check whether mapped value  is present
                var v = entry.XPathSelectElement(custFeedProperty.XPathTarget, custFeedProperty.nsResolver);
                if (v != null)
                {
                    string schema = custFeedProperty.GetRngSchema();

                    RngVerifier verifier = new RngVerifier(schema);
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

        /// <summary>
        /// Inner class to help generate rng schema 
        /// </summary>
        class EntryCore2010CFP : CustomizedFeedProperty
        {
            public EntryCore2010CFP(XElement nodeDecl, XElement entry)
                : base(nodeDecl, entry)
            {
            }

            protected override string GetRngCoreNode()
            {
                string result;
                if (this.isAttributeTarget)
                {
                    string nameAttribute = this.targetPath[this.targetPath.Length - 1].Substring(1);
                    result = string.Format(EntryCore2010CFP.fmtAttribute, nameAttribute, string.Empty);
                }
                else
                {
                    string lastElement = this.targetPath[this.targetPath.Length - 1];
                        string attr = null;
                        if (this.fc_ContentKind == "text")
                        {
                            attr = "<optional>" + string.Format(EntryCore2010CFP.fmtTypeAttribute, "text") + "</optional>";
                        }
                        else
                        {
                            attr = string.Format(EntryCore2010CFP.fmtTypeAttribute, this.fc_ContentKind);
                        }

                        result = string.Format(EntryCore2010CFP.fmtElementCore, lastElement, attr);
                }
                return result;
            }

            private const string fmtAttribute = @"<attribute name=""{0}""><empty/></attribute>";
            private const string fmtElementCore = @"<element name=""{0}"">{1}<empty/></element>";
            private const string fmtTypeAttribute = @"<attribute name=""type""><value>{0}</value></attribute>";
        }
    }
}
