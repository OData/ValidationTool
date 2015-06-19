// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    #endregion

    /// <summary>
    /// The navigation property (only be used in v4 odata service) class of an entity type.
    /// </summary>
    public class NavigProperty
    {
        /// <summary>
        /// Initializes the NavigProperty class.
        /// </summary>
        /// <param name="navigPropName"></param>
        /// <param name="navigPropType"></param>
        /// <param name="navigPropPartner"></param>
        public NavigProperty(string navigPropName, string navigPropType, string navigPropPartner)
        {
            if (string.IsNullOrEmpty(navigPropName))
            {
                throw new ArgumentNullException("navigPropName", "The value of the input parameter 'navigPropName' MUST NOT be null or empty.");
            }

            if (string.IsNullOrEmpty(navigPropType))
            {
                throw new ArgumentNullException("navigPropType", "The value of the input parameter 'navigPropType' MUST NOT be null or empty.");
            }

            this.navigationPropertyName = navigPropName;
            this.navigationPropertyType = navigPropType;
            this.navigationPropertyPartner = navigPropPartner;
            this.navigationRoughType =
                this.navigationPropertyType.StartsWith(@"Collection(") && this.navigationPropertyType.EndsWith(@")") ?
                Helper.NavigationRoughType.CollectionValued : Helper.NavigationRoughType.SingleValued;
        }

        /// <summary>
        /// Gets or sets the navigation property name.
        /// </summary>
        public string NavigationPropertyName
        {
            get
            {
                return navigationPropertyName;
            }
            set
            {
                navigationPropertyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the navigation property type.
        /// </summary>
        public string NavigationPropertyType
        {
            get
            {
                return navigationPropertyType;
            }
            set
            {
                navigationPropertyType = value;
            }
        }

        /// <summary>
        /// Gets or sets the navigation property partner.
        /// </summary>
        public string NavigationPropertyPartner
        {
            get
            {
                return navigationPropertyPartner;
            }
            set
            {
                navigationPropertyPartner = value;
            }
        }

        /// <summary>
        /// Gets or sets the rough-type of a navigation property.
        /// </summary>
        public NavigationRoughType NavigationRoughType
        {
            get
            {
                return navigationRoughType;
            }
            set
            {
                navigationRoughType = value;
            }
        }

        /// <summary>
        /// Remove the key word "Collection" and paretheses sign from collection-valued navigation property.
        /// </summary>
        /// <param name="navigationPropertyType">A navigation property type.</param>
        /// <returns>Returns a navigation type without key word "Collection" and paretheses sign.</returns>
        public static string RemoveCollectionParentheses(string navigationPropertyType)
        {
            const string StartMarker = @"Collection(";
            const string EndMarker = @")";

            if (navigationPropertyType.StartsWith(StartMarker) &&
                navigationPropertyType.EndsWith(EndMarker))
            {
                return navigationPropertyType.Remove(0, StartMarker.Length).RemoveEnd(EndMarker);
            }

            return navigationPropertyType;
        }

        /// <summary>
        /// The navigation property name.
        /// </summary>
        private string navigationPropertyName;

        /// <summary>
        /// The navigation property type.
        /// </summary>
        private string navigationPropertyType;

        /// <summary>
        /// The navigation property partner.
        /// </summary>
        private string navigationPropertyPartner;

        /// <summary>
        /// The rought-type of a navigation property.
        /// </summary>
        private NavigationRoughType navigationRoughType;
    }
}
