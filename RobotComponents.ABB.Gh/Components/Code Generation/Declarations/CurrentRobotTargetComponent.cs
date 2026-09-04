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
// RobotComponents
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Current Robot Target Component.
    /// Wraps the RAPID built-in CRobT([\TaskRef]|[\TaskName] [\Tool] [\WObj]) function into a
    /// RAPIDExpression, returning the robot's current TCP position as a robtarget.
    /// </summary>
    public class CurrentRobotTargetComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public CurrentRobotTargetComponent() : base("Current Robot Target", "CRobT", "Advanced RAPID Features",
            "Wraps the RAPID built-in CRobT(...) function into a RAPID expression, returning the robot's " +
            "current TCP position as a robtarget. The result can be wired into any RAPID Expression input.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Tool", "T",
                "Optional tool to resolve the position for (\\Tool:=...). Accepts a Robot Tool, a RAPID Variable, " +
                "or a RAPID Expression. Leave unconnected to omit the \\Tool switch, i.e. use the tool the " +
                "program is currently working with.",
                GH_ParamAccess.item);
            pManager.AddGenericParameter("Work Object", "WO",
                "Optional work object to resolve the position relative to (\\WObj:=...). Accepts a Work Object, " +
                "a RAPID Variable, or a RAPID Expression. Leave unconnected to omit the \\WObj switch, i.e. use " +
                "the work object the program is currently working with.",
                GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RAPIDExpression(), "Expression", "E",
                "The CRobT(...) function call as a RAPID expression, usable as a value in other instructions.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object rawTool = null;
            object rawWObj = null;

            DA.GetData(0, ref rawTool);
            DA.GetData(1, ref rawWObj);

            // Both inputs are optional: when unconnected, ResolveRAPIDValueExpression(null) returns
            // null, so the corresponding \Tool / \WObj switch is simply omitted from the call.
            string toolText = HelperMethods.ResolveRAPIDValueExpression(rawTool);
            string wobjText = HelperMethods.ResolveRAPIDValueExpression(rawWObj);

            List<string> switches = new List<string>();
            if (!string.IsNullOrWhiteSpace(toolText)) { switches.Add($"\\Tool:={toolText}"); }
            if (!string.IsNullOrWhiteSpace(wobjText)) { switches.Add($"\\WObj:={wobjText}"); }

            // CRobT's optional switch arguments are space-separated, not comma-separated like a
            // regular RAPID function call, so this is built directly rather than through
            // RAPIDExpression.FromFunctionCall (which always comma-joins its arguments).
            RAPIDExpression expression = RAPIDExpression.FromString($"CRobT({string.Join(" ", switches)})");

            DA.SetData(0, new GH_RAPIDExpression(expression));
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
            get { return Properties.Resources.CurrentRobotTarget_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3C4396F5-DF3D-4EC8-8DBE-5217CECF9979"); }
        }
        #endregion
    }
}
