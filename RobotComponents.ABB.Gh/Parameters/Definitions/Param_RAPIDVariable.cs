// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2026 EDEK Uni Kassel
//
// Author:
//   - Jan Philipp Drude (2026)
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
    /// RAPID Variable parameter.
    /// </summary>
    public class Param_RAPIDVariable : GH_RobotParam<GH_RAPIDVariable>
    {
        /// <summary>
        /// Initializes a new instance of the Param_RAPIDVariable class.
        /// </summary>
        public Param_RAPIDVariable() : base("RAPID Variable", "RV", "Parameters",
                "Contains the data of a RAPID variable declaration (VAR, PERS, or INOUT).")
        {
        }

        /// <summary>
        /// Converts this structure to a human-readable string.
        /// </summary>
        /// <returns> A string representation of the parameter. </returns>
        public override string ToString()
        {
            return "RAPID Variable";
        }

        /// <summary>
        /// Gets or sets the name of the object.
        /// </summary>
        public override string Name { get => base.Name; set => base.Name = value; }

        /// <summary>
        /// Override this function to supply a custom icon (24x24 pixels).
        /// The result of this property is cached, so don't worry if icon retrieval is not very fast.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.Variable_Parameter_Icon; }
        }

        /// <summary>
        /// Gets the exposure of this object in the Graphical User Interface.
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
            get { return new Guid("2C8D3F7A-1E94-4B52-A06C-9D5E1B3F8C2A"); }
        }
    }
}
