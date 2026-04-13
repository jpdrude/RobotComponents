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
using Grasshopper.Kernel.Types;
// RobotComponents
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions.Dynamic;
using RobotComponents.ABB.Gh.Parameters.Definitions;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents RAPID Variable Assignment Component.
    /// Produces a RAPID assignment instruction: variableName := value;
    /// </summary>
    public class AssignVariableValueComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public AssignVariableValueComponent() : base("Assign Variable Value", "AVV", "Code Generation",
            "Creates a RAPID assignment instruction (variableName := value;). " +
            "Connect either a RAPID Variable or a plain text name to the Variable input.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Variable", "V",
                "Variable to assign to. Connect a RAPID Variable param or a plain text variable name.",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Value", "Val",
                "Value to assign. Any valid RAPID expression, e.g. 42, TRUE, \"hello\".",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RAPIDVariable(), "Variable", "V",
                "Pass-through of the input RAPID Variable. Null when only a name string was connected.",
                GH_ParamAccess.item);
            pManager.RegisterParam(new Param_CodeLine(), "Assignment", "A",
                "RAPID assignment instruction: variableName := value;",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Variable name.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_Goo variableGoo = null;
            string value = "";

            if (!DA.GetData(0, ref variableGoo)) { return; }
            if (!DA.GetData(1, ref value)) { return; }

            // Try to extract a RAPIDVariable from the input
            RAPIDVariable variable = null;
            string variableName = null;

            if (variableGoo is GH_RAPIDVariable rapidVarGoo)
            {
                variable = rapidVarGoo.Value;
                variableName = variable?.Name;
            }
            else
            {
                // Try casting to RAPIDVariable directly
                RAPIDVariable directVar = null;
                if (variableGoo.CastTo(out directVar) && directVar != null)
                {
                    variable = directVar;
                    variableName = variable.Name;
                }
                else
                {
                    // Fall back to interpreting the input as a plain text name
                    GH_String ghStr = null;
                    if (variableGoo.CastTo(out ghStr) && ghStr != null)
                    {
                        variableName = ghStr.Value;
                    }
                    else
                    {
                        variableName = variableGoo.ToString();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(variableName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Variable name is empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Value is empty.");
                return;
            }

            string code = $"{variableName} := {value.Trim()};";

            DA.SetData(0, variable);
            DA.SetData(1, new CodeLine(code, CodeType.Instruction));
            DA.SetData(2, variableName);
        }

        #region properties
        /// <summary>
        /// Override the component exposure.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
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
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A1F4C2E7-8D53-4B96-B0A3-7C2E5F9D3A81"); }
        }
        #endregion
    }
}
