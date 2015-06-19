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
    /// Class of extension rule for Advanced.Conformance.1005
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance1005 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.1005";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "5. MUST support the lambda operators any and all on navigation- and collection-valued properties (section 5.1.1.5 in [OData-URL])";
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

            bool? passed = null;
            info = null;
            
            // Verify the lambda operator "any".
            HttpStatusCode? statusCode1 = VerificationHelper.VerifyLambdaOperators(context, LambdaOperatorType.Any, out passed, out info);
            if (info != null)
            {
                info.SetDetailsName(this.Name);
            }
            if (true != passed)
            {
                return passed;
            }
            
            // Verify the lambda operator "all".
            HttpStatusCode? statusCode2 = VerificationHelper.VerifyLambdaOperators(context, LambdaOperatorType.All, out passed, out info);
            if (info != null)
            {
                info.SetDetailsName(this.Name);
            }

            return passed;
        }
    }
}
