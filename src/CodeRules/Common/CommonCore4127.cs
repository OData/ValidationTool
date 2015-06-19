// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule
{
    #region Namespaces
    using System;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Newtonsoft.Json.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Class of code rule applying to entry payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4127_Entry : CommonCore4127
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Entry;
            }
        }
    }

    /// <summary>
    /// Class of code rule applying to feed payload
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public class CommonCore4127_Feed : CommonCore4127
    {
        /// <summary>
        /// Gets the payload type to which the rule applies.
        /// </summary>
        public override PayloadType? PayloadType
        {
            get
            {
                return RuleEngine.PayloadType.Feed;
            }
        }
    }

    /// <summary>
    /// Class of extension rule for Common.Core.4127
    /// </summary>
    [Export(typeof(ExtensionRule))]
    public abstract class CommonCore4127 : ExtensionRule
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
                return "Common.Core.4127";
            }
        }

        /// <summary>
        /// Gets rule description
        /// </summary>
        public override string Description
        {
            get
            {
                return @"When annotating a name/value pair for which the value is represented as a JSON array or primitive value, each annotation that applies to this name/value pair MUST be placed next to the annotated name/value pair and represented as a single name/value pair.";
            }
        }

        /// <summary>
        /// Gets rule specification in OData document
        /// </summary>
        public override string V4SpecificationSection
        {
            get
            {
                return "18.2";
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override ODataVersion? Version
        {
            get
            {
                return ODataVersion.V3_V4;
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
        /// Gets the payload format to which the rule applies.
        /// </summary>
        public override PayloadFormat? PayloadFormat
        {
            get
            {
                return RuleEngine.PayloadFormat.JsonLight;
            }
        }

        /// <summary>
        /// Gets the odata metadata type to which the rule applies.
        /// </summary>
        public override ODataMetadataType? OdataMetadataType
        {
            get
            {
                return RuleEngine.ODataMetadataType.FullOnly;
            }
        }

        /// <summary>
        /// Gets the RequireMetadata property to which the rule applies.
        /// </summary>
        public override bool? RequireMetadata
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the IsOfflineContext property to which the rule applies.
        /// </summary>
        public override bool? IsOfflineContext
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Verifies the extension rule.
        /// </summary>
        /// <param name="context">The Interop service context</param>
        /// <param name="info">out parameter to return violation information when rule does not pass</param>
        /// <returns>true if rule passes; false otherwise</returns>
        public override bool? Verify(ServiceContext context, out ExtensionRuleViolationInfo info)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool? passed = null;
            info = null;
            bool isDefinedInMetadata = false;
            bool isAnnotatingProNext = false;
            bool isAnnotatingProPrevious = false;

            JObject allobject;
            context.ResponsePayload.TryToJObject(out allobject);

            // Access the metadata document and get the node which will be used.
            string xpath = string.Format(@"//*[local-name()='EntityType']/*[local-name()='Property']", context.EntityTypeShortName);
            XElement metadata = XElement.Parse(context.MetadataDocument);
            var propNodesList = metadata.XPathSelectElements(xpath, ODataNamespaceManager.Instance);

            // If PayloadType is Feed, verify as below, else is Entry.
            if (context.PayloadType.Equals(RuleEngine.PayloadType.Feed))
            {
                var entries = JsonParserHelper.GetEntries(allobject);
                foreach (JObject entry in entries)
                {
                    var jProps = entry.Children();

                    foreach (JProperty jProp in jProps)
                    {
                        // Whether json property is annotation.
                        if (JsonSchemaHelper.IsAnnotation(jProp.Name) && !jProp.Name.StartsWith("@"))
                        {
                            if (jProp.Name.Contains("@"))
                            {
                                // Parse the annotation name to two parts.
                                string annotatedPro = jProp.Name.Remove(jProp.Name.LastIndexOf("@"), (jProp.Name.Length - jProp.Name.LastIndexOf("@")));

                                JProperty nextJProperty = ((JProperty)jProp.Next);
                                JProperty previousJProperty = ((JProperty)jProp.Previous);

                                // Compare the navigation properties of navigation links,
                                foreach (var propNode in propNodesList)
                                {
                                    if (propNode.Attribute("Name").ToString().Contains(annotatedPro))
                                    {
                                        isDefinedInMetadata = true;
                                        break;
                                    }
                                    else
                                    {
                                        isDefinedInMetadata = false;
                                    }
                                }

                                // Whether the property defined in metadata.
                                if (isDefinedInMetadata && nextJProperty != null)
                                {
                                    if (nextJProperty.Name.ToString().Equals(annotatedPro))
                                    {
                                        if ((nextJProperty.Value.Type == JTokenType.Array) || (nextJProperty.Value.Type != JTokenType.Object))
                                        {
                                            isAnnotatingProNext = true;
                                        }
                                        else if ((nextJProperty.Value.Type == JTokenType.Object))
                                        {
                                            if ((context.Version == ODataVersion.V3) && (jProp.Value.ToString().StripOffDoubleQuotes().StartsWith("Edm.Geography") || jProp.Value.ToString().StripOffDoubleQuotes().StartsWith("Edm.Geometry")))
                                            {
                                                isAnnotatingProNext = true;
                                            }
                                            else if ((context.Version == ODataVersion.V4) && (jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#').StartsWith("Geography") || jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#').StartsWith("Geometry")))
                                            {
                                                isAnnotatingProNext = true;
                                            }
                                            else
                                            {
                                                isAnnotatingProNext = false;
                                            }
                                        }
                                    }
                                    else if (previousJProperty.Name.ToString().Equals(annotatedPro))
                                    {
                                        if ((previousJProperty.Value.Type == JTokenType.Array) || (nextJProperty.Value.Type != JTokenType.Object))
                                        {
                                            isAnnotatingProPrevious = true;
                                        }
                                        else if ((previousJProperty.Value.Type == JTokenType.Object))
                                        {
                                            if ((context.Version == ODataVersion.V3) && (jProp.Value.ToString().StripOffDoubleQuotes().StartsWith("Edm.Geography") || jProp.Value.ToString().StripOffDoubleQuotes().StartsWith("Edm.Geometry")))
                                            {
                                                isAnnotatingProNext = true;
                                            }
                                            else if ((context.Version == ODataVersion.V4) && (jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#').StartsWith("Geography") || jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#').StartsWith("Geometry")))
                                            {
                                                isAnnotatingProNext = true;
                                            }
                                            else
                                            {
                                                isAnnotatingProNext = false;
                                            }
                                        }
                                    }

                                    // If isAnnotatingProNext or isAnnotatingProPrevious is true, it means the name and value of annotation is not null.
                                    if (isAnnotatingProNext || isAnnotatingProPrevious)
                                    {
                                        passed = true;
                                    }
                                    else
                                    {
                                        passed = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (allobject != null)
                {
                    var jProps = allobject.Children();

                    // New a string to record the annotation name.
                    string recordName = string.Empty;
                    string recordValue = string.Empty;

                    foreach (JProperty jProp in jProps)
                    {
                        // Whether json property is annotation.
                        if (JsonSchemaHelper.IsAnnotation(jProp.Name) && !jProp.Name.StartsWith("@"))
                        {
                            if (jProp.Name.Contains("@"))
                            {
                                // Prase the annotation name to two parts.
                                string annotatedPro = jProp.Name.Remove(jProp.Name.LastIndexOf("@"), (jProp.Name.Length - jProp.Name.LastIndexOf("@")));

                                JProperty nextJProperty = ((JProperty)jProp.Next);
                                JProperty previousJProperty = ((JProperty)jProp.Previous);

                                // Compare the navigation properties of navigation links,
                                foreach (var propNode in propNodesList)
                                {
                                    if (propNode.Attribute("Name").ToString().Contains(annotatedPro))
                                    {
                                        isDefinedInMetadata = true;
                                        break;
                                    }
                                    else
                                    {
                                        isDefinedInMetadata = false;
                                    }
                                }

                                // Whether the property defined in metadata.
                                if (isDefinedInMetadata && nextJProperty != null)
                                {
                                    if (nextJProperty.Name.ToString().Equals(annotatedPro))
                                    {
                                        if ((nextJProperty.Value.Type == JTokenType.Array) || (nextJProperty.Value.Type != JTokenType.Object))
                                        {
                                            isAnnotatingProNext = true;
                                        }
                                        else if ((nextJProperty.Value.Type == JTokenType.Object))
                                        {
                                            if ((context.Version == ODataVersion.V3) && (jProp.Value.ToString().StripOffDoubleQuotes().StartsWith("Edm.Geography") || jProp.Value.ToString().StripOffDoubleQuotes().StartsWith("Edm.Geometry")))
                                            {
                                                isAnnotatingProNext = true;
                                            }
                                            else if ((context.Version == ODataVersion.V4) && (jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#').StartsWith("Geography") || jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#').StartsWith("Geometry")))
                                            {
                                                isAnnotatingProNext = true;
                                            }
                                            else
                                            {
                                                isAnnotatingProNext = false;
                                            }
                                        }
                                    }
                                    else if (previousJProperty.Name.ToString().Equals(annotatedPro))
                                    {
                                        if ((previousJProperty.Value.Type == JTokenType.Array) || (nextJProperty.Value.Type != JTokenType.Object))
                                        {
                                            isAnnotatingProPrevious = true;
                                        }
                                        else if ((previousJProperty.Value.Type == JTokenType.Object))
                                        {
                                            if ((context.Version == ODataVersion.V3) && (jProp.Value.ToString().StripOffDoubleQuotes().StartsWith("Edm.Geography") || jProp.Value.ToString().StripOffDoubleQuotes().StartsWith("Edm.Geometry")))
                                            {
                                                isAnnotatingProNext = true;
                                            }
                                            else if ((context.Version == ODataVersion.V4) && (jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#').StartsWith("Geography") || jProp.Value.ToString().StripOffDoubleQuotes().TrimStart('#').StartsWith("Geometry")))
                                            {
                                                isAnnotatingProNext = true;
                                            }
                                            else
                                            {
                                                isAnnotatingProNext = false;
                                            }
                                        }
                                    }

                                    // If isAnnotatingProNext or isAnnotatingProPrevious is true, it means the name and value of annotation is not null.
                                    if (isAnnotatingProNext || isAnnotatingProPrevious)
                                    {
                                        passed = true;
                                    }
                                    else
                                    {
                                        passed = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (passed == false)
            {
                info = new ExtensionRuleViolationInfo(this.ErrorMessage, context.Destination, context.ResponsePayload);
            }

            return passed;
        }
    }
}
