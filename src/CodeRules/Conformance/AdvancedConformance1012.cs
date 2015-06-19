// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of extension rule for Advanced.Conformance.1012
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance1012 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.1012";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "12. MUST support the resource path conventions defined in [OData-URL]";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.3";
            }
        }

        /// <summary>
        /// Verifies the extension rule.
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

            bool svcDocResult = false;
            bool metadataResult = false;
            bool feedAndEntryResult = false;
            bool referenceResult = false;
            bool propertyResult = false;
            bool collectionCountResult = false;
            bool derivedTypeResult = false;
            bool mediaStreamResult = false;
            bool crossJoinResult = false;
            bool allEntitiesResult = false;
            ExtensionRuleViolationInfo infoForOne = null;
            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();

            svcDocResult = VerificationHelper.VerifySvcDoc(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            metadataResult = VerificationHelper.VerifyMetadata(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            feedAndEntryResult = VerificationHelper.VerifyFeedAndEntry(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            referenceResult = VerificationHelper.VerifyReference(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            propertyResult = VerificationHelper.VerifyPropertyAndValue(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            collectionCountResult = VerificationHelper.VerifyCollectionCount(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            derivedTypeResult = VerificationHelper.VerifyDerivedType(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            mediaStreamResult = VerificationHelper.VerifyMediaStream(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            crossJoinResult = VerificationHelper.VerifyCrossJoin(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            allEntitiesResult = VerificationHelper.VerifyAllEntities(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);
            info.SetDetailsName(this.Name);

            return svcDocResult && metadataResult && feedAndEntryResult && referenceResult && propertyResult
                && collectionCountResult && derivedTypeResult && mediaStreamResult && crossJoinResult && allEntitiesResult;
        }
    }
}
