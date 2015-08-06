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
    public class S12_ATOMRule_IndividualProperty_Validation : TestSuiteBase
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
        public void S12_TC01_ATOMV4Rule_VerifyIndividualProperty()
        {
            List<string> expectedRuleList_PrimitiveString = new List<string>()
            {
                // Individual property rule list
                "IndividualProperty.Core.4604", 

                // Common rule list
                "Common.Core.4606", "Common.Core.4663", 
                "Common.Core.4613", "Common.Core.4614", "Common.Core.4656",
                "Common.Core.4664", "Common.Core.4665", "Common.Core.4616",
                 
                 "Common.Core.4649", "Common.Core.4650", "Common.Core.4651",
                "Common.Core.4653", "Common.Core.4654",  
            };

            List<string> expectedRuleList_NullPrimitiveString = new List<string>()
            {
                // Individual property rule list
                "IndividualProperty.Core.4601", "IndividualProperty.Core.4604",
            };

            List<string> expectedRuleList_PrimitiveNonString = new List<string>()
            {
                // Individual property rule list
                "IndividualProperty.Core.4606", "IndividualProperty.Core.4604",
            };

            List<string> expectedRuleList_CollectionPrimitive = new List<string>()
            {
                // Individual property rule list
                 "IndividualProperty.Core.4608", "IndividualProperty.Core.4610",
                 "IndividualProperty.Core.4613", 

                //Instance Annotation rules
                "Common.Core.4621", "Common.Core.4622", "Common.Core.4623",
                "Common.Core.4624", "Common.Core.4625", "Common.Core.4626"
            };

            List<string> expectedRuleList_CollectionDerivedComplex = new List<string>()
            {
                // Individual property rule list
                 "IndividualProperty.Core.4608", "IndividualProperty.Core.4610",
                 "IndividualProperty.Core.4613", "IndividualProperty.Core.4614",

                 //Instance Annotation rules
                 "Common.Core.4627", "Common.Core.4628", "Common.Core.4633", 
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(new List<string>() { "IndividualProperty.Core.4605", "Common.Core.4617" }, ValidationResultConstants.Recommendation);
            negativeDic.Add(new List<string>() { "Common.Core.4620", "IndividualProperty.Core.4607", "IndividualProperty.Core.4612", "IndividualProperty.Core.4615"}, ValidationResultConstants.NotApplicable);

            List<ValidationElement> verifyAndNegativeList = new List<ValidationElement>()
            {
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_IndividualProperty_PrimitiveString, FormatConstants.FormatXml), expectedRuleList_PrimitiveString, negativeDic),
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_IndividualProperty_PrimitiveNonString, FormatConstants.FormatXml), expectedRuleList_PrimitiveNonString),
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_IndividualProperty_NullPrimitiveString, FormatConstants.FormatXml), expectedRuleList_NullPrimitiveString),
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_IndividualProperty_CollectionPrimitive, FormatConstants.FormatXml), expectedRuleList_CollectionPrimitive),
                new ValidationElement(new RequestElement(AtomDataSvc_URLConstants.URL_IndividualProperty_CollectionDerivedComplex, FormatConstants.FormatXml), expectedRuleList_CollectionDerivedComplex),
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeList);
        }

        #endregion
    }
}
