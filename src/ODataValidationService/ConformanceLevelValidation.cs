// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.ValidationService
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using RuleEngine;

    /// <summary>Process rules for conformance level.</summary>
    public static class ConformanceLevelValidation
    {
        private static List<TestResult> allRulesResult = new List<TestResult>();
        private static List<TestResult> pendingRulesResult = new List<TestResult>();

        /// <summary>
        /// Get all conformance rules from TestResult table.
        /// </summary>
        /// <param name="guid">The job id of conformance rules.</param>
        public static void GetAllConformanceLevelRules(Guid guid)
        {
            try
            {
                allRulesResult.Clear();

                using (var ctx = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
                {
                    var testResults = (from j in ctx.TestResults
                                       where guid == j.ValidationJobID
                                       orderby j.ID descending
                                       select j);

                    if (testResults != null)
                    {
                        foreach (TestResult result in testResults)
                        {
                                allRulesResult.Add(result);
                         }

                        pendingRulesResult = (from j in allRulesResult
                                              where j.Classification == "pending"
                                              select j).ToList();
                    }
                }
            }
            catch (System.Data.OptimisticConcurrencyException)
            {
                // error occurred while trying to mark operation as complete.  This is not a terminal error for this system and 
                // this is on a threadpool thread so swallow the exception
            }
        }

        /// <summary>
        /// Update dependent rule's result according to bingding rules' result.
        /// </summary>
        /// <param name="rule">The dependent rule.</param>
        public static void UpdateRuleTestResult(Rule rule)
        {
            List<string> passedRules = new List<string>();
            List<string> failedRules = new List<string>();
            List<string> recommendationRules = new List<string>();
            List<string> warningRules = new List<string>();
            List<string> abortRules = new List<string>();
            List<string> inapplicableRules = new List<string>();
            string ruleResult = string.Empty;
            string errorMessage = string.Empty;
            IEnumerable<TestResult> testResults = null;

            // Whether the rule exists in all rules.
            var mainRule = (from j in pendingRulesResult
                            where j.RuleName == rule.Name
                            select j).FirstOrDefault();

            if (mainRule == null)
            {
                return;
            }

            if (rule.DependencyInfo.CheckType == ConformanceCheckType.AllMinimal)
            {
                testResults = (from j in allRulesResult
                               where j.RuleName.StartsWith("Minimal.Conformance.") && j.Classification != "pending"
                               select j);
            }
            else if (rule.DependencyInfo.CheckType == ConformanceCheckType.AllIntermediate)
            {
                testResults = (from j in allRulesResult
                               where j.RuleName.StartsWith("Intermediate.Conformance.") && j.Classification != "pending"
                               select j);
            }
            else
            {
                // Select all binding rules.
                testResults = (from j in allRulesResult
                               where rule.DependencyInfo.BindingRules.Contains(j.RuleName)
                               select j);
            }

            if (testResults != null)
            {
                int successNum = 0;

                foreach (var result in testResults)
                {
                    if (result.Classification.Equals("success"))
                    {
                        passedRules.Add(result.RuleName);
                        successNum++;
                    }
                    else if (result.Classification.Equals("error") || result.Classification.Equals("aborted"))
                    {
                        failedRules.Add(result.RuleName);
                    }
                    else if (result.Classification.Equals("recommendation"))
                    {
                        recommendationRules.Add(result.RuleName);
                    }
                    else if (result.Classification.Equals("warning"))
                    {
                        warningRules.Add(result.RuleName);
                    }
                    else if (result.Classification.Equals("notApplicable"))
                    {
                        inapplicableRules.Add(result.RuleName);
                    }
                }

                if (rule.DependencyInfo.CheckType == ConformanceCheckType.AllPass)
                {
                    ruleResult = passedRules.Count == rule.DependencyInfo.BindingRules.Length ? "success" : "error";
                    if (failedRules.Count > 0)
                    {
                        ruleResult = "error";
                    }
                    else if (warningRules.Count > 0)
                    {
                        ruleResult = "warning";
                    }
                    else if (recommendationRules.Count > 0)
                    {
                        ruleResult = "recommendation";
                    }
                    else if (inapplicableRules.Count > 0)
                    {
                        ruleResult = "notApplicable";
                    }
                    else
                    {
                        ruleResult = "success";
                    }
                }
                else //if (checkType == CheckType.MustRulesPass)
                {
                    ruleResult = failedRules.Count == 0 ? "success" : "error";
                }

                if (rule.DependencyInfo.CheckType == ConformanceCheckType.AllMinimal && ruleResult.Equals("error"))
                {
                    errorMessage = "Please check the results of Minimal conformance level rules.";
                }
                else if (rule.DependencyInfo.CheckType == ConformanceCheckType.AllIntermediate && ruleResult.Equals("error"))
                {
                    errorMessage = "Please check the results of Intermediate conformance level rules.";
                }
                else
                {
                    if (rule.DependencyInfo.RuleRelationship == ConformanceRuleRelationship.DerivedRule)
                    {
                        errorMessage = successNum + " of " + rule.DependencyInfo.BindingRules.Length + " derived rules pass in the validation. ";
                    }
                    else if (rule.DependencyInfo.RuleRelationship == ConformanceRuleRelationship.SubRule)
                    {
                        errorMessage = successNum + " of " + rule.DependencyInfo.BindingRules.Length + " sub rules pass in the validation. ";
                    }
                }

                WriteToTestResult(mainRule.ID, ruleResult, errorMessage);
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Write conformance test result to database.
        /// </summary>
        /// <param name="testResultID">The updated rule's test result ID.</param>
        /// <param name="result">The result info.</param>
        /// <param name="errorMessage">The error message.</param>
        private static void WriteToTestResult(int testResultID, string result, string errorMessage)
        {
            try
            {
                var testResult = (from j in allRulesResult
                                  where j.ID == testResultID
                                  select j).FirstOrDefault();

                if (testResult != null)
                {
                    using (var ctx = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
                    {
                        var resultInTable = (from j in ctx.TestResults
                                             where j.ID == testResultID && j.ValidationJobID == testResult.ValidationJobID
                                             select j).FirstOrDefault();

                        if (resultInTable != null)
                        {
                            resultInTable.Classification = result;
                            resultInTable.ErrorMessage = errorMessage;
                            ctx.SaveChanges();

                            var detailInTable = (from j in ctx.ResultDetails
                                                 where j.TestResultID == testResultID
                                                 select j);
                            if (detailInTable != null && detailInTable.Count() > 0)
                            {
                                detailInTable.FirstOrDefault().ErrorMessage = errorMessage;
                            }
                            else
                            {
                                ResultDetail resultDetailInDB = new ResultDetail();
                                resultDetailInDB.TestResultID = resultInTable.ID;
                                resultDetailInDB.RuleName = resultInTable.RuleName;
                                resultDetailInDB.URI = "";
                                resultDetailInDB.HTTPMethod = "";
                                resultDetailInDB.RequestHeaders = "";
                                resultDetailInDB.RequestData = "";
                                resultDetailInDB.ResponseStatusCode = "";
                                resultDetailInDB.ResponseHeaders = "";
                                resultDetailInDB.ResponsePayload = "";
                                resultDetailInDB.ErrorMessage = errorMessage;
                                ctx.AddToResultDetails(resultDetailInDB);
                            }

                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (System.Data.OptimisticConcurrencyException)
            {
                // error occurred while trying to mark operation as complete.  This is not a terminal error for this system and 
                // this is on a thread-pool thread so swallow the exception
            }
        }

        /// <summary>
        /// Update all conformance level rules test result.
        /// </summary>
        /// <param name="guid"></param>
        public static void UpdateAllConformanceLevelRules(Guid guid)
        {
            GetAllConformanceLevelRules(guid);
            // Update rule test result for dependency rules
            var dependencyRules = RuleCatalogCollection.Instance.Where(r => pendingRulesResult.Any(u => r.Name == u.RuleName && r.Category.Equals("conformance", StringComparison.OrdinalIgnoreCase))).ToList();
            foreach (var dependencyRule in dependencyRules)
            {
                UpdateRuleTestResult(dependencyRule);
            }
        }
    }
}
