// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper.Geo
{
    #region Namespace.
    using System;
    #endregion

    /// <summary>
    /// The LinearEquation class.
    /// </summary>
    public class LinearEquation
    {
        /// <summary>
        /// The constructor of the LinearEquation class.
        /// </summary>
        /// <param name="pt1">The first point.</param>
        /// <param name="pt2">The second point.</param>
        public LinearEquation(Point pt1, Point pt2)
        {
            this.slope = (pt1.Y - pt2.Y) / (pt1.X - pt2.X);
            if (!double.IsInfinity(this.slope))
            {
                this.bias = pt1.Y - this.slope * pt1.X;
            }
            else
            {
                this.bias = pt1.X;
            }
        }

        /// <summary>
        /// The constructor of the LinearEquation class.
        /// </summary>
        /// <param name="pt">The specified point.</param>
        /// <param name="type">The type of the special line.</param>
        public LinearEquation(Point pt, LinearType type)
        {
            if (LinearType.ParallelXAxis == type)
            {
                this.slope = 0.0;
                this.bias = pt.Y;
            }
            else
            {
                this.slope = Double.PositiveInfinity;
                this.bias = pt.X;
            }
        }

        #region Encapsulate fields.
        /// <summary>
        /// Gets the slope of the line.
        /// </summary>
        public double Slope
        {
            get
            {
                return this.slope;
            }
        }

        /// <summary>
        /// Gets the bias of the line.
        /// </summary>
        public double Bias
        {
            get
            {
                return this.bias;
            }
        }
        #endregion

        /// <summary>
        /// Calculate the value of x-coordianate from the y-coordianate.
        /// </summary>
        /// <param name="coordY">The value of the y-coordinate.</param>
        /// <returns>Returns the value of the x-coordinate.</returns>
        public double GetCoordX(double coordY)
        {
            if (!double.IsInfinity(this.slope))
            {
                return (coordY - this.bias) / this.slope;
            }
            else
            {
                return this.bias;
            }
        }

        /// <summary>
        /// Calculate the value of y-coordinate from the x-coordinate.
        /// </summary>
        /// <param name="coordX">The value of the x-coordinate.</param>
        /// <returns>Returns the value of the y-coordinate.</returns>
        public double GetCoordY(double coordX)
        {
            if (!double.IsInfinity(this.slope))
            {
                return this.slope * coordX + this.bias;
            }
            else
            {
                return this.slope;
            }
        }

        /// <summary>
        /// Calculate the intersection point between the 2 lines.
        /// </summary>
        /// <param name="eq1">The first line equation.</param>
        /// <param name="eq2">The second line equation.</param>
        /// <returns>Return the intersection point.</returns>
        public static Point GetIntersection(LinearEquation eq1, LinearEquation eq2)
        {
            if (eq1.slope == eq2.slope)
            {
                return null;
            }

            if (double.IsInfinity(eq1.slope))
            {
                return new Point(eq1.bias, eq2.GetCoordY(eq1.bias));
            }

            if (double.IsInfinity(eq2.slope))
            {
                return new Point(eq2.bias, eq1.GetCoordY(eq2.bias));
            }

            double x = (eq2.bias - eq1.bias) / (eq1.slope - eq2.slope);
            double y = eq1.GetCoordY(x);

            return new Point(x, y);
        }

        #region Private members.
        /// <summary>
        /// The slope of the line.
        /// </summary>
        private double slope;

        /// <summary>
        /// The bias of the line.
        /// </summary>
        private double bias;
        #endregion
    }

    /// <summary>
    /// The LinearType enumeration.
    /// </summary>
    public enum LinearType
    {
        /// <summary>
        /// Parallel to x axis.
        /// </summary>
        ParallelXAxis = 0x01,

        /// <summary>
        /// Parallel to y axis.
        /// </summary>
        ParallelYAxis
    }
}
