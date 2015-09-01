// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper.Geo
{
    #region Namespace.
    using System;
    #endregion

    /// <summary>
    /// The Point class.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// The constructor of the Point class.
        /// </summary>
        /// <param name="x">The value of the coordinate x.</param>
        /// <param name="y">The value of the coordinate y.</param>
        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Gets or private sets the value of the coordinate x.
        /// </summary>
        public double X
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets the value of the coordinate y.
        /// </summary>
        public double Y
        {
            get;
            private set;
        }

        public static Point operator +(Point pt1, Point pt2)
        {
            return pt1.Add(pt2);
        }

        public static Point operator -(Point pt1, Point pt2)
        {
            return pt1.Subtract(pt2);
        }

        public static bool operator ==(Point p1, Point p2)
        {
            if (object.Equals(p1, null))
            {
                if (object.Equals(p2, null))
                {
                    return true;
                }

                return false;
            }

            return p1.Equals(p2);
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Add operation. (Between 2 points.) 
        /// </summary>
        /// <param name="pt">The other point.</param>
        /// <returns>Returns the result.</returns>
        public Point Add(Point pt)
        {
            return new Point(this.X + pt.X, this.Y + pt.Y);
        }

        /// <summary>
        /// Subtract operation. (Between 2 points.)
        /// </summary>
        /// <param name="pt">The other point.</param>
        /// <returns>Returns the result.</returns>
        public Point Subtract(Point pt)
        {
            return new Point(this.X - pt.X, this.Y - pt.Y);
        }

        /// <summary>
        /// Judge whether the 2 points are equal.
        /// </summary>
        /// <param name="obj">The other point.</param>
        /// <returns>Returns the result.</returns>
        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }

            var pt = obj as Point;

            return this.X == pt.X && this.Y == pt.Y;
        }

        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns>Returns a value with string type.</returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", this.X, this.Y);
        }

        /// <summary>
        /// Calculate the distance between the 2 points.
        /// </summary>
        /// <param name="pt1">The first point.</param>
        /// <param name="pt2">The second point.</param>
        /// <returns>Returns the distance between them.</returns>
        public static double GetDistance(Point pt1, Point pt2)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2.0) + Math.Pow(pt1.Y - pt2.Y, 2.0));
        }
    }
}
