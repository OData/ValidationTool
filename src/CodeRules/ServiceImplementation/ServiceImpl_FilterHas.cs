// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Xml;
    #endregion

    /// <summary>
    /// Class of service implemenation feature to query an entity with filter Has.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_FilterHas : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_FilterHas";
            }
        }

        /// <summary>
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                return new ServiceImplCategory(ServiceImplCategoryName.LogicalOperators,new ServiceImplCategory(ServiceImplCategoryName.SystemQueryOption,new ServiceImplCategory(ServiceImplCategoryName.RequestingData)));
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",$filter (Has)";
            }
        }

        /// <summary>
        /// Gets the service implementation feature specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the service implementation feature level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }


        /// <summary>
        /// Verifies the service implementation feature.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if the service implementation feature passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;

            List<ExtensionRuleResultDetail> details = new List<ExtensionRuleResultDetail>();

            XmlDocument metadata = new XmlDocument();
            metadata.LoadXml(ServiceStatus.GetInstance().MetadataDocument);

            string xPath = "//*[local-name()='EnumType']";
            XmlNodeList enumTypes = metadata.SelectNodes(xPath, ODataNamespaceManager.Instance);

            foreach (XmlNode enumType in enumTypes)
            {
                xPath = "//*[local-name()='EntityType']";
                XmlNodeList entityTypes = metadata.SelectNodes(xPath, ODataNamespaceManager.Instance);

                List<string> typeNames = new List<string> {enumType.ParentNode.Attributes["Namespace"].Value+"."+ enumType.Attributes["Name"].Value };

                if (enumType.ParentNode.Attributes["Alias"] != null)
                {
                    typeNames.Add(enumType.ParentNode.Attributes["Alias"].Value + "." + enumType.Attributes["Name"].Value);
                }
                List<string> properties = null;
                foreach(XmlNode entityType in entityTypes)
                {
                    properties = MetadataHelper.GetPropertiesWithSpecifiedTypeFromEntityType(entityType.Attributes["Name"].Value, 
                        ServiceStatus.GetInstance().MetadataDocument, typeNames);
                    if (properties == null || properties.Count < 1)
                    {
                        continue;
                    }

                    string entitySetUrl = entityType.Attributes["Name"].Value.MapEntityTypeShortNameToEntitySetName();
                    
                    Response resp = WebHelper.Get(new Uri(context.ServiceBaseUri + "/" + entitySetUrl), Constants.AcceptHeaderJson, 
                        RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);

                    if (null == resp || HttpStatusCode.OK != resp.StatusCode)
                    {
                        continue;
                    }

                    JObject feed;
                    resp.ResponsePayload.TryToJObject(out feed);

                    if (feed == null || JTokenType.Object != feed.Type)
                    {
                        continue;
                    }

                    JArray entities = JsonParserHelper.GetEntries(feed);
                    string enumValue = entities[0][properties[0].Split(',')[0]].ToString();

                    string url = context.ServiceBaseUri + "/" + entitySetUrl + "?$filter=" + properties[0].Split(',')[0] + " has " +
                        typeNames[0] + "\'" + enumValue + "\'";

                    resp = WebHelper.Get(new Uri(url), Constants.AcceptHeaderJson, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, context.RequestHeaders);
                    details.Add(new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, ""));

                    if (null == resp || HttpStatusCode.OK != resp.StatusCode)
                    {
                        passed = false; break;
                    }

                    resp.ResponsePayload.TryToJObject(out feed);

                    if (feed == null || JTokenType.Object != feed.Type)
                    {
                        passed = false; break;
                    }

                    entities = JsonParserHelper.GetEntries(feed);

                    foreach (JToken entity in entities)
                    {
                        Dictionary<string, ushort> map = buildEnumMapping(enumType);
                        ushort expect, actual;
                        if (!mappingToUshort(map,enumValue, out expect) || !mappingToUshort(map,entity[properties[0].Split(',')[0]].ToString(), out actual))
                        {
                            passed = false; break;
                        }

                        if (enumType.Attributes["IsFlags"] != null && enumType.Attributes["IsFlags"].Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((expect & actual) != expect)
                            {
                                passed = false; break;
                            }
                        }
                        else
                        {
                            if (expect != actual)
                            {
                                passed = false; break;
                            }
                        }

                        //if (!entity[properties[0].Split(',')[0]].ToString().Equals(enumValue))
                        //{ passed = false; break; }
                    }

                    if (passed.HasValue && !passed.Value)
                    { break; }
                    else
                    { passed = true; break; }
                }

                if (passed != null)
                    break;
            }

            info = new ExtensionRuleViolationInfo(context.Destination, context.ResponsePayload, details);
            return passed;
        }

        private bool mappingToUshort(Dictionary<string, ushort> map, string enumValue, out ushort result)
        {
            if (map.TryGetValue(enumValue, out result))
            {
                return true;
            }
            else
            {
                string[] enumValueSplices = enumValue.Split(',');
                ushort temp;
                foreach (string value in enumValueSplices)
                {
                    if (map.TryGetValue(value.Trim(), out temp))
                    {
                        result |= temp;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private Dictionary<string, ushort> buildEnumMapping(XmlNode enumType)
        {
            Dictionary<string, ushort> map = new Dictionary<string, ushort>();

            foreach (XmlNode enumValue in enumType.SelectNodes("./*[local-name()='Member' and @Name and @Value]"))
            {
                map.Add(enumValue.Attributes["Name"].Value, ushort.Parse(enumValue.Attributes["Value"].Value));
            }

            return map;
        }
    }
}
