// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespaces
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    #endregion

    /// <summary>
    /// Helper class to get server response for given uri string and request Accept header value
    /// </summary>
    public static class WebHelper
    {
        /// <summary>
        /// Gets response content from uri string and accept header value
        /// </summary>
        /// <param name="uri">uri pointing to the destination resource</param>
        /// <param name="acceptHeader">value of Accept header in request</param>
        /// <param name="maximumPayloadSize">maximum size of payload in byte</param>
        /// <param name="reqHeaders">collection of Http header to be sent out</param>
        /// <returns>Reponse object which contains payload, response headers and status code</returns>
        /// <exception cref="ArgumentException">Throws exception when parameter is out of scope</exception>
        /// <exception cref="OversizedPayloadException">Throws exception when payload content exceeds the set maximum size</exception>
        public static Response Get(Uri uri, string acceptHeader, int maximumPayloadSize, IEnumerable<KeyValuePair<string, string>> reqHeaders)
        {
            var req = WebRequest.Create(uri);

            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                req.Credentials = new NetworkCredential(Uri.UnescapeDataString(uri.UserInfo.Split(':')[0]), Uri.UnescapeDataString(uri.UserInfo.Split(':')[1]));
            }

            var reqHttp = req as HttpWebRequest;

            if (!string.IsNullOrEmpty(acceptHeader) && reqHttp != null)
            {
                reqHttp.Accept = acceptHeader;
            }

            if (reqHeaders != null && reqHeaders.Any())
            {
                foreach (var p in reqHeaders)
                {
                    if (!string.IsNullOrEmpty(p.Key))
                    {
                        reqHttp.Headers[p.Key] = p.Value;
                    }
                }
            }

            return WebHelper.Get(reqHttp, maximumPayloadSize);
        }

        /// <summary>
        /// Returns Reponse object from a WebRequest object
        /// </summary>
        /// <param name="request">WebRequest object for which the response is returned</param>
        /// <param name="maximumPayloadSize">maximum size of payload in byte</param>
        /// <returns>Reponse object which contains payload, response headers and status code</returns>
        /// <exception cref="ArgumentException">Throws exception when parameter is out of scope</exception>
        /// <exception cref="OversizedPayloadException">Throws exception when payload content exceeds the set maximum size</exception>
        [SuppressMessage("DataWeb.Usage", "AC0013: call WebUtil.GetResponseStream instead of calling the method directly on the HTTP object.", Justification = "interop prefers to interact directly with network")]
        public static Response Get(WebRequest request, int maximumPayloadSize)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            try
            {
                request.Timeout = int.Parse(Constants.WebRequestTimeOut);
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; });

                if (DataService.serviceInstance != null)
                {
                    string responseHeaders = string.Empty;
                    string responsePayload = string.Empty;
                    HttpStatusCode? statusCode = null;

                    DataService service = new DataService();
                    service.HandleRequest(request, out statusCode, out responseHeaders, out responsePayload);

                    return new Response(statusCode, responseHeaders, responsePayload);
                }
                else
                {
                    using (WebResponse resp = request.GetResponse())
                    {
                        string responseHeaders, responsePayload;
                        HttpStatusCode? statusCode = WebHelper.ParseResponse(maximumPayloadSize, resp, out responseHeaders, out responsePayload);
                        return new Response(statusCode, responseHeaders, responsePayload);
                    }
                }
            }
            catch (WebException wex)
            {
                try
                {
                    if (wex.Response != null)
                    {
                        string responseHeaders, responsePayload;
                        HttpStatusCode? statusCode = WebHelper.ParseResponse(maximumPayloadSize, wex.Response, out responseHeaders, out responsePayload);
                        return new Response(statusCode, responseHeaders, responsePayload);
                    }
                }
                catch (OversizedPayloadException)
                {
                    return new Response(null, null, null);
                }
            }

            return new Response(null, null, null);
        }

        /// <summary>
        /// Extracts status code, response headers and payload (as string) from a WebResponse object
        /// </summary>
        /// <param name="maximumPayloadSize">maximum size of payload in bytes</param>
        /// <param name="response">WebResponse object containing all the information about the response</param>
        /// <param name="responseHeaders">respone header block in WebResponse object</param>
        /// <param name="responsePayload">response payload in WebResponse object</param>
        /// <returns>http status code if http/https protocol is invloved; otherwise, null</returns>
        /// <exception cref="ArgumentException">Throws exception when parameter is out of scope</exception>
        /// <exception cref="OversizedPayloadException">Throws exception when payload content exceeds the set maximum size</exception>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "response header and payload are returned values too")]
        [SuppressMessage("DataWeb.Usage", "AC0013: call WebUtil.GetResponseStream instead of calling the method directly on the HTTP object.", Justification = "interop prefers to interact directly woth network")]
        public static HttpStatusCode? ParseResponse(int maximumPayloadSize, WebResponse response, out string responseHeaders, out string responsePayload)
        {
            HttpStatusCode? statusCode = null;
            responseHeaders = null;
            responsePayload = null;

            if (response != null)
            {
                if (maximumPayloadSize <= 0)
                {
                    throw new ArgumentException(Resource.ArgumentNegativeOrZero, "maximumPayloadSize");
                }

                if (response.ContentLength > maximumPayloadSize)
                {
                    throw new OversizedPayloadException(string.Format(CultureInfo.CurrentCulture, Resource.formatPayloadSizeIsTooBig, response.ContentLength));
                }

                var headers = response.Headers;
                responseHeaders = headers != null ? headers.ToString() : null;

                string charset = null;
                var httpWebResponse = response as HttpWebResponse;
                if (httpWebResponse != null)
                {
                    charset = httpWebResponse.CharacterSet;
                    statusCode = httpWebResponse.StatusCode;
                }
                else
                {
                    charset = responseHeaders.GetCharset();
                }

                string contentType = responseHeaders.GetContentTypeValue();

                using (var stream = response.GetResponseStream())
                {
                    responsePayload = WebHelper.GetPayloadString(stream, maximumPayloadSize, charset, contentType);
                }
            }

            return statusCode;
        }

        /// <summary>
        /// Canonicalizes a uri
        /// </summary>
        /// <param name="uri">uri to be canonicalized</param>
        /// <returns>the canonicalized uri</returns>
        public static Uri Canonicalize(this Uri uri)
        {
            Uri canonical = null;

            if (uri != null)
            {
                try
                {
                    string safeUnescaped = uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
                    string normalized = Regex.Replace(safeUnescaped, "(?<!:)/+", "/");
                    canonical = new Uri(normalized);
                }
                catch (UriFormatException)
                {
                    // does nothing
                }
            }

            return canonical;
        }

        /// <summary>
        /// Get response content using the http POST method.
        /// </summary>
        /// <param name="request">The odata service request.</param>
        /// <returns>Response object which contains payload, response headers and status code.</returns>
        public static Response Post(Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var req = WebRequest.Create(new Uri(request.Url));
            var httpReq = req as HttpWebRequest;
            httpReq.Method = request.HttpMethod;
            httpReq.ContentType = request.ContentType;
            byte[] reqData = Encoding.UTF8.GetBytes(request.RequestPayload);
            httpReq.ContentLength = reqData.Length;

            if (request.RequestHeaders != null)
            {
                foreach (var header in request.RequestHeaders)
                {
                    httpReq.Headers.Add(header.Key, header.Value);
                }
            }

            if (request.RequestPayload != string.Empty)
            {
                Stream dataStream = httpReq.GetRequestStream();
                dataStream.Write(reqData, 0, reqData.Length);
                dataStream.Close();
            }

            return Get(httpReq, request.MaxPayloadSize);
        }

        #region Based Read/Write Operations (Create, Delete, Search, Update and Batch Ops).
        /// <summary>
        /// Create an entity and inserts it in an entity-set.
        /// </summary>
        /// <param name="url">The URL of an entity-set.</param>
        /// <param name="entity">An entity data.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <returns>Return the response of creating operation.</returns>
        public static Response CreateEntity(string url, string entity, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) || !entity.IsJsonPayload())
            {
                return null;
            }

            Request req = new Request();
            req.Url = url;
            req.HttpMethod = HttpMethod.Post;
            req.ContentType = Constants.ContentTypeJson;
            req.MaxPayloadSize = RuleEngineSetting.Instance().DefaultMaximumPayloadSize;
            req.RequestHeaders = requestHeaders;
            req.RequestPayload = entity;
            Response resp = Post(req);

            return resp;
        }

        /// <summary>
        /// Delete an entity from an entity-set.
        /// </summary>
        /// <param name="url">The URL of an deleted entity.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <returns>Returns the response of deleting operation.</returns>
        public static Response DeleteEntity(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            Request req = new Request();
            req.Url = url;
            req.HttpMethod = HttpMethod.Delete;
            req.ContentType = Constants.ContentTypeJson;
            req.MaxPayloadSize = RuleEngineSetting.Instance().DefaultMaximumPayloadSize;
            req.RequestHeaders = requestHeaders;
            req.RequestPayload = string.Empty;
            Response resp = Post(req);

            return resp;
        }

        /// <summary>
        /// Get an entity from an entity-set.
        /// </summary>
        /// <param name="url">The URL of an entity in an entity-set.</param>
        /// <param name="entityPayload">An entity which was got from entity-set.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <returns>Return the response of getting operation.</returns>
        public static Response GetEntity(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            var resp = Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, requestHeaders);

            return resp;
        }

        /// <summary>
        /// Get the value of the property.
        /// </summary>
        /// <param name="url">The URL of an property value in an entity.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <returns>Return the response of getting operation.</returns>
        public static Response GetPropertyValue(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            var resp = Get(new Uri(url), null, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, requestHeaders);

            return resp;
        }

        /// <summary>
        /// Send the delta get request with the odata.track-changes prefer header.
        /// </summary>
        /// <param name="url">The URL to the resources whose change is asked to track.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <returns>Return the response of getting operation.</returns>
        public static Response GetDeltaLink(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            Uri uri = new Uri(url);

            var headers = new WebHeaderCollection();
            foreach (var header in requestHeaders)
            {
                headers.Add(header.Key, header.Value);
            }

            headers.Add("Prefer", "odata.track-changes");

            var req = WebRequest.Create(uri);

            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                req.Credentials = new NetworkCredential(Uri.UnescapeDataString(uri.UserInfo.Split(':')[0]), Uri.UnescapeDataString(uri.UserInfo.Split(':')[1]));
            }

            var reqHttp = req as HttpWebRequest;

            reqHttp.Headers = headers;
            reqHttp.Accept =  Constants.V4AcceptHeaderJsonFullMetadata;

            return WebHelper.Get(reqHttp, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
        }

        /// <summary>
        /// Update an entity to an entity-set.
        /// </summary>
        /// <param name="url">The URL of an entity-set.</param>
        /// <param name="entity">An entity data.</param>
        /// <param name="httpMethod">The http method.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <returns>Return the response of updating operation.</returns>
        public static Response UpdateEntity(string url, string entity, string httpMethod, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) ||
                !entity.IsJsonPayload() ||
                !(httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Patch))
            {
                return null;
            }

            Request req = new Request();
            req.Url = url;
            req.HttpMethod = httpMethod;
            req.ContentType = Constants.ContentTypeJson;
            req.MaxPayloadSize = RuleEngineSetting.Instance().DefaultMaximumPayloadSize;
            req.RequestHeaders = requestHeaders;
            req.RequestPayload = entity;
            Response resp = Post(req);

            return resp;
        }

        /// <summary>
        /// Upsert an entity to an entity-set.
        /// </summary>
        /// <param name="url">The URL of an entity-set.</param>
        /// <param name="entity">An entity data.</param>
        /// <param name="httpMethod">The http method.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <returns>Return the response of upserting operation.</returns>
        public static Response UpsertEntity(string url, string entity, string httpMethod, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) ||
                !entity.IsJsonPayload() ||
                !(httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Patch))
            {
                return null;
            }

            Request req = new Request();
            req.Url = url;
            req.HttpMethod = httpMethod;
            req.ContentType = Constants.ContentTypeJson;
            req.MaxPayloadSize = RuleEngineSetting.Instance().DefaultMaximumPayloadSize;
            req.RequestHeaders = requestHeaders;
            req.RequestPayload = entity;
            Response resp = Post(req);

            return resp;
        }

        /// <summary>
        /// Send a batch request to server.
        /// </summary>
        /// <param name="serviceUrl">A service document url.</param>
        /// <param name="boundary">A boundary flag of batch operations.</param>
        /// <param name="requestData">A request data stores all the batch operations.</param>
        /// <param name="requestHeaders">The request headers of this batch request.</param>
        /// <param name="host">The host of this batch request.</param>
        /// <returns>Returns the response of batch operation.</returns>
        public static Response BatchOperation(string serviceUrl, string requestData, string boundary, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
        {
            if (!Uri.IsWellFormedUriString(serviceUrl, UriKind.Absolute) ||
                string.IsNullOrEmpty(requestData) ||
                string.IsNullOrEmpty(boundary))
            {
                return null;
            }

            Request req = new Request();
            req.HttpMethod = HttpMethod.Post;
            req.MaxPayloadSize = RuleEngineSetting.Instance().DefaultMaximumPayloadSize;
            req.RequestPayload = requestData;
            req.Url = serviceUrl.TrimEnd('/') + "/$batch";
            req.ContentType = string.Format("multipart/mixed;boundary={0}", boundary);
            req.RequestHeaders = requestHeaders;

            return WebHelper.Post(req);
        }

        /// <summary>
        /// Gets the content using specified URL for more times utill the following has been happen.
        /// 1. The response status code is 200 (OK);
        /// 2. Timeout.
        /// </summary>
        /// <param name="url">The specified URL.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <param name="interval">The time interval between the 2 request. (The default value is 0.1s.)</param>
        /// <param name="times">The times of the request will be executed. (The default value is 5.)</param>
        /// <param name="response">Output the response.</param>
        /// <returns>true: Get the specified content; false: otherwise.</returns>
        public static bool GetContent(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, int interval, int times, out Response response)
        {
            int loop = 0;
            do
            {
                response = GetEntity(url, requestHeaders);
                if (times <= loop++)
                {
                    break;
                }

                Thread.Sleep(interval);
            }
            while (response.StatusCode != HttpStatusCode.OK);

            return null != response;
        }
        #endregion

        #region Facade methods.
        /// <summary>
        /// Gets the content using specified URL for more times utill the following has been happen.
        /// 1. The response status code is 200 (OK);
        /// 2. Timeout.
        /// </summary>
        /// <param name="url">The specified URL.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <param name="response">Output the response.</param>
        /// <returns>true: Get the specified content; false: otherwise.</returns>
        public static bool GetContent(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, out Response response)
        {
            return WebHelper.GetContent(url, requestHeaders, WebHelper.RequestInterval, WebHelper.RequestTimes, out response);
        }

        /// <summary>
        /// Create an entity with any type and insert it to an entity-set on the service.
        /// </summary>
        /// <param name="url">The URL of the entity-set on the service.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <param name="entity">The non-media type entity template.</param>
        /// <param name="isMediaType">Indicate whether the inserted entity is media type or not.</param>
        /// <param name="additionInfos">The addition information. 
        /// (This output parameter must be qualified with an 'ref' key word.)</param>
        /// <returns>Returns the response.</returns>
        public static Response CreateEntity(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, JObject entity, bool isMediaType, ref List<AdditionalInfo> additionalInfos)
        {
            Response resp = null;
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) || null == entity)
            {
                return resp;
            }

            if (isMediaType)
            {
                AdditionalInfo additionalInfo = null;
                resp = CreateMediaTypeEntity(url, requestHeaders, out additionalInfo);
                additionalInfos.RemoveAt(additionalInfos.Count - 1);
                additionalInfos.Add(additionalInfo);
            }
            else
            {
                resp = CreateEntity(url, entity.ToString(), requestHeaders);
            }

            return resp;
        }

        /// <summary>
        /// Delete an entity with an odata.etag annotation from an entity-set.
        /// </summary>
        /// <param name="url">The URL of an deleted entity.</param>
        /// <param name="hasEtag">The flag indicates whether the entity has an odata.etag annotation or not.</param>
        /// <returns>Returns the response of deleting operation.</returns>
        public static Response DeleteEntity(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, bool hasEtag)
        {
            var headers = requestHeaders.ToList();
            if (hasEtag)
            {
                headers.Add(new KeyValuePair<string, string>("If-Match", "*"));
            }

            return WebHelper.DeleteEntity(url, headers);
        }

        /// <summary>
        /// Delete all the entities created by customer.
        /// </summary>
        /// <param name="additionalInfos">The additional information.</param>
        /// <returns>Returns a response list.</returns>
        public static List<Response> DeleteEntities(IEnumerable<KeyValuePair<string, string>> requestHeaders, List<AdditionalInfo> additionalInfos)
        {
            var resp = new List<Response>();
            foreach (var info in additionalInfos)
            {
                resp.Add(WebHelper.DeleteEntity(info.EntityId, requestHeaders, info.HasEtag));
            }

            return resp;
        }

        /// <summary>
        /// Update an entity with an odata.etag annotation from an entity-set.
        /// </summary>
        /// <param name="url">The URL of an entity-set.</param>
        /// <param name="entity">An entity data.</param>
        /// <param name="httpMethod">The http method.</param>
        /// <param name="hasEtag">The flag indicates whether the entity has an odata.etag annotation or not.</param>
        /// <returns>Return the response of updating operation.</returns>
        public static Response UpdateEntity(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, string entity, string httpMethod, bool hasEtag)
        {
            var headers = requestHeaders.ToList();
            if (hasEtag)
            {
                headers.Add(new KeyValuePair<string, string>("If-Match", "*"));
            }

            return WebHelper.UpdateEntity(url, entity, httpMethod, headers);
        }

        /// <summary>
        /// Update an entity a string property.
        /// </summary>
        /// <param name="url">The URL the property.</param>
        /// <param name="requestHeaders">The headers of the request.</param>
        /// <param name="hasEtag">The flag indicates whether the entity has an odata.etag annotation or not.</param>
        /// <returns>Return the response of updating operation.</returns>
        public static Response UpdateAStringProperty(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, bool hasEtag)
        {
            var headers = requestHeaders.ToList();
            if (hasEtag)
            {
                headers.Add(new KeyValuePair<string, string>("If-Match", "*"));
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            Request req = new Request();
            req.Url = url;
            req.HttpMethod = HttpMethod.Put;
            req.ContentType = Constants.ContentTypeTextPlain;
            req.MaxPayloadSize = RuleEngineSetting.Instance().DefaultMaximumPayloadSize;
            req.RequestHeaders = headers;
            req.RequestPayload = Constants.UpdateData;
            Response resp = Post(req);

            return resp;
        }

        /// <summary>
        /// Update an entity complex property.
        /// </summary>
        /// <param name="url">The URL the property.</param>
        /// <param name="requestHeaders">The headers of the request.</param>
        /// <param name="hasEtag">The flag indicates whether the entity has an odata.etag annotation or not.</param>
        /// <param name="propsToUpdate">The properties names of the complex type of the complex property.</param>
        /// <returns>Return the response of updating operation.</returns>
        public static Response UpdateComplexProperty(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, JProperty propContent, bool hasEtag, List<string> propsToUpdate)
        {
            var headers = requestHeaders.ToList();
            if (hasEtag)
            {
                headers.Add(new KeyValuePair<string, string>("If-Match", "*"));
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            Request req = new Request();
            req.Url = url;
            req.HttpMethod = HttpMethod.Patch;
            req.ContentType = Constants.ContentTypeJson;
            req.MaxPayloadSize = RuleEngineSetting.Instance().DefaultMaximumPayloadSize;
            req.RequestHeaders = headers;
            JObject updateContent = new JObject();
            foreach (var prop in propContent.Value.Children<JProperty>())
            {
                if (propsToUpdate.Contains(prop.Name))
                {
                    updateContent.Add(prop.Name, Constants.UpdateData);
                }
            }
            req.RequestPayload = updateContent.ToString();
            Response resp = Post(req);

            return resp;
        }

        /// <summary>
        /// Update an entity collection property.
        /// </summary>
        /// <param name="url">The URL the property.</param>
        /// <param name="requestHeaders">The headers of the request.</param>
        /// <param name="propContent">The constructed property content of the request.</param>
        /// <param name="hasEtag">The flag indicates whether the entity has an odata.etag annotation or not.</param>
        /// <returns>Return the response of updating operation.</returns>
        public static Response UpdateCollectionProperty(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, JProperty propContent, bool hasEtag)
        {
            var headers = requestHeaders.ToList();
            if (hasEtag)
            {
                headers.Add(new KeyValuePair<string, string>("If-Match", "*"));
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            Request req = new Request();
            req.Url = url;
            req.HttpMethod = HttpMethod.Put;
            req.ContentType = Constants.ContentTypeJson;
            req.MaxPayloadSize = RuleEngineSetting.Instance().DefaultMaximumPayloadSize;
            req.RequestHeaders = headers;
            int count = propContent.Value.Count();
            JObject updateContent = new JObject();
            
            for (int i = 0; i < count; i++ )
            {
                propContent.Value[i] = Constants.UpdateData;
            }
            
            updateContent.Add("value", propContent.Value);

            req.RequestPayload = updateContent.ToString();
            Response resp = Post(req);

            return resp;
        }


        /// <summary>
        /// Update the media-type entity.
        /// </summary>
        /// <param name="url">The URL of an entity-set with media-type.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <param name="hasEtag">true, if the entity has etag; false, otherwise.</param>
        /// <returns>Returns the response.</returns>
        public static Response UpdateMediaTypeEntity(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, bool hasEtag)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            var headers = requestHeaders.ToList();
            if (hasEtag)
            {
                headers.Add(new KeyValuePair<string, string>("If-Match", "*"));
            }

            string payload = string.Empty;

            var imageResp = UpdateImage(url, headers, WebHelper.ImageUpdatePath);

            return new Response(imageResp.StatusCode, imageResp.Headers.ToString(), payload);
        }

        #endregion

        #region Private members.
        /// <summary>
        /// Indicates the request interval.
        /// </summary>
        private const int RequestInterval = 100;

        /// <summary>
        /// Indicates the request times.
        /// </summary>
        private const int RequestTimes = 5;

        /// <summary>
        /// Indicates the testing image path.
        /// </summary>
        private readonly static string ImagePath = AppDomain.CurrentDomain.BaseDirectory + "Images\\ODataValidatorToolLogo.jpg";
        private readonly static string ImageUpdatePath = AppDomain.CurrentDomain.BaseDirectory + "Images\\EditImageStream.jpg";
        #endregion

        #region Private methods.
        /// <summary>
        /// Read the payload with the specified charset or embedded encoding in xml block if applicable
        /// </summary>
        /// <param name="stream">payload stream</param>
        /// <param name="maximumPayloadSize">maximum payload size in bytes</param>
        /// <param name="charset">charset value in content-type header</param>
        /// <param name="contentType">content-type value</param>
        /// <returns>payload text read using proper encoding</returns>
        private static string GetPayloadString(Stream stream, int maximumPayloadSize, string charset, string contentType)
        {
            byte[] buffer = WebHelper.GetPayloadBytes(stream, maximumPayloadSize);
            if (buffer == null || buffer.Length == 0)
            {
                return null;
            }

            Encoding encoding = Encoding.Default;
            if (!string.IsNullOrEmpty(charset))
            {
                try
                {
                    encoding = Encoding.GetEncoding(charset);
                }
                catch (ArgumentException)
                {
                    charset = null;
                }
            }

            if (buffer.Length > 3 && buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
            {
                buffer[0] = 0x20;
                buffer[1] = 0x20;
                buffer[2] = 0x20;
            }

            string responsePayload = encoding.GetString(buffer).TrimStart();

            if (string.IsNullOrEmpty(charset))
            {
                string payloadInEmbbedEncoding;
                if (TryReadWithEmbeddedEncoding(buffer, contentType, responsePayload, out payloadInEmbbedEncoding))
                {
                    return payloadInEmbbedEncoding;
                }
            }

            return responsePayload;
        }

        /// <summary>
        /// Read stream into a byte array while limiting the size to the specified maximum 
        /// </summary>
        /// <param name="stream">stream obhect to read from</param>
        /// <param name="maximumPayloadSize">the maximum number of bytes to read</param>
        /// <returns>byte array</returns>
        [SuppressMessage("Microsoft.MSInternal", "CA908:generic method that does not require JIT compilation at runtime", Justification = "no other way to strip byte[] yet")]
        private static byte[] GetPayloadBytes(Stream stream, int maximumPayloadSize)
        {
            if (stream == null)
            {
                return null;
            }

            byte[] buffer = new byte[maximumPayloadSize];
            int roomLeft = maximumPayloadSize;
            int offset = 0;

            int count;
            while ((count = stream.Read(buffer, offset, roomLeft)) > 0)
            {
                offset += count;
                roomLeft -= count;
            }

            if (roomLeft == 0 && stream.ReadByte() != -1)
            {
                throw new OversizedPayloadException(Resource.PayloadSizeIsTooBig);
            }

            Array.Resize<byte>(ref buffer, offset);
            return buffer;
        }

        /// <summary>
        /// Try to read byte arrary into a string using the possibly-existent encoding seeting within
        /// </summary>
        /// <param name="buffer">byte arrary to read from</param>
        /// <param name="contentType">content type of the byte array</param>
        /// <param name="contentHint">string of byte array in the default encoding</param>
        /// <param name="content">output string of byte array in the embedded encoding</param>
        /// <returns>true if content is read using a found valid encoding; otherwise false</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308: replace the call to 'string.ToLowerInvariant()' with String.ToUpperInvariant()'.", Justification = "the comparation targets are in lower case")]
        private static bool TryReadWithEmbeddedEncoding(byte[] buffer, string contentType, string contentHint, out string content)
        {
            if (!string.IsNullOrEmpty(contentType))
            {
                contentType = contentType.ToLowerInvariant();
                switch (contentType)
                {
                    case Constants.ContentTypeXml:
                    case Constants.ContentTypeAtom:
                    case Constants.ContentTypeTextXml:
                    case Constants.ContentTypeJson:
                        return TryReadXmlUsingEmbeddedEncoding(buffer, contentHint, out content);
                }
            }

            content = null;
            return false;
        }

        /// <summary>
        /// Try to read XML content using the embedded encoding
        /// </summary>
        /// <param name="buffer">byte array of the XML literal</param>
        /// <param name="xmlHint">string of the xml literal in the default encoding</param>
        /// <param name="xml">output string of the xml literal in the embedded encoding</param>
        /// <returns>true if the output is read using a valid embedded encoding; otherwise false</returns>
        private static bool TryReadXmlUsingEmbeddedEncoding(byte[] buffer, string xmlHint, out string xml)
        {
            string charsetEmbedded = null;
            charsetEmbedded = xmlHint.GetEmbeddedEncoding();

            if (!string.IsNullOrEmpty(charsetEmbedded))
            {
                try
                {
                    Encoding encoding = Encoding.GetEncoding(charsetEmbedded);
                    xml = encoding.GetString(buffer);
                    return true;
                }
                catch (ArgumentException)
                {
                    // does nothing
                }
            }

            xml = null;
            return false;
        }

        /// <summary>
        /// Create an image on an entity-set with media-type.
        /// </summary>
        /// <param name="url">The URL of an entity-set with media-type.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <param name="imagePath">The local path of an image which will be inserted to server.</param>
        /// <returns>Returns the HTTP response.</returns>
        private static HttpWebResponse CreateImage(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, string imagePath)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) ||
                string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            var headers = new WebHeaderCollection();
            foreach (var header in requestHeaders)
            {
                headers.Add(header.Key, header.Value);
            }

            try
            {
                byte[] image = File.ReadAllBytes(imagePath);
                if (null != image && 0 != image.Length)
                {
                    var req = WebRequest.Create(url) as HttpWebRequest;
                    req.Method = HttpMethod.Post;
                    req.Headers = headers;
                    req.ContentType = Constants.ContentTypeJPEGImage;
                    req.ContentLength = image.Length;

                    using (Stream dataStream = req.GetRequestStream())
                    {
                        dataStream.Write(image, 0, image.Length);
                    }

                    return req.GetResponse() as HttpWebResponse;
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new DirectoryNotFoundException(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException(ex.Message);
            }
            catch (WebException ex)
            {
                return ex.Response as HttpWebResponse;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Change an image of the media entity to another image by HTTP put method.
        /// </summary>
        /// <param name="url">The URL of an entity-set with media-type.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <param name="imagePath">The local path of an image which will be inserted to server.</param>
        /// <returns>Returns the HTTP response.</returns>
        private static HttpWebResponse UpdateImage(string url, List<KeyValuePair<string, string>> requestHeaders, string imagePath)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) ||
                string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            var headers = new WebHeaderCollection();
            foreach (var header in requestHeaders)
            {
                headers.Add(header.Key, header.Value);
            }

            try
            {
                byte[] image = File.ReadAllBytes(imagePath);
                if (null != image && 0 != image.Length)
                {
                    var req = WebRequest.Create(url) as HttpWebRequest;
                    req.Method = HttpMethod.Put;
                    req.Headers = headers;
                    req.ContentType = Constants.ContentTypeJPEGImage;
                    req.ContentLength = image.Length;

                    using (Stream dataStream = req.GetRequestStream())
                    {
                        dataStream.Write(image, 0, image.Length);
                    }

                    return req.GetResponse() as HttpWebResponse;
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new DirectoryNotFoundException(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException(ex.Message);
            }
            catch (WebException ex)
            {
                return ex.Response as HttpWebResponse;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Create the entity with the media-type.
        /// </summary>
        /// <param name="url">The URL of an entity-set with media-type.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <param name="additionalInfo">The additional information of the new inserted entity.</param>
        /// <returns>Returns the response.</returns>
        private static Response CreateMediaTypeEntity(string url, IEnumerable<KeyValuePair<string, string>> requestHeaders, out AdditionalInfo additionalInfo)
        {
            additionalInfo = null;
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            string payload = string.Empty;
            var imageResp = CreateImage(url, requestHeaders, WebHelper.ImagePath);
            if (null != imageResp && HttpStatusCode.Created == imageResp.StatusCode)
            {
                string entity = string.Empty;
                using (var sr = new StreamReader(imageResp.GetResponseStream()))
                {
                    string entityId = string.Empty;
                    string etag = string.Empty;
                    string mediaEtag = string.Empty;
                    string mediaReadLink = string.Empty;
                    string contextURL = string.Empty;
                    payload = sr.ReadToEnd();
                    var mediaEntity = JObject.Parse(payload);
                    var props = mediaEntity.Children<JProperty>();
                    foreach (var prop in props)
                    {
                        if (Constants.OdataV4JsonIdentity == prop.Name)
                        {
                            contextURL = prop.Value.ToString();
                        }
                        else if (Constants.V4OdataMediaReadLink == prop.Name)
                        {
                            mediaReadLink = prop.Value.ToString();
                        }
                        else if (Constants.V4OdataId == prop.Name)
                        {
                            entityId = prop.Value.ToString();
                        }
                        else if (Constants.V4OdataEtag == prop.Name)
                        {
                            etag = prop.Value.ToString();
                        }
                        else if (prop.Name.Contains(Constants.V4OdataMediaEtag))
                        {
                            mediaEtag = prop.Value.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(entityId))
                    {
                        if(!mediaReadLink.StartsWith("http"))
                        {
                            entityId = contextURL.Split('$')[0] + mediaReadLink;
                        }
                        else
                        {
                            entityId = mediaReadLink.Split('$')[0];
                        }
                    }

                    entityId = entityId.Split('$')[0];

                    additionalInfo = new AdditionalInfo(entityId, etag, mediaEtag);
                }
            }

            return new Response(imageResp.StatusCode, imageResp.Headers.ToString(), payload);
        }
        #endregion
    }
}
