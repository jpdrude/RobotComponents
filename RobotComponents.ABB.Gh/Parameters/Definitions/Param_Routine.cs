// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2025 EDEK Uni Kassel
//
// Author:
//   - Jan Philipp Drude (2025)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
// Grasshopper Libs
using Grasshopper.Kernel;

// RobotComponents Libs
using RobotComponents.ABB.Gh.Goos.Definitions;

namespace RobotComponents.ABB.Gh.Parameters.Definitions
{
    /// <summary>
    /// Routine parameter
    /// </summary>
    public class Param_Routine : GH_RobotParam<GH_Routine>
    {
        /// <summary>
        /// Initializes a new instance of the Param_Routine class
        /// </summary>
        public Param_Routine() : base("Routine Parameter", "RP", "Parameters",
                "Contains the data of a Routine (PROC or TRAP).")
        {
        }

        /// <summary>
        /// Converts this structure to a human-readable string.
        /// </summary>
        /// <returns> A string representation of the parameter. </returns>
        public override string ToString()
        {
            return "Routine";
        }

        /// <summary>
        /// Gets or sets the name of the object. This field typically remains fixed during the lifetime of an object.
        /// </summary>
        public override string Name { get => "Routine"; set => base.Name = value; }

        /// <summary>
        /// Override this function to supply a custom icon (24x24 pixels). 
        /// The result of this property is cached, so don't worry if icon retrieval is not very fast.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.RoutineParameter_Icon; }
        }

        /// <summary>
        /// Gets the exposure of this object in the Graphical User Interface.The default is to expose everywhere.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quarternary; }
        }

        /// <summary>
        /// Returns a consistent ID for this object type. 
        /// Every object must supply a unique and unchanging ID that is used to identify objects of the same type.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7E3F8A2D-D4C6-4B91-A8E7-6C9D2F5B4E8A"); }
        }
    }
}
