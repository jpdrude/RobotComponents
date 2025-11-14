// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2025 EDEK Uni Kassel
//
// Authors:
//   - Jan Philipp Drude (2025)
//   - Johannes Pfleging (2025)
//
// For license details, see the LICENSE file in the project root.

namespace RobotComponents.ABB.Kinematics.IkGeo.Geometry
{
    /// <summary>
    /// Represents IkGeo's internal Quaternion class.
    /// </summary>
    /// <remarks>
    /// Be careful with quaternion naming conventions. 
    /// The conversion is as follows; A=w, B=x, C=y, D=z.
    /// </remarks>
    internal struct Quaternion
    {
        /// <summary>
        /// Public components of the quaternion.
        /// </summary>
        /// <remarks>
        /// Fields are named according to IkGeo's convention.
        /// </remarks>
        public double x, y, z, w;

        /// <summary>
        /// Initializes a new instance of the <see cref="Quaternion"/> structure with the specified components.
        /// </summary>
        /// <param name="x">The X component of the quaternion. (<c>B</c> component of Rhino quaternion)</param>
        /// <param name="y">The Y component of the quaternion. (<c>C</c> component of Rhino quaternion)</param>
        /// <param name="z">The Z component of the quaternion. (<c>D</c> component of Rhino quaternion)</param>
        /// <param name="w">The W component of the quaternion, typically representing the scalar part. (<c>A</c> component of Rhino quaternion)</param>
        public Quaternion(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quaternion"/> structure using the specified 
        /// <see cref="Rhino.Geometry.Quaternion"/>.
        /// </summary>
        /// <remarks>The components of the <see cref="Quaternion"/> are mapped as follows: 
        /// <list type="bullet"> <item><description><c>x</c> is set to the <c>B</c> component of the input
        /// quaternion.</description></item> <item><description><c>y</c> is set to the <c>C</c> component of the input
        /// quaternion.</description></item> <item><description><c>z</c> is set to the <c>D</c> component of the input
        /// quaternion.</description></item> <item><description><c>w</c> is set to the <c>A</c> component of the input
        /// quaternion.</description></item> </list></remarks>
        /// <param name="quaternion">The <see cref="Rhino.Geometry.Quaternion"/> instance from which to initialize the components of the
        /// quaternion.</param>
        public Quaternion(Rhino.Geometry.Quaternion quaternion)
        {
            x = quaternion.B;
            y = quaternion.C;
            z = quaternion.D;
            w = quaternion.A;
        }

        /// <summary>
        /// Converts this <see cref="Quaternion"/> instance to a <see cref="Rhino.Geometry.Quaternion"/>.
        /// </summary>
        public Rhino.Geometry.Quaternion ToRhinoQuaternion()
        {
            return new Rhino.Geometry.Quaternion(w, x, y, z);
        }
    }
}

