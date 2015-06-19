// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespace
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    #endregion

    /// <summary>
    /// Interface for a data service
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// The defined service handle the request.
        /// </summary>
        /// <param name="request">The request url.</param>
        /// <param name="statusCode">output the status code.</param>
        /// <param name="responseHeaders">output the headers of response.</param>
        /// <param name="responsePayload">output the payload of response.</param>
        void HandleRequest(WebRequest request, out HttpStatusCode? statusCode, out string responseHeaders, out string responsePayload);

        /// <summary>
        /// Get the path of extension rule store.
        /// </summary>
        /// <returns>extension rule store</returns>
        string GetExtensionStorePath();

        /// <summary>
        /// Get the path of XML rule store.
        /// </summary>
        /// <returns>XML rule store</returns>
        string GetRulestorePath();

        /// <summary>
        /// Get the connection string of ODataValidationSuiteEntities, which defined in web.config
        /// </summary>
        /// <returns>the connection string</returns>
        string GetConnectionString();

        void SwitchRule(string ruleName);
    }
}
