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
using System.Linq;
using System.Windows.Forms;
// Grasshopper Libs
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
// RobotComponents
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Empty Line Component.
    /// Outputs a blank RAPID code line to add visual spacing in the generated module, either
    /// among the instructions (default) or among the declarations.
    /// Right-click → "Add Type Input" adds an optional input to choose between the two.
    /// </summary>
    public class EmptyLineComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private bool _expire = false;
        private bool _typeInputParam = false;
        private const string _typeParamName = "Type";
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public EmptyLineComponent() : base("Empty Line", "EL", "Advanced RAPID Features",
            "Outputs a blank line to add visual spacing inside a RAPID routine. " +
            "Right-click to add a Type input to choose instruction vs. declaration.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // No inputs by default — right-click "Add Type Input" to add the optional
            // instruction/declaration selector.
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Action(), "Empty Line", "EL",
                "Blank RAPID code line for visual spacing.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int type = 0;

            if (_typeInputParam)
            {
                int typeParamIndex = Params.Input.FindIndex(x => x.Name == _typeParamName);

                if (typeParamIndex != -1)
                {
                    // Creates the input value list and attachs it to the input parameter
                    if (Params.Input[typeParamIndex].SourceCount == 0)
                    {
                        _expire = true;
                        HelperMethods.CreateValueList(this, typeof(CodeType), typeParamIndex);
                    }

                    // Expire solution of this component
                    if (_expire)
                    {
                        _expire = false;
                        this.ExpireSolution(true);
                        return;
                    }

                    if (!DA.GetData(typeParamIndex, ref type)) { type = 0; }

                    // Check if a right value is used for the type
                    if (type != 0 && type != 1)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Type value <" + type + "> is invalid. " +
                            "It can only be set to 0 or 1. Use 0 for an instruction, 1 for a declaration.");
                    }
                }
            }

            if ((CodeType)type == CodeType.Declaration)
            {
                // Added as a Comment with an empty text, not a CodeLine: a declaration-type
                // CodeLine is written into the separate "User defined code lines" section, whereas
                // an (empty) declaration-type Comment is written straight into the same
                // declarations list as the RAPID Variable/Robot Target/etc. declarations
                // themselves, in insertion order — so the blank line stays interleaved exactly
                // where it was placed relative to those declarations.
                DA.SetData(0, new Comment("", CodeType.Declaration));
            }
            else
            {
                DA.SetData(0, new CodeLine("    ", CodeType.Instruction));
            }
        }

        #region menu items
        /// <summary>
        /// Appends "Add Type Input" toggle to the right-click context menu.
        /// </summary>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Add Type Input", MenuItemClickTypeInput, true, _typeInputParam);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void MenuItemClickTypeInput(object sender, EventArgs e)
        {
            RecordUndoEvent("Add Type Input");
            _typeInputParam = !_typeInputParam;
            ToggleTypeParam();
        }

        /// <summary>
        /// Adds or removes the optional Type input parameter.
        /// </summary>
        private void ToggleTypeParam()
        {
            if (_typeInputParam)
            {
                Params.RegisterInputParam(new Param_Integer
                {
                    Name        = _typeParamName,
                    NickName    = "T",
                    Description = "Type of the empty line. Use 0 for adding it as an instruction, 1 for adding it as a declaration.",
                    Access      = GH_ParamAccess.item,
                    Optional    = true
                });
            }
            else
            {
                var typeParam = Params.Input.FirstOrDefault(x => x.Name == _typeParamName);
                if (typeParam != null)
                    Params.UnregisterInputParameter(typeParam, true);
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        #endregion

        #region serialization
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("TypeInputParam", _typeInputParam);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("TypeInputParam"))
                _typeInputParam = reader.GetBoolean("TypeInputParam");
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
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
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
            get { return Properties.Resources.EmptyLine_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2BDB6D70-99CB-4509-BCEB-67031D588450"); }
        }
        #endregion
    }
}
