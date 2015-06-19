// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region Namespaces
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Helper class having factory method to create various ICheck validation processors
    /// </summary>
    internal static class VerifierFactory
    {
        /// <summary>
        /// Factory method to create various IVerifier objects
        /// </summary>
        /// <param name="xml">The xml element representing the validating action</param>
        /// <returns>IVerifer derived object for validating action</returns>
        [SuppressMessage("DataWeb.Usage", "AC0014:DoNotHandleProhibitedExceptionsRule", Justification = "Taken care of by similar mechanism")]
        public static IVerifier Create(XElement xml)
        {
            var attrProcessor = xml.Attribute("processor");

            if (attrProcessor == null)
            {
                return null;
            }

            IVerifier verifier = null;

            try
            {
                switch (attrProcessor.Value)
                {
                    case "rng":
                        verifier = VerifierFactory.CreateRngVerifier(xml.Elements().Single());
                        break;
                    case "regex":
                        verifier = VerifierFactory.CreateRegexVerifier(xml.Element("regex"));
                        break;
                    case "headerregex":
                        verifier = VerifierFactory.CreateHeaderRegexVerifier(xml.Elements().Single());
                        break;
                    case "xslt+rng":
                        verifier = VerifierFactory.CreateXsltRngVerifier(xml.Elements().Single());
                        break;
                    case "jsonschema":
                        verifier = VerifierFactory.CreateJsonSchemaVerifier(xml.Element("jsonschema"));
                        break;
                    case "xslt+jsonschema":
                        verifier = VerifierFactory.CreateXstJsonSchemaVerifier(xml.Elements().Single());
                        break;
                    default:
                        // not supported processor types
                        break;
                }
            }
            catch (Exception e)
            {
                if (!ExceptionHelper.IsCatchableExceptionType(e))
                {
                    throw;
                }

                verifier = null;
            }

            return verifier;
        }

        /// <summary>
        /// Creates ReleaxNG verifier object from xml element node
        /// </summary>
        /// <param name="xmlRngSchema">the xml element node containing the relaxNG schema</param>
        /// <returns>IVerifier interface of the newly created ReleaxNG verifier object</returns>
        private static IVerifier CreateRngVerifier(XElement xmlRngSchema)
        {
            if (xmlRngSchema == null)
            {
                throw new ArgumentNullException("xmlRngSchema");
            }

            if (!xmlRngSchema.Name.LocalName.Equals("grammar", StringComparison.Ordinal))
            {
                throw new ArgumentException(Resource.RelaxNGRootNode);
            }

            return new RngVerifier(xmlRngSchema.ToString());
        }

        /// <summary>
        /// Creates regular expression verifier from xml element node
        /// </summary>
        /// <param name="xmlRegex">xml node containing the pattern of regular expression</param>
        /// <returns>IVerifier interface of the newly created regular expression pattern verifier object</returns>
        private static IVerifier CreateRegexVerifier(XElement xmlRegex)
        {
            if (xmlRegex != null && !string.IsNullOrEmpty(xmlRegex.UnescapeElementValue()))
            {
                return new RegexVerifier(xmlRegex.UnescapeElementValue());
            }
 
            return null;
        }

        /// <summary>
        /// Creates Http header pattern verifier object from xml element node
        /// </summary>
        /// <param name="xmlHeader">the xml element node containing the header field name and repected regular expression value </param>
        /// <returns>IVerifier interface of the newly created Http Header pattern verifier object</returns>
        private static IVerifier CreateHeaderRegexVerifier(XElement xmlHeader)
        {
            if (xmlHeader != null && xmlHeader.Attribute("field") != null && xmlHeader.Element("regex") != null)
            {
                return new HttpHeaderRegexVerifier(xmlHeader.GetAttributeValue("field"), xmlHeader.GetFirstSubElementValue("regex"));
            }
 
            return null;
        }

        /// <summary>
        /// Creates XSLT-ReleaxNG composite verifier from xml element node 
        /// </summary>
        /// <param name="xmlSxlt">the xml node containing XSLT instuctions</param>
        /// <returns>IVerifier interface of the newly created XSLT-ReleaxNG composite verifier object</returns>
        private static IVerifier CreateXsltRngVerifier(XElement xmlSxlt)
        {
            if (xmlSxlt == null)
            {
                throw new ArgumentNullException("xmlSxlt");
            }

            return new XsltRngVerifier(xmlSxlt.ToString());
        }

        /// <summary>
        /// Creates Json schema verifer object from xml element node
        /// </summary>
        /// <param name="xmlJsonSchema">the xml node contining Json schema</param>
        /// <returns>IVerifier interface of the newly created Json schema verifer object</returns>
        private static IVerifier CreateJsonSchemaVerifier(XElement xmlJsonSchema)
        {
            if (xmlJsonSchema == null)
            {
                throw new ArgumentNullException("xmlJsonSchema");
            }

            string jsonSchema = xmlJsonSchema.UnescapeElementValue();
            return new JsonSchemaVerifier(jsonSchema);
        }

        /// <summary>
        /// Creates composite dynamic(xslt-processed) Jscon schema verifier from xml node
        /// </summary>
        /// <param name="xmlSxlt">the xml node containing xslt instructions</param>
        /// <returns>IVerifier interface of the newly created composite XSLT-Jscon schema verifier object</returns>
        private static IVerifier CreateXstJsonSchemaVerifier(XElement xmlSxlt)
        {
            if (xmlSxlt == null)
            {
                throw new ArgumentNullException("xmlSxlt");
            }

            return new XsltJsonSchemaVerifier(xmlSxlt.ToString());
        }
    }
}
