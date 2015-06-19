// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Cache;
    using ODataValidator.RuleEngine.Common;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Helper class to get OData response from a OData request via Http protocol.
    /// </summary>
    internal static class WebResponseHelper
    {
        /// <summary>
        /// Gets the ETag header value of the specified entry resource
        /// </summary>
        /// <param name="entryTarget">The Uri pointing to the entry resource</param>
        /// <param name="toAccept">The Accept header value in the request header</param>
        /// <returns>The ETag header value in the HTTP response header</returns>
        public static string GetETagOfEntry(string entryTarget, string toAccept)
        {
            var reqHttp = WebRequest.Create(entryTarget) as HttpWebRequest;
            reqHttp.Accept = toAccept;
            var respHeader = reqHttp.GetResponse().Headers;
            return respHeader["ETag"];
        }

        /// <summary>
        /// Gets web response of Uri request with custom header DataServiceVersion
        /// </summary>
        /// <param name="uri">Uri of request</param>
        /// <param name="inJsonFormat">Flag of whether response in Json format is expected</param>
        /// <param name="headers">Headers sent in web request</param>
        /// <param name="maximumPayloadSize">Maximum size of response payload in byte to be received</param>
        /// <param name="ctx">The reference context whose request headers are carried over</param>
        /// <returns>Web response</returns>
        public static Response GetWithHeaders(Uri uri, bool inJsonFormat, IEnumerable<KeyValuePair<string, string>> headers, int maximumPayloadSize, ServiceContext context)
        {
            var req = WebRequest.Create(uri);
            var reqHttp = req as HttpWebRequest;

            if (context.RequestHeaders != null)
            {
                foreach (var head in context.RequestHeaders)
                {
                    reqHttp.Headers[head.Key] = head.Value;
                }
            }

            if (headers != null)
            {
                foreach (var head in headers)
                {
                    reqHttp.Headers[head.Key] = head.Value;
                }
            }

            string acceptHeaderValue = inJsonFormat ? Constants.AcceptHeaderJson : Constants.AcceptHeaderAtom;
            return WebResponseHelper.Get(reqHttp, acceptHeaderValue, maximumPayloadSize);
        }

        /// <summary>
        /// Gets web response of request specifying accept header header explicitly
        /// </summary>
        /// <param name="request">HttpWebRequest request</param>
        /// <param name="acceptHeaderValue">The specified accept header</param>
        /// <param name="maximumPayloadSize">Maximum size of response in byte to be allowed to receive</param>
        /// <returns>Web response</returns>
        public static Response Get(HttpWebRequest request, string acceptHeaderValue, int maximumPayloadSize)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!string.IsNullOrEmpty(acceptHeaderValue))
            {
                request.Accept = acceptHeaderValue;
            }

            // make sure cache disabled, otherwise intermediate proxy would likely return cached responses
            HttpRequestCachePolicy cachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = cachePolicy;
            return WebHelper.Get(request, maximumPayloadSize);
        }
    }
}
