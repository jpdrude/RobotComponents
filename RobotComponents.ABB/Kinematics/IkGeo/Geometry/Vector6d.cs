// This file is part of Robot Components. Robot Components is licensed 
// under the terms of GNU General Public License version 3.0 (GPL v3.0)
// as published by the Free Software Foundation. For more information and 
// the LICENSE file, see <https://github.com/RobotComponents/RobotComponents>.

// System Libs
using System.Collections.Generic;
// Robot Components Libs
using RobotComponents.ABB.Actions.Declarations;

namespace RobotComponents.ABB.Kinematics.IkGeo.Geometry
{
    /// <summary>
    /// Represents IkGeo's internal Vector6d class.
    /// </summary>
    /// <remarks>
    /// Stores 6-axis robot joint position angles.
    /// </remarks>
    internal struct Vector6d
    {
        /// <summary>
        /// public components of the vector.
        /// </summary>
        public double x1, x2, x3, x4, x5, x6;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector6d"/> class with six double-precision components representing robot joint position angles.
        /// </summary>
        /// <param name="x1">The value of the first robot joint.</param>
        /// <param name="x2">The value of the second robot joint.</param>
        /// <param name="x3">The value of the third robot joint.</param>
        /// <param name="x4">The value of the fourth robot joint.</param>
        /// <param name="x5">The value of the fifth robot joint.</param>
        /// <param name="x6">The value of the sixth robot joint.</param>
        public Vector6d(double x1, double x2, double x3, double x4, double x5, double x6)
        {
            this.x1 = x1;
            this.x2 = x2;
            this.x3 = x3;
            this.x4 = x4;
            this.x5 = x5;
            this.x6 = x6;
        }

        /// <summary>
        /// Converts the current object to an array of six double-precision floating-point numbers.
        /// </summary>
        /// <returns>An array of six <see cref="double"/> values representing the object's state.</returns>
        public double[] ToArray()
        {
            return new double[6] { x1, x2, x3, x4, x5, x6 };
        }

        /// <summary>
        /// Converts the current object to a list of double-precision floating-point numbers.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of type <see cref="double"/> containing the values represented by the object.</returns>
        public List<double> ToList()
        {
            return new List<double>() { x1, x2, x3, x4, x5, x6 };
        }

        /// <summary>
        /// Converts the current object to a <see cref="RobotJointPosition"/> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="RobotJointPosition"/> object initialized with the joint positions represented by the current instance.
        /// </returns>
        public RobotJointPosition ToRobotJointPosition()
        {
            return new RobotJointPosition(x1, x2, x3, x4, x5, x6);
        }
    }
}

