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

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Configuration Number component: builds the cfx value of a confdata from
    /// the arm's three configuration choices.
    /// </summary>
    /// <remarks>
    /// cfx = (shoulder ? 4 : 0) + (elbow ? 2 : 0) + (wrist ? 1 : 0), the same bit order the
    /// inverse kinematics uses to sort its eight solutions (IkGeoSolver).
    /// </remarks>
    public class ConfigurationNumberComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public ConfigurationNumberComponent() : base("Configuration Number", "CN", "Code Generation",
              "Builds the CFX value of a Configuration Data from the arm configuration: shoulder, elbow and wrist.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Shoulder", "S", "True if the wrist center is behind axis 1 (shoulder turned back). Adds 4 to CFX.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Elbow", "E", "True if the wrist center is behind the lower arm. Adds 2 to CFX.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Wrist", "W", "True if axis 5 is negative (wrist flipped). Adds 1 to CFX.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("CFX", "CFX", "The robot configuration as a number from 0 to 7. Wire it into the CFX input of Configuration Data.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool shoulder = false;
            bool elbow = false;
            bool wrist = false;

            if (!DA.GetData(0, ref shoulder)) { return; }
            if (!DA.GetData(1, ref elbow)) { return; }
            if (!DA.GetData(2, ref wrist)) { return; }

            int cfx = (shoulder ? 1 << 2 : 0) | (elbow ? 1 << 1 : 0) | (wrist ? 1 : 0);

            DA.SetData(0, cfx);
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Gets whether this object is obsolete.
        /// </summary>
        public override bool Obsolete
        {
            get { return false; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ConfigurationNumber_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e9d2990f-40d6-4cd0-8f70-a1d2db7abbba"); }
        }
        #endregion
    }
}
