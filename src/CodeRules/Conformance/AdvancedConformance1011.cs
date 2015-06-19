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
    /// Class of extension rule for Advanced.Conformance.1011
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance1011 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.1011";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "11. MUST support batch requests (section11.7 and all subsections)";
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
        /// Gets the resource type to which the rule applies.
        /// </summary>
        public override ConformanceServiceType? ResourceType
        {
            get
            {
                return ConformanceServiceType.ReadWrite;
            }
        }

        /// <summary>
        /// Gets the conformance rule dependency info.
        /// </summary>
        public override ConformanceRuleDependencyInfo DependencyInfo
        {
            get
            {
                return new ConformanceRuleDependencyInfo(ConformanceCheckType.AllPass, ConformanceRuleRelationship.DerivedRule, 
                    new string[] 
                    { 
                        "Advanced.Conformance.101101", "Advanced.Conformance.101102", "Advanced.Conformance.101103", 
                        "Advanced.Conformance.101106", "Advanced.Conformance.101107", "Advanced.Conformance.101108", 
                        "Advanced.Conformance.101109", "Advanced.Conformance.101110", "Advanced.Conformance.101111", 
                        "Advanced.Conformance.101117"
                    });
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
