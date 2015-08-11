// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class encapsulating OData interop request context
    /// </summary>
    public class ServiceContext : ServiceContextCore
    {
        private List<KeyValuePair<string, string>> reqHeaders;

        /// <summary>
        /// The response header of interop service context.
        /// </summary>
        private string respHeaders;

        /// <summary>
        /// The payload literal body of interop service context.
        /// </summary>
        private string payload;

        /// <summary>
        /// Metadata document.
        /// </summary>
        private string metadataDocument;

        /// <summary>
        /// Service document.
        /// </summary>
        private string serviceDocument;

        /// <summary>
        /// Fully-qualified name of entity type if applicable.
        /// </summary>
        private string entityTypeFullName;

        /// <summary>
        /// The complete metadata document that is merged with the including metadata document.
        /// </summary>
        string mergedMetadataDocument = null;

        /// <summary>
        /// The Metadata Service Schema Document.
        /// </summary>
        string metadataServiceSchemaDoc = null;

        /// <summary>
        /// Whether the metadata document includes external metadata document.
        /// </summary>
        bool? containsExternalSchema;

        /// <summary>
        /// Creates an instance of ServiceContext.
        /// </summary>
        /// <param name="destination">request Uri of the service context</param>
        /// <param name="jobId">Job identifier</param>
        /// <param name="statusCode">Http status code of the response</param>
        /// <param name="responseHttpHeaders">Header text of the response</param>
        /// <param name="responsePayload">Payload content of the response</param>
        /// <param name="entityType">Fully-qualified name of entity type the repsonse payload is about</param>
        /// <param name="serviceBaseUri">Uri of service document</param>
        /// <param name="serviceDocument">Content of service document</param>
        /// <param name="metadataDocument">Content of metadata document</param>
        /// <param name="offline">Flag of conetxt being offline(true) or live(false)</param>
        /// <param name="reqHeaders">Http headers used as part of header</param>
        /// <param name="odataMetadata">Odata metadata type</param>
        public ServiceContext(
            Uri destination,
            Guid jobId,
            HttpStatusCode? statusCode,
            string responseHttpHeaders,
            string responsePayload,
            string entityType,
            Uri serviceBaseUri,
            string serviceDocument,
            string metadataDocument,
            bool offline,
            IEnumerable<KeyValuePair<string, string>> reqHeaders,
            ODataMetadataType odataMetadata = ODataMetadataType.MinOnly,
            string jsonFullmetadataPayload = null,
            string category = "core")
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            // TODO: uncomment this to enable uri canonicalization
            // this.Destination = destination.Canonicalize();
            this.Destination = destination;
            this.DestinationBasePath = this.Destination.GetLeftPart(UriPartial.Path).TrimEnd('/');
            AnnotationDVsManager.Instance(this.DestinationBasePath);

            if (!string.IsNullOrEmpty(this.DestinationBasePath))
            {
                string lastLeg = this.DestinationBasePath.Substring(this.DestinationBasePath.LastIndexOf('/'));
                if (!string.IsNullOrEmpty(lastLeg))
                {
                    this.DestinationBaseLastSegment = lastLeg.TrimStart('/');
                }
            }

            this.Projection = this.Destination.Query.IndexOf("$select=", StringComparison.OrdinalIgnoreCase) >= 0;
            this.JobId = jobId;
            this.HttpStatusCode = statusCode;
            this.ResponseHttpHeaders = responseHttpHeaders;
            this.OdataMetadataType = odataMetadata;
            this.JsonFullMetadataPayload = jsonFullmetadataPayload;
            this.MetadataDocument = metadataDocument;
            this.ResponsePayload = responsePayload;
            this.EntityTypeFullName = entityType;
            this.ServiceBaseUri = serviceBaseUri;
            this.ServiceDocument = serviceDocument;
            this.IsOffline = offline;
            this.RequestHeaders = reqHeaders;
            this.Category = category;
            this.ServiceType = ConformanceServiceType.ReadWrite;
            this.LevelTypes = new ConformanceLevelType[] { ConformanceLevelType.Minimal };
            if (category.Contains(";"))
            {
                string[] array = category.Split(';');
                this.Category = array[0];
                this.ServiceType = (ConformanceServiceType)Enum.Parse(typeof(ConformanceServiceType), array[1]);
                if (array.Length > 2 && array[2].Contains(","))
                {
                    string[] levelArray = array[2].Split(',');
                    this.LevelTypes = new ConformanceLevelType[levelArray.Length];
                    for (int i = 0; i < levelArray.Length; i++)
                    {
                        ConformanceLevelType level;
                        if (Enum.TryParse(levelArray[i], out level))
                        {
                            this.LevelTypes[i] = level;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets Http header key/value pairs to be sent to server
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> RequestHeaders
        {
            get
            {
                return this.reqHeaders;
            }

            private set
            {
                if (value != null && value.Any())
                {
                    this.reqHeaders = new List<KeyValuePair<string, string>>();
                    foreach (var p in value)
                    {
                        if (!string.IsNullOrEmpty(p.Key))
                        {
                            this.reqHeaders.Add(p);

                            if (p.Key.ToLower().Contains("version"))
                            {
                                switch (p.Value)
                                {
                                    case "3.0":
                                        this.Version = ODataVersion.V3;
                                        break;
                                    case "4.0":
                                        this.Version = ODataVersion.V4;
                                        break;
                                    default:
                                        this.Version = ODataVersion.V1_V2;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets Validation Job Id
        /// </summary>
        public Guid JobId { get; private set; }

        /// <summary>
        /// Gets the canonicalized uri pointing to the OData service endpoint to be validated
        /// </summary>
        public Uri Destination { get; private set; }

        /// <summary>
        /// Gets string of uri pointing to the OData service endpoint excluding the query options
        /// </summary>
        public string DestinationBasePath { get; private set; }

        /// <summary>
        /// Gets the last segment of uri pointing to the OData service endpoint excluding the query options
        /// </summary>
        public string DestinationBaseLastSegment { get; private set; }

        /// <summary>
        /// Gets/sets http status code for the payload response when http/https protocol is used
        /// </summary>
        public HttpStatusCode? HttpStatusCode { get; private set; }

        /// <summary>
        /// Gets payload format indicated by http response header of Content-Type
        /// </summary>
        public PayloadFormat ContentType { get; private set; }

        /// <summary>
        /// Gets the underlying entity type short name this payload is about (applicable to feed or entry only)
        /// </summary>
        public string EntityTypeShortName { get; private set; }

        /// <summary>
        /// Get the full metadata json format payload
        /// </summary>
        public string JsonFullMetadataPayload { get; private set; }

        /// <summary>
        /// Gets the underlying entity type full name this payload is about (applicable to feed or entry only)
        /// </summary>
        public string EntityTypeFullName
        {
            get
            {
                return this.entityTypeFullName;
            }

            private set
            {
                this.entityTypeFullName = value;
                this.EntityTypeShortName = this.entityTypeFullName.GetLastSegment();
            }
        }

        /// <summary>
        /// Gets response headers
        /// </summary>
        public string ResponseHttpHeaders
        {
            get
            {
                return this.respHeaders;
            }

            private set
            {
                this.respHeaders = value;
                //this.Version = this.respHeaders.GetODataVersion(); // We should set version by UI selected
                this.ContentType = this.respHeaders.GetContentType();
            }
        }

        /// <summary>
        /// Gets response payload
        /// </summary>
        public string ResponsePayload
        {
            get
            {
                return this.payload;
            }

            private set
            {
                this.PayloadFormat = value.GetFormatFromPayload();
                this.payload = value.FineFormat(this.PayloadFormat);
                this.PayloadType = ContextHelper.GetPayloadType(this.payload, this.PayloadFormat, this.respHeaders, this.MetadataDocument);

                if (this.OdataMetadataType == ODataMetadataType.MinOnly)
                {
                    this.EntityTypeFullName = this.JsonFullMetadataPayload.GetFullEntityTypeFromPayload(this.PayloadType, this.PayloadFormat);
                }
                else
                {
                    this.EntityTypeFullName = this.payload.GetFullEntityTypeFromPayload(this.PayloadType, this.PayloadFormat);
                }

                if (!string.IsNullOrEmpty(this.EntityTypeFullName))
                {
                    var segs = this.EntityTypeFullName.Split('.');
                    this.EntityTypeShortName = segs[segs.Length - 1];
                }

                this.IsMediaLinkEntry = this.payload.IsMediaLinkEntry(this.PayloadType, this.PayloadFormat);
            
                TermDocuments termDocs = TermDocuments.GetInstance();
                this.VocCapabilities = termDocs.VocCapabilitiesDoc;
                this.VocCore = termDocs.VocCoreDoc;
                this.VocMeasures = termDocs.VocMeasuresDoc;
            }
        }

        /// <summary>
        /// Gets uri pointing to the service base of the OData service this context refers to
        /// </summary>
        public Uri ServiceBaseUri { get; private set; }

        /// <summary>
        /// Gets content of service document of the OData service this context refers to
        /// </summary>
        public string ServiceDocument
        {
            get
            {
                return this.serviceDocument;
            }

            private set
            {
                this.serviceDocument = value;
                this.HasServiceDocument = !string.IsNullOrEmpty(this.ServiceDocument);
            }
        }

        /// <summary>
        /// Gets content of metadata document of the OData service the uri refers to
        /// </summary>
        public string MetadataDocument
        {
            get
            {
                return this.metadataDocument;
            }

            private set
            {
                this.metadataDocument = value;

                this.HasMetadata = !string.IsNullOrEmpty(this.MetadataDocument) && this.MetadataDocument.IsMetadata();
            }
        }

        /// <summary>
        /// The complete metadata document that is merged with the including metadata document.
        /// </summary>
        public string MergedMetadataDocument
        {
            get
            {
                if (!String.IsNullOrEmpty(mergedMetadataDocument))
                {
                    return this.mergedMetadataDocument;
                }
                else
                {
                    try
                    {
                        this.mergedMetadataDocument = this.getAndMergeExternalSchema();
                    }
                    catch (Exception)
                    {
                        this.mergedMetadataDocument = this.metadataDocument;
                    }
                    return this.mergedMetadataDocument;
                }
            }
        }

        /// <summary>
        /// Gets whether metadata service document schema has been found for the OData service this context refers to
        /// </summary>
        public bool HasMetadataServiceSchema { get; protected set; }

        /// <summary>
        /// The metadata service schema document.
        /// /// </summary>
        public string MetadataServiceSchemaDoc
        {
            get
            {
                if (!String.IsNullOrEmpty(metadataServiceSchemaDoc))
                {
                    return this.metadataServiceSchemaDoc;
                }
                else
                {
                    try
                    {
                        this.metadataServiceSchemaDoc = this.GetMetadataServiceSchemaDoc();
                        this.HasMetadataServiceSchema = !string.IsNullOrEmpty(this.metadataServiceSchemaDoc) && this.metadataServiceSchemaDoc.IsMetadata();
                    }
                    catch (Exception)
                    {
                        this.metadataServiceSchemaDoc = this.metadataDocument;
                    }
                    return this.metadataServiceSchemaDoc;
                }
            }
        }

        /// <summary>
        /// Whether the metadata document includes external metadata document.
        /// </summary>
        public bool ContainsExternalSchema
        {
            get
            {
                if (containsExternalSchema != null)
                {
                    return containsExternalSchema.Value;
                }
                else
                {
                    XElement md = XElement.Parse(this.metadataDocument);
                    XElement entity = md.XPathSelectElement("./*[local-name()='Reference']/*[local-name()='Include']");
                    this.containsExternalSchema = entity != null;
                    return containsExternalSchema.Value;
                }
            }
        }

        public string VocCapabilities { get; private set; }

        public string VocCore { get; private set; }

        public string VocMeasures { get; private set; }

        /// <summary>
        /// Cache the Service verification result for some functions
        /// </summary>
        public ServiceVerification ServiceVerResult = new ServiceVerification();

        /// <summary>
        /// Gets response payload line-by-line
        /// </summary>
        /// <returns>lines of payload content</returns>
        [SuppressMessage("Microsoft.Design", "CA1024: convert method to property if appropriate", Justification = "defined as api expected by online service")]
        public IEnumerable<string> GetPayloadLines()
        {
            if (string.IsNullOrEmpty(this.payload))
            {
                yield break;
            }

            if (ContextHelper.IsValidPayload(this.PayloadType))
            {
                StringReader sr = new StringReader(this.payload);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
            else
            {
                yield break;
            }
        }

        public bool IsRequestVersion()
        {
            var respVersion = this.respHeaders.GetODataVersion();
            return respVersion == this.Version;
        }

        /// <summary>
        /// Merge the include metadata with current.
        /// </summary>
        /// <returns></returns>
        private string getAndMergeExternalSchema()
        {
            XmlDocument xmlDoc=new XmlDocument();
            xmlDoc.LoadXml(this.MetadataDocument);

            XmlNode dataServices = xmlDoc.SelectSingleNode("/*[local-name()='Edmx']/*[local-name()='DataServices']");

            string xpath = "/*[local-name()='Edmx']/*[local-name()='Reference']";
            XmlNodeList referenceNodeList = xmlDoc.SelectNodes(xpath);

            foreach (XmlNode reference in referenceNodeList)
            {
                if (reference.Attributes["Uri"] != null)
                {
                    try
                    {
                        Uri referenceUri = new Uri(reference.Attributes["Uri"].Value, UriKind.RelativeOrAbsolute);
                        var payload = XElement.Parse(this.MetadataDocument);
                        string baseUriString = payload.GetAttributeValue("xml:base", ODataNamespaceManager.Instance);
                        if (!string.IsNullOrEmpty(baseUriString) && Uri.IsWellFormedUriString(reference.Attributes["Uri"].Value, UriKind.Relative))
                        {
                            Uri referenceUriRelative = new Uri(reference.Attributes["Uri"].Value, UriKind.Relative);
                            Uri baseUri = new Uri(baseUriString, UriKind.Absolute);
                            referenceUri = new Uri(baseUri, referenceUriRelative);
                        }

                        Response response = WebHelper.Get(referenceUri, string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, this.RequestHeaders);
                        if (response != null && response.StatusCode.Value == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                        {
                            XmlDocument referenceDoc = new XmlDocument();
                            referenceDoc.LoadXml(response.ResponsePayload);
                            XmlNodeList referencedSchemas = referenceDoc.SelectNodes("/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema']");
                            merge(dataServices, referencedSchemas, reference);
                        }
                    }
                    // if one reference doc failed, we should continue merging others.
                    catch (ArgumentException)
                    { continue; }
                    catch (UriFormatException)
                    { continue; }
                    catch (XmlException)
                    { continue; }
                    catch (OversizedPayloadException)
                    { continue; }
                }
            }

            return xmlDoc.OuterXml;
        }

        private void merge(XmlNode dataServices, XmlNodeList referencedSchemas, XmlNode reference)
        {
            foreach (XmlNode include in reference.SelectNodes("./*[local-name()='Include' and @Namespace]"))
            {
                foreach (XmlNode node in referencedSchemas)
                {
                    if (node.Attributes["Namespace"] != null && include.Attributes["Namespace"].Value.Equals(node.Attributes["Namespace"].Value))
                    {
                        XmlNode toAdd = dataServices.OwnerDocument.CreateNode(XmlNodeType.Element, node.Name, node.NamespaceURI);
                        toAdd.InnerXml = node.InnerXml;
                        XmlAttribute ns = dataServices.OwnerDocument.CreateAttribute("Namespace");
                        ns.Value = node.Attributes["Namespace"].Value;
                        toAdd.Attributes.Append(ns);

                        if (include.Attributes["Alias"] != null)
                        {
                            XmlAttribute alias = dataServices.OwnerDocument.CreateAttribute("Alias");
                            alias.Value = include.Attributes["Alias"].Value;
                            toAdd.Attributes.Append(alias);
                        }

                        dataServices.AppendChild(toAdd);
                        break;
                    }
                }
            }
        }//merge

        /// <summary>
        /// Get the metadata service schema from the internet.
        /// </summary>
        /// <returns>The string of the metadata service schema document.</returns>
        private string GetMetadataServiceSchemaDoc()
        {
            //Due to the OData example services on OData.org haven't implement the metadata service schema, pause the testing about it now.
            //Uri metadataMetadataUri = new Uri("/$metadata/$metadata", UriKind.Relative);
            //Uri metadataServiceUri = new Uri(this.ServiceBaseUri, metadataMetadataUri); //Uri metadataServiceUri = new Uri("http://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/models/MetadataService.edmx", UriKind.Absolute);

            //Response response = WebHelper.Get(metadataServiceUri, string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, RequestHeaders);

            //if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
            //{
            //    return response.ResponsePayload;
            //}
            //else
            //{
                return null;
            //}
        }
    }
}
