// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Net;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of extension rule for Intermediate.Conformance.1002
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IntermediateConformance1002 : ConformanceIntermediateExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Intermediate.Conformance.1002";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "2. MUST successfully parse the [OData-ABNF] and either follow the specification or return 501 Not Implemented for any unsupported functionality (section 9.3.1).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.2";
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


            bool? passed = true;
            info = null;
            bool? result = null;
            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();

            // call helper method
            ExtensionRuleViolationInfo info1 = null;
            HttpStatusCode? selectResult = VerificationHelper.VerifySelect(context, out result, out info1);
            if (info1 != null)
            {
                info1.SetDetailsName(this.Name);
                details.AddRange(info1.Details);
            }
            if (false == result || !(selectResult.HasValue && (selectResult.Value == HttpStatusCode.OK || selectResult.Value == HttpStatusCode.NotImplemented)))
            {
                passed = false;
            }

            ExtensionRuleViolationInfo info2 = null;
            HttpStatusCode? searchResult = VerificationHelper.VerifySearch(context, out result, out info2);
            if (info2 != null)
            {
                info2.SetDetailsName(this.Name);
                details.AddRange(info2.Details);
            }
            if (false == result || !(searchResult.HasValue && (searchResult.Value == HttpStatusCode.OK || searchResult.Value == HttpStatusCode.NotImplemented)))
            {
                passed = false;
            }

            ExtensionRuleViolationInfo info3 = null;
            HttpStatusCode? filterResult = VerificationHelper.VerifyLambdaOperators(context, LambdaOperatorType.All, out result, out info3);
            if (info3 != null)
            {
                info3.SetDetailsName(this.Name);
                details.AddRange(info3.Details);
            }
            if (false == result || !(filterResult.HasValue && (filterResult.Value == HttpStatusCode.OK || filterResult.Value == HttpStatusCode.NotImplemented)))
            {
                passed = false;
            }

            ExtensionRuleViolationInfo info4 = null;
            HttpStatusCode? countResult = VerificationHelper.VerifyCount(context, out result, out info4);
            if (info4 != null)
            {
                info4.SetDetailsName(this.Name);
                details.AddRange(info4.Details);
            }
            if (false == result || !(countResult.HasValue && (countResult.Value == HttpStatusCode.OK || countResult.Value == HttpStatusCode.NotImplemented)))
            {
                passed = false;
            }

            ExtensionRuleViolationInfo info5 = null;
            HttpStatusCode? skipResult = VerificationHelper.VerifySkip(context, out result, out info5);
            if (info5 != null)
            {
                info5.SetDetailsName(this.Name);
                details.AddRange(info5.Details);
            }
            if (false == result || !(skipResult.HasValue && (skipResult.Value == HttpStatusCode.OK || skipResult.Value == HttpStatusCode.NotImplemented)))
            {
                passed = false;
            }

            ExtensionRuleViolationInfo info6 = null;
            HttpStatusCode? topResult = VerificationHelper.VerifyTop(context, out result, out info6);
            if (info6 != null)
            {
                info6.SetDetailsName(this.Name);
                details.AddRange(info6.Details);
            }
            if (false == result || !(topResult.HasValue && (topResult.Value == HttpStatusCode.OK || topResult.Value == HttpStatusCode.NotImplemented)))
            {
                passed = false;
            }

            ExtensionRuleViolationInfo info7 = null;
            HttpStatusCode? orderbyResult = VerificationHelper.VerifySortEntities(context, SortedType.ASC, out result, out info7);
            if (info7 != null)
            {
                info7.SetDetailsName(this.Name);
                details.AddRange(info7.Details);
            }
            if (false == result || !(orderbyResult.HasValue && (orderbyResult.Value == HttpStatusCode.OK || orderbyResult.Value == HttpStatusCode.NotImplemented)))
            {
                passed = false;
            }

            if (passed == null)
            {
                passed = true;
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);
            return passed;
        }
    }
}
