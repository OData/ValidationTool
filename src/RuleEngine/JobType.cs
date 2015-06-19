// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    /// <summary>
    /// Enum of payload types like servcie document, metadata document, feed, entry etc
    /// </summary>
    public enum JobType
    {
        None = 0,

        Normal = 1,

        Conformance = 2,

        ConformanceRerun = 3,
        
        Uri = 4,

        UriRerun = 5,

        Payload,

        PayloadRerun
    }
}
