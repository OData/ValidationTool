// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Eucritta
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    class SimpleJobPlanner : IJobPlanner
    {
        private ServiceContext rootCtx;
        private ServiceContext metaCtx;
        private string[] feeds;
        private string acceptHeaderValue;
        private int maxPayloadSize;
        private string metaResource;
        private string category;

        public SimpleJobPlanner(ServiceContext rootCtx, string formatHint, int maxPayloadSize, string category="core")
        {
            this.rootCtx = rootCtx;
            this.metaResource = rootCtx.DestinationBasePath + "/$metadata";
            this.acceptHeaderValue = formatHint.MapFormatToAcceptValue();
            this.maxPayloadSize = maxPayloadSize;
            this.category = category;
            var payloadFormat = this.rootCtx.ServiceDocument.GetFormatFromPayload();
            this.feeds = ContextHelper.GetFeeds(this.rootCtx.ServiceDocument, payloadFormat).ToArray();
        }

        public IEnumerable<ServiceContext> GetPlannedJobs(out List<KeyValuePair<string, string>> failedTargets)
        {
            List<KeyValuePair<string, Exception>> subFailures = null;
            this.metaCtx = this.CreateMetadataJob(ref subFailures);
            var errorJob = this.CreateErrorPayloadJob(ref subFailures);
            var feedJob = this.CreateFeedJob(ref subFailures);
            ServiceContext entryJob = (feedJob == null || feedJob.PayloadType != PayloadType.Feed) ? null : this.CreateEntryJob(feedJob, feedJob.PayloadFormat, ref subFailures);

            failedTargets = new List<KeyValuePair<string, string>>();
            List<ServiceContext> jobs = new List<ServiceContext>() { this.rootCtx };

            if (subFailures != null)
            {
                failedTargets.AddRange (from fail in subFailures
                    select new KeyValuePair<string, string>(fail.Key, 
                        (fail.Value is OversizedPayloadException) ? "Resource content exceeds the maximum limit." 
                                                                  : "Unexpected error occurred while fetching the endpoint as OData resource." 
                    )
                );
            }

            // For crawling jobs, we remove the metadata validation as it has a new tab now.
            /*if (this.metaCtx != null)
            {
                if (this.metaCtx.PayloadType == PayloadType.Metadata) 
                { 
                    jobs.Add(this.metaCtx); 
                } 
                else 
                {
                    failedTargets.Add(new KeyValuePair<string, string>(this.metaCtx.Destination.OriginalString, "Either the resource is not found or content exceeds the maximum limit.")); 
                }
            }*/

            if (feedJob != null)
            {
                if (feedJob.PayloadType == PayloadType.Feed)
                {
                    jobs.Add(feedJob);
                }
                else
                {
                    failedTargets.Add(new KeyValuePair<string, string>(feedJob.Destination.OriginalString, "Resource is not an OData feed as expected."));
                }
            }

            if (errorJob != null)
            {
                if (errorJob.PayloadType == PayloadType.Error)
                {
                    jobs.Add(errorJob);
                }
                else
                {
                    failedTargets.Add(new KeyValuePair<string, string>(errorJob.Destination.OriginalString, "Resource is not an OData error payload as expected."));
                }
            }

            if (entryJob != null)
            {
                if (entryJob.PayloadType == PayloadType.Entry)
                {
                    jobs.Add(entryJob);
                }
                else
                {
                    failedTargets.Add(new KeyValuePair<string, string>(entryJob.Destination.OriginalString, "Resource is not an OData entry as expected."));
                }
            }

            return jobs;
        }

        private ServiceContext SetupCrawlSubJob(Func<ServiceContext> f, string target, ref List<KeyValuePair<string, Exception>> failedTargets)
        {
            ServiceContext context = null;

            try
            {
                context = f();
            }
            catch (Exception ex)
            {
                if (ExceptionHelper.IsCatchableExceptionType(ex))
                {
                    if (failedTargets == null)
                    {
                        failedTargets = new List<KeyValuePair<string, Exception>>();
                    }

                    failedTargets.Add(new KeyValuePair<string, Exception>(target, ex));
                }
                else
                {
                    throw;
                }
            }

            return context;
        }

        private ServiceContext SetupCrawlSubJob(Uri targetUri, ref List<KeyValuePair<string, Exception>> failedTargets)
        {
            Func<ServiceContext> f = () => ServiceContextFactory.Create(targetUri,
                    this.acceptHeaderValue,
                    this.rootCtx.ServiceBaseUri,
                    this.rootCtx.ServiceDocument,
                    this.rootCtx.MetadataDocument,
                    Guid.NewGuid(),
                    this.maxPayloadSize,
                    this.rootCtx.RequestHeaders,
                    this.category);
            return SetupCrawlSubJob(f, targetUri.AbsoluteUri ,ref failedTargets);
        }

        private ServiceContext CreateMetadataJob(ref List<KeyValuePair<string, Exception>> failedTargets)
        {
            Func<ServiceContext> f = () => ServiceContextFactory.CreateMetadataContext(new Uri(this.metaResource),
                    this.rootCtx.ServiceBaseUri,
                    this.rootCtx.ServiceDocument,
                    this.rootCtx.MetadataDocument,
                    Guid.NewGuid(),
                    this.rootCtx.RequestHeaders,
                    this.rootCtx.ResponseHttpHeaders,
                    this.category);

            return this.SetupCrawlSubJob(f, this.metaResource, ref failedTargets);
        }

        private ServiceContext CreateErrorPayloadJob(ref List<KeyValuePair<string, Exception>> failedTargets)
        {
            string errorFeed = "foo";
            while (this.feeds.Contains(errorFeed))
            {
                errorFeed += "o";
            }

            Uri target = new Uri(this.rootCtx.DestinationBasePath + "/" + errorFeed);
            return this.SetupCrawlSubJob(target, ref failedTargets);
        }

        private ServiceContext CreateFeedJob(ref List<KeyValuePair<string, Exception>> failedTargets)
        {
            if (this.feeds.Any())
            {
                string feedTarget = this.rootCtx.DestinationBasePath + "/" + this.feeds.First() + "?$top=1";
                Uri target = new Uri(feedTarget);
                return this.SetupCrawlSubJob(target, ref failedTargets);
            }
            else
            {
                return null;
            }
        }

        private ServiceContext CreateEntryJob(ServiceContext feedJob, PayloadFormat feedFormat,ref List<KeyValuePair<string, Exception>> failedTargets)
        {
            ServiceContext context = null;
            string entry = string.Empty;

            if (!string.IsNullOrEmpty(feedJob.ResponsePayload))
            {              
                var entries = ContextHelper.GetEntries(feedJob.ResponsePayload, feedFormat).ToArray();
                
                if (entries.Any())
                {
                    entry = entries.First();                   
                }
                else
                {
                    string acceptHeader = Constants.V3AcceptHeaderJsonFullMetadata;

                    if (feedJob.Version == ODataVersion.V4)
                    {
                        acceptHeader = Constants.V4AcceptHeaderJsonFullMetadata;
                    }

                    var response = WebHelper.Get(feedJob.Destination, acceptHeader, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, feedJob.RequestHeaders);

                    if (response != null && !string.IsNullOrEmpty(response.ResponsePayload))
                    {
                        entries = ContextHelper.GetEntries(response.ResponsePayload, feedFormat).ToArray();

                        if (entries.Any())
                        {
                            entry = entries.First();
                        }
                    }
                }

                Uri target = new Uri(entry);
                context = this.SetupCrawlSubJob(target, ref failedTargets);
            }

            return context;
        }
    }
}
