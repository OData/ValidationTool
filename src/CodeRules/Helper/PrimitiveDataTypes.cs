// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    using System.Collections.Generic;

    /// <summary>
    /// Enumerate all the primitive data type.
    /// </summary>
    public static class PrimitiveDataTypes
    {
        public const string Null = @"Null";

        public const string Binary = @"Edm.Binary";

        public const string Boolean = @"Edm.Boolean";

        public const string Byte = @"Edm.Byte";

        public const string DateTime = @"Edm.DateTime";

        public const string Decimal = @"Edm.Decimal";

        public const string Double = @"Edm.Double";

        public const string Single = @"Edm.Single";

        public const string Guid = @"Edm.Guid";

        public const string Int16 = @"Edm.Int16";

        public const string Int32 = @"Edm.Int32";

        public const string Int64 = @"Edm.Int64";

        public const string SByte = @"Edm.SByte";

        public const string String = @"Edm.String";

        public const string Time = @"Edm.Time";

        public const string DateTimeOffset = @"Edm.DateTimeOffset";

        public const string Stream = @"Edm.Stream";

        public static List<string> NonQualifiedTypes = new List<string>() {"Null", "Binary", "Boolean", "Byte", "Date", "DateTimeOffset", 
                                                    "Decimal", "Double", "Duration", "Guid", "Int16", "Int32", "Int64", 
                                                    "Single", "SByte", "Stream", "String", "TimeOfDay", "Geography", "GeographyPoint",
                                                    "GeographyLineString", "GeographyPolygon", "GeographyMultiPoint", "GeographyMultiLineString",
                                                    "GeographyMultiPolygon", "GeographyCollection", "Geometry", "GeometryPoint", "GeometryLineString",
                                                    "GeometryPolygon", "GeometryMultiPoint", "GeometryMultiLineString", "GeometryMultiPolygon","GeometryCollection"};

        public static List<string> NumbericTypesDefinedInOdataTypeValue = new List<string>() { "#Decimal", "#Double", "#Int16", "#Int32", "#Int64" };
    }
}
