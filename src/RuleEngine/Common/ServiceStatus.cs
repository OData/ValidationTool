// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Runtime.CompilerServices;
    #endregion

    /// <summary>
    /// The ServiceStatus class.
    /// </summary>
    public class ServiceStatus
    {
        /// <summary>
        /// Get the instance of class type ServiceStatus.
        /// </summary>
        /// <returns>The instance of class type TermDocument.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static ServiceStatus GetInstance(string url = null, string headers=null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (null == serviceStatus || 
                    url != serviceStatus.rootURL || 
                    string.IsNullOrEmpty(serviceStatus.serviceDoc) || 
                    string.IsNullOrEmpty(serviceStatus.metadataDoc))
                {
                    serviceStatus = new ServiceStatus(url, headers);
                }
            }

            return serviceStatus;
        }

        /// <summary>
        /// Revise the online metadata document to the testing metadata document.
        /// NOTE: This method MUST be only used in TestSuites.
        /// </summary>
        /// <param name="metadataDoc">The testing metadata document.</param>
        public static void ReviseMetadata(string metadataDoc)
        {
            if (metadataDoc.IsXmlPayload())
            {
                serviceStatus.metadataDoc = metadataDoc;
            }
        }

        /// <summary>
        /// Gets the service root URL.
        /// </summary>
        public string RootURL
        {
            get
            {
                return this.rootURL;
            }
        }

        /// <summary>
        /// Gets the service document.
        /// </summary>
        public string ServiceDocument
        {
            get
            {
                if (!this.serviceDoc.IsJsonPayload())
                {
                    throw new FormatException("The class member 'serviceDoc' does not store a JSON format data.");
                }

                return this.serviceDoc;
            }
        }

        /// <summary>
        /// Gets the metadata document.
        /// </summary>
        public string MetadataDocument
        {
            get
            {
                if (!this.metadataDoc.IsXmlPayload())
                {
                    throw new FormatException("The class member 'metadataDoc' does not store a XML format data.");
                }

                return this.metadataDoc;
            }
        }

        /// <summary>
        /// Gets or sets the request headers.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> DefaultHeaders
        {
            get
            {
                return this.defaultHeaders;
            }
            set 
            {
                this.defaultHeaders = value;
            }
        }

        /// <summary>
        /// The service status.
        /// </summary>
        private static ServiceStatus serviceStatus;

        /// <summary>
        /// The service root URL.
        /// </summary>
        private string rootURL;

        /// <summary>
        /// The service document.
        /// </summary>
        private string serviceDoc;

        /// <summary>
        /// The metadata document.
        /// </summary>
        private string metadataDoc;

        /// <summary>
        /// The default request headers.
        /// </summary>
        private IEnumerable<KeyValuePair<string, string>> defaultHeaders = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string> ("ODataVersion", "4.0" ) };

        /// <summary>
        /// The constructor of class type ServiceStatus.
        /// </summary>
        private ServiceStatus(string url, string headers)
        {
            var uri = this.ConvertToUri(url);

            if (!string.IsNullOrWhiteSpace(headers))
            {
                string[] headersSplited = headers.Split(';');
                List<KeyValuePair<string, string>> heaserCollection = new List<KeyValuePair<string, string>>();
                foreach (string header in headersSplited)
                {
                    string[] headerKeyValue = header.Split(':');
                    if (headerKeyValue.Length == 2)
                    {
                        heaserCollection.Add(new KeyValuePair<string, string>(headerKeyValue[0], headerKeyValue[1]));
                    }
                }
                defaultHeaders = heaserCollection;
            }

            // Get the service document.
            if (this.GetServiceDocument(uri, out this.rootURL, out this.serviceDoc))
            {
                // Get the metadata document.
                string metadataURL = string.Format("{0}/$metadata", this.rootURL.TrimEnd('/'));
                var req = (HttpWebRequest)WebRequest.Create(metadataURL);
                var response = ServiceStatus.Get(req);
                this.metadataDoc = HttpStatusCode.OK == response.StatusCode ? response.ResponsePayload : string.Empty;
            }
        }

        /// <summary>
        /// Get the service document.
        /// </summary>
        /// <param name="uri">The uri inputted by the user.</param>
        /// <param name="rootURL">The service root URL which is outputted by the program.</param>
        /// <param name="svcDoc">The service document which is outputted by the program.</param>
        /// <returns>Returns the boolean value to indicate whether the method has been got a service document or not.</returns>
        private bool GetServiceDocument(Uri uri, out string rootURL, out string svcDoc)
        {
            rootURL = string.Empty;
            svcDoc = string.Empty;
            if (null == uri || !uri.IsAbsoluteUri)
            {
                return false;
            }

            var req = (HttpWebRequest)WebRequest.Create(uri.ToString());

            foreach(var header in  defaultHeaders)
            {
                req.Headers.Add(header.Key, header.Value);
            }

            var resp = ServiceStatus.Get(req);
            if(null == resp)
            {
                return false;
            }

            if (HttpStatusCode.OK == resp.StatusCode)
            {
                if (resp.IsServiceDocument())
                {
                    this.rootURL = uri.ToString();
                    this.serviceDoc = resp.ResponsePayload;

                    return true;
                }
                else
                {
                    var segments = uri.Segments;
                    if (null != segments && segments.Length > 0)
                    {
                        Uri parentUri;
                        if (Uri.TryCreate(uri.GetLeftPart(UriPartial.Authority) + string.Join(string.Empty, segments.Take(segments.Length - 1).ToArray()), UriKind.Absolute, out parentUri))
                        {
                            if (GetServiceDocument(parentUri, out rootURL, out svcDoc))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else if (HttpStatusCode.Unauthorized == resp.StatusCode)
            {
                throw new UnauthorizedAccessException();
            }

            return false;
        }

        /// <summary>
        /// Convert a URL string to an URI.
        /// </summary>
        /// <param name="url">A URL string.</param>
        /// <returns>Returns an matched URI.</returns>
        private Uri ConvertToUri(string url)
        {
            Uri uri;

            try
            {
                uri = new Uri(url.TrimEnd('/'));
            }
            catch (UriFormatException)
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    uri = new Uri(SupportedScheme.SchemeHttp + "://" + url);
                }
                else
                {
                    uri = new Uri(Uri.EscapeUriString(url));
                }
            }

            return uri;
        }

        private static Response Get(HttpWebRequest request)
        {
            if (null == request)
            {
                throw new ArgumentNullException("request");
            }

            StreamReader streamReader = null;
            string responseHeaders, responsePayload;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; });
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                {
                    responseHeaders = resp.Headers.ToString();
                    streamReader = new StreamReader(resp.GetResponseStream());
                    responsePayload = streamReader.ReadToEnd();
                    streamReader.Close();

                    return new Response(resp.StatusCode, responseHeaders, responsePayload);
                }
            }
            catch (WebException e)
            {
                try
                {
                    if (null != e.Response)
                    {
                        HttpWebResponse response = (HttpWebResponse)e.Response;
                        responseHeaders = response.Headers.ToString();
                        streamReader = new StreamReader(response.GetResponseStream());
                        responsePayload = streamReader.ReadToEnd();

                        return new Response(response.StatusCode, responseHeaders, responsePayload);
                    }
                }
                catch (Exception)
                {
                    return new Response(null, null, null);
                }
            }

            return new Response(null, null, null);
        }
    }
}
