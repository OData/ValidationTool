// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    /// <summary>
    /// Definitions of methods for sending a request to odata service using in HttpRequest.
    /// </summary>
    public static class HttpMethod
    {
        /// <summary>
        /// Get method
        /// </summary>
        public const string Get = System.Net.WebRequestMethods.Http.Get;

        /// <summary>
        /// Post method
        /// </summary>
        public const string Post = System.Net.WebRequestMethods.Http.Post;

        /// <summary>
        /// Put method
        /// </summary>
        public const string Put = System.Net.WebRequestMethods.Http.Put;

        /// <summary>
        /// Delete method
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        /// Merge method
        /// </summary>
        public const string Merge = "MERGE";

        /// <summary>
        /// Patch method
        /// </summary>
        public const string Patch = "PATCH";
    }
}
