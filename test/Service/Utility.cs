// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Microsoft.Protocols.TestSuites.Validator
{
    public static class Utility
    {
        public static string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
