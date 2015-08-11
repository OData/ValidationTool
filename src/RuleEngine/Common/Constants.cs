// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    /// <summary>
    /// Class to keep cross-component constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The timeout value of web request.
        /// </summary>
        public const string WebRequestTimeOut = "8000";

        /// <summary>
        /// HTTP header Content-Type for json-formatted payload
        /// </summary>
        public const string ContentTypeJson = @"application/json";

        /// <summary>
        /// HTTP header Content-Type for text/plain payload
        /// </summary>
        public const string ContentTypeTextPlain = @"text/plain";

        /// <summary>
        /// Regular expression for json content type
        /// </summary>
        public const string RegexContentTypeJson = @"application/json";

        /// <summary>
        /// HTTP header Content-Type for atom-formatted payload
        /// </summary>
        public const string ContentTypeAtom = @"application/atom+xml";

        /// <summary>
        /// HTTP header Content-Type for atomsvc-formatted payload
        /// </summary>
        public const string ContentTypeAtomSvc = @"application/atomsvc+xml";

        /// <summary>
        /// Regular expression for atompub content type
        /// </summary>
        public const string RegexContentTypeAtom = @"application/atom\+xml";

        /// <summary>
        /// HTTP header Content-Type for xml-formatted payload
        /// </summary>
        public const string ContentTypeXml = @"application/xml";

        /// <summary>
        /// Regular expression for xml content type
        /// </summary>
        public const string RegexContentTypeXml = @"application/xml";

        /// <summary>
        /// HTTP header Content-Type for text/xml
        /// </summary>
        public const string ContentTypeTextXml = @"text/xml";

        /// <summary>
        /// Regular expression for text content type
        /// </summary>
        public const string RegexContentTypeTextXml = @"text/xml";

        /// <summary>
        /// HTTP header Content-Type for image/jpeg
        /// </summary>
        public const string ContentTypeJPEGImage = @"image/jpeg";

        /// <summary>
        /// HTTP header Content-Type for image/*
        /// </summary>
        public const string ContentTypeImage = @"image/.*";

        /// <summary>
        /// Regular expression for image content type
        /// </summary>
        public const string RegexContentTypeImage = @"image/.*";

        /// <summary>
        /// appendix segment to OData service base uri for endpoint of metadata document
        /// </summary>
        public const string OptionMetadata = "/" + Constants.Metadata;

        /// <summary>
        /// Keyword of content_type.
        /// </summary>
        public const string ContentType = "Content-Type";

        /// <summary>
        /// Keyword of DataServiceVersion.
        /// </summary>
        public const string DataServiceVersion = "DataServiceVersion";

        /// <summary>
        /// Keyword of OData-Version.
        /// </summary>
        public const string ODataVersion = "OData-Version";

        /// <summary>
        /// Keyword of Access-Control-Allow-Headers.
        /// </summary>
        public const string AllowHeaders = "Access-Control-Allow-Headers";

        /// <summary>
        /// Keyword of AcceptHeader.
        /// </summary>
        public const string AcceptHeader = "Accept";

        /// <summary>
        /// Keyword of MaxDataServiceVersion.
        /// </summary>
        public const string MaxVersion = "MaxDataServiceVersion";

        /// <summary>
        /// keyword of metadata document
        /// </summary>
        public const string Metadata = "$metadata";

        /// <summary>
        /// HTTP request header Accept to expect atom/xml response
        /// </summary>
        public const string AcceptHeaderAtom = @"*/*; q=0.2, application/atom+xml, application/xml; q=0.5";

        /// <summary>
        /// HTTP request header Accept to expect json response
        /// </summary>
        public const string AcceptHeaderJson = @"application/json";

        /// <summary>
        /// Undefined HTTP request header  Accept to expect a 415 Unsupported Media Type HTTP status code.
        /// </summary>
        public const string UndefinedAcceptHeader = @"odata/test";
        
        /// <summary>
        /// HTTP request odata v3 header Accept to expect json verbose format
        /// </summary>
        public const string V3AcceptHeaderJsonVerbose = AcceptHeaderJson + ";odata=verbose";

        /// <summary>
        /// HTTP request odata v3 header Accept to expect json response with full metadata
        /// </summary>
        public const string V3AcceptHeaderJsonFullMetadata = AcceptHeaderJson + @";odata=fullmetadata";

        /// <summary>
        /// HTTP request odata v3 header Accept to expect json response with minimal metadata
        /// </summary>
        public const string V3AcceptHeaderJsonMinimalMetadata = AcceptHeaderJson + @";odata=minimalmetadata";

        /// <summary>
        /// HTTP request odata v3 header Accept to expect json response with no metadata
        /// </summary>
        public const string V3AcceptHeaderJsonNoMetadata = AcceptHeaderJson + @";odata=nometadata";

        /// <summary>
        /// HTTP request odata v4 header Accept to expect json response with full metadata
        /// </summary>
        public const string V4AcceptHeaderJsonFullMetadata = AcceptHeaderJson + @";odata.metadata=full";

        /// <summary>
        /// HTTP request odata v4 header Accept to expect json response with minimal metadata
        /// </summary>
        public const string V4AcceptHeaderJsonMinimalMetadata = AcceptHeaderJson + @";odata.metadata=minimal";

        /// <summary>
        /// HTTP request odata v4 header Accept to expect json response with no metadata
        /// </summary>
        public const string V4AcceptHeaderJsonNoMetadata = AcceptHeaderJson + @";odata.metadata=none";

        /// <summary>
        /// expecting request context to be atom/xml format
        /// </summary>
        public const string FormatAtomOrXml = "atompub";

        /// <summary>
        /// expecting request context to be json format
        /// </summary>
        public const string FormatJson = "json";
        
        /// <summary>
        /// expecting request odata v3 context to be json verbose format
        /// </summary>
        public const string V3FormatJsonVerbose = FormatJson + ";odata=verbose";

        /// <summary>
        /// expecting request odata v3 context to be json format and full metadata
        /// </summary>
        public const string V3FormatJsonFullMetadata = FormatJson + ";odata=fullmetadata";

        /// <summary>
        /// expecting request odata v3 context to be json format and minimal metadata
        /// </summary>
        public const string V3FormatJsonMinimalMetadata = FormatJson + ";odata=minimalmetadata";

        /// <summary>
        /// expecting request odata v3 context to be json format and minimal metadata
        /// </summary>
        public const string V3FormatJsonNoMetadata = FormatJson + ";odata=nometadata";

        /// <summary>
        /// expecting request odata v4 context to be json format and full metadata
        /// </summary>
        public const string V4FormatJsonFullMetadata = FormatJson + ";odata.metadata=full";

        /// <summary>
        /// expecting request odata v4 context to be json format and minimal metadata
        /// </summary>
        public const string V4FormatJsonMinimalMetadata = FormatJson + ";odata.metadata=minimal";

        /// <summary>
        /// expecting request odata v4 context to be json format and minimal metadata
        /// </summary>
        public const string V4FormatJsonNoMetadata = FormatJson + ";odata.metadata=none";

        /// <summary>
        /// classification of success
        /// </summary>
        public const string ClassificationSuccess = "success";

        /// <summary>
        /// classification of error
        /// </summary>
        public const string ClassificationError = "error";

        /// <summary>
        /// classification of warning
        /// </summary>
        public const string ClassificationWarning = "warning";

        /// <summary>
        /// classification of recommendation
        /// </summary>
        public const string ClassificationRecommendation = "recommendation";

        /// <summary>
        /// classification of not-applicable
        /// </summary>
        public const string ClassificationNotApplicable = "notApplicable";

        /// <summary>
        /// classification of aborted
        /// </summary>
        public const string ClassificationAborted = "aborted";

        /// <summary>
        /// classification of pending
        /// </summary>
        public const string ClassificationPending = "pending";

        /// <summary>
        /// classification of pending
        /// </summary>
        public const string ClassificationSkip = "skip";

        /// <summary>
        /// The default target uri string of offline service context. 
        /// This is pretty much an dummy string serving as a placeholder of offline context.
        /// </summary>
        public const string DefaultOfflineTarget = "http://offline";

        /// <summary>
        /// The property name of __deferred in v3 json verbose response. 
        /// </summary>
        public const string JsonVerboseDeferredPropertyName = @"__deferred";

        /// <summary>
        /// The property name of content_type in v3 json verbose response. 
        /// </summary>
        public const string JsonVerboseContent_TypeProperty = "content_type";

        /// <summary>
        /// The "d" mark in v3 json verbose response. 
        /// </summary>
        public const string BeginMarkD = @"d";

        /// <summary>
        /// The results property in v3 json verbose response. 
        /// </summary>
        public const string Results = @"results";

        /// <summary>
        /// The result property in v3 json verbose response. 
        /// </summary>
        public const string Result = @"result";

        /// <summary>
        /// The property name of __metadata in v3 json verbose response. 
        /// </summary>
        public const string JsonVerboseMetadataPropertyName = @"__metadata";

        /// <summary>
        /// The property name of uri in v3 json verbose response. 
        /// </summary>
        public const string JsonVerboseUriPropertyName = @"uri";

        /// <summary>
        /// The first property name of Odata v3 json response. 
        /// </summary>
        public const string OdataV3JsonIdentity = "odata.metadata";

        /// <summary>
        /// The first property name of Odata v4 json response. 
        /// </summary>
        public const string OdataV4JsonIdentity = "@odata.context";

        /// <summary>
        /// The identity of Service Document's odata.metadata value in Odata v3 json response. 
        /// </summary>
        public const string JsonSvcDocIdentity = "$metadata";

        /// <summary>
        /// The identity of Entity's odata.metadata value in Odata v3 json response. 
        /// </summary>
        public const string V3JsonEntityIdentity = "@Element";

        /// <summary>
        /// The identity of Entity's odata.context value in Odata v4 json response. 
        /// </summary>
        public const string V4JsonEntityIdentity = "$entity";

        /// <summary>
        /// The identity of feed's odata.metadata value in Odata v3 json response. 
        /// </summary>
        public const string JsonFeedIdentity = "$metadata#";
        
        /// <summary>
        /// The identity of Odata v3 json verbose error response. 
        /// </summary>
        public const string V3JsonVerboseErrorResponseIdentity = "error";

        /// <summary>
        /// The identity of Odata v3 json light error response. 
        /// </summary>
        public const string V3JsonLightErrorResponseIdentity = "odata.error";

        /// <summary>
        /// The identity of Odata v4 json light error response. 
        /// </summary>
        public const string V4JsonLightErrorResponseIdentity = "error";

        /// <summary>
        /// The message object name in json error response. 
        /// </summary>
        public const string MessageNameInJsonErrorResponse = "message";

        /// <summary>
        /// The identity of feed's odata.metadata value in Odata v3 json response. 
        /// </summary>
        public const string V3JsonDeltaResponseIdentity = "@delta";

        /// <summary>
        /// The identity of feed's odata.context value in Odata v4 json response. 
        /// </summary>
        public const string V4JsonDeltaResponseIdentity = "$delta";

        /// <summary>
        /// The identity of collection entity reference in Odata v4 json response. 
        /// </summary>
        public const string V4JsonCollectionEntityRefIdentity = @"$metadata#Collection($ref)";
        
        /// <summary>
        /// The identity of entity reference in Odata v4 json response. 
        /// </summary>
        public const string V4JsonEntityRefIdentity = @"$metadata#$ref";

        /// <summary>
        /// The "id" property of deleted entity in delta response.
        /// </summary>
        public const string ID = "id";

        /// <summary>
        /// The "reason" property of deleted entity in delta response.
        /// </summary>
        public const string Reason = "reason";

        /// <summary>
        /// The "source" property of link object in delta response. 
        /// </summary>
        public const string Source = "source";

        /// <summary>
        /// The "relationship" property of link object in delta response. 
        /// </summary>
        public const string Relationship = "relationship";

        /// <summary>
        /// The "target" property of link object in delta response. 
        /// </summary>
        public const string Target = "target";

        /// <summary>
        /// The value property name.
        /// </summary>
        public const string Value = "value";

        /// <summary>
        /// The kind property name.
        /// </summary>
        public const string Kind = "kind";

        /// <summary>
        /// The url property name.
        /// </summary>
        public const string Url = "url";

        /// <summary>
        /// The name property name.
        /// </summary>
        public const string Name = "name";

        /// <summary>
        /// The type property name.
        /// </summary>
        public const string Type = "type";

        /// <summary>
        /// The EntitySets property name.
        /// </summary>
        public const string EntitySets = "EntitySets";

        /// <summary>
        /// The EntitySet property name.
        /// </summary>
        public const string EntitySet = "EntitySet";

        /// <summary>
        /// The FunctionImport kind value.
        /// </summary>
        public const string FunctionImport = "FunctionImport";

        /// <summary>
        /// The Name attribute name.
        /// </summary>
        public const string NameAttribute = "Name";

        /// <summary>
        /// The EntityType attribute name.
        /// </summary>
        public const string EntityTypeAttribute = "EntityType";

        /// <summary>
        /// The odata namespace with dot.
        /// </summary>
        public const string OdataNS = "odata.";

        /// <summary>
        /// The v4 odata namespace with dot.
        /// </summary>
        public const string V4OdataNS = "@odata.";

        /// <summary> 
        /// The identity of entry's edit link name in Odata json response. 
        /// </summary> 
        public const string OdataEditLink = @"odata.editLink";

        /// <summary> 
        /// The identity of v4 entry's edit link name in Odata json response. 
        /// </summary> 
        public const string V4OdataEditLink = @"@odata.editLink";

        /// <summary>
        /// The identity of navigation link property's suffix name in Odata json response.
        /// </summary>
        public const string OdataNavigationLinkPropertyNameSuffix = @"@odata.navigationLink";

        /// <summary>
        /// The identity of association link property's suffix name in Odata json response.
        /// </summary>
        public const string OdataAssociationLinkPropertyNameSuffix = @"@odata.associationLink";

        /// <summary>
        /// The v3 odata.type property name.
        /// </summary>
        public const string OdataType = "odata.type";

        /// <summary>
        /// The v4 odata.type property name.
        /// </summary>
        public const string V4OdataType = "@odata.type";

        /// <summary>
        /// The v3 odata.id property name.
        /// </summary>
        public const string OdataId = "odata.id";

        /// <summary>
        /// The v4 odata.id property name.
        /// </summary>
        public const string V4OdataId = "@odata.id";

        /// <summary> 
        /// The v3 odata.etag annotation.
        /// </summary> 
        public const string OdataEtag = "odata.etag";

        /// <summary> 
        /// The v4 odata.etag annotation.
        /// </summary> 
        public const string V4OdataEtag = "@odata.etag";

        /// <summary> 
        /// The odata.readLink annotation.
        /// </summary> 
        public const string OdataReadLink = "odata.readLink";

        /// <summary> 
        /// The v4 odata.readLink annotation.
        /// </summary> 
        public const string V4OdataReadLink = "@odata.readLink";

        /// <summary> 
        /// The v3 odata.count property name. 
        /// </summary> 
        public const string OdataCount = "odata.count";

        /// <summary> 
        /// The v4 odata.count property name. 
        /// </summary> 
        public const string V4OdataCount = "@odata.count";

        /// <summary> 
        /// The odata.nextLink property name. 
        /// </summary> 
        public const string OdataNextLink = "odata.nextLink";

        /// <summary> 
        /// The v4 odata.nextLink property name. 
        /// </summary> 
        public const string V4OdataNextLink = "@odata.nextLink";

        /// <summary> 
        /// The odata.deltaLink property name. 
        /// </summary> 
        public const string OdataDeltaLink = "odata.deltaLink";

        /// <summary> 
        /// The v4 odata.deltaLink property name. 
        /// </summary> 
        public const string V4OdataDeltaLink = "@odata.deltaLink";

        /// <summary> 
        /// The start string of media entity annotations.
        /// </summary> 
        public const string OdataMedia = "odata.media";

        /// <summary> 
        /// The start string of v4 media entity annotations.
        /// </summary> 
        public const string V4OdataMedia = "@odata.media";

        /// <summary> 
        /// The odata.mediaEtag annotation.
        /// </summary> 
        public const string OdataMediaEtag = "odata.mediaEtag";

        /// <summary> 
        /// The v4 odata.mediaEtag annotation.
        /// </summary> 
        public const string V4OdataMediaEtag = "@odata.mediaEtag";

        /// <summary> 
        /// The identity of entry's media edit link name in Odata json response. 
        /// </summary> 
        public const string OdataMediaEditLink = @"odata.mediaEditLink";

        /// <summary> 
        /// The identity of v4 entry's media edit link name in Odata json response. 
        /// </summary> 
        public const string V4OdataMediaEditLink = @"@odata.mediaEditLink";

        /// <summary> 
        /// The identity of entry's media read link name in Odata json response. 
        /// </summary> 
        public const string OdataMediaReadLink = @"odata.mediaReadLink";

        /// <summary> 
        /// The identity of v4 entry's media read link name in Odata json response. 
        /// </summary> 
        public const string V4OdataMediaReadLink = @"@odata.mediaReadLink";

        /// <summary> 
        /// The odata.mediaContentType annotation.
        /// </summary> 
        public const string OdataMediaContentType = "odata.mediaContentType";

        /// <summary> 
        /// The v4 odata.mediaContentType annotation.
        /// </summary> 
        public const string V4OdataMediaContentType = "@odata.mediaContentType";

        /// <summary>
        /// The v3 service supports odata.streaming.
        /// </summary>
        public const string OdataStreaming = @"streaming=true";

        /// <summary>
        /// The v4 service supports odata.streaming.
        /// </summary>
        public const string V4OdataStreaming = @"odata.streaming=true";

        /// <summary>
        /// The prefix "Edm." in metadata.
        /// </summary>
        public const string EdmDotPrefix = @"Edm.";

        /// <summary>
        /// The namespace http://www.w3.org/2007/app
        /// </summary>
        public const string NSApp = @"http://www.w3.org/2007/app";

        /// <summary>
        /// The namespace http://www.w3.org/2005/Atom
        /// </summary>
        public const string NSAtom = @"http://www.w3.org/2005/Atom";

        /// <summary>
        /// The namespace http://schemas.microsoft.com/ado/2007/08/dataservices/metadata.
        /// </summary>
        public const string V3NSMetadata = @"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        /// <summary>
        /// The namespace http://docs.oasis-open.org/odata/ns/metadata.
        /// </summary>
        public const string NSMetadata = @"http://docs.oasis-open.org/odata/ns/metadata";

        /// <summary>
        /// The namespace http://schemas.microsoft.com/ado/2007/08/dataservices
        /// </summary>
        public const string V3NSData = @"http://schemas.microsoft.com/ado/2007/08/dataservices";

        /// <summary>
        /// The namespace http://docs.oasis-open.org/odata/ns/data.
        /// </summary>
        public const string V4NSData = @"http://docs.oasis-open.org/odata/ns/data";

        /// <summary>
        /// The scheme URL http://docs.oasis-open.org/odata/ns/scheme
        /// </summary>
        public const string SchemeURL = @"http://docs.oasis-open.org/odata/ns/scheme";

        /// <summary>
        /// The scheme URL http://docs.oasis-open.org/odata/ns/edmx
        /// </summary>
        public const string EdmxNs = @"http://docs.oasis-open.org/odata/ns/edmx";

        /// <summary>
        /// The namespace http://docs.oasis-open.org/odata/ns/relatedlinks
        /// </summary>
        public const string NSAssociationLink = @"http://docs.oasis-open.org/odata/ns/relatedlinks";

        /// <summary>
        /// The EDM namespace http://docs.oasis-open.org/odata/ns/edm
        /// </summary>
        public const string EdmNs = @"http://docs.oasis-open.org/odata/ns/edm";

        /// <summary>
        /// The immutable Collection regular expressions.
        /// </summary>
        public const string ImmutableCollectionRegexPattern = @"^#*Collection\((\w+\.)*\w+\)$";

        /// <summary>
        /// The mutable Collection regular expressions.
        /// </summary>
        public const string MutableCollectionRegexPattern = @"^#*Collection\({0}\.\w+\)$";

        /// <summary>
        /// The normal properties' data of an updatable entity.
        /// </summary>
        public const string UpdateData = @"[OData Validation Tool] Updated data";

        /// <summary> 
        /// The OData Test Key Name.
        /// </summary> 
        public const string ODataKeyName = "ODataKeyName";

        /// <summary> 
        /// The Location header.
        /// </summary> 
        public const string LocationHeader = "LocationHeader";      

        /// <summary>
        /// Error URI template.
        /// </summary>
        public const string ErrorURI = "<i><b>{0}</b> {1} <b>{2}</b></i> <br>";

        /// <summary>
        /// Error message template.
        /// </summary>
        public const string ErrorMsg = "<font color=\"red\"> {0} </font><br>";
    }
}
