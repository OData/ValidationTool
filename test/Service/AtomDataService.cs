// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Protocols.TestSuites.Validator
{
    using System;
    using System.Net;
    using Microsoft.Protocols.TestTools;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The ATOM Data Service
    /// </summary>
    public class AtomDataService : IDataService
    {
        /// <summary>
        /// The ITestSite object which is used to get PTF configuration value.
        /// </summary>
        private static ITestSite site;

        /// <summary>
        /// The mapping between URL and file name.
        /// </summary>
        private Dictionary<string, string> URL2FileName;

        /// <summary>
        /// The name of the rule.
        /// </summary>
        private string RuleName;

        public AtomDataService()
        {
            InitUrlFileDict();
        }

        public static void SetTestSite(ITestSite testSite)
        {
            site = testSite;
        }

        public void InitUrlFileDict()
        {
            URL2FileName = new Dictionary<string, string>();

            URL2FileName[AtomDataSvc_URLConstants.URL_ServiceDocument.ToLower()] = "ODataROServiceDocument";
            URL2FileName[AtomDataSvc_URLConstants.URL_ServiceDocumentWithXMLFormat.ToLower()] = "ODataROServiceDocument";
            URL2FileName[AtomDataSvc_URLConstants.URL_ServiceDocumentWithAtomAbbrFormat.ToLower()] = "ODataROServiceDocument";
            URL2FileName[AtomDataSvc_URLConstants.URL_ServiceDocumentWithAtomFormat.ToLower()] = "ODataROServiceDocument";
            URL2FileName[AtomDataSvc_URLConstants.URL_Metadata.ToLower()] = "ODataROMetadata";

            URL2FileName[AtomDataSvc_URLConstants.URL_Entity_Product_ExpandAll.ToLower()] = "ODataRO_ODataDemo_Entity_Product_ExpandAll";
            URL2FileName[AtomDataSvc_URLConstants.URL_Entity_Product.ToLower()] = "ODataRO_ODataDemo_Entity_Product";
            URL2FileName[AtomDataSvc_URLConstants.URL_Entity_Product_AtomFormat.ToLower()] = "ODataRO_ODataDemo_Entity_Product";
            URL2FileName[AtomDataSvc_URLConstants.URL_Entity_Product_XmlAbbrFormat.ToLower()] = "ODataRO_ODataDemo_Entity_Product_XmlFormat";
            URL2FileName[AtomDataSvc_URLConstants.URL_Entity_Product_XmlFormat.ToLower()] = "ODataRO_ODataDemo_Entity_Product_XmlFormat";

            URL2FileName[TripPinSvc_URLConstants.URL_Metadata.ToLower()] = "ODataRO_TripPin_Metadata";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_AtomAbbrFormat.ToLower()] = "ODataRO_TripPin_Entity_Person";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_Photos_AtomAbbrFormat.ToLower()] = "ODataRO_TripPin_Entity_Photo";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_Airports_AtomAbbrFormat.ToLower()] = "ODataRO_TripPin_Entity_Airport";

            URL2FileName[AtomDataSvc_URLConstants.URL_IndividualProperty_PrimitiveString.ToLower()] = "ODataROProduct0Name_IP";
            URL2FileName[AtomDataSvc_URLConstants.URL_IndividualProperty_PrimitiveStringWithAtomAbbrFormat.ToLower()] = "ODataROProduct0Name_IP";
            URL2FileName[AtomDataSvc_URLConstants.URL_IndividualProperty_PrimitiveStringWithAtomFormat.ToLower()] = "ODataROProduct0Name_IP";
            URL2FileName[AtomDataSvc_URLConstants.URL_IndividualProperty_PrimitiveStringWithXmlFormat.ToLower()] = "ODataROProduct0Name_IP";
            URL2FileName[AtomDataSvc_URLConstants.URL_IndividualProperty_NullPrimitiveString.ToLower()] = "ODataROProduct0DiscontinuedDate_IP";
            URL2FileName[AtomDataSvc_URLConstants.URL_IndividualProperty_PrimitiveNonString.ToLower()] = "ODataROProduct0ReleaseDate_IP";
            URL2FileName[AtomDataSvc_URLConstants.URL_IndividualProperty_CollectionPrimitive.ToLower()] = "ODataROSupplier0Tels_IP";
            URL2FileName[AtomDataSvc_URLConstants.URL_IndividualProperty_CollectionDerivedComplex.ToLower()] = "ODataROAllAddresses_IP";

            URL2FileName[AtomDataSvc_URLConstants.URL_EntitySet_Products.ToLower()] = "ODataROProducts";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntitySet_ProductsWithAtomFormat.ToLower()] = "ODataROProducts";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntitySet_ProductsWithXmlFormat.ToLower()] = "ODataROProducts";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntitySet_ProductsWithXmlAbbrFormat.ToLower()] = "ODataROProducts";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntitySet_ProductsSkip.ToLower()] = "ODataROProductsSkip";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntitySet_ProductsCount.ToLower()] = "ODataROProductsCount";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntitySet_ProductsDelta.ToLower()] = "Delta";

            URL2FileName[AtomDataSvc_URLConstants.URL_EntityReferenceSingle.ToLower()] = "ODataROProductSupplierRef";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntityReferenceSingleWithAtomFormat.ToLower()] = "ODataROProductSupplierRef";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntityReferenceSingleWithAtomAbbrFormat.ToLower()] = "ODataROProductSupplierRef";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntityReferenceCollectionWithAtomFormat.ToLower()] = "ODataROProductCategoriesRefs";
            URL2FileName[AtomDataSvc_URLConstants.URL_EntityReferenceCollection.ToLower()] = "ODataROProductCategoriesRefs";

            URL2FileName[AtomDataSvc_URLConstants.URL_Error.ToLower()] = "Error";
            URL2FileName[AtomDataSvc_URLConstants.URL_ErrorWithAtomFormat.ToLower()] = "Error";
            //TODO ADD  
        }

        public void HandleRequest(WebRequest request, out HttpStatusCode? statusCode, out string responseHeaders, out string responsePayload)
        {
            statusCode = null;
            responseHeaders = string.Empty;
            responsePayload = string.Empty;

            var reqHttp = request as HttpWebRequest;
            string filePath = string.Empty;
            string filename = string.Empty;
            string requestURL = request.RequestUri.AbsoluteUri.ToLower().TrimEnd('/');

            try
            {
                if (URL2FileName.ContainsKey(requestURL))
                {
                    filename = this.URL2FileName[requestURL];

                    filePath = site.Properties["Atom_Response_Data_Path"] + filename + ".txt";
                }
                else
                {
                    if (!requestURL.StartsWith(URL_SrvDocConstants.URL_SrvDoc_OData))
                    {
                        using (WebResponse resp = request.GetResponse())
                        {
                            statusCode = WebHelper.ParseResponse(RuleEngineSetting.Instance().DefaultMaximumPayloadSize, resp, out responseHeaders, out responsePayload);
                        }
                    }
                    return;
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        string txtFileContent = Utility.ReadFile(filePath);
                        string[] lines = txtFileContent.Split(new string[] { "\r\n\r\n" }, 2, StringSplitOptions.None);

                        responseHeaders = lines[0];
                        responsePayload = lines[1];
                        statusCode = HttpStatusCode.OK;

                        if (filename.Equals("Error"))
                        {
                            statusCode = HttpStatusCode.InternalServerError;
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Read file '{0}' gets exception: {1}!", filePath, ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Handle Request '{0}' gets exception: {1}.", request.RequestUri.AbsoluteUri, ex.Message);
            }
            finally
            {
                try
                {
                    reqHttp.Abort();
                }
                catch (Exception reqEx)
                {
                    System.Diagnostics.Debug.WriteLine("Abort Request '{0}' gets exception: {1}.", request.RequestUri.AbsoluteUri, reqEx.Message);
                }
            }
        }

        public string GetExtensionStorePath()
        {
            return site.Properties["ExtensionStorePath"];
        }

        public string GetRulestorePath()
        {
            return site.Properties["RulestorePath"];
        }

        public string GetConnectionString()
        {
            return site.Properties["ConnectionString"];
        }

        public void SwitchRule(string ruleName)
        {
            this.RuleName = ruleName;
        }
    }
}
