// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using Commons.Xml.Relaxng;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Xml;    
    #endregion

    /// <summary>
    /// Class that verifies Xml or Atompub payload with RelaxNG schema 
    /// </summary>
    public class RngVerifier : IVerifier
    {
        /// <summary>
        /// Compiled ReleaxNG schema
        /// </summary>
        private RelaxngPattern rngPattern;

        /// <summary>
        /// Initializes a new instance of RngVerifier class from specified RelaxNG schema definition
        /// </summary>
        /// <param name="schema">RelaxNG schema definition</param>
        public RngVerifier(string schema)
        {
            using (var rdr = new StringReader(schema))
            {
                XmlTextReader xtrRng = new XmlTextReader(rdr);
                this.rngPattern = RelaxngPattern.Read(xtrRng);
                this.rngPattern.Compile();
            }
        }

        /// <summary>
        /// Verifies whether the payload of current request session complies to the specified RelaxNG schema or not
        /// </summary>
        /// <param name="context">Context object representing the current OData interop session</param>
        /// <param name="result">Output parameter of validation result</param>
        /// <returns>True if passed; false if failed</returns>
        public bool Verify(ServiceContext context, out TestResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return this.Verify(context.ResponsePayload, out result);
        }

        /// <summary>
        /// Verifies whether the specified content complies to the specified RelaxNG schema or not
        /// </summary>
        /// <param name="content">The payload content to be verified</param>
        /// <param name="result">Output parameter of validation result</param>
        /// <returns>True if passed; false if failed</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "followed the overiden method")]
        public bool Verify(string content, out TestResult result)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            using (StringReader sr = new StringReader(content))
            {
                XmlTextReader xtrXml = new XmlTextReader(sr);

                using (RelaxngValidatingReader vr = new RelaxngValidatingReader(xtrXml, this.rngPattern))
                {
                    try
                    {
                        while (vr.Read())
                        {
                            // Ignore
                        }

                        result = new TestResult();
                        return true;
                    }
                    catch (RelaxngException rngex)
                    {
                        result = new TestResult() { LineNumberInError = vr.LineNumber, ErrorDetail = rngex.Message };
                    }

                    return false;
                }
            }
        }
    }
}
