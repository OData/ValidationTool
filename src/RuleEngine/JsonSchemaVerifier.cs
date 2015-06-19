// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Schema;
    #endregion

    /// <summary>
    /// Class to validate payload with Json Schema.
    /// Current support Json schema Draft 3.
    /// </summary>
    public class JsonSchemaVerifier : IVerifier
    {
        /// <summary>
        /// compiled Json schema
        /// </summary>
        private JsonSchema schema;

        /// <summary>
        /// Initializes a new instance of JsonSchemaVerifier from the schema text
        /// </summary>
        /// <param name="jsonSchema">text of json schema</param>
        public JsonSchemaVerifier(string jsonSchema)
        {
            this.schema = JsonSchema.Parse(jsonSchema);
        }

        /// <summary>
        /// Verifies the specified payload of interop request context against current regular expression rule
        /// </summary>
        /// <param name="context">interop request session whose payload is to be verified</param>
        /// <param name="result">output parameter of verification result</param>
        /// <returns>true if passed; false if failed</returns>
        /// <exception cref="ArgumentNullExption">Throws excpetion when context parameter is null</exception>
        /// <exception cref="ArgumentException">Throws exception when context payload is not of Json</exception>
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

            return this.Verify(context.ResponsePayload, out result);
        }

        /// <summary>
        /// Verifies Json literal content 
        /// </summary>
        /// <param name="content">the Json literal to be verified</param>
        /// <param name="result">output paramter of test result</param>
        /// <returns>true if verification passes; false otherwiser</returns>
        public bool Verify(string content, out TestResult result)
        {
            using (var stringReader = new StringReader(content))
            {
                using (JsonTextReader rdr = new JsonTextReader(stringReader))
                {
                    using (JsonValidatingReader vr = new JsonValidatingReader(rdr))
                    {
                        vr.Schema = this.schema;

                        try
                        {
                            while (vr.Read()) 
                            { 
                                // Ignore
                            }

                            result = new TestResult();
                            return true;
                        }
                        catch (JsonSchemaException jex)
                        {
                            result = new TestResult() { LineNumberInError = jex.LineNumber, ErrorDetail = jex.Message };
                            return false;
                        }
                    }
                }
            }
        }
    }
}
