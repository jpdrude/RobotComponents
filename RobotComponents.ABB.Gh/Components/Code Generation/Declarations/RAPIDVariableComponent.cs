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
using System.Text;

// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Parameters.Actions.Dynamic;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents RAPID Variable Declaration Component.
    /// </summary>
    public class RAPIDVariableComponent : GH_RobotComponent
    {
        #region fields
        private bool _expire = false;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel.
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public RAPIDVariableComponent() : base("RAPID Variable", "RV", "Code Generation",
            "Declares a RAPID variable (VAR, PERS, or INOUT).")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Level", "L", "Declaration level. Use 0 for Module scope, 1 for Routine scope.", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Scope", "S", "Variable scope. Use 0 for GLOBAL and 1 for LOCAL. Notice, routine variables are always local and have to be declared at the beginning of a routine.", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Keyword", "K", "Storage keyword. Use 0 for VAR, 1 for PERS, 2 for INOUT.", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Type", "T", "RAPID data type (e.g. num, bool, string).", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Variable name.", GH_ParamAccess.item);
            pManager.AddTextParameter("Value", "V", "Initial value. Leave unconnected to omit the assignment.", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RAPIDVariable(), "Variable", "V", "Resulting RAPID variable declaration.", GH_ParamAccess.item);
            pManager.RegisterParam(new Param_CodeLine(), "Variable Declaration", "VD", "RAPID code line representing the variable declaration. Notice, routine variables are always local and have to be declared at the beginning of a routine.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Variable name.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Create value lists when inputs are not connected
            if (this.Params.Input[0].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, typeof(RAPIDVariableLevel), 0);
            }
            if (this.Params.Input[2].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, typeof(RAPIDVariableKeyword), 2);
            }

            if (_expire == true)
            {
                _expire = false;
                OnPingDocument()?.ScheduleSolution(5, d => ExpireSolution(true));
                return;
            }

            int level = 0;
            int scope = 2;
            int keyword = 0;
            string type = "";
            string name = "";
            string value = null;

            if (!DA.GetData(0, ref level)) { level = 0; }
            if (!DA.GetData(1, ref scope)) { scope = 2; }
            if (!DA.GetData(2, ref keyword)) { keyword = 0; }
            if (!DA.GetData(3, ref type)) { return; }
            if (!DA.GetData(4, ref name)) { return; }
            DA.GetData(5, ref value);

            if (scope == 2 && level == 1)
            {
                scope = 1; // Routine-level variables are always LOCAL
            }
            else if (scope == 2 && level == 0)
            {
                scope = 0; // Module-level variables default to GLOBAL
            }

            if (level < (int)RAPIDVariableLevel.Module || level > (int)RAPIDVariableLevel.Routine)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Level <" + level + "> is invalid. Use 0 for Module and 1 for Routine.");
            }
            if (keyword < (int)RAPIDVariableKeyword.VAR || keyword > (int)RAPIDVariableKeyword.INOUT)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Keyword <" + keyword + "> is invalid. Use 0 for VAR, 1 for PERS, 2 for INOUT.");
            }
            if (scope == 0 && level == 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Routine variables cannot be global. Scope has been set to LOCAL.");
                scope = 1;
            }

            RAPIDVariable variable = new RAPIDVariable(
                (RAPIDVariableLevel)level,
                (Scope)scope,
                (RAPIDVariableKeyword)keyword,
                type.Trim(),
                name.Trim(),
                value?.Trim());

            DA.SetData(0, variable);
            DA.SetData(1, ToRAPIDDeclaration(variable));
            DA.SetData(2, variable.Name);
        }

        #region Additional Methods
        CodeLine ToRAPIDDeclaration(RAPIDVariable var)
        {
            string code = "";

            if (var.Level == RAPIDVariableLevel.Module)
                if (var.Scope == Scope.LOCAL)
                    code = "LOCAL ";

            code += var.ToRAPID();

            if (var.Level == RAPIDVariableLevel.Module)
                return new CodeLine(code, CodeType.Declaration);
            else
                return new CodeLine(code, CodeType.Instruction);
        }
        #endregion


        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
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
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3B7E1F4A-9C82-4D16-A05B-6E3D2C8F7B94"); }
        }
        #endregion
    }
}
