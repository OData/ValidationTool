// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "ODataValidator.ValidationService")]
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DataServices.Extensions")]
[module: SuppressMessage("Microsoft.MSInternal", "CA904:DeclareTypesInMicrosoftOrSystemNamespace", Scope = "namespace", Target = "ODataValidator.ValidationService")]
[module: SuppressMessage("Microsoft.MSInternal", "CA904:DeclareTypesInMicrosoftOrSystemNamespace", Scope = "namespace", Target = "DataServices.Extensions")]

// for generated code
// unable to suppress violations of parameters in static methods at module level; put SuppressMessage directly in code instead
// [module: SuppressMessage("Microsoft.Naming", "CA1702:Correct the wording of 'TimeStamp'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.EngineRuntimeException.#CreateEngineRuntimeException(System.Guid, System.String, System.DateTime, System.String)", MessageId = "timeStamp")]
// [module: SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "generated code", Scope = "member", Target = "ODataValidationService.EngineRuntimeException.#CreateEngineRuntimeException(global::System.Guid id, global::System.String ruleName, global::System.DateTime timeStamp, global::System.String uri)", MessageId = "3#")]
// [module: SuppressMessage("Microsoft.Naming", "CA1709: Correct the casing of 'ID'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.PayloadLine.#CreatePayloadLine(global::System.Guid id, global::System.Int32 lineNumber, global::System.String lineText, global::System.Guid jobID)", MessageId = "3#")]
// [module: SuppressMessage("Microsoft.Design", "CA1054: Change to System.Uri type", Justification = "generated code", Scope = "member", Target = "ODataValidationService.RequestLog.#CreateRequestLog(global::System.Int32 id, global::System.String uri, global::System.DateTime date, global::System.String format)", MessageId = "1#")]
// [module: SuppressMessage("Microsoft.Naming", "CA1709: Correct the casing of 'ID'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.TestResult.#CreateTestResult(global::System.Int32 id, global::System.String ruleName, global::System.String description, global::System.String classification, global::System.String oDataLevel, global::System.Guid validationJobID, global::System.String errorMessage)", MessageId = "5#")]
// [module: SuppressMessage("Microsoft.Naming", "CA1704: correct the spelling of 'o' in parameter name 'oDataLevel'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.TestResult.#CreateTestResult(global::System.Int32 id, global::System.String ruleName, global::System.String description, global::System.String classification, global::System.String oDataLevel, global::System.Guid validationJobID, global::System.String errorMessage)", MessageId = "4#")]
// [module: SuppressMessage("Microsoft.Design", "CA1054: Change to System.Uri type", Justification = "generated code", Scope = "member", Target = "ODataValidationService.ValidationJob.#CreateValidationJob(global::System.Guid, global::System.String, global::System.String)", MessageId = "3#")]
[module: SuppressMessage("Microsoft.Naming", "CA1711:Rename type name 'EngineRuntimeException'", Justification = "generated code", Scope = "type", Target = "ODataValidationService.EngineRuntimeException")]
[module: SuppressMessage("Microsoft.Naming", "CA1709: Correct the casing of 'ID'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.EngineRuntimeException.#ID")]
[module: SuppressMessage("Microsoft.Naming", "CA1702: Correct the wording of 'TimeStamp'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.EngineRuntimeException.#TimeStamp")]
[module: SuppressMessage("Microsoft.Design", "CA1056:Change to System.Uri type", Justification = "generated code", Scope = "member", Target = "ODataValidationService.EngineRuntimeException.#Uri")]

[module: SuppressMessage("Microsoft.Naming", "CA1709: Correct the casing of 'ID'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.PayloadLine.#ID")]
[module: SuppressMessage("Microsoft.Naming", "CA1709: Correct the casing of 'ID'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.PayloadLine.#JobID")]

[module: SuppressMessage("Microsoft.Naming", "CA1709: Correct the casing of 'ID'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.RequestLog.#ID")]
[module: SuppressMessage("Microsoft.Design", "CA1056: Change to System.Uri type", Justification = "generated code", Scope = "member", Target = "ODataValidationService.RequestLog.#Uri")]
        
[module: SuppressMessage("Microsoft.Naming", "CA1709:Correct the casing of 'ID'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.TestResult.#ID")]
[module: SuppressMessage("Microsoft.Design", "CA1056:Change to System.Uri type", Justification = "generated code", Scope = "member", Target = "ODataValidationService.TestResult.#HelpUri")]
[module: SuppressMessage("Microsoft.Design", "CA1056:Change to System.Uri type", Justification = "generated code", Scope = "member", Target = "ODataValidationService.TestResult.#SpecificationUri")]
[module: SuppressMessage("Microsoft.Naming", "CA1709: Correct the casing of 'ID'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.TestResult.#ValidationJobID")]

[module: SuppressMessage("Microsoft.Naming", "CA1709: Correct the casing of 'ID'", Justification = "generated code", Scope = "member", Target = "ODataValidationService.ValidationJob.#ID")]
[module: SuppressMessage("Microsoft.Design", "CA1056: Change to System.Uri type", Justification = "generated code", Scope = "member", Target = "ODataValidationService.ValidationJob.#Uri")]
[module: SuppressMessage("Microsoft.Performance", "CA1819: To return a collection", Justification = "generated code", Scope = "member", Target = "ODataValidationService.ValidationJob.#Version")]

[module: SuppressMessage("Microsoft.Usage", "CA2227:Change 'ValidationJob.PayloadLines' to be read-only", Justification = "generated code", Scope = "member", Target = "ODataValidationService.ValidationJob.#PayloadLines")]
[module: SuppressMessage("Microsoft.Usage", "CA2227:Change 'ValidationJob.TestResults' to be read-only", Justification = "generated code", Scope = "member", Target = "ODataValidationService.ValidationJob.#TestResults")]
