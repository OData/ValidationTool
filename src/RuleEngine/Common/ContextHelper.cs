// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    #endregion

    /// <summary>
    /// Helper class of extension methods useful for context construction (including header parsing, payload parsing, etc) 
    /// </summary>
    public static class ContextHelper
    {
        /// <summary>
        /// Extension method to extract value of the named field in the header content
        /// </summary>
        /// <param name="headers">the headers content</param>
        /// <param name="keyword">field name</param>
        /// <param name="opt">comparison option like case-sensitive or not</param>
        /// <returns>string value of the field if it exists; otherwise null</returns>
        public static string GetHeaderValue(this string headers, string keyword, StringComparison opt = StringComparison.Ordinal)
        {
            if (!string.IsNullOrEmpty(headers))
            {
                var lines = headers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var pair = line.Split(new char[] {':' }, 2);
                    if (pair.Length == 2)
                    {
                        if (pair[0].Equals(keyword, opt))
                        {
                            return pair[1];
                        }
                    }
                }               
            }

            return null;
        }

        /// <summary>
        /// Extension method to map string to proper enum value of OData protocol version
        /// </summary>
        /// <param name="version">string; usually should be value of DataServiceVersion header field</param>
        /// <returns>OData protocol version, or null if input does not match any</returns>
        public static ODataVersion ToODataVersion(this string version)
        {
            if (!string.IsNullOrEmpty(version))
            {
                string v = version.TrimStart();
                if (Regex.IsMatch(v, @"^1\.0\s*;?\s*$"))
                {
                    return ODataVersion.V1;
                }
                else if (Regex.IsMatch(v, @"^2\.0\s*;?\s*$"))
                {
                    return ODataVersion.V2;
                }
                else if (Regex.IsMatch(v, @"^3\.0\s*;?\s*$"))
                {
                    return ODataVersion.V3;
                }
                else if (Regex.IsMatch(v, @"^4\.0\s*;?\s*$"))
                {
                    return ODataVersion.V4;
                }
                else if (v.StartsWith("1-2"))
                {
                    return ODataVersion.V1_V2;
                }
                else if (v.StartsWith("*"))
                {
                    return ODataVersion.V_All;
                }
            }

            return ODataVersion.UNKNOWN;
        }

        /// <summary>
        /// Extension method to determine OData protocol version from headers 
        /// </summary>
        /// <param name="headers">header block of response</param>
        /// <returns>OData protocol version decided from header block</returns>
        public static ODataVersion GetODataVersion(this string headers)
        {
            if (!string.IsNullOrEmpty(headers.GetHeaderValue(Constants.DataServiceVersion)))
            {
                string version = headers.GetHeaderValue(Constants.DataServiceVersion);

                if (version.Contains(";"))
                {
                    // To get v1/v2 version like "DataServiceVersion: 2.0; pyslet 0.4.20140429 "
                    return version.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0].ToODataVersion();
                }
                else
                {
                    return version.ToODataVersion();
                }
            }
            else
            {
                return headers.GetHeaderValue(Constants.ODataVersion).ToODataVersion();
            }           
        }

        /// <summary>
        /// Extension method to determine content-type value from headers
        /// </summary>
        /// <param name="headers">header block of response</param>
        /// <returns>Atom, Json, Xml, Other, None payload format based on Content-Type header value</returns>
        public static PayloadFormat GetContentType(this string headers)
        {
            string contentType = headers.GetContentTypeValue();
            ODataVersion version = headers.GetODataVersion();

            return contentType.ToContentType(version);
        }

        /// <summary>
        /// Extracts content type value from header string
        /// </summary>
        /// <param name="headers">response header</param>
        /// <returns>value of content type</returns>
        public static string GetContentTypeValue(this string headers)
        {
            if (!string.IsNullOrEmpty(headers))
            {
                var contentType = headers.GetHeaderValue(Constants.ContentType);

                if (string.IsNullOrEmpty(contentType))
                {
                    return null;
                }

                string[] pairs = contentType.Split(';');
                if (pairs.Length >= 1)
                {
                    var pair = pairs[0];
                    return pair.Trim();
                }
            }

            return null;
        }

        /// <summary>
        /// Extracts charset value from header block
        /// </summary>
        /// <param name="headers">response header</param>
        /// <returns>charset value</returns>
        public static string GetCharset(this string headers)
        {
            if (!string.IsNullOrEmpty(headers))
            {
                string contentType = headers.GetHeaderValue(Constants.ContentType);
                if (string.IsNullOrEmpty(contentType))
                {
                    return null;
                }

                string[] pairs = contentType.Split(';');
                foreach (var pair in pairs)
                {
                    string[] t = pair.Split(new char[] { '=' }, 2);
                    if (t.Length == 2)
                    {
                        string key = t[0].Trim();
                        if (key.Equals("charset", StringComparison.OrdinalIgnoreCase))
                        {
                            return t[1].Trim();
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method to determine payload format by looking at payload content
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>Atom, Json, Xml, Other, None payload format based on payload content</returns>
        public static PayloadFormat GetFormatFromPayload(this string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return PayloadFormat.None;
            }
            else if (payload.IsXmlPayload())
            {
                if (payload.IsEntityRef())
                {
                    return PayloadFormat.Xml;
                }

                return (payload.IsAtomFeed() || payload.IsAtomEntry()) ? PayloadFormat.Atom : PayloadFormat.Xml;
            }
            else if (payload.IsJsonPayload())
            {
                return payload.IsJsonVerbosePayload()? PayloadFormat.Json : PayloadFormat.JsonLight;
            }
            else
            {
                return PayloadFormat.Other;
            }
        }

        /// <summary>
        /// Extension method to determine the type of payload by looking at its content
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <param name="formatHint">content format (atom/xml or json)</param>
        /// <returns>Feed, Entry, SvcDoc, Metadata etc payload type based on payload content</returns>
        public static PayloadType GetTypeFromPayload(this string payload, PayloadFormat formatHint, string metadata = null)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return PayloadType.None;
            }
  
            switch (formatHint) 
            {
                case PayloadFormat.Atom:
                        return payload.IsAtomFeed() ? PayloadType.Feed:PayloadType.Entry;
                case PayloadFormat.Json:
                        return GetTypeFromJsonVerbosePayload(payload);
                case PayloadFormat.JsonLight:
                        return GetTypeFromJsonLightPayload(payload, metadata);
                case PayloadFormat.Xml:
                        return GetTypeFromXmlPayload(payload);
                default:
                    {
                        return PayloadType.Other;
                    }
            }
        }

        /// <summary>
        /// Gets payload type from XML payload
        /// </summary>
        /// <param name="payload">The payload in XML format</param>
        /// <returns>The type of payload</returns>
        private static PayloadType GetTypeFromXmlPayload(string payload)
        {
            if (payload.IsAtomServiceDocument())
            {
                return PayloadType.ServiceDoc;
            }
            else if (payload.IsMetadata())
            {
                return PayloadType.Metadata;
            }
            else if (payload.IsAtomIndividualProperty())
            {
                return PayloadType.IndividualProperty;
            }
            else if (payload.IsError())
            {
                return PayloadType.Error;
            }
            else if (payload.IsLink())
            {
                return PayloadType.Link;
            }
            else if (payload.IsEntityRef())
            {
                return PayloadType.EntityRef;
            }
            else if (payload.IsProperty())
            {
                return PayloadType.Property;
            }          
            else
            {
                return PayloadType.Other;
            }
        }

        /// <summary>
        /// Gets payload type from Json verbose payload
        /// </summary>
        /// <param name="payload">The payload in Json verbose format</param>
        /// <returns>The type of payload</returns>
        private static PayloadType GetTypeFromJsonVerbosePayload(string payload)
        {
            if (payload.IsJsonVerboseFeed())
            {
                return PayloadType.Feed;
            }
            else if (payload.IsJsonVerboseEntry())
            {
                return PayloadType.Entry;
            }
            else if (payload.IsJsonVerboseSvcDoc())
            {
                return PayloadType.ServiceDoc;
            }
            else if (payload.IsJsonVerboseError())
            {
                return PayloadType.Error;
            }
            else if (payload.IsJsonVerboseLink())
            {
                return PayloadType.Link;
            }
            else if (payload.IsJsonVerboseProperty())
            {
                return PayloadType.Property;
            }
            else
            {
                return PayloadType.Other;
            }
        }

        /// <summary>
        /// Gets payload type from Json light payload
        /// </summary>
        /// <param name="payload">The payload in Json light format</param>
        /// <returns>The type of payload</returns>
        private static PayloadType GetTypeFromJsonLightPayload(string payload, string metadata = null)
        {
            if (payload.IsJsonLightFeed(metadata))
            {
                return PayloadType.Feed;
            }
            else if (payload.IsJsonLightEntry(metadata))
            {
                return PayloadType.Entry;
            }
            else if (payload.IsJsonLightSvcDoc())
            {
                return PayloadType.ServiceDoc;
            }
            else if (payload.IsJsonLightError())
            {
                return PayloadType.Error;
            }
            else if (payload.IsJsonLightDeltaResponse())
            {
                return PayloadType.Delta;
            }    
            else if (payload.IsJsonLightEntityRef())
            {
                return PayloadType.EntityRef;
            }
            else if (payload.IsJsonLightIndividualProperty(metadata))
            {
                return PayloadType.IndividualProperty;
            }
            else
            {
                return PayloadType.Other;
            }
        }


        /// <summary>
        /// Extension method to extract th interesting entity type value from payload content
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <param name="typeHint">type of payload content</param>
        /// <param name="formatHint">format of payload content</param>
        /// <returns>the interesting entityset value (with namespace) if payload is entry or feed</returns>
        public static string GetFullEntityTypeFromPayload(this string payload, PayloadType typeHint, PayloadFormat formatHint)
        {
            if (!string.IsNullOrEmpty(payload) && (typeHint == PayloadType.Entry || typeHint == PayloadType.Feed))
            {
                switch (formatHint)
                {
                    case PayloadFormat.Atom:
                        {
                            XElement xml;
                            if (payload.TryToXElement(out xml))
                            {
                                return xml.GetEntityType();
                            }
                        }

                        break;
                    case PayloadFormat.Json:                    
                        {
                            JObject jo;
                            if (payload.TryToJObject(out jo))
                            {
                                return jo.GetJsonVerboseEntityType();
                            }
                        }

                        break;
                    case PayloadFormat.JsonLight:
                        {                            
                            JObject jo;
                            if (payload.TryToJObject(out jo))
                            {
                                if (typeHint == PayloadType.Feed)
                                {
                                    return jo.GetJsonLightEntityTypeFromFeed();
                                }
                                else if (typeHint == PayloadType.Entry)
                                {
                                    return jo.GetJsonLightEntityTypeFromEntry();
                                }
                            }
                        }
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets EntitySet value from payload
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <param name="typeHint">The type of payload</param>
        /// <param name="formatHint">The format of payload</param>
        /// <returns>EntitySet value extracted from the payload if one is found; null otherwise</returns>
        public static string GetEntitySetFromPayload(this string payload, PayloadType typeHint, PayloadFormat formatHint)
        {
            string entitySet = null;

            if (!string.IsNullOrEmpty(payload) && (typeHint == PayloadType.Entry || typeHint == PayloadType.Feed))
            {
                switch (formatHint)
                {
                    case PayloadFormat.Atom:
                        {
                            XElement xml;
                            if (payload.TryToXElement(out xml))
                            {
                                entitySet = ContextHelper.GetEntitySetName(xml.GetEntitySetFullPath());
                            }
                        }

                        break;
                    case PayloadFormat.Json:
                        {
                            JObject jo;
                            if (payload.TryToJObject(out jo))
                            {
                                entitySet = ContextHelper.GetEntitySetName(jo.GetEntitySetFullPath());
                            }
                        }

                        break;
                    case PayloadFormat.JsonLight:
                        {
                            entitySet = ContextHelper.GetEntitySetNameFromJsonLightPayload(payload);
                        }
                        break;
                }
            }

            return entitySet;
        }

        /// <summary>
        /// Gets full name of entity type from the metadata document by entity set short name.
        /// </summary>
        /// <param name="metadata">The metadata document</param>
        /// <param name="entitySet">The entity set short name</param>
        /// <returns>The full name of entity type if one can be found; null otherwise</returns>
        public static string GetFullEntityTypeFromMetadata(this XElement metadata, string entitySet)
        {
            string fullEntityType = null;

            if (metadata != null)
            {
                var entitySets = from e in metadata.Descendants()
                                 where e.Name.LocalName.Equals(Constants.EntitySet, StringComparison.OrdinalIgnoreCase)
                                 && (e.Attribute(Constants.NameAttribute) != null && e.Attribute(Constants.NameAttribute).Value.Equals(entitySet, StringComparison.OrdinalIgnoreCase))
                                 select e;
                if (entitySets.Any())
                {
                    var nodeEntitySet = entitySets.First();
                    if (nodeEntitySet.Attribute(Constants.EntityTypeAttribute) != null)
                    {
                        fullEntityType = nodeEntitySet.Attribute(Constants.EntityTypeAttribute).Value;
                    }
                }
            }

            return fullEntityType;
        }

        /// <summary>
        /// Extension method to indicate whether a payload is about a Media Link Entry
        /// </summary>
        /// <param name="payload">text content of the payload</param>
        /// <param name="typeHint">payload type, like feed, entry, etc</param>
        /// <param name="formatHint">payload format, like atom, json, etc</param>
        /// <returns>true if payload is a Media Link Entry; otherwise false</returns>
        public static bool IsMediaLinkEntry(this string payload, PayloadType typeHint, PayloadFormat formatHint)
        {
            if (typeHint == PayloadType.Entry)
            {
                switch (formatHint)
                {
                    case PayloadFormat.Atom:
                        {
                            XElement xml;
                            if (payload.TryToXElement(out xml))
                            {
                                return xml.IsMediaLinkEntry();
                            }
                        }

                        break;
                    case PayloadFormat.Json:
                        {
                            JObject jo;
                            if (payload.TryToJObject(out jo))
                            {
                                return jo.IsMediaLinkEntry();
                            }
                        }

                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// Extension method to properly format xml or json content
        /// </summary>
        /// <param name="payload">the content to be formatted</param>
        /// <param name="formatHint">format hint indicating whether content is xml/atom, json, or other</param>
        /// <returns>well-formatted string breaking into logical lines</returns>
        public static string FineFormat(this string payload, PayloadFormat formatHint)
        {
            switch (formatHint)
            {
                case PayloadFormat.Xml:
                    return payload.FineFormatAsXml();
                case PayloadFormat.Atom:
                    return payload.FineFormatAsXml();
                case PayloadFormat.Json:
                case PayloadFormat.JsonLight:
                    {
                        JObject jo;
                        if (payload.TryToJObject(out jo))
                        {
                            return jo.FineFormat();
                        }
                        else
                        {
                            return null;
                        }
                    }

                default:
                    return payload;
            }
        }

        /// <summary>
        /// Checks whether the payload type is a valid endpoint of OData service resource which the tool is able to validate
        /// </summary>
        /// <param name="payloadType">Payload type</param>
        /// <returns>True if the payload type is considered as a type which can be validated against some rules</returns>
        public static bool IsValidPayload(this PayloadType payloadType)
        {
            return payloadType == PayloadType.Entry
                || payloadType == PayloadType.Feed
                || payloadType == PayloadType.ServiceDoc
                || payloadType == PayloadType.Metadata
                || payloadType == PayloadType.Error
                || payloadType == PayloadType.Property
                || payloadType == PayloadType.RawValue
                || payloadType == PayloadType.Link
                || payloadType == PayloadType.Delta
                || payloadType == PayloadType.EntityRef
                || payloadType == PayloadType.IndividualProperty;
        }

        /// <summary>
        /// Gets the full entity type name from payload content with resolution of metadata if applicable
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <param name="payloadType">The payload type</param>
        /// <param name="payloadFormat">The payload format</param>
        /// <param name="metadataDocument">The metadata document</param>
        /// <returns>The fuly-qualified entity type name that be found; null if not found</returns>
        public static string GetFullEntityType(this string payload, PayloadType payloadType, PayloadFormat payloadFormat, string metadataDocument)
        {
            string entityType = null;
            if (!string.IsNullOrEmpty(payload))
            {
                entityType = payload.GetFullEntityTypeFromPayload(payloadType, payloadFormat);
                if (string.IsNullOrEmpty(entityType) && !string.IsNullOrEmpty(metadataDocument))
                {
                    string entitySet = payload.GetEntitySetFromPayload(payloadType, payloadFormat);
                    if (!string.IsNullOrEmpty(entitySet) && !string.IsNullOrEmpty(metadataDocument))
                    {
                        XElement metadata;
                        if (metadataDocument.TryToXElement(out metadata))
                        {
                            entityType = metadata.GetFullEntityTypeFromMetadata(entitySet);
                        }
                    }
                }
            }

            return entityType;
        }

        /// <summary>
        /// Gets the last segment of string delimited by '.' char
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The last segment separated by . char</returns>
        public static string GetLastSegment(this string input)
        {
            string lastLeg = null;
            if (!string.IsNullOrEmpty(input))
            {
                var segs = input.Split('.');
                lastLeg = segs[segs.Length - 1];
            }

            return lastLeg;
        }

        /// <summary>
        /// Gets the self id string of feed or entry payload
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <param name="format">The payload format</param>
        /// <returns>The Uri string pointing to itself</returns>
        public static string GetIdFromFeedOrEntry(this string payload, PayloadFormat format)
        {
            string id = null;
            JObject jo;
            if (format == PayloadFormat.Json)
            {               
                if (payload.TryToJObject(out jo))
                {
                    string idEntry = jo.GetEntitySetFullPath();
                    if (JsonHelper.IsJsonVerboseFeed(payload))
                    {
                        int posLeftParenthesis = idEntry.LastIndexOf('(');
                        string idFeed = posLeftParenthesis >= 0 ? idEntry.Substring(0, posLeftParenthesis) : idEntry;
                        id = idFeed;
                    }
                    else if (JsonHelper.IsJsonVerboseEntry(payload))
                    {
                        id = idEntry;
                    }
                }
            }
            else if (format == PayloadFormat.JsonLight)
            {
                if (payload.TryToJObject(out jo))
                {
                    if (JsonHelper.IsJsonLightFeed(payload))
                    {
                        var valueArray = (JArray)jo[Constants.Value];

                        if (valueArray != null && valueArray.Count > 0 && ((JArray)valueArray).First[Constants.OdataId] != null)
                        {
                            string idEntry = ((JArray)valueArray).First[Constants.OdataId].Value<string>();
                            int posLeftParenthesis = idEntry.LastIndexOf('(');
                            string idFeed = posLeftParenthesis >= 0 ? idEntry.Substring(0, posLeftParenthesis) : idEntry;
                            id = idFeed;
                        }
                    }
                    else if (JsonHelper.IsJsonLightEntry(payload) && jo[Constants.OdataId] != null)
                    {
                        id = jo[Constants.OdataId].Value<string>();
                    }
                }
            }
            else
            {
                id = XmlHelper.GetIdFromFeedOrEntry(payload);
            }

            return id;
        }

        /// <summary>
        /// Gets projection of properties of the specified entity type from payload
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <param name="format">The payload format</param>
        /// <param name="metadata">The metadata document</param>
        /// <param name="shortEntityType">The short name of entity type</param>
        /// <returns>Collection of projected properties</returns>
        public static IEnumerable<string> GetProjectedPropertiesFromFeedOrEntry(this string payload, PayloadFormat format, string metadata, string shortEntityType)
        {
            if (format == PayloadFormat.Json)
            {
                return JsonHelper.GetProjectedPropertiesFromJsonVerboseFeedOrEntry(payload, metadata, shortEntityType);
            }
            else if (format == PayloadFormat.JsonLight)
            {
                return JsonHelper.GetProjectedPropertiesFromJsonLightFeedOrEntry(payload, metadata, shortEntityType);
            }
            else
            {
                return XmlHelper.GetProjectedPropertiesFromFeedOrEntry(payload, metadata, shortEntityType);
            }
        }

        /// <summary>
        /// Gets collection of feeds defined in the service document
        /// </summary>
        /// <param name="serviceDocument">The OData service document</param>
        /// <param name="payloadFormat">The format type of service document</param>
        /// <returns>The collection of feed exposed</returns>
        public static IEnumerable<string> GetFeeds(string serviceDocument, PayloadFormat payloadFormat)
        {
            if (payloadFormat == PayloadFormat.Json || payloadFormat == PayloadFormat.JsonLight)
            {
                return JsonHelper.GetFeeds(serviceDocument);
            }           
            else
            {
                return XmlHelper.GetFeeds(serviceDocument);
            }
        }

        /// <summary>
        /// Gets collection of entries from the feed resource
        /// </summary>
        /// <param name="feed">The feed resource content</param>
        /// <param name="payloadFormat">The format of feed resource content</param>
        /// <returns>The entries included in the feed content</returns>
        public static IEnumerable<string> GetEntries(string feed, PayloadFormat payloadFormat)
        {
            if (payloadFormat == PayloadFormat.Json || payloadFormat == PayloadFormat.JsonLight)
            {
                return JsonHelper.GetEntries(feed);
            }            
            else
            {
                return XmlHelper.GetEntries(feed);
            }
        }

        /// <summary>
        /// Gets payload type from payload content, format hint, and HTTP header if existent
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <param name="payloadFormat">The payload format hint</param>
        /// <param name="headers">The HTTP header</param>
        /// <returns>The payload type</returns>
        public static PayloadType GetPayloadType(string payload, PayloadFormat payloadFormat, string headers, string metadata = null)
        {
            var payloadType = payload.GetTypeFromPayload(payloadFormat, metadata);

            // for other arbitrary payload to further differentiate those that seem response from an OData producer
            if (payloadType == PayloadType.Other && !string.IsNullOrEmpty(headers))
            {
                if (Regex.IsMatch(headers, @"^\s*DataServiceVersion\s*:", RegexOptions.Multiline)
                    || Regex.IsMatch(headers, @"^\s*OData-Version\s*:", RegexOptions.Multiline))
                {
                    payloadType = PayloadType.RawValue;
                }
            }

            //ContentType header shall be used to reveal raw value messages disguised with live producers
            if (!string.IsNullOrEmpty(headers))
            {
                if (payloadType == PayloadType.Property || payloadType == PayloadType.Link)
                {
                    if (Regex.IsMatch(headers, @"^\s*Content-Type\s*:\s*text/plain\s*;", RegexOptions.Multiline))
                    {
                        payloadType = PayloadType.RawValue;
                    }
                }
            }

            return payloadType;
        }

        /// <summary>
        /// If the string is quoted by double quotes, strip off the double quotes.
        /// </summary>
        /// <param name="valueString">The string need to be stripped off double quotes.</param>
        /// <returns>The string between double quotes.</returns>
        public static string StripOffDoubleQuotes(this string valueString)
        {
            string result = valueString;

            if (valueString.StartsWith("\"") && valueString.EndsWith("\""))
            {
                result = valueString.Substring(1, valueString.Length - 2);
            }

            return result;
        }

        /// <summary>
        /// Extension method to map string to enum value of Atom, JsonVerbos,JsonVerbos, Xml, etc
        /// </summary>
        /// <param name="value">value of content-type field of http header</param>
        /// <param name="version">Odata service version</param>
        /// <returns>Atom, JsonVerbos,JsonVerbos, Xml, Other, None based on input string value</returns>
        private static PayloadFormat ToContentType(this string value, ODataVersion version)
        {
            if (string.IsNullOrEmpty(value))
            {
                return PayloadFormat.None;
            }
            else if (Regex.IsMatch(value, Constants.RegexContentTypeJson))
            {
                if (version == ODataVersion.V3 || version == ODataVersion.V4)
                {
                    return PayloadFormat.JsonLight;
                }
                else
                {
                    return PayloadFormat.Json;
                }                
            }
            else if (Regex.IsMatch(value, Constants.RegexContentTypeAtom))
            {
                return PayloadFormat.Atom;
            }
            else if (Regex.IsMatch(value, Constants.RegexContentTypeXml))
            {
                return PayloadFormat.Xml;
            }
            else if (Regex.IsMatch(value, Constants.RegexContentTypeImage))
            {
                return PayloadFormat.Image;
            }
            else
            {
                return PayloadFormat.Other;
            }
        }

        /// <summary>
        /// Returns fine-formatted text for Xml literal
        /// </summary>
        /// <param name="payload">xml literal to be fine-formatted</param>
        /// <returns>fine-formatted xml text</returns>
        private static string FineFormatAsXml(this string payload)
        {
            XElement xml;
            if (payload.TryToXElement(out xml))
            {
                // to preserve the xml decalration line
                XDocument xdoc = XDocument.Parse(payload);
                if (xdoc.Declaration != null)
                {
                    string xmlDeclaration = xdoc.Declaration.ToString();
                    if (!string.IsNullOrEmpty(xmlDeclaration))
                    {
                        return xdoc.Declaration.ToString() + Environment.NewLine + xml.FineFormat();
                    }
                }
                else
                {
                    return xml.FineFormat();
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the EntitySet name from the full path of URI
        /// </summary>
        /// <param name="fullPath">Full path of the URI</param>
        /// <returns>EntitySet name</returns>
        private static string GetEntitySetName(string fullPath)
        {
            string entitySet = null;

            if (!string.IsNullOrEmpty(fullPath))
            {
                fullPath = fullPath.TrimEnd().Trim('/');
                var segments = fullPath.Split('/');
                if (segments.Length > 0)
                {
                    var lastSeg = segments[segments.Length - 1];
                    int posLeftParenthesis = lastSeg.IndexOf('(');
                    entitySet = posLeftParenthesis >= 0 ? lastSeg.Substring(0, posLeftParenthesis) : lastSeg;
                }
            }

            return entitySet;
        }

        /// <summary>
        /// Gets the EntitySet name from the json light payload
        /// </summary>
        /// <param name="payload">The josn light payload</param>
        /// <returns>EntitySet name</returns>
        private static string GetEntitySetNameFromJsonLightPayload(string payload)
        {
            string entitySet = null;
            string fullUri = null;
            JObject jo;

            if (payload.TryToJObject(out jo))
            {
                if (jo[Constants.OdataV3JsonIdentity] != null)
                {
                    fullUri = jo[Constants.OdataV3JsonIdentity].Value<string>().StripOffDoubleQuotes();
                }
                else if (jo[Constants.OdataV4JsonIdentity] != null)
                {
                    fullUri = jo[Constants.OdataV4JsonIdentity].Value<string>().StripOffDoubleQuotes();
                }

                if (fullUri != null)
                {
                    string[] metadataValueSegments = fullUri.Split('/');

                    foreach (string sub in metadataValueSegments)
                    {
                        // $metadata#
                        string identity = Constants.JsonFeedIdentity; 
                        if (sub.StartsWith(identity))
                        {
                            entitySet = sub.Substring(identity.Length);
                            break;
                        }
                    }

                }
            }

            return entitySet;
        }
    }
}
