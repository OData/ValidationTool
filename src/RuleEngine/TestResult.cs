// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using ODataValidator.RuleEngine.Common;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// Class of checking result details
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Constructor to initialize CheckResult object
        /// </summary>
        public TestResult()
        {
            this.LineNumberInError = -1;
        }

        /// <summary>
        /// Gets/sets classification: success, warning, error, recommendation
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// Gets/sets rule name
        /// </summary>
        public string RuleName { get; private set; }

        /// <summary>
        /// Get/sets payload target (recognized payload type)
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// Get/sets rule description
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets/sets rule error message
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets/sets requirement level of the validated rule: must, should, may, recommended, etc
        /// </summary>
        public RequirementLevel RequirementLevel { get; private set; }

        /// <summary>
        /// Gets/sets category value of the validated rule
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// Gets/sets specification section information of the validated rule
        /// </summary>
        public string SpecificationSection { get; private set; }

        /// <summary>
        /// Gets/sets odata v4 specification section information of the validated rule
        /// </summary>
        public string V4SpecificationSection { get; private set; }

        /// <summary>
        /// Gets/sets odata v4 specification information of the validated rule
        /// </summary>
        public string V4Specification { get; private set; }

        /// <summary>
        /// Gets/sets odata version of the validated rule
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Gets/sets help link's uri 
        /// </summary>
        public string HelpLink { get; private set; }

        /// <summary>
        /// Gets/sets significant text portion for found issues (such as offending header field and value)
        /// </summary>
        public string TextInvolved { get; set; }

        /// <summary>
        /// Gets/sets the line number of payload content for found issues
        /// </summary>
        public int LineNumberInError { get; set; }

        /// <summary>
        /// Gets/sets validation job id of the interop context
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Gets/sets supplementary error information for internal use
        /// </summary>
        public string ErrorDetail { get; set; }

        /// <summary>
        /// Gets the detail information of the validation.
        /// </summary>
        public List<ExtensionRuleResultDetail> Details { get; set; }

        /// <summary>
        /// Creates an aborted test result from rule definition and job ID
        /// </summary>
        /// <param name="rule">The rule this aborted result is about</param>
        /// <param name="jobId">The job this aborted validation is part of</param>
        /// <returns>TestResult object for aborted validation</returns>
        public static TestResult CreateAbortedResult(Rule rule, Guid jobId)
        {
            TestResult result = new TestResult();
            result.JobId = jobId;
            result.SetProperties(rule, true);
            result.Classification = Constants.ClassificationAborted;
            return result;
        }

        /// <summary>
        /// Sets properties properly based on execution result of specified rule
        /// </summary>
        /// <param name="rule">the rule validated</param>
        /// <param name="passed">whether the rule passed or not</param>
        public void SetProperties(Rule rule, bool passed)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            if (rule.DependencyType.HasValue) // For dependency and skip rule, set the default test result
            {
                if (rule.DependencyType.Value == ConformanceDependencyType.Dependency)
                {
                    this.Classification = Constants.ClassificationPending;
                }
                else if (rule.DependencyType.Value == ConformanceDependencyType.Skip)
                {
                    this.Classification = Constants.ClassificationSkip;
                }
            }
            if (string.IsNullOrEmpty(this.Classification))
            {
                if (passed)
                {
                    this.Classification = Constants.ClassificationSuccess;
                }
                else
                {
                    switch (rule.RequirementLevel)
                    {
                        case RequirementLevel.Must:
                        case RequirementLevel.MustNot:
                            this.Classification = Constants.ClassificationError;
                            break;
                        case RequirementLevel.Should:
                        case RequirementLevel.ShouldNot:
                            this.Classification = Constants.ClassificationWarning;
                            break;
                        case RequirementLevel.May:
                        case RequirementLevel.Recommended:
                            this.Classification = Constants.ClassificationRecommendation;
                            break;
                        default:
                            this.Classification = Constants.ClassificationWarning;
                            break;
                    }

                    this.Description = rule.ErrorMessage;
                }
            }

            this.RuleName = rule.Name;
            this.Category = rule.Category;
            this.HelpLink = rule.HelpLink;
            this.SpecificationSection = rule.SpecificationSection;
            this.V4SpecificationSection = rule.V4SpecificationSection;
            this.V4Specification = rule.V4Specification;
            this.RequirementLevel = rule.RequirementLevel;
            this.Target = rule.PayloadType.HasValue ? rule.PayloadType.Value.ToString() : null;
            this.Description = rule.Description;
            this.ErrorMessage = rule.ErrorMessage;
            this.Version = rule.Version.HasValue ? rule.Version.Value.ToString() : null;
        }
    }
}