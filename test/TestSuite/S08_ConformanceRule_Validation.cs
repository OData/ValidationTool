// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Protocols.TestSuites.Validator
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Protocols.TestTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RuleEngine = ODataValidator.RuleEngine;
    using ValidationService = ODataValidator.ValidationService;

    /// <summary>
    /// This class contains test cases for creating objects.
    /// </summary>
    [TestClass]
    public class S08_ConformanceRule_Validation : TestSuiteBase
    {
        #region Class Initialization and Cleanup

        /// <summary>
        /// Initialize class fields before running any test case.
        /// </summary>
        /// <param name="context">
        /// Used to store information that is provided to unit tests.
        /// </param>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TestSuiteBase.TestSuiteClassInitialize(context);
        }

        protected override void TestInitialize()
        {
            dataService = new ConformanceDataService();
            base.TestInitialize();
        }

        /// <summary>
        /// Cleanup class fields after running all test cases.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestSuiteBase.TestSuiteClassCleanup();
        }

        #endregion

        #region Test Cases

        /// <summary>
        /// Test case to Verify Entity_Expand Rule
        /// </summary>
        [TestMethod]
        public void S08_TC01_ConformanceRule_Verify()
        {
            #region Execute conformance rule validation jobs

            int ruleCount = 0;
            int ConformanceRuleCount = 90;
            List<ValidationService.TestResult> testResults = null;

            Guid jobID = this.adapter.SendRequest(URL_SrvDocConstants.URL_SrcDoc_Conformance, FormatConstants.V4FormatJsonFullMetadata, null, RequestHeaderConstants.V4Version, "ReadWrite", "no", "Minimal,Intermediate,Advanced")[0];
            Site.Assert.IsTrue(jobID != Guid.Empty, "No JobGroup is generated.");

            if (this.adapter.IsJobCompleted(jobID, out ruleCount))
            {
                testResults = this.adapter.GetTestResults(jobID);
                Site.Assert.IsTrue(testResults.Count == ConformanceRuleCount,
                string.Format("Job Verification Result - There should be {0} rules verified but actually get {1} rules.", ruleCount, testResults.Count));
            }

            #endregion

            #region Analyze test result and record result in file

            List<string> failedRuleList = new List<string>();
            List<string> negativeRuleList = new List<string>()
            {
                "Intermediate.Conformance.1010", "Minimal.Conformance.1009", "Minimal.Conformance.1010", "Minimal.Conformance.1011",
            };
            for (int i = 0; i < testResults.Count; i++)
            {
                if (testResults[i].Classification != ValidationResultConstants.Success
                    && testResults[i].Classification != ValidationResultConstants.Skip)
                {
                    failedRuleList.Add(testResults[i].RuleName);
                    continue;
                }
                int validationID = this.GetRuleTypeConstantsByRuleName(testResults[i].RuleName) + Int32.Parse(testResults[i].RuleName.Split('.')[2]);
                Site.CaptureRequirementIfAreEqual(
                    negativeRuleList.Contains(testResults[i].RuleName) ? ValidationResultConstants.Skip : ValidationResultConstants.Success,
                       testResults[i].Classification,
                       validationID,
                       testResults[i].Description);
            }

            string fileName = string.Format("{0}_TestResult.txt", TestContext.TestName);
            string folderPath = GetTestResultFolderPathByCurrentDirectory();
            System.IO.StreamWriter file = this.CreateTestResultFile(folderPath, fileName);
            string filePath = string.Format("{0}\\{1}", folderPath, fileName);
            file.WriteLine(string.Format("------Start Test Case {0}------", TestContext.TestName));

            failedRuleList.Sort();
            for (int j = 0; j < failedRuleList.Count; j++)
            {
                int i = 0;
                while (failedRuleList[j] != testResults[i].RuleName)
                {
                    i++;
                }

                file.WriteLine("{0}:\t{1}", testResults[i].RuleName, testResults[i].Classification + "\t" + testResults[i].Description);
            }
            file.WriteLine("Expected {0} rules executed, actually {1} rules executed, {3} rules not verified, fail Count {2}.",
                ConformanceRuleCount,
                testResults.Count,
                failedRuleList.Count,
                negativeRuleList.Count);

            file.Close();

            if (this.adapter.GetRulesCountByRequirementLevel(failedRuleList, filePath))
            {
                Site.Assert.Fail("There are {0} rules failed, for detail, please check the failure result from file:\n{1}.", failedRuleList.Count, filePath);
            }

            #endregion
        }

        #endregion
    }
}
