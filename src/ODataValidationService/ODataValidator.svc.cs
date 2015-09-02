// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.ValidationService
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Web;
    using System.Threading;
    using System.Web;
    using Eucritta;
    using RuleEngine;
    using System.Text.RegularExpressions;

    /// <summary>ODataValidator class hosting the OData service for job submission and rule processing</summary>
    [System.ServiceModel.ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [JSONPSupportBehavior]
    public sealed class ODataValidator : DataService<ODataValidationSuiteEntities>, IDisposable
    {
        /// <summary>Name of the service</summary>
        public const string ServiceName = "odatavalidator";

        /// <summary>Maximum bytes of the payload accepted</summary>
        private static int MaxPayloadByteCount;

        /// <summary>Maximum size of the ValidationJobQueue</summary>
        private static int MaxValidationJobQueueSize;

        /// <summary>rule store folder path for rules</summary>
        private static string RulestorePath;

        /// <summary>Folder path for extension rule assemblies</summary>
        private static string ExtensionStorePath;

        /// <summary>Represent an instance of ValidationJobState</summary>
        private List<ValidationJobState> jobStates = new List<ValidationJobState>();

        /// <summary>Represent how many ValidationJobState is queued</summary>
        private int validationQueued = 0;

        /// <summary>Constructor</summary>
        [SuppressMessage("Microsoft.Performance", "CA1810: AvoidStaticConstructor", Justification = "static member initialization has logic which oneliner cannot accommodate")]
        static ODataValidator()
        {
            // Get the 3 term documents.
            RuleEngine.Common.TermDocuments.GetInstance();
            ThreadPool.QueueUserWorkItem(new WaitCallback(ValidationJobQueueWorkers.ValidationQueueListener));

            if (!int.TryParse(ConfigurationManager.AppSettings["MaxPayloadByteCount"], out MaxPayloadByteCount))
            {
                // Set the maximum bytes of the payload accepted to 1 MB
                MaxPayloadByteCount = 1000000;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings["MaxValidationJobQueueSize"], out MaxValidationJobQueueSize))
            {
                MaxValidationJobQueueSize = 20;
            }

            string serverMappedRulestorePath = string.Empty;
            string serverMappedExtensionStorePath = string.Empty;

            // Add for Test project
            if (DataService.serviceInstance != null)
            {
                DataService service = new DataService();
                serverMappedRulestorePath = service.GetRulestorePath();
                serverMappedExtensionStorePath = service.GetExtensionStorePath();
            }
            else
            {
                RulestorePath = ConfigurationManager.AppSettings["RulestorePath"] ?? "~/rulestore";
                ODataValidator.ExtensionStorePath = ConfigurationManager.AppSettings["ExtensionStorePath"] ?? "~/extensions";
                serverMappedRulestorePath = HttpContext.Current.Server.MapPath(RulestorePath);
                serverMappedExtensionStorePath = HttpContext.Current.Server.MapPath(ODataValidator.ExtensionStorePath);
            }

            // Load the rule catalog from the rule store
            Guid initJobId = Guid.NewGuid();
            ILogger logger = new DatabaseResultProvider(initJobId);
            RuleStoreAsXmlFolder ruleStore = new RuleStoreAsXmlFolder(serverMappedRulestorePath, logger);
            foreach (var r in ruleStore.GetRules())
            {
                RuleCatalogCollection.Instance.Add(r);
            }

            // Load the rule catalog from the extension rule store
            ExtensionRuleStore extensionStore = new ExtensionRuleStore(serverMappedExtensionStorePath, logger);
            foreach (var rule in extensionStore.GetRules())
            {
                if (rule.IsValid())
                {
                    RuleCatalogCollection.Instance.Add(rule);
                }
            }
        }

        /// <summary>Public constructor</summary>
        public ODataValidator()
        {
            this.ProcessingPipeline.ProcessingRequest += new EventHandler<DataServiceProcessingPipelineEventArgs>(this.ProcessingPipeline_ProcessingRequest);
        }

        /// <summary>Initialize the OData Service</summary>
        /// <param name="config">Service configuration</param>
        [SuppressMessage("Microsoft.Design", "CA1062: Validate input parameters before using them", Justification = "framework ensures they have valid values")]
        public static void InitializeService(DataServiceConfiguration config)
        {
            config.SetEntitySetAccessRule("ValidationJobs", EntitySetRights.ReadSingle);
            config.SetEntitySetAccessRule("ExtValidationJobs", EntitySetRights.ReadSingle | EntitySetRights.WriteAppend);
            config.SetEntitySetAccessRule("JobGroups", EntitySetRights.AllRead);
            config.SetEntitySetAccessRule("TestResults", EntitySetRights.AllRead);
            config.SetEntitySetAccessRule("PayloadLines", EntitySetRights.ReadMultiple);
            config.SetEntitySetAccessRule("ResultDetails", EntitySetRights.AllRead);
            config.SetServiceOperationAccessRule("UriValidationJobs", ServiceOperationRights.ReadMultiple);
            config.SetServiceOperationAccessRule("ConformanceRerunJob", ServiceOperationRights.ReadMultiple);
            config.SetServiceOperationAccessRule("SimpleRerunJob", ServiceOperationRights.ReadMultiple);
            config.SetServiceOperationAccessRule("Reload", ServiceOperationRights.ReadMultiple);
            config.SetEntitySetAccessRule("Records", EntitySetRights.ReadSingle | EntitySetRights.WriteAppend);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            config.UseVerboseErrors = true;
        }

        /// <summary>
        /// Returns a collection of validation jobs from one validation request
        /// </summary>
        /// <param name="Uri">The Uri input of the validation request</param>
        /// <param name="Format">The format hint of the validation request</param>
        /// <param name="toCrawl">indicating whether the request is crawling ("yes") or not (other than "yes") </param>
        /// <param name="byMetadata">indicating whether the request is metadata validation ("yes") or not (other than "yes") </param>
        /// <param name="Headers">The request headers.</param>
        /// <param name="isConformance">The Conformance ReadWrite/ReadOnly text.</param>
        /// <param name="levelTypes">The validated conformance level types.</param>
        /// <returns>The collection of validation jobs</returns>
        [WebGet]
        public IEnumerable<JobGroup> UriValidationJobs(string Uri, string Format, string toCrawl, string byMetadata, string Headers, string isConformance, string levelTypes = null, string serviceImplementation = null)
        {
            Uri = HttpUtility.UrlDecode(Uri).Trim();

            try
            {
                // Try to get the necessary information for only one time.
                RuleEngine.Common.ServiceStatus.GetInstance(Uri, Headers);
            }
            catch (UnauthorizedAccessException)
            {
                return new JobGroup[] { new JobGroup()
                    {
                        Uri = Uri,
                        MasterJobId = Guid.Empty,
                        DerivativeJobId = Guid.Empty,
                        ResourceType = string.Empty,
                        RuleCount = 0,
                        Issues = "Error: The current user is unauthorized to access the endpoint." 
                    }};
            }
            catch (UriFormatException)
            {
                return new JobGroup[]
                    {
                        new JobGroup(){
                            Uri = Uri==null? "":Uri,
                            MasterJobId = Guid.Empty,
                            DerivativeJobId = Guid.Empty,
                            ResourceType = "",
                            RuleCount = 0,
                            Issues = "Error: The input is not a valid OData service endpoint.",
                        }
                    };
            }

            Format = HttpUtility.UrlDecode(Format);
            toCrawl = HttpUtility.UrlDecode(toCrawl);

            List<KeyValuePair<string, string>> reqHeaders = ToHeaderCollection(Headers);

            if (!string.IsNullOrEmpty(toCrawl) && toCrawl.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                return CreateCrawlingJobsByUri(Uri, Format, reqHeaders);
            }
            else
            {
                if (!string.IsNullOrEmpty(isConformance))
                {
                    return CreateSimpleConformanceValidationJobByUri(Uri, Format, reqHeaders, isConformance, levelTypes);
                }
                else
                {
                    return CreateSimpleValidationJobByUri(Uri, Format, reqHeaders, 
                        !string.IsNullOrEmpty(byMetadata) && byMetadata.Equals("yes", StringComparison.OrdinalIgnoreCase),
                        !string.IsNullOrEmpty(serviceImplementation) && serviceImplementation.Equals("yes", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        /// <summary>
        /// Rerun rules for simple rerun job
        /// </summary>
        /// <param name="jobIdStr">The string for job ID which is the validated job.</param>
        /// <param name="testResultIdsStr">The string for the rerun test result IDs.</param>
        /// <returns>The collection of validation jobs.</returns>
        [WebGet]
        public IEnumerable<JobGroup> SimpleRerunJob(string jobIdStr, string testResultIdsStr, string authorizationHeader)
        {
            Guid masterJobId = new Guid(jobIdStr);
            var testResultIds = testResultIdsStr.TrimEnd(';').Split(';').Select(item => int.Parse(item)).ToList();

            using (var x = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
            {
                JobType rerunType = JobType.None;
                var validationJob = (from j in x.ExtValidationJobs where j.ID == masterJobId select j).FirstOrDefault();

                var toUpdateTestResults = (from t in x.TestResults where testResultIds.Contains(t.ID) select t);
                List<string> ruleNameList = (from t in toUpdateTestResults select t.RuleName).Distinct().ToList();

                string Uri = validationJob.Uri;
                if (validationJob.Complete != true)
                {
                    return new JobGroup[]
                    {
                        new JobGroup(){
                            Uri = Uri==null? "":Uri,
                            MasterJobId = Guid.Empty,
                            DerivativeJobId = Guid.Empty,
                            ResourceType = "Rerun Simple Rules",
                            RuleCount = 0,
                            Issues = "Error: Job is not complete or somebody else is rerunning this job!",
                        }
                    };
                }

                validationJob.Complete = false;
                x.SaveChanges();

                string stringHeaders = validationJob.ReqHeaders;

                if(!string.IsNullOrEmpty(authorizationHeader))
                {
                    stringHeaders = Regex.Replace(stringHeaders, "authorization:.*(;|$)", "", RegexOptions.IgnoreCase);
                    stringHeaders += authorizationHeader.Trim();
                }

                try
                {
                    // Try to get the necessary information for only one time.
                    RuleEngine.Common.ServiceStatus.GetInstance(Uri, stringHeaders);
                }
                catch (UnauthorizedAccessException)
                {
                    return new JobGroup[] { new JobGroup()
                    {
                        Uri = Uri,
                        MasterJobId = Guid.Empty,
                        DerivativeJobId = Guid.Empty,
                        ResourceType = string.Empty,
                        RuleCount = 0,
                        Issues = "Error: The current user is unauthorized to access the endpoint." 
                    }};
                }

                ServiceContext ctxMaster = null;
                string Format = validationJob.Format;
                List<KeyValuePair<string, string>> reqHeaders = ToHeaderCollection(stringHeaders);

                if (string.IsNullOrEmpty(Uri))
                {
                    rerunType = JobType.PayloadRerun;
                    ctxMaster = CreateRuleEngineContext(validationJob);
                }
                else
                {

                    rerunType = JobType.UriRerun;
                    ctxMaster = ServiceContextFactory.Create(Uri, Format, masterJobId, MaxPayloadByteCount, reqHeaders);
                }

                return new JobGroup[] { CreateJobGroupItem(Format, masterJobId, ctxMaster, rerunType, ruleNameList, false) };
            }
        }

        [WebGet]
        public IEnumerable<JobGroup> ConformanceRerunJob(string jobIdStr, string testResultIdsStr, string authorizationHeader)
        {
            Guid masterJobId = new Guid(jobIdStr);
            var testResultIds = testResultIdsStr.TrimEnd(';').Split(';').Select(item => int.Parse(item)).ToList();

            using (var x = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
            {
                JobType rerunType = JobType.ConformanceRerun;
                var validationJob = (from j in x.ExtValidationJobs where j.ID == masterJobId select j).FirstOrDefault();

                var toUpdateTestResults = (from t in x.TestResults where testResultIds.Contains(t.ID) select t);
                List<string> ruleNameList = (from t in toUpdateTestResults select t.RuleName).Distinct().ToList();

                string Uri = validationJob.Uri;
                if (validationJob.Complete != true)
                {
                    return new JobGroup[]
                    {
                        new JobGroup(){
                            Uri = Uri==null? "":Uri,
                            MasterJobId = Guid.Empty,
                            DerivativeJobId = Guid.Empty,
                            ResourceType = "Rerun Conformance Rules",
                            RuleCount = 0,
                            Issues = "Error: Job is not complete or somebody else is rerunning this job!",
                        }
                    };
                }

                validationJob.Complete = false;
                x.SaveChanges();

                string stringHeaders = validationJob.ReqHeaders;

                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    stringHeaders = Regex.Replace(stringHeaders, "authorization:.*(;|$)", "", RegexOptions.IgnoreCase);
                    stringHeaders += authorizationHeader.Trim();
                }

                try
                {
                    // Try to get the necessary information for only one time.
                    RuleEngine.Common.ServiceStatus.GetInstance(Uri, stringHeaders);
                }
                catch (UnauthorizedAccessException)
                {
                    return new JobGroup[] { new JobGroup()
                    {
                        Uri = Uri,
                        MasterJobId = Guid.Empty,
                        DerivativeJobId = Guid.Empty,
                        ResourceType = string.Empty,
                        RuleCount = 0,
                        Issues = "Error: The current user is unauthorized to access the endpoint." 
                    }};
                }

                string Format = validationJob.Format;
                string category = "conformance;" + validationJob.ServiceType + ";" + validationJob.LevelTypes;
                List<KeyValuePair<string, string>> reqHeaders = ToHeaderCollection(stringHeaders);

                ServiceContext ctx = ServiceContextFactory.Create(Uri, Format, masterJobId, MaxPayloadByteCount, reqHeaders, category);

                // make sure the target is a service doc resource
                if (ctx.PayloadType != PayloadType.ServiceDoc)
                {
                    return new JobGroup[] { new JobGroup()
                    {
                        Uri = Uri,
                        MasterJobId = Guid.Empty,
                        DerivativeJobId = Guid.Empty,
                        ResourceType = string.Empty,
                        RuleCount = 0,
                        Issues = "Error: The input is not an OData service document resource.",
                    }};
                }
                else
                {
                    return new JobGroup[] { CreateJobGroupItem(Format, masterJobId, ctx, rerunType, ruleNameList, false) };
                }
            }
        }

        /// <summary>
        /// Reload test result for given jobID
        /// </summary>
        /// <param name="jobIdStr">The string for the JobID of the reload test result.</param>
        /// <returns>The job group for this jobID.</returns>
        [WebGet]
        public IEnumerable<JobGroup> Reload(string jobIdStr)
        {
            Guid jobId = new Guid(jobIdStr);
            using (var x = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
            {
                var jobs = (from j in x.JobGroups
                            where j.MasterJobId == jobId
                            select j);

                if (jobs.Count() != 0)
                {
                    return jobs.ToArray();
                }

                var job = (from j in x.ExtValidationJobs
                           where j.ID == jobId
                           select j).FirstOrDefault();
                if (job != null)
                {
                    return new JobGroup[]
                    {
                        new JobGroup() {
                            Uri = job.Uri==null? "":job.Uri,
                            MasterJobId = job.ID,
                            DerivativeJobId = job.ID,
                            ResourceType = job.ResourceType == null? "":job.ResourceType,
                            RuleCount = job.RuleCount.Value,
                        }
                    };
                }
            }

            return new JobGroup[]
                {
                    new JobGroup() {
                        Uri = "Uri",
                        MasterJobId = Guid.Empty,
                        DerivativeJobId = Guid.Empty,
                        ResourceType = "ResourceType",
                        RuleCount = 0,
                        Issues = "Error: Cannot get the extension validation job information for the given job.",
                    }
                };
        }

        /// <summary>Initialize a new validation job</summary>
        /// <param name="job">Row of ExtValidationJobs table(updateable view)</param>
        /// <param name="op">UpdateOperations</param>
        [ChangeInterceptor("ExtValidationJobs")]
        public void OnInsertJob(ExtValidationJobs job, UpdateOperations op)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }

            // ensure only insert ops are allowed
            ValidateOperationType(op);

            // limit total size of validation job queue
            CheckValidationJobQueueSize();

            job.Complete = false;
            job.CreatedDate = DateTime.Now;
            job.ID = Guid.NewGuid();

            // instantiate validation engine
            var ctx = CreateRuleEngineContext(job);
            IResultProvider resultProvider = new DatabaseResultProvider(job.ID);
            ILogger logger = resultProvider as ILogger;
            RuleEngineWrapper ruleEngine = new RuleEngineWrapper(ctx, resultProvider, logger);

            // set total # of rules to execute for the given URI & format
            job.RuleCount = ruleEngine.RuleCount;
            job.ResourceType = ctx.PayloadType.ToString();
            job.ServiceType = ctx.ServiceType.ToString();
            foreach (var levelType in ctx.LevelTypes)
            {
                job.LevelTypes += Enum.GetName(typeof(ConformanceLevelType), levelType).ToString() + ',';
            }
            job.LevelTypes.TrimEnd(',');

            LogJobRespHeaders(job, ctx);
            AddPayloadLinesToJob(job, ctx);

            // state object is used to start the validation workflow on a threadpool thread 
            this.InitJobState(ruleEngine, job.ID);
        }

        /// <summary>Dispose</summary>
        public void Dispose()
        {
            foreach (var j in this.jobStates)
            {
                Interlocked.Increment(ref this.validationQueued);
                if (j != null && j.RuleEngine != null)
                {
                    ValidationJobQueue.EnqueueJob(j);
                }
            }
        }

        /// <summary>Exception handling</summary>
        /// <param name="args">args</param>
        [SuppressMessage("Microsoft.Design", "CA1062: Validate input parameters befor using them", Justification = "framework ensures they have valid values")]
        [SuppressMessage("Microsoft.Naming", "CA2204: Correct the spelling of the unrecognized token 'OData'", Justification = "OData is protocol name")]
        protected override void HandleException(HandleExceptionArgs args)
        {
            // turn unexpected errors into generic 500 error response
            if (args.Exception != null && args.Exception.InnerException != null && args.Exception.InnerException is RuleEngine.Common.OversizedPayloadException)
            {
                // turn an over-sized payload exception into HTTP error 509 Bandwidth Limit Exceeded
                throw new DataServiceException(509, args.Exception.InnerException.Message);
            }
            else if (args.Exception != null && args.Exception.InnerException != null && args.Exception.InnerException is System.ArgumentException)
            {
                if (((System.ArgumentException)args.Exception.InnerException).ParamName.Equals("destination", StringComparison.OrdinalIgnoreCase))
                {
                    // turn this ArgumentException exception (thrown for unregistered uri schema) into HTTP error 403 Forbidden
                    throw new DataServiceException(403, args.Exception.InnerException.Message);
                }
            }

            if (args.Exception.GetType() != typeof(DataServiceException))
            {
                throw new DataServiceException(500, "Oops. An unexpected error occurred while trying to validate your OData endpoint.");
            }
        }

        private static List<KeyValuePair<string, string>> ToHeaderCollection(string Headers)
        {
            List<KeyValuePair<string, string>> reqHeaders = null;
            if (!string.IsNullOrEmpty(Headers))
            {
                reqHeaders = new List<KeyValuePair<string, string>>();
                string[] lines = Headers.Split(';');
                foreach (var line in lines)
                {
                    string[] pair = line.Split(new char[] { ':' }, 2);
                    if (pair.Length == 2)
                    {
                        reqHeaders.Add(new KeyValuePair<string, string>(pair[0].Trim(), HttpUtility.UrlDecode(pair[1].Trim())));
                    }
                    else if (pair.Length == 1)
                    {
                        reqHeaders.Add(new KeyValuePair<string, string>(pair[0].Trim(), null));
                    }
                }
            }
            return reqHeaders;
        }

        /// <summary>Check the operation type</summary>
        /// <param name="op">UpdateOperations</param>
        private static void ValidateOperationType(UpdateOperations op)
        {
            // ensure only insert operations are supported
            if (op != UpdateOperations.Add || HttpContext.Current.Request.HttpMethod.ToUpperInvariant() != "POST")
            {
                throw new DataServiceException(405, "Method Not Allowed");
            }
        }

        /// <summary>Check the size of ValidationJobQueue</summary>
        private static void CheckValidationJobQueueSize()
        {
            if (ValidationJobQueue.Count > MaxValidationJobQueueSize)
            {
                throw new DataServiceException(503, "Server Busy - Lots of validation going on at the moment. Please retry.");
            }
        }

        private static void AddPayloadLinesToJobInImmediateDBEnv(ExtValidationJobs job, ServiceContext ctx)
        {
            using (var x = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
            {
                var j = x.ExtValidationJobs;
                j.AddObject(job);

                if (ctx.PayloadType == PayloadType.RawValue && ctx.ContentType == PayloadFormat.Image)
                {
                    PayloadLine payloadLine = new PayloadLine();
                    payloadLine.ID = Guid.NewGuid();
                    payloadLine.LineNumber = 1;
                    payloadLine.LineText = "( Image data )";
                    job.PayloadLines.Add(payloadLine);
                }
                else
                {
                    PayloadLine payloadLine;
                    int lineNumber = 0;
                    foreach (var responseLine in ctx.GetPayloadLines())
                    {
                        payloadLine = new PayloadLine();
                        payloadLine.ID = Guid.NewGuid();
                        payloadLine.LineNumber = ++lineNumber;
                        payloadLine.LineText = responseLine;
                        job.PayloadLines.Add(payloadLine);
                    }
                }

                x.SaveChanges();
            }
        }

        /// <summary>
        /// Logs response Http headers for the interop validation context to persistent storage
        /// </summary>
        /// <param name="job">The validation job object</param>
        /// <param name="ctx">The interop validation context</param>
        private static void LogJobRespHeaders(ExtValidationJobs job, ServiceContext ctx)
        {
            if (!string.IsNullOrEmpty(ctx.ResponseHttpHeaders))
            {
                using (var x = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
                {
                    var j = x.JobData;
                    JobData jobData = new JobData();
                    jobData.ID = Guid.NewGuid();
                    jobData.RespHeaders = ctx.ResponseHttpHeaders;
                    jobData.JobID = job.ID;

                    j.AddObject(jobData);
                    x.SaveChanges();
                }
            }
        }

        /// <summary>Add the payload lines to the PayloadLines table</summary>
        /// <param name="job">Row of ValidationJob</param>
        /// <param name="ctx">ServiceContext</param>
        private static void AddPayloadLinesToJob(ExtValidationJobs job, ServiceContext ctx)
        {
            if (ctx.PayloadType == PayloadType.RawValue && ctx.ContentType == PayloadFormat.Image)
            {
                PayloadLine payloadLine = new PayloadLine();
                payloadLine.ID = Guid.NewGuid();
                payloadLine.LineNumber = 1;
                payloadLine.LineText = "( Image data )";
                job.PayloadLines.Add(payloadLine);
            }
            else
            {
                PayloadLine payloadLine;
                int lineNumber = 0;
                foreach (var responseLine in ctx.GetPayloadLines())
                {
                    payloadLine = new PayloadLine();
                    payloadLine.ID = Guid.NewGuid();
                    payloadLine.LineNumber = ++lineNumber;
                    payloadLine.LineText = responseLine;
                    job.PayloadLines.Add(payloadLine);
                }
            }
        }

        /// <summary>Create the ServiceContext based on the ValidationJob</summary>
        /// <param name="job">Row of ValidationJob in the SQL table</param>
        /// <returns>ServiceContext class</returns>
        private static ServiceContext CreateRuleEngineContext(ExtValidationJobs job)
        {
            if (!string.IsNullOrEmpty(job.Uri))
            {
                // an online validation
                return ServiceContextFactory.Create(job.Uri, job.Format, job.ID, MaxPayloadByteCount, ToHeaderCollection(job.ReqHeaders));
            }
            else
            {
                // an offline validation
                if (job.PayloadText.Length > MaxPayloadByteCount || job.MetadataText.Length > MaxPayloadByteCount)
                {
                    throw new DataServiceException(509, "Content exceeds the allowed maximum size. Please copy and paste smaller content and retry.");
                }
                string payloadText = SanitizeXmlLiteral(job.PayloadText);
                string metadataText = SanitizeXmlLiteral(job.MetadataText);
                return ServiceContextFactory.Create(payloadText, metadataText, job.ID, job.ReqHeaders, ToHeaderCollection(job.ReqHeaders));
            }
        }

        /// <summary>
        /// Limits allowable paths to /ValidationJobs(id), /ValidationJobs(id)/TestResults, /ValidationJobs(id)/PayloadLines 
        /// This path scoping is done such that users cannot query on information not relevant to the job they submitted.
        /// Finally, this disallows use of $filter for now to limit cost per request
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void ProcessingPipeline_ProcessingRequest(object sender, DataServiceProcessingPipelineEventArgs e)
        {
            int i;
            for (i = 0; i < e.OperationContext.AbsoluteRequestUri.Segments.Length; i++)
            {
                if (e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith(ServiceName, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            // i at index of segment immediately after the service root segment
            i++;
            if (i < e.OperationContext.AbsoluteRequestUri.Segments.Length &&
                    !e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith("ExtValidationJobs", StringComparison.OrdinalIgnoreCase) &&
                    !e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith("validationjobs", StringComparison.OrdinalIgnoreCase) &&
                    !e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith("$metadata", StringComparison.OrdinalIgnoreCase) &&
                    !e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith("UriValidationJobs", StringComparison.OrdinalIgnoreCase) &&
                    !e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith("ConformanceRerunJob", StringComparison.OrdinalIgnoreCase) &&
                    !e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith("SimpleRerunJob", StringComparison.OrdinalIgnoreCase) &&
                    !e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith("TestResults", StringComparison.OrdinalIgnoreCase) &&
                    !e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith("Records", StringComparison.OrdinalIgnoreCase) &&
                    !e.OperationContext.AbsoluteRequestUri.Segments[i].ToUpperInvariant().StartsWith("Reload", StringComparison.OrdinalIgnoreCase)
                )
            {
                throw new DataServiceException(400, "Test result and payload line sets must be accessed via an associated validation job");
            }

            // for now limiting complexity of queries the service is willing to run
            if (e.OperationContext.AbsoluteRequestUri.Query != null &&
                e.OperationContext.AbsoluteRequestUri.Query.Contains("$filter"))
            {
                throw new DataServiceException(400, "This service does not currently support $filter requests");
            }
        }

        /// <summary>Initialize a new instance ValidationJobState</summary>
        /// <param name="ruleEngine">Rule engine</param>
        /// <param name="jobId">Guid</param>
        private void InitJobState(RuleEngineWrapper ruleEngine, Guid jobId)
        {
            var jobState = new ValidationJobState();
            jobState.RuleEngine = ruleEngine;
            jobState.JobId = jobId;
            this.jobStates.Add(jobState);
        }

        static private string SanitizeXmlLiteral(string input)
        {
            return input.Trim(' ', '\t', '\r', '\n');
        }

        private IEnumerable<JobGroup> CreateSimpleValidationJobByUri(string Uri, string Format, IEnumerable<KeyValuePair<string, string>> reqHeaders, bool isMetadataValidation = false, bool serviceImplementation = false)
        {
            ServiceContext ctx;

            if (serviceImplementation)
            {
                ctx = ServiceContextFactory.Create(Uri, Format, Guid.NewGuid(), MaxPayloadByteCount, reqHeaders, "ServiceImpl");
            }
            else
            {
                ctx = ServiceContextFactory.Create(Uri, Format, Guid.NewGuid(), MaxPayloadByteCount, reqHeaders);
            }

            bool isNotVerifyMetadata = isMetadataValidation ^ (ctx != null && ctx.PayloadType == PayloadType.Metadata);
            bool isVersionNotSupported = ctx != null && !ctx.IsRequestVersion() && (ctx.HttpStatusCode == System.Net.HttpStatusCode.UnsupportedMediaType || ctx.HttpStatusCode == System.Net.HttpStatusCode.BadRequest);
            if (ctx == null || ctx.PayloadType == PayloadType.None || isNotVerifyMetadata || isVersionNotSupported)
            {
                return new JobGroup[] { new JobGroup()
                    {
                        Uri = Uri,
                        MasterJobId = Guid.Empty,
                        DerivativeJobId = Guid.Empty,
                        ResourceType = string.Empty,
                        RuleCount = 0,
                        Issues = isVersionNotSupported
                        ? "Error: The input is not a supported service resource for selected version." 
                        : isNotVerifyMetadata
                        ? isMetadataValidation ? "Error: The input is not an OData metadata resource."
                        : "Error: Not support for OData metadata resource."
                        : "Error: The input is not a valid OData service endpoint.",
                    }};
            }
            else
            {
                return new JobGroup[] { CreateJobGroupItem(Format, ctx.JobId, ctx, JobType.Normal) };
            }
        }

        private IEnumerable<JobGroup> CreateSimpleConformanceValidationJobByUri(string Uri, string Format, IEnumerable<KeyValuePair<string, string>> reqHeaders, string isConformance, string levelTypes)
        {
            string category = "conformance;" + isConformance + ";" + levelTypes;
            ServiceContext ctx = ServiceContextFactory.Create(Uri, Format, Guid.NewGuid(), MaxPayloadByteCount, reqHeaders, category);

            // make sure the target is a service doc resource
            if (ctx.PayloadType != PayloadType.ServiceDoc)
            {
                return new JobGroup[] { new JobGroup()
                    {
                        Uri = Uri,
                        MasterJobId = Guid.Empty,
                        DerivativeJobId = Guid.Empty,
                        ResourceType = string.Empty,
                        RuleCount = 0,
                        Issues = "Error: The input is not an OData service document resource.",
                    }};
            }
            else
            {
                return new JobGroup[] { CreateJobGroupItem(Format, ctx.JobId, ctx, JobType.Conformance) };
            }
        }

        private IEnumerable<JobGroup> CreateCrawlingJobsByUri(string Uri, string Format, IEnumerable<KeyValuePair<string, string>> reqHeaders)
        {
            ServiceContext ctxMaster = ServiceContextFactory.Create(Uri, Format, Guid.NewGuid(), MaxPayloadByteCount, reqHeaders);

            // make sure the master target is a service doc resource
            if (ctxMaster.PayloadType != PayloadType.ServiceDoc)
            {
                return new JobGroup[]
                    {
                        new JobGroup(){
                            Uri = Uri,
                            MasterJobId = Guid.Empty,
                            DerivativeJobId = Guid.Empty,
                            ResourceType = ctxMaster.PayloadType.ToString(),
                            RuleCount = 0,
                            Issues = ctxMaster.HttpStatusCode == System.Net.HttpStatusCode.UnsupportedMediaType 
                            ? "Error: The input is not a supported service resource for selected version."
                            : "Error: The input is not an OData service document resource.",
                        }
                    };
            }

            SimpleJobPlanner jobPlanner = new SimpleJobPlanner(ctxMaster, Format, MaxPayloadByteCount);
            List<KeyValuePair<string, string>> failures;
            var jobs = jobPlanner.GetPlannedJobs(out failures);
            List<JobGroup> jobGroups = new List<JobGroup>();

            foreach (var job in jobs)
            {
                jobGroups.Add(CreateJobGroupItem(Format, ctxMaster.JobId, job, JobType.Uri));
            }

            if (failures != null)
            {
                foreach (var fail in failures)
                {
                    var item = JobGroup.CreateJobGroup(ctxMaster.JobId, "0", 0, fail.Key);
                    item.Issues = fail.Value;
                    jobGroups.Add(item);
                }
            }

            using (var x = SuiteEntitiesUtility.GetODataValidationSuiteEntities())
            {
                var j = x.JobGroups;

                foreach (var job in jobGroups)
                {
                    JobGroup jobGroup = JobGroup.CreateJobGroup(ctxMaster.JobId, job.ResourceType, job.RuleCount, job.Uri);
                    jobGroup.Issues = job.Issues;
                    jobGroup.DerivativeJobId = job.DerivativeJobId;
                    j.AddObject(jobGroup);
                }
                x.SaveChanges();
            }

            return jobGroups;
        }

        private JobGroup CreateJobGroupItem(string format, Guid masterJobId, ServiceContext ctx, JobType jobType, List<string> ruleNameList = null, bool insertJobToDB = true)
        {
            RuleEngine.IResultProvider resultProvider = new DatabaseResultProvider(ctx.JobId, jobType);
            ILogger logger = resultProvider as ILogger;
            RuleEngineWrapper ruleEngine = new RuleEngineWrapper(ctx, resultProvider, logger, ruleNameList);

            // start the validation workflow on the backend threadpool thread 
            this.InitJobState(ruleEngine, ctx.JobId);

            if (insertJobToDB)
            {
                // insert validation job entry, and insert payload lines, to underlying DB
                ExtValidationJobs extJob = new ExtValidationJobs();

                extJob.ID = ctx.JobId;
                extJob.Complete = false;
                extJob.CreatedDate = DateTime.Now;
                extJob.Uri = ctx.Destination.OriginalString;
                extJob.Format = format;
                extJob.RuleCount = ruleEngine.RuleCount;
                extJob.ResourceType = ctx.PayloadType.ToString();
                extJob.ServiceType = ctx.ServiceType.ToString();
                foreach (var levelType in ctx.LevelTypes)
                {
                    extJob.LevelTypes += Enum.GetName(typeof(ConformanceLevelType), levelType).ToString() + ',';
                }
                extJob.LevelTypes = extJob.LevelTypes.TrimEnd(',');

                if (ctx.RequestHeaders != null && ctx.RequestHeaders.Any())
                {
                    var headers = "";
                    foreach (var p in ctx.RequestHeaders)
                    {
                        if (p.Key.Equals("Authorization")) continue;
                        headers += p.Key + ":" + p.Value + ";";
                    }
                    extJob.ReqHeaders = headers;
                }
                AddPayloadLinesToJobInImmediateDBEnv(extJob, ctx);
                LogJobRespHeaders(extJob, ctx);
            }

            // populate the data to return
            JobGroup item = JobGroup.CreateJobGroup(masterJobId, ctx.PayloadType.ToString(), ruleEngine.RuleCount, ctx.Destination.OriginalString);
            item.DerivativeJobId = ctx.JobId;

            return item;
        }
    }
}
