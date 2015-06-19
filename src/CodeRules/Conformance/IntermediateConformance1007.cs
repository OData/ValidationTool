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
    /// Class of extension rule for Intermediate.Conformance.1007
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class IntermediateConformance1007 : ConformanceIntermediateExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Intermediate.Conformance.1007";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "7. MUST support $filter (section 11.2.5.1)";
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
                return new ConformanceRuleDependencyInfo(ConformanceCheckType.AllPass, ConformanceRuleRelationship.SubRule, 
                    new string[] 
                    { 
                        "Intermediate.Conformance.100701", "Intermediate.Conformance.100702", "Intermediate.Conformance.100703", 
                        "Intermediate.Conformance.100704", "Intermediate.Conformance.100705" 
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
