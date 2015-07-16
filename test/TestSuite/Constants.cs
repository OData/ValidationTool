// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Protocols.TestSuites.Validator
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    using Microsoft.Protocols.TestTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class Constants
    {
        public const int None = 00000;
        public const int ServiceDocument = 10000;
        public const int Entry = 20000;
        public const int Feed = 30000;
        public const int Delta = 40000;
        public const int IndividualProperty = 50000;
        public const int EntityReference = 60000;
        public const int Error = 70000;
        public const int Common = 80000;
        public const int Minimal = 90000;
        public const int Intermediate = 100000;
        public const int Advanced = 110000;
    }

    public static class ValidationResultConstants
    {
        public const string Success = "success";
        public const string Recommendation = "recommendation";
        public const string NotApplicable = "notApplicable";
        public const string Pending = "pending";
        public const string Skip = "skip";
        public const string Warning = "warning";
    }
}
