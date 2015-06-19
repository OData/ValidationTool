// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    #region Namespaces
    using System;
    using System.Data.Metadata.Edm;
    using System.Linq;
    #endregion

    /// <summary>
    /// Class of ODataUriItem
    /// </summary>
    class ODataUriItem
    {
        public MetadataItem Item { get; private set; }
        public UriType uriType { get; private set; }

        public ODataUriItem(MetadataItem item_, UriType uriType_)
        {
            this.Item = item_;
            this.uriType = uriType_;
        }

        /// <summary>
        /// Gets ODataUriItem instance by specified name
        /// </summary>
        /// <param name="name">name of sub item</param>
        /// <returns>sub-item if one is found; null otherwise</returns>
        public ODataUriItem GetItem(string name)
        {
            switch (this.uriType)
            {
                case UriType.URI_Container:
                    return GetItem((EntityContainer)this.Item, name);
                case UriType.URI1:
                    return GetItem((EntitySet)this.Item, name);
                case UriType.URI2:
                    return GetItem((EntityType)this.Item, name);
                case UriType.URI3:
                    ComplexType ct = (ComplexType)((EdmProperty)this.Item).TypeUsage.EdmType;
                    return GetItem(ct, name);
                case UriType.URI4:
                case UriType.URI5:
                case UriType.URI14:
                    if (name.Equals("$value", StringComparison.Ordinal))
                    {
                        return this;
                    }
                    else
                    {
                        return null;
                    }
                case UriType.URI6:
                    RelationshipEndMember asso = (RelationshipEndMember)this.Item;
                    var alias = new ODataUriItem(asso.GetEntityType(), UriType.URI2);
                    return alias.GetItem(name);
                case UriType.URI_Link:
                    var next = GetItem((EntityType)this.Item, name);
                    if (next.uriType == UriType.URI6)
                    {
                        return new ODataUriItem(next.Item, UriType.URI7);
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets sub item of the specified EntityContainer instance by the name 
        /// </summary>
        /// <param name="container">The EntityContainer instance</param>
        /// <param name="name">The name of sub item</param>
        /// <returns>sub item object if one is found; null otherwise</returns>
        private static ODataUriItem GetItem(EntityContainer container, string name)
        {
            ODataUriItem result = null;

            EntitySet entitySet;
            bool isEntitySet = container.TryGetEntitySetByName(name, false, out entitySet);
            if (isEntitySet)
            {
                return new ODataUriItem(entitySet, UriType.URI1);
            }
            else
            {
                var fs = container.FunctionImports.Where(x => x.Name.Equals(name, StringComparison.Ordinal));
                if (fs.Any())
                {
                    EdmFunction func = fs.First();
                    var retVal = func.ReturnParameter;
                    var retType = retVal.TypeUsage.EdmType;
                    UriType retUriType = GetUriTypeOfFuncReturn(retType);
                    if (retUriType == UriType.URI11 || retUriType == UriType.URI13 || retUriType == UriType.URI_CollEt)
                    {
                        result = new ODataUriItem(((CollectionType)retType).TypeUsage.EdmType, retUriType);
                    }
                    else
                    {
                        result = new ODataUriItem(retType, retUriType);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Lookup UriType value for FunctionImport Return parameter type
        /// </summary>
        /// <param name="retType">FunctionImport Return parameter type</param>
        /// <returns>The UriType value</returns>
        private static UriType GetUriTypeOfFuncReturn(EdmType retType)
        {
            switch (retType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntityType:
                    return UriType.URI10;
                case BuiltInTypeKind.ComplexType:
                    return UriType.URI12;
                case BuiltInTypeKind.PrimitiveType: 
                    return UriType.URI14;
                case BuiltInTypeKind.CollectionType:
                    {
                        var innerType = ((CollectionType)retType).TypeUsage.EdmType;
                        switch (innerType.BuiltInTypeKind)
                        {
                            case BuiltInTypeKind.ComplexType:
                                return UriType.URI11;
                            case BuiltInTypeKind.PrimitiveType:
                                return UriType.URI13;
                            case BuiltInTypeKind.EntityType:
                                return UriType.URI_CollEt;
                            default:
                                return UriType.URIUNKNOWN;
                        }
                    }
                default:
                    return UriType.URIUNKNOWN;
            }
        }

        /// <summary>
        /// Gets sub item of the specified EntitySet instance by the name 
        /// </summary>
        /// <param name="entitySet">The EntitySet instance</param>
        /// <param name="name">The name of sub item</param>
        /// <returns>sub item object if one is found; null otherwise</returns>
        private static ODataUriItem GetItem(EntitySet entitySet, string name)
        {
            if (name.Equals("$count", StringComparison.Ordinal))
            {
                return new ODataUriItem(entitySet, UriType.URI15);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets sub item of the specified EntityType instance by the name 
        /// </summary>
        /// <param name="entityType">The EntityType instance</param>
        /// <param name="name">The name of sub item</param>
        /// <returns>sub item object if one is found; null otherwise</returns>
        private static ODataUriItem GetItem(EntityType entityType, string name)
        {
            if (name.Equals("$links", StringComparison.Ordinal))
            {
                return new ODataUriItem(entityType, UriType.URI_Link);
            }
            else if (name.Equals("$count", StringComparison.Ordinal))
            {
                return new ODataUriItem(entityType, UriType.URI16);
            }
            else if (name.Equals("$value", StringComparison.Ordinal))
            {
                return new ODataUriItem(entityType, UriType.URI17);
            }
            else
            {
                ODataUriItem result = null;

                EdmProperty property;
                bool isProperty = entityType.Properties.TryGetValue(name, false, out property);
                if (isProperty)
                {
                    EdmType targetType = property.TypeUsage.EdmType;
                    result = new ODataUriItem(property, targetType.BuiltInTypeKind == BuiltInTypeKind.ComplexType ? UriType.URI3 : UriType.URI5);
                }
                else
                {
                    NavigationProperty navProperty;
                    bool isNavProperty = entityType.NavigationProperties.TryGetValue(name, false, out navProperty);
                    if (isNavProperty)
                    {
                        result = new ODataUriItem(navProperty.ToEndMember, UriType.URI6);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets sub item of the specified ComplexType instance by the name 
        /// </summary>
        /// <param name="type">The ComplexType instance</param>
        /// <param name="name">The name of sub item</param>
        /// <returns>sub item object if one is found; null otherwise</returns>
        private static ODataUriItem GetItem(ComplexType type, string name)
        {
            ODataUriItem result = null;

            EdmProperty property;
            bool isProperty = type.Properties.TryGetValue(name, false, out property);
            if (isProperty)
            {
                result = new ODataUriItem(property, property.BuiltInTypeKind == BuiltInTypeKind.ComplexType ? UriType.URI3 : UriType.URI5);
            }

            return result;
        }
    }
}
