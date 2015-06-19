// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.ValidationService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    
    /// <summary>Class of ValidationJobState to represent the state of the job submitted</summary>
    public class ValidationJobState
    {
        /// <summary>Provide access to the rule engine</summary>
        public RuleEngine.RuleEngineWrapper RuleEngine { get; set; }

        /// <summary>Job Id as guid</summary>
        public Guid JobId { get; set; }
    }
}
