// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using System;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// Base class to help with Customized Feed Property
    /// </summary>
    abstract class CustomizedFeedProperty
    {
        public string fcTargetPath { get; private set; }
        public string fc_KeepInContent { get; private set; }
        public string fc_ContentKind { get; private set; }

        public bool isAtomSpecific { get; private set; }
        protected bool isAttributeTarget { get; private set; }

        public string[] targetPath { get; private set; }
        public string XPathTarget { get; private set; }

        private XElement entry;
        private XElement node;
        public IXmlNamespaceResolver nsResolver { get; private set; }
        private string extraNamespaceDecl;

        protected bool IsPropertyInContent { get; private set; }
        protected string PropertyInContent { get; private set; }
        protected bool IsPropertyInTarget { get; private set; }
        protected string PropertyInTarget { get; private set; }

        public CustomizedFeedProperty(XElement nodeDecl, XElement entry)
        {
            this.entry = entry;
            this.node = nodeDecl;

            this.fcTargetPath = nodeDecl.GetAttributeValue("m:FC_TargetPath", ODataNamespaceManager.Instance);
            this.fc_KeepInContent = nodeDecl.GetAttributeValue("m:FC_KeepInContent", ODataNamespaceManager.Instance);
            if (string.IsNullOrEmpty(fc_KeepInContent))
            {
                this.fc_KeepInContent = "true"; // default setting
            }
            this.fc_ContentKind = nodeDecl.GetAttributeValue("m:FC_ContentKind", ODataNamespaceManager.Instance);
            if (string.IsNullOrEmpty(this.fc_ContentKind))
            {
                this.fc_ContentKind = "text";    //default setting
            }

            string target;
            this.isAtomSpecific = AtomTargetMapping.TryGetTarget(this.fcTargetPath, out target);

            if (isAtomSpecific)
            {
                this.targetPath = target.Split('/');
                this.nsResolver = ODataNamespaceManager.Instance;
                extraNamespaceDecl = string.Empty;
            }
            else
            {
                this.targetPath = fcTargetPath.Split('/');

                string fc_NsPrefix = nodeDecl.GetAttributeValue("m:FC_NsPrefix", ODataNamespaceManager.Instance);
                string fc_NsUri = nodeDecl.GetAttributeValue("m:FC_NsUri", ODataNamespaceManager.Instance);
                if (string.IsNullOrEmpty(fc_NsPrefix))
                {
                    fc_NsPrefix = "ioftNs_wfnqpz"; //just make up a random one
                }
                this.AddNsPrefixToTarget(fc_NsPrefix);

                // a custom resolver is needed sice there is non standard namespace definition
                this.nsResolver = CreateCustomNSResolver(fc_NsPrefix, fc_NsUri);
                this.extraNamespaceDecl = string.Format(@"xmlns:{0}=""{1}""", fc_NsPrefix, fc_NsUri);
            }

            this.isAttributeTarget = IsAttributeTarget(this.targetPath[this.targetPath.Length - 1]);

            this.ProcessPropertyInContent();
            this.ProcessPropertyInTarget();
        }

        private static IXmlNamespaceResolver CreateCustomNSResolver(string fc_NsPrefix, string fc_NsUri)
        {
            XmlNamespaceManager resolver = new XmlNamespaceManager(new NameTable());
            resolver.AddNamespace("app", "http://www.w3.org/2007/app");
            resolver.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            resolver.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            resolver.AddNamespace(fc_NsPrefix, fc_NsUri);
            return resolver;
        }

        private static bool IsAttributeTarget(string node)
        {
            return node.StartsWith("@");
        }

        private void ProcessPropertyInContent()
        {
            if (fc_KeepInContent.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                this.IsPropertyInContent = false;
                this.PropertyInContent = null;
            }
            else
            {
                // get the property value under m:properties
                string pname = this.node.GetAttributeValue("Name");
                string xp = string.Format(".//m:properties/*[local-name()='{0}']", pname);
                var p = this.entry.XPathSelectElement(xp, ODataNamespaceManager.Instance);
                this.IsPropertyInContent = p != null;
                this.PropertyInContent = (p != null) ? p.UnescapeElementValue() : null;
            }
        }

        private void AddNsPrefixToTarget(string fc_NsPrefix)
        {
            for (int i = 0; i < targetPath.Length; i++)
            {
                if (IsAttributeTarget(targetPath[i]))
                {
                    targetPath[i] = "@" + fc_NsPrefix + ":" + targetPath[i].Substring(1);
                }
                else
                {
                    targetPath[i] = fc_NsPrefix + ":" + targetPath[i];
                }
            }
        }

        private void ProcessPropertyInTarget()
        {
            if (!this.isAttributeTarget)
            {
                this.XPathTarget = "./" + string.Join("/", this.targetPath);
                var n = entry.XPathSelectElement(this.XPathTarget, this.nsResolver);
                this.IsPropertyInTarget = n != null;
                this.PropertyInTarget = (n != null) ? n.UnescapeElementValue() : null;
            }
            else
            {
                string xpath = "./" + string.Join("/", this.targetPath.Take(this.targetPath.Length - 1));
                string attrName = this.targetPath[this.targetPath.Length - 1].Substring(1);

                this.XPathTarget = xpath + "[@" + attrName + "]";

                var n = this.entry.XPathSelectElement(xpath, this.nsResolver);

                this.IsPropertyInTarget = false;
                this.PropertyInTarget = null;
                if (n != null)
                {
                    // get attribute name with namespace name
                    string fullName = attrName;
                    string[] name = attrName.Split(new char[] { ':' }, 2);
                    if (name.Length == 2)
                    {
                        var ns = this.nsResolver.LookupNamespace(name[0]);
                        fullName = string.Format("{{{0}}}{1}", ns, name[1]);
                    }

                    this.IsPropertyInTarget = n.Attribute(fullName) != null;
                    if (this.IsPropertyInTarget)
                    {
                        this.PropertyInTarget = n.GetAttributeValue(fullName);
                    }
                }
            }
        }

        protected abstract string GetRngCoreNode();

        public string GetRngSchema()
        {
            // construct the rng schema based on the target path & type attribute
            string core = this.GetRngCoreNode();

            string output = core;
            foreach (var t in this.targetPath.Take(this.targetPath.Length - 1).Reverse())
            {
                output = string.Format(CustomizedFeedProperty.miniFormat, t, output);
            }

            string schema = string.Format(CustomizedFeedProperty.formatRng,
                extraNamespaceDecl,
                this.targetPath[0],
                output,
                RngCommonPattern.CommonPatterns);

            return schema;
        }

        private const string miniFormat = @"<element name=""{0}""><interleave><ref name=""anyContent""/>{1}</interleave></element>";

        private const string formatRng = @"<grammar xmlns=""http://relaxng.org/ns/structure/1.0""
         xmlns:atom=""http://www.w3.org/2005/Atom""
         xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" {0}>

  <start>
    <ref name=""an_entry""/>
  </start>

  <define name=""an_entry"">
    <element>
      <anyName/>
      <ref name=""anyAttributes""/>
      <mixed>
        <ref name=""myElements"" />
      </mixed>
    </element>
  </define>

    <define name=""myElements"" combine=""interleave"">
      <zeroOrMore>
		<element>
			<anyName>
				<except>
					<name>{1}</name>
				</except>
			</anyName>
			<ref name=""anyAttributes"" />
			<mixed><ref name=""anyContent""/></mixed>
		</element>
      </zeroOrMore>
    </define>

    <define name=""myElements"" combine=""interleave"">
        {2}
    </define>

    {3}
</grammar>
";
    }
}
