// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    /// <summary>
    /// The PropertyElement class.
    /// </summary>
    public class PropertyElement
    {
        /// <summary>
        /// The constructor of the PropertyElement class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="type">The property type.</param>
        /// <param name="defaultValue">The default value.</param>
        public PropertyElement(string name, string type, object defaultValue = null, bool nullable = true)
        {
            this.Name = name;
            this.Type = type;
            this.DefaultValue = defaultValue;
            this.Nullable = nullable;
        }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the property's default value.
        /// </summary>
        public object DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                defaultValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the property's nullable value.
        /// </summary>
        public bool Nullable
        {
            get
            {
                return nullable;
            }
            set
            {
                nullable = value;
            }
        }

        /// <summary>
        /// The default value.
        /// </summary>
        private object defaultValue;

        /// <summary>
        /// The nullable value.
        /// </summary>
        private bool nullable = true;
    }
}
