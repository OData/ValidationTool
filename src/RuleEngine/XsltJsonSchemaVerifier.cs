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
    /// Class of dynamic Json schema verifier 
    /// </summary>
    public class XsltJsonSchemaVerifier : IVerifier
    {
        /// <summary>
        /// member of xsl instruction text
        /// </summary>
        private string xslt;

        /// <summary>
        /// Initializes a new instance of XsltJsonSchemaVerifier from xsl instruction text
        /// </summary>
        /// <param name="xslt">Xsl instruction text</param>
        public XsltJsonSchemaVerifier(string xslt) 
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

            if (context.PayloadFormat != PayloadFormat.Json
                && context.PayloadFormat != PayloadFormat.JsonLight)
            {
                throw new ArgumentException(Resource.PayloadFormatUnexpected);
            }

            if (!context.HasMetadata)
            {
                throw new ArgumentException(Resource.MetadataUnavailable);
            }

            string xsltMaterialized = XsltRulePreprocessor.Preprocess(context, this.xslt);
            return XsltJsonSchemaVerifier.Verify(xsltMaterialized, context.ResponsePayload, context.MetadataDocument, out result);
        }

        /// <summary>
        /// Verifies content against xslt instruction and metadata document
        /// </summary>
        /// <param name="xsltRule">Xslt instruction</param>
        /// <param name="content">Content to be verified</param>
        /// <param name="metadata">Metadata document</param>
        /// <param name="result">Output of test result</param>
        /// <returns>True if passed; false otherwise</returns>
        /// <exception cref="RuntimeException">Throws exception when rule engine encounters runtime errors</exception>
        [SuppressMessage("DataWeb.Usage", "AC0014:DoNotHandleProhibitedExceptionsRule", Justification = "Taken care of by similar mechanism")]
        private static bool Verify(string xsltRule, string content, string metadata, out TestResult result)
        {
            string jsonSchema = XsltTransformer.Transform(metadata, xsltRule);

            try
            {
                var jsonSchemaVerifier = new JsonSchemaVerifier(jsonSchema);
                return jsonSchemaVerifier.Verify(content, out result);
            }
            catch (Exception e)
            {
                if (!ExceptionHelper.IsCatchableExceptionType(e))
                {
                    throw;
                }

                throw new RuntimeException(e, jsonSchema);
            }
        }
    }
}
