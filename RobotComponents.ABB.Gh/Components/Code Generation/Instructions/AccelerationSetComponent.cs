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

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Action : Acceleration Set component.
    /// </summary>
    public class AccelerationSetComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public AccelerationSetComponent() : base("Acceleration Set", "AS", "Code Generation",
              "Defines an instruction to override the acceleration and decceleration values.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            var accelParam = new Param_RAPIDExpression();
            accelParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(100.0)), new GH_Path(0));
            pManager.AddParameter(accelParam, "Acceleration", "A",
                "Acceleration and deceleration as a percentage (20-100) of normal values. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);
            var rampParam = new Param_RAPIDExpression();
            rampParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(100.0)), new GH_Path(0));
            pManager.AddParameter(rampParam, "Ramp", "R",
                "Rate at which acceleration increases as a percentage (10-100) of normal values. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_AccelerationSet(), "Acceleration Set", "AS", "Resulting Acceleration Set instruction");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_RAPIDExpression accelerationExpr = null;
            GH_RAPIDExpression rampExpr = null;

            if (!DA.GetData(0, ref accelerationExpr)) { return; }
            if (!DA.GetData(1, ref rampExpr)) { return; }

            string acceleration = accelerationExpr?.Value?.Expression ?? "100";
            string ramp = rampExpr?.Value?.Expression ?? "100";
            if (!RAPIDExpression.IsValidExpression(acceleration))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Acceleration \"{acceleration}\" does not appear to be a valid RAPID expression.");
            if (!RAPIDExpression.IsValidExpression(ramp))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Ramp \"{ramp}\" does not appear to be a valid RAPID expression.");

            DA.SetData(0, new AccelerationSet(acceleration, ramp));
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
            get { return Properties.Resources.AccelerationSet_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("39F78FC0-5D07-4FC0-9860-EBBDBF7801C6"); }
        }
        #endregion
    }
}