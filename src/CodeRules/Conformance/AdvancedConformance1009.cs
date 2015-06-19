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
    /// Class of extension rule for Advanced.Conformance.1009
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class AdvancedConformance1009 : ConformanceAdvancedExtensionRule
    {
        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Advanced.Conformance.1009";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return "9. MUST support $expand (section 11.2.4.2)";
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
        /// Gets the conformance rule dependency info.
        /// </summary>
        public override ConformanceRuleDependencyInfo DependencyInfo
        {
            get
            {
                return new ConformanceRuleDependencyInfo(ConformanceCheckType.AllPass, ConformanceRuleRelationship.SubRule, 
                    new string[] 
                    { 
                        "Advanced.Conformance.100901", "Advanced.Conformance.100902", "Advanced.Conformance.100903", 
                        "Advanced.Conformance.100904", "Advanced.Conformance.100905", "Advanced.Conformance.100906", 
                        "Advanced.Conformance.100907", "Advanced.Conformance.100908"
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
