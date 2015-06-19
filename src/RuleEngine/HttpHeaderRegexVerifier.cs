// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// Class that verifies a Http header field having value matching a regular expression pattern
    /// </summary>
    public class HttpHeaderRegexVerifier : RegexVerifier, IVerifier
    {
        /// <summary>
        /// Regular expression pattern to match the header line with specified field name
        /// </summary>
        private Regex regexHeader;

        /// <summary>
        /// Initializes a new instance of HttpHeaderRegexVerifier from a header field name and value pattern
        /// </summary>
        /// <param name="field">Name of header field</param>
        /// <param name="value">Regular expression pattern of header value</param>
        public HttpHeaderRegexVerifier(string field, string value)
            : base(@"(^|([\r\n]+))\s*"  + field + @"\s*:" + value)
        {
            string taggedRegex = @"((^|([\r\n]+))\s*" + "(?<_involving_>" + field + @"\s*:.*?)[\r\n$])";
            this.regexHeader = new Regex(taggedRegex, RegexOptions.Compiled | RegexOptions.Multiline);
        }

        /// <summary>
        /// Verifies the specified interop request context
        /// </summary>
        /// <param name="context">The interop request context</param>
        /// <param name="result">output parameter of validation result</param>
        /// <returns>true if passed; false if failed</returns>
        [SuppressMessage("Microsoft.Design", "CA1062: validate local variable '(*result)' before using it.", Justification = "delegated to the calling method.")]
        public new bool Verify(ServiceContext context, out TestResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool passed  = this.Verify(context.ResponseHttpHeaders, out result);
            
            if (!passed)
            {
                result.TextInvolved = this.GetHeaderLineOfName(context.ResponseHttpHeaders);
            }

            return passed;
        }

        /// <summary>
        /// Gets the header line with specified field name
        /// </summary>
        /// <param name="input">headers</param>
        /// <returns>The line in the headers block that has the specified field name</returns>
        private string GetHeaderLineOfName(string input)
        {
            var match = this.regexHeader.Match(input);
            if (match.Success)
            {
                return match.Groups["_involving_"].Value;
            }

            return null;
        }
    }
}
