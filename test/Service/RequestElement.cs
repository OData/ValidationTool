// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Protocols.TestSuites.Validator
{
    public class RequestElement
    {
        private string requestUrl;
        public string RequestUrl
        {
            get
            {
                return this.requestUrl;
            }
        }

        private string requestFormat;
        public string RequestFormat
        {
            get
            {
                return this.requestFormat;
            }
            set
            {
                this.requestFormat = FormatConstants.V4FormatJsonFullMetadata;
            }
        }

        private string requestHeader;
        public string RequestHeader
        {
            get
            {
                return this.requestHeader;
            }
            set
            {
                this.requestHeader = RequestHeaderConstants.V4Version;
            }
        }

        public RequestElement(string reqUrl, string reqFormat = FormatConstants.V4FormatJsonFullMetadata, string reqHeader = RequestHeaderConstants.V4Version)
        {
            this.requestUrl = reqUrl;
            this.requestFormat = reqFormat;
            this.requestHeader = reqHeader;
        }
    }
}
