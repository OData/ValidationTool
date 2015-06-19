// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namesapces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Abstract base class of code rule to check semantic expectation. 
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore2007 : ExtensionRule
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
                return "Common.Core.2007";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"URI17 = scheme serviceRoot ""/"" entitySet ""("" keyPredicate "")"" value MUST identify the Media Resource [RFC5023] "
                + @"associated with the identified EntityType instance. The EntityType that defines the entity identified MUST be annotated with the ""HasStream"" "
                + @"attribute, as defined in Conceptual Schema Definition Language Document for Data Services (section 2.2.3.7.2).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.3.5";
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
        /// Gets the payload type this rule applies to
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Entry;
            }
        }

        /// <summary>
        /// Gets the flag whether it requires metadata document or not
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the flag whether it applies to projection query or not
        /// </summary>
        public override bool? Projection
        {
            get
            {
                return false;
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
        /// Verifies the semantic rule
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out paramater to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool passed = true;
            info = null;

            string segment = context.Destination.GetLeftPart(UriPartial.Path).Trim('/');
            XElement meta = XElement.Parse(context.MetadataDocument);
            string xpath = string.Format(".//*[local-name() = 'EntityType' and @Name = '{0}' and @m:HasStream = 'true']", context.EntityTypeShortName);
            var entityTypeHasStream = meta.XPathSelectElement(xpath, ODataNamespaceManager.Instance) != null;

            Uri resource = new Uri(context.ServiceBaseUri, segment + "/$value");
            var resp = WebResponseHelper.GetWithHeaders(resource, false, null, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context);
            if (resp.StatusCode.HasValue)
            {
                int statusCode = (int)resp.StatusCode.Value;
                if (statusCode >= 200 && statusCode < 300)
                {
                    // ensure metadata document defines HasStream attribute for the entity type
                    passed = entityTypeHasStream;
                    if (!passed)
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo("URI-17 is not associated to a MLE", context.Destination, "");
                    }
                }
                else
                {
                    // ensure metadata document does not defines HasStream attribute for the entity type
                    passed = !entityTypeHasStream;
                    if (!passed)
                    {
                        passed = false;
                        info = new ExtensionRuleViolationInfo("URI-17 does not return the media resource", context.Destination, "");
                    }
                }
            }
            else
            {
                passed = !entityTypeHasStream;
                if (!passed)
                {
                    passed = false;
                    info = new ExtensionRuleViolationInfo("URI-17 does not return the media resource", context.Destination, "");
                }
            }

            return passed;
        }
    }
}
