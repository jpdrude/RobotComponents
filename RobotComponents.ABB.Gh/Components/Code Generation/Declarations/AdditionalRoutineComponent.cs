// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2025 EDEK Uni Kassel
//
// Author:
//   - Jan Philipp Drude (2025)
//
// For license details, see the LICENSE file in the project root.

// System Libs
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
// RobotComponents
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Actions.Dynamic;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Routine Declaration Component.
    /// </summary>
    public class AdditionalRoutineComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private bool _expire = false;
        private bool _isTrap = false;
        private bool _previousIsTrap = false;
        private const int staticInputCount = 4;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public AdditionalRoutineComponent() : base("Routine", "R", "Code Generation", 
            "Defines manually a PROC or TRAP declaration.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Action(), "Actions", "A", "Actions as list of instructive Actions. Declarations will be added to the module scope.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Type", "T", "Type of the routine. Use 0 for adding a PROC. Use 1 for adding a TRAP.", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Name", "N", "Routine Identifier", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Scope", "S", "Scope of the routine. Use 0 for GLOBAL scope, 1 for LOCAL scope and 2 for TASK scope.", GH_ParamAccess.item, 0);

            pManager[1].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Determines whether a parameter can be inserted at the specified index on the given side.
        /// </summary>
        /// <remarks>Returns <see langword="false"/> if the component is in a trap state or if the
        /// specified side is not <see cref="GH_ParameterSide.Input"/>.</remarks>
        /// <param name="side">The side of the component to check for parameter insertion. Only <see cref="GH_ParameterSide.Input"/> is
        /// supported.</param>
        /// <param name="index">The zero-based index at which to check for parameter insertion.</param>
        /// <returns><see langword="true"/> if a parameter can be inserted at the specified index on the input side; otherwise,
        /// <see langword="false"/>.</returns>
        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (_isTrap) return false;
            return side == GH_ParameterSide.Input && index >= staticInputCount;
        }

        /// <summary>
        /// Determines whether a parameter at the specified index can be removed from the given side of the component.
        /// </summary>
        /// <remarks>A parameter can only be removed from the input side if there is more than one input
        /// parameter and the component is not in a trap state.</remarks>
        /// <param name="side">The side of the component (<see cref="GH_ParameterSide"/>) from which to attempt removal. Only <see
        /// cref="GH_ParameterSide.Input"/> is supported.</param>
        /// <param name="index">The zero-based index of the parameter to remove.</param>
        /// <returns><see langword="true"/> if the parameter can be removed from the specified side and index; otherwise, <see
        /// langword="false"/>.</returns>
        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (_isTrap) return false;
            return side == GH_ParameterSide.Input && index >= staticInputCount && Params.Input.Count > staticInputCount;
        }

        /// <summary>
        /// Removes the parameter at the specified index from the given side of the component.
        /// </summary>
        /// <param name="side">The side of the component from which to remove the parameter. </param>
        /// <param name="index">The zero-based index of the parameter to remove on the specified side.</param>
        /// <returns><see langword="true"/> if the parameter was successfully removed; otherwise, <see langword="false"/>.</returns>
        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        /// <summary>
        /// Creates a new string parameter for use as a routine argument at the specified side and index.
        /// </summary>
        /// <param name="side">The parameter side (input or output) where the new parameter will be added.</param>
        /// <param name="index">The zero-based index indicating the position of the parameter on the specified side.</param>
        /// <returns>An <see cref="IGH_Param"/> representing a string argument at the given side and index.</returns>
        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            int varIndex = index - staticInputCount;
            var param = new Param_RoutineArgument()
            {
                Name = $"Argument {varIndex + 1}",
                NickName = $"Arg{varIndex + 1}",
                Description = "Routine Argument",
                Access = GH_ParamAccess.item,
                Optional = true
            };
            return param;
        }

        /// <summary>
        /// Renames input parameters to maintain consistent naming after parameters are added or removed.
        /// </summary>
        /// <remarks>This method updates the <c>Name</c> and <c>NickName</c> properties of each input
        /// parameter in the <c>Params.Input</c> collection, assigning sequential names based on their current order.
        /// Call this method after modifying the input parameter list to ensure that parameter names remain clear and
        /// predictable for users and downstream consumers.</remarks>
        public void VariableParameterMaintenance()
        {
            for (int i = staticInputCount; i < Params.Input.Count; i++)
            {
                Params.Input[i].Name = $"Argument {i - staticInputCount + 1}";
                Params.Input[i].NickName = $"Arg{i - staticInputCount + 1}";
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Routine(), "Routine", "R", "Resulting Routine", GH_ParamAccess.item);
            pManager.RegisterParam(new Param_CodeLine(), "Routine Call", "C", "Call to routine to enter into RAPID Code", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Creates the input value list and attachs it to the input parameter
            if (this.Params.Input[1].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, typeof(RoutineType), 1);
            }
            if (this.Params.Input[3].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, typeof(Scope), 3);
            }

            // Expire solution of this component
            if (_expire == true)
            {
                _expire = false;
                this.ExpireSolution(true);
                return;
            }

            // Input variables
            List<IAction> actions = new List<IAction>();
            int type = 0;
            string name = "";
            int scope = 0;
            List<RoutineArgument> arguments = new List<RoutineArgument>();

            // Catch the input data
            if (!DA.GetDataList(0, actions)) { return; }
            if (!DA.GetData(1, ref type)) { return; }
            if (!DA.GetData(2, ref name)) { return; }
            if (!DA.GetData(3, ref scope)) { return; }

            // Check if a right value is used for the method type and scope
            if (type < (int)RoutineType.PROC || type > (int)RoutineType.TRAP)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Routine type <" + type + "> is invalid. " +
                    "It can only be set to 0 or 1. Use 0 for PROC and 1 for TRAP. Other routine types as well as routines with arguments are not supported.");
            }
            if (scope < (int)Scope.GLOBAL || scope > (int)Scope.TASK)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Routine scope <" + scope + "> is invalid. " +
                    "It can only be set to 0, 1, or 2. Use 0 for GLOBAL scope, 1 for LOCAL scope and 2 for TASK scope.");
            }

            // Check if routine is a TRAP and remove variable parameters
            _isTrap = type == (int)RoutineType.TRAP;
            if (_isTrap != _previousIsTrap)
            {
                _previousIsTrap = _isTrap;

                UpdateVariableParameters(_isTrap);
            }

            if (!_isTrap)
            {
                // Get routine arguments from variable input parameters
                for (int i = staticInputCount; i < Params.Input.Count; i++)
                {
                    RoutineArgument arg = null;
                    if (DA.GetData(i, ref arg))
                    {
                        arguments.Add(arg);
                    }
                }
            }

            // Checks if routine name starts with a number
            if (HelperMethods.StringStartsWithNumber(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The routine name starts with a number which is not allowed in RAPID code.");
            }
            if (HelperMethods.StringExeedsCharacterLimit32(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The routine name exceeds the character limit of 32 characters.");
            }

            //Generates Output
            Routine method = new Routine(actions, (RoutineType)type, name, (Scope)scope, arguments);

            //Generates Routine Call if PROC
            List<CodeLine> routineCall = new List<CodeLine>();
            if (method.Type == RoutineType.PROC)
            {
                string call = method.Name;

                if (arguments != null && arguments.Count > 0)
                {
                    List <string> argValues = new List<string>();
                    foreach (RoutineArgument arg in arguments)
                    {
                        argValues.Add(arg.ToCallString());
                    }

                    call += " " + string.Join(", ", argValues);
                }

                call += ";";

                routineCall.Add(new CodeLine(call, CodeType.Instruction));
            }

            // Sets Output
            DA.SetData(0, method);
            DA.SetDataList(1, routineCall);

            if (method.Type == RoutineType.TRAP)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "TRAP routines need to be connected using custom code and interrupt logic.");
        }

        /// <summary>
        /// Updates the set of variable input parameters. Interrupts (Traps) do not support arguments.
        /// </summary>
        /// <remarks>This method schedules the update operation and triggers parameter change
        /// notifications. If the document context is unavailable, no update occurs.</remarks>
        /// <param name="isTrap">Indicates whether the routine type is an interrupt and deletes argument inputs accordingly.
        /// If <see langword="true"/>, input parameters beyond the static count are unregistered.</param>
        private void UpdateVariableParameters(bool isTrap)
        {
            var doc = OnPingDocument();
            if (doc == null) return;

            doc.ScheduleSolution(5, d =>
            {
                if (isTrap)
                {
                    while (Params.Input.Count > staticInputCount)
                    {
                        Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], true);
                    }
                }

                Params.OnParametersChanged();
            });
        }

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
            get { return Properties.Resources.Routine_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4A1C9E7B-3F82-4D56-B0A3-8E2D6C4F5A91"); }
        }
        #endregion
    }
}
