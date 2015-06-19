// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespaces

    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.Xsl;

    #endregion

    /// <summary>
    /// Class of XSL transformation 
    /// </summary>
    internal static class XsltTransformer
    {
        /// <summary>   
        /// Transforms the supplied xml using the supplied xslt and returns the result of the transformation   
        /// </summary>   
        /// <param name="xml">The xml to be transformed</param>   
        /// <param name="xslt">The xslt to transform the xml</param>   
        /// <returns>The transformed xml</returns>   
        /// <exception cref="ArgumentException">Throws exception when input parameters are null or empty</exception>
        /// <exception cref="XsltException">Throws exception when XSLT transformation fails</exception>
        public static string Transform(string xml, string xslt)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentException(Resource.ArgumentNotNullOrEmpty, "xml");
            }

            if (string.IsNullOrEmpty(xslt))
            {
                throw new ArgumentException(Resource.ArgumentNotNullOrEmpty, "xslt");
            }

            using (var rdr = new StringReader(xslt))
            {
                XmlTextReader xsltReader = new XmlTextReader(rdr);
                using (var sdr = new StringReader(xml))
                {
                    XmlTextReader xmlReader = new XmlTextReader(sdr);
                    using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
                    {
                        XmlTextWriter transformedXml = new XmlTextWriter(stringWriter);

                        // Create a XslCompiledTransform to perform transformation   
                        XslCompiledTransform xsltTransform = new XslCompiledTransform();

                        xsltTransform.Load(xsltReader);
                        xsltTransform.Transform(xmlReader, transformedXml);

                        return stringWriter.ToString();
                    }
                }
            }
        }
    }
}
