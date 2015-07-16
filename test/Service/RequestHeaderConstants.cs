// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Protocols.TestSuites.Validator
{
    public static class RequestHeaderConstants
    {
        public const string V4Version = @"OData-Version:4.0;";
        public const string V4MinVersion = @"minDataServiceVersion:4.0;";
        public const string V3MinVersion = @"minDataServiceVersion:3.0;";
    }
}
