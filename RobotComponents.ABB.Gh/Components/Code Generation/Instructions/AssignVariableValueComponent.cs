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
using System.Linq;
using System.Windows.Forms;
// Grasshopper Libs
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
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
    /// Right-click → "Assign Array Values" switches to array mode, where
    /// the value input becomes a list and the output is variableName := [v1, v2, ...];
    /// </summary>
    public class AssignVariableValueComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private bool _arrayValuesInputParam = false;
        private bool _arrayIndexInputParam = false;

        private const string _scalarValueName = "Value";
        private const string _arrayValuesName = "Values";
        private const string _indexParamName  = "Index";
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public AssignVariableValueComponent() : base("Assign Variable Value", "AVV", "Advanced RAPID Features",
            "Creates a RAPID assignment instruction (variableName := value;). " +
            "Right-click to enable array assignment mode.")
        {
            Message = "EXTENDABLE";
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_RAPIDVariable(), "Variable", "V",
                "Variable to assign to.",
                GH_ParamAccess.item);
            // Index 1 — scalar mode default
            pManager.AddTextParameter(_scalarValueName, "Val",
                "Value to assign. Any valid RAPID expression, e.g. 42, TRUE, \"hello\".",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RAPIDVariable(), "Variable", "V",
                "Pass-through of the input RAPID Variable.",
                GH_ParamAccess.item);
            pManager.RegisterParam(new Param_CodeLine(), "Assignment", "A",
                "RAPID assignment instruction.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RAPIDVariable variable = null;
            if (!DA.GetData(0, ref variable)) { return; }

            if (string.IsNullOrWhiteSpace(variable.Name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Variable does not have a name.");
                return;
            }

            // Resolve optional array index (int literal or RAPID variable name)
            string indexExpr = null;
            if (_arrayIndexInputParam)
            {
                IGH_Goo indexGoo = null;
                if (!DA.GetData(_indexParamName, ref indexGoo) || indexGoo == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Array index is missing.");
                    return;
                }

                if (indexGoo is GH_RAPIDVariable rapidVarGoo)
                {
                    if (string.IsNullOrWhiteSpace(rapidVarGoo.Value?.Name))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "RAPID Variable used as index has no name.");
                        return;
                    }
                    indexExpr = rapidVarGoo.Value.Name;
                }
                else if (indexGoo.CastTo(out int intIndex))
                {
                    indexExpr = intIndex.ToString();
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Index must be an integer or a RAPID Variable.");
                    return;
                }
            }

            string targetName = indexExpr != null ? $"{variable.Name}{{{indexExpr}}}" : variable.Name;
            bool isArray = Params.Input.Any(x => x.Name == _arrayValuesName);

            if (isArray)
            {
                // --- Array assignment ---
                List<string> values = new List<string>();
                if (!DA.GetDataList(_arrayValuesName, values) || values.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Values list is empty.");
                    return;
                }

                string arrayLiteral = $"[{string.Join(", ", values)}]";
                variable.Value = arrayLiteral;
                string code = $"{targetName} := {arrayLiteral};";

                DA.SetData(0, variable);
                DA.SetData(1, new CodeLine(code, CodeType.Instruction));
            }
            else
            {
                // --- Scalar assignment ---
                string value = "";
                if (!DA.GetData(_scalarValueName, ref value)) { return; }

                if (string.IsNullOrWhiteSpace(value))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Value is empty.");
                    return;
                }

                variable.Value = value.Trim();
                string code = $"{targetName} := {value.Trim()};";

                DA.SetData(0, variable);
                DA.SetData(1, new CodeLine(code, CodeType.Instruction));
            }
        }

        #region menu items
        /// <summary>
        /// Appends right-click context menu items.
        /// </summary>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Assign Array Values", MenuItemClickArrayValues, true, _arrayValuesInputParam);
            Menu_AppendItem(menu, "Assign at Index", MenuItemClickArrayIndex, true, _arrayIndexInputParam);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void MenuItemClickArrayValues(object sender, EventArgs e)
        {
            RecordUndoEvent("Assign Array Values");
            _arrayValuesInputParam = !_arrayValuesInputParam;
            ToggleArrayParams();
        }

        private void MenuItemClickArrayIndex(object sender, EventArgs e)
        {
            RecordUndoEvent("Assign at Index");
            _arrayIndexInputParam = !_arrayIndexInputParam;
            ToggleIndexParam();
        }

        /// <summary>
        /// Swaps the value parameter at index 1 between scalar (item) and array (list) mode.
        /// </summary>
        private void ToggleArrayParams()
        {
            if (_arrayValuesInputParam)
            {
                // Remove scalar "Value" param
                var valueParam = Params.Input.FirstOrDefault(x => x.Name == _scalarValueName);
                if (valueParam != null)
                    Params.UnregisterInputParameter(valueParam, true);

                // Add "Values" list param at index 1
                Params.RegisterInputParam(new Param_String
                {
                    Name        = _arrayValuesName,
                    NickName    = "Val",
                    Description = "Values to assign as a list. " +
                                  "Output: variableName := [v1, v2, ...];",
                    Access      = GH_ParamAccess.list
                }, 1);
            }
            else
            {
                // Remove array "Values" param
                var valuesParam = Params.Input.FirstOrDefault(x => x.Name == _arrayValuesName);
                if (valuesParam != null)
                    Params.UnregisterInputParameter(valuesParam, true);

                // Restore scalar "Value" param at index 1
                Params.RegisterInputParam(new Param_String
                {
                    Name        = _scalarValueName,
                    NickName    = "Val",
                    Description = "Value to assign. Any valid RAPID expression, e.g. 42, TRUE, \"hello\".",
                    Access      = GH_ParamAccess.item
                }, 1);
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        /// <summary>
        /// Adds or removes the Index input parameter.
        /// </summary>
        private void ToggleIndexParam()
        {
            if (_arrayIndexInputParam)
            {
                Params.RegisterInputParam(new Param_GenericObject
                {
                    Name        = _indexParamName,
                    NickName    = "I",
                    Description = "Array index (1-based in RAPID). " +
                                  "Accepts an integer literal or a RAPID Variable whose name is used as the index expression. " +
                                  "Output: variableName{index} := value;",
                    Access      = GH_ParamAccess.item
                });
            }
            else
            {
                var indexParam = Params.Input.FirstOrDefault(x => x.Name == _indexParamName);
                if (indexParam != null)
                    Params.UnregisterInputParameter(indexParam, true);
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        #endregion

        #region serialization
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("ArrayValuesInputParam", _arrayValuesInputParam);
            writer.SetBoolean("ArrayIndexInputParam", _arrayIndexInputParam);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("ArrayValuesInputParam"))
                _arrayValuesInputParam = reader.GetBoolean("ArrayValuesInputParam");
            if (reader.ItemExists("ArrayIndexInputParam"))
                _arrayIndexInputParam = reader.GetBoolean("ArrayIndexInputParam");
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
            get { return Properties.Resources.AssignVariable_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A1F4C2E7-8D53-4B96-B0A3-7C2E5F9D3A81"); }
        }
        #endregion
    }
}
