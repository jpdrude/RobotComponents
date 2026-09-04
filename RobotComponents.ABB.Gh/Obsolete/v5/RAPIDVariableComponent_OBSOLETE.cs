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

#pragma warning disable CS1591 // Missing XML comment — obsolete shim, kept for .ghx backwards compatibility.

// System Libs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
// Grasshopper Libs
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
// RobotComponents
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Components;
using RobotComponents.ABB.Gh.Parameters.Actions.Dynamic;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Obsolete
{
    /// <summary>
    /// RobotComponents RAPID Variable Declaration Component.
    /// Supports both scalar and array declarations.
    /// Right-click → "Set Array Size" switches between scalar and array mode.
    /// </summary>
    /// <remarks>
    /// Hidden from the menu since the Value/Values inputs were changed from Param_String to
    /// Param_GenericObject (so a RAPID declaration/variable/expression resolves to its actual
    /// value instead of a stringified description). Retained so older .gh files that placed this
    /// component before that change continue to load and resolve their saved param GUIDs and
    /// param type unchanged; the live component now has a new GUID for its new input type.
    /// </remarks>
    [Obsolete("This component is OBSOLETE and will be removed in the future. Use RAPID Variable instead.", false)]
    public class RAPIDVariableComponent_OBSOLETE : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private bool _expire = false;
        private bool _arraySizeInputParam = false;

        // Fixed parameter count: Level(0), Scope(1), Keyword(2), Type(3), Name(4).
        // Index 5 onward is mode-dependent:
        //   Scalar : Value       (text, item,  optional)
        //   Array  : Array Size  (int,  item)  +  Values  (text, list, optional)
        private const int _fixedParamCount = 5;

        private const string _valueName     = "Value";
        private const string _arraySizeName = "Array Size";
        private const string _valuesName    = "Values";
        #endregion

        public RAPIDVariableComponent_OBSOLETE() : base("RAPID Variable", "RV", "Advanced RAPID Features",
            "Declares a RAPID variable (VAR, PERS, or INOUT). " +
            "Right-click to enable array mode.")
        {
            Message = "EXTENDABLE";
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Level", "L",
                "Declaration level. Use 0 for Module scope, 1 for Routine scope.",
                GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Scope", "S",
                "Variable scope. Use 0 for GLOBAL and 1 for LOCAL.",
                GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Keyword", "K",
                "Storage keyword. Use 0 for VAR, 1 for PERS, 2 for INOUT, 3 for CONST.",
                GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Type", "T",
                "RAPID data type (e.g. num, bool, string).",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N",
                "Variable name.",
                GH_ParamAccess.item);
            // Index 5 — scalar mode default
            pManager.AddTextParameter(_valueName, "V",
                "Initial value. Leave unconnected to omit the assignment.",
                GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RAPIDVariable(), "Variable", "V",
                "Resulting RAPID variable declaration.",
                GH_ParamAccess.item);
            pManager.RegisterParam(new Param_CodeLine(), "Variable Declaration", "VD",
                "RAPID code line representing the variable declaration.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Auto-create value lists on first use
            if (this.Params.Input[0].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, typeof(RAPIDVariableLevel), 0);
            }
            if (this.Params.Input[1].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, typeof(Scope), 1);
            }
            if (this.Params.Input[2].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, typeof(RAPIDVariableKeyword), 2);
            }

            if (_expire)
            {
                _expire = false;
                OnPingDocument()?.ScheduleSolution(5, d => ExpireSolution(true));
                return;
            }

            // --- Read fixed inputs ---
            int level = 0, scope = 2, keyword = 0;
            string type = "", name = "";

            if (!DA.GetData(0, ref level))   { level   = 0; }
            if (!DA.GetData(1, ref scope))   { scope   = 2; }
            if (!DA.GetData(2, ref keyword)) { keyword = 0; }
            if (!DA.GetData(3, ref type))    { return; }
            if (!DA.GetData(4, ref name))    { return; }

            // Resolve scope defaults
            if (scope == 2) scope = level == 1 ? 1 : 0;

            // Validate
            if (level < 0 || level > 1)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Level <{level}> is invalid. Use 0 for Module and 1 for Routine.");
            if (keyword < 0 || keyword > 3)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Keyword <{keyword}> is invalid. Use 0 for VAR, 1 for PERS, 2 for INOUT, 3 for CONST.");
            if (scope == 0 && level == 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Routine variables cannot be global. Scope set to LOCAL.");
                scope = 1;
            }
            if (keyword == 3 && level == 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Routines cannot contain CONST variables. Keyword set to VAR.");
                keyword = 0;
            }

            bool isArray = Params.Input.Any(x => x.Name == _arraySizeName);

            if (isArray)
            {
                // --- Array mode ---
                int arraySize = 0;
                if (!DA.GetData(_arraySizeName, ref arraySize)) { return; }

                if (arraySize <= 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        $"Array size must be greater than 0, got {arraySize}.");
                    return;
                }

                List<string> values = new List<string>();
                DA.GetDataList(_valuesName, values); // optional

                if (values.Count > 0 && values.Count != arraySize)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        $"Number of values ({values.Count}) does not match array size ({arraySize}). " +
                        "All values must be provided or none.");

                string arrayInit = values.Count == arraySize
                    ? $"[{string.Join(", ", values)}]"
                    : null;

                var variable = new RAPIDVariable(
                    (RAPIDVariableLevel)level, (Scope)scope, (RAPIDVariableKeyword)keyword,
                    type.Trim(), name.Trim(), arrayInit);

                DA.SetData(0, variable);
                DA.SetData(1, BuildArrayDeclaration(variable, arraySize));
            }
            else
            {
                // --- Scalar mode ---
                string value = null;
                DA.GetData(_valueName, ref value);

                if (keyword == 3 && string.IsNullOrEmpty(value))
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        "CONST variables must have an initial value.");

                var variable = new RAPIDVariable(
                    (RAPIDVariableLevel)level, (Scope)scope, (RAPIDVariableKeyword)keyword,
                    type.Trim(), name.Trim(), value?.Trim());

                DA.SetData(0, variable);
                DA.SetData(1, BuildScalarDeclaration(variable));
            }
        }

        #region declaration helpers
        private CodeLine BuildScalarDeclaration(RAPIDVariable var)
        {
            string prefix = var.Level == RAPIDVariableLevel.Module && var.Scope == Scope.LOCAL
                ? "LOCAL " : "";
            string code = prefix + var.ToRAPID();

            return var.Level == RAPIDVariableLevel.Module
                ? new CodeLine(code, CodeType.Declaration)
                : new CodeLine(code, CodeType.Instruction);
        }

        private CodeLine BuildArrayDeclaration(RAPIDVariable var, int arraySize)
        {
            string prefix = var.Level == RAPIDVariableLevel.Module && var.Scope == Scope.LOCAL
                ? "LOCAL " : "";

            // RAPID array syntax:  KEYWORD type name{size} := [v1, v2, ...];
            string decl = $"{var.Keyword} {var.Type} {var.Name}{{{arraySize}}}";
            if (!string.IsNullOrEmpty(var.Value))
                decl += $" := {var.Value}";
            decl += ";";

            string code = prefix + decl;

            return var.Level == RAPIDVariableLevel.Module
                ? new CodeLine(code, CodeType.Declaration)
                : new CodeLine(code, CodeType.Instruction);
        }
        #endregion

        #region menu items
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Set Array Size", MenuItemClickArraySize, true, _arraySizeInputParam);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void MenuItemClickArraySize(object sender, EventArgs e)
        {
            RecordUndoEvent("Set Array Size");
            _arraySizeInputParam = !_arraySizeInputParam;
            ToggleArrayParams();
        }

        /// <summary>
        /// Swaps the parameter set at index 5+ between scalar mode (Value item) and
        /// array mode (Array Size int + Values list).
        /// </summary>
        private void ToggleArrayParams()
        {
            if (_arraySizeInputParam)
            {
                // Remove scalar "Value" param
                var valueParam = Params.Input.FirstOrDefault(x => x.Name == _valueName);
                if (valueParam != null)
                    Params.UnregisterInputParameter(valueParam, true);

                // Add "Array Size" at the end of fixed params
                Params.RegisterInputParam(new Param_Integer
                {
                    Name        = _arraySizeName,
                    NickName    = "AS",
                    Description = "Number of elements in the array.",
                    Access      = GH_ParamAccess.item
                }, _fixedParamCount);

                // Add "Values" after Array Size
                Params.RegisterInputParam(new Param_String
                {
                    Name        = _valuesName,
                    NickName    = "V",
                    Description = "Initial values for the array elements as a list of strings. " +
                                  "Count must match Array Size. Leave unconnected for an uninitialised array.",
                    Access      = GH_ParamAccess.list,
                    Optional    = true
                }, _fixedParamCount + 1);
            }
            else
            {
                // Remove array params
                var arraySizeParam = Params.Input.FirstOrDefault(x => x.Name == _arraySizeName);
                if (arraySizeParam != null)
                    Params.UnregisterInputParameter(arraySizeParam, true);

                var valuesParam = Params.Input.FirstOrDefault(x => x.Name == _valuesName);
                if (valuesParam != null)
                    Params.UnregisterInputParameter(valuesParam, true);

                // Restore scalar "Value" param
                Params.RegisterInputParam(new Param_String
                {
                    Name        = _valueName,
                    NickName    = "V",
                    Description = "Initial value. Leave unconnected to omit the assignment.",
                    Access      = GH_ParamAccess.item,
                    Optional    = true
                }, _fixedParamCount);
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        #endregion

        #region serialization
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("ArraySizeInputParam", _arraySizeInputParam);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("ArraySizeInputParam"))
                _arraySizeInputParam = reader.GetBoolean("ArraySizeInputParam");
            return base.Read(reader);
        }
        #endregion

        #region IGH_VariableParameterComponent
        // Menu-driven only — no + / - buttons.
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index) => false;
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index) => false;
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index) => null;
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index) => false;
        void IGH_VariableParameterComponent.VariableParameterMaintenance() { }
        #endregion

        #region properties
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }

        public override bool Obsolete
        {
            get { return true; }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.Variable_Icon; }
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
