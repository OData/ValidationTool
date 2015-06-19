// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;

    /// <summary>
    /// Class of extension rule for Metadata.Core.4382
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class MetadataCore4382 : ExtensionRule
    {
        /// <summary>
        /// Gets Category property
        /// </summary>
        public override string Category
        {
            get
            {
                return "core";
            }
        }

        /// <summary>
        /// Gets rule name
        /// </summary>
        public override string Name
        {
            get
            {
                return "Metadata.Core.4382";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"The target of the annotation MUST comply with any AppliesTo constraint.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string SpecificationSection
        {
            get
            {
                return "14.3.1";
            }
        }

        /// <summary>
        /// Gets location of help information of the rule
        /// </summary>
        public override string HelpLink
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the error message for validation failure
        /// </summary>
        public override string ErrorMessage
        {
            get
            {
                return this.Description;
            }
        }

        /// <summary>
        /// Gets the requirement level.
        /// </summary>
        public override RequirementLevel RequirementLevel
        {
            get
            {
                return RequirementLevel.Must;
            }
        }

        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Metadata;
            }
        }

        /// <summary>
        /// Gets the offline context to which the rule applies
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.Xml;
            }
        }

        /// <summary>
        /// Gets the OData version
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V4;
            }
        }

        /// <summary>
        /// Verify Metadata.Core.4382
        /// </summary>
        /// <param name="context">Service context</param>
        /// <param name="info">out parameter to return violation information when rule fail</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;

            // Create a table for all the terms that conatins AppliesTo attribute
            Dictionary<string, string[]> termAppliesToTable = new Dictionary<string,string[]>();

            XmlDocument fullMetadata = new XmlDocument();
            fullMetadata.LoadXml(context.ContainsExternalSchema ? context.MergedMetadataDocument : context.MetadataDocument);

            XmlNodeList termCollection = fullMetadata.SelectNodes(@"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema' and @Namespace]/*[local-name()='Term' and @Name and @AppliesTo]");
            foreach (XmlNode term in termCollection)
            {
                string qualifiedName = term.ParentNode.Attributes["Namespace"].Value + "." + term.Attributes["Name"].Value;
                string[] appliesTo = term.Attributes["AppliesTo"].Value.Split(' ');
                termAppliesToTable.Add(qualifiedName, appliesTo);

                if (term.ParentNode.Attributes["Alias"] != null)
                {
                    qualifiedName = term.ParentNode.Attributes["Alias"].Value + "." + term.Attributes["Name"].Value;
                    termAppliesToTable.Add(qualifiedName, appliesTo);
                }
            }
            
            string annotationXpath = @"//*[local-name()='Annotation' and @Term]";
            XmlDocument metadata = new XmlDocument();
            metadata.LoadXml(context.MetadataDocument);
            XmlNodeList annotationCollection = metadata.SelectNodes(annotationXpath);

            // if a annotation's Term attribute is contained in termAppliesToTable, check whether the annotation target is right.
            foreach (XmlNode annotation in annotationCollection)
            {
                string[] appliesTo;
                if (!termAppliesToTable.TryGetValue(annotation.Attributes["Term"].Value, out appliesTo))
                {
                    continue;
                }
                
                if (annotation.ParentNode.LocalName == "Annotations")
                {
                    // If this annotation is child element of Annotations, wes hould resolve Annotations' Target attribute.
                    if (annotation.ParentNode.Attributes["Target"] == null)
                    {
                        continue;
                    }

                    XmlNode resolvedResult = null;
                    if (ResolveToModelElement(annotation.ParentNode.Attributes["Target"].Value, fullMetadata, out resolvedResult)
                        && termAppliesToTable[annotation.Attributes["Term"].Value].Contains<string>(resolvedResult.LocalName))
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false; break;
                    }
                }
                else
                {
                    // If this annotation is not child element of Annotations, the parent node is its target.
                    if (termAppliesToTable[annotation.Attributes["Term"].Value].Contains<string>(annotation.ParentNode.LocalName))
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false; break;
                    }
                }
            }

            info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            return passed;
        }

        private bool ResolveToModelElement(string path, XmlNode metadata, out XmlNode result)
        {
            result = null;
            string[] pathes = path.Split('/');

            // Parse the first part of the path.
            int dotIndex = pathes[0].LastIndexOf('.');
            if (dotIndex == 0 || dotIndex == -1 || dotIndex == pathes[0].Length - 1)
            {
                return false;
            }
            string ns = pathes[0].Substring(0, dotIndex);
            string name = pathes[0].Substring(dotIndex + 1);
            result = metadata.SelectSingleNode(string.Format(@"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{0}')]/*[@Name='{1}']", ns, name));
            if (result == null)
            {
                return false;
            }

            // Parse the left parts.
            for (int i = 1; i < pathes.Length; )
            {
                // For EntitySet we should find its EntityType first, then continue finding in the EntityType.
                if (result.LocalName == "EntitySet")
                {
                    if (result.Attributes["EntityType"].Value.Equals(pathes[i]))
                    {
                        // We should not excute i++ if EntityType is not a part of the path.
                        i++;
                    }
                    if (result.Attributes["EntityType"] == null)
                    {
                        return false;
                    }
                    dotIndex = result.Attributes["EntityType"].Value.LastIndexOf('.');
                    if (dotIndex == 0 || dotIndex == -1 || dotIndex == pathes[0].Length - 1)
                    {
                        return false;
                    }
                    ns = result.Attributes["EntityType"].Value.Substring(0, dotIndex);
                    name = result.Attributes["EntityType"].Value.Substring(dotIndex + 1);
                    result = metadata.SelectSingleNode(string.Format(@"/*[local-name()='Edmx']/*[local-name()='DataServices']/*[local-name()='Schema' and (@Namespace='{0}' or @Alias='{0}')]/*[@Name='{1}']", ns, name));
                    if (result == null)
                    {
                        return false;
                    }
                }
                else
                {
                    // If not EntitySet, we just only need to check in its child nodes.
                    result = result.SelectSingleNode(string.Format(@"./*[@Name='{0}']", pathes[i]));
                    if (result == null)
                    {
                        return false;
                    }
                    i++;
                }
            }

            return true;
        }
    }
}

