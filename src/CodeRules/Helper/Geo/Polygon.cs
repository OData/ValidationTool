// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.Rule.Helper.Geo
{
    #region Namespace.
    using System.Collections.Generic;
    using System.Linq;
    #endregion

    /// <summary>
    /// The Polygon class.
    /// </summary>
    public class Polygon
    {
        /// <summary>
        /// The constructor of the Polygon class.
        /// </summary>
        /// <param name="endpoints">All the endpoints of a polygon.</param>
        public Polygon(IEnumerable<Point> endpoints)
        {
            this.endpoints = endpoints.ToList();
            this.sides = new List<SegmentEquation>();
            this.Initialize();
        }

        #region Encapsulate fields.
        /// <summary>
        /// Gets the endpoints of the polygon.
        /// </summary>
        public List<Point> Endpoints
        {
            get
            {
                return this.endpoints;
            }
        }

        /// <summary>
        /// Gets the sides of the polygon.
        /// </summary>
        public List<SegmentEquation> Sides
        {
            get
            {
                return this.sides;
            }
        }
        #endregion

        /// <summary>
        /// Judge wether the specified point lies within the interior or on the boundary of the specified polygon.
        /// </summary>
        /// <param name="pt">The specified point.</param>
        /// <returns>Return true if the specified point intersects the polygon, otherwise false.</returns>
        public bool IsIntersects(Point pt)
        {
            var line = new LinearEquation(pt, LinearType.ParallelXAxis);
            var pts = SegmentEquation.GetIntersectionSp(this.sides, line);
            int counter = 0;
            foreach (var point in pts)
            {
                if (point == pt)
                {
                    return true;
                }
                else
                {
                    if (point.X < pt.X)
                    {
                        counter++;
                    }
                }
            }

            return counter % 2 == 1;
        }

        #region Private members.
        /// <summary>
        /// The endpoints of the polygon.
        /// </summary>
        private List<Point> endpoints;

        /// <summary>
        /// The sides of the polygon.
        /// </summary>
        private List<SegmentEquation> sides;
        #endregion

        #region Private methods.
        /// <summary>
        /// Create the polygon using all the endpoints.
        /// </summary>
        private void Initialize()
        {
            for (int i = 0; i < this.endpoints.Count - 1; i++)
            {
                this.sides.Add(new SegmentEquation(this.endpoints[i], this.endpoints[i + 1]));
            }

            this.sides.Add(new SegmentEquation(this.endpoints[this.endpoints.Count - 1], this.endpoints[0]));
        }
        #endregion

    }
}
