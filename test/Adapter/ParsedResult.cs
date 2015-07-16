// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ODataValidator.ValidationService;

namespace Microsoft.Protocols.TestSuites.Validator
{
    public class ParsedResult
    {
        public Dictionary<string, string> RuleNameAndResult
        {
            get;
            set;
        }

        public Dictionary<string, string> RuleNameAndDescription
        {
            get;
            set;
        }

        public int executedV4RuleCount
        {
            get;
            set;
        }

        public ParsedResult()
        {
            this.RuleNameAndResult = new Dictionary<string, string>();
            this.RuleNameAndDescription = new Dictionary<string, string>();
        }

        public void Parse(List<TestResult> testResults)
        {
            executedV4RuleCount = 0;

            foreach (TestResult tr in testResults)
            {
                if (!this.RuleNameAndResult.ContainsKey(tr.RuleName) && !this.RuleNameAndDescription.ContainsKey(tr.RuleName))
                {
                    this.RuleNameAndResult.Add(tr.RuleName, tr.Classification);
                    this.RuleNameAndDescription.Add(tr.RuleName, tr.RuleName + ": " + tr.Description);
                }

                //TODO
                if (tr.RuleName.Split('.')[2].StartsWith("4"))
                {
                    this.executedV4RuleCount++;
                }
            }
        }
    }
}
