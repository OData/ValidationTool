// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    using System.Net;

    public class ServiceVerificationResult
    {
        public bool? Passed = null;
        public ExtensionRuleViolationInfo ViolationInfo = null;
        public HttpStatusCode? ResponseStatusCode = null;

        public ServiceVerificationResult(bool? passed, ExtensionRuleViolationInfo violationInfo, HttpStatusCode? responseStatusCode = null)
        {
            Passed = passed;
            ViolationInfo = violationInfo;
            ResponseStatusCode = responseStatusCode;
        }
    }
}
