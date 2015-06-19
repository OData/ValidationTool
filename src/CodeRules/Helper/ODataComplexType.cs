// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The ComplexType class.
    /// </summary>
    public class ODataComplexType
    {
        /// <summary>
        /// The constructor of the ComplexType class.
        /// </summary>
        /// <param name="name">The complex type name.</param>
        public ODataComplexType(string name)
        {
            this.Name = name;
            this.properties = new List<ODataProperty>();
        }

        /// <summary>
        /// Gets or sets the name of the complex type.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Add a property to the complex type properties' list.
        /// </summary>
        /// <param name="property">A property.</param>
        public void AddProperty(ODataProperty property)
        {
            this.properties.Add(property);
        }

        /// <summary>
        /// Gets all the properties of the complex type.
        /// </summary>
        /// <returns>Returns all the properties.</returns>
        public List<ODataProperty> GetProperties()
        {
            return this.properties;
        }

        /// <summary>
        /// The properties in the complex type.
        /// </summary>
        private List<ODataProperty> properties;
    }
}
