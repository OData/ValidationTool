// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespace
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using System.Net;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of service implemenation feature to delete a Stream property's value.
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class ServiceImpl_DeleteStreamPropertyValue : ServiceImplExtensionRule
    {
        /// <summary>
        /// Gets the service implementation feature name
        /// </summary>
        public override string Name
        {
            get
            {
                return "ServiceImpl_DeleteStreamPropertyValue";
            }
        }

        /// <summary>
        /// Gets the service implementation category.
        /// </summary>
        public override ServiceImplCategory CategoryInfo
        {
            get
            {
                var parent = new ServiceImplCategory(ServiceImplCategoryName.DataModification);
                return new ServiceImplCategory(ServiceImplCategoryName.ManagingStreamProperties, parent);
            }
        }

        /// <summary>
        /// Gets the service implementation feature description
        /// </summary>
        public override string Description
        {
            get
            {
                return this.CategoryInfo.CategoryFullName + ",Deleting Stream Values";
            }
        }

        /// <summary>
        /// Gets the service implementation feature specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "11.4.8.2";
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
        /// Verifies the extension rule.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            ServiceStatus serviceStatus = ServiceStatus.GetInstance();
            DataFactory dFactory = DataFactory.Instance();
            var detail = new ExtensionRuleResultDetail(this.Name, serviceStatus.RootURL, HttpMethod.Post, string.Empty);
            List<string> keyPropertyTypes = new List<string>() { "Edm.Int32", "Edm.Int16", "Edm.Int64", "Edm.Guid", "Edm.String" };
            List<EntityTypeElement> entityTypeElements = MetadataHelper.GetEntityTypes(serviceStatus.MetadataDocument, 1, keyPropertyTypes, null, NavigationRoughType.None).ToList();

            if (null == entityTypeElements || 0 == entityTypeElements.Count())
            {
                detail.ErrorMessage = "To verify this rule it expects an entity type with Int32/Int64/Int16/Guid/String key property, but there is no this entity type in metadata so can not verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);
                return passed;
            }


            EntityTypeElement eTypeElement = new EntityTypeElement();

            Dictionary<string, string> entityAndpaths = MetadataHelper.StreamPropertyEntities(serviceStatus.MetadataDocument);

            string relativePath = string.Empty;

            foreach (var en in entityTypeElements)
            {
                if (entityAndpaths.Keys.Contains(en.EntityTypeShortName) && !string.IsNullOrEmpty(eTypeElement.EntitySetName))
                {
                    eTypeElement = en;
                    relativePath = entityAndpaths[en.EntityTypeShortName];
                    break;
                }
            }

            if (eTypeElement != null)
            {
                detail.ErrorMessage = "Cannot find the appropriate entity set to verify this rule.";
                info = new ExtensionRuleViolationInfo(new Uri(serviceStatus.RootURL), serviceStatus.ServiceDocument, detail);

                return passed;
            }

            string url = serviceStatus.RootURL.TrimEnd('/') + @"/" + eTypeElement.EntitySetName;
            var additionalInfos = new List<AdditionalInfo>();
            var reqData = dFactory.ConstructInsertedEntityData(eTypeElement.EntitySetName, eTypeElement.EntityTypeShortName, null, out additionalInfos);
            string reqDataStr = reqData.ToString();
            bool isMediaType = !string.IsNullOrEmpty(additionalInfos.Last().ODataMediaEtag);
            var resp = WebHelper.CreateEntity(url, context.RequestHeaders, reqData, isMediaType, ref additionalInfos);
            detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Post, string.Empty, resp, string.Empty, reqDataStr);
            if (HttpStatusCode.Created == resp.StatusCode || HttpStatusCode.NoContent == resp.StatusCode)
            {
                url = additionalInfos.Last().EntityId.TrimEnd('/') + "/" + relativePath.TrimEnd('/');
                if(url.Equals("/")) 
                {
                    detail.ErrorMessage = string.Format("Fail to find the stream property read and edit URL.", resp.StatusCode);
                    return passed;
                }                    

                resp = WebHelper.Get(new Uri(url), null, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);
                detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Get, StringHelper.MergeHeaders(Constants.V4AcceptHeaderJsonFullMetadata, serviceStatus.DefaultHeaders), resp);
                if ((resp.StatusCode.HasValue && HttpStatusCode.OK == resp.StatusCode) || HttpStatusCode.NoContent == resp.StatusCode)
                {
                    string xpath = string.Format(@"//*[local-name()='Property' and @Name='{0}' and @Type='Edm.Stream']", url.Split('/').Last());
                    XElement md = XElement.Parse(serviceStatus.MetadataDocument);
                    XElement propElem = md.XPathSelectElement(xpath, ODataNamespaceManager.Instance);
                    
                    bool nullable = true;

                    if( propElem.Attribute("Nullable") !=null && propElem.Attribute("Nullable").Value.ToLower().Equals("false"))
                    {
                        nullable = false;
                    }

                    resp = WebHelper.DeleteEntity(url, context.RequestHeaders, additionalInfos.Last().HasEtag);
                    detail = new ExtensionRuleResultDetail(this.Name, url, HttpMethod.Delete, string.Empty, resp, string.Empty, "Successfully updated the stream of the image.");

                    if (null != resp && (HttpStatusCode.NoContent == resp.StatusCode || (nullable == false && Convert.ToInt32(resp.StatusCode) >= 400)))
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        detail.ErrorMessage = string.Format("HTTP delete to delete the stream property value to null failed with the error {0}.", resp.StatusCode);
                    }

                }
                else
                {
                    detail.ErrorMessage = "Get stream property failed from above URI.";
                }

                // Restore the service.
                var resps = WebHelper.DeleteEntities(context.RequestHeaders, additionalInfos);
            }
            else
            {
                detail.ErrorMessage = "Created the new entity failed for above URI.";
            }

            var details = new List<ExtensionRuleResultDetail>() { detail };
            info = new ExtensionRuleViolationInfo(new Uri(url), serviceStatus.ServiceDocument, details);

            return passed;
        }
    }
}
