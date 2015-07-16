// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Protocols.TestSuites.Validator
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using Microsoft.Protocols.TestTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RuleEngine = ODataValidator.RuleEngine;
    using ValidationService = ODataValidator.ValidationService;
    using System.Collections;

    [TestClass]
    public partial class TestSuiteBase : TestClassBase
    {
        #region Fields

        /// <summary>
        /// The protocol adapter instance.
        /// </summary>
        protected IValidatorAdapter adapter;

        protected RuleEngine.DataService service = new RuleEngine.DataService();

        protected RuleEngine.IDataService dataService = null;

        public static string isVerifyMetadata = "no"; //add for new feature Metadata v4 xml rules test, value "yes" means need test, others will not.

        #endregion

        #region Class Initialization and Cleanup

        /// <summary>
        /// Initialize class fields before running any test case.
        /// </summary>
        /// <param name="context">
        /// Used to store information that is provided to unit tests.
        /// </param>
        [ClassInitialize]
        public static void TestSuiteClassInitialize(TestContext context)
        {
            TestClassBase.Initialize(context);
        }

        /// <summary>
        /// Cleanup class fields after running all test cases.
        /// </summary>
        [ClassCleanup]
        public static void TestSuiteClassCleanup()
        {
            TestClassBase.Cleanup();
        }

        #endregion

        #region Test Initialization and Cleanup

        /// <summary>
        /// Initialize instance fields before running one test case.
        /// </summary>
        protected override void TestInitialize()
        {
            base.TestInitialize();

            // Register the service
            if (dataService == null)
            {
                dataService = new TripPinService();
            }
            service.RegisterDataService(dataService);

            // Create protocol adapter instance.
            this.adapter = Site.GetAdapter<IValidatorAdapter>();
        }

        /// <summary>
        /// Cleanup instance fields after running one test case.
        /// </summary>
        protected override void TestCleanup()
        {
            base.TestCleanup();

            // Un-register the service
            service.UnregisterDataService();
        }

        #endregion

        public void WriteRuleRequestRecordLog(Dictionary<string, string> FailedTestResult)
        {
            System.IO.StreamWriter logStream = this.CreateTestResultFile(this.Site.Properties["Conformance_Response_Data_Path"], ".log");

            if (logStream == null)
                return;

            try
            {
                IDictionaryEnumerator ruleRequestRecord = ((ConformanceDataService)dataService).RequestRecords;
                logStream.WriteLine("------conformance test result ( failed count {0} )------", FailedTestResult.Count);

                while (ruleRequestRecord.MoveNext())
                {
                    string rulename = (string)ruleRequestRecord.Key;
                    if (FailedTestResult.ContainsKey(rulename))
                    {
                        string result = string.Empty;
                        List<string> reqeustRecords = (List<string>)ruleRequestRecord.Value;

                        result += "\t" + reqeustRecords.Count.ToString();

                        foreach (string record in reqeustRecords)
                        {
                            result += "\t" + record;
                        }

                        logStream.WriteLine(string.Format("{0}\t{1}{2}", rulename, FailedTestResult[rulename], result));

                        FailedTestResult.Remove(rulename);
                    }
                    else
                    {
                        List<string> reqeustRecords = (List<string>)ruleRequestRecord.Value;
                        logStream.WriteLine(string.Format("{0}\tsuccess\t{1}", rulename, reqeustRecords.Count));
                    }
                }

                if (FailedTestResult.Count != 0)
                {
                    logStream.WriteLine("some rule's request not in ConformanceDataService.ruleRequestRecord");
                }
                foreach (var ruleResult in FailedTestResult)
                {
                    string rulename = ruleResult.Key;
                    logStream.WriteLine(rulename + "\t" + ruleResult.Value);
                }

                logStream.WriteLine();
            }
            catch (Exception ex)
            {
                logStream.WriteLine("Exception occur when write log");
                logStream.WriteLine(ex);
            }
            finally
            {
                logStream.Close();
            }
        }

        #region Helper methods

        /// <summary>
        /// Get test result folder path by current directory
        /// </summary>
        /// <returns>Test result folder path</returns>
        protected string GetTestResultFolderPathByCurrentDirectory()
        {
            char separator = '\\';
            int curentDicLength = Environment.CurrentDirectory.Length;
            int lastSecondSprIndex = Environment.CurrentDirectory.LastIndexOf(separator, curentDicLength - 5); // Not consider last \\
            string firstPath = Environment.CurrentDirectory.Substring(0, lastSecondSprIndex);
            string secondPath = Environment.CurrentDirectory.Substring(lastSecondSprIndex + 1, curentDicLength - lastSecondSprIndex - 1);
            secondPath = secondPath.Split(separator)[0];

            return string.Format("{0}Log\\{1}", firstPath, secondPath);
        }

        /// <summary>
        /// Create test result file
        /// </summary>
        /// <param name="folderPath">The folder path</param>
        /// <param name="fileName">The file name</param>
        /// <returns>File streamwriter object</returns>
        protected StreamWriter CreateTestResultFile(string folderPath, string fileName)
        {
            StreamWriter file = null;
            string filePath = string.Format("{0}\\{1}", folderPath, fileName);

            // If folder not exists, try to create the folder
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            // If file not exists, try to create the file
            if (!System.IO.File.Exists(string.Format(filePath)))
            {
                try
                {
                    file = System.IO.File.CreateText(filePath);
                }
                catch (Exception ex)
                {
                    Site.Assert.Fail("Cannot create test result out file, the exception is: {0}.", ex.Message);
                }
            }

            return file;
        }

        /// <summary>
        /// Get rule type constants value by rule name
        /// </summary>
        /// <param name="ruleName">Rule name</param>
        /// <returns>Rule type constants value</returns>
        protected int GetRuleTypeConstantsByRuleName(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName))
                return Constants.None;

            string ruleType = ruleName.Substring(0, ruleName.IndexOf('.')).ToLower();
            switch (ruleType)
            {
                case "common":
                    return Constants.Common;
                case "svcdoc":
                    return Constants.ServiceDocument;
                case "entry":
                    return Constants.Entry;
                case "feed":
                    return Constants.Feed;
                case "delta":
                    return Constants.Delta;
                case "entityreference":
                    return Constants.EntityReference;
                case "error":
                    return Constants.Error;
                case "individualproperty":
                    return Constants.IndividualProperty;
                case "minimal":
                    return Constants.Minimal;
                case "intermediate":
                    return Constants.Intermediate;
                case "advanced":
                    return Constants.Advanced;
                default:
                    return Constants.None;
            }
        }

        /// <summary>
        /// Verify some rules as base test case
        /// </summary>
        /// <param name="requestElement">The request element (including: url, format, header)</param>
        /// <param name="expectedRuleList">To be verified rule list</param>
        /// <param name="negativeRuleDic">Those rules which have no such test data to be negative and its expected result</param>
        protected void VerifyRules_BaseTestCase(RequestElement requestElement, List<string> expectedRuleList, Dictionary<List<string>, string> negativeRuleDic = null)
        {
            if (requestElement == null || expectedRuleList == null)
            {
                Site.Assert.Inconclusive("The to be verified rule list is empty.");
                return;
            }

            var verifyAndNegativeRuleList = new List<ValidationElement>()
            {
                new ValidationElement(requestElement, expectedRuleList, negativeRuleDic)
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeRuleList);
        }

        /// <summary>
        /// Verify some rules as base test case
        /// </summary>
        /// <param name="verifyWithNegativeList">The verify and negative rule list for every request element</param>
        protected void VerifyRules_BaseTestCase(List<ValidationElement> verifyWithNegativeList)
        {
            if (verifyWithNegativeList == null || verifyWithNegativeList.Count < 1)
                return;

            #region Definition for params

            int ruleCount = 0;
            string url = string.Empty;
            string format = string.Empty;
            string header = string.Empty;
            string expectedResult = string.Empty;
            string notInListTotal = string.Empty;
            Guid jobID = Guid.Empty;
            ParsedResult parsedResult = null;
            List<string> verifyRules = null;
            List<string> failedRuleList = new List<string>();
            List<string> expectedTotalRuleList = new List<string>();
            List<string> actualTotalRuleList = new List<string>();
            List<string> negativeRuleList = new List<string>();
            List<ValidationService.TestResult> testResults = null;
            Dictionary<List<string>, string> negativeRuleDic = null;

            #endregion

            #region Create test result file

            string fileName = string.Format("{0}_TestResult.txt", TestContext.TestName);
            string folderPath = GetTestResultFolderPathByCurrentDirectory();
            System.IO.StreamWriter file = this.CreateTestResultFile(folderPath, fileName);
            string filePath = string.Format("{0}\\{1}", folderPath, fileName);
            file.WriteLine(string.Format("------Start Test Case {0}------", TestContext.TestName));

            #endregion

            #region Verify the expected rule list

            foreach (var overify in verifyWithNegativeList)
            {
                url = overify.RequestEle.RequestUrl;
                format = overify.RequestEle.RequestFormat;
                header = overify.RequestEle.RequestHeader;
                verifyRules = overify.PassRuleList;
                negativeRuleDic = overify.NegativeRuleDic;

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(format) || string.IsNullOrEmpty(header) || verifyRules == null || verifyRules.Count < 1)
                    continue;

                jobID = this.adapter.SendRequest(url, format, null, header, null, isVerifyMetadata, null)[0];
                Site.Assert.IsTrue(jobID != Guid.Empty, "No JobGroup is generated.");

                if (this.adapter.IsJobCompleted(jobID, out ruleCount))
                {
                    testResults = this.adapter.GetTestResults(jobID);
                    Site.Assert.IsTrue(testResults.Count == ruleCount,
                    string.Format("Job Verification Result - There should be {0} rules verified but actually get {1} rules.", ruleCount, testResults.Count));

                    parsedResult = this.adapter.ParseResults(testResults);
                }

                foreach (var rule in verifyRules)
                {
                    if (!expectedTotalRuleList.Contains(rule))
                    {
                        expectedTotalRuleList.Add(rule);
                    }

                    Site.Log.Add(LogEntryKind.Debug, "Verify " + rule);
                    if (!parsedResult.RuleNameAndResult.ContainsKey(rule))
                    {
                        file.WriteLine("{0}:\tNot in DB", rule);
                        continue;
                    }
                    if (ValidationResultConstants.Success != parsedResult.RuleNameAndResult[rule])
                    {
                        file.WriteLine("{0}:\t{1}", rule, parsedResult.RuleNameAndResult[rule] + '\t' + parsedResult.RuleNameAndDescription[rule]);
                        failedRuleList.Add(rule);
                        continue;
                    }
                    Site.CaptureRequirementIfAreEqual(
                           ValidationResultConstants.Success,
                           parsedResult.RuleNameAndResult[rule],
                           this.GetRuleTypeConstantsByRuleName(rule) + Int32.Parse(rule.Split('.')[2]),
                           parsedResult.RuleNameAndDescription[rule]);
                }

                foreach (var rule in parsedResult.RuleNameAndResult.Keys)
                {
                    if (!actualTotalRuleList.Contains(rule) && rule.Split('.')[2].StartsWith("4"))
                    {
                        actualTotalRuleList.Add(rule);
                    }
                }

                #region Verify negative rule list

                if (negativeRuleDic != null)
                {
                    foreach (var okey in negativeRuleDic)
                    {
                        if (okey.Key == null)
                            continue;

                        foreach (string rule in okey.Key)
                        {
                            expectedResult = okey.Value;
                            Site.Log.Add(LogEntryKind.Debug, "Verify " + rule);
                            Site.Assert.IsTrue(parsedResult.RuleNameAndResult.ContainsKey(rule),
                            string.Format("The expected rule {0} does not exist in database.", rule));

                            Site.CaptureRequirementIfAreEqual(
                                expectedResult,
                                parsedResult.RuleNameAndResult[rule],
                                this.GetRuleTypeConstantsByRuleName(rule) + Int32.Parse(rule.Split('.')[2]),
                                parsedResult.RuleNameAndDescription[rule]);

                            if (!negativeRuleList.Contains(rule))
                            {
                                negativeRuleList.Add(rule);
                            }
                        }
                    }
                }

                #endregion
            }

            #endregion

            #region Summary the test result and output in file

            file.WriteLine();
            file.WriteLine("Summary:");
            file.WriteLine("Expected {0} rules executed, actually {1} rules executed, {3} negative rules not verified, fail Count {2}.",
                expectedTotalRuleList.Count + negativeRuleList.Count, actualTotalRuleList.Count, failedRuleList.Count, negativeRuleList.Count);

            foreach (string act in actualTotalRuleList)
            {
                if (!expectedTotalRuleList.Contains(act) && !negativeRuleList.Contains(act))
                {
                    notInListTotal += act + " ";
                }
            }
            if (!string.IsNullOrEmpty(notInListTotal))
            {
                file.WriteLine();
                file.WriteLine("Not in rule LIST:\t {0}", notInListTotal);
            }
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
