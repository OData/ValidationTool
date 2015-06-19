// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    #region Namespace.
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    #endregion

    /// <summary>
    /// The TermDocument class.
    /// </summary>
    public class TermDocuments
    {
        /// <summary>
        /// Get the instance of class type TermDocument.
        /// </summary>
        /// <returns>The instance of class type TermDocument.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static TermDocuments GetInstance()
        {
            if (null == termDocs)
            {
                termDocs = new TermDocuments();
            }

            return termDocs;
        }

        /// <summary>
        /// Get or private set the voc-capabilities document.
        /// </summary>
        public string VocCapabilitiesDoc
        {
            get;
            private set;
        }

        /// <summary>
        /// Get or private set the voc-core document.
        /// </summary>
        public string VocCoreDoc
        {
            get;
            private set;
        }

        /// <summary>
        /// Get or private set the voc-measures document.
        /// </summary>
        public string VocMeasuresDoc
        {
            get;
            private set;
        }

        /// <summary>
        /// The vocabularies capabilities URL.
        /// </summary>
        private const string VocCapabilitiesURL = "http://docs.oasis-open.org/odata/odata/v4.0/cos01/vocabularies/Org.OData.Capabilities.V1.xml";

        /// <summary>
        /// The vocabularies core URL.
        /// </summary>
        private const string VocCoreURL = "http://docs.oasis-open.org/odata/odata/v4.0/cos01/vocabularies/Org.OData.Core.V1.xml";

        /// <summary>
        /// The vocabularies measures URL.
        /// </summary>
        private const string VocMeasuresURL = "http://docs.oasis-open.org/odata/odata/v4.0/cos01/vocabularies/Org.OData.Measures.V1.xml";

        /// <summary>
        /// The term restrictions documents.
        /// </summary>
        private static TermDocuments termDocs;

        /// <summary>
        /// The constructor of class type TermDocument.
        /// </summary>
        private TermDocuments()
        {
            // Get the voc-capabilities term restrictions.
            var response = WebHelper.Get(new Uri(VocCapabilitiesURL), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);
            this.VocCapabilitiesDoc = HttpStatusCode.OK == response.StatusCode ? response.ResponsePayload : string.Empty;

            // Get the voc-core term restrictions.
            response = WebHelper.Get(new Uri(VocCoreURL), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);
            this.VocCoreDoc = HttpStatusCode.OK == response.StatusCode ? response.ResponsePayload : string.Empty;

            // Get the voc-measures term restrictions.
            response = WebHelper.Get(new Uri(VocMeasuresURL), string.Empty, RuleEngineSetting.Instance().DefaultMaximumPayloadSize, null);
            this.VocMeasuresDoc = HttpStatusCode.OK == response.StatusCode ? response.ResponsePayload : string.Empty;
        }
    }
}
