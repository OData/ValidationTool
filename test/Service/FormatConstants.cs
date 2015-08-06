// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Protocols.TestSuites.Validator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The constants of formats
    /// </summary>
    public static class FormatConstants
    {
        /// <summary>
        /// The ATOM full format string.
        /// </summary>
        public const string FormatAtom = "application/atom+xml";

        /// <summary>
        /// The ATOM service document full format string
        /// </summary>
        public const string FormatAtomServiceDoc = "application/atomsvc+xml";

        /// <summary>
        /// The XML full format string
        /// </summary>
        public const string FormatXml = "application/xml";

        /// <summary>
        /// The ATOM format abbreviation.
        /// </summary>
        public const string FormatAtomAbbre = "atom";

        /// <summary>
        /// The XML format abbreviation
        /// </summary>
        public const string FormatXmlAbbre = "xml";

        /// <summary>
        /// The JSON format abbreviation
        /// </summary>
        public const string FormatJson = "json";

        /// <summary>
        /// expecting request odata v3 context to be json verbose format
        /// </summary>
        public const string V3FormatJsonVerbose = FormatJson + ";odata=verbose";

        /// <summary>
        /// expecting request odata v3 context to be json format and full metadata
        /// </summary>
        public const string V3FormatJsonFullMetadata = FormatJson + ";odata=fullmetadata";

        /// <summary>
        /// expecting request odata v3 context to be json format and minimal metadata
        /// </summary>
        public const string V3FormatJsonMinimalMetadata = FormatJson + ";odata=minimalmetadata";

        /// <summary>
        /// expecting request odata v3 context to be json format and minimal metadata
        /// </summary>
        public const string V3FormatJsonNoMetadata = FormatJson + ";odata=nometadata";

        /// <summary>
        /// expecting request odata v4 context to be json format and full metadata
        /// </summary>
        public const string V4FormatJsonFullMetadata = FormatJson + ";odata.metadata=full";

        /// <summary>
        /// expecting request odata v4 context to be json format and minimal metadata
        /// </summary>
        public const string V4FormatJsonMinimalMetadata = FormatJson + ";odata.metadata=minimal";

        /// <summary>
        /// expecting request odata v4 context to be json format and minimal metadata
        /// </summary>
        public const string V4FormatJsonNoMetadata = FormatJson + ";odata.metadata=none";
    }
}
