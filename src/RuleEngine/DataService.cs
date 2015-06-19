// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespace
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    #endregion

    /// <summary>
    /// The class for data service
    /// </summary>
    public class DataService
    {
        public static IDataService serviceInstance;

        /// <summary>
        /// Register a user-defined service.
        /// </summary>
        /// <param name="dataService">The user-defined service</param>
        public void RegisterDataService(IDataService dataService)
        {
            serviceInstance = dataService;
        }

        /// <summary>
        /// Unregister a user-defined service.
        /// </summary>
        /// <param name="dataService">The user-defined service</param>
        public void UnregisterDataService()
        {
            serviceInstance = null;
        }

        /// <summary>
        /// The defined service handle the request.
        /// </summary>
        /// <param name="request">The request url.</param>
        /// <param name="statusCode">output the status code.</param>
        /// <param name="responseHeaders">output the headers of response.</param>
        /// <param name="responsePayload">output the payload of response.</param>
        public void HandleRequest(WebRequest request, out HttpStatusCode? statusCode, out string responseHeaders, out string responsePayload)
        {
            statusCode = null;
            responseHeaders = string.Empty;
            responsePayload = string.Empty;

            serviceInstance.HandleRequest(request, out statusCode, out responseHeaders, out responsePayload);
        }

        /// <summary>
        /// Get the path of extension rule store.
        /// </summary>
        /// <returns>extension rule store</returns>
        public string GetExtensionStorePath()
        {
            return serviceInstance.GetExtensionStorePath();
        }

        /// <summary>
        /// Get the path of XML rule store.
        /// </summary>
        /// <returns>XML rule store</returns>
        public string GetRulestorePath()
        {
            return serviceInstance.GetRulestorePath();
        }

        /// <summary>
        /// Get the connection string of ODataValidationSuiteEntities, which defined in web.config
        /// </summary>
        /// <returns>the connection string</returns>
        public string GetConnectionString()
        {
            return serviceInstance.GetConnectionString();
        }

        public void SwitchRule(string ruleName)
        {
            serviceInstance.SwitchRule(ruleName);
        }
    }
}
