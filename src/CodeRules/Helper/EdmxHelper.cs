// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Xml.Linq;
    #endregion

    /// <summary>
    /// Helper class to encapsulate POCO Edmx parser
    /// </summary>
    public class EdmxHelper
    {
        // metadata items of Edmx document
        private EdmItemCollection edmxItems;

        /// <summary>
        /// Gets collection of EntityContainers
        /// </summary>
        public IEnumerable<EntityContainer> containers { get; private set; }

        /// <summary>
        /// Gets the default entity container
        /// </summary>
        public EntityContainer containerDefault { get; private set; }

        /// <summary>
        /// Creates an instance of EdmxHelper from XML input
        /// </summary>
        /// <param name="metadta">The XML input as metadata document</param>
        public EdmxHelper(XElement metadta)
        {
            var csdlNodes = metadta.Descendants("{http://schemas.microsoft.com/ado/2007/06/edmx}DataServices").First().Elements();
            this.edmxItems = new EdmItemCollection(csdlNodes.Select(x => x.CreateReader()));
            this.containers = from i in this.edmxItems where i.BuiltInTypeKind == BuiltInTypeKind.EntityContainer select (EntityContainer)i;
            this.containerDefault = this.GetDefaultEntityContainer();
        }

        /// <summary>
        /// Picks the default one from the collection of entity container
        /// </summary>
        /// <returns>The default container if one is found; null otherwise</returns>
        private EntityContainer GetDefaultEntityContainer()
        {
            foreach (var c in this.containers)
            {
                foreach (var a in c.MetadataProperties)
                {
                    if (a.Name == "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata:IsDefaultEntityContainer")
                    {
                        var v = Convert.ToBoolean(a.Value);
                        if (v)
                        {
                            return c;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a strongly typed GlobalItem object by using the specified full name
        /// </summary>
        /// <typeparam name="T">The type of which the returned object is</typeparam>
        /// <param name="fullName">The full name of the item</param>
        /// <param name="item">Output parameter of the item</param>
        /// <returns>true if there is an item that has the full name and is of type T</returns>
        public bool TryGetItem<T>(string fullName, out T item) where T : GlobalItem
        {
            return this.edmxItems.TryGetItem(fullName, out item);
        }
    }
}
