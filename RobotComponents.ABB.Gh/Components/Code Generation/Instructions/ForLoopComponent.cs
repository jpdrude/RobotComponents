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
using System.Globalization;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents For Loop Component.
    /// Generates a RAPID FOR...ENDFOR loop from an optional counter variable,
    /// a From value, a To value, and a list of body actions.
    /// When no counter variable is provided a local VAR num i is used automatically.
    /// From and To accept integers, doubles, RAPIDExpression strings, or RAPIDVariable references.
    /// </summary>
    public class ForLoopComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public ForLoopComponent() : base("For Loop", "FL", "Advanced RAPID Features",
            "Creates a RAPID FOR loop that repeats a set of actions from a start value to an end value.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_RAPIDVariable(), "Counter", "C",
                "Counter variable (must be of RAPID type num). " +
                "Leave unconnected to automatically use a local VAR num i.",
                GH_ParamAccess.item);
            pManager.AddParameter(new Param_RAPIDExpression(), "From", "Fr",
                "Start value of the loop range. Accepts an integer, double, RAPID variable, or expression. " +
                "Defaults to 1 when unconnected.",
                GH_ParamAccess.item);
            pManager.AddParameter(new Param_RAPIDExpression(), "To", "To",
                "End value of the loop range. Accepts an integer, double, RAPID variable, or expression.",
                GH_ParamAccess.item);
            pManager.AddParameter(new Param_Action(), "Actions", "A",
                "Actions to repeat inside the loop body.",
                GH_ParamAccess.list);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Action(), "For Loop", "FL",
                "RAPID FOR loop as a list of code lines and actions.",
                GH_ParamAccess.list);
            pManager.RegisterParam(new Param_RAPIDVariable(), "Counter", "C",
                "The counter variable used in the loop (provided or auto-created as VAR num i).",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_RAPIDVariable counterGoo = null;
            GH_RAPIDExpression fromExpr = null;
            GH_RAPIDExpression toExpr = null;
            List<IAction> bodyActions = new List<IAction>();

            DA.GetData(0, ref counterGoo);
            DA.GetData(1, ref fromExpr);
            if (!DA.GetData(2, ref toExpr)) { return; }
            if (!DA.GetDataList(3, bodyActions)) { return; }

            // Resolve counter: use supplied variable or fall back to VAR num i
            RAPIDVariable counter = counterGoo?.Value;
            bool autoCreated = counter == null || string.IsNullOrEmpty(counter.Name);
            if (autoCreated)
                counter = new RAPIDVariable(RAPIDVariableLevel.Routine, Scope.LOCAL, RAPIDVariableKeyword.VAR, "num", "i");

            // Type check only when user supplied the variable
            if (!autoCreated && !string.Equals(counter.Type, "num", StringComparison.OrdinalIgnoreCase))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Counter variable type is \"{counter.Type}\". RAPID FOR loops require a num counter.");

            // Resolve From (default "1" when unconnected) and To
            string fromStr = fromExpr != null
                ? HelperMethods.CheckRAPIDExpression(this, fromExpr, "From", "1")
                : "1";
            string toStr = HelperMethods.CheckRAPIDExpression(this, toExpr, "To", "");

            // Integer check only for numeric literals (variable names will not parse)
            if (double.TryParse(fromStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double fromNum)
                && fromNum != Math.Floor(fromNum))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"From value ({fromStr}) is not an integer. RAPID FOR loop counters must be integers.");

            if (double.TryParse(toStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double toNum)
                && toNum != Math.Floor(toNum))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"To value ({toStr}) is not an integer. RAPID FOR loop counters must be integers.");

            // Build output code lines
            List<IAction> loopCode = new List<IAction>();
            loopCode.Add(new CodeLine($"FOR {counter.Name} FROM {fromStr} TO {toStr} DO", CodeType.Instruction));
            foreach (IAction action in bodyActions)
            {
                IAction dup = action.DuplicateAction();
                dup.IndentationLevel = action.IndentationLevel + 1;
                loopCode.Add(dup);
            }
            loopCode.Add(new CodeLine("ENDFOR", CodeType.Instruction));

            DA.SetDataList(0, loopCode);
            DA.SetData(1, new GH_RAPIDVariable(counter));
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
            get { return Properties.Resources.ForLoop_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3D7F1A92-6E84-4C5B-B2F9-0A8D3E6C1F47"); }
        }
        #endregion
    }
}
