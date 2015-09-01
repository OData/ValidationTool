// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper.Geo
{
    #region Namespace.
    using System;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The SegmentEquation class.
    /// </summary>
    public class SegmentEquation : LinearEquation
    {
        /// <summary>
        /// The constructor of the SegmentEquation class.
        /// </summary>
        /// <param name="pt1">The first point.</param>
        /// <param name="pt2">The second point.</param>
        public SegmentEquation(Point pt1, Point pt2)
            : base(pt1, pt2)
        {
            this.endpoint1 = pt1;
            this.endpoint2 = pt2;
            this.length = Point.GetDistance(pt1, pt2);
            if (pt1.X >= pt2.X)
            {
                this.xUpperBoundary = pt1.X;
                this.xLowerBoundary = pt2.X;
            }
            else
            {
                this.xUpperBoundary = pt2.X;
                this.xLowerBoundary = pt1.X;
            }

            if (pt1.Y >= pt2.Y)
            {
                this.yUpperBoundary = pt1.Y;
                this.yLowerBoundary = pt2.Y;
            }
            else
            {
                this.yUpperBoundary = pt2.Y;
                this.yLowerBoundary = pt1.Y;
            }
        }

        #region Encapsulate fields.
        /// <summary>
        /// Gets the length of the specified segment.
        /// </summary>
        public double Length
        {
            get
            {
                return this.length;
            }
        }
        #endregion

        /// <summary>
        /// Gets the value range of the x-coordinate.
        /// </summary>
        /// <returns>Returns the value range of the x-coordinate.</returns>
        public Tuple<double, double> GetXRange()
        {
            return new Tuple<double, double>(xLowerBoundary, xUpperBoundary);
        }

        /// <summary>
        /// Gets the value range of the y-coordinate.
        /// </summary>
        /// <returns>Returns the value range of the y-coordinate.</returns>
        public Tuple<double, double> GetYRange()
        {
            return new Tuple<double, double>(yLowerBoundary, yUpperBoundary);
        }

        /// <summary>
        /// Calculate the value of x-coordianate from the y-coordianate.
        /// </summary>
        /// <param name="coordY">The value of the y-coordinate.</param>
        /// <returns>Returns the value of the x-coordinate. 
        /// If the value has been out of the value range, it will return double.NaN.
        /// </returns>
        public new double GetCoordX(double coordY)
        {
            double x = base.GetCoordX(coordY);
            if (this.IsValueInXRange(x))
            {
                return x;
            }

            return double.NaN;
        }

        /// <summary>
        /// Calculate the value of y-coordinate from the x-coordinate.
        /// </summary>
        /// <param name="coordX">The value of the x-coordinate.</param>
        /// <returns>Returns the value of the y-coordinate. 
        /// If the value has been out of the value range, it will return double.NaN.
        /// </returns>
        public new double GetCoordY(double coordX)
        {
            double y = base.GetCoordY(coordX);
            if (this.IsValueInYRange(y))
            {
                return y;
            }

            return double.NaN;
        }

        /// <summary>
        /// Calculate the intersection point between the 2 segments.
        /// </summary>
        /// <param name="eq1">The first segment equation.</param>
        /// <param name="eq2">The second segment equation.</param>
        /// <returns>Returns the intersection point.</returns>
        public static Point GetIntersection(SegmentEquation eq1, SegmentEquation eq2)
        {
            var pt = LinearEquation.GetIntersection(eq1, eq2);
            if (eq1.IsPointInRange(pt) && eq2.IsPointInRange(pt))
            {
                return pt;
            }

            return null;
        }

        /// <summary>
        /// Calculate the intersection point between the segment and line.
        /// </summary>
        /// <param name="sEq">The segment equation.</param>
        /// <param name="lEq">The line equation.</param>
        /// <returns>Returns the intersection point.</returns>
        public static Point GetIntersection(SegmentEquation sEq, LinearEquation lEq)
        {
            var pt = LinearEquation.GetIntersection(sEq, lEq);
            if (sEq.IsPointInRange(pt))
            {
                return pt;
            }

            return null;
        }

        /// <summary>
        /// Calculate the intersection point between the segment and line (Special).
        /// </summary>
        /// <param name="sEq">The segment equation.</param>
        /// <param name="lEq">The line equation.</param>
        /// <returns>Returns the intersection points.</returns>
        public static Point GetIntersectionSp(SegmentEquation sEq, LinearEquation lEq)
        {
            var pt = SegmentEquation.GetIntersection(sEq, lEq);
            if (pt == sEq.endpoint1)
            {
                if (pt.Y <= sEq.endpoint2.Y)
                {
                    pt = null;
                }
            }
            else if (pt == sEq.endpoint2)
            {
                if (pt.Y <= sEq.endpoint1.Y)
                {
                    pt = null;
                }
            }

            return pt;
        }

        /// <summary>
        /// Calculate the intersection points when a line across many segments.
        /// </summary>
        /// <param name="sEqs">The segment equations.</param>
        /// <param name="lEq">The line equation.</param>
        /// <returns>Returns the intersection points.</returns>
        public static List<Point> GetIntersection(IEnumerable<SegmentEquation> sEqs, LinearEquation lEq)
        {
            var pts = new List<Point>();
            foreach (var sEq in sEqs)
            {
                var pt = SegmentEquation.GetIntersection(sEq, lEq);
                if (null != pt && sEq.IsPointInRange(pt))
                {
                    pts.Add(pt);
                }
            }

            return pts;
        }

        /// <summary>
        /// Calculate the intersection points when a line across many segments (Special).
        /// </summary>
        /// <param name="sEqs">The segment equations.</param>
        /// <param name="lEq">The line equation.</param>
        /// <returns>Returns the intersection points.</returns>
        public static List<Point> GetIntersectionSp(IEnumerable<SegmentEquation> sEqs, LinearEquation lEq)
        {
            var pts = new List<Point>();
            foreach (var sEq in sEqs)
            {
                var pt = SegmentEquation.GetIntersectionSp(sEq, lEq);
                if (null != pt && sEq.IsPointInRange(pt))
                {
                    pts.Add(pt);
                }
            }

            return pts;
        }

        #region Private members.
        /// <summary>
        /// The one endpoint of the segment.
        /// </summary>
        private Point endpoint1;

        /// <summary>
        /// The other endpoint of the segment.
        /// </summary>
        private Point endpoint2;

        /// <summary>
        /// The length of the segment.
        /// </summary>
        private double length;

        /// <summary>
        /// The upper boundary on the x-coordinate.
        /// </summary>
        private double xUpperBoundary;

        /// <summary>
        /// The lower boundary on the x-coordinate.
        /// </summary>
        private double xLowerBoundary;

        /// <summary>
        /// The upper boundary on the y-coordinate.
        /// </summary>
        private double yUpperBoundary;

        /// <summary>
        /// The lower boundary on the y-coordinate.
        /// </summary>
        private double yLowerBoundary;
        #endregion

        #region Private methods.
        /// <summary>
        /// Judge whether the specified value in the value range of the x-coordinate.
        /// </summary>
        /// <param name="val">The specified value.</param>
        /// <returns>Return the result.</returns>
        private bool IsValueInXRange(double val)
        {
            return this.xLowerBoundary <= val && val <= this.xUpperBoundary;
        }

        /// <summary>
        /// Judge whether the specified value in the value range of the y-coordinate.
        /// </summary>
        /// <param name="val">The specified value.</param>
        /// <returns>Return the result.</returns>
        private bool IsValueInYRange(double val)
        {
            return this.yLowerBoundary <= val && val <= this.yUpperBoundary;
        }

        /// <summary>
        /// Judge whether the specified point in the value range.
        /// </summary>
        /// <param name="pt">The specified point.</param>
        /// <returns>Return the result.</returns>
        private bool IsPointInRange(Point pt)
        {
            if (null == pt)
            {
                return false;
            }

            return this.IsValueInXRange(pt.X) && this.IsValueInYRange(pt.Y);
        }
        #endregion
    }
}