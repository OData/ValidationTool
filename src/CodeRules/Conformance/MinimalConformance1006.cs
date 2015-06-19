// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.ComponentModel.Composition;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class of extension rule for Minimal.Conformance.1006
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MinimalConformance1006 : ConformanceMinimalExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Minimal.Conformance.1006";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "6. MUST follow OData guidelines for extensibility (section 6 and all subsections)";
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
                return ConformanceDependencyType.Dependency;
            }
        }

        /// <summary>
        /// Gets the conformance rule dependency info.
        /// </summary>
        public override ConformanceRuleDependencyInfo DependencyInfo
        {
            get
            {
                return new ConformanceRuleDependencyInfo(ConformanceCheckType.AllPass, ConformanceRuleRelationship.DerivedRule, new string[] { "Minimal.Conformance.100611", "Minimal.Conformance.100603" });
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
            
            return passed;
        }
    }
}
