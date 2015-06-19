// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Diagnostics.CodeAnalysis;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class that verifies with a XSLT+RelaxNG composite rule
    /// </summary>
    internal class XsltRngVerifier : IVerifier
    {
        /// <summary>
        /// Original rule definition string containing the xslt instructions
        /// </summary>
        private string xslt;

        /// <summary>
        /// Initializes a new instance of XsltRngVerifier class from xslt instruction string
        /// </summary>
        /// <param name="xslt">The xslt instruction literal</param>
        public XsltRngVerifier(string xslt) 
        {
            this.xslt = xslt;
        }

        /// <summary>
        /// Verifies the specified interop request context with the rule
        /// </summary>
        /// <param name="context">The interop request context</param>
        /// <param name="result">Output parameter of validation result</param>
        /// <returns>True if passed; false if failed</returns>
        public bool Verify(ServiceContext context, out TestResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.PayloadFormat != PayloadFormat.Xml && context.PayloadFormat != PayloadFormat.Atom)
            {
                throw new ArgumentException(Resource.PayloadFormatUnexpected);
            }

            if (!context.HasMetadata)
            {
                throw new ArgumentException(Resource.MetadataUnavailable);
            }

            string xsltMaterialized = XsltRulePreprocessor.Preprocess(context, this.xslt);
            return XsltRngVerifier.Verify(xsltMaterialized, context.ResponsePayload, context.MetadataDocument, out result);
        }

        /// <summary>
        /// Verifies the specified content with the rule
        /// </summary>
        /// <param name="xsltRule">rule definition</param>
        /// <param name="content">content to be verified</param>
        /// <param name="metadata">metadata document of content</param>
        /// <param name="result">output parameter of test result</param>
        /// <returns>true if rule is verified and passed; false if rule is not passed.</returns>
        /// <exception cref="RuntimeException">Throws exception when rule engine encounters runtime errors</exception>
        [SuppressMessage("DataWeb.Usage", "AC0014:DoNotHandleProhibitedExceptionsRule", Justification = "Taken care of by similar mechanism")]
        private static bool Verify(string xsltRule, string content, string metadata, out TestResult result)
        {
            string rng = XsltTransformer.Transform(metadata, xsltRule);

            try
            {
                var rngChecker = new RngVerifier(rng);
                return rngChecker.Verify(content, out result);
            }
            catch (Exception e)
            {
                if (!ExceptionHelper.IsCatchableExceptionType(e))
                {
                    throw;
                }

                throw new RuntimeException(e, rng);
            }
        }
    }
}
