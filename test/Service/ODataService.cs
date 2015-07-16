namespace Microsoft.Protocols.TestSuites.Validator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using Microsoft.Protocols.TestTools;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    public class ODataService : IDataService
    {
        /// <summary>
        /// The ITestSite object which is used to get ptf value.
        /// </summary>
        private static ITestSite site;

        public ODataService()
        {
        }

        public ODataService(ITestSite testSite)
        {
            site = testSite;
        }

        public void HandleRquest(WebRequest request, out HttpStatusCode? statusCode, out string responseHeaders, out string responsePayload)
        {
            statusCode = null;
            responseHeaders = string.Empty;
            responsePayload = string.Empty;

            if (request.RequestUri == null || string.IsNullOrEmpty(request.RequestUri.AbsoluteUri))
                return;

            using (WebResponse resp = request.GetResponse())
            {
                statusCode = WebHelper.ParseResponse(RuleEngineSetting.Instance().DefaultMaximumPayloadSize, resp, out responseHeaders, out responsePayload);
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
    }
}