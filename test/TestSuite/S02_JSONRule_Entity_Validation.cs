// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Protocols.TestSuites.Validator
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Protocols.TestTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RuleEngine = ODataValidator.RuleEngine;
    using ValidationService = ODataValidator.ValidationService;

    /// <summary>
    /// This class contains test cases for creating objects.
    /// </summary>
    [TestClass]
    public class S02_JSONRule_Entity_Validation : TestSuiteBase
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
        /// Test case to Verify Entity Rules
        /// </summary>
        [TestMethod]
        public void S02_TC01_JSONV4Rule_VerifyEntity()
        {
            List<string> expectedRuleList_People_FullOnly = new List<string>()
            {
                // Code rule list
                "Entry.Core.4043", "Entry.Core.4068", "Entry.Core.4076", "Entry.Core.4077", "Entry.Core.4080", "Entry.Core.4082", 
                "Entry.Core.4086", "Entry.Core.4092", "Entry.Core.4094", "Entry.Core.4095", "Entry.Core.4097", "Entry.Core.4099", 
                "Entry.Core.4100", "Entry.Core.4102", "Entry.Core.4300", "Entry.Core.4302", 
                // Common rule list
                "Common.Core.4000", "Common.Core.4003", "Common.Core.4004", "Common.Core.4012", "Common.Core.4013", "Common.Core.4014", 
                "Common.Core.4017", "Common.Core.4018", "Common.Core.4019", "Common.Core.4028", "Common.Core.4038", "Common.Core.4042", 
                "Common.Core.4052", "Common.Core.4053", "Common.Core.4065", "Common.Core.4066", "Common.Core.4067", "Common.Core.4068", 
                "Common.Core.4069", "Common.Core.4070", "Common.Core.4100", "Common.Core.4119", "Common.Core.4122", "Common.Core.4123", 
                "Common.Core.4124", "Common.Core.4125", "Common.Core.4127", "Common.Core.4129", "Common.Core.4130", "Common.Core.4308", 
                "Common.Core.4318", "Common.Core.4401", "Common.Core.4402", "Common.Core.4403", "Common.Core.4404", "Common.Core.4405", "Common.Core.4406", 
                "Common.Core.4514", "Common.Core.4515", "Common.Core.4517", "Common.Core.4518", "Common.Core.4519", "Common.Core.4520", 
                "Common.Core.4601", "Common.Core.4703", "Common.Core.4712", "Common.Core.4715", "Common.Core.4717", "Common.Core.4719", 
                "Common.Core.4722", "Common.Core.4723",
                // Xml rule list
                "Entry.Core.4065", "Entry.Core.4066", "Entry.Core.4089", "Entry.Core.4090", "Entry.Core.4093", "Entry.Core.4098", 
                "Entry.Core.4103", "Entry.Core.4301", 
            };

            List<string> expectedRuleList_People_MinOnly = new List<string>()
            {
                // Code rule list
                "Entry.Core.4241", "Entry.Core.4320", "Entry.Core.4322", "Entry.Core.4328", "Entry.Core.4329",
                // Common rule list
                "Common.Core.4044", "Common.Core.4502", 
            };

            List<string> expectedRuleList_Photo_FullOnly = new List<string>()
            {
                // Code rule list
                "Entry.Core.4057", 
                // Common rule list
                "Common.Core.4054", "Common.Core.4309"
            };

            List<string> expectedRuleList_Products_FullOnly = new List<string>() 
            {
                // Code rule list
                "Entry.Core.4078", "Entry.Core.4081", "Entry.Core.4084", 
                // Common rule ist
                "Common.Core.4029", 
            };

            List<string> expectedRuleList_PersonDetails_FullOnly = new List<string>()
            {
                // Code rule list
                "Entry.Core.4079",
            };

            List<string> expectedRuleList_People_Multi_Expand_FullOnly = new List<string>()
            {
                // Code rule list
                "Entry.Core.4108", "Entry.Core.4109", "Entry.Core.4337", "Entry.Core.4338", 
            };

            List<string> expectedRuleList_People_One_Expand_FullOnly = new List<string>()
            {
                // Code rule list
                "Entry.Core.4104", "Entry.Core.4105", "Entry.Core.4106", 
            };


            List<string> expectedRuleList_People_Null_Expand_FullOnly = new List<string>()
            {
                // Code rule list
                "Entry.Core.4107", "Entry.Core.4326", 
            };
            List<string> expectedRuleList_People_Null_Expand_Array_FullOnly = new List<string>()
            {
                // Code rule list
                "Entry.Core.4110", "Entry.Core.4502", 
            };

            List<string> expectedRuleList_FeaturedProduct_Full = new List<string>()
            {
                // Code rule list
                "Entry.Core.4310", 
                // Common rule list
                "Common.Core.4027", 
            };

            List<string> expectedRuleList_Airports_Full = new List<string>()
            {
                // Code rule list
                "Entry.Core.4323", "Entry.Core.4324", "Entry.Core.4325", "Entry.Core.4577",
            };

            List<string> expectedRuleList_Persons_Full = new List<string>()
            {
                // Common rule list
                "Common.Core.4500", "Common.Core.4506",
            };
            List<string> expectedRuleList_People_None = new List<string>()
            {
                // Common rule list
                "Common.Core.4700", 
            };

            List<string> negativeRuleListRecomm_FullOnly = new List<string>()
            {
                // == May rules(recommendation) - not need to verify ==
                // Code rule list
                "Entry.Core.4125", "Entry.Core.4168", "Entry.Core.4509", "Entry.Core.4510", "Entry.Core.4511", "Entry.Core.4512",
                // Common rule list
                "Common.Core.4006", "Common.Core.4516"
            };

            List<string> negativeRuleListNotApp_FullOnly = new List<string>()
            {
                // == May rules(notApplicable) - not need to verify ==
                // Code rule list
                "Entry.Core.4060", "Entry.Core.4063", "Entry.Core.4088", "Entry.Core.4111", "Entry.Core.4112", "Entry.Core.4339",
                // Common rule list
                "Common.Core.4010", "Common.Core.4015",
                // TODO: Must rules(notApplicable) - There should be no such data 
                "Common.Core.4037"
            };

            List<string> negativeRuleListRecomm_MinOnly = new List<string>()
            {
                // == May rules(recommendation) - not need to verify ==
                // Code rule list
                "Entry.Core.4507", "Entry.Core.4508",
            };

            Dictionary<List<string>, string> negativeDic_FullOnly = new Dictionary<List<string>, string>();
            negativeDic_FullOnly.Add(negativeRuleListRecomm_FullOnly, ValidationResultConstants.Recommendation);
            negativeDic_FullOnly.Add(negativeRuleListNotApp_FullOnly, ValidationResultConstants.NotApplicable);

            Dictionary<List<string>, string> negativeDic_MinOnly = new Dictionary<List<string>, string>();
            negativeDic_MinOnly.Add(negativeRuleListRecomm_MinOnly, ValidationResultConstants.Recommendation);

            List<ValidationElement> verifyAndNegativeList = new List<ValidationElement>()
            {
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_People_Full), expectedRuleList_People_FullOnly, negativeDic_FullOnly),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_People_Minimal_WithUncomputedLink, FormatConstants.V4FormatJsonMinimalMetadata), expectedRuleList_People_MinOnly, negativeDic_MinOnly),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_Photos_Full), expectedRuleList_Photo_FullOnly),
                new ValidationElement(new RequestElement(ODataSvc_URLConstants.URL_Entity_Products_Full), expectedRuleList_Products_FullOnly),
                new ValidationElement(new RequestElement(ODataSvc_URLConstants.URL_Entity_PersonDetails_Full), expectedRuleList_PersonDetails_FullOnly),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_People_Expand_MultiEntity), expectedRuleList_People_Multi_Expand_FullOnly),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_People_Expand_NullEntity), expectedRuleList_People_Null_Expand_FullOnly),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_People_Expand_NullEntity_Array), expectedRuleList_People_Null_Expand_Array_FullOnly),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_People_Expand_OneEntity), expectedRuleList_People_One_Expand_FullOnly),
                new ValidationElement(new RequestElement(ODataSvc_URLConstants.URL_Entity_FeaturedProduct_Full), expectedRuleList_FeaturedProduct_Full),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_Airports_Full), expectedRuleList_Airports_Full),
                new ValidationElement(new RequestElement(ODataSvc_URLConstants.URL_Entity_Persons_Full), expectedRuleList_Persons_Full),
                new ValidationElement(new RequestElement(TripPinSvc_URLConstants.URL_Entity_People_None, FormatConstants.V4FormatJsonNoMetadata), expectedRuleList_People_None),
            };

            this.VerifyRules_BaseTestCase(verifyAndNegativeList);
        }

        #endregion
    }
}
