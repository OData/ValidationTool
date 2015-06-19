// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// Enum dependency type of conformance level rules.
    /// </summary>
    public enum ConformanceDependencyType
    {
        /// <summary>
        /// The single rule.
        /// </summary>
        Single = 0,

        /// <summary>
        /// The dependency rule.
        /// </summary>
        Dependency = 1,

        /// <summary>
        /// The conformance rule is skipped.
        /// </summary>
        Skip = 2,       
    }

     /// <summary>
    /// Enum service type of conformance level rules.
    /// </summary>
    public enum ConformanceServiceType
    {
        /// <summary>
        /// The ReadWrite resource.
        /// </summary>
        ReadWrite = 0,

        /// <summary>
        /// The ReadOnly resource.
        /// </summary>
        ReadOnly = 1,
    }

    /// <summary>
    /// Enum level type of conformance level rules.
    /// </summary>
    public enum ConformanceLevelType
    {
        /// <summary>
        /// The Minimal level.
        /// </summary>
        Minimal = 0,

        /// <summary>
        /// The Intermediate level.
        /// </summary>
        Intermediate = 1,

        /// <summary>
        /// The Advanced level
        /// </summary>
        Advanced = 2,
    }

    /// <summary>
    /// Enum check type of checking dependent rules.
    /// </summary>
    public enum ConformanceCheckType
    {
        /// <summary>
        /// All binding rules need to be passed.
        /// </summary>
        AllPass,

        /// <summary>
        /// Check all minimal rules' result.
        /// </summary>
        AllMinimal,

        /// <summary>
        /// Check all Intermediate rules' result.
        /// </summary>
        AllIntermediate
    }

    /// <summary>
    /// The relationship of main rule and dependent rules.
    /// </summary>
    public enum ConformanceRuleRelationship
    {
        /// <summary>
        /// No relationship of the rule.
        /// </summary>
        None,

        /// <summary>
        /// The dependent rules are sub rules of main rule.
        /// </summary>
        SubRule,

        /// <summary>
        /// The dependent rules are derived rules of main rules.
        /// </summary>
        DerivedRule,
    }

    public class ConformanceRuleDependencyInfo
    {
        public string[] BindingRules { get; set; }
        public ConformanceCheckType CheckType { get; set; }
        public ConformanceRuleRelationship RuleRelationship { get; set; }

        public ConformanceRuleDependencyInfo(ConformanceCheckType checkType, ConformanceRuleRelationship ruleRelationship = ConformanceRuleRelationship.None, string[] bindingRules = null)
        {
            this.CheckType = checkType;
            this.RuleRelationship = ruleRelationship;
            this.BindingRules = bindingRules;
        }
    };
}
