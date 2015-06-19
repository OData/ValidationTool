// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.ValidationService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using RuleEngine;
    /// <summary>Write the test result into TestResults table</summary>
    public class DatabaseResultProvider : IResultProvider, ILogger
    {
        /// <summary>Batch size for writing test result</summary>
        private const int ResultBatchSize = 4;

        /// <summary>Job Id</summary>
        private Guid jobId;

        /// <summary>List of the TestResult</summary>
        private List<TestResult> resultsToSave;

        private JobType jobType;

        /// <summary>List of the detail information.</summary>
        private List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();

        /// <summary>Constructor</summary>
        /// <param name="validationJobId">Guid</param>
        public DatabaseResultProvider(Guid validationJobId, JobType jt = JobType.None)
        {
            this.jobId = validationJobId;
            this.resultsToSave = new List<TestResult>();
            this.jobType = jt;
        }

        /// <summary>Save the test results to TestResults table</summary>
        /// <param name="result">TestResult</param>
        public void Accept(RuleEngine.TestResult result)
        {
            if (result == null)
            {
                return;
            }

            TestResult testResult = new TestResult();
            testResult.ValidationJobID = this.jobId;
            testResult.RuleName = result.RuleName;
            testResult.Description = result.Description;

            // TODO: need ErrorMessage property on CheckResult
            testResult.ErrorMessage = result.ErrorDetail != null ? result.ErrorDetail : string.Empty;
            testResult.HelpUri = result.HelpLink;

            testResult.SpecificationUri = "version:" + result.Version + ";";

            // TODO: need spec back in HTML form.
            if (result.SpecificationSection != null && result.V4SpecificationSection != null)
            {
                testResult.SpecificationUri += "V4SpecificationSection:" + result.V4SpecificationSection + "&SpecificationSection:" + result.SpecificationSection;
            }
            else if (result.SpecificationSection != null && result.V4SpecificationSection == null)
            {
                testResult.SpecificationUri += result.SpecificationSection;
            }
            else
            {
                testResult.SpecificationUri += result.V4SpecificationSection;
            }

            if (result.V4Specification != null)
            {
                testResult.SpecificationUri += ";V4Specification:" + result.V4Specification;
            }

            testResult.Classification = result.Classification;
            testResult.ODataLevel = result.Category;
            testResult.LineNumberInError = result.LineNumberInError.ToString(CultureInfo.CurrentCulture);

            this.resultsToSave.Add(testResult);

            if (result.Details != null)
            {
                foreach (ExtensionRuleResultDetail detail in result.Details)
                {
                    this.details.Add(detail.Clone());
                }
            }

            // save results to DB in batches of 5
            if (this.resultsToSave.Count >= ResultBatchSize)
            {
                using (var ctx = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
                {
                    this.ProcessResultsBatchByJobCompleted(ctx, false);
                }
            }
        }

        /// <summary>Mark the ValidationJob complete</summary>
        /// <param name="errorOccurred">True of false if there's any Rule Engine exception</param>
        public void JobCompleted(bool errorOccurred)
        {
            using (var ctx = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
            {
                this.ProcessResultsBatchByJobCompleted(ctx, true);

                var currentJob = (from j in ctx.ExtValidationJobs
                                  where j.ID == this.jobId
                                  select j).FirstOrDefault();
                if (currentJob == null)
                    return;

                currentJob.ErrorOccurred = errorOccurred;
                currentJob.Complete = true;
                currentJob.CompleteDate = DateTime.Now;
                ctx.SaveChanges();
            }
        }

        private void UpdateSimpleRerunTestResult(out List<TestResult> newTestResult)
        {
            using (var x = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
            {
                this.resultsToSave = this.resultsToSave.OrderBy(item => item.RuleName).ToList();
                var names = (from result in this.resultsToSave orderby result.RuleName select result.RuleName).ToList();

                newTestResult = (
                    from t in x.TestResults
                    where t.ValidationJobID == this.jobId && names.Contains(t.RuleName)
                    orderby t.RuleName
                    select t
                ).ToList();

                for(int i = 0; i < newTestResult.Count;i++)
                {
                    var testResultInDB = newTestResult[i];
                    var testResultNew = this.resultsToSave[i];
                    testResultInDB.Classification = testResultNew.Classification;
                    testResultInDB.AppliesTo = testResultNew.AppliesTo;
                    testResultInDB.ErrorMessage = testResultNew.ErrorMessage;
                }

                x.SaveChanges();
            }

            this.resultsToSave.Clear();
        }

        private void UpdateResultDetails(List<TestResult> newTestResult)
        {
            using (var ctx = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
            {
                foreach (var testResult in newTestResult)
                {
                    var updateExistDetails = (from t in ctx.ResultDetails
                                              where t.TestResultID == testResult.ID
                                              select t).ToList();

                    var newDetails = (from t in this.details
                                      where t.RuleName == testResult.RuleName
                                      select t).ToList();

                    int i = 0, j = 0;
                    for (; i < newDetails.Count; i++)
                    {
                        if (j < updateExistDetails.Count)
                        {
                            updateExistDetails[j].URI = newDetails[i].URI;
                            updateExistDetails[j].HTTPMethod = newDetails[i].HTTPMethod;
                            updateExistDetails[j].RequestHeaders = newDetails[i].RequestHeaders;
                            updateExistDetails[j].RequestData = newDetails[i].RequestData;
                            if (string.IsNullOrEmpty(newDetails[i].ResponseStatusCode))
                            {
                                updateExistDetails[j].ResponseStatusCode = newDetails[i].ResponseStatusCode;
                            }
                            else
                            {
                                HttpStatusCode? responseStatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), newDetails[i].ResponseStatusCode);
                                int statusCode = Convert.ToInt32(responseStatusCode);
                                updateExistDetails[j].ResponseStatusCode = string.Format("{0},{1}", statusCode.ToString(), newDetails[i].ResponseStatusCode.ToString());
                            }
                            updateExistDetails[j].ResponseHeaders = newDetails[i].ResponseHeaders;
                            updateExistDetails[j].ResponsePayload = newDetails[i].ResponsePayload;
                            updateExistDetails[j].ErrorMessage = newDetails[i].ErrorMessage;
                            j++;
                        }
                        else
                        {
                            AddTestResultDetailToDB(ctx, testResult.ID, newDetails[i]);
                        }
                    }
                    for (; j < updateExistDetails.Count; j++)
                    {
                        updateExistDetails[j].ErrorMessage = string.Empty;
                    }

                    ctx.SaveChanges();
                }
            }

            this.details.Clear();
        }

        /// <summary>Save the exception into EngineRuntimeExceptions table</summary>
        /// <param name="runtimeError">Rule Engine exception</param>
        [SuppressMessage("DataWeb.Usage", "AC0014:DoNotHandleProhibitedExceptionsRule", Justification = "Taken care of by similar mechanism")]
        public void Log(RuntimeException runtimeError)
        {
            if (runtimeError == null)
            {
                return;
            }

            try
            {
                using (var ctx = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
                {
                    var runtimeException = EngineRuntimeException.CreateEngineRuntimeException(
                        runtimeError.JobId,
                        runtimeError.RuleName,
                        runtimeError.Timestamp,
                        runtimeError.DestinationEndpoint,
                        0);
                    runtimeException.Message = runtimeError.Message;
                    runtimeException.StackTrace = runtimeError.StackTrace;
                    runtimeException.Detail = runtimeError.Detail;

                    ctx.AddToEngineRuntimeExceptions(runtimeException);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                if (!RuleEngine.Common.ExceptionHelper.IsCatchableExceptionType(ex))
                {
                    throw;
                }

                // swallows the exception since logging is considered low-prio task
            }
        }

        private void ProcessResultsBatchByJobCompleted(ODataValidationSuiteEntities ctx, bool isToCompleteJob = false)
        {
            if (this.jobType != JobType.UriRerun
                  && this.jobType != JobType.PayloadRerun
                  && this.jobType != JobType.ConformanceRerun)
            {
                this.ProcessResultsBatch(ctx);
                // Update conformance dependency rules result
                if (this.jobType == JobType.Conformance && isToCompleteJob)
                {
                    ConformanceLevelValidation.UpdateAllConformanceLevelRules(this.jobId);
                }
            }

            if (this.jobType == JobType.UriRerun
                || this.jobType == JobType.PayloadRerun
                || this.jobType == JobType.ConformanceRerun)
            {
                List<TestResult> newTestResult = null;
                UpdateSimpleRerunTestResult(out newTestResult);
                // Update conformance result details and dependency rules result
                if (this.jobType == JobType.ConformanceRerun)
                {
                    UpdateResultDetails(newTestResult);
                    if (isToCompleteJob)
                    {
                        ConformanceLevelValidation.UpdateAllConformanceLevelRules(this.jobId);
                    }
                }
            }
        }

        /// <summary>Save the test results to TestResults table</summary>
        /// <param name="ctx">ODataValidationSuiteEntities</param>
        private void ProcessResultsBatch(ODataValidationSuiteEntities ctx)
        {
            // write back to the DB in blocks of 5 test results
            foreach (TestResult res in this.resultsToSave)
            {
                ctx.AddToTestResults(res);
            }

            ctx.SaveChanges();

            Dictionary<string, int> dic = new Dictionary<string, int>();

            // Get the rule name and test result id.
            foreach (TestResult res in this.resultsToSave)
            {
                var resultID = from j in ctx.TestResults
                               where j.RuleName == res.RuleName && j.ValidationJobID == res.ValidationJobID
                               orderby j.ID
                               select j.ID;

                if (resultID.Any())
                {
                    dic.Add(res.RuleName, Int32.Parse(resultID.First().ToString()));
                }
            }

            // Write details information to the DB.
            foreach (ExtensionRuleResultDetail detail in this.details)
            {
                AddTestResultDetailToDB(ctx, dic[detail.RuleName], detail);
            }

            ctx.SaveChanges();
            this.resultsToSave.Clear();
            this.details.Clear();
            dic.Clear();
        }

        private void AddTestResultDetailToDB(ODataValidationSuiteEntities ctx, int testResulID, ExtensionRuleResultDetail detail)
        {
            // Write one detail information to the DB.
            ResultDetail resultDetailInDB = new ResultDetail();
            int resultID = testResulID;
            resultDetailInDB.TestResultID = resultID;
            resultDetailInDB.RuleName = detail.RuleName;
            resultDetailInDB.URI = detail.URI;
            resultDetailInDB.HTTPMethod = detail.HTTPMethod;
            resultDetailInDB.RequestHeaders = detail.RequestHeaders;
            resultDetailInDB.RequestData = detail.RequestData;
            if (string.IsNullOrEmpty(detail.ResponseStatusCode))
            {
                resultDetailInDB.ResponseStatusCode = detail.ResponseStatusCode;
            }
            else
            {
                HttpStatusCode? responseStatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), detail.ResponseStatusCode);
                int statusCode = Convert.ToInt32(responseStatusCode);
                resultDetailInDB.ResponseStatusCode = string.Format("{0},{1}", statusCode.ToString(), detail.ResponseStatusCode.ToString());
            }

            resultDetailInDB.ResponseHeaders = detail.ResponseHeaders;
            resultDetailInDB.ResponsePayload = detail.ResponsePayload;
            resultDetailInDB.ErrorMessage = detail.ErrorMessage;

            ctx.AddToResultDetails(resultDetailInDB);
        }
    }
}
