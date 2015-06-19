// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    #endregion

    /// <summary>
    /// Helper class of Json payload manipulation 
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Extension method to convert a string to JSON JObject object
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <param name="jo">output parameter of the JObject object converted from payload literal</param>
        /// <returns>true if payload is converted to a JObject; otherwise false</returns>
        public static bool TryToJObject(this string payload, out JObject jo)
        {
            if (!string.IsNullOrEmpty(payload))
            {
                try
                {
                    jo = JObject.Parse(payload);
                    return true;
                }
                catch (Exception ex)
                {
                    if (!ExceptionHelper.IsCatchableExceptionType(ex))
                    {
                        throw;
                    }
                    // it's alright if JSON parsing complaints
                }
            }

            jo = null;
            return false;
        }

        /// <summary>
        /// Extension method to determine whether content is a valid Json literral
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonPayload(this string payload)
        {
            JObject jo;
            return payload.TryToJObject(out jo);
        }

        /// <summary>
        /// Extension method to determine a content to be a Json verbose feed literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonVerboseFeed(this string payload)
        {
            JObject jo;
            if (payload.TryToJObject(out jo) && payload.IsJsonVerbosePayload())
            {
                var inner = jo.ReachInnerToken();
                return (inner != null && inner.Type == JTokenType.Array);
            }

            return false;
        }

        /// <summary>
        /// Extension method to determine a content to be a Json light feed literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonLightFeed(this string payload, string metadata = null)
        {
            JObject jo;
            string metadataValue = null;
            bool result = false;
            bool isValueArrayRight = false;

            if (payload.TryToJObject(out jo) && !payload.IsJsonVerbosePayload())
            {
                if (jo.Type == JTokenType.Object)
                {
                    var o = (JObject)jo;
                    var et = (JProperty)o.First;

                    if (et.Name.Equals(Constants.OdataV3JsonIdentity) || et.Name.Equals(Constants.OdataV4JsonIdentity))
                    {
                        metadataValue = et.Value.ToString().StripOffDoubleQuotes();
                        string[] metadataValueSegments = metadataValue.Split('/');
                        if (metadataValueSegments != null)
                        {
                            int length = metadataValueSegments.Length;
                            if (length != 0)
                            {                                
                                if (et.Name.Equals(Constants.OdataV3JsonIdentity))
                                {
                                    // Whether last segment start with "$metadata#" and does NOT contains '.'.
                                    result = metadataValueSegments[length - 1].StartsWith(Constants.JsonFeedIdentity)
                                        && !metadataValueSegments[length - 1].Contains(".");
                                }
                                else if (et.Name.Equals(Constants.OdataV4JsonIdentity))
                                {
                                    if (string.IsNullOrEmpty(metadata))
                                    {
                                        // Whether last segment start with "$metadata#" and does NOT start with "$metadata#ref" and does NOT contains '.'.
                                        if (!payload.IsJsonLightEntityRef() && !payload.IsJsonLightPrimitiveAndComplexType() && !payload.IsJsonLightDeltaResponse())
                                        {
                                            if (metadataValueSegments[length - 1].StartsWith(Constants.JsonFeedIdentity))
                                            {
                                                result = true;
                                            }
                                            else
                                            {
                                                // For example: http://host/service/$metadata#Orders(4711)/Items, Items should be navigation property.
                                                if (metadataValue.Contains(Constants.JsonFeedIdentity))
                                                {
                                                    string[] feedSegment = metadataValue.Substring(metadataValue.IndexOf(Constants.JsonFeedIdentity)).Split('/');

                                                    if (feedSegment.Length > 1)
                                                    {
                                                        result = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (metadataValue.Contains(Constants.JsonFeedIdentity))
                                        {
                                            List<string> entitySetNames = XmlHelper.GetEntitySetNames(metadata);
                                            string entitySetSegment = metadataValue.Remove(0, metadataValue.IndexOf(Constants.JsonFeedIdentity) + Constants.JsonFeedIdentity.Length);

                                            if (entitySetNames.Contains(entitySetSegment))
                                            {
                                                result = true;
                                            }
                                            else if (entitySetSegment.Contains('/'))
                                            {
                                                string entitySet = entitySetSegment.Split('/')[0].Split('(')[0];

                                                if (entitySetNames.Contains(entitySet))
                                                {
                                                    string propertyName = entitySetSegment.Split('/')[1];
                                                    List<string> navigationProperties = XmlHelper.GetNavigationProperties(entitySet, metadata);

                                                    if (navigationProperties.Contains(propertyName))
                                                    {
                                                        result = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (jo[Constants.Value] != null && jo[Constants.Value].Type == JTokenType.Array)
                    {
                        if (((JArray)jo[Constants.Value]).Count == 0)
                        {
                            isValueArrayRight = true;
                        }
                        else
                        {
                            if (jo[Constants.Value].First.Type == JTokenType.Object)
                            {
                                isValueArrayRight = true;
                            }
                        }
                    }
                }
            }

            return result && isValueArrayRight;
        }

        /// <summary>
        /// Extension method to determine a content to be a Json light Individual Property literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonLightIndividualProperty(this string payload, string metadata = null)
        {
            JObject jo;
            string metadataValue = null;
            bool result = false;
            if (payload.TryToJObject(out jo) && !payload.IsJsonVerbosePayload())
            {
                if (jo.Type == JTokenType.Object)
                {
                    var o = (JObject)jo;
                    var et = (JProperty)o.First;

                    if (et.Name.Equals(Constants.OdataV3JsonIdentity) || et.Name.Equals(Constants.OdataV4JsonIdentity))
                    {
                        metadataValue = et.Value.ToString().StripOffDoubleQuotes();

                        if (metadataValue.Contains(Constants.JsonFeedIdentity))
                        {
                            string[] feedSegment = metadataValue.Remove(0, metadataValue.IndexOf(Constants.JsonFeedIdentity) + Constants.JsonFeedIdentity.Length).Split('/');

                            if (feedSegment.Length > 1)
                            {
                                if (string.IsNullOrEmpty(metadata))
                                {
                                    if (feedSegment[0].Contains('(') && feedSegment[0].Contains(')'))
                                    {
                                        result = true;
                                    }
                                }
                                else
                                {
                                    string entitySet = feedSegment[0].Split('(')[0];
                                    List<string> entitySetNames = XmlHelper.GetEntitySetNames(metadata);

                                    if (entitySetNames.Contains(entitySet))
                                    {
                                        string propertyName = feedSegment[1];
                                        List<string> individualProperties = XmlHelper.GetNormalProperties(entitySet, metadata);

                                        if (individualProperties.Contains(propertyName))
                                        {
                                            result = true;
                                        }
                                    }
                                }
                            }
                        }                        
                    }
                }
            }

            return result;
        }

        public static bool IsJsonLightPrimitiveAndComplexType(this string payload)
        {
            JObject jo;
            string metadataValue = null;
            bool result = false;
            if (payload.TryToJObject(out jo) && !payload.IsJsonVerbosePayload())
            {
                if (jo.Type == JTokenType.Object)
                {
                    var o = (JObject)jo;
                    var et = (JProperty)o.First;

                    if (et.Name.Equals(Constants.OdataV3JsonIdentity) || et.Name.Equals(Constants.OdataV4JsonIdentity))
                    {
                        metadataValue = et.Value.ToString().StripOffDoubleQuotes();
                        string[] metadataValueSegments = metadataValue.Split('/');
                        if (metadataValueSegments != null)
                        {
                            int length = metadataValueSegments.Length;
                            if (length != 0)
                            {
                                result = metadataValueSegments[length - 1].StartsWith(Constants.JsonFeedIdentity)
                                        && metadataValueSegments[length - 1].Contains(".");
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extension method to determine a content to be a Json light delta response literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonLightDeltaResponse(this string payload)
        {
            JObject jo;
            string metadataValue = null;
            if (payload.TryToJObject(out jo) && !payload.IsJsonVerbosePayload())
            {
                if (jo.Type == JTokenType.Object)
                {
                    var o = (JObject)jo;
                    var et = (JProperty)o.First;
                  
                    if (et.Name.Equals(Constants.OdataV3JsonIdentity))
                    {
                        metadataValue = et.Value.ToString().StripOffDoubleQuotes();
                        return metadataValue.EndsWith(Constants.V3JsonDeltaResponseIdentity);                           
                    }
                    else if (et.Name.Equals(Constants.OdataV4JsonIdentity))
                    {
                        metadataValue = et.Value.ToString().StripOffDoubleQuotes();
                        return metadataValue.EndsWith(Constants.V4JsonDeltaResponseIdentity);
                    }
                }
            }

            return false;
        }       

        /// <summary>
        /// Extension method to determine a content to be a Json light entity reference literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonLightEntityRef(this string payload)
        {
            JObject jo;
            string metadataValue = null;
            if (payload.TryToJObject(out jo) && !payload.IsJsonVerbosePayload())
            {
               if (jo.Type == JTokenType.Object)
                {
                    var o = (JObject)jo;
                    var et = (JProperty)o.First;

                    if (et.Name.Equals(Constants.OdataV4JsonIdentity))
                    {
                        metadataValue = et.Value.ToString().StripOffDoubleQuotes();
                        return metadataValue.EndsWith(Constants.V4JsonCollectionEntityRefIdentity)
                            || metadataValue.EndsWith(Constants.V4JsonEntityRefIdentity);                           
                    }
                    else if (et.Name.Equals(Constants.OdataV3JsonIdentity))
                    {
                        metadataValue = et.Value.ToString().StripOffDoubleQuotes();
                        string[] metadataValueSegments = metadataValue.Split('/');
                        if (metadataValueSegments != null && metadataValueSegments.Length != 0)
                        {
                            if (metadataValueSegments[metadataValueSegments.Length - 2].Equals(@"$links")
                                && metadataValueSegments[metadataValueSegments.Length - 3].StartsWith(@"$metadata#"))
                            {
                                return true;
                            }
                        }                                            
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Extension method to determine whether a content is a Json verbose entry literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonVerboseEntry(this string payload)
        {
            JObject jo;
            if (payload.TryToJObject(out jo) && payload.IsJsonVerbosePayload())
            {
                var inner = jo.ReachInnerToken();
                if (inner != null && inner.Type == JTokenType.Object)
                {
                    return ((JObject)inner).Count > 1;
                }
            }

            return false;
        }

        /// <summary>
        /// Extension method to determine whether a content is a Json light entry literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonLightEntry(this string payload, string metadata = null)
        {
            JObject jo;
            string metadataValue = null;
            bool isQueriedEntry = false;

            if (payload.TryToJObject(out jo) && !payload.IsJsonVerbosePayload())
            {
                if (jo.Type == JTokenType.Object)
                {
                    var o = (JObject)jo;
                    var et = (JProperty)o.First;

                    if (et.Name.Equals(Constants.OdataV3JsonIdentity))
                    {
                        metadataValue = et.Value.ToString().StripOffDoubleQuotes();
                        string[] metadataValueSegments = metadataValue.Split('/');
                        if (metadataValueSegments != null && metadataValueSegments.Length != 0)
                        {
                            int length = metadataValueSegments.Length;

                            // Whether last segment start with "@Element"
                            isQueriedEntry = metadataValueSegments[length - 1].StartsWith(Constants.V3JsonEntityIdentity);
                        }

                        return metadataValue.EndsWith(Constants.V3JsonEntityIdentity) || isQueriedEntry;
                    }
                    else if (et.Name.Equals(Constants.OdataV4JsonIdentity))
                    {
                        metadataValue = et.Value.ToString().StripOffDoubleQuotes();
                        
                        if (metadataValue.EndsWith(Constants.V4JsonEntityIdentity))
                        {
                            // {context-url}#{entity-set}/$entity
                            return true;
                        }
                        else
                        {
                            string[] metadataValueSegments = metadataValue.Split('/');
                            if (metadataValueSegments != null)
                            {
                                int length = metadataValueSegments.Length;

                                if (metadataValueSegments[length - 1].StartsWith(Constants.JsonFeedIdentity))
                                {
                                    if (string.IsNullOrEmpty(metadata))
                                    {
                                        // singleton
                                        return !metadataValueSegments[length - 1].EndsWith(Constants.V4JsonCollectionEntityRefIdentity)
                                              && !metadataValueSegments[length - 1].EndsWith(Constants.V4JsonEntityRefIdentity)
                                              && !metadataValueSegments[length - 1].Contains(".")
                                              && jo[Constants.Value] == null;
                                    }
                                    else
                                    {
                                        string entitySet = metadataValueSegments[length - 1].Remove(0, Constants.JsonFeedIdentity.Length);
                                        List<string> singletonNames = XmlHelper.GetSingletonNames(metadata);

                                        if (singletonNames.Contains(entitySet))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Extension method to determine whether a content is a Json verbose service document literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonVerboseSvcDoc(this string payload)
        {
            JObject jo;
            if (payload.TryToJObject(out jo) && payload.IsJsonVerbosePayload())
            {
                var inner = jo.ReachInnerToken();
                if (inner != null && inner.Type == JTokenType.Object)
                {
                    var o = (JObject)inner;
                    var et = (JProperty)o.First;
                    return o.Count == 1 && et.Name.Equals(Constants.EntitySets, StringComparison.Ordinal);
                }
            }

            return false;
        }

        /// <summary>
        /// Extension method to determine whether a content is a Json light service document literal
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>true/false</returns>
        public static bool IsJsonLightSvcDoc(this string payload)
        {
            JObject jo;
            if (payload.TryToJObject(out jo) && !payload.IsJsonVerbosePayload())
            {
                if (jo.Type == JTokenType.Object)
                {
                    var o = (JObject)jo;
                    var et = (JProperty)o.First;

                    if (et.Name.Equals(Constants.OdataV3JsonIdentity) || et.Name.Equals(Constants.OdataV4JsonIdentity))
                    {
                        string metadataValue = et.Value.ToString().StripOffDoubleQuotes();
                        return metadataValue.EndsWith(Constants.JsonSvcDocIdentity);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Extension method to determine whether a content is OData error literal in Json verbose format
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>True if it is OData error literal in Json format; false otherwise</returns>
        public static bool IsJsonVerboseError(this string payload)
        {
            bool payloadIsError = false;

            JObject jo;
            if (payload.TryToJObject(out jo))
            {
                if (jo != null && jo.Type == JTokenType.Object)
                {
                    var o = (JObject)jo;
                    var et = (JProperty)o.First;
                    if (o.Count == 1 && et.Name.Equals(Constants.V3JsonVerboseErrorResponseIdentity, StringComparison.Ordinal))
                    {
                        var sub = et.Value;

                        // get the value of "error"
                        if (sub.Type == JTokenType.Object)
                        {
                            var err = (JObject)sub;
                           
                            // find "message:..."
                            foreach (var r in err.Children<JProperty>())
                            {
                                if (r.Name.Equals(Constants.MessageNameInJsonErrorResponse, StringComparison.Ordinal))
                                {
                                    if (r.Value.Type == JTokenType.Object)
                                    {
                                        payloadIsError = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return payloadIsError;
        }

        /// <summary>
        /// Extension method to determine whether a content is OData error literal in Json light format
        /// </summary>
        /// <param name="payload">payload content</param>
        /// <returns>True if it is OData error literal in Json format; false otherwise</returns>
        public static bool IsJsonLightError(this string payload)
        {
            bool payloadIsLightError = false;

            JObject jo;
            if (payload.TryToJObject(out jo))
            {
                if (jo != null && jo.Type == JTokenType.Object)
                {
                    var o = (JObject)jo;
                    var et = (JProperty)o.First;
                    if (o.Count == 1 
                        && (et.Name.Equals(Constants.V3JsonLightErrorResponseIdentity, StringComparison.Ordinal)
                            || et.Name.Equals(Constants.V4JsonLightErrorResponseIdentity, StringComparison.Ordinal)))
                    {
                        var sub = et.Value;

                        // get the value of "error"
                        if (sub.Type == JTokenType.Object)
                        {
                            var err = (JObject)sub;

                            // find "message:..."
                            foreach (var r in err.Children<JProperty>())
                            {
                                if (r.Name.Equals(Constants.MessageNameInJsonErrorResponse, StringComparison.Ordinal))
                                {
                                    if ((et.Name.Equals(Constants.V3JsonLightErrorResponseIdentity, StringComparison.Ordinal)&& r.Value.Type == JTokenType.Object)
                                        || (et.Name.Equals(Constants.V4JsonLightErrorResponseIdentity, StringComparison.Ordinal) && r.Value.Type == JTokenType.String))
                                    {
                                        payloadIsLightError = true;
                                    }
                                   
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return payloadIsLightError;
        }

        /// <summary>
        /// Checks whether payload is OData link message
        /// </summary>
        /// <param name="payload">The payload to be checked</param>
        /// <returns>True if it is OData link message; false otherwise</returns>
        public static bool IsJsonVerboseLink(this string payload)
        {
            bool result = false;

            JObject jo;
            if (payload.TryToJObject(out jo) && payload.IsJsonVerbosePayload())
            {
                if (jo != null && jo.Type == JTokenType.Object)
                {
                    var inner = jo.ReachInnerToken();
                    if (inner.Type == JTokenType.Array)
                    {
                        var e = ((JArray)inner).First();
                        if (e.Type == JTokenType.Object)
                        {
                            result = JsonObjectIsLink((JObject)e);
                        }
                    }
                    else if (inner.Type == JTokenType.Object)
                    {
                        result = JsonObjectIsLink((JObject)inner);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether a JObject instance is OData link message
        /// </summary>
        /// <param name="o">The JObject instance to be checked</param>
        /// <returns>True if it is OData link message; false otherwise</returns>
        private static bool JsonObjectIsLink(JObject o)
        {
            bool result = false;
            if (o.Count == 1)
            {
                var p = (JProperty)o.First;
                result = p.Name.Equals(Constants.JsonVerboseUriPropertyName, StringComparison.Ordinal);
            }
            return result;
        }

        /// <summary>
        /// Checks whether payload is OData property
        /// </summary>
        /// <param name="payload">The payload to be checked</param>
        /// <returns>True if it is OData property message</returns>
        public static bool IsJsonVerboseProperty(this string payload)
        {
            bool result = false;

            JObject jo;
            if (payload.TryToJObject(out jo) && payload.IsJsonVerbosePayload())
            {
                if (jo != null && jo.Type == JTokenType.Object)
                {
                    var inner = jo.ReachInnerToken();
                    if (inner.Type == JTokenType.Object)
                    {
                        result = true;
                    }
                    else if (inner.Type == JTokenType.Array)
                    {
                        result = !((JArray)inner).Any(x => x.Type != JTokenType.Object);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extension method to extract category type string value from a JObject object
        /// </summary>
        /// <param name="jo">The payload in json verbose format</param>
        /// <returns>The full string of type property value of __metadata node in payload</returns>
        public static string GetJsonVerboseEntityType(this JObject jo)
        {           
            return JsonHelper.GetPropertyOfChild(jo, Constants.JsonVerboseMetadataPropertyName, Constants.Type);           
        }

        /// <summary>
        /// Extension method to extract category type string value from a JObject object
        /// </summary>
        /// <param name="jo">The feed payload in json light format</param>
        /// <returns>The full string of type property value of odata.type node in payload</returns>
        public static string GetJsonLightEntityTypeFromFeed(this JObject jo)
        {
            string entityType = string.Empty;
            string contextValue = string.Empty;

            if (jo[Constants.Value] != null)
            {
                if (jo[Constants.Value].First != null && jo[Constants.Value].First[Constants.OdataType] != null)
                {
                    entityType = JsonHelper.GetPropertyOfChild(jo, Constants.Value, Constants.OdataType);
                }
                else if (jo[Constants.Value].First != null && jo[Constants.Value].First[Constants.V4OdataType] != null)
                {
                    entityType = JsonHelper.GetPropertyOfChild(jo, Constants.Value, Constants.V4OdataType);
                }
            }
            
            if (!string.IsNullOrEmpty(entityType))
            {
                entityType = entityType.TrimStart('#');
            }

            return entityType;            
        }

        /// <summary>
        /// Extension method to extract category type string value from a JObject object
        /// </summary>
        /// <param name="jo">The entry payload in json light format</param>
        /// <returns>The full string of type property value of odata.type node in payload</returns>
        public static string GetJsonLightEntityTypeFromEntry(this JObject jo)
        {
            string entityType = string.Empty;
            string contextValue = string.Empty;

            if (jo[Constants.OdataType] != null)
            {
                entityType = JsonHelper.GetChildPropertyValue(jo, Constants.OdataType);
            }
            else if (jo[Constants.V4OdataType] != null)
            {
                entityType = JsonHelper.GetChildPropertyValue(jo, Constants.V4OdataType);
            }           

            if (!string.IsNullOrEmpty(entityType))
            {
                entityType = entityType.TrimStart('#');
            }

            return entityType;
        }               

        /// <summary>
        /// Gets full path of entity set.
        /// </summary>
        /// <param name="jo">The payload in json format</param>
        /// <returns>The full path to entity set if one can be found; null otherwise</returns>
        public static string GetEntitySetFullPath(this JObject jo)
        {
            return JsonHelper.GetPropertyOfChild(jo, Constants.JsonVerboseMetadataPropertyName, Constants.JsonVerboseUriPropertyName);
        }

        /// <summary>
        /// Gets specify child property value.
        /// </summary>
        /// <param name="jo">The payload in json format</param>
        /// <returns>The specify child property value.</returns>
        public static string GetChildPropertyValue(this JObject jo, string childPropertyName)
        {
            string value = null;

            if (jo != null)
            {
                foreach (var r in jo.Children<JProperty>())
                {
                    if (r.Name.Equals(childPropertyName, StringComparison.Ordinal))
                    {
                        value = r.Value.ToString().StripOffDoubleQuotes();
                        break;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Gets property string value of a named child of the Json object.
        /// </summary>
        /// <param name="jo">The Json object</param>
        /// <param name="child">Name of the child object</param>
        /// <param name="property">Name of the property</param>
        /// <returns>Value as string literal of the property</returns>
        public static string GetPropertyOfChild(this JObject jo, string child, string property)
        {
            string value = null;

            if (jo != null)
            {
                var inner = jo.ReachInnerToken();
                if (inner != null)
                {
                    switch (inner.Type)
                    {
                        case JTokenType.Object:
                            value = ((JObject)inner).GetPropertyOfElement(child, property);
                            break;
                        case JTokenType.Array:
                            {
                                var array = (JArray)inner;
                                if (array.Count > 0)
                                {
                                    if (array[0] is JObject)
                                    {
                                        value = ((JObject)array[0]).GetPropertyOfElement(child, property);
                                    }
                                }
                            }

                            break;
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Extension method to indicate whether a payload Json object is about media link entry
        /// </summary>
        /// <param name="jo">Json object representing payload content</param>
        /// <returns>True for a media link entry; otherwise false</returns>
        public static bool IsMediaLinkEntry(this JObject jo)
        {
            if (jo == null)
            {
                throw new ArgumentNullException("jo");
            }

            var inner = jo.ReachInnerToken();
            if (inner != null && inner.Type == JTokenType.Object) 
            {
                JObject response = (JObject)inner;

                var ps = from p in response.Children<JProperty>()
                         where p.Name.Equals(Constants.JsonVerboseMetadataPropertyName, StringComparison.Ordinal)
                         select p;

                if (ps.Any())
                {
                    var meta = ps.First();
                    var et = from e in meta.Value.Children<JProperty>()
                             where e.Name.Equals(Constants.JsonVerboseContent_TypeProperty, StringComparison.Ordinal)
                             select e;
                    return et.Any();
                }
            }

            return false;
        }

        /// <summary>
        /// Extension method to indicate whether a payload Json object is verbose format. 
        /// </summary>
        /// <param name="jo">JObject which reprensents whole json payload</param>
        /// <returns>true:json verbose; false:json light</returns>
        public static bool IsJsonVerbosePayload(this string payload)
        {
            bool result = false;
            
            JObject jo;
            if (payload.TryToJObject(out jo))
            {
                if (jo.Count == 1)
                {
                    var d = (JProperty)jo.First;
                    if (d.Name.Equals(Constants.BeginMarkD, StringComparison.Ordinal))
                    {
                        result = true;
                    }
                    else if (payload.IsJsonVerboseError())
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }                 
                }
            }

            return result;
        }


        /// <summary>
        /// Extension method to properly format json payload
        /// </summary>
        /// <param name="jo">JObject object whose payload is to be formatted</param>
        /// <returns>well-formatted payload</returns>
        public static string FineFormat(this JObject jo)
        {
            string result = null; 

            if (jo != null)
            {
                result = jo.ToString();
            }

            return result;
        }

        /// <summary>
        /// Gets property value of named element (direct child) of Json object.
        /// </summary>
        /// <param name="jo">Json object</param>
        /// <param name="element">Name of element of Json object</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Literal string of the found property value of element if one can be found; null otherwise</returns>
        public static string GetPropertyOfElement(this JObject jo, string element, string propertyName)
        {
            string propertyValue = null;

            if (jo != null)
            {
                var items = from p in jo.Children<JProperty>()
                         where p.Name.Equals(element, StringComparison.Ordinal)
                         select p;

                var item = items.FirstOrDefault();
                if (item != null)
                {
                    var itemValue = item.Value;
                    if (itemValue != null)
                    {
                        if (itemValue.Type == JTokenType.Array && ((JArray)itemValue).Count > 0)
                        {
                            itemValue = itemValue.First();
                        }
                        var properties = from e in itemValue.Children<JProperty>()
                                 where e.Name.Equals(propertyName, StringComparison.Ordinal)
                                 select e;
                        if (properties.Count() > 0)
                        {
                            var typeVal = properties.First().Value;
                            if (typeVal.Type == JTokenType.String)
                            {
                                propertyValue = ((JValue)typeVal).Value.ToString();
                            }
                        }
                    }
                }
            }

            return propertyValue;
        }

        /// <summary>
        /// Gets projection of properties of the specified entity type from json verbose payload
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <param name="metadataDocument">The metadata document</param>
        /// <param name="shortEntityType">The short name of entity type</param>
        /// <returns>Collection of projected properties</returns>
        public static IEnumerable<string> GetProjectedPropertiesFromJsonVerboseFeedOrEntry(string payload, string metadataDocument, string shortEntityType)
        {
            IEnumerable<string> projectedProperties = null;

            JObject jo;
            if (payload.TryToJObject(out jo))
            {
                var inner = jo.ReachInnerToken();
                if (inner != null)
                {
                    JToken node = null;

                    if (inner.Type == JTokenType.Array)
                    {
                        // a feed; needs to go further to get one subelement
                        JArray array = (JArray)inner;
                        node = array.First;
                    }
                    else
                    {
                        node = inner;
                    }

                    if (node != null && node.Type == JTokenType.Object)
                    {
                        var ps = from JProperty n in node.Children<JProperty>()
                                 where !n.Name.Equals(Constants.JsonVerboseMetadataPropertyName)
                                 select n.Name;
                        if (ps.Any())
                        {
                            int count = ps.Count();
                            int propertiesInMetada = XmlHelper.GetCountOfProperties(shortEntityType, metadataDocument);
                            if (count < propertiesInMetada)
                            {
                                // navigation properties may be included in case of no-projection
                                projectedProperties = ps;
                            }
                        }
                    }
                }
            }

            return projectedProperties;
        }

        /// <summary>
        /// Gets projection of properties of the specified entity type from json light payload
        /// </summary>
        /// <param name="payload">The payload content</param>
        /// <param name="metadataDocument">The metadata document</param>
        /// <param name="shortEntityType">The short name of entity type</param>
        /// <returns>Collection of projected properties</returns>
        public static IEnumerable<string> GetProjectedPropertiesFromJsonLightFeedOrEntry(string payload, string metadataDocument, string shortEntityType)
        {
            IEnumerable<string> projectedProperties = null;

            JObject jo;
            if (payload.TryToJObject(out jo))
            {
                JToken node = null;
                if (payload.IsJsonLightEntry())
                {
                    node = jo;
                }
                else if (payload.IsJsonLightFeed())
                {
                    if (jo[Constants.Value] != null && ((JArray)jo[Constants.Value]).Count > 0)
                    {
                        node = ((JArray)jo[Constants.Value]).First;
                    }
                }

                if (node != null && node.Type == JTokenType.Object)
                {
                    var ps = from JProperty n in node.Children<JProperty>()
                             where !n.Name.StartsWith(Constants.OdataNS) && !n.Name.Contains("@" + Constants.OdataNS) && !n.Name.StartsWith(Constants.V4OdataNS)
                             select n.Name;

                    if (ps.Any())
                    {
                        int count = ps.Count();
                        int propertiesInMetada = XmlHelper.GetCountOfProperties(shortEntityType, metadataDocument);
                        if (count < propertiesInMetada)
                        {
                            // navigation properties may be included in case of no-projection
                            projectedProperties = ps;
                        }
                    }
                }
            }

            return projectedProperties;
        }

        /// <summary>
        /// Extension method to get the real Json payload object by stripping off wrapper of "d", and "results"(if it exists) 
        /// </summary>
        /// <param name="jo">JObject which reprensents whole json payload</param>
        /// <returns>JToken inside the trivial wrapper</returns>
        public static JToken ReachInnerToken(this JObject jo)
        {
            if (jo == null)
            {
                return null;
            }

            if (jo.Count == 1)
            {
                var d = (JProperty)jo.First;
                if (d.Name.Equals(Constants.BeginMarkD, StringComparison.Ordinal))
                {
                    var sub = d.Value;

                    // strip off v2.0 wrapper of "results"
                    if (sub.Type == JTokenType.Object)
                    {
                        var o = (JObject)sub;
                        if (o.Count == 1)
                        {
                            var r = (JProperty)o.First;
                            if (r.Name.Equals(Constants.Results, StringComparison.Ordinal))
                            {
                                sub = r.Value;
                            }
                        }
                        else if (o.Count > 1)
                        {
                            // may have "__next:..."
                            foreach (var r in o.Children<JProperty>())
                            {
                                if (r.Name.Equals(Constants.Results, StringComparison.Ordinal))
                                {
                                    sub = r.Value;
                                    break;
                                }
                            }
                        }
                    }

                    return sub;
                }
            }

            return jo;
        }

        /// <summary>
        /// Gets list of feeds from a service document in JSON format
        /// </summary>
        /// <param name="serviceDocument">The content of service document</param>
        /// <returns>The returned list of feeds exposed by the OData service</returns>
        public static IEnumerable<string> GetFeeds(string serviceDocument)
        {
            if (!serviceDocument.IsJsonVerboseSvcDoc() && !serviceDocument.IsJsonLightSvcDoc())
            {
                throw new ArgumentException(Resource.NotServiceDocument, "serviceDocument");
            }

            IEnumerable<string> feeds = null;
            JObject jo;

            serviceDocument.TryToJObject(out jo);

            if (serviceDocument.IsJsonVerboseSvcDoc())
            {
                var inner = jo.ReachInnerToken();
                if (inner != null && inner.Type == JTokenType.Object)
                {
                    var o = (JObject)inner;

                    // must be the EntitySets element
                    var et = (JProperty)o.First;    
                    if (et != null && et.Value != null)
                    {
                        if (et.Value.Type == JTokenType.Array)
                        {
                            feeds = from feed in (JArray)et.Value
                                    select feed.Value<string>();
                        }
                    }
                }
            }
            else
            {
                if (jo[Constants.Value] != null && jo[Constants.Value].Type == JTokenType.Array)
                {
                    feeds = from feed in (JArray)(jo[Constants.Value])
                            where (feed[Constants.Kind] != null && feed[Constants.Kind].Value<string>().Equals("EntitySet") 
                            || feed[Constants.Kind] == null)
                            select feed[Constants.Url].Value<string>();
                }
            }

            return feeds;
        }

        /// <summary>
        /// Gets collection of entries from the feed resource in JSON format
        /// </summary>
        /// <param name="feed">The feed resource content</param>
        /// <returns>The entries included in the feed content</returns>
        public static IEnumerable<string> GetEntries(string feed)
        {
            if (!feed.IsJsonVerboseFeed() && !feed.IsJsonLightFeed())
            {
                throw new ArgumentException(Resource.NotFeed, "feed");
            }

            IEnumerable<string> entries = null;
            JObject jo;
            feed.TryToJObject(out jo);

            string odataId = string.Empty;

            if (feed.IsJsonVerboseFeed())
            {
                var inner = jo.ReachInnerToken();
                if (inner != null)
                {
                    if (inner.Type == JTokenType.Array)
                    {
                        entries = from JObject a in (JArray)inner
                                  select a.GetPropertyOfElement(Constants.JsonVerboseMetadataPropertyName, Constants.JsonVerboseUriPropertyName);
                    }
                }
            }
            else
            {
                if (jo[Constants.Value] != null && jo[Constants.Value].Type == JTokenType.Array)
                {
                    if (jo[Constants.Value].First[Constants.OdataId] != null)
                    {
                        odataId = Constants.OdataId;
                    }
                    else if (jo[Constants.Value].First[Constants.V4OdataId] != null)
                    {
                        odataId = Constants.V4OdataId;
                    }

                    entries = from a in (JArray)jo[Constants.Value]
                              where a[odataId] != null
                              select a[odataId].Value<string>();
                }
            }

            return entries;
        }

        /// <summary>
        /// Get collection of links from the entry in JSON format
        /// </summary>
        /// <param name="entry">The entry resource content</param>
        /// <returns>The links included in the entry content</returns>
        public static IEnumerable<JProperty> GetLinks(string entry)
        {
            if (!entry.IsJsonVerboseEntry() && !entry.IsJsonLightEntry())
            {
                throw new ArgumentException("The payload is not an entry");
            }

            IEnumerable<JProperty> links = null;
            JObject jo;
            if (entry.TryToJObject(out jo))
            {
                var inner = jo.ReachInnerToken();
                if (inner != null)
                {
                    if (inner.Type == JTokenType.Object)
                    {
                        if (entry.IsJsonVerboseEntry())
                        {
                            links = from p in ((JObject)inner).Properties()
                                    where p.Value.Type == JTokenType.Object && ((JObject)p.Value).Property(Constants.JsonVerboseDeferredPropertyName) != null
                                    select p;
                        }
                        else
                        {
                            links = from p in ((JObject)inner).Properties()
                                    where p.Name.Contains(Constants.OdataNavigationLinkPropertyNameSuffix)
                                    select p;
                        }
                    }
                }
            }

            return links;
        }

        /// <summary>
        /// Compare the two JToken lists' sequence.
        /// </summary>
        /// <param name="jTokens1">The first JToken list.</param>
        /// <param name="jTokens2">The second JToken list.</param>
        /// <returns>Returns the result.</returns>
        public static bool SequenceEquals(this List<JToken> jTokens1, List<JToken> jTokens2)
        {
            if (jTokens1 == null || jTokens2 == null)
            {
                return false;
            }

            bool result = true;

            if (jTokens1.Count == jTokens2.Count)
            {
                for (int i = 0; i < jTokens1.Count; i++)
                {
                    if (!JToken.DeepEquals(jTokens1[i], jTokens2[i]))
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}
