// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Protocols.TestSuites.Validator
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This class contains test cases for creating objects.
    /// </summary>
    [TestClass]
    public class S07_JSONRule_Error_Validation : TestSuiteBase
    {
        #region Class Initialization and Cleanup

        /// <summary>
        /// Initialize class fields before running any test case.
        /// </summary>
        /// <param name="context">
        /// Used to store information that is provided to unit tests.
        /// </param>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TestSuiteBase.TestSuiteClassInitialize(context);
        }

        /// <summary>
        /// Cleanup class fields after running all test cases.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestSuiteBase.TestSuiteClassCleanup();
        }

        #endregion

        #region Test Cases

        /// <summary>
        /// Test case to verify Error Rules
        /// </summary>
        [TestMethod]
        public void S07_TC01_JSONV4Rule_VerifyError()
        {
            List<string> expectedRuleList = new List<string>()
            {
                // Code rule list
                "Error.Core.4009", "Error.Core.4010", "Error.Core.4012", 
                // Xml rule list
                "Error.Core.4000", "Error.Core.4001", "Error.Core.4003", "Error.Core.4004", 
                "Error.Core.4005", "Error.Core.4006", "Error.Core.4011", "Error.Core.4100", 
                "Error.Core.4101", 
                // Common rule list
                "Common.Core.4000", "Common.Core.4004", "Common.Core.4006",
            };

            List<string> negativeRuleListRecomm = new List<string>()
            {
                // == May rules(recommendation) - not need to verify ==
                // Common rule list
                "Common.Core.4018"
            };
            
            List<string> negativeRuleListNotApp = new List<string>()
            {
                // == May rules(notApplicable) - not need to verify ==
                // Code rule list
                "Error.Core.4014", 
                // Common rule list - Not applicable for error response
                "Common.Core.4010", "Common.Core.4601", "Common.Core.4119", "Common.Core.4122", "Common.Core.4123", "Common.Core.4700", 
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(negativeRuleListRecomm, ValidationResultConstants.Recommendation);
            negativeDic.Add(negativeRuleListNotApp, ValidationResultConstants.NotApplicable);

            this.VerifyRules_BaseTestCase(new RequestElement(TripPinSvc_URLConstants.URL_Error), expectedRuleList, negativeDic);
        }
       
        #endregion
    }
}
