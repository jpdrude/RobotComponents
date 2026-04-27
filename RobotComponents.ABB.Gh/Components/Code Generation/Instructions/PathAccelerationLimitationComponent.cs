// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2024 Arjen Deetman
//
// Authors:
//   - Arjen Deetman (2024)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
// Grasshopper Libs
using Grasshopper.Kernel;
// Grasshopper Libs (additional)
using Grasshopper.Kernel.Data;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions.Instructions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Action : Path Acceleration Limitation component.
    /// </summary>
    public class PathAccelerationLimitationComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public PathAccelerationLimitationComponent() : base("Path Acceleration Limitation", "PAL", "Code Generation",
              "Defines an instruction used to set or reset limitations on TCP acceleration and/or TCP deceleration along the movement path.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Acceleration Limitation", "AL", "Specifies whether or not the acceleration is limited as a Boolean.", GH_ParamAccess.item, false);
            var amParam = new Param_RAPIDExpression();
            amParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(0.0)), new GH_Path(0));
            pManager.AddParameter(amParam, "Acceleration Max", "AM",
                "Absolute value of the acceleration limitation in m/s^2. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);
            pManager.AddBooleanParameter("Deceleration Limitation", "DL", "Specifies whether or not the deceleration is limited as a Boolean.", GH_ParamAccess.item, false);
            var dmParam = new Param_RAPIDExpression();
            dmParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(0.0)), new GH_Path(0));
            pManager.AddParameter(dmParam, "Deceleration Max", "DM",
                "Absolute value of the deceleration limitation in m/s^2. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_PathAccelerationLimitation(), "Path Acceleration Limitation", "PAL", "Resulting Path Acceleration Limitation instruction");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool accelerationLimitation = false;
            GH_RAPIDExpression accelerationMaxExpr = null;
            bool decelerationLimitation = false;
            GH_RAPIDExpression decelerationMaxExpr = null;

            if (!DA.GetData(0, ref accelerationLimitation)) { return; }
            if (!DA.GetData(1, ref accelerationMaxExpr)) { return; }
            if (!DA.GetData(2, ref decelerationLimitation)) { return; }
            if (!DA.GetData(3, ref decelerationMaxExpr)) { return; }

            string accelerationMax = accelerationMaxExpr?.Value?.Expression ?? "0";
            string decelerationMax = decelerationMaxExpr?.Value?.Expression ?? "0";
            if (!RAPIDExpression.IsValidExpression(accelerationMax))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Acceleration Max \"{accelerationMax}\" does not appear to be a valid RAPID expression.");
            if (!RAPIDExpression.IsValidExpression(decelerationMax))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Deceleration Max \"{decelerationMax}\" does not appear to be a valid RAPID expression.");

            DA.SetData(0, new PathAccelerationLimitation(accelerationLimitation, accelerationMax, decelerationLimitation, decelerationMax));
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary | GH_Exposure.obscure; }
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
            get { return Properties.Resources.PathAccelerationLimitation_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B36062B1-3985-4AF1-972A-4BFF1AA2561F"); }
        }
        #endregion
    }

}
