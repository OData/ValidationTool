// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of entension rule for rule #255
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class EntryCore2016 : ExtensionRule
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
                return "Entry.Core.2016";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The server MUST NOT include an ETag header if the request URI identifies a single entity whose type is not enabled for optimistic concurrency control.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "2.2.5.4";
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
                return RequirementLevel.MustNot;
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
                return null;
            }
        }

        /// <summary>
        /// Gets the flag whether it applies to offline context.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the falg whether it applies to projection query or not
        /// </summary>
        public override bool? Projection
        {
            get
            {
                return null;
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
        /// Verify the rule
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
            bool isConcurrencyEnabled = false;
            info = null;
            List<string> vocDocs = new List<string>() { context.VocCapabilities, context.VocCore, context.VocMeasures };
            string entitySetName = context.EntityTypeShortName.MapEntityTypeShortNameToEntitySetName();
            if (!string.IsNullOrEmpty(entitySetName))
            {
                OptimisticConcurrencyControlType? optimisticConcurrencyCtrlType =
                    SupportiveFeatureHelper.GetOptimisticConcurrencyControl(entitySetName, context.MetadataDocument, vocDocs);
                isConcurrencyEnabled = null != optimisticConcurrencyCtrlType && 0 != optimisticConcurrencyCtrlType.Value.ETagDependsOn.Count;
            }

            // to apply this rule only to entry which is not optimal concurrency enabled
            XElement meta;
            context.MetadataDocument.TryToXElement(out meta);
            string xpath = string.Format("//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and @ConcurrencyMode='Fixed']",
                context.EntityTypeShortName);
            isConcurrencyEnabled = null != meta.XPathSelectElement(xpath, ODataNamespaceManager.Instance) || isConcurrencyEnabled;

            if (!isConcurrencyEnabled)
            {
                var etagInHeader = context.ResponseHttpHeaders.GetHeaderValue("ETag");
                passed = etagInHeader == null;

                if (!passed.Value)
                {
                    info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, etagInHeader);
                }
            }

            return passed;
        }
    }
}

