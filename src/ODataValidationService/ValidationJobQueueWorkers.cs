// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.ValidationService
{
    #region Namespaces
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    #endregion

    /// <summary>Class for running the validation job in queue</summary>
    public static class ValidationJobQueueWorkers
    {
        /// <summary>Maximum ValidationJob can run concurrently</summary>
        private static int MaxConcurrentValidationJobs;

        /// <summary>Worker Pool</summary>
        private static Semaphore workerPool;

        /// <summary>Constructor</summary>
        [SuppressMessage("Microsoft.Performance", "CA1810: AvoidStaticConstructor", Justification = "static member initialization has logic which oneliner cannot accommodate")]
        static ValidationJobQueueWorkers()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["MaxConcurrentValidationJobs"], out MaxConcurrentValidationJobs))
            {
                MaxConcurrentValidationJobs = 4;
            }

            if (RuleEngine.DataService.serviceInstance != null)
            {
                MaxConcurrentValidationJobs = 1;
            }

            workerPool = new Semaphore(MaxConcurrentValidationJobs, MaxConcurrentValidationJobs);
        }

        /// <summary>Queue listener</summary>
        /// <param name="state">state</param>
        public static void ValidationQueueListener(object state)
        {
            while (true)
            {
                // blocks until there is an item to dequeue
                var jobState = ValidationJobQueue.DequeueJob();

                if (jobState != null)
                {
                    // enter semaphore
                    workerPool.WaitOne();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ValidationJobWorker), jobState);
                }
            }
        }

        /// <summary>Run the validation job</summary>
        /// <param name="state">state</param>
        [SuppressMessage("DataWeb.Usage", "AC0014:DoNotHandleProhibitedExceptionsRule", Justification = "Taken care of by similar mechanism")]
        private static void ValidationJobWorker(object state)
        {
            ValidationJobState jobState = (ValidationJobState)state;
            try
            {
                // start validation job workflow
                jobState.RuleEngine.Validate();
            }
            catch (Exception ex)
            {
                if (!RuleEngine.Common.ExceptionHelper.IsCatchableExceptionType(ex))
                {
                    throw;
                }

                MarkJobAsCompletedInError(jobState.JobId);
                if (ex is SystemException)
                {
                    throw;
                }
            }
            finally
            {
                workerPool.Release();
            }
        }

        /// <summary>Set the Complete column to true for the row in ValidationJob table</summary>
        /// <param name="jobID">Guid</param>
        private static void MarkJobAsCompletedInError(Guid jobID)
        {
            try
            {
                using (var ctx = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
                {
                    var job = (from j in ctx.ExtValidationJobs
                               where j.ID == jobID
                               select j).FirstOrDefault();
                    if (job != null)
                    {
                        job.Complete = true;
                        job.ErrorOccurred = true;
                        ctx.SaveChanges();
                    }
                }
            }
            catch (System.Data.OptimisticConcurrencyException)
            {
                // error occured while trying to mark operation as complete.  This is not a terminal error for this system and 
                // this is on a threadpool thread so swallow the exception
            }
        }
    }
}
