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

    public class TripPinService : IDataService
    {
        /// <summary>
        /// The ITestSite object which is used to get ptf value.
        /// </summary>
        private static ITestSite site;
        private Dictionary<string, string> URL2FileName;
        private string RuleName;

        public TripPinService()
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

            URL2FileName[TripPinSvc_URLConstants.URL_ServiceDocument.ToLower()] = "ServiceDocument";
            URL2FileName[TripPinSvc_URLConstants.URL_ServiceDocumentWithJsonFormat.ToLower()] = "ServiceDocumentWithFormatQuery";
            URL2FileName[TripPinSvc_URLConstants.URL_Metadata.ToLower()] = "Metadata";

            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Full.ToLower()] = "Entity_People_Full";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_Photos_Full.ToLower()] = "Entity_Photos_Full";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_Airports_Full.ToLower()] = "Entity_Airports_Full";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_None.ToLower()] = "Entity_People_None";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Minimal.ToLower()] = "Entity_People_Minimal";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Minimal_WithUncomputedLink.ToLower()] = "Entity_People_Minimal_WithUncomputedLink";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_MinimalWithFormat.ToLower()] = "Entity_People_Minimal";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_AtomFormat.ToLower()] = "Entity_People_AtomFormat";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Expand_MultiEntity.ToLower()] = "Entity_People_Expand_MultiEntity";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Expand_OneEntity.ToLower()] = "Entity_People_Expand_OneEntity";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Expand_NullEntity.ToLower()] = "Entity_People_Expand_NullEntity";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Expand_NullEntity_Array.ToLower()] = "Entity_People_Expand_NullEntity_Array";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Expand_Friends.ToLower()] = "Entity_People_Expand_Friends";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Expand_Trips.ToLower()] = "Entity_People_Expand_Trips";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_Expand_Photo.ToLower()] = "Entity_People_Expand_Photo";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_IEEE754CompatibleFalse.ToLower()] = "Entity_People_Full";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_IEEE754CompatibleTrue.ToLower()] = "EntitySet_People_Full_IEEE754CompatibleTrue";
            URL2FileName[TripPinSvc_URLConstants.URL_Entity_People_FullWithAllParams.ToLower()] = "EntitySet_People_Full_IEEE754CompatibleTrue";

            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_People_Full.ToLower()] = "EntitySet_People_Full";
            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_People_Minimal.ToLower()] = "EntitySet_People_Minimal";
            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_People_IEEE754CompatibleFalse.ToLower()] = "EntitySet_People_Full";
            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_People_IEEE754CompatibleTrue.ToLower()] = "EntitySet_People_Full_IEEE754CompatibleTrue";
            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_People_JsonFormat.ToLower()] = "EntitySet_People_Minimal";
            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_People_Count.ToLower()] = "EntitySet_People_Minimal";
            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_People_Skip8.ToLower()] = "EntitySet_People_Skip8";
            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_People_Skip16.ToLower()] = "EntitySet_People_Skip16";
            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_Empty.ToLower()] = "EntitySet_Empty";
            URL2FileName[TripPinSvc_URLConstants.URL_EntitySet_Photos.ToLower()] = "EntitySet_Photos_Full";

            URL2FileName[URL_PathConstants.URL_Path_AcceptHeader_StreamingTrue.ToLower()] = "OnlyStreaming";

            URL2FileName[TripPinSvc_URLConstants.URL_IndividualProperty_Primitive.ToLower()] = "IndividualProperty_Primitive";
            URL2FileName[TripPinSvc_URLConstants.URL_IndividualProperty_Collection.ToLower()] = "IndividualProperty_Collection";
            URL2FileName[TripPinSvc_URLConstants.URL_IndividualProperty_Complex.ToLower()] = "IndividualProperty_Complex";
            URL2FileName[TripPinSvc_URLConstants.URL_EntityReference.ToLower()] = "EntityReference";
            URL2FileName[TripPinSvc_URLConstants.URL_Error.ToLower()] = "Error";
            URL2FileName[TripPinSvc_URLConstants.URL_Delta.ToLower()] = "Delta";

            URL2FileName[TripPinSvc_URLConstants.URL_IndividualPropertyWithAllParams.ToLower()] = "IndividualProperty_Primitive";
            URL2FileName[TripPinSvc_URLConstants.URL_EntityReferenceWithAllParams.ToLower()] = "EntityReference";
            URL2FileName[TripPinSvc_URLConstants.URL_ErrorWithAllParams.ToLower()] = "Error";
            URL2FileName[TripPinSvc_URLConstants.URL_DeltaWithAllParams.ToLower()] = "Delta";

            URL2FileName[ODataSvc_URLConstants.URL_Entity_Products_Full.ToLower()] = "ODataSvc_Entity_Products_Full";
            URL2FileName[ODataSvc_URLConstants.URL_Entity_PersonDetails_Full.ToLower()] = "ODataSvc_Entity_PersonDetails_Full";
            URL2FileName[ODataSvc_URLConstants.URL_Entity_FeaturedProduct_Full.ToLower()] = "ODataSvc_Entity_FeaturedProducts_Full";

            URL2FileName[ODataSvc_URLConstants.URL_Entity_Persons_Full.ToLower()] = "ODataSvc_Entity_Persons_Full";
            URL2FileName[ODataSvc_URLConstants.URL_Entity_Persons_Full_IEEE754CompatibleFalse.ToLower()] = "ODataSvc_Entity_Persons_Full";
            URL2FileName[ODataSvc_URLConstants.URL_Entity_Persons_Full_IEEE754CompatibleTrue.ToLower()] = "ODataSvc_Entity_Persons_Full_IEEE754CompatibleTrue";

            URL2FileName[ODataSvc_URLConstants.URL_EntitySet_Products_Full.ToLower()] = "ODataSvc_EntitySet_Products_Full";

            //TODO ADD  
        }

        public void HandleRequest(WebRequest request, out HttpStatusCode? statusCode, out string responseHeaders, out string responsePayload)
        {
            statusCode = null;
            responseHeaders = string.Empty;
            responsePayload = string.Empty;

            var reqHttp = request as HttpWebRequest;
            string filename = string.Empty;
            string requestURL = request.RequestUri.AbsoluteUri.ToLower().TrimEnd('/');

            try
            {
                if (!string.IsNullOrEmpty(reqHttp.Accept)
                    && reqHttp.Accept.Equals(URL_PathConstants.URL_Path_AcceptHeader_StreamingTrue))
                {
                    statusCode = HttpStatusCode.NotAcceptable;
                    filename = site.Properties["Response_Path"] + "OnlyStreaming.txt";
                }
                else if (URL2FileName.ContainsKey(requestURL))
                {
                    filename = this.URL2FileName[requestURL];

                    if (filename == "ServiceDocument"
                        && !string.IsNullOrEmpty(reqHttp.Accept)
                        && reqHttp.Accept.Contains(FormatConstants.V4FormatJsonFullMetadata))
                    {
                        filename += "_Full";
                    }
                    else if (filename == "Entity_People_Minimal"
                        && !string.IsNullOrEmpty(reqHttp.Accept)
                        && reqHttp.Accept.Contains(FormatConstants.V4FormatJsonFullMetadata))
                    {
                        filename = "Entity_People_Full";
                    }
                    else if (filename == "EntitySet_People_Minimal"
                        && !string.IsNullOrEmpty(reqHttp.Accept)
                        && reqHttp.Accept.Contains(FormatConstants.V4FormatJsonFullMetadata))
                    {
                        filename = "EntitySet_People_Full";
                    }
                    else if (filename.Contains("EntitySet")
                        && !string.IsNullOrEmpty(reqHttp.Accept)
                        && reqHttp.Accept.Contains(FormatConstants.V4FormatJsonNoMetadata))
                    {
                        filename = "EntitySet_People_None";
                    }

                    filename = site.Properties["Response_Path"] + filename + ".txt";
                }
                else
                {
                    if (!requestURL.StartsWith(URL_SrvDocConstants.URL_SrvDoc_TripPin))
                    {
                        using (WebResponse resp = request.GetResponse())
                        {
                            statusCode = WebHelper.ParseResponse(RuleEngineSetting.Instance().DefaultMaximumPayloadSize, resp, out responseHeaders, out responsePayload);
                        }
                    }
                    return;
                }

                if (!string.IsNullOrEmpty(filename))
                {
                    try
                    {
                        string txtFileContent = Utility.ReadFile(filename);
                        string[] lines = txtFileContent.Split(new string[] { "\r\n\r\n" }, 2, StringSplitOptions.None);

                        responseHeaders = lines[0];
                        responsePayload = lines[1];
                        statusCode = HttpStatusCode.OK;
                        var serviceStatus = ServiceStatus.GetInstance(requestURL);
                        if (responsePayload.IsMetadata())
                        {
                            ServiceStatus.ReviseMetadata(responsePayload);
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Read file '{0}' gets exception: {1}!", filename, ex.Message));
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
