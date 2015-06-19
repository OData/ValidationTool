// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Helper class to verify rules.
    /// </summary>
    public static class VerificationHelper
    {
        /// <summary>
        /// Verifies sorting entities order by ascending or descending.
        /// </summary>
        /// <param name="context">The Interop service context.</param>
        /// <param name="sortedType">The sorted type.</param>
        /// <param name="info">Out parameter indicates the extension rule violation information.</param>
        /// <returns>True: The sort validation pass; false: otherwise</returns>
        public static bool? VerifySortEntities(
            ServiceContext context,
            SortedType sortedType,
            out ExtensionRuleViolationInfo info)
        {
            List<JToken> entities1 = null;
            List<JToken> entities2 = new List<JToken>();
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail();
            string entitySet = string.Empty;
            string navigPropName = string.Empty;
            string sortedPropName = string.Empty;
            string sortedPropType = string.Empty;
            var restrictions = new Dictionary<string, Tuple<List<NormalProperty>, List<NavigProperty>>>();

            if (!AnnotationsHelper.GetExpandRestrictions(context.MetadataDocument, context.VocCapabilities, ref restrictions))
            {
                detail1.ErrorMessage = "Cannot find any appropriate entity-sets which support system query options $expand in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return null;
            }

            foreach (var r in restrictions)
            {
                if (string.IsNullOrEmpty(r.Key) ||
                    null == r.Value.Item1 || !r.Value.Item1.Any() ||
                    null == r.Value.Item2 || !r.Value.Item2.Any())
                {
                    continue;
                }

                bool flag = false;

                foreach (var np in r.Value.Item2)
                {
                    if (NavigationRoughType.CollectionValued == np.NavigationRoughType)
                    {
                        var nEntityTypeShortName = np.NavigationPropertyType.RemoveCollectionFlag().GetLastSegment();
                        var nEntitySetName = nEntityTypeShortName.MapEntityTypeShortNameToEntitySetName();
                        var funcs = new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>()
                        {
                            AnnotationsHelper.GetFilterRestrictions
                        };
                        List<string> sortedPropTypes = new List<string>()
                        {
                            PrimitiveDataTypes.Binary, PrimitiveDataTypes.Boolean, PrimitiveDataTypes.Byte, PrimitiveDataTypes.Decimal,
                            PrimitiveDataTypes.Double, PrimitiveDataTypes.Guid, PrimitiveDataTypes.Int16, PrimitiveDataTypes.Int32,
                            PrimitiveDataTypes.Int64, PrimitiveDataTypes.SByte, PrimitiveDataTypes.Single, PrimitiveDataTypes.String
                        };
                        var filterRestrictions = nEntitySetName.GetRestrictions(context.MetadataDocument, context.VocCapabilities, funcs, sortedPropTypes);

                        if (!string.IsNullOrEmpty(filterRestrictions.Item1) ||
                            null != filterRestrictions.Item2 || filterRestrictions.Item2.Any() ||
                            null != filterRestrictions.Item3 || filterRestrictions.Item3.Any())
                        {
                            flag = true;
                            entitySet = r.Key;
                            navigPropName = np.NavigationPropertyName;
                            sortedPropName = filterRestrictions.Item2.First().PropertyName;
                            sortedPropType = filterRestrictions.Item2.First().PropertyType;

                            break;
                        }
                    }
                }

                if (flag)
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(sortedPropName) ||
                string.IsNullOrEmpty(sortedPropType) ||
                string.IsNullOrEmpty(entitySet) ||
                string.IsNullOrEmpty(navigPropName))
            {
                detail1.ErrorMessage = "Cannot find an appropriate entity-set to verify the system query option $orderby.\r\n";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);

                return null;
            }

            string url = string.Format("{0}/{1}?$expand={2}($orderby={3} {4})", context.ServiceBaseUri, entitySet, navigPropName, sortedPropName, sortedType.ToString().ToLower());
            Uri uri = new Uri(url);
            Response resp = WebHelper.Get(uri, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail1 = new ExtensionRuleResultDetail("", uri.AbsoluteUri, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

            if (resp != null && resp.StatusCode == HttpStatusCode.OK)
            {
                JObject feed;
                resp.ResponsePayload.TryToJObject(out feed);
                var entities = JsonParserHelper.GetEntries(feed);

                if (null == entities)
                {
                    detail1.ErrorMessage = string.Format("The entity-set '{0}' has no entity in the service.", entitySet);
                    info = new ExtensionRuleViolationInfo(uri, resp.ResponsePayload, detail1);

                    return null;
                }

                foreach (var en in entities)
                {
                    if (null != en[navigPropName] && JTokenType.Array == en[navigPropName].Type)
                    {
                        entities1 = en[navigPropName].ToList();
                        break;
                    }
                }

                entities2.AddRange(entities1);
                entities2.Sort(new JTokenCompare(sortedPropName, sortedPropType, sortedType));
            }
            else
            {
                detail1.ErrorMessage = "The service does not support the system query option '$orderby'.";
                info = new ExtensionRuleViolationInfo(uri, resp.ResponsePayload, detail1);
                return false;
            }

            bool? result = entities1.SequenceEquals(entities2);
            if (result == false)
            {
                detail1.ErrorMessage = string.Format("The service does not support $orderby ({0}) system query options.", sortedType.ToString().ToLower());
            }

            info = new ExtensionRuleViolationInfo(uri, resp.ResponsePayload, detail1);
            return result;
        }

        /// <summary>
        /// Verifies sorting entities order by ascending or descending.
        /// </summary>
        /// <param name="context">The Interop service context.</param>
        /// <param name="sortedType">The sorted type.</param>
        /// <param name="passed">Out parameter indicates validation for sort pass or not</param>
        /// <param name="info">Out parameter indicates the extension rule violation information.</param>
        /// <returns>Returns the HTTP status code of the response.</returns>
        public static HttpStatusCode? VerifySortEntities(ServiceContext context, SortedType sortedType, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            if (SortedType.ASC == sortedType && context.ServiceVerResult.AscSortVerResult != null && context.ServiceVerResult.AscSortVerResult.Passed.HasValue)
            {
                info = context.ServiceVerResult.AscSortVerResult.ViolationInfo;
                passed = context.ServiceVerResult.AscSortVerResult.Passed;
                return context.ServiceVerResult.AscSortVerResult.ResponseStatusCode;
            }
            if (SortedType.DESC == sortedType && context.ServiceVerResult.DescSortVerResult != null && context.ServiceVerResult.DescSortVerResult.Passed.HasValue)
            {
                info = context.ServiceVerResult.DescSortVerResult.ViolationInfo;
                passed = context.ServiceVerResult.DescSortVerResult.Passed;
                return context.ServiceVerResult.DescSortVerResult.ResponseStatusCode;
            }

            passed = null;
            HttpStatusCode? respStatusCode = null;
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail();
            List<string> sortedPropTypes = new List<string>()
            {
                PrimitiveDataTypes.Binary, PrimitiveDataTypes.Boolean, PrimitiveDataTypes.Byte, PrimitiveDataTypes.Decimal,
                PrimitiveDataTypes.Double, PrimitiveDataTypes.Guid, PrimitiveDataTypes.Int16, PrimitiveDataTypes.Int32,
                PrimitiveDataTypes.Int64, PrimitiveDataTypes.SByte, PrimitiveDataTypes.Single, PrimitiveDataTypes.String
            };


            var restrictions = AnnotationsHelper.GetFilterRestrictions(context.MetadataDocument, context.VocCapabilities, sortedPropTypes);

            if (string.IsNullOrEmpty(restrictions.Item1) ||
                null == restrictions.Item2 || !restrictions.Item2.Any())
            {
                detail1.ErrorMessage = "Cannot find any appropriate entity-sets which support system query options $filter in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);
            }

            string entitySet = restrictions.Item1;
            string sortedPropName = restrictions.Item2.First().PropertyName;
            string sortedPropType = restrictions.Item2.First().PropertyType;
            string url = string.Format("{0}/{1}?$orderby={2} {3}", context.ServiceBaseUri, entitySet, sortedPropName, sortedType.ToString().ToLower());
            var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail1 = new ExtensionRuleResultDetail(string.Empty, url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

            if (resp != null && resp.StatusCode == HttpStatusCode.OK)
            {
                JObject feed;
                resp.ResponsePayload.TryToJObject(out feed);

                if (feed == null || VerifySortedEntitiesSequence(feed, sortedPropName, sortedPropType, sortedType) != true)
                {
                    passed = false;
                    detail1.ErrorMessage = string.Format("The service does not execute an accurate result on the system query option $orderby {0} on individual properties.", sortedType == SortedType.ASC ? "asc" : "desc");
                }
                else
                {
                    passed = true;
                }
            }
            else
            {
                passed = false;
                detail1.ErrorMessage = "The service does not support the system query option '$orderby'.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail1);
            respStatusCode = resp != null ? resp.StatusCode : null;

            if (SortedType.ASC == sortedType)
            {
                context.ServiceVerResult.AscSortVerResult = new ServiceVerificationResult(passed, info, respStatusCode);
            }
            else if (SortedType.DESC == sortedType)
            {
                context.ServiceVerResult.DescSortVerResult = new ServiceVerificationResult(passed, info, respStatusCode);
            }

            return respStatusCode;
        }

        /// <summary>
        /// Verify the Lambda operators.
        /// </summary>
        /// <param name="context">The Interop service context.</param>
        /// <param name="lambdaOpType">The type of lambda operator.</param>
        /// <param name="info">Out parameter indicates the extension rule violation information.</param>
        /// <returns>Returns the http status code of the response.</returns>
        public static HttpStatusCode? VerifyLambdaOperators(ServiceContext context, LambdaOperatorType lambdaOpType, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            bool flag = false;
            passed = null;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            var restrictions = AnnotationsHelper.GetRestrictions(
                context.MetadataDocument,
                context.VocCapabilities,
                new List<Func<string, string, string, List<NormalProperty>, List<NavigProperty>, bool>>() { AnnotationsHelper.GetExpandRestrictions, AnnotationsHelper.GetFilterRestrictions },
                null,
                NavigationRoughType.CollectionValued);

            if (string.IsNullOrEmpty(restrictions.Item1) ||
                null == restrictions.Item2 || !restrictions.Item2.Any() ||
                null == restrictions.Item3 || !restrictions.Item3.Any())
            {
                detail.ErrorMessage = "Cannot find an appropriate entity-set to verify the system query options $expand and $filter at the same time.\r\n";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return null;
            }

            string entitySet = restrictions.Item1;
            var collectionValuedNavigProps = restrictions.Item3
                .Where(np => NavigationRoughType.CollectionValued == np.NavigationRoughType)
                .Select(np => np);

            if (null == collectionValuedNavigProps || !collectionValuedNavigProps.Any())
            {
                detail.ErrorMessage = "Cannot find any collection-valued navigation properties in the entity-set.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return null;
            }

            string navigProp = collectionValuedNavigProps.First().NavigationPropertyName;
            string entityTypeShortName = collectionValuedNavigProps.First().NavigationPropertyType
                .RemoveCollectionFlag()
                .GetLastSegment();

            List<string> specifiedTypes = new List<string>()
            {
                PrimitiveDataTypes.Byte, PrimitiveDataTypes.Int16, PrimitiveDataTypes.Int32, PrimitiveDataTypes.Int64,
                PrimitiveDataTypes.Decimal, PrimitiveDataTypes.Double, PrimitiveDataTypes.Single, PrimitiveDataTypes.SByte
            };

            var props = MetadataHelper.GetPropertiesWithSpecifiedTypeFromEntityType(entityTypeShortName, context.MetadataDocument, specifiedTypes);

            if (null == props || props.Count == 0)
            {
                detail.ErrorMessage = "Cannot find any appropriate primitive types of properties in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return null;
            }

            var prop = props.First();
            string[] strArr = prop.Split(',');

            JObject feed;
            JArray entities;
            string url = LambdaOperatorType.Any == lambdaOpType ?
                string.Format(@"{0}/{1}?$expand={2}&$filter={2}/any(d:d/{3} ge 1)", context.ServiceBaseUri, entitySet, navigProp, strArr[0]) :
                string.Format(@"{0}/{1}?$expand={2}&$filter={2}/all(d:d/{3} ge 1)", context.ServiceBaseUri, entitySet, navigProp, strArr[0]);
            Uri destination = new Uri(url);
            Response resp = WebHelper.Get(destination, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail("", url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

            if (resp != null && resp.StatusCode == HttpStatusCode.OK)
            {
                resp.ResponsePayload.TryToJObject(out feed);
                entities = JsonParserHelper.GetEntries(feed);

                foreach (var entity in entities)
                {
                    JArray temp = entity[navigProp] as JArray;
                    flag = LambdaOperatorType.Any == lambdaOpType ? false : true;

                    foreach (var t in temp)
                    {
                        if (LambdaOperatorType.Any == lambdaOpType ?
                            t[strArr[0]].GreaterThanOrEquals(1, strArr[1]) :
                            !t[strArr[0]].GreaterThanOrEquals(1, strArr[1]))
                        {
                            flag = LambdaOperatorType.Any == lambdaOpType ? true : false;
                            break;
                        }
                    }

                    if (flag == false)
                    {
                        passed = false;
                        detail.ErrorMessage = string.Format("The service does not execute an accurate result on the lambda operators '{0}' on navigation- and collection-valued properties.", LambdaOperatorType.Any == lambdaOpType ? "any" : "all");

                        break;
                    }
                    else
                    {
                        passed = true;
                    }
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The service does not support the system query option '$expand' or lambda operators (all and any).";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return resp != null ? resp.StatusCode : null;
        }

        /// <summary>
        /// Verify metadata request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match metadata; false: otherwise</returns>
        public static bool VerifyMetadata(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;
            string metadataUrl = string.Format("{0}/$metadata", context.DestinationBasePath);
            Response response = WebHelper.Get(new Uri(metadataUrl), null, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(string.Empty, metadataUrl, "GET", StringHelper.MergeHeaders(string.Empty, context.RequestHeaders), response);
            var payloadFormat = response.ResponsePayload.GetFormatFromPayload();
            var payloadType = ContextHelper.GetPayloadType(response.ResponsePayload, payloadFormat, response.ResponseHeaders);

            if (payloadType == RuleEngine.PayloadType.Metadata)
            {
                result = true;
            }
            else
            {
                detail.ErrorMessage = "The response is not a metadata document.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return result;
        }

        /// <summary>
        /// Verify service document request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match service document; false: otherwise</returns>
        public static bool VerifySvcDoc(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;

            Response response = WebHelper.Get(context.Destination, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail("", context.Destination.AbsoluteUri, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), response);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var payloadFormat = response.ResponsePayload.GetFormatFromPayload();
                var payloadType = ContextHelper.GetPayloadType(response.ResponsePayload, payloadFormat, response.ResponseHeaders);

                if (payloadType == RuleEngine.PayloadType.ServiceDoc)
                {
                    result = true;
                }
                else
                {
                    detail.ErrorMessage = "The response is not a service document.";
                }
            }
            else
            {
                detail.ErrorMessage = "Get service document failed from above URI.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return result;
        }

        /// <summary>
        /// Verify error request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match error; false: otherwise</returns>
        public static bool VerifyError(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;
            var payloadFormat = context.ServiceDocument.GetFormatFromPayload();
            string[] feeds = ContextHelper.GetFeeds(context.ServiceDocument, payloadFormat).ToArray();

            string errorFeed = "foo";
            while (feeds.Contains(errorFeed))
            {
                errorFeed += "o";
            }

            string errorURL = context.Destination + "/" + errorFeed;
            Response response = WebHelper.Get(new Uri(errorURL), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(string.Empty, errorURL, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), response);

            payloadFormat = response.ResponsePayload.GetFormatFromPayload();
            var payloadType = ContextHelper.GetPayloadType(response.ResponsePayload, payloadFormat, response.ResponseHeaders);

            if (payloadType == RuleEngine.PayloadType.Error)
            {
                result = true;
            }
            else
            {
                detail.ErrorMessage = "The response is not an error response.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return result;
        }

        /// <summary>
        /// Verify feed and entry request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match feed and entry; false: otherwise</returns>
        public static bool VerifyFeedAndEntry(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;
            var payloadFormat = context.ServiceDocument.GetFormatFromPayload();
            string[] feeds = ContextHelper.GetFeeds(context.ServiceDocument, payloadFormat).ToArray();
            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();
            ExtensionRuleResultDetail detail1 = new ExtensionRuleResultDetail();

            if (feeds.Any())
            {
                string feedURL = context.Destination + "/" + feeds.First() + "?$top=1";
                Response response = WebHelper.Get(new Uri(feedURL), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail1 = new ExtensionRuleResultDetail(string.Empty, feedURL, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), response);
                payloadFormat = response.ResponsePayload.GetFormatFromPayload();
                var payloadType = ContextHelper.GetPayloadType(response.ResponsePayload, payloadFormat, response.ResponseHeaders);

                if (payloadType == RuleEngine.PayloadType.Feed)
                {
                    var entries = ContextHelper.GetEntries(response.ResponsePayload, payloadFormat).ToArray();
                    if (entries.Any())
                    {
                        string entryURL = entries.First();
                        response = WebHelper.Get(new Uri(entryURL), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                        ExtensionRuleResultDetail detail2 = new ExtensionRuleResultDetail(string.Empty, entryURL, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), response);
                        payloadFormat = response.ResponsePayload.GetFormatFromPayload();
                        payloadType = ContextHelper.GetPayloadType(response.ResponsePayload, payloadFormat, response.ResponseHeaders);

                        if (payloadType == RuleEngine.PayloadType.Entry)
                        {
                            result = true;
                        }
                        else
                        {
                            detail2.ErrorMessage = "The response is not entry response.";
                        }
                        details.Insert(0, detail2);
                    }
                    else
                    {
                        detail1.ErrorMessage = "There is no entry instance.";
                    }
                }
                else
                {
                    detail1.ErrorMessage = "The response is not a feed response.";
                }
            }
            else
            {
                detail1.ErrorMessage = "There is no feed instance.";
            }
            details.Insert(0, detail1);

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);
            return result;
        }

        /// <summary>
        /// Verify property request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match property; false: otherwise</returns>
        public static bool VerifyPropertyAndValue(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;
            var payloadFormat = context.ServiceDocument.GetFormatFromPayload();
            string[] feeds = ContextHelper.GetFeeds(context.ServiceDocument, payloadFormat).ToArray();
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            string propertyURL = MetadataHelper.GenerateIndividualPropertyURL(context.MetadataDocument, context.ServiceDocument, context.ServiceBaseUri.AbsoluteUri, new List<string>() { PrimitiveDataTypes.Boolean, PrimitiveDataTypes.String, PrimitiveDataTypes.Int32 });

            if (string.IsNullOrEmpty(propertyURL))
            {
                detail.ErrorMessage = "Can not generate property URI from this service.";
            }
            else
            {
                Response response = WebHelper.Get(new Uri(propertyURL), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail = new ExtensionRuleResultDetail(string.Empty, propertyURL, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), response);

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    JObject jo;
                    response.ResponsePayload.TryToJObject(out jo);

                    if (jo != null && jo[Constants.OdataV4JsonIdentity] != null && jo[Constants.Value] != null)
                    {
                        string propertyValueURL = propertyURL + @"/$value";
                        response = WebHelper.Get(new Uri(propertyValueURL), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                        if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                        {
                            result = true;
                        }
                        else
                        {
                            detail.ErrorMessage = "The response is not an property value response.";
                        }
                    }
                    else
                    {
                        detail.ErrorMessage = "The response is not an property response.";
                    }
                }
                else
                {
                    detail.ErrorMessage = "The service does not return a property.";
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return result;
        }

        /// <summary>
        /// Verify the count of collection request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match count of collection; false: otherwise</returns>
        public static bool VerifyCollectionCount(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;
            var payloadFormat = context.ServiceDocument.GetFormatFromPayload();
            string[] feeds = ContextHelper.GetFeeds(context.ServiceDocument, payloadFormat).ToArray();
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            if (feeds.Any())
            {
                string feedCountURL = context.Destination + "/" + feeds.First() + @"/$count";
                Response response = WebHelper.Get(new Uri(feedCountURL), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail = new ExtensionRuleResultDetail(string.Empty, feedCountURL, "GET", StringHelper.MergeHeaders(string.Empty, context.RequestHeaders), response);

                if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                {
                    result = true;
                }
                else
                {
                    detail.ErrorMessage = "The response is not the count of collection.";
                }
            }
            else
            {
                detail.ErrorMessage = "There is no feed instance.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return result;
        }

        /// <summary>
        /// Verify the derived type request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match derived type; false: otherwise</returns>
        public static bool VerifyDerivedType(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;
            string derivedTypeURL = MetadataHelper.GenerateDerivedTypeURL(context.MetadataDocument, context.ServiceDocument).TrimStart('/');
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            if (string.IsNullOrEmpty(derivedTypeURL))
            {
                detail.ErrorMessage = string.Format("There is no usable URI to address derived types.");
            }
            else
            {
                derivedTypeURL = context.ServiceBaseUri.AbsoluteUri.TrimEnd('/') + @"/" + derivedTypeURL;
                Response response = WebHelper.Get(new Uri(derivedTypeURL), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail = new ExtensionRuleResultDetail(string.Empty, derivedTypeURL, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), response);

                if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                {
                    JObject jo;
                    response.ResponsePayload.TryToJObject(out jo);

                    if (jo != null && jo[Constants.OdataV4JsonIdentity] != null && jo[Constants.Value] != null)
                    {
                        result = true;
                    }
                    else
                    {
                        detail.ErrorMessage = "The response is not a JSON object including \"@odata.context\" and \"value\" properties.";
                    }
                }
                else
                {
                    detail.ErrorMessage = "The response is not a JSON object including \"@odata.context\" and \"value\" properties.";
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return result;
        }

        /// <summary>
        /// Verify the media stream request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match Media Stream; false: otherwise</returns>
        public static bool VerifyMediaStream(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;
            string mediaStreamURL = MetadataHelper.GenerateMediaStreamURL(context.MetadataDocument, context.ServiceDocument, context.ServiceBaseUri.AbsoluteUri);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            if (string.IsNullOrEmpty(mediaStreamURL))
            {
                detail.ErrorMessage = "There is no usable URI to address the Media Stream of a Media Entity.";
            }
            else
            {
                mediaStreamURL += @"/$value";
                Response response = WebHelper.Get(new Uri(mediaStreamURL), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail = new ExtensionRuleResultDetail(string.Empty, mediaStreamURL, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), response);

                if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                {
                    result = true;
                }
                else
                {
                    detail.ErrorMessage = "Address the media property failed from above URI.";
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return result;
        }

        /// <summary>
        /// Verify the cross join request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>True: The request and response match cross join; false: otherwise</returns>
        public static bool VerifyCrossJoin(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context.ServiceVerResult.CrossJoinVerResult != null)
            {
                info = context.ServiceVerResult.CrossJoinVerResult.ViolationInfo;
                return context.ServiceVerResult.CrossJoinVerResult.Passed == null ? false : context.ServiceVerResult.CrossJoinVerResult.Passed.Value;
            }

            bool result = false;
            string crossJoinURL = string.Empty;
            var payloadFormat = context.ServiceDocument.GetFormatFromPayload();
            string[] feeds = ContextHelper.GetFeeds(context.ServiceDocument, payloadFormat).ToArray();
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            if (feeds.Length < 2)
            {
                detail.ErrorMessage = "No usable URI to address the Cross Join of Entity Sets because there are less than 2 EntitySets.";
            }
            else
            {
                crossJoinURL = context.ServiceBaseUri.AbsoluteUri.TrimEnd('/') + string.Format(@"/$crossjoin({0},{1})", feeds[0], feeds[1]);
                Response response = WebHelper.Get(new Uri(crossJoinURL), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail = new ExtensionRuleResultDetail(string.Empty, (new Uri(crossJoinURL)).AbsoluteUri, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), response);

                if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                {
                    result = true;
                }
                else
                {
                    detail.ErrorMessage = "Address the Cross Join of EntitySets failed from above URI.";
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            context.ServiceVerResult.CrossJoinVerResult = new ServiceVerificationResult(result, info);

            return result;
        }

        /// <summary>
        /// Verify the all entities request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match all entities; false: otherwise</returns>
        public static bool VerifyAllEntities(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;

            string allEntitiesURL = context.Destination.AbsoluteUri + @"/$all";
            Response response = WebHelper.Get(new Uri(allEntitiesURL), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(string.Empty, allEntitiesURL, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), response);

            if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
            {
                result = true;
            }
            else
            {
                detail.ErrorMessage = "Address all entities failed from above URI.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return result;
        }

        /// <summary>
        /// Verify the reference request and response.
        /// </summary>
        /// <param name="context">The service document context.</param>
        /// <returns>True: The request and response match reference; false: otherwise</returns>
        public static bool VerifyReference(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            bool result = false;
            string referenceURL = MetadataHelper.GenerateReferenceURL(context.MetadataDocument, context.ServiceDocument, context.ServiceBaseUri.AbsoluteUri);
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            if (string.IsNullOrEmpty(referenceURL))
            {
                detail.ErrorMessage = "Can not get reference URL from service.";
            }
            else
            {
                Response response = WebHelper.Get(new Uri(referenceURL), Constants.V4AcceptHeaderJsonFullMetadata, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail = new ExtensionRuleResultDetail(string.Empty, referenceURL, "GET", StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, context.RequestHeaders), response);

                if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.ResponsePayload))
                {
                    JObject jo;
                    response.ResponsePayload.TryToJObject(out jo);

                    if (jo != null)
                    {
                        result = true;
                    }
                    else
                    {
                        detail.ErrorMessage = "The response is not JSON object.";
                    }
                }
                else
                {
                    detail.ErrorMessage = "There is no response returned.";
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return result;
        }

        /// <summary>
        /// Verify the canonical function.
        /// </summary>
        /// <param name="context">The service document.</param>
        /// <param name="canonicalFuncType">The canonical function type.</param>
        /// <param name="info">Out parameter indicates the extension rule violation information.</param>
        /// <returns>Returns the http status code of the response.</returns>
        public static HttpStatusCode? VerifyCanonicalFunction(ServiceContext context, CanonicalFunctionType canonicalFuncType, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            passed = null;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(string.Empty);
            var filterRestrictions = AnnotationsHelper.GetFilterRestrictions(context.MetadataDocument, context.VocCapabilities, new List<string>() { "Edm.String" });

            if (string.IsNullOrEmpty(filterRestrictions.Item1) ||
                null == filterRestrictions.Item2 || !filterRestrictions.Item2.Any())
            {
                detail.ErrorMessage = "Cannot find an appropriate entity-set which supports $filter system query options in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return null;
            }

            string entitySet = filterRestrictions.Item1;
            string primitivePropName = string.Empty;

            foreach (var prop in filterRestrictions.Item2)
            {
                if ("Edm.String" == prop.PropertyType)
                {
                    primitivePropName = prop.PropertyName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(primitivePropName))
            {
                detail.ErrorMessage = "Cannot find an appropriate primitive properties of entity type in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return null;
            }

            string url = string.Format("{0}/{1}", context.ServiceBaseUri, entitySet);
            var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

            if (null == resp || HttpStatusCode.OK != resp.StatusCode)
            {
                detail.ErrorMessage = JsonParserHelper.GetErrorMessage(resp.ResponsePayload);
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return null;
            }

            JObject feed;
            resp.ResponsePayload.TryToJObject(out feed);

            if (feed != null)
            {
                var entity = JsonParserHelper.GetEntries(feed).First;
                string propVal = entity[primitivePropName].ToString();

                url = canonicalFuncType == CanonicalFunctionType.Supported ?
                    string.Format("{0}?$filter=contains({1},'{2}')", url, primitivePropName, propVal) :
                    string.Format("{0}?$filter=undefined({1},'{2}')", url, primitivePropName, propVal);
                resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                detail = new ExtensionRuleResultDetail("", url, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), resp);

                if (resp != null && HttpStatusCode.OK == resp.StatusCode)
                {
                    JObject jObj;
                    resp.ResponsePayload.TryToJObject(out jObj);

                    if (jObj != null && JTokenType.Object == jObj.Type)
                    {
                        var entries = JsonParserHelper.GetEntries(jObj).ToList();
                        var temp = entries.FindAll(en => propVal == en[primitivePropName].ToString()).Select(en => en);

                        if (entries.Count() != temp.Count())
                        {
                            passed = false;
                            detail.ErrorMessage = canonicalFuncType == CanonicalFunctionType.Supported ?
                                "The service does not support the canonical functions." :
                                "The service does not return 501 Not Implemented for any unsupported canonical functions.";
                        }
                        else
                        {
                            passed = true;
                        }
                    }
                    else
                    {
                        passed = false;
                        detail.ErrorMessage = canonicalFuncType == CanonicalFunctionType.Supported ?
                               "The service does not support the canonical functions." :
                               "The service does not return 501 Not Implemented for any unsupported canonical functions.";
                    }
                }
                else if (resp != null && HttpStatusCode.NotImplemented == resp.StatusCode)
                {
                    passed = false;
                    detail.ErrorMessage = canonicalFuncType == CanonicalFunctionType.Supported ?
                               "The service does not support the canonical functions." :
                               "The service does not return 501 Not Implemented for any unsupported canonical functions.";
                }
                else
                {
                    passed = false;
                    detail.ErrorMessage = JsonParserHelper.GetErrorMessage(resp.ResponsePayload);
                }
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return resp != null ? resp.StatusCode : null;
        }

        /// <summary>
        /// Verify the "/$count" segment.
        /// </summary>
        /// <param name="context">The service document.</param>
        /// <param name="info">Out parameter indicates the extension rule violation information.</param>
        /// <returns>Returns the http status code of the response.</returns>
        public static HttpStatusCode? VerifyDollarCountSegment(ServiceContext context, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            passed = null;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();
            var countRestrictions = AnnotationsHelper.GetCountRestrictions(context.MetadataDocument, context.VocCapabilities);

            if (string.IsNullOrEmpty(countRestrictions.Item1))
            {
                detail.ErrorMessage = "The service does not support /$count segement.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return null;
            }

            string entitySet = countRestrictions.Item1;
            string url = string.Format("{0}/{1}", context.ServiceBaseUri, entitySet);
            var resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail(string.Empty, url, "GET", string.Empty, resp);

            if (resp != null && resp.StatusCode == HttpStatusCode.OK)
            {
                JObject feed;
                resp.ResponsePayload.TryToJObject(out feed);

                if (feed != null && JTokenType.Object == feed.Type)
                {
                    int actualAmount = 0;
                    JsonParserHelper.GetEntitiesCountFromFeed(new Uri(url), feed, context.RequestHeaders, ref actualAmount);
                    url = string.Format("{0}/$count", url);
                    var req = WebRequest.Create(url) as HttpWebRequest;
                    resp = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
                    detail = new ExtensionRuleResultDetail(string.Empty, url, "GET", string.Empty, resp);

                    if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                    {
                        if (actualAmount != Convert.ToInt32(resp.ResponsePayload))
                        {
                            passed = false;
                            detail.ErrorMessage = string.Format("The actual entities' amount of the '{0}' set does not equal to the value which is returned by using '/$count' segment to request to the service.", context.DestinationBaseLastSegment);
                        }
                        else
                        {
                            passed = true;
                        }
                    }
                    else
                    {
                        passed = false;
                        detail.ErrorMessage = "The service does not support the /$count segment.";
                    }
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = string.Format("Cannot find the entity-set '{0}' on the service.", entitySet);
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            return resp != null ? resp.StatusCode : null;
        }

        /// <summary>
        /// Verifies $select.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public static HttpStatusCode? VerifySelect(ServiceContext context, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            if (context.ServiceVerResult.SelectResult != null)
            {
                info = context.ServiceVerResult.SelectResult.ViolationInfo;
                passed = context.ServiceVerResult.SelectResult.Passed;
                return context.ServiceVerResult.SelectResult.ResponseStatusCode;
            }

            passed = null;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            var payloadFormat = context.ServiceDocument.GetFormatFromPayload();
            string[] entitySetUrls = ContextHelper.GetFeeds(context.ServiceDocument, payloadFormat).ToArray();
            if (null == entitySetUrls || !entitySetUrls.Any())
            {
                detail.ErrorMessage = "The service does not have any feed returned.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
                context.ServiceVerResult.SelectResult = new ServiceVerificationResult(passed, info);

                return null;
            }

            string entitySetName = entitySetUrls.First().MapEntitySetURLToEntitySetName();
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();
            var props = MetadataHelper.GetAllPropertiesOfEntity(context.MetadataDocument, entityTypeShortName, MatchPropertyType.Normal);
            List<string> propNames = new List<string>();
            foreach (var prop in props)
            {
                propNames.Add(prop.Attribute("Name").Value);
            }

            string url = string.Format("{0}/{1}?$select={2}", context.DestinationBasePath, entitySetName, propNames[0]);
            var req = WebRequest.Create(url) as HttpWebRequest;
            Response selectResponse = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
            detail = new ExtensionRuleResultDetail(string.Empty, url, "GET", string.Empty, selectResponse);

            if (selectResponse.StatusCode == HttpStatusCode.OK)
            {
                JObject jo;
                selectResponse.ResponsePayload.TryToJObject(out jo);

                foreach (JObject ob in (JArray)jo[Constants.Value])
                {
                    if (ob[propNames[0]] == null)
                    {
                        passed = false;
                        detail.ErrorMessage = "The selected property does not exist in response payload.";
                        break;
                    }
                }

                if (null == passed)
                {
                    passed = true;
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The service does not support '$select' system query option.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            context.ServiceVerResult.SelectResult = new ServiceVerificationResult(passed, info, selectResponse.StatusCode);

            return selectResponse.StatusCode;
        }

        /// <summary>
        /// Verifies $expand.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public static HttpStatusCode? VerifyExpand(ServiceContext context, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            if (context.ServiceVerResult.ExpandResult != null)
            {
                info = context.ServiceVerResult.ExpandResult.ViolationInfo;
                passed = context.ServiceVerResult.ExpandResult.Passed;
                return context.ServiceVerResult.ExpandResult.ResponseStatusCode;
            }

            passed = null;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            var expandRestrictions = AnnotationsHelper.GetExpandRestrictions(context.MetadataDocument, context.VocCapabilities);
            if (string.IsNullOrEmpty(expandRestrictions.Item1) ||
                null == expandRestrictions.Item3 || !expandRestrictions.Item3.Any())
            {
                detail.ErrorMessage = "Cannot find an entity-set which supports $expand system query options.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
                context.ServiceVerResult.ExpandResult = new ServiceVerificationResult(passed, info);

                return null;
            }

            string entitySet = expandRestrictions.Item1;
            string navigProp = expandRestrictions.Item3.First().NavigationPropertyName;

            string url = string.Format("{0}/{1}?$expand={2}", context.ServiceBaseUri, entitySet, navigProp);
            Response resp = WebHelper.Get(WebRequest.Create(url), RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
            detail = new ExtensionRuleResultDetail(string.Empty, url, "GET", string.Empty, resp);

            if (!string.IsNullOrEmpty(resp.ResponsePayload))
            {
                JObject feed;
                resp.ResponsePayload.TryToJObject(out feed);
                var entities = JsonParserHelper.GetEntries(feed);

                foreach (JObject ob in entities)
                {
                    if (ob[navigProp] == null)
                    {
                        passed = false;
                        detail.ErrorMessage = string.Format("The expanded property {0} does not exist in response payload.", navigProp);
                        break;
                    }
                    else
                    {
                        passed = true;
                    }
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = string.Format("Get response failed from above URI.");
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            context.ServiceVerResult.ExpandResult = new ServiceVerificationResult(passed, info, resp.StatusCode);

            return resp.StatusCode;
        }

        /// <summary>
        /// Verifies $top.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public static HttpStatusCode? VerifyTop(ServiceContext context, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            if (context.ServiceVerResult.TopResult != null)
            {
                info = context.ServiceVerResult.TopResult.ViolationInfo;
                passed = context.ServiceVerResult.TopResult.Passed;
                return context.ServiceVerResult.TopResult.ResponseStatusCode;
            }

            passed = null;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(string.Empty);

            var countRestrictions = AnnotationsHelper.GetCountRestrictions(
                context.MetadataDocument,
                context.VocCapabilities,
                null,
                NavigationRoughType.None,
                new List<Func<string, string, List<string>, bool?>>() { SupportiveFeatureHelper.IsEntitySetSupportTopQuery });

            if (string.IsNullOrEmpty(countRestrictions.Item1))
            {
                detail.ErrorMessage = "The service does not contain any entity-sets which support system query $top.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
                context.ServiceVerResult.TopResult = new ServiceVerificationResult(passed, info);

                return null;
            }

            var entitySet = countRestrictions.Item1;

            // Send /$count and get the Entries count of response payload.
            var url = string.Format("{0}/{1}/$count", context.ServiceBaseUri, entitySet);
            var req = WebRequest.Create(url) as HttpWebRequest;
            Response resp = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
            detail = new ExtensionRuleResultDetail(string.Empty, url, "GET", null, resp);

            if (HttpStatusCode.OK != resp.StatusCode)
            {
                detail.ErrorMessage = "The service does not support /$count query in the request URL.";
                info = new ExtensionRuleViolationInfo(new Uri(url), resp.ResponsePayload, detail);
                context.ServiceVerResult.TopResult = new ServiceVerificationResult(passed, info);

                return null;
            }

            Random rnd = new Random();
            int topNumber = rnd.Next(0, Convert.ToInt32(resp.ResponsePayload));
            if (DataService.serviceInstance != null) // Add for test project
            {
                topNumber = Convert.ToInt32(resp.ResponsePayload) > 2 ? 2 : 0;
            }
            string topUri = string.Format("{0}/{1}/?$top={2}", context.ServiceBaseUri, entitySet, topNumber);
            Response topResponse = WebHelper.Get(new Uri(topUri), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail(string.Empty, topUri, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), topResponse);

            JObject feed;
            topResponse.ResponsePayload.TryToJObject(out feed);
            int countAfterTop = 0;
            JsonParserHelper.GetEntitiesCountFromFeed(new Uri(topUri), feed, context.RequestHeaders, ref countAfterTop);

            if (topResponse.StatusCode == HttpStatusCode.OK)
            {
                if (countAfterTop != topNumber)
                {
                    passed = false;
                    detail.ErrorMessage = "The service does not execute an accurate result on '$top' system query option.";
                }
                else
                {
                    passed = true;
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = JsonParserHelper.GetErrorMessage(topResponse.ResponsePayload);
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            context.ServiceVerResult.TopResult = new ServiceVerificationResult(passed, info, topResponse.StatusCode);

            return topResponse.StatusCode;
        }

        /// <summary>
        /// Verifies $count.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public static HttpStatusCode? VerifyCount(ServiceContext context, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            if (context.ServiceVerResult.CountResult != null)
            {
                info = context.ServiceVerResult.CountResult.ViolationInfo;
                passed = context.ServiceVerResult.CountResult.Passed;
                return context.ServiceVerResult.CountResult.ResponseStatusCode;
            }

            passed = null;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(string.Empty);
            var countRestrictions = AnnotationsHelper.GetCountRestrictions(context.MetadataDocument, context.VocCapabilities);

            if (string.IsNullOrEmpty(countRestrictions.Item1))
            {
                detail.ErrorMessage = "The service does not support $count system query option.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
                context.ServiceVerResult.CountResult = new ServiceVerificationResult(passed, info);

                return null;
            }

            string entitySet = countRestrictions.Item1;
            string url = string.Format("{0}/{1}?$count=true", context.ServiceBaseUri, entitySet);
            Uri uri = new Uri(url);
            Response countResponse = WebHelper.Get(uri, Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            detail = new ExtensionRuleResultDetail(string.Empty, uri.AbsoluteUri, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), countResponse);

            JObject feed;
            countResponse.ResponsePayload.TryToJObject(out feed);
            // Get @odata.count value and totalCountOfEntities.
            int odataCount = Int32.Parse(feed[Constants.V4OdataCount].ToString());

            // Get actual amount of entities in the current entity-set.
            int totalCountOfEntities = 0;
            JsonParserHelper.GetEntitiesCountFromFeed(uri, feed, context.RequestHeaders, ref totalCountOfEntities);

            if (HttpStatusCode.OK == countResponse.StatusCode)
            {
                if (totalCountOfEntities != odataCount)
                {
                    passed = false;
                    detail.ErrorMessage = string.Format("The odata.count value is {0} and the total count of entities is {1}, they should be equal.", odataCount, totalCountOfEntities);
                }
                else
                {
                    passed = true;
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "Request with $count query failed with above URI.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            context.ServiceVerResult.CountResult = new ServiceVerificationResult(passed, info, countResponse.StatusCode);

            return countResponse.StatusCode;
        }

        /// <summary>
        /// Verifies $skip.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public static HttpStatusCode? VerifySkip(ServiceContext context, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            if (context.ServiceVerResult.SkipResult != null)
            {
                info = context.ServiceVerResult.SkipResult.ViolationInfo;
                passed = context.ServiceVerResult.SkipResult.Passed;
                return context.ServiceVerResult.SkipResult.ResponseStatusCode;
            }

            HttpStatusCode? statusCode = null;
            passed = null;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail(string.Empty);

            var countRestrictions = AnnotationsHelper.GetCountRestrictions(
                context.MetadataDocument,
                context.VocCapabilities,
                null,
                NavigationRoughType.None,
                new List<Func<string, string, List<string>, bool?>>() { SupportiveFeatureHelper.IsEntitySetSupportSkipQuery });

            if (string.IsNullOrEmpty(countRestrictions.Item1))
            {
                detail.ErrorMessage = "The service does not contain any entity-sets which support system query $skip.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);

                return null;
            }

            var entitySet = countRestrictions.Item1;

            // Send /$count and get the Entries count of response payload.
            string url = string.Format("{0}/{1}/$count", context.ServiceBaseUri, entitySet);
            var req = WebRequest.Create(url) as HttpWebRequest;
            Response resp = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
            detail = new ExtensionRuleResultDetail(string.Empty, url, "GET", null, resp);

            if (HttpStatusCode.OK != resp.StatusCode)
            {
                detail.ErrorMessage = "The service does not support /$count query in the request URL.";
                info = new ExtensionRuleViolationInfo(new Uri(url), resp.ResponsePayload, detail);
                context.ServiceVerResult.SkipResult = new ServiceVerificationResult(passed, info);

                return null;
            }

            Random rnd = new Random();
            int skipNumber = rnd.Next(0, Convert.ToInt32(resp.ResponsePayload));
            if (DataService.serviceInstance != null) // Add for test project
            {
                skipNumber = Convert.ToInt32(resp.ResponsePayload) > 2 ? 2 : 0;
            }

            string skipUri = string.Format("{0}/{1}/?$skip={2}", context.ServiceBaseUri, entitySet, skipNumber);
            Response skipResponse = WebHelper.Get(new Uri(skipUri), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
            statusCode = skipResponse.StatusCode;
            detail = new ExtensionRuleResultDetail(string.Empty, skipUri, "GET", StringHelper.MergeHeaders(Constants.AcceptHeaderJson, context.RequestHeaders), skipResponse);

            JObject jo;
            skipResponse.ResponsePayload.TryToJObject(out jo);
            int countAfterSkip = 0;
            JsonParserHelper.GetEntitiesCountFromFeed(new Uri(skipUri), jo, context.RequestHeaders, ref countAfterSkip);

            if (statusCode == HttpStatusCode.OK)
            {
                if (Convert.ToInt32(resp.ResponsePayload) - skipNumber != countAfterSkip)
                {
                    passed = false;
                    detail.ErrorMessage = string.Format("The service does not execute an accurate result on system query option '$skip' (Actual Value: {0}, Expected Value: {1}).", countAfterSkip, Convert.ToInt32(resp.ResponsePayload) - skipNumber);
                }
                else
                {
                    passed = true;
                }
            }
            else
            {
                passed = false;
                detail.ErrorMessage = "The service does not support the system query option '$skip'.";
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
            context.ServiceVerResult.SkipResult = new ServiceVerificationResult(passed, info, statusCode);

            return statusCode;
        }

        /// <summary>
        /// Verifies system query $search.
        /// </summary>
        /// <param name="context">The service document.</param>
        /// <param name="info">The extension rule violation information.</param>
        /// <returns>Returns true if the OData service supports system query $search, otherwise false.</returns>
        public static HttpStatusCode? VerifySearch(ServiceContext context, out bool? passed, out ExtensionRuleViolationInfo info)
        {
            if (context.ServiceVerResult.SearchVerResult != null)
            {
                info = context.ServiceVerResult.SearchVerResult.ViolationInfo;
                passed = context.ServiceVerResult.SearchVerResult.Passed;
                return context.ServiceVerResult.SearchVerResult.ResponseStatusCode;
            }

            bool? flag = null;
            passed = null;
            info = null;
            ExtensionRuleResultDetail detail = new ExtensionRuleResultDetail();

            var payloadFormat = context.ServiceDocument.GetFormatFromPayload();
            string[] entitySetUrls = ContextHelper.GetFeeds(context.ServiceDocument, payloadFormat).ToArray();
            if (null == entitySetUrls || !entitySetUrls.Any())
            {
                detail.ErrorMessage = "The service does not have any feed returned.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
                context.ServiceVerResult.SearchVerResult = new ServiceVerificationResult(passed, info);

                return null;
            }

            string entitySetName = entitySetUrls.First().MapEntitySetURLToEntitySetName();
            string entityTypeShortName = entitySetName.MapEntitySetNameToEntityTypeShortName();
            var props = MetadataHelper.GetPropertiesWithSpecifiedTypeFromEntityType(entityTypeShortName, context.MetadataDocument, new List<string>() { PrimitiveDataTypes.String });
            if (props == null || !props.Any())
            {
                detail.ErrorMessage = "Cannot find an appropriate primitive properties of entity type in the service.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
                context.ServiceVerResult.SearchVerResult = new ServiceVerificationResult(passed, info);

                return null;
            }

            string url = string.Format("{0}/{1}", context.DestinationBasePath, entitySetName);
            var req = WebRequest.Create(url) as HttpWebRequest;
            var resp = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
            detail = new ExtensionRuleResultDetail(string.Empty, url, "GET", string.Empty, resp);
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                passed = false;
                detail.ErrorMessage = "The service does not have any entity set returned.";
                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
                context.ServiceVerResult.SearchVerResult = new ServiceVerificationResult(passed, info);

                return null;
            }

            string prop = props.First();
            string propName = prop.Split(',')[0];
            JObject feed1;
            resp.ResponsePayload.TryToJObject(out feed1);

            if (feed1 != null && JTokenType.Object == feed1.Type)
            {
                var entity = JsonParserHelper.GetEntries(feed1).First;
                string searchVal = entity[propName].ToString().Contains(" ") ? string.Format("\"{0}\"", entity[propName].ToString()) : entity[propName].ToString();
                url = string.Format("{0}/{1}?$search={2}", context.DestinationBasePath, entitySetName, searchVal);
                req = WebRequest.Create(url) as HttpWebRequest;
                resp = WebHelper.Get(req, RuleEngineSetting.Instance().DefaultMaximumPayloadSize);
                detail = new ExtensionRuleResultDetail(string.Empty, url, "GET", string.Empty, resp);

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    JObject feed2;
                    resp.ResponsePayload.TryToJObject(out feed2);

                    if (feed2 != null && JTokenType.Object == feed2.Type)
                    {
                        var entities = JsonParserHelper.GetEntries(feed2).ToList();

                        if (null == entities || !entities.Any())
                        {
                            passed = false;
                            detail.ErrorMessage = "The service does not return an expected value for $search system query options.";
                        }

                        foreach (var en in entities)
                        {
                            var jProps = en.Children<JProperty>();
                            flag = null;

                            foreach (var jProp in jProps)
                            {
                                if (jProp.Value.ToString() == searchVal.StripOffDoubleQuotes())
                                {
                                    flag = true;
                                    break;
                                }
                            }

                            if (flag == null)
                            {
                                passed = false;
                                detail.ErrorMessage = "The service does not execute an accurate result on the '$search' system query option.";
                                break;
                            }
                            else
                            {
                                passed = true;
                            }
                        }
                    }
                }
                else if (resp.StatusCode != HttpStatusCode.NotImplemented)
                {
                    passed = false;
                    detail.ErrorMessage = "The service should support the '$search' system query option or return 501 Not Implemented.";
                }

                info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, detail);
                context.ServiceVerResult.SearchVerResult = new ServiceVerificationResult(passed, info, resp != null ? resp.StatusCode : null);

                return resp != null ? resp.StatusCode : null;
            }

            context.ServiceVerResult.SearchVerResult = new ServiceVerificationResult(passed, info);
            return null;
        }

        /// <summary>
        /// Get whether the individual property is a single primitive type.
        /// </summary>
        /// <param name="responsePayload">the string of the response payload.</param>
        /// <param name="payloadType">the type of the response payload.</param>
        /// <returns>True, if the the individual property is a single primitive type; false otherwise. </returns>
        public static bool IsIndividualPropertySinglePrimitiveType(
            string responsePayload, PayloadType payloadType)
        {
            if (payloadType != PayloadType.IndividualProperty || string.IsNullOrEmpty(responsePayload))
                return false;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responsePayload);
            XmlElement root = xmlDoc.DocumentElement;

            if (root.LocalName.Equals("value") && root.NamespaceURI.Equals(Constants.NSMetadata))
            {
                if (root.Attributes["context", Constants.NSMetadata] != null)
                {
                    string contextURI = root.Attributes["context", Constants.NSMetadata].Value;
                    string propertyType = contextURI.Remove(0, contextURI.IndexOf('#') + 1);

                    if (propertyType.Contains("Collection("))
                        return false;
                }

                if (root.Attributes["type", Constants.NSMetadata] != null)
                {
                    string typeName = root.Attributes["type", Constants.NSMetadata].Value;
                    if (!typeName.Contains("."))
                    {
                        typeName = "Edm." + typeName;
                    }

                    if (EdmTypeManager.IsEdmSimpleType(typeName))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove all nullable details.
        /// </summary>
        /// <param name="details">All the details.</param>
        /// <returns>Returns all the non-null details.</returns>
        public static List<ExtensionRuleResultDetail> RemoveNullableDetails(this IEnumerable<ExtensionRuleResultDetail> details)
        {
            return details
                .Where(d => null != d.HTTPMethod && null != d.URI && null != d.ResponseStatusCode || null != d.ErrorMessage)
                .Select(d => d)
                .ToList();
        }

        #region Verify the format information for CSDL rules.
        /// <summary>
        /// Verify whether the string is a SimpleIdentifier value or not.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <returns>Returns the verification result.</returns>
        public static bool IsSimpleIdentifier(this string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                return false;
            }

            if (target.Length <= 128)
            {
                string pattern = @"[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}]{0,}";
                Regex regex = new Regex(pattern);

                return regex.Match(target).Value == target;
            }
            else
            {
                // If the length of target is greater than 128, it is not a SimpleIdentifier value.
                return false;
            }
        }

        /// <summary>
        /// Verify whether the string is a TypeName value or not.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <returns>Returns the verification result.</returns>
        public static bool IsTypeName(this string target)
        {
            // The definition of qualifiedTypeName is as follows:
            // singleQualifiedTypeName = qualifiedEntityTypeName 
            // / qualifiedComplexTypeName
            // / qualifiedTypeDefinitionName
            // / qualifiedEnumTypeName
            // / primitiveTypeName 

            // qualifiedTypeName = singleQualifiedTypeName                  
            // / 'Collection' OPEN singleQualifiedTypeName CLOSE
            if (string.IsNullOrEmpty(target))
            {
                return false;
            }

            string typeFullName = target.RemoveCollectionFlag();
            if (typeFullName.StartsWith("Edm."))
            {
                return true;
            }

            string typeShortName = typeFullName.GetLastSegment();
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format(
                "//*[(local-name()='EntityType' or local-name()='ComplexType' or local-name()='TypeDefinition' or local-name()='EnumType') and @Name='{0}']",
                typeShortName);
            var typeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != typeElem)
            {
                var aliasAndNamespace = typeElem.GetAliasAndNamespace();

                return string.Format("{0}.{1}", aliasAndNamespace.Alias, typeElem.GetAttributeValue("Name")) == typeFullName ||
                    string.Format("{0}.{1}", aliasAndNamespace.Namespace, typeElem.GetAttributeValue("Name")) == typeFullName;
            }

            return false;
        }

        /// <summary>
        /// Verify the target string is QualifiedName.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <returns>Returns the verification result.</returns>
        public static bool IsQualifiedName(this string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                return false;
            }

            if (target.StartsWith("Edm."))
            {
                return true;
            }

            string name = target.GetLastSegment();
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = "//*[local-name()='Schema']/*";
            var elems = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            if (null != elems && elems.Any())
            {
                foreach (var elem in elems)
                {
                    var aliasAndNamespace = elem.GetAliasAndNamespace();
                    if (null != elem.Attribute("Name"))
                    {
                        string nameAttribVal = elem.GetAttributeValue("Name");
                        if (target == string.Format("{0}.{1}", aliasAndNamespace.Alias, nameAttribVal) ||
                            target == string.Format("{0}.{1}", aliasAndNamespace.Namespace, nameAttribVal))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Verify the target string is a type cast segment.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <returns>Returns the verification result.</returns>
        public static bool IsTypeCastSegment(this string target)
        {
            if (!target.IsQualifiedName())
            {
                return false;
            }

            string typeShortName = target.GetLastSegment();
            string aliasOrNamespace = target.RemoveEnd("." + typeShortName);
            string xPath = string.Format("//*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{0}']", typeShortName);
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            var entityTypeElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != entityTypeElem)
            {
                var aliasAndNamespace = entityTypeElem.GetAliasAndNamespace();

                return aliasAndNamespace.Alias == aliasOrNamespace || aliasAndNamespace.Namespace == aliasOrNamespace;
            }

            return false;
        }

        /// <summary>
        /// Verify the target string is a target path.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <returns>Returns the verification result.</returns>
        public static bool IsTargetPath(this string target)
        {
            if (string.IsNullOrEmpty(target) && !target.Contains('/'))
            {
                return false;
            }

            string[] seperations = target.Split('/');
            if (seperations.Length < 2)
            {
                return false;
            }

            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Empty;
            string entityContainerShortName = string.Empty;
            string @namespace = string.Empty;
            if (!string.IsNullOrEmpty(seperations[0]) && seperations[0].IsQualifiedName())
            {
                entityContainerShortName = seperations[0].GetLastSegment();
                @namespace = seperations[0].RemoveEnd("." + entityContainerShortName);
                xPath = string.Format("//*[local-name()='Schema' and (@Alias='{0}' or @Namespace='{0}')]/*[local-name()='EntityContainer' and @Name='{1}']", @namespace, entityContainerShortName);
                var entityContainerElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                if (null == entityContainerElem)
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(seperations[1]))
            {
                xPath += "/*";
                var childElemNames = metadata.XPathSelectElements(xPath, ODataNamespaceManager.Instance)
                    .Where(elem => null != elem.Attribute("Name"))
                    .Select(elem => elem.GetAttributeValue("Name"));
                if (!childElemNames.Contains(seperations[1]))
                {
                    return false;
                }
            }

            xPath += string.Format("[(local-name()='EntitySet' or local-name()='Singleton') and @Name='{0}']", seperations[1]);
            var element = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null == element)
            {
                return false;
            }

            string entityTypeFullName = "EntitySet" == element.Name.LocalName ?
                seperations[1].MapEntitySetNameToEntityTypeFullName() : seperations[1].MapSingletonNameToEntityTypeFullName();
            var stack = new Stack<string>();
            for (int i = seperations.Length - 1; i >= 2; i--)
            {
                stack.Push(seperations[i]);
            }

            if (!stack.Any())
            {
                return true;
            }
            else
            {
                return VerificationHelper.HandleTargetPathSegments(entityTypeFullName, ref stack);
            }
        }

        /// <summary>
        /// Handle all the target path segments.
        /// </summary>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="segments">All the segments store in a stack.</param>
        /// <returns>Return the verification result.</returns>
        public static bool HandleTargetPathSegments(string typeFullName, ref Stack<string> segments)
        {
            if (VerificationHelper.StartsWithProperty(typeFullName, ref segments))
            {
                return true;
            }
            else
            {
                if (VerificationHelper.StartsWithNavigationProperty(typeFullName, ref segments))
                {
                    return true;
                }
                else
                {
                    if (VerificationHelper.StartsWithTypeCastSegment(typeFullName, ref segments))
                    {
                        return true;
                    }
                }

            }

            return false;
        }

        /// <summary>
        /// Handle the property segment.
        /// </summary>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="segments">All the segments store in a stack.</param>
        /// <returns>Returns the verification result.</returns>
        public static bool StartsWithProperty(string typeFullName, ref Stack<string> segments)
        {
            if (!typeFullName.IsSpecifiedEntityTypeFullNameExist() || null == segments || !segments.Any())
            {
                return false;
            }

            string typeShortName = typeFullName.GetLastSegment();
            string @namespace = typeFullName.RemoveEnd("." + typeShortName);
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format(
                "//*[local-name()='Schema' and @Namespace='{0}']/*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{1}']/*[local-name()='Property' and @Name='{2}']",
                @namespace, typeShortName, segments.Peek());
            var propElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != propElem)
            {
                segments.Pop();
                if (!segments.Any())
                {
                    return true;
                }
                else
                {
                    if (null != propElem.Attribute("Type"))
                    {
                        string propertyType = propElem.GetAttributeValue("Type");
                        //if (propertyType.StartsWith("Collection("))
                        //{
                        //    return false;
                        //}

                        return VerificationHelper.HandleTargetPathSegments(propertyType.RemoveCollectionFlag(), ref segments);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Handle the navigation property segment.
        /// </summary>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="segments">All the segments store in a stack.</param>
        /// <returns>Returns the verification result.</returns>
        public static bool StartsWithNavigationProperty(string typeFullName, ref Stack<string> segments)
        {
            if (!typeFullName.IsSpecifiedEntityTypeFullNameExist() || null == segments || !segments.Any())
            {
                return false;
            }

            string typeShortName = typeFullName.GetLastSegment();
            string @namespace = typeFullName.RemoveEnd("." + typeShortName);
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            string xPath = string.Format(
                "//*[local-name()='Schema' and @Namespace='{0}']/*[(local-name()='EntityType' or local-name()='ComplexType') and @Name='{1}']/*[local-name()='NavigationProperty' and @Name='{2}']",
                @namespace, typeShortName, segments.Peek());
            var navigPropElem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
            if (null != navigPropElem)
            {
                segments.Pop();
                if (!segments.Any())
                {
                    return true;
                }
                else
                {
                    if (null != navigPropElem.Attribute("Type"))
                    {
                        string navigPropType = navigPropElem.GetAttributeValue("Type");
                        //if (navigPropType.StartsWith("Collection("))
                        //{
                        //    return false;
                        //}

                        return VerificationHelper.HandleTargetPathSegments(navigPropType.RemoveCollectionFlag(), ref segments);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Handle the type cast segment.
        /// </summary>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="segments">All the segments store in a stack.</param>
        /// <returns>Returns the verification result.</returns>
        public static bool StartsWithTypeCastSegment(string typeFullName, ref Stack<string> segments)
        {
            if (!typeFullName.IsSpecifiedEntityTypeFullNameExist() || null == segments || !segments.Any())
            {
                return false;
            }

            string typeCastSegment = segments.Peek();
            if (!typeCastSegment.IsTypeCastSegment())
            {
                return false;
            }

            List<string> derivedTypeFullNames = typeFullName.GetDerivedTypeFullNames();
            if(null != derivedTypeFullNames && derivedTypeFullNames.Any())
            {
                if (derivedTypeFullNames.Contains(typeCastSegment))
                {
                    segments.Pop();
                    if (!segments.Any())
                    {
                        return false;
                    }
                    else
                    {
                        return VerificationHelper.HandleTargetPathSegments(typeCastSegment, ref segments);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get the full names of derived types through a base type.
        /// </summary>
        /// <param name="typeFullName">The full name of the base type.</param>
        /// <returns>Returns the type full names' list.</returns>
        public static List<string> GetDerivedTypeFullNames(this string typeFullName)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(typeFullName))
            {
                return result;
            }
            
            string typeShortName = typeFullName.GetLastSegment();
            string @namespace = typeFullName.RemoveEnd("." + typeShortName);
            var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
            while (true)
            {
                string xPath = string.Format("//*[local-name()='Schema' and @Namespace='{0}']/*[(local-name()='EntityType' or local-name()=-'ComplexType') and @BaseType='{1}']", @namespace, typeFullName);
                var elem = metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance);
                if (null != elem && null != elem.Attribute("Name"))
                {
                    typeShortName = elem.GetAttributeValue("Name");
                    typeFullName = @namespace + "." + typeShortName;
                    result.Add(typeFullName);

                    continue;
                }

                break;
            }

            return result;
        }

        /// <summary>
        /// Verify the referential constraint property's value.
        /// </summary>
        /// <param name="propName">The property's name.</param>
        /// <param name="entityTypeElem">The element of entity type in metadata.</param>
        /// <returns>Returns true or false.</returns>
        public static bool VerifyReferentialConstraintPropertyVal(this string propName, XElement entityTypeElem)
        {
            string xPath = string.Format("./*[local-name()='Property']", propName);
            var propElems = entityTypeElem.XPathSelectElements(xPath, ODataNamespaceManager.Instance);
            var primPropElems = propElems
                .Where(p => null != p.Attribute("Type") && p.GetAttributeValue("Type").RemoveCollectionFlag().StartsWith("Edm."))
                .Select(p => p);
            foreach (var prop in primPropElems)
            {
                if (null != prop.Attribute("Name") && prop.GetAttributeValue("Name") == propName)
                {
                    return true;
                }
            }

            var compPropElems = propElems
                .Where(p => null != p.Attribute("Type") && !p.GetAttributeValue("Type").RemoveCollectionFlag().StartsWith("Edm."))
                .Select(p => p);
            foreach (var prop in compPropElems)
            {
                var metadata = XElement.Parse(ServiceStatus.GetInstance().MetadataDocument);
                string complexTypeShortName = prop.GetAttributeValue("Type").RemoveCollectionFlag().GetLastSegment();
                xPath = string.Format("//*[local-name()='ComplexType' and @Name='{0}']", complexTypeShortName);
                var cPropElem = (metadata.XPathSelectElement(xPath, ODataNamespaceManager.Instance).Nodes() as List<XElement>)
                    .Where(p => null != p.Attribute("Name") && p.GetAttributeValue("Name") == propName)
                    .Select(p => p);
                if (null != cPropElem && cPropElem.Any())
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Verify the sequence of two entities.
        /// </summary>
        /// <param name="setOfSortedEntities">The second entity which has been sorted.</param>
        /// <param name="sortedPropName">The property name which will be used to sort the collection of entities.</param>
        /// <param name="sortedPropType">The property type which will be used to sort the collection of entities.</param>
        /// <param name="sortedType">The sorted type (asc or desc).</param>
        /// <returns>Returns the result.</returns>
        private static bool? VerifySortedEntitiesSequence(JObject setOfSortedEntities, string sortedPropName, string sortedPropType, SortedType sortedType)
        {
            if (null == setOfSortedEntities || string.IsNullOrEmpty(sortedPropName) || string.IsNullOrEmpty(sortedPropType))
            {
                return null;
            }

            List<JToken> entities1 = JsonParserHelper.GetEntries(setOfSortedEntities).ToList();
            entities1.Sort(new JTokenCompare(sortedPropName, sortedPropType, sortedType));

            List<JToken> entities2 = JsonParserHelper.GetEntries(setOfSortedEntities).ToList();

            if (entities1.Count == 0 || entities2.Count == 0 || entities1.Count != entities2.Count)
            {
                return null;
            }

            return entities1.SequenceEquals(entities2);
        }
    }
}
