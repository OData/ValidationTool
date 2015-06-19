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
    /// Class of extension rule for SvcDoc.Core.4612
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class SvcDocCore4612 : ExtensionRule
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
                return "SvcDoc.Core.4612";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The metadata:name attribute in app:collection element MUST contain the name of the entity set.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "5.3.2";
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
        /// Verify SvcDoc.Core.4612
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
            //xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
//<service xml:base=""http://services.odata.org/V4/OData/OData.svc/"" xmlns=""http://www.w3.org/2007/app"" xmlns:atom=""http://www.w3.org/2005/Atom"" xmlns:m=""http://docs.oasis-open.org/odata/ns/metadata"" m:context=""http://services.odata.org/V4/OData/OData.svc/$metadata"">
//  <workspace>
//    <atom:title type=""text"">Default</atom:title>
//    <collection href=""Products"" m:name=""Products"">
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
//  </workspace>
//</service>");
            xmlDoc.LoadXml(context.ResponsePayload);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("app", Constants.NSApp);
            nsmgr.AddNamespace("metadata", Constants.NSMetadata);
            XmlNodeList nameNodes = xmlDoc.SelectNodes(@"//app:workspace/app:collection/@metadata:name", nsmgr);

            if (nameNodes.Count > 0)
            {
                string xpath = @"//*[local-name()='EntityContainer']/*[local-name()='EntitySet']";
                List<string> entitysetNames = Helper.MetadataHelper.GetPropertyValues(context, xpath, "Name");

                foreach (XmlNode node in nameNodes)
                {
                    if (entitysetNames.Contains(node.Value))
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
           
            return passed;
        }
    }
}
