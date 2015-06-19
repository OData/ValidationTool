// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Xml;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for SvcDoc.Core.4625
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class SvcDocCore4625 : ExtensionRule
    {
        /// <summary>
        /// Gets Category property
        /// </summary>
        public override string Category
        {
            get
            {
                return "core";
            }
        }

        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "SvcDoc.Core.4625";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"If the href attribute of a metadata:singleton element does not contain a relative url, the metadata:name attribute MUST be specified and MUST contain the name of the singleton.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "5.5.2";
            }
        }

        /// <summary>
        /// Gets location of help information of the rule
        /// </summary>
        public override string HelpLink
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the error message for validation failure
        /// </summary>
        public override string ErrorMessage
        {
            get
            {
                return this.Description;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V3_V4;
            }
        }

        /// <summary>
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.ServiceDoc;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }

        /// <summary>
        /// Gets the IsOfflineContext property to which the rule applies.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the flag whether the rule requires metadata document
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }       

        /// <summary>
        /// Verify SvcDoc.Core.4625
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out parameter to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            XmlDocument xmlDoc = new XmlDocument();
//            xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
//<service xml:base=""http://services.odata.org/V4/OData/OData.svc/"" xmlns=""http://www.w3.org/2007/app"" xmlns:atom=""http://www.w3.org/2005/Atom"" xmlns:m=""http://docs.oasis-open.org/odata/ns/metadata"" m:context=""http://services.odata.org/V4/OData/OData.svc/$metadata"">
//  <workspace>
//    <atom:title type=""text"">Default</atom:title>
//    <collection href=""Products"">
//      <atom:title type=""text"">Products</atom:title>
//    </collection>
//    <collection href=""ProductDetails"">
//      <atom:title type=""text"">ProductDetails</atom:title>
//    </collection>
//    <collection href=""Categories"">
//      <atom:title type=""text"">Categories</atom:title>
//    </collection>
//    <collection href=""Suppliers"">
//      <atom:title type=""text"">Suppliers</atom:title>
//    </collection>
//    <collection href=""Persons"">
//      <atom:title type=""text"">Persons</atom:title>
//    </collection>
//    <collection href=""PersonDetails"">
//      <atom:title type=""text"">PersonDetails</atom:title>
//    </collection>
//    <collection href=""Advertisements"">
//      <atom:title type=""text"">Advertisements</atom:title>
//    </collection>
//	<m:function-import href=""TopProducts"" m:name=""TopProducts"">
//      <atom:title>Best-Selling Products</atom:title>
//    </m:function-import>
//	<m:singleton href=""http://ODatademo.Contoso"" m:name=""Contoso"">
//      <atom:title>Contoso Ltd.</atom:title>
//    </m:singleton>
//  </workspace>
//</service>");
            xmlDoc.LoadXml(context.ResponsePayload);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("app", Constants.NSApp);
            nsmgr.AddNamespace("metadata", Constants.NSMetadata);
            XmlNodeList nodes = xmlDoc.SelectNodes(@"//app:workspace/metadata:singleton", nsmgr);

            if (nodes.Count > 0)
            {
                string xpath = @"//*[local-name()='EntityContainer']/*[local-name()='Singleton']";
                List<string> singletonNames = Helper.MetadataHelper.GetPropertyValues(context, xpath, "Name");

                foreach (XmlNode node in nodes)
                {
                    if (!Uri.IsWellFormedUriString(node.Attributes["href"].Value, UriKind.Relative))
                    {
                        if (node.Attributes["name", Constants.NSMetadata] != null && singletonNames.Contains(node.Attributes["name", Constants.NSMetadata].Value))
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
                            break;
                        }
                    }                 
                }              
            }        
           
            return passed;
        }
    }
}
