// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Xml.Linq;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The class to extract validation rules from xml files under the folder
    /// </summary>
    public class RuleStoreAsXmlFile : IRuleStore
    {
        /// <summary>
        /// Path to the xml rule file
        /// </summary>
        private string filePath;

        /// <summary>
        /// Logger object to log runtime errors
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Creates an instance of RuleStoreAsXmlFile from the xml file path.
        /// </summary>
        /// <param name="path">XML file path</param>
        public RuleStoreAsXmlFile(string path)
            : this(path, null)
        {
        }

        /// <summary>
        /// Creates an instance of RuleStoreAsXmlFile from the xml file path and ILogger object.
        /// </summary>
        /// <param name="path">XML file path</param>
        /// <param name="logger">The ILogger object to log runtime errors</param>
        public RuleStoreAsXmlFile(string path, ILogger logger)
        {
            this.filePath = path;
            this.logger = logger;
        }

        /// <summary>
        /// Gets rules from the rule store
        /// </summary>
        /// <returns>Collection of rules</returns>
        [SuppressMessage("DataWeb.Usage", "AC0014:DoNotHandleProhibitedExceptionsRule", Justification = "Taken care of by similar mechanism")]
        public IEnumerable<Rule> GetRules()
        {
            IEnumerable<Rule> rules = null;

            try
            {
                using (FileStream stream = File.OpenRead(this.filePath))
                {
                    XElement xml = XElement.Load(stream);
                    rules = CreateRules(xml);
                }
            }
            catch (Exception ex)
            {
                if (!ExceptionHelper.IsCatchableExceptionType(ex))
                {
                    throw;
                }

                RuntimeException.WrapAndLog(ex, Resource.InvalidXmlRule + ":" + this.filePath, this.logger);
            }

            return rules;
        }

        /// <summary>
        /// Gets rules defined under the specified XML node
        /// </summary>
        /// <param name="xml">The parent xml node contains all the rules</param>
        /// <returns>Collection of rules</returns>
        private static IEnumerable<Rule> CreateRules(XElement xml)
        {
            foreach (var e in xml.Descendants("rule"))
            {
                var rule = CreateRule(e);
                if (rule != null && rule.IsValid())
                {
                    yield return rule;
                }
            }
        }

        /// <summary>
        /// Gets one rule as defined by the xml node
        /// </summary>
        /// <param name="x">Xml node which defines the rule</param>
        /// <returns>Rule as defined by xml node</returns>
        private static Rule CreateRule(XElement x)
        {
            try
            {
                string name = x.GetAttributeValue("id");
                string specificationSection = x.GetAttributeValue("specificationsection");
                string v4specificationSection = x.GetAttributeValue("v4specificationsection");
                string v4specification = x.GetAttributeValue("v4specification");
                string requirementLevel = x.GetAttributeValue("requirementlevel");
                string category = x.GetAttributeValue("category");
                string useCase = x.GetAttributeValue("usecase");
                string respVersion = x.GetAttributeValue("version");
                string target = x.GetAttributeValue("target");
                string format = x.GetAttributeValue("format");
                string mle = x.GetAttributeValue("mle");
                string projection = x.GetAttributeValue("projection");
                string metadata = x.GetAttributeValue("metadata");
                string serviceDocument = x.GetAttributeValue("svcdoc");
                string description = x.GetFirstSubElementValue("description");
                string errorMessage = x.GetFirstSubElementValue("errormessage");
                string offline = x.GetAttributeValue("offline");
                string odatametadatatype = x.GetAttributeValue("odatametadatatype");
                string conformancetype = x.GetAttributeValue("conformancetype");

                string helpLink = (x.Element("helplink") != null && x.Element("helplink").Element("a") != null) ?
                    x.Element("helplink").Element("a").GetAttributeValue("href") :
                    null;

                if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(name))
                {
                    return null;
                }
                else
                {
                    Rule rule = new Rule();
                    rule.Name = name;
                    rule.SpecificationSection = specificationSection;
                    rule.V4SpecificationSection = v4specificationSection;
                    rule.V4Specification = v4specification;
                    rule.HelpLink = helpLink;
                    rule.Category = category;
                    rule.PayloadType = target.ToNullable<PayloadType>();

                    if (!string.IsNullOrEmpty(mle) 
                        && rule.PayloadType.HasValue 
                        && rule.PayloadType.Value == PayloadType.Entry)
                    {
                        rule.IsMediaLinkEntry = mle.ToNullableBool();
                    }

                    if (!string.IsNullOrEmpty(projection)
                        && rule.PayloadType.HasValue
                        && (rule.PayloadType.Value == PayloadType.Entry || rule.PayloadType.Value == PayloadType.Feed))
                    {
                        rule.Projection = projection.ToNullableBool();
                    }

                    rule.RequireMetadata = metadata.ToNullableBool();
                    rule.RequireServiceDocument = serviceDocument.ToNullableBool();
                    rule.Description = description;
                    rule.ErrorMessage = errorMessage;
                    rule.RequirementLevel = (RequirementLevel)Enum.Parse(typeof(RequirementLevel), requirementLevel, true);
                    rule.Aspect = useCase;
                    rule.Version = respVersion.ToNullable<ODataVersion>();
                    rule.Offline = offline.ToNullableBool();
                    rule.PayloadFormat = format.ToNullable<PayloadFormat>();
                   
                    if (!string.IsNullOrEmpty(odatametadatatype))
                    {
                        rule.OdataMetadataType = (ODataMetadataType)Enum.Parse(typeof(ODataMetadataType), odatametadatatype, true);
                    }

                    SetRuleCondition(ref rule, x.Element("condition"));
                    SetRuleAction(ref rule, x.Element("action"));

                    return rule;
                }
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Sets condition property of the specified rule from the xml node
        /// </summary>
        /// <param name="rule">The specified rule to have its condition property be set</param>
        /// <param name="condition">xml node containing the condition verification</param>
        private static void SetRuleCondition(ref Rule rule, XElement condition)
        {
            if (condition != null)
            {
                rule.Condition = VerifierFactory.Create(condition);
            }
        }

        /// <summary>
        /// Sets action property of the specified rule from the xml node
        /// </summary>
        /// <param name="rule">The specified rule to have its action property be set</param>
        /// <param name="action">xml node containing the action verification</param>
        private static void SetRuleAction(ref Rule rule, XElement action)
        {
            rule.Action = VerifierFactory.Create(action);
        }
    }
}
