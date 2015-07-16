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
    public class S06_JSONRule_EntityReference_Validation : TestSuiteBase
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
        /// Test case to Verify EntityReference Rules
        /// </summary>
        [TestMethod]
        public void S06_TC01_JSONV4Rule_VerifyEntityReference()
        {
            List<string> expectedRuleList = new List<string>()
            {
                // Code rule list
                "EntityReference.Core.4303", "EntityReference.Core.4332",
                // Xml rule list
                "EntityReference.Core.4302", "EntityReference.Core.4333",
                // Common rule list
                "Common.Core.4000", "Common.Core.4004", "Common.Core.4018", 
                "Common.Core.4019", "Common.Core.4100", "Common.Core.4119", 
                "Common.Core.4122", "Common.Core.4123", "Common.Core.4601",
            };

            List<string> negativeRuleListRecomm = new List<string>() 
            { 
                // == May rules(recommendation) - not need to verify ==
                // Code rule list
                "EntityReference.Core.4331", 
                // Xml rule list 
                "EntityReference.Core.4334", "EntityReference.Core.4335", "EntityReference.Core.4336",
                // Common rule list
                "Common.Core.4006", "Common.Core.4512"
            };

            List<string> negativeRuleListNotApp = new List<string>() 
            { 
                // == May rules(notApplicable) - not need to verify ==
                // Common rule list
                "Common.Core.4010", "Common.Core.4511", "Common.Core.4132", "Common.Core.4504", "Common.Core.4700",
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(negativeRuleListRecomm, ValidationResultConstants.Recommendation);
            negativeDic.Add(negativeRuleListNotApp, ValidationResultConstants.NotApplicable);

            this.VerifyRules_BaseTestCase(new RequestElement(TripPinSvc_URLConstants.URL_EntityReference), expectedRuleList, negativeDic);
        }

        #endregion
    }
}
