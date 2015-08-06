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
    public class S09_ATOMRule_ServiceDocument_Validation : TestSuiteBase
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

        protected override void TestInitialize()
        {
            dataService = new AtomDataService();
            base.TestInitialize();
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
        /// Test case to Verify ServiceDocument Rules
        /// </summary>
        [TestMethod]
        public void S09_TC01_ATOMV4Rule_VerifyServiceDocument()
        {
            List<string> expectedRuleList = new List<string>()
            {
                // Service document rule list
                "SvcDoc.Core.4635", "SvcDoc.Core.4636", "SvcDoc.Core.4637", "SvcDoc.Core.4602", "SvcDoc.Core.4603",
                "SvcDoc.Core.4609", "SvcDoc.Core.4610", "SvcDoc.Core.4612", "SvcDoc.Core.4614", "SvcDoc.Core.4616",
                "SvcDoc.Core.4617", "SvcDoc.Core.4620",
                "SvcDoc.Core.4618", "SvcDoc.Core.4631", "SvcDoc.Core.4623", "SvcDoc.Core.4625",
                "SvcDoc.Core.4626", "SvcDoc.Core.4630", "SvcDoc.Core.4632", "SvcDoc.Core.4633", "SvcDoc.Core.4634",
                // Common rule list
                "Common.Core.4600", 
                "Common.Core.4614", "Common.Core.4617", "Common.Core.4664", "Common.Core.4665", "Common.Core.4615", 
                "Common.Core.4649", "Common.Core.4650", "Common.Core.4651", "Common.Core.4653", "Common.Core.4654",
                "Common.Core.4656"
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(new List<string>() { "SvcDoc.Core.4624", "SvcDoc.Core.4604", "SvcDoc.Core.4622", "SvcDoc.Core.4628" }, ValidationResultConstants.Recommendation);
            this.VerifyRules_BaseTestCase(new RequestElement(AtomDataSvc_URLConstants.URL_ServiceDocument, FormatConstants.FormatXml), expectedRuleList, negativeDic);
        }

        #endregion
    }
}
