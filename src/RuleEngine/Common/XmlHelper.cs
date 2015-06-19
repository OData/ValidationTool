// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    #endregion

    /// <summary>
    /// Helper class for extension methods of Xml-Atom content
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Extension method to convert a string to an xml object
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <param name="xml">output parameter of the XElement object to read the data thta is contained in the stream</param>
        /// <returns>true if payload is converted to xml; otherwise false</returns>
        public static bool TryToXElement(this string payload, out XElement xml)
        {
            if (!string.IsNullOrEmpty(payload))
            {
                try
                {
                    using (StringReader sr = new StringReader(payload))
                    {
                        xml = XElement.Load(sr);
                        return true;
                    }
                }
                catch (XmlException)
                {
                    // does nothing
                }
            }

            xml = null;
            return false;
        }

        /// <summary>
        /// Extension method to determine whether a content if a valid XML literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsXmlPayload(this string payload)
        {
            XElement xml;
            return payload.TryToXElement(out xml);
        }

        /// <summary>
        /// Extension method to determine whether a content is an Atom feed document
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsAtomFeed(this string payload)
        {
            XElement xml;
            return payload.TryToXElement(out xml) && xml.Name.LocalName.Equals("feed", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Extension method to determine whether a content is an Atom feed document
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsAtomEntry(this string payload)
        {
            XElement xml;
            return payload.TryToXElement(out xml) && xml.Name.LocalName.Equals("entry", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Extension method to determine whether a content is Atom Service Document (in XML format)
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsAtomServiceDocument(this string payload)
        {
            XElement xml;
            return payload.TryToXElement(out xml) && xml.Name.LocalName.Equals("service", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Extension method to detemine whether a content is an metadata document (in xml format)
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsMetadata(this string payload)
        {
            XElement xml;
            return payload.TryToXElement(out xml) && xml.Name.LocalName.Equals("Edmx", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks whether the payload is of OData error message.
        /// </summary>
        /// <param name="payload">The payload content to be checked</param>
        /// <returns>True if it is OData error message; false otherwise</returns>
        public static bool IsError(this string payload)
        {
            XElement xml;
            return payload.TryToXElement(out xml) && xml.Name.LocalName.Equals("error", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks whether the payload is OData link message
        /// </summary>
        /// <param name="payload">The payload content to be checked</param>
        /// <returns>True if it is OData link message; false otherwise</returns>
        public static bool IsLink(this string payload)
        {
            bool result = false;
            XElement xml;
            
            if (payload.TryToXElement(out xml))
            {
                result = xml.Name.LocalName.Equals("uri", System.StringComparison.Ordinal)
                    || xml.Name.LocalName.Equals("links", System.StringComparison.Ordinal);
            }

            return result;
        }

        /// <summary>
        /// Checks whether the payload is of OData Individual Property message.
        /// </summary>
        /// <param name="payload">The payload content to be checked</param>
        /// <returns>True if it is OData error message; false otherwise</returns>
        public static bool IsAtomIndividualProperty(this string payload)
        {
            XElement xml;
            return payload.TryToXElement(out xml) && xml.Name.LocalName.Equals("value", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks whether the payload is OData entity reference.
        /// </summary>
        /// <param name="payload">The paylod to be checked</param>
        /// <returns>True if it is OData entity reference message</returns>
        public static bool IsEntityRef(this string payload)
        {
            bool result = false;

            XmlDocument xmlPayload = new XmlDocument();

            xmlPayload.LoadXml(payload);
            string xPath = @"//*[local-name()='ref']";

            XmlNodeList elems = xmlPayload.SelectNodes(xPath, ODataNamespaceManager.Instance);

            if (null != elems && elems.Count > 0)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Checks whether the payload is OData property
        /// </summary>
        /// <param name="payload">The paylod to be checked</param>
        /// <returns>True if it is OData property message</returns>
        public static bool IsProperty(this string payload)
        {
            // any XML document can represent an OData property / collection of OData properties
            XElement xml;
            return payload.TryToXElement(out xml);
        }

        /// <summary>
        /// Extension method to extract full value of Category's term attribute out of Atom formatted payload
        /// </summary>
        /// <param name="payload">payload content as xml node</param>
        /// <returns>string value of the interesting entityset if one could be found</returns>
        public static string GetEntityType(this XContainer payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            // to avoid atom:category elements expanded from navigation property
            string xpath = "./atom:category | ./atom:entry/atom:category";
            var categories = payload.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            if (categories.Any())
            {
                var attribute = categories.First().Attribute("term");
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method to get unescaped string value of a XElement object
        /// </summary>
        /// <param name="xmlNode">the XElement object that might contain value text</param>
        /// <returns>unescaped text of the XElement object's text value</returns>
        public static string UnescapeElementValue(this XContainer xmlNode)
        {
            if (xmlNode != null)
            {
                var nodes = xmlNode.Nodes().OfType<XText>();
                if (nodes.Count() > 0)
                {
                    return nodes.First().Value.Trim();
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method to get unescaped text value of a named subelement
        /// </summary>
        /// <param name="xml">XElement object which may include a subelement</param>
        /// <param name="subElementName">name of the sub element</param>
        /// <returns>unescaped text value of the named sub element</returns>
        public static string GetFirstSubElementValue(this XContainer xml, string subElementName)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            var sub = xml.Element(subElementName);

            if (sub != null)
            {
                return sub.UnescapeElementValue();
            }
 
            return null;
        }

        /// <summary>
        /// Extension method to get a named attribute value of an xml element
        /// </summary>
        /// <param name="xml">xml element node</param>
        /// <param name="attribute">name of the attribute</param>
        /// <returns>string value of named attribute if one could be found</returns>
        public static string GetAttributeValue(this XElement xml, string attribute)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            var attr = xml.Attribute(attribute);
            return (attr != null) ? attr.Value : null;
        }

        /// <summary>
        /// Gets a named attribute value of an xml element
        /// </summary>
        /// <param name="xml">The xml node</param>
        /// <param name="attribute">The attribute name. It could be of prefix:localname form</param>
        /// <param name="nsResolver">The Xml namespace resolver object</param>
        /// <returns>The value of named attribute if found; null otherwise</returns>
        public static string GetAttributeValue(this XElement xml, string attribute, XmlNamespaceManager nsResolver)
        {
            string fullName = attribute;
            string[] name = attribute.Split(new char[]{':'}, 2);
            if (name != null && name.Length == 2)
            {
                var ns = nsResolver.LookupNamespace(name[0]);
                fullName = string.Format("{{{0}}}{1}", ns, name[1]);
            }

            return GetAttributeValue(xml, fullName);
        }

        /// <summary>
        /// Extension method to indicate whether a payload XElement object is about media link entry
        /// </summary>
        /// <param name="xml">XElement object representing payload content</param>
        /// <returns>true for a media link entry; otherwise false</returns>
        public static bool IsMediaLinkEntry(this XElement xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            if (xml.Name.LocalName.Equals("entry", System.StringComparison.Ordinal))
            {
                var children = from e in xml.Elements()
                               where e.Name.LocalName == "properties"
                               select e;
                return children.Count() == 1;
            }

            return false;
        }

        /// <summary>
        /// Extension method to properly format xml payload
        /// </summary>
        /// <param name="node">XElement object whose payload is to be formatted</param>
        /// <returns>well-formatted payload</returns>
        public static string FineFormat(this XElement node)
        {
            if (node == null)
            {
                return null;
            }

            return node.ToString();
        }

        /// <summary>
        /// Gets encoding value from xml declaration
        /// </summary>
        /// <param name="xml">xml literal</param>
        /// <returns>string of encoding</returns>
        public static string GetEmbeddedEncoding(this string xml)
        {
            if (!string.IsNullOrEmpty(xml))
            {
                try
                {
                    var xdoc = XDocument.Parse(xml);
                    if (xdoc != null)
                    {
                        var xdecl = xdoc.Declaration;
                        if (xdecl != null)
                        {
                            return xdecl.Encoding;
                        }
                    }
                }
                catch (XmlException)
                {
                    // does nothing
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the full path of Uri which contains the current EntitySet fron the payload content
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <returns>The full path of Uri which indicates the current entity set</returns>
        public static string GetEntitySetFullPath(this XContainer payload)
        {
            string entitySet = null;

            if (payload != null)
            {
                var elements = from n in payload.Descendants()
                               where n.Name.LocalName.Equals("id", System.StringComparison.Ordinal)
                               select n.UnescapeElementValue();

                if (elements.Any())
                {
                    entitySet = elements.FirstOrDefault();
                }
            }

            return entitySet;
        }


        /// <summary>
        /// Gets id string of payload in atompub format
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <returns>The id Uri string pointing to itself</returns>
        public static string GetIdFromFeedOrEntry(string payload)
        {
            string id = null;

            try
            {
                XElement xml = XElement.Parse(payload);
                var node = xml.XPathSelectElement("./atom:id", ODataNamespaceManager.Instance);
                if (node != null)
                {
                    id = node.Value;
                }
            }
            catch (XmlException)
            {
                // do nothing
            }

            return id;
        }

        /// <summary>
        /// Gets projection of properties of the specified entity type from payload
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <param name="metadataDocument">The metadata document</param>
        /// <param name="shortEntityType">The short name of entity type</param>
        /// <returns>Collection of projected properties</returns>
        public static IEnumerable<string> GetProjectedPropertiesFromFeedOrEntry(string payload, string metadataDocument, string shortEntityType)
        {
            string[] projectedProperties = null;
            XElement xml = XElement.Parse(payload);

            if (!string.IsNullOrEmpty(metadataDocument) && !string.IsNullOrEmpty(shortEntityType))
            {
                var entry = xml.Name.LocalName.Equals("feed") ? xml.XPathSelectElement("./atom:entry", ODataNamespaceManager.Instance)
                    : xml.Name.LocalName.Equals("entry") ? xml
                    : null;
                if (entry != null)
                {
                    var ts = entry.XPathSelectElements(".//m:properties/*", ODataNamespaceManager.Instance); // mle or non-mle will do
                    int n = ts.Count();

                    XElement meta = XElement.Parse(metadataDocument);
                    string xpath = string.Format(".//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", shortEntityType);
                    var e = meta.XPathSelectElements(xpath);
                    var m = e.Count();

                    bool projectced = n != m;

                    if (projectced)
                    {
                        projectedProperties = ts.Select(t => t.Name.LocalName).ToArray();
                    }
                }
            }

            return projectedProperties;
        }

        /// <summary>
        /// Gets the collection of properties defined in the metadata document for the named entity type
        /// </summary>
        /// <param name="metadataDocument">The metadata document</param>
        /// <param name="shortEntityType">The specified entity type name</param>
        /// <returns>The collection of property names</returns>
        public static IEnumerable<string> GetProperties(string metadataDocument, string shortEntityType)
        {
            IEnumerable<string> result = null;

            if (!string.IsNullOrEmpty(metadataDocument) && !string.IsNullOrEmpty(shortEntityType))
            {
                XElement meta = XElement.Parse(metadataDocument);
                result = GetProperties(meta, shortEntityType);
            }

            return result;
        }

        /// <summary>
        /// Gets the count of properties of the specified entity type as defined in the metadata document
        /// </summary>
        /// <param name="shortEntityType">The short name of entity type</param>
        /// <param name="metadataDocument">The metadata document</param>
        /// <returns>The count of defined properties</returns>
        public static int GetCountOfProperties(string shortEntityType, string metadataDocument)
        {
            int count = 0;
            if (!string.IsNullOrEmpty(metadataDocument))
            {
                try
                {
                    XElement meta = XElement.Parse(metadataDocument);
                    string xpath = string.Format(".//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", shortEntityType);
                    var properties = meta.XPathSelectElements(xpath);
                    count = properties.Count();
                }
                catch (XmlException)
                {
                }
            }

            return count;
        }

        /// <summary>
        /// Gets list of feeds from a service document in Atompub format
        /// </summary>
        /// <param name="serviceDocument">The content of service document</param>
        /// <returns>The returned list of feeds exposed by the OData service</returns>
        public static IEnumerable<string> GetFeeds(string serviceDocument)
        {
            if (!serviceDocument.IsAtomServiceDocument())
            {
                throw new ArgumentException("Service document is expected.", "serviceDocument");
            }

            XElement service;
            TryToXElement(serviceDocument, out service);

            var collection = service.XPathSelectElements("//app:collection", ODataNamespaceManager.Instance);
            return from feed in collection select feed.GetAttributeValue("href");
        }

        /// <summary>
        /// Gets collection of entries from the feed resource in Atompub format
        /// </summary>
        /// <param name="feed">The feed resource content</param>
        /// <returns>The entries included in the feed content</returns>
        public static IEnumerable<string> GetEntries(string feed)
        {
            if (!feed.IsAtomFeed())
            {
                throw new ArgumentException("Feed resource is expected.", "feed");
            }

            XElement xml;
            TryToXElement(feed, out xml);
            var collection = xml.XPathSelectElements("/atom:entry/atom:id", ODataNamespaceManager.Instance);
            return from entry in collection select entry.Value;
        }

        public static List<string> GetSingletonNames(string metadataDocument)
        {
            List<string> names = new List<string>();

            if (!string.IsNullOrEmpty(metadataDocument))
            {
                try
                {
                    XElement meta = XElement.Parse(metadataDocument);
                    string xpath = ".//*[local-name()='EntityContainer']/*[local-name()='Singleton']";
                    var properties = meta.XPathSelectElements(xpath);

                    if (properties.Any())
                    {
                        foreach (XElement xe in properties)
                        {
                            names.Add(xe.GetAttributeValue("Name"));
                        }
                    }
                }
                catch (XmlException)
                {
                }
            }

            return names;
        }

        public static List<string> GetEntitySetNames(string metadataDocument)
        {
            List<string> names = new List<string>();

            if (!string.IsNullOrEmpty(metadataDocument))
            {
                try
                {
                    XElement meta = XElement.Parse(metadataDocument);
                    string xpath = ".//*[local-name()='EntityContainer']/*[local-name()='EntitySet']";
                    var properties = meta.XPathSelectElements(xpath);

                    if (properties.Any())
                    {
                        foreach (XElement xe in properties)
                        {
                            names.Add(xe.GetAttributeValue("Name"));
                        }
                    }
                }
                catch (XmlException)
                {
                }
            }

            return names;
        }

        public static string GetEntityTypeShortName(string entitySetName, string metadataDocument)
        {
            string entityTypeName = string.Empty;

            if (!string.IsNullOrEmpty(metadataDocument))
            {
                try
                {
                    XElement meta = XElement.Parse(metadataDocument);
                    string xpath = string.Format(".//*[local-name()='EntityContainer']/*[local-name()='EntitySet' and @Name='{0}']", entitySetName);
                    var entitySet = meta.XPathSelectElement(xpath);

                    if (entitySet != null)
                    {
                        string entityTypeNameWithNS = entitySet.GetAttributeValue("EntityType");
                        entityTypeName = entityTypeNameWithNS.Contains('.') ? entityTypeNameWithNS.Remove(0, entityTypeNameWithNS.LastIndexOf('.') + 1) : entityTypeNameWithNS;
                    }
                }
                catch (XmlException)
                {
                }
            }

            return entityTypeName;
        }


        public static List<string> GetNavigationProperties(string entitySetName, string metadataDocument)
        {
            List<string> names = new List<string>();
            string entityTypeName = string.Empty;

            if (!string.IsNullOrEmpty(metadataDocument))
            {
                try
                {
                    entityTypeName = GetEntityTypeShortName(entitySetName, metadataDocument);
                    XElement meta = XElement.Parse(metadataDocument);
                    string xpath = string.Format(".//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='NavigationProperty']", entityTypeName);
                    var properties = meta.XPathSelectElements(xpath);

                    if (properties.Any())
                    {
                        foreach (XElement xe in properties)
                        {
                            names.Add(xe.GetAttributeValue("Name"));
                        }
                    }
                }
                catch (XmlException)
                {
                }
            }

            return names;
        }

        public static List<string> GetNormalProperties(string entitySetName, string metadataDocument)
        {
            List<string> names = new List<string>();
            string entityTypeName = string.Empty;

            if (!string.IsNullOrEmpty(metadataDocument))
            {
                try
                {
                    XElement meta = XElement.Parse(metadataDocument);
                    entityTypeName = GetEntityTypeShortName(entitySetName, metadataDocument);
                    string xpath = string.Format(".//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property']", entityTypeName);
                    var properties = meta.XPathSelectElements(xpath);

                    if (properties.Any())
                    {
                        foreach (XElement xe in properties)
                        {
                            names.Add(xe.GetAttributeValue("Name"));
                        }
                    }
                }
                catch (XmlException)
                {
                }
            }

            return names;
        }

        /// <summary>
        /// Gets the collection of exposed properties of a entity type
        /// </summary>
        /// <param name="meta">The metadata document</param>
        /// <param name="shortEntityType">The entity type name</param>
        /// <returns>The collection of the exposed properties</returns>
        private static IEnumerable<string> GetProperties(XElement meta, string shortEntityType)
        {
            List<string> result = new List<string>();

            // if the netity type has @BaseType property, get the base type's proterties first
            string xpathEntityType = string.Format(".//*[local-name()='EntityType' and @Name='{0}']", shortEntityType);
            var et = meta.XPathSelectElement(xpathEntityType);
            if (et != null)
            {
                var baseType = et.GetAttributeValue("BaseType");
                if (!string.IsNullOrEmpty(baseType))
                {
                    var shortBaseType = baseType.GetLastSegment();
                    var props = GetProperties(meta, shortBaseType);
                    result = props.ToList();
                }
            }

            // to get properties of its own - excluding those having @m:FC_KeepInContent='false'
            string[] properties = null;
            string xpath = string.Format(".//*[local-name()='EntityType' and @Name='{0}']/*[local-name()='Property' and (not(@m:FC_KeepInContent) or @m:FC_KeepInContent!='false')]", shortEntityType);
            var e = meta.XPathSelectElements(xpath, ODataNamespaceManager.Instance);
            if (e != null)
            {
                var m = e.Count();
                properties = e.Select(t => t.Attribute("Name").Value).ToArray();
                result.AddRange(properties);
            }

            return result;
        }
    }
}
