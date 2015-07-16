// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using ODataValidator.ValidationService;
using Microsoft.Protocols.TestTools;

namespace Microsoft.Protocols.TestSuites.Validator
{
    public class ValidatorAdapter : ManagedAdapterBase, IValidatorAdapter
    {
        /// <summary>
        /// 
        /// </summary>
        private ODataValidator.ValidationService.ODataValidator validator;

        /// <summary>
        /// Initialize, generate the transport
        /// </summary>
        /// <param name="testSite">The initialed test site</param>
        public override void Initialize(ITestSite testSite)
        {
            base.Initialize(testSite);
            testSite.DefaultProtocolDocShortName = "ODataValidator";
            TripPinService.SetTestSite(testSite);
            ConformanceDataService.SetTestSite(testSite);
            AtomDataService.SetTestSite(testSite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="format"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public Guid[] SendRequest(string uri, string format, string toCrawl, string headers, string isConformance = null, string isMetaData = "no", string levelTypes = null)
        {
            validator = new ODataValidator.ValidationService.ODataValidator();
            IEnumerable<JobGroup> groups = validator.UriValidationJobs(uri, format, toCrawl, isMetaData, headers, isConformance, levelTypes);
            JobGroup[] JobGroupArray = groups.ToArray<JobGroup>();

            for (int i = 0; i < JobGroupArray.Length; i++)
            {
                if (JobGroupArray[i].MasterJobId == JobGroupArray[i].DerivativeJobId)
                {
                    if (i != 0)
                    {
                        JobGroup temp = JobGroupArray[i];
                        JobGroupArray[i] = JobGroupArray[0];
                        JobGroupArray[0] = temp;
                    }
                    break;
                }
            }

            var jobIDs = (from job in JobGroupArray select job.DerivativeJobId).ToArray();

            if (jobIDs.Length == 0)
                return new Guid[] { Guid.Empty };

            validator.Dispose();

            return (from job in jobIDs where job.HasValue select job.Value).ToArray<Guid>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public bool IsJobCompleted(Guid jobId, out int ruleCount)
        {
            ruleCount = 0;

            try
            {
                while (true)
                {
                    using (var ctx = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
                    {
                        var job = (from j in ctx.ExtValidationJobs
                                   where j.ID == jobId
                                   select j).FirstOrDefault();

                        if (job != null && job.Complete.HasValue && job.Complete == true)
                        {
                            ruleCount = job.RuleCount.Value;
                            return true;
                        }
                        else
                        {
                            Thread.Sleep(5000);
                        }
                    }
                }
            }
            catch (System.Data.OptimisticConcurrencyException)
            {
                return false;
                // error occured while trying to mark operation as complete.  This is not a terminal error for this system and 
                // this is on a threadpool thread so swallow the exception
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public List<TestResult> GetTestResults(Guid jobId)
        {
            List<TestResult> testResults = new List<TestResult>();

            try
            {
                using (var ctx = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
                {
                    var results = from j in ctx.TestResults
                                  where j.ValidationJobID == jobId
                                  select j;

                    testResults = results.ToList();
                }
            }
            catch (System.Data.OptimisticConcurrencyException)
            {
                // error occured while trying to mark operation as complete.  This is not a terminal error for this system and 
                // this is on a threadpool thread so swallow the exception
            }

            return testResults;
        }

        public ParsedResult ParseResults(List<TestResult> results)
        {
            ParsedResult parsedResult = new ParsedResult();
            parsedResult.Parse(results);
            return parsedResult;
        }

        public bool GetRulesCountByRequirementLevel(List<string> RuleNameList, string testResultPath)
        {
            if (null == RuleNameList || RuleNameList.Count == 0)
                return false;

            RuleNameList.Sort();

            List<string> notInRuleArrayNameList = new List<string>();
            // Init level count
            string[] levelList = Enum.GetNames(typeof(ODataValidator.RuleEngine.RequirementLevel));
            Dictionary<string, int> levelCount = new Dictionary<string, int>();
            foreach (var level in levelList)
            {
                levelCount.Add(level, 0);
            }

            // Total the level count
            var ruleArray = ODataValidator.RuleEngine.RuleCatalogCollection.Instance.ToArray();
            foreach (var name in RuleNameList)
            {
                int ruleIndex = ruleArray.ToList().FindIndex(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase) == true);
                if (ruleIndex >= 0)
                {
                    levelCount[Enum.GetName(typeof(ODataValidator.RuleEngine.RequirementLevel), ruleArray[ruleIndex].RequirementLevel)]++;
                }
                else // Cannot find this rule in ruleArry
                {
                    notInRuleArrayNameList.Add(name);
                }
            }

            // Out the log in test result file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(testResultPath, true))
            {
                file.WriteLine();
                foreach (var pair in levelCount)
                {
                    if (pair.Value > 0)
                    {
                        file.WriteLine("{0} : {1}", pair.Key, pair.Value);
                    }
                }
                if (notInRuleArrayNameList.Count > 0)
                {
                    file.WriteLine("Not in Rule Store : {0}", notInRuleArrayNameList.Count);
                    foreach (var oname in notInRuleArrayNameList)
                    {
                        file.WriteLine(string.Format("\t{0}", oname));
                    }
                }
                file.Close();

                return true;
            }
        }
    }
}
