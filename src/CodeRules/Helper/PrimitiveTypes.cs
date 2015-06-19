// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper
{
    /// <summary>
    /// Identify the Primitive types.
    /// </summary>
    public enum PrimitiveTypes : byte
    {
        /// <summary>
        /// Fixed-length or variable-length binary data
        /// </summary>
        Binary = 0x00,

        /// <summary>
        /// Binary-valued logic
        /// </summary>
        Boolean,
        /// <summary>
        /// Unsigned 8-bit integer
        /// </summary>
        Byte,

        /// <summary>
        /// Date without a time-zone offset
        /// </summary>
        Date,

        /// <summary>
        /// Date and time with a time-zone offset, no leap seconds
        /// </summary>
        DateTimeOffset,
        /// <summary>
        /// Numeric values with fixed precision and scale
        /// </summary>
        Decimal,
        /// <summary>
        /// Floating-point number with 15 digits precision
        /// </summary>
        Double,

        /// <summary>
        /// Signed duration in days, hours, minutes, and (sub)seconds
        /// </summary>
        Duration,

        /// <summary>
        /// 16-byte (128-bit) unique identifier
        /// </summary>
        Guid,

        /// <summary>
        /// Signed 16-bit integer
        /// </summary>
        Int16,

        /// <summary>
        /// Signed 32-bit integer
        /// </summary>
        Int32,

        /// <summary>
        /// Signed 64-bit integer
        /// </summary>
        Int64,

        /// <summary>
        /// Signed 8-bit integer
        /// </summary>
        SByte,

        /// <summary>
        /// Floating-point number with 7 digits precision
        /// </summary>
        Single,

        /// <summary>
        /// Fixed-length or variable-length data stream
        /// </summary>
        Stream,

        /// <summary>
        /// Fixed-length or variable-length sequence of UTF-8 characters
        /// </summary>
        String,

        /// <summary>
        /// Clock time 0-23:59:59.999999999999
        /// </summary>
        TimeOfDay,

        /// <summary>
        /// Abstract base type for all Geography types
        /// </summary>
        Geography,

        /// <summary>
        /// A point in a round-earth coordinate system
        /// </summary>
        GeographyPoint,

        /// <summary>
        /// Line string in a round-earth coordinate system
        /// </summary>
        GeographyLineString,

        /// <summary>
        /// Polygon in a round-earth coordinate system
        /// </summary>
        GeographyPolygon,

        /// <summary>
        /// Collection of points in a round-earth coordinate system
        /// </summary>
        GeographyMultiPoint,

        /// <summary>
        /// Collection of line strings in a round-earth coordinate system
        /// </summary>
        GeographyMultiLineString,

        /// <summary>
        /// Collection of polygons in a round-earth coordinate system
        /// </summary>
        GeographyMultiPolygon,

        /// <summary>
        /// Collection of arbitrary Geography values
        /// </summary>
        GeographyCollection,

        /// <summary>
        /// Abstract base type for all Geometry types
        /// </summary>
        Geometry,

        /// <summary>
        /// Point in a flat-earth coordinate system
        /// </summary>
        GeometryPoint,

        /// <summary>
        /// Line string in a flat-earth coordinate system
        /// </summary>
        GeometryLineString,

        /// <summary>
        /// Polygon in a flat-earth coordinate system
        /// </summary>
        GeometryPolygon,

        /// <summary>
        /// Collection of points in a flat-earth coordinate system
        /// </summary>
        GeometryMultiPoint,

        /// <summary>
        /// Collection of line strings in a flat-earth coordinate system
        /// </summary>
        GeometryMultiLineString,

        /// <summary>
        /// Collection of polygons in a flat-earth coordinate system
        /// </summary>
        GeometryMultiPolygon,

        /// <summary>
        /// Collection of arbitrary Geometry values
        /// </summary>
        GeometryCollection,
    }
}
