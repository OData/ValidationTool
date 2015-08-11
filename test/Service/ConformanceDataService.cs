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
    using System.IO;
    using System.Text;
    using System.Xml.Linq;
    using System.Collections.Specialized;
    using System.Collections;

    public class ConformanceDataService : IDataService
    {
        /// <summary>
        /// The ITestSite object which is used to get ptf value.
        /// </summary>
        private static ITestSite site;
        private static int RequestTotalTime = 0;
        private string RuleName;
        private int RuleRequestNumber;
        private OrderedDictionary ruleRequestRecord;

        public static void SetTestSite(ITestSite testSite)
        {
            site = testSite;
        }

        public ConformanceDataService()
        {
            this.ruleRequestRecord = new OrderedDictionary();
            this.RuleName = string.Empty;
        }

        public IDictionaryEnumerator RequestRecords
        {
            get
            {
                return ruleRequestRecord.GetEnumerator();
            }
        }

        public void HandleRequest(WebRequest request, out HttpStatusCode? statusCode, out string responseHeaders, out string responsePayload)
        {
            statusCode = null;
            responseHeaders = string.Empty;
            responsePayload = string.Empty;
            var reqHttp = request as HttpWebRequest;

            if (!string.IsNullOrEmpty(this.RuleName))
            {
                string shorturl = request.RequestUri.AbsoluteUri.Replace(URL_SrvDocConstants.URL_SrcDoc_Conformance, "~");
                string record = request.Method + " " + shorturl;
                ((List<string>)ruleRequestRecord[this.RuleName]).Add(record);
                this.RuleRequestNumber += 1;
            }

            RequestTotalTime += 1;

            string filename = string.Empty;
            string fullFileName = string.Empty;
            string dataRootDir = string.Empty;
            try
            {
                dataRootDir = site.Properties["Conformance_Response_Data_Path"];
                if (string.IsNullOrEmpty(this.RuleName))
                {
                    filename = getFileName(request);
                    fullFileName = dataRootDir + filename;
                    if (Directory.Exists(dataRootDir) && !File.Exists(fullFileName))
                    {
                        var datafiles = (new DirectoryInfo(dataRootDir)).GetFiles(string.Format("GET_!TERM_{0}.txt", getFileCode(request)));
                        if (datafiles != null && datafiles.Length > 0)
                        {
                            filename = datafiles[0].Name;
                            fullFileName = dataRootDir + filename;
                        }
                    }
                }
                else
                {
                    filename = string.Format("{0}_{1}", this.RuleRequestNumber, getFileName(request));
                    dataRootDir += this.RuleName;
                    fullFileName = dataRootDir + "\\" + filename;

                    if (Directory.Exists(dataRootDir) && !File.Exists(fullFileName))
                    {
                        var datafiles = (new DirectoryInfo(dataRootDir)).GetFiles(this.RuleRequestNumber.ToString() + "_*.txt");//TODO
                        if (datafiles != null && datafiles.Length > 0)
                        {
                            filename = datafiles[0].Name;
                            List<string> res = (List<string>)this.ruleRequestRecord[this.RuleName];
                            res[res.Count - 1] = res[res.Count - 1] + " transto " + filename;
                            fullFileName = dataRootDir + "\\" + filename;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(fullFileName))
                {
                    statusCode = readTextData(fullFileName, out responseHeaders, out responsePayload);
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

        #region RuleRequestUtility

        private HttpStatusCode readTextData(string FullFileName, out string responseHeaders, out string responsePayload)
        {
            
            string TextDataContent = Utility.ReadFile(FullFileName);
            string[] lines = TextDataContent.Split(new string[] { "\r\n\r\n" }, 2, StringSplitOptions.None);

            var Headers = lines[0].Split( new string[] { "\r\n" }, 3, StringSplitOptions.None );

            HttpStatusCode statusCode = (HttpStatusCode)(Enum.Parse(typeof(HttpStatusCode), Headers[1]));

            responseHeaders = Headers[2];
            responsePayload = lines[1];

            return statusCode;
        }
        
        private int getConstantHashString(string p)
        {
            int ConstantHash = 0;
            int seed = 131;
            foreach (var charactor in p)
            {
                ConstantHash = ConstantHash * seed + (int)charactor;
            }
            return ConstantHash;
        }

        private string getFileCode(WebRequest request)
        {
            int urlHashCode = getConstantHashString(request.RequestUri.AbsoluteUri) + getConstantHashString(request.Method);
            int headerHashCode = getConstantHashString(request.Headers.ToString());
            int fileCode = urlHashCode + headerHashCode;

            return string.Format("{0,8:X08}", fileCode);
        }

        private string getFileName(WebRequest request)
        {
            string From = @"/:*?""<>|\";
            string To = @".;#7'{}!~";

            StringBuilder requestURL = new StringBuilder(request.RequestUri.AbsoluteUri);

            if (request.RequestUri.AbsoluteUri.StartsWith(URL_SrvDocConstants.URL_SrcDoc_Conformance))
            {
                requestURL = requestURL.Remove(0, URL_SrvDocConstants.URL_SrcDoc_Conformance.Length);
            }
            else
            {
                requestURL = new StringBuilder("!TERM");
            }

            for (int i = 0; i < requestURL.Length; i++)
            {
                int m = From.IndexOf(requestURL[i]);
                if (-1 != m)
                {
                    requestURL[i] = To[m];
                }
            }

            string FileName = string.Format("{0}_{1}.txt", request.Method, requestURL);

            return FileName;
        }

        #endregion

        ~ConformanceDataService()
        {
            System.IO.StreamWriter logStream = new StreamWriter(site.Properties["Conformance_Response_Data_Path"] + ".log", true);

            logStream.WriteLine("handle total {0} time request", RequestTotalTime);

            logStream.Close();
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
            this.RuleRequestNumber = 0;
            ruleRequestRecord.Add(this.RuleName, new List<string>());
        }
    }
}
