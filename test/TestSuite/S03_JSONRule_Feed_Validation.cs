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
    public class S03_JSONRule_Feed_Validation : TestSuiteBase
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
        /// Test case to Verify Feed Rules
        /// </summary>
        [TestMethod]
        public void S03_TC01_JSONV4Rule_VerifyFeed()
        {
            List<string> expectedRuleList1 = new List<string>()
            {
                // Code rule list
                "Feed.Core.4009","Feed.Core.4011","Feed.Core.4013","Feed.Core.4015","Feed.Core.4033","Feed.Core.4300",
                // Common rule list
                "Common.Core.4000", "Common.Core.4004", "Common.Core.4003", 
                "Common.Core.4012", "Common.Core.4013", "Common.Core.4014", "Common.Core.4017", "Common.Core.4018",
                "Common.Core.4019", "Common.Core.4028", "Common.Core.4034", 
                "Common.Core.4038", "Common.Core.4042",  "Common.Core.4052", "Common.Core.4053",
                "Common.Core.4065", "Common.Core.4066", "Common.Core.4067", "Common.Core.4068", "Common.Core.4069",
                "Common.Core.4070", "Common.Core.4100", "Common.Core.4119", "Common.Core.4122", "Common.Core.4123", "Common.Core.4124",
                "Common.Core.4125", "Common.Core.4127", "Common.Core.4129", "Common.Core.4130", "Common.Core.4132", "Common.Core.4308", 
                "Common.Core.4309", "Common.Core.4318", "Common.Core.4401", "Common.Core.4402", "Common.Core.4403", "Common.Core.4404", 
                "Common.Core.4405", "Common.Core.4406", "Common.Core.4500", "Common.Core.4506", "Common.Core.4514", 
                "Common.Core.4518", "Common.Core.4519", "Common.Core.4520",
                
                "Common.Core.4503", "Common.Core.4504", "Common.Core.4511", "Common.Core.4512", 
                "Common.Core.4601", "Common.Core.4700",  "Common.Core.4712", 
                "Common.Core.4715", "Common.Core.4717", "Common.Core.4719", "Common.Core.4722", 
                "Common.Core.4723",
                
                // Xml rule list
               "Feed.Core.4006",  "Feed.Core.4007",  "Feed.Core.4008",  "Feed.Core.4012",  "Feed.Core.4108", "Feed.Core.4208"
            };

            List<string> expectedRuleList2 = new List<string>() 
            { 
                "Feed.Core.4014"
            };

            List<string> expectedRuleList3 = new List<string>()
            {
                "Common.Core.4044", "Common.Core.4502", "Common.Core.4703", 
            };

            List<string> expectedRuleList4 = new List<string>() 
            { 
                "Common.Core.4054", "Feed.Core.4400", 
            };

            List<string> expectedRuleList5 = new List<string>() 
            { 
                "Common.Core.4029"
            };

            Dictionary<List<string>, string> negativeDic = new Dictionary<List<string>, string>();
            negativeDic.Add(new List<string>() { "Common.Core.4006", "Common.Core.4515", "Common.Core.4516", "Common.Core.4517", "Feed.Core.4308", }, ValidationResultConstants.Recommendation);
            negativeDic.Add(new List<string>() { "Common.Core.4037", "Common.Core.4010", "Common.Core.4015", }, ValidationResultConstants.NotApplicable);

            List<ValidationElement> verifyAndNegativeList = new List<ValidationElement>()
            {
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_EntitySet_People_Full), expectedRuleList1, negativeDic),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_EntitySet_Empty), expectedRuleList2),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_EntitySet_People_Minimal, FormatConstants.V4FormatJsonMinimalMetadata), expectedRuleList3),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_EntitySet_Photos), expectedRuleList4),
                new ValidationElement(new RequestElement(ODataSvc_URLConstants.URL_EntitySet_Products_Full), expectedRuleList5),
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeList);
        }

        #endregion
    }
}
