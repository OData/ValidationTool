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
    /// Class of entension rule for Entry.Core.2012
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2012 : ExtensionRule
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
                return "Entry.Core.2012";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "If the Entity Type instance represented includes Customizable Feeds annotations in the data services metadata document,"
                    + @" then the properties with custom mappings must be represented as directed by the mappings information specified in Conceptual Schema Definition Language Document"
                    + @" for Version 2.0 Data Services (section 2.2.3.7.2.1).";
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

            string xpath = string.Format(".//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and @Name and @m:FC_TargetPath]", context.EntityTypeShortName);
            var mappedProperties = meta.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            foreach (var mp in mappedProperties)
            {
                var custFeedProperty = new EntryCore2012CFP(mp, entry);
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

            return passed;
        }

        /// <summary>
        /// Inner class to help generate rng schema 
        /// </summary>
        class EntryCore2012CFP : CustomizedFeedProperty
        {
            public EntryCore2012CFP(XElement nodeDecl, XElement entry)
                : base(nodeDecl, entry)
            {
            }

            protected override string GetRngCoreNode()
            {
                string result;
                if (this.isAttributeTarget)
                {
                    string nameAttribute = this.targetPath[this.targetPath.Length - 1].Substring(1);
                    if (string.IsNullOrEmpty(this.PropertyInContent))
                    {
                        result = string.Format(EntryCore2012CFP.fmtAttribute, nameAttribute, string.Empty);
                    }
                    else
                    {
                        result = string.Format(EntryCore2012CFP.fmtAttribute, nameAttribute, "<value>" + HttpUtility.HtmlEncode(this.PropertyInContent) + "</value>");
                    }
                }
                else
                {
                    bool valueNotMatched = (this.IsPropertyInContent && this.PropertyInContent != this.PropertyInTarget);
                    string lastElement = this.targetPath[this.targetPath.Length - 1];
                    if (!valueNotMatched)
                    {
                        string attr = null;
                        if (this.fc_ContentKind == "text")
                        {
                            attr = "<optional>" + string.Format(EntryCore2012CFP.fmtTypeAttribute, "text") + "</optional>";
                        }
                        else
                        {
                            attr = string.Format(EntryCore2012CFP.fmtTypeAttribute, this.fc_ContentKind);
                        }

                        result = string.Format(EntryCore2012CFP.fmtElementCore_ValueMatched, lastElement, attr);
                    }
                    else
                    {
                        result = string.Format(EntryCore2012CFP.fmtElementCore_ValueNotMatched, lastElement);
                    }
                }
                return result;
            }

            private const string fmtElementCore_ValueMatched = @"<element name=""{0}""><ref name=""anyContent""/>{1}</element>";
            private const string fmtElementCore_ValueNotMatched = @"<element name=""{0}""><ref name=""anyContent""/><attribute name=""ioftanchor_9dje8v""/></element>";
            private const string fmtAttribute = @"<attribute name=""{0}"">{1}</attribute>";
            private const string fmtTypeAttribute = @"<attribute name=""type""><value>{0}</value></attribute>";
        }
    }
}