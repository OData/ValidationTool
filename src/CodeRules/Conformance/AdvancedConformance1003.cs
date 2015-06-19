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
    /// Class of extension rule for Advanced.Conformance.1003
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance1003 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.1003";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "3. MUST support the [OData-JSON] format.";
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
            bool errorResponseResult = false;
            bool feedAndEntryResult = false;
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

            errorResponseResult = VerificationHelper.VerifyError(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            feedAndEntryResult = VerificationHelper.VerifyFeedAndEntry(context, out infoForOne);
            if (infoForOne != null)
            {
                details.AddRange(infoForOne.Details);
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);
            info.SetDetailsName(this.Name);

            return svcDocResult && metadataResult && feedAndEntryResult && errorResponseResult;
        }
    }
}
