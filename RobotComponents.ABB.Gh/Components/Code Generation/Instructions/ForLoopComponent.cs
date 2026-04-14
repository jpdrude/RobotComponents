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
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Definitions;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents For Loop Component.
    /// Generates a RAPID FOR...ENDFOR loop from a counter variable, an interval (from/to), and a list of actions.
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
                "Counter variable (must be of RAPID type num).",
                GH_ParamAccess.item);
            pManager.AddIntervalParameter("Range", "R",
                "Loop range as a domain. The start (T0) is the FROM value and the end (T1) is the TO value.",
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
            pManager.RegisterParam(new Param_Action(), "For Loop", "FL",
                "RAPID FOR loop as a list of code lines and actions.",
                GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RAPIDVariable counter = null;
            Rhino.Geometry.Interval range = new Rhino.Geometry.Interval();
            List<IAction> bodyActions = new List<IAction>();

            if (!DA.GetData(0, ref counter)) { return; }
            if (!DA.GetData(1, ref range)) { return; }
            if (!DA.GetDataList(2, bodyActions)) { return; }

            // Validate that the counter variable has a numeric RAPID type
            if (!string.Equals(counter.Type, "num", StringComparison.OrdinalIgnoreCase))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Counter variable type is \"{counter.Type}\". RAPID FOR loops require a num counter.");
            }

            // Warn if the range values are not integers (FOR loop counters are integers in RAPID)
            double from = range.T0;
            double to = range.T1;

            if (from != Math.Floor(from))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"FROM value ({from}) is not an integer. RAPID FOR loop counters must be integers.");
            }
            if (to != Math.Floor(to))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"TO value ({to}) is not an integer. RAPID FOR loop counters must be integers.");
            }

            string fromStr = ((int)from).ToString(CultureInfo.InvariantCulture);
            string toStr = ((int)to).ToString(CultureInfo.InvariantCulture);

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
