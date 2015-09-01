// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of service implemenation feature to call an ActionImport.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_ActionImport : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_ActionImport";
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",ActionImport";
            }
        }

        /// <summary>
        /// Gets the service implementation feature specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.5.4.1";
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
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                return new ServiceImplCategory(ServiceImplCategoryName.Operations, null);
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
            info = null;
            var svcStatus = ServiceStatus.GetInstance();

            string url = svcStatus.RootURL.TrimEnd('/');
            var detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, string.Empty);

            JObject serviceJO;
            svcStatus.ServiceDocument.TryToJObject(out serviceJO);

            JArray serviceOA = JsonParserHelper.GetEntries(serviceJO);

            string xpath = string.Format(@"//*[local-name()='ActionImport']");
            XElement md = XElement.Parse(svcStatus.MetadataDocument);
            XElement actionImport = md.XPathSelectElement(xpath, ODataNamespaceManager.Instance);

            if (actionImport == null)
            {
                detail.ErrorMessage = "The service has no action Import.";
                info = new ExtensionRuleViolationInfo(new Uri(url), string.Empty, detail);
                return passed;
            }

            string actionShortName = actionImport.Attribute("Action").Value.GetLastSegment();

            XElement action = md.XPathSelectElement(string.Format(@"//*[local-name()='Action' and @Name='{0}' and not (@IsBound='true')]", actionShortName), ODataNamespaceManager.Instance);

            JObject parameterEntity = new JObject();
            List<KeyValuePair<string, string>> paralist = new List<KeyValuePair<string, string>>();

            if (action != null)
            {
                IEnumerable<XElement> parameters = action.XPathSelectElements(@"./*[local-name()='Parameter' and @Nullable='false']");
                foreach (XElement pa in parameters)
                {
                    string value = string.Empty;

                    if (EdmTypeManager.IsEdmSimpleType(pa.Attribute("Type").Value))
                    {
                        IEdmType typeInterface = EdmTypeManager.GetEdmType(pa.Attribute("Type").Value);
                        if (typeInterface != null)
                        {
                            value = typeInterface.GetXmlValueTemplate();
                        }
                    }

                    if (!string.IsNullOrEmpty(value))
                    {
                        KeyValuePair<string, string> kv = new KeyValuePair<string, string>(pa.Attribute("Name").Value, value);
                        paralist.Add(kv);
                    }
                    else
                    {
                        detail.ErrorMessage = "Parameter type is not supported by this test.";
                        info = new ExtensionRuleViolationInfo(new Uri(url), string.Empty, detail);
                        return passed;
                    }
                }
            }

            for (int i = 0; i < paralist.Count; i++)
            {
                parameterEntity.Add(paralist[i].Key, paralist[i].Value);
            }

            url += @"/" + actionShortName;

            Response response = WebHelper.CreateEntity(url, parameterEntity.ToString(), context.RequestHeaders);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.NoContent)
            {
                detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty);
                passed = true;
            }
            else
            {
                passed = false;
            }

            info = new ExtensionRuleViolationInfo(new Uri(url), string.Empty, detail);
            return passed;
        }
    }
}
