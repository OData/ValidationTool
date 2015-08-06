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
    public class S04_JSONRule_Delta_Validation : TestSuiteBase
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
        /// Test case to Verify Delta Rules
        /// </summary>
        [TestMethod]
        public void S04_TC01_JSONV4Rule_VerifyDelta()
        {
            List<string> expectedRuleList = new List<string>()
            {
                // Code rule list
                "Delta.Core.4001", "Delta.Core.4013", "Delta.Core.4019", "Delta.Core.4020", "Delta.Core.4026", "Delta.Core.4027", 
                "Delta.Core.4028", "Delta.Core.4037", "Delta.Core.4038", "Delta.Core.4039", "Delta.Core.4100",
                // Common rule list
                "Common.Core.4000", "Common.Core.4004", "Common.Core.4003", "Common.Core.4018", "Common.Core.4019",  
                "Common.Core.4036", "Common.Core.4037", "Common.Core.4038", "Common.Core.4065", "Common.Core.4067", "Common.Core.4068", 
                "Common.Core.4069", "Common.Core.4100", "Common.Core.4119", "Common.Core.4122", "Common.Core.4123", "Common.Core.4601", 
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(new List<string>() { "Common.Core.4006" }, ValidationResultConstants.Recommendation);
            negativeDic.Add(new List<string>() { "Common.Core.4010", "Common.Core.4034", "Common.Core.4700", }, ValidationResultConstants.NotApplicable);
            
            this.VerifyRules_BaseTestCase(new RequestElement(TripPinSvc_URLConstants.URL_Delta), expectedRuleList, negativeDic);
        }

       
        #endregion

    }
}
