// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using ODataValidator.Rule.Helper;
    using ODataValidator.RuleEngine.Common;
    #endregion

    /// <summary>
    /// The NavigateTreeNode class.
    /// </summary>
    public class NavigateTreeNode
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">The navigation related data.</param>
        public NavigateTreeNode(NavigationData data)
        {
            this.data = data;
            this.children = new List<NavigateTreeNode>();
        }

        /// <summary>
        /// Gets the navigation related data.
        /// </summary>
        public NavigationData Data
        {
            get
            {
                return this.data;
            }
        }

        /// <summary>
        /// Gets the parent node of the current navigation tree node.
        /// </summary>
        public NavigateTreeNode Parent
        {
            get
            {
                return this.parent;
            }
            private set
            {
                this.parent = value;
            }
        }

        /// <summary>
        /// Gets the children of the current navigation tree node.
        /// </summary>
        public List<NavigateTreeNode> Children
        {
            get
            {
                return this.children;
            }
        }

        /// <summary>
        /// Add the child to the navigation tree node.
        /// </summary>
        /// <param name="child">The child node.</param>
        public void AddChild(NavigateTreeNode child)
        {
            if (null != child)
            {
                this.children.Add(child);
            }
        }

        /// <summary>
        /// Add the children to the navigation tree node.
        /// </summary>
        /// <param name="children">The child nodes.</param>
        public void AddChildren(IEnumerable<NavigateTreeNode> children)
        {
            if (null != children && children.Any())
            {
                this.children.AddRange(children);
            }
        }

        /// <summary>
        /// Search the navigation property tree node with the specifed information.
        /// </summary>
        /// <param name="navigationPropertyName">The navigation property's name.</param>
        /// <param name="entityTypeShortName">The short name of an entity type which contains the navigation property.</param>
        /// <returns>Returns navigation property tree node with the specified information.</returns>
        public NavigateTreeNode Search(string navigationPropertyName, string entityTypeShortName = null)
        {
            if (string.IsNullOrEmpty(navigationPropertyName))
            {
                return null;
            }

            var queue = new Queue<NavigateTreeNode>();
            if (navigationPropertyName == this.data.Name)
            {
                return this;
            }
            else
            {
                this.children.ForEach(c => { queue.Enqueue(c); });
            }

            while (queue.Any())
            {
                var elem = queue.Dequeue();
                if (navigationPropertyName == elem.data.Name)
                {
                    var entityTypeShortNames = entityTypeShortName.GetEntityTypeShortNamesInEveryGeneration();
                    if (string.IsNullOrEmpty(entityTypeShortName) || entityTypeShortNames.Contains(elem.parent.data.TypeShortName))
                    {
                        return elem;
                    }
                }
                else
                {
                    elem.children.ForEach(c => { queue.Enqueue(c); });
                }
            }

            return null;
        }

        /// <summary>
        /// Parse to a navigation tree node using the entity-type short name.
        /// </summary>
        /// <param name="entityTypeShortName"></param>
        /// <returns></returns>
        public static NavigateTreeNode Parse(string entityTypeShortName)
        {
            NavigateTreeNode.Path = string.Empty;

            // Construct the root node.
            var navigData = new NavigationData("root", entityTypeShortName, string.Empty, string.Empty, false, string.Empty, false);
            var rootNode = new NavigateTreeNode(navigData);
            rootNode.parent = null;

            return Construct(rootNode);
        }

        #region Private members.
        private NavigationData data;
        private NavigateTreeNode parent;
        private List<NavigateTreeNode> children;

        private static string Path;

        /// <summary>
        /// Construct all the child nodes for the current navigation tree node.
        /// </summary>
        /// <param name="node">The current navigation tree node.</param>
        /// <returns>Returns the current node with all its child nodes.</returns>
        private static NavigateTreeNode Construct(NavigateTreeNode node)
        {
            if (null == node || !node.data.TypeShortName.IsSpecifiedEntityTypeShortNameExist())
            {
                return node;
            }

            NavigateTreeNode.Path += node.data.Name != "root" ? "/" + node.data.Name + "/" + node.data.TypeFullName : string.Empty;
            var navigationPropertyElems = MetadataHelper.GetNavigProperties(node.data.TypeShortName, NavigateTreeNode.Path);
            foreach (var navigationPropertyElem in navigationPropertyElems)
            {
                if (null != navigationPropertyElem.Item1.Attribute("Name") && null != navigationPropertyElem.Item1.Attribute("Type"))
                {
                    string navigationPropName = navigationPropertyElem.Item1.GetAttributeValue("Name");
                    string entityTypeFullName = navigationPropertyElem.Item1.GetAttributeValue("Type").RemoveCollectionFlag();
                    string entityTypeShortName = entityTypeFullName.GetLastSegment();
                    string path = navigationPropertyElem.Item2;
                    bool isCollection = navigationPropertyElem.Item1.GetAttributeValue("Type").StartsWith("Collection(");
                    string partner =
                        null != navigationPropertyElem.Item1.Attribute("Partner") ?
                        navigationPropertyElem.Item1.GetAttributeValue("Partner") :
                        string.Empty;
                    bool containsTarget =
                        null != navigationPropertyElem.Item1.Attribute("ContainsTarget") ?
                        Convert.ToBoolean(navigationPropertyElem.Item1.GetAttributeValue("ContainsTarget")) :
                        false;

                    var data = new NavigationData(navigationPropName, entityTypeShortName, entityTypeFullName, path, isCollection, partner, containsTarget);
                    var childNode = new NavigateTreeNode(data);
                    childNode.parent = node;
                    if (childNode.data.ContainsTarget && !IsEntityTypeTraverse(entityTypeShortName, node))
                    {
                        node.AddChild(NavigateTreeNode.Construct(childNode));
                    }
                    else
                    {
                        node.AddChild(childNode);
                    }
                }
            }

            return node;
        }

        /// <summary>
        /// Verify wether the entity-type short name has been exist on the navigation tree node or its child nodes.
        /// </summary>
        /// <param name="entityTypeShortName">The entity-type short name.</param>
        /// <param name="node">The node of the navigation tree.</param>
        /// <returns>Returns the verification result.</returns>
        private static bool IsEntityTypeTraverse(string entityTypeShortName, NavigateTreeNode node)
        {
            if (!entityTypeShortName.IsSpecifiedEntityTypeShortNameExist() || null == node)
            {
                return false;
            }

            do
            {
                if (node.data.TypeShortName == entityTypeShortName)
                {
                    return true;
                }

                node = node.parent;
            }
            while (null != node);

            return false;
        }
        #endregion

    }

    /// <summary>
    /// The NavigationData class.
    /// </summary>
    public class NavigationData
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The navigation property's name.</param>
        /// <param name="typeShortName">The navigation property's type.</param>
        /// <param name="containsTarget">The navigation property element's value of attribute 'ContainsTarget'.</param>
        public NavigationData(string name, string typeShortName, string typeFullName, string path, bool isCollection, string partner, bool containsTarget)
        {
            this.name = name;
            this.typeShortName = typeShortName;
            this.typeFullName = typeFullName;
            this.path = path;
            this.isCollection = isCollection;
            this.partner = partner;
            this.containsTarget = containsTarget;
        }

        /// <summary>
        /// Gets the navigation property's name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the navigation property's type short name.
        /// </summary>
        public string TypeShortName
        {
            get
            {
                return this.typeShortName;
            }
        }

        /// <summary>
        /// Gets the navigation property's type full name.
        /// </summary>
        public string TypeFullName
        {
            get
            {
                return this.typeFullName;
            }
        }

        /// <summary>
        /// Gets the navigation property's path.
        /// </summary>
        public string Path
        {
            get
            {
                return this.path;
            }
        }

        /// <summary>
        /// Gets the navigation property's type.
        /// </summary>
        public bool IsCollection
        {
            get
            {
                return this.isCollection;
            }
        }

        /// <summary>
        /// Gets the navigation property's value of the attribute 'Partner'.
        /// </summary>
        public string Partner
        {
            get
            {
                return this.partner;
            }
        }

        /// <summary>
        /// Gets the navigation property's value of the attribute 'ContainsTarget'.
        /// </summary>
        public bool ContainsTarget
        {
            get
            {
                return this.containsTarget;
            }
        }

        #region Private members.
        private string name;
        private string typeFullName;
        private string typeShortName;
        private string path;
        private bool isCollection;
        private string partner;
        private bool containsTarget;
        #endregion
    }
}
