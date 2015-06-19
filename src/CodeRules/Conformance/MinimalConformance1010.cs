// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.1010
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1010 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1010";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "10. MUST NOT violate any OData update semantics (section 11.4 and all subsections).";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "13.1.1";
            }
        }

        /// <summary>
        /// Gets the conformance rule type to which the rule applies.
        /// </summary>
        public override ConformanceDependencyType? DependencyType
        {
            get
            {
                return ConformanceDependencyType.Skip;
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

            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(this.Name);
            detail.ErrorMessage = @"This negative rule is not verified.";
            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
                        
            return passed;
        }
    }
}
