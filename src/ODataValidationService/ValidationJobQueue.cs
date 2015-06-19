// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.ValidationService
{
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Validation Job Queue for queuing the jobs submitted</summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:Rename type name 'ValidationJobQueue' so that it does not end in 'Queue'.", Justification = "prefer to naming woth hint of implementation")]
    public static class ValidationJobQueue
    {
        /// <summary>Job Queue</summary>
        private static BlockingCollection<ValidationJobState> jobQueue = new BlockingCollection<ValidationJobState>();

        /// <summary>Count of number of job in the queue</summary>
        public static int Count
        {
            get
            {
                return jobQueue.Count;
            }
        }

        /// <summary>Enqueue job</summary>
        /// <param name="jobState">ValidationJobState for enqueue</param>
        public static void EnqueueJob(ValidationJobState jobState)
        {
            jobQueue.Add(jobState);
        }

        /// <summary>Dequeue job</summary>
        /// <returns>Return the dequeue ValidationJobState</returns>
        public static ValidationJobState DequeueJob()
        {
            return jobQueue.Take();
        }
    }
}
