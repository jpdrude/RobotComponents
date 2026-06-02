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
using System.Collections.Generic;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents While Loop Component.
    /// Generates a RAPID WHILE...ENDWHILE loop from a condition expression and a list of body actions.
    /// </summary>
    public class WhileLoopComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public WhileLoopComponent() : base("While Loop", "WL", "Advanced RAPID Features",
            "Creates a RAPID WHILE loop that repeats a set of actions while a condition is true.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_RAPIDExpression(), "Condition", "C",
                "Loop condition as a RAPID boolean expression (e.g. counter < 10). " +
                "Accepts a RAPIDExpression or a plain string.",
                GH_ParamAccess.item);
            pManager.AddParameter(new Param_Action(), "Actions", "A",
                "Actions to repeat inside the loop body.",
                GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Action(), "While Loop", "WL",
                "RAPID WHILE loop as a list of code lines and actions.",
                GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_RAPIDExpression condExpr = null;
            List<IAction> bodyActions = new List<IAction>();

            if (!DA.GetData(0, ref condExpr)) { return; }
            if (!DA.GetDataList(1, bodyActions)) { return; }

            string condition = HelperMethods.CheckRAPIDExpression(this, condExpr, "Condition", "true");

            List<IAction> loopCode = new List<IAction>();
            loopCode.Add(new CodeLine($"WHILE {condition} DO", CodeType.Instruction));
            foreach (IAction action in bodyActions)
            {
                IAction dup = action.DuplicateAction();
                dup.IndentationLevel = action.IndentationLevel + 1;
                loopCode.Add(dup);
            }
            loopCode.Add(new CodeLine("ENDWHILE", CodeType.Instruction));

            DA.SetDataList(0, loopCode);
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
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
            get { return Properties.Resources.CodeLine_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C5F3A7B2-4E08-4D19-B6F4-209C3E8D0F51"); }
        }
        #endregion
    }
}
