// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Helper class to instantiate service request context
    /// </summary>
    public static class ServiceContextFactory
    {
        /// <summary>
        /// Factory method to create the crawling metadata service context
        /// </summary>
        /// <param name="metaTarget">location pointing to the OData metadata document resource</param>
        /// <param name="serviceRoot">URI pointing to the OData service document resource</param>
        /// <param name="serviceDocument">Content of service document</param>
        /// <param name="metadataDocument">Content of metadata document</param>
        /// <param name="jobId">The validation job identifier</param>
        /// <param name="reqHeaders">Http headers sent as part of request</param>
        /// <returns>The constructed crawling metadata context</returns>
        /// <exception cref="CrawlRuntimeException">Throws exception when metadata document cannot be fetched</exception>
        /// <exception cref="ArgumentException">Throws exception when the metadata document content is empty or null</exception>
        public static ServiceContext CreateMetadataContext(Uri metaTarget, Uri serviceRoot, string serviceDocument, string metadataDocument, Guid jobId, IEnumerable<KeyValuePair<string, string>> reqHeaders, string respHeaders, string category = "core")
        {
            if (string.IsNullOrEmpty(metadataDocument))
            {
                throw new ArgumentException(Resource.ArgumentNotNullOrEmpty, "metadataDocument");
            }

            return new ServiceContext(metaTarget, jobId, HttpStatusCode.OK, respHeaders, metadataDocument, null, serviceRoot, serviceDocument, metadataDocument, false, reqHeaders, ODataMetadataType.MinOnly, null, category);
        }

        /// <summary>
        /// Factory method to create the light-weight online service context (without fetching service document and metadata document which are known already)
        /// </summary>
        /// <param name="target">The target resource to be crawled</param>
        /// <param name="acceptHeaderValue">The accept header value defined in the Http header</param>
        /// <param name="serviceRoot">The URI pointing to the service document resource</param>
        /// <param name="serviceDocument">The content of service document</param>
        /// <param name="metadataDocument">The content of metadata document</param>
        /// <param name="jobId">The validation job identifier</param>
        /// <param name="maximumPayloadSize">The maximum number of bytes allowed to retrieve from the target resource</param>
        /// <param name="reqHeaders">Http headers sent as part of request</param>
        /// <returns>The constructed service context</returns>
        /// <exception cref="CrawlRuntimeException">Throws exception when target resource exceeds the maximum size allowed</exception>
        public static ServiceContext Create(Uri target, 
            string acceptHeaderValue, 
            Uri serviceRoot, 
            string serviceDocument, 
            string metadataDocument, 
            Guid jobId, 
            int maximumPayloadSize, 
            IEnumerable<KeyValuePair<string, string>> reqHeaders,
            string category="core")
        {
            Response response = WebHelper.Get(target, acceptHeaderValue, maximumPayloadSize, reqHeaders);
            ODataMetadataType odataMetadata = ServiceContextFactory.MapAcceptHeaderToMetadataType(acceptHeaderValue);
            var payloadFormat = response.ResponsePayload.GetFormatFromPayload();
            var payloadType = response.ResponsePayload.GetTypeFromPayload(payloadFormat);
            string entityType = response.ResponsePayload.GetFullEntityType(payloadType, payloadFormat, metadataDocument);

            return new ServiceContext(target,
                jobId,
                response.StatusCode,
                response.ResponseHeaders,
                response.ResponsePayload,
                entityType,
                serviceRoot,
                serviceDocument,
                metadataDocument,
                false,
                reqHeaders,
                odataMetadata,
                null,
                category);
        }

        /// <summary>
        /// Factory method to set up the OData Interop context based on uri, desired format and job id
        /// </summary>
        /// <param name="destination">uri string pointing to a OData service endpoint</param>
        /// <param name="format">format preference, could be either "atom" or "json" - default and falling back to atom</param>
        /// <param name="jobId">unique identifier to tag this context</param>
        /// <param name="maximumPayloadSize">the maximum number of bytes rule engine is willing to retrieve from the Uri provided</param>
        /// <param name="reqHeaders">Http headers sent as part of request</param>
        /// <returns>context object representing the interop request session</returns>
        public static ServiceContext Create(string destination, string format, Guid jobId, int maximumPayloadSize, IEnumerable<KeyValuePair<string, string>> reqHeaders, string category="core")
        {
            Uri inputUri;
            Uri serviceBaseUri = null;
            string serviceDocument = null;
            string metadataDocument = null;
            string entityType = null;
            string jsonFullMetadataPayload = null;
            var serviceStatus = ServiceStatus.GetInstance();

            if (string.IsNullOrEmpty(format))
            {
                throw new ArgumentException(Resource.ArgumentNotNullOrEmpty, "format");
            }

            try
            {
                if (destination.EndsWith(@"/"))
                {
                    destination = destination.TrimEnd('/');
                }

                inputUri = new Uri(destination);
            }
            catch (UriFormatException)
            {
                if (!destination.StartsWith(@"http://") && !destination.StartsWith(@"https://"))
                {
                    inputUri = new Uri(SupportedScheme.SchemeHttp + "://" + destination);
                }
                else
                {
                    inputUri = new Uri(Uri.EscapeUriString(destination));
                }
            }

            if (!SupportedScheme.Instance.Contains(inputUri.Scheme))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resource.formatSchemeNotSupported, inputUri.Scheme),
                    "destination");
            }

            string acceptHeader = ServiceContextFactory.MapFormatToAcceptValue(format);
            ODataMetadataType odataMetadata = ServiceContextFactory.MapFormatToMetadataType(format);
            Response response = WebHelper.Get(inputUri, acceptHeader, maximumPayloadSize, reqHeaders);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var payloadFormat = response.ResponsePayload.GetFormatFromPayload();
            var payloadType = ContextHelper.GetPayloadType(response.ResponsePayload, payloadFormat, response.ResponseHeaders);

            switch (payloadType)
            {
                case PayloadType.ServiceDoc:
                    serviceBaseUri = inputUri;
                    serviceDocument = response.ResponsePayload;
                    break;
                case PayloadType.Metadata:
                    if (inputUri.AbsoluteUri.EndsWith(Constants.OptionMetadata, StringComparison.Ordinal))
                    {
                        if (payloadFormat == PayloadFormat.JsonLight)
                        {
                            serviceBaseUri = new Uri(serviceStatus.RootURL);
                            serviceDocument = serviceStatus.ServiceDocument;
                        }
                        else
                        {
                            serviceBaseUri = new Uri(inputUri.AbsoluteUri.Substring(0, inputUri.AbsoluteUri.Length - Constants.OptionMetadata.Length));
                            var respSvcDoc = WebHelper.Get(serviceBaseUri, acceptHeader, maximumPayloadSize, reqHeaders);
                            serviceDocument = respSvcDoc.ResponsePayload;
                        }
                    }

                    break;
                default:
                    if (payloadType.IsValidPayload())
                    {
                        string fullPath = inputUri.GetLeftPart(UriPartial.Path).TrimEnd('/');
                        var uriPath = new Uri(fullPath);
                        Uri baseUri;
                        Response serviceDoc;
                        if (ServiceContextFactory.TryGetServiceDocument(maximumPayloadSize, uriPath, acceptHeader, uriPath.AbsoluteUri == fullPath, out baseUri, out serviceDoc, reqHeaders))
                        {
                            serviceBaseUri = baseUri;
                            serviceDocument = serviceDoc.ResponsePayload;
                        }
                    }

                    break;
            }

            if (payloadType == PayloadType.Metadata)
            {
                metadataDocument = response.ResponsePayload;
            }
            else
            {
                if (serviceBaseUri != null)
                {
                    if (payloadFormat == PayloadFormat.JsonLight && payloadType == PayloadType.ServiceDoc)
                    {
                        metadataDocument = serviceStatus.MetadataDocument;
                    }
                    else
                    {
                        try
                        {
                            string metadataURL = serviceBaseUri.AbsoluteUri.EndsWith(@"/") ? Constants.OptionMetadata.TrimStart('/') : Constants.OptionMetadata;
                            var responseMetadata = WebHelper.Get(new Uri(serviceBaseUri.AbsoluteUri + metadataURL), null, maximumPayloadSize, reqHeaders);
                            if (responseMetadata != null)
                            {
                                metadataDocument = responseMetadata.ResponsePayload;
                            }
                        }
                        catch (OversizedPayloadException)
                        {
                            // do nothing
                        }
                    }
                }
            }

            if (payloadFormat == PayloadFormat.JsonLight)
            {
                try
                {
                    // Specify full metadata accept header
                    string acceptHeaderOfFullMetadata = string.Empty;
                    if (acceptHeader.Equals(Constants.V3AcceptHeaderJsonFullMetadata)
                        || acceptHeader.Equals(Constants.V3AcceptHeaderJsonMinimalMetadata)
                        || acceptHeader.Equals(Constants.V3AcceptHeaderJsonNoMetadata))
                    {
                        acceptHeaderOfFullMetadata = Constants.V3AcceptHeaderJsonFullMetadata;
                    }
                    else 
                    {
                        acceptHeaderOfFullMetadata = Constants.V4AcceptHeaderJsonFullMetadata;
                    }

                    // Send full metadata request and get full metadata response.
                    var responseFullMetadata = WebHelper.Get(inputUri, acceptHeaderOfFullMetadata, maximumPayloadSize, reqHeaders);
                    if (responseFullMetadata != null)
                    {
                        jsonFullMetadataPayload = responseFullMetadata.ResponsePayload;
                        entityType = jsonFullMetadataPayload.GetFullEntityType(payloadType, payloadFormat, metadataDocument);
                    }
                }
                catch (OversizedPayloadException)
                {
                    // do nothing
                }
            }
            else
            {
                entityType = response.ResponsePayload.GetFullEntityType(payloadType, payloadFormat, metadataDocument);
            }
             
            return new ServiceContext(inputUri, 
                jobId, 
                response.StatusCode, 
                response.ResponseHeaders, 
                response.ResponsePayload, 
                entityType, 
                serviceBaseUri, 
                serviceDocument, 
                metadataDocument, 
                false,
                reqHeaders,
                odataMetadata,
                jsonFullMetadataPayload,
                category);
        }

        /// <summary>
        /// Creates an offline service context with specified job id from payload context and metadata document. 
        /// </summary>
        /// <param name="payload">The payload context</param>
        /// <param name="metadata">The metadata document</param>
        /// <param name="jobId">The job id</param>
        /// <param name="respHeaders">The optional Http response headers</param>
        /// <param name="reqHeaders">The request headers</param>
        /// <returns>Service context object which represents the offline interop validation session</returns>
        public static ServiceContext Create(string payload, string metadata, Guid jobId, string respHeaders, IEnumerable<KeyValuePair<string, string>> reqHeaders)
        {
            if (string.IsNullOrEmpty(payload) && string.IsNullOrEmpty(metadata))
            {
                throw new ArgumentException("invalid parameters");
            }

            if (string.IsNullOrEmpty(payload))
            {
                payload = metadata;
                metadata = null;
            }

            Uri inputUri = new Uri(Constants.DefaultOfflineTarget);
            string entityType = null;
            Uri serviceBaseUri = null;
            string serviceDocument = null;
            string metadataDocument = null;
            var payloadFormat = payload.GetFormatFromPayload();
            var payloadType = payload.GetTypeFromPayload(payloadFormat);
            ODataMetadataType metadataType = ServiceContextFactory.GetMetadataTypeFromOfflineHeader(respHeaders);

            if (payloadType != PayloadType.Metadata && !string.IsNullOrEmpty(metadata))
            {
                var metaFormat = metadata.GetFormatFromPayload();
                if (metaFormat == PayloadFormat.Xml)
                {
                    var metaType = metadata.GetTypeFromPayload(metaFormat);
                    if (metaType == PayloadType.Metadata)
                    {
                        //do sanity check to ensure a matching metadata, should payload be a feed or an entry
                        if (IsMetadataMacthing(payload, metadata, payloadFormat, payloadType))
                        {
                            metadataDocument = metadata;
                        }
                    }
                }
            }

            switch (payloadType)
            {
                case PayloadType.ServiceDoc:
                    serviceDocument = payload;
                    if (payloadFormat == PayloadFormat.Xml || payloadFormat == PayloadFormat.Atom)
                    {
                        XElement payloadXml = XElement.Parse(payload);
                        XNamespace ns = "http://www.w3.org/XML/1998/namespace"; // xmlns:xml definition according to http://www.w3.org/TR/REC-xml-names/
                        var xmlBase = payloadXml.Attribute(ns + "base"); // rfc5023: it MAY have an "xml:base" attribute ... serving as the base URI
                        if (xmlBase != null && !string.IsNullOrEmpty(xmlBase.Value))
                        {
                            inputUri = new Uri(xmlBase.Value);
                        }
                    }
                    break;
                case PayloadType.Metadata:
                    serviceDocument = null;
                    metadataDocument = payload;
                    break;
                case PayloadType.Error:
                    break;
                case PayloadType.Feed:
                case PayloadType.Entry:
                    {
                        entityType = payload.GetFullEntityType(payloadType, payloadFormat, metadataDocument);
                        string shortEntityTypeName = entityType.GetLastSegment();
                        string target = payload.GetIdFromFeedOrEntry(payloadFormat);
                        if (!string.IsNullOrEmpty(target))
                        {
                            var projectedProperties = payload.GetProjectedPropertiesFromFeedOrEntry(payloadFormat, metadataDocument, shortEntityTypeName);
                            if (projectedProperties != null && projectedProperties.Any())
                            {
                                var opt = string.Join(",", projectedProperties);
                                try
                                {
                                    inputUri = new Uri(target + "?$select=" + opt);
                                }
                                catch (UriFormatException)
                                {
                                    //do nothing
                                }
                            }
                            else
                            {
                                try
                                {
                                    inputUri = new Uri(target);
                                }
                                catch (UriFormatException)
                                {
                                    //do nothing
                                }
                            }
                        }
                    }
                    break;
            }

            return new ServiceContext(inputUri, jobId, null, respHeaders, payload, entityType, serviceBaseUri, serviceDocument, metadataDocument, true, reqHeaders, metadataType);
        }

        /// <summary>
        /// Decides whether the payload and metadata are matched
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <param name="metadata">The metadata document content</param>
        /// <param name="payloadFormat">The payload format</param>
        /// <param name="payloadType">The payload type</param>
        /// <returns>True if they are matched; false otherwise</returns>
        private static bool IsMetadataMacthing(string payload, string metadata, PayloadFormat payloadFormat, PayloadType payloadType)
        {
            bool matched = false;

            // only payloads of feed or entry can check metadata matching 
            if (payloadType == PayloadType.Feed || payloadType == PayloadType.Entry)
            {
                XElement xmlMetadata;
                if (metadata.TryToXElement(out xmlMetadata))
                {
                    // try to extract domain namespace defined as attribute of edmN:Schema node in CSDL metadata 
                    var nodeSchema = xmlMetadata.XPathSelectElement("/*/*[local-name()='Schema' and @Namespace]", ODataNamespaceManager.Instance);
                    if (nodeSchema != null)
                    {
                        string domainNamespace = nodeSchema.Attribute("Namespace").Value;

                        // namespace must qualify the full entity type name
                        string entityTypeFullName = payload.GetFullEntityTypeFromPayload(payloadType, payloadFormat);

                        if (string.IsNullOrEmpty(entityTypeFullName))
                        {
                            string entitySetName = payload.GetEntitySetFromPayload(payloadType, payloadFormat);
                            entityTypeFullName = xmlMetadata.GetFullEntityTypeFromMetadata(entitySetName);
                        }

                        if (!string.IsNullOrEmpty(entityTypeFullName))
                        {
                            matched = entityTypeFullName.StartsWith(domainNamespace + ".");
                        }
                    }
                }
            }
            else if (payloadType == PayloadType.Metadata)
            {
                matched = string.Equals(payload, metadata, StringComparison.Ordinal);
            }
            else
            {
                matched = true;
            }

            return matched;
        }

        /// <summary>
        /// Determine the accept header value according to the format hint
        /// </summary>
        /// <param name="format">The format hint</param>
        /// <returns>The value string to be set to Accept header field of HTTP requests</returns>
        public static string MapFormatToAcceptValue(this string format)
        {
            if (format.Equals(Constants.FormatJson, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.AcceptHeaderJson;
            }
            else if (format.Equals(Constants.V3FormatJsonVerbose, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.V3AcceptHeaderJsonVerbose;
            }
            else if (format.Equals(Constants.V3FormatJsonFullMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.V3AcceptHeaderJsonFullMetadata;
            }
            else if (format.Equals(Constants.V3FormatJsonMinimalMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.V3AcceptHeaderJsonMinimalMetadata;
            }
            else if (format.Equals(Constants.V3FormatJsonNoMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.V3AcceptHeaderJsonNoMetadata;
            }
            else if (format.Equals(Constants.V4FormatJsonFullMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.V4AcceptHeaderJsonFullMetadata;
            }
            else if (format.Equals(Constants.V4FormatJsonMinimalMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.V4AcceptHeaderJsonMinimalMetadata;
            }
            else if (format.Equals(Constants.V4FormatJsonNoMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.V4AcceptHeaderJsonNoMetadata;
            }
            else
            {
                return Constants.AcceptHeaderAtom;
            }
        }

        /// <summary>
        /// Helper method to find the service document of the OData service this context refers to
        /// </summary>
        /// <param name="maximumPayloadSize">maximum payload size in byte</param>
        /// <param name="uri">uri hint, which is used to derive various possible uris in hope one of them is the service base endpoint</param>
        /// <param name="acceptHeaderValue">value of header of Accept that will be used in request getting the response</param>
        /// <param name="IgnoreInputUri">if true, the given uri is known other than service base</param>
        /// <param name="serviceBaseUri">out parameter: the service base uri if one shall be found</param>
        /// <param name="responseServiceDocument">out parameter: the response object including payload, headers and status code</param>
        /// <param name="reqHeaders">Http headers to be sent out to server</param>
        /// <returns>true if a valid service document is found; otherwise false</returns>
        private static bool TryGetServiceDocument(int maximumPayloadSize,
            Uri uri,
            string acceptHeaderValue,
            bool IgnoreInputUri,
            out Uri serviceBaseUri,
            out Response responseServiceDocument,
            IEnumerable<KeyValuePair<string, string>> reqHeaders)
        {
            if (!IgnoreInputUri)
            {
                try
                {
                    var response = WebHelper.Get(uri, acceptHeaderValue, maximumPayloadSize, reqHeaders);
                    if (response.IsServiceDocument())
                    {
                        serviceBaseUri = uri;
                        responseServiceDocument = response;
                        return true;
                    }
                }
                catch (OversizedPayloadException)
                {
                    // does nothing
                }
            }

            var segments = uri.Segments;
            if (segments != null && segments.Length > 0)
            {
                Uri uriParent;
                if (Uri.TryCreate(uri.GetLeftPart(UriPartial.Authority) + string.Join("", segments.Take(segments.Length - 1).ToArray()), UriKind.Absolute, out uriParent))
                {
                    if (uri != uriParent)
                    {
                        return ServiceContextFactory.TryGetServiceDocument(maximumPayloadSize, uriParent, acceptHeaderValue, false, out serviceBaseUri, out responseServiceDocument, reqHeaders);
                    }
                }
            }

            serviceBaseUri = null;
            responseServiceDocument = null;
            return false;
        }

        /// <summary>
        /// Determine the odata metadata type accrding to the format hint
        /// </summary>
        /// <param name="format">The format hint</param>
        /// <returns>The odata metadata type in request</returns>
        public static ODataMetadataType MapFormatToMetadataType(this string format)
        {
            if (format.Equals(Constants.V3FormatJsonFullMetadata, StringComparison.OrdinalIgnoreCase)
                || format.Equals(Constants.V4FormatJsonFullMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return ODataMetadataType.FullOnly;
            }
            else if (format.Equals(Constants.V3FormatJsonMinimalMetadata, StringComparison.OrdinalIgnoreCase)
                || format.Equals(Constants.V4FormatJsonMinimalMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return ODataMetadataType.MinOnly;
            }
            else 
            {
                return ODataMetadataType.None;
            }                 
        }

        /// <summary>
        /// Determine the odata metadata type accrding to the accept header hint
        /// </summary>
        /// <param name="format">The accept header hint</param>
        /// <returns>The odata metadata type in request</returns>
        public static ODataMetadataType MapAcceptHeaderToMetadataType(this string acceptHeader)
        {
            if (acceptHeader.Equals(Constants.V3AcceptHeaderJsonFullMetadata, StringComparison.OrdinalIgnoreCase)
                || acceptHeader.Equals(Constants.V4AcceptHeaderJsonFullMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return ODataMetadataType.FullOnly;
            }
            else if (acceptHeader.Equals(Constants.V3AcceptHeaderJsonMinimalMetadata, StringComparison.OrdinalIgnoreCase)
                || acceptHeader.Equals(Constants.V4AcceptHeaderJsonMinimalMetadata, StringComparison.OrdinalIgnoreCase))
            {
                return ODataMetadataType.MinOnly;
            }
            else
            {
                return ODataMetadataType.None;
            }
        }

        /// <summary>
        /// Parse the metadata type of offline payload from header.
        /// </summary>
        /// <param name="offlineHeader">The string offline header.</param>
        /// <returns>The metadata type of offline payload.</returns>
        public static ODataMetadataType GetMetadataTypeFromOfflineHeader(this string offlineHeader)
        {
            ODataMetadataType metadata = ODataMetadataType.MinOnly;

            if (offlineHeader.Contains(Constants.V3AcceptHeaderJsonFullMetadata) || offlineHeader.Contains(Constants.V4AcceptHeaderJsonFullMetadata))
            {
                metadata = ODataMetadataType.FullOnly;
            }

            return metadata;
        }
    }
}
