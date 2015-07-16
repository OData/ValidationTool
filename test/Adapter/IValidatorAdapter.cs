// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using ODataValidator.ValidationService;
using Microsoft.Protocols.TestTools;

namespace Microsoft.Protocols.TestSuites.Validator
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public interface IValidatorAdapter : IAdapter
    {
        Guid[] SendRequest(string uri, string format, string toCrawl, string headers, string isConformance, string isMetaData, string levelTypes);

        bool IsJobCompleted(Guid jobId, out int ruleCount);

        List<TestResult> GetTestResults(Guid jobId);

        ParsedResult ParseResults(List<TestResult> results);

        bool GetRulesCountByRequirementLevel(List<string> RuleNameList, string testResultPath);
    }
}
