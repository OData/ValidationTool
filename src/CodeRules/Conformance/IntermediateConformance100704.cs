// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Net;
    using System.ComponentModel.Composition;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of extension rule for Intermediate.Conformance.100704
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IntermediateConformance100704 : ConformanceIntermediateExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Intermediate.Conformance.100704";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "7.4. SHOULD support the canonical functions (section 11.2.5.1.2) and MUST return 501 Not Implemented for any unsupported canonical functions (section 9.3.1)";
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
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Should;
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

            bool? passed = null;
            info = null;

            HttpStatusCode? statusCode1 = VerificationHelper.VerifyCanonicalFunction(context, CanonicalFunctionType.Supported, out passed, out info);
            if (true != passed)
            {
                if (info != null)
                {
                    info.SetDetailsName(this.Name);
                }
                return passed;
            }

            HttpStatusCode? statusCode2 = VerificationHelper.VerifyCanonicalFunction(context, CanonicalFunctionType.Unsupported, out passed, out info);
            passed = statusCode2 == HttpStatusCode.NotImplemented ? true : false;
            if (info != null)
            {
                info.SetDetailsName(this.Name);
            }

            return passed;
        }
    }
}
