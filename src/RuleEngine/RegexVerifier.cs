// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;
    using ODataValidator.RuleEngine;
    #endregion

    /// <summary>
    /// Class that verifies a payload content with specified regular expression pattern
    /// </summary>
    public class RegexVerifier : IVerifier
    {
        /// <summary>
        /// Regular expression pattern to verify content with
        /// </summary>
        private Regex regex;

        /// <summary>
        /// Initializes a new instance of RegexVerifier class with the pattern string
        /// </summary>
        /// <param name="pattern">regular expression pattern</param>
        public RegexVerifier(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentException(Resource.ArgumentNotNullOrEmpty, "pattern");
            }

            string taggedRegex = "(?<_involving_>" + pattern + ")";
            this.regex = new Regex(taggedRegex, RegexOptions.Compiled | RegexOptions.Multiline);
        }

        /// <summary>
        /// Verifies the specified payload of interop request context against current regular expression rule
        /// </summary>
        /// <param name="context">interop request session whose payload is to be verified</param>
        /// <param name="result">output paramater of verification result</param>
        /// <returns>true if passed; false if failed</returns>
        public bool Verify(ServiceContext context, out TestResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return this.Verify(context.ResponsePayload, out result);
        }

        /// <summary>
        /// Verifies the specified content against current regular expression rule
        /// </summary>
        /// <param name="input">the content to be verified</param>
        /// <param name="result">output paramater of verification result</param>
        /// <returns>true if passed; false failed</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "follow the orverload")]
        protected bool Verify(string input, out TestResult result)
        {
            var match = this.regex.Match(input);
            if (match.Success)
            {
                result = new TestResult() { TextInvolved = match.Groups["_involving_"].Value, };
            }
            else
            {
                result = new TestResult() { ErrorDetail = Resource.RegexPatternNotFound };
            }

            return match.Success;
        }
    }
}
