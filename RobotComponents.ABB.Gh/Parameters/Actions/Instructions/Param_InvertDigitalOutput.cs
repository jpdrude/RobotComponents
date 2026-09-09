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
using RobotComponents.ABB.Gh.Goos.Actions.Instructions;

namespace RobotComponents.ABB.Gh.Parameters.Actions.Instructions
{
    /// <summary>
    /// Invert Digital Output parameter
    /// </summary>
    public class Param_InvertDigitalOutput : GH_RobotParam<GH_InvertDigitalOutput>
    {
        /// <summary>
        /// Initializes a new instance of the Param_InvertDigitalOutput class
        /// </summary>
        public Param_InvertDigitalOutput() : base("Invert Digital Output", "IDOP", "Parameters",
                "Contains the data of an Invert Digital Output instruction.")
        {
        }

        /// <summary>
        /// Converts this structure to a human-readable string.
        /// </summary>
        /// <returns> A string representation of the parameter. </returns>
        public override string ToString()
        {
            return "Invert Digital Output";
        }

        /// <summary>
        /// Gets or sets the name of the object. This field typically remains fixed during the lifetime of an object.
        /// </summary>
        public override string Name { get => base.Name; set => base.Name = value; }

        /// <summary>
        /// Override this function to supply a custom icon (24x24 pixels).
        /// The result of this property is cached, so don't worry if icon retrieval is not very fast.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.InvertDigitalOutput_Parameter_Icon; }
        }

        /// <summary>
        /// Gets the exposure of this object in the Graphical User Interface.The default is to expose everywhere.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

        /// <summary>
        /// Returns a consistent ID for this object type.
        /// Every object must supply a unique and unchanging ID that is used to identify objects of the same type.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("CF30CA34-C3C1-4802-BB79-0C1CFDFFABA4"); }
        }
    }
}
