// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    using System.Linq;
    #endregion

    /// <summary>
    /// The PredicateHelper class helps to assert whether the specified conditions have been matched or not.
    /// </summary>
    public class PredicateHelper
    {
        /// <summary>
        /// Asserts whether any properties of an entity-type matches the specified conditions.
        /// </summary>
        /// <param name="entityType">An entity-type.</param>
        /// <param name="containedKeyPropSum">Limits the contained key properties' sum of an entity-type.</param>
        /// <param name="containedKeyPropTypes">Limits the contained key properties' types of an entity-type.</param>
        /// <param name="containedNormalPropTypes">Limits contained normal properties' types of an entity-type.</param>
        /// <param name="containedNavigRoughType">Limits the contained navigation properties' rough-type of an entity-type.</param>
        /// <returns>Returns the decision outcome.</returns>
        public static bool EntityTypeAnyPropertiesMeetsSpecifiedConditions(
            EntityTypeElement entityType,
            uint? containedKeyPropSum,
            IEnumerable<string> containedKeyPropTypes,
            IEnumerable<string> containedNormalPropTypes,
            NavigationRoughType containedNavigRoughType)
        {
            if (entityType == null)
            {
                return false;
            }

            if (null != containedKeyPropSum)
            {
                if (containedKeyPropSum != entityType.KeyProperties.Count())
                {
                    return false;
                }
            }

            if (null != containedKeyPropTypes)
            {
                var appropriateKeyProps = entityType.KeyProperties
                    .Where(keyProp => containedKeyPropTypes.Contains(keyProp.PropertyType))
                    .Select(keyProp => keyProp);

                if (0 == appropriateKeyProps.Count())
                {
                    return false;
                }
            }

            if (null != containedNormalPropTypes)
            {
                var appropriateNormalProps = entityType.NormalProperties
                    .Where(norProp => containedNormalPropTypes.Contains(norProp.PropertyType))
                    .Select(norProp => norProp);

                if (0 == appropriateNormalProps.Count())
                {
                    return false;
                }
            }

            if (NavigationRoughType.None != containedNavigRoughType)
            {
                var appropriateNavigProps = entityType.NavigationProperties
                    .Where(navProp => containedNavigRoughType == navProp.NavigationRoughType)
                    .Select(navProp => navProp);

                if (0 == appropriateNavigProps.Count())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
