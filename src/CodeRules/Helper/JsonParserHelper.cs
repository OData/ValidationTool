// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Utility class of JSON response parser. 
    /// </summary>
    public static class JsonParserHelper
    {
        /// <summary>
        /// Gets JSON response object from service context object.
        /// </summary>
        /// <param name="context">The service context</param>
        /// <returns>JObject of the response in JSON format; null otherwise</returns>
        public static JObject GetResponseObject(ServiceContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return JsonParserHelper.GetResponseObject(context.ResponsePayload);
        }

        /// <summary>
        /// Gets JSON object of response payload.
        /// </summary>
        /// <param name="payload">The payload literal</param>
        /// <returns>JObject of the response in JSON format; null otherwise</returns>
        public static JObject GetResponseObject(string payload)
        {
            JObject result = null;

            if (payload.TryToJObject(out result))
            {
                // do nothing; already get it in result object
            }

            return result;
        }

        /// <summary>
        /// Gets array of JSON object representing collection of entities.
        /// </summary>
        /// <param name="feed">The JSON objet of response payload for feed type</param>
        /// <returns>JArray object representing the feed; null if it is not feed in JSON</returns>
        public static JArray GetEntries(JObject feed)
        {
            JArray result = null;

            if (feed == null)
            {
                return null;
            }
            else if (feed.Count == 1)
            {
                var d = (JProperty)feed.First;
                if (d.Name.Equals(Constants.BeginMarkD, StringComparison.Ordinal))
                {
                    var sub = d.Value;

                    if (sub.Type == JTokenType.Array)
                    {
                        // V1 JSON format
                        result = (JArray)sub;
                    }
                    else if (sub.Type == JTokenType.Object)
                    {
                        // V2 format: looking for name-value pair of "result:{...}"
                        JObject resultObject = (JObject)sub;
                        var resultValues = from p in resultObject.Children<JProperty>()
                                           where p.Name.Equals(Constants.Results, StringComparison.Ordinal)
                                           && p.Value.Type == JTokenType.Array
                                           select (JArray)p.Value;

                        if (resultValues.Any())
                        {
                            result = resultValues.First();
                        }
                    }
                }
            }
            else
            {
                foreach (var r in feed.Children<JProperty>())
                {
                    if (r.Name.Equals(Constants.Value, StringComparison.Ordinal)
                        && r.Value.Type == JTokenType.Array)
                    {
                        result = (JArray)r.Value;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all entities from the service's response payload.
        /// </summary>
        /// <param name="responsePayload">The specified response payload.</param>
        /// <returns>Returns the entities' list.</returns>
        public static List<JObject> GetEntities(string responsePayload)
        {
            var entities = new List<JObject>();
            if (string.IsNullOrEmpty(responsePayload))
            {
                return entities;
            }

            JObject jObj = JObject.Parse(responsePayload);
            JArray jArr = jObj.GetValue("value") as JArray;
            if (null != jArr)
            {
                if (jArr.Any())
                {
                    foreach (JObject entity in jArr)
                    {
                        entities.Add(entity);
                    }
                }
                else
                {
                    return entities;
                }
            }
            else
            {
                entities.Add(jObj);
            }

            return entities;
        }

        /// <summary>
        /// Gets the last entry from the collection of entries.
        /// </summary>
        /// <param name="entries">The collection of entries</param>
        /// <returns>JOBject of the last entity </returns>
        public static JObject GetLastEntry(JArray entries)
        {
            JObject lastEntry = null;

            if (entries != null && entries.Count > 0)
            {
                JToken lastItem = entries.Last(o => o.Type == JTokenType.Object);
                lastEntry = (JObject)lastItem;
            }

            return lastEntry;
        }

        /// <summary>
        /// Gets the opaque token value of the entity.
        /// </summary>
        /// <param name="entry">The entity</param>
        /// <returns>The token value if one is found; null otherwise</returns>
        public static string GetTokenOfEntry(JObject entry)
        {
            string token = null;

            string selfTag = JsonHelper.GetPropertyOfElement(entry, Constants.JsonVerboseMetadataPropertyName, Constants.JsonVerboseUriPropertyName);
            if (!string.IsNullOrEmpty(selfTag))
            {
                int length = selfTag.Length;
                if (selfTag[length - 1] == ')')
                {
                    int posLastSeg = selfTag.LastIndexOf('/');
                    int openParenthesis = posLastSeg >= 0 ? selfTag.IndexOf('(', posLastSeg) : selfTag.LastIndexOf('(');
                    token = selfTag.Substring(openParenthesis + 1, length - openParenthesis - 2);
                }
            }

            return token;
        }

        /// <summary>
        /// Validates a Json schema against a payload string
        /// </summary>
        /// <param name="jschema">The JSon schema</param>
        /// <param name="payload">The payload string</param>
        /// <param name="testResult">output parameter of detailed test result</param>
        /// <returns>Returns true when the payload is validated; otherwise false</returns>
        public static bool ValidateJson(string jschema, string payload, out RuleEngine.TestResult testResult)
        {
            JsonSchemaVerifier verifer = new JsonSchemaVerifier(jschema);
            return verifer.Verify(payload, out testResult);
        }

        /// <summary>
        /// Gets the OData version implication from Json payload object 
        /// </summary>
        /// <param name="jo">The Json payload input</param>
        /// <returns>The OData version implication based on the Json object wrapping</returns>
        public static ODataVersion GetPayloadODataVersion(JObject jo)
        {
            ODataVersion result = ODataVersion.UNKNOWN;

            var d = from p in jo.Properties()
                    where p.Name.Equals(Constants.BeginMarkD, StringComparison.Ordinal)
                    select p;

            if (d.Any())
            {
                result = ODataVersion.V1;

                var dToken = d.First().Value;
                if (dToken.Type == JTokenType.Object)
                {
                    var dObject = (JObject)dToken;
                    var r = from p in dObject.Properties()
                            where p.Name.Equals(Constants.Result, StringComparison.Ordinal)
                            select p;
                    if (r.Any())
                    {
                        result = ODataVersion.V2;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Whether specified Annoatation exist.
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="specifiedAnnoatation">The specified Annoatation</param>
        /// <returns>true if exist; false otherwise</returns>
        public static bool IsSpecifiedAnnotationExist(ServiceContext context, string specifiedAnnoatation)
        {
            JObject allobject;
            context.ResponsePayload.TryToJObject(out allobject);
            bool isExist = false;

            // If PayloadType is Feed, verify as below.
            if (context.PayloadType.Equals(RuleEngine.PayloadType.Feed))
            {
                var entries = JsonParserHelper.GetEntries(allobject);
                foreach (JObject entry in entries)
                {
                    var jProps = entry.Children();

                    foreach (JProperty jProp in jProps)
                    {
                        // Whether specified Annoatation exist in response.
                        if (jProp.Name.Equals(specifiedAnnoatation))
                        {
                            isExist = true;
                            break;
                        }
                        else
                        {
                            isExist = false;
                        }
                    }

                    if (isExist)
                    {
                        break;
                    }
                }
            }
            else
            {
                var jProps = allobject.Children();

                foreach (JProperty jProp in jProps)
                {
                    // Whether specified Annoatation exist in response.
                    if (jProp.Name.Equals(specifiedAnnoatation))
                    {
                        isExist = true;
                        break;
                    }
                    else
                    {
                        isExist = false;
                    }
                }
            }

            return isExist;
        }

        /// <summary>
        /// Get all values of specified property from JSON object.
        /// </summary>
        /// <param name="jo">The JSON object instance.</param>
        /// <param name="identity">The identity of property name.</param>
        /// <param name="compareType">The compare method of identity.</param>
        /// <returns>The string list of got values.</returns>
        public static List<string> GetPropValuesFromJSONObject(JObject jo, string identity, StringCompareType compareType)
        {
            List<string> results = new List<string>();
            bool isExist = false;

            foreach (var p in jo)
            {
                isExist = false;

                if (p.Value.Type == JTokenType.Object)
                {
                    foreach (string link in GetPropValuesFromJSONObject((JObject)p.Value, identity, compareType))
                    {
                        results.Add(link);
                    }
                }
                else if (p.Value.Type == JTokenType.Array)
                {
                    foreach (string link in GetPropValuesFromJSONArray((JArray)p.Value, identity, compareType))
                    {
                        results.Add(link);
                    }
                }
                else
                {
                    switch (compareType)
                    {
                        case StringCompareType.Equal:
                            isExist = p.Key.ToString().Equals(identity);
                            break;
                        case StringCompareType.Contain:
                            isExist = p.Key.ToString().Contains(identity);
                            break;
                        default:
                            break;
                    }

                    if (isExist)
                    {
                        results.Add(p.Value.ToString().StripOffDoubleQuotes());
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Get all values of specified property from JSON array.
        /// </summary>
        /// <param name="jo">The JSON array instance.</param>
        /// <param name="identity">The identity of property name.</param>
        /// <param name="compareType">The compare method of identity.</param>
        /// <returns>The string list of got values.</returns>
        public static List<string> GetPropValuesFromJSONArray(JArray ja, string identity, StringCompareType compareType)
        {
            List<string> results = new List<string>();

            foreach (var p in ja)
            {
                if (p.Type == JTokenType.Object)
                {
                    foreach (string s in GetPropValuesFromJSONObject((JObject)p, identity, compareType))
                    {
                        results.Add(s);
                    }
                }
                else if (p.Type == JTokenType.Array)
                {
                    foreach (string s in GetPropValuesFromJSONArray((JArray)p, identity, compareType))
                    {
                        results.Add(s);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Gets all the json objects from the current response payload.
        /// </summary>
        /// <param name="jObj">The response payload.</param>
        /// <param name="objects">Stores all the json objects which were got from response payload.</param>
        public static void GetJsonObjectsFromRespPayload(JObject jObj, ref List<JToken> objects)
        {
            if (jObj != null && objects != null)
            {
                objects.Add(jObj);
                var jProps = jObj.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (jProp.Value.Type == JTokenType.Object)
                    {
                        GetJsonObjectsFromRespPayload((JObject)jProp.Value, ref objects);
                    }
                    else if (jProp.Value.Type == JTokenType.Array)
                    {
                        var objs = jProp.Value.Children();

                        foreach (var obj in objs)
                        {
                            if (typeof(JObject) == obj.GetType())
                            {
                                JObject jo = obj as JObject;
                                GetJsonObjectsFromRespPayload(jo, ref objects);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets all the annotations from the response payload.
        /// </summary>
        /// <param name="jObj">The response payload.</param>
        /// <param name="version">The version of the odata service.</param>
        /// <param name="type">The annotation type.</param>
        /// <param name="annotations">Stores all the annotation which were got from the response payload.</param>
        public static void GetAnnotationsFromResponsePayload(JObject jObj, ODataVersion version, AnnotationType type, ref List<JProperty> annotations)
        {
            if (jObj != null && annotations != null)
            {
                var jProps = jObj.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (jProp.Value.Type == JTokenType.Object)
                    {
                        GetAnnotationsFromResponsePayload((JObject)jProp.Value, version, type, ref annotations);
                    }
                    else if (jProp.Value.Type == JTokenType.Array)
                    {
                        var objs = jProp.Value.Children();

                        foreach (var obj in objs)
                        {
                            if (typeof(JObject) == obj.GetType())
                            {
                                GetAnnotationsFromResponsePayload((JObject)obj, version, type, ref annotations);
                            }
                        }
                    }
                    else
                    {
                        // If the property's name contains dot(.), it will indicate that this property is an annotation.
                        if (jProp.Name.Contains("."))
                        {
                            switch (type)
                            {
                                default:
                                case AnnotationType.All:
                                    annotations.Add(jProp);
                                    break;
                                case AnnotationType.ArrayOrPrimitive:
                                    if (version == ODataVersion.V4 ? !jProp.Name.StartsWith("@") : jProp.Name.Contains("@"))
                                    {
                                        annotations.Add(jProp);
                                    }
                                    break;
                                case AnnotationType.Object:
                                    if (version == ODataVersion.V4 ? jProp.Name.StartsWith("@") : !jProp.Name.Contains("@"))
                                    {
                                        annotations.Add(jProp);
                                    }
                                    break;
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get specified property's value from an entry payload.
        /// </summary>
        /// <param name="entry">An entry.</param>
        /// <param name="elementName">An element name which is expected to get.</param>
        /// <returns>Returns a list of values.</returns>
        public static List<JToken> GetSpecifiedPropertyValsFromEntryPayload(JObject entry, string elementName, MatchType matchType)
        {
            if (entry == null || elementName == null || elementName == string.Empty || matchType == MatchType.None)
            {
                return vals;
            }

            if (entry != null && entry.Type == JTokenType.Object)
            {
                var jProps = entry.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (matchType == MatchType.Equal && jProp.Name == elementName)
                    {
                        vals.Add(jProp.Value);
                    }
                    else if (matchType == MatchType.Contained && jProp.Name.Contains(elementName))
                    {
                        vals.Add(jProp.Value);
                    }

                    if (jProp.Value.Type == JTokenType.Object)
                    {
                        GetSpecifiedPropertyValsFromEntryPayload(jProp.Value as JObject, elementName, matchType);
                    }

                    if (jProp.Value.Type == JTokenType.Array && ((JArray)(jProp.Value)).Count != 0)
                    {
                        var collection = jProp.Value.Children();
                        int childrenNum = collection.Count();
                        var element = collection.First();

                        while (--childrenNum >= 0)
                        {
                            if (element.Type == JTokenType.Object)
                            {
                                GetSpecifiedPropertyValsFromEntryPayload(element as JObject, elementName, matchType);
                            }

                            if (childrenNum > 0)
                            {
                                element = element.Next;
                            }
                        }
                    }
                }
            }

            return vals;
        }

        /// <summary>
        /// Get specified property's name from an entry payload.
        /// </summary>
        /// <param name="entry">An entry.</param>
        /// <param name="elementName">An element name which is expected to get.</param>
        /// <returns>Returns a list of names.</returns>
        public static List<string> GetSpecifiedPropertyNamesFromEntryPayload(JObject entry, string elementName, MatchType matchType)
        {
            if (entry == null || elementName == null || elementName == string.Empty || matchType == MatchType.None)
            {
                return names;
            }

            if (entry != null && entry.Type == JTokenType.Object)
            {
                var jProps = entry.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (matchType == MatchType.Equal && jProp.Name == elementName)
                    {
                        names.Add(jProp.Name);
                    }
                    else if (matchType == MatchType.Contained && jProp.Name.Contains(elementName))
                    {
                        names.Add(jProp.Name);
                    }

                    if (jProp.Value.Type == JTokenType.Object)
                    {
                        GetSpecifiedPropertyValsFromEntryPayload(jProp.Value as JObject, elementName, matchType);
                    }

                    if (jProp.Value.Type == JTokenType.Array && ((JArray)(jProp.Value)).Count != 0)
                    {
                        var collection = jProp.Value.Children();
                        int childrenNum = collection.Count();
                        var element = collection.First();

                        while (--childrenNum >= 0)
                        {
                            if (element.Type == JTokenType.Object)
                            {
                                GetSpecifiedPropertyValsFromEntryPayload(element as JObject, elementName, matchType);
                            }

                            if (childrenNum > 0)
                            {
                                element = element.Next;
                            }
                        }
                    }
                }
            }

            return names;
        }

        /// <summary>
        /// Get specified properties from an entry payload.
        /// </summary>
        /// <param name="entry">An entry.</param>
        /// <param name="elementName">An element name which is expected to get.</param>
        /// <returns>Returns a list of properties.</returns>
        public static List<JProperty> GetSpecifiedPropertiesFromEntryPayload(JObject entry, string elementName)
        {
            if (entry == null || elementName == null || elementName == string.Empty)
            {
                return props;
            }

            if (entry != null && entry.Type == JTokenType.Object)
            {
                var jProps = entry.Children();

                foreach (JProperty jProp in jProps)
                {
                    if (jProp.Name == elementName)
                    {
                        props.Add(jProp);
                    }

                    if (jProp.Value.Type == JTokenType.Object)
                    {
                        GetSpecifiedPropertiesFromEntryPayload(jProp.Value as JObject, elementName);
                    }

                    if (jProp.Value.Type == JTokenType.Array && ((JArray)(jProp.Value)).Count != 0)
                    {
                        var collection = jProp.Value.Children();
                        int childrenNum = collection.Count();
                        var element = collection.First();

                        while (--childrenNum >= 0)
                        {
                            if (element.Type == JTokenType.Object)
                            {
                                GetSpecifiedPropertiesFromEntryPayload(element as JObject, elementName);
                            }

                            if (childrenNum > 0)
                            {
                                element = element.Next;
                            }
                        }
                    }
                }
            }

            return props;
        }

        /// <summary>
        /// Get specified properties from an feed payload.
        /// </summary>
        /// <param name="feed">A feed.</param>
        /// <param name="elementName">An element name which is expected to get.</param>
        /// <returns>Returns a list of properties.</returns>
        public static List<JProperty> GetSpecifiedPropertiesFromFeedPayload(JArray feed, string elementName)
        {
            if (feed == null || elementName == null || elementName == string.Empty)
            {
                return props;
            }

            if (feed != null && feed.Type == JTokenType.Array)
            {
                foreach (JObject entry in feed.Children())
                {
                    props = GetSpecifiedPropertiesFromEntryPayload(entry, elementName);
                }
            }

            return props;
        }

        /// <summary>
        /// Whether the json property is function or action property.
        /// </summary>
        /// <param name="FucOrActNames">The function or action names from metadata</param>
        /// <param name="jProp">The json property</param>
        /// <param name="metadataSchema">The metadata Schema XML element.</param>
        /// <returns>true if function or action property; false otherwise</returns>
        public static bool isFunctionOrActionProperty(List<string> FucOrActNames, JProperty jProp, XElement metadataSchema)
        {
            bool isImportName = false;

            AliasNamespacePair aliasNamespacePair = MetadataHelper.GetAliasAndNamespace(metadataSchema);

            foreach (string name in FucOrActNames)
            {
                if (!string.IsNullOrEmpty(name) &&
                    (jProp.Name.Equals("#" + aliasNamespacePair.Alias + "." + name)
                    || jProp.Name.Equals("#" + aliasNamespacePair.Namespace + "." + name)))
                {
                    isImportName = true;
                    break;
                }
            }

            return isImportName;
        }

        /// <summary>
        /// Get total count of entities in feed response.
        /// </summary>
        /// <param name="url">The input url.</param>
        /// <param name="feed">The json object of feed response.</param>
        /// <param name="version">The version of the service.</param>
        /// <param name="RequestHeaders">The request headers.</param>
        /// <param name="totalCount">The amount of the entities.</param>
        public static void GetEntitiesCountFromFeed(Uri url, JObject feed, IEnumerable<KeyValuePair<string, string>> RequestHeaders, ref int totalCount)
        {
            int skiptoken = 0;

            foreach (var r in feed.Children<JProperty>())
            {
                if (r.Name.Equals(Constants.Value, StringComparison.Ordinal) && r.Value.Type == JTokenType.Array)
                {
                    totalCount += ((JArray)r.Value).Count;
                }

                // When entities are more than one page.
                if (r.Name.Equals(Constants.V4OdataNextLink, StringComparison.Ordinal))
                {
                    string[] skiptokenValues = r.Value.ToString().StripOffDoubleQuotes().Split(new string[] { "skiptoken=" }, StringSplitOptions.None);
                    skiptoken = Int32.Parse(skiptokenValues[1]);
                    string nextLinkUrl = !url.AbsoluteUri.Contains("?$") ? url + @"?$skiptoken=" + skiptoken.ToString() : url + @"&$skiptoken=" + skiptoken.ToString();
                    Response response = WebHelper.Get(new Uri(nextLinkUrl), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, RequestHeaders);

                    JObject jo;
                    response.ResponsePayload.TryToJObject(out jo);

                    GetEntitiesCountFromFeed(url, jo, RequestHeaders, ref totalCount);
                }
            }
        }

        /// <summary>
        /// Gets the amount of entities in a navigation property with collection type.
        /// </summary>
        /// <param name="url">An entry input URL.</param>
        /// <param name="entry">The entry JSON response payload.</param>
        /// <param name="navigPropName">The name of navigation property with collection type.</param>
        /// <param name="requestHeaders">The request header.</param>
        /// <param name="totalCount">The actual amount of the entities in a navigation property with collection type.</param>
        public static void GetEntitiesNumFromCollectionValuedNavigProp(string url, JObject entry, string navigPropName, IEnumerable<KeyValuePair<string, string>> requestHeaders, ref int totalCount)
        {
            if (string.IsNullOrEmpty(url) || entry == null || string.IsNullOrEmpty(navigPropName) || requestHeaders == null)
            {
                return;
            }

            foreach (var jProp in entry.Children<JProperty>())
            {
                if (jProp.Name.Equals(navigPropName, StringComparison.Ordinal) && jProp.Value.Type == JTokenType.Array)
                {
                    totalCount += ((JArray)jProp.Value).Count;
                }

                if (jProp.Name.Equals(navigPropName + Constants.V4OdataNextLink, StringComparison.Ordinal))
                {
                    string[] nextLinkInfo = jProp.Value.ToString().StripOffDoubleQuotes().Split(new string[] { "skiptoken=" }, StringSplitOptions.None);
                    string nextLinkUrl = string.Format("{0}/{1}?$skiptoken={2}", url, navigPropName, nextLinkInfo[1]);
                    Response resp = WebHelper.Get(new Uri(nextLinkUrl), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, requestHeaders);
                    JObject jObj;
                    resp.ResponsePayload.TryToJObject(out jObj);
                    GetEntitiesCountFromFeed(new Uri(string.Format("{0}/{1}", url, navigPropName)), jObj, requestHeaders, ref totalCount);
                }
            }
        }

        /// <summary>
        /// Gets the error message from response payload.
        /// </summary>
        /// <param name="responsePayload">The content of response payload.</param>
        /// <returns>Returns the error message.</returns>
        public static string GetErrorMessage(this string responsePayload)
        {
            if (string.IsNullOrEmpty(responsePayload))
            {
                return string.Empty;
            }

            string result = string.Empty;

            if (responsePayload.IsXmlPayload())
            {
                try
                {
                    string xpath = @"./*[local-name()='message']";
                    XElement errorMsg = XElement.Parse(responsePayload);

                    result = errorMsg.XPathSelectElement(xpath, ODataNamespaceManager.Instance).Value;
                }
                catch (FormatException e)
                {
                    throw new FormatException("This XML format of error message is unknown.", e.InnerException);
                }

            }
            else if (responsePayload.IsJsonPayload())
            {
                var payload = JToken.Parse(responsePayload);

                try
                {
                    var errorBody = ((JObject)payload)["error"];
                    var message = ((JObject)errorBody)["message"];
                    result = message.ToString();
                }
                catch (FormatException e)
                {
                    throw new FormatException("This JSON format of error message is unknown.", e.InnerException);
                }
            }
            else
            {
                throw new FormatException("The response payload is unknown.");
            }

            return result;
        }

        /// <summary>
        /// Get specified count of entities from first feed of service document.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>Returns an entity URLs's list.</returns>
        public static bool GetBatchSupportedEntityUrls(out KeyValuePair<string, IEnumerable<string>> entityUrls)
        {
            entityUrls = new KeyValuePair<string, IEnumerable<string>>();
            var svcStatus = ServiceStatus.GetInstance();
            List<string> vocDocs = new List<string>() { TermDocuments.GetInstance().VocCapabilitiesDoc };
            var payloadFormat = svcStatus.ServiceDocument.GetFormatFromPayload();
            var batchSupportedEntitySetUrls = new List<string>();
            var batchSupportedEntitySetNames = AnnotationsHelper.SelectEntitySetSupportBatch(svcStatus.MetadataDocument, vocDocs);
            batchSupportedEntitySetNames.ForEach(temp => { batchSupportedEntitySetUrls.Add(temp.MapEntitySetNameToEntitySetURL()); });
            var entitySetUrls = ContextHelper.GetFeeds(svcStatus.ServiceDocument, payloadFormat).ToArray();

            if (entitySetUrls.Any())
            {
                string entitySetUrl = string.Empty;
                foreach (var setUrl in entitySetUrls)
                {
                    string entityTypeShortName = setUrl.MapEntitySetURLToEntityTypeShortName();
                    if (batchSupportedEntitySetUrls.Contains(setUrl) && !entityTypeShortName.IsMediaType())
                    {
                        entitySetUrl = setUrl;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(entitySetUrl))
                {
                    return false;
                }

                string url = string.Format("{0}/{1}?$top=1", svcStatus.RootURL, entitySetUrl);
                Response response = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, svcStatus.DefaultHeaders);
                payloadFormat = response.ResponsePayload.GetFormatFromPayload();
                var payloadType = ContextHelper.GetPayloadType(response.ResponsePayload, payloadFormat, response.ResponseHeaders);
                if (payloadType == RuleEngine.PayloadType.Feed)
                {
                    entityUrls = new KeyValuePair<string, IEnumerable<string>>(entitySetUrl, ContextHelper.GetEntries(response.ResponsePayload, payloadFormat));

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the amount of entities from a feed.
        /// </summary>
        /// <param name="entitySetUrl">A feed URL.</param>
        /// <returns>Return the amount of entities in current feed.</returns>
        public static int GetEntitiesCountFromFeed(string entitySetUrl, IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            int amount = 0;
            if (!Uri.IsWellFormedUriString(entitySetUrl, UriKind.Absolute))
            {
                throw new UriFormatException("The input parameter 'entitySetUrl' is not a URL string.");
            }

            var resp = WebHelper.Get(new Uri(entitySetUrl), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, headers);
            if (null != resp && HttpStatusCode.OK == resp.StatusCode)
            {
                var jObj = JObject.Parse(resp.ResponsePayload);
                var jArr = jObj.GetValue(Constants.Value) as JArray;
                amount += jArr.Count;
                while (null != jObj[Constants.V4OdataNextLink])
                {
                    string url = Uri.IsWellFormedUriString(jObj[Constants.V4OdataNextLink].ToString(), UriKind.Absolute) ?
                        jObj[Constants.V4OdataNextLink].ToString() :
                        ServiceStatus.GetInstance().RootURL.TrimEnd('/') + "/" + jObj[Constants.V4OdataNextLink].ToString();
                    resp = WebHelper.Get(new Uri(url), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, headers);
                    jObj = JObject.Parse(resp.ResponsePayload);
                    jArr = jObj.GetValue(Constants.Value) as JArray;
                    amount += jArr.Count;
                }
            }

            return amount;
        }

        /// <summary>
        /// Clear the property values list.
        /// </summary>
        public static void ClearPropertyNamesList()
        {
            names = new List<string>();
        }

        /// <summary>
        /// Clear the property values list.
        /// </summary>
        public static void ClearProperyValsList()
        {
            vals = new List<JToken>();
        }

        /// <summary>
        /// Clear the properties list.
        /// </summary>
        public static void ClearPropertiesList()
        {
            props = new List<JProperty>();
        }

        /// <summary>
        /// Store the name which is got from entry's properties.
        /// </summary>
        private static List<string> names = new List<string>();

        /// <summary>
        /// Store the value which is got from entry's properties.
        /// </summary>
        private static List<JToken> vals = new List<JToken>();

        /// <summary>
        /// Store the properties which is got from entry.
        /// </summary>
        private static List<JProperty> props = new List<JProperty>();

    }
}
