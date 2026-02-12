// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2025-2026 EDEK Uni Kassel
//
// Authors:
//   - Jan Philipp Drude (2025-2026)
//   - Johannes Pfleging (2025)
//
// For license details, see the LICENSE file in the project root.

namespace RobotComponents.ABB.Kinematics.IkGeo.Geometry
{
    /// <summary>
    /// Represents IkGeo's internal Vector3d class.
    /// </summary>
    internal struct Vector3d
    {
        /// <summary>
        /// public components of the vector.
        /// </summary>
        public double x, y, z;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3d"/> struct with the specified X, Y, and Z components.
        /// </summary>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        /// <param name="z">The Z component of the vector.</param>
        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3d"/> struct using a specified Rhino.Geometry.Point3d.
        /// </summary>
        /// <param name="point">A <see cref="Rhino.Geometry.Point3d"/> representing the 3D point to initialize the vector with.</param>
        public Vector3d(Rhino.Geometry.Point3d point)
        {
            x = point.X;
            y = point.Y;
            z = point.Z;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3d"/> class using a specified Rhino.Geometry.Vector3d.
        /// </summary>
        /// <param name="vector">The <see cref="Rhino.Geometry.Vector3d"/> instance used to initialize the vector components.</param>
        public Vector3d(Rhino.Geometry.Vector3d vector)
        {
            x = vector.X;
            y = vector.Y;
            z = vector.Z;
        }

        /// <summary>
        /// Converts the current vector to a <see cref="Rhino.Geometry.Point3d"/> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="Rhino.Geometry.Point3d"/> representing the point with the same X, Y, and Z coordinates as the current instance.
        /// </returns>
        public Rhino.Geometry.Point3d ToRhinoPoint3d()
        {
            return new Rhino.Geometry.Point3d(x, y, z);
        }

        /// <summary>
        /// Converts the current vector to a <see cref="Rhino.Geometry.Vector3d"/> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="Rhino.Geometry.Vector3d"/> representing the point with the same X, Y, and Z coordinates as the current instance.
        /// </returns>
        public Rhino.Geometry.Vector3d ToRhinoVector3d()
        {
            return new Rhino.Geometry.Vector3d(x, y, z);
        }
    }
}

