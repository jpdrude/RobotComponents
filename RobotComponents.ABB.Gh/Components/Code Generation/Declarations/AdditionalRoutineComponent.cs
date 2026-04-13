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
// Grasshopper Libs
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
// RobotComponents
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Actions.Dynamic;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _isFunc = false;
        private bool _isTrap = false;
        private bool _previousIsTrap = false;
        private const int staticInputCount = 4;
        private const string ReturnTypeParamName = "Return Type";
        private const string ReturnValueOutputName = "Return Value";
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public AdditionalRoutineComponent() : base("Routine", "R", "Code Generation",
            "Defines manually a PROC, FUNC or TRAP declaration.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Action(), "Actions", "A", "Actions as list of instructive Actions. Declarations will be added to the module scope.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Type", "T", "Type of the routine. Use 0 for adding a PROC. Use 1 for adding a FUNC. Use 2 for adding a TRAP.", GH_ParamAccess.item, 0);
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
            int floor = staticInputCount + (_isFunc ? 1 : 0);
            return side == GH_ParameterSide.Input && index >= floor;
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
            int floor = staticInputCount + (_isFunc ? 1 : 0);
            return side == GH_ParameterSide.Input && index >= floor && Params.Input.Count > floor;
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
            int floor = staticInputCount + (_isFunc ? 1 : 0);
            int varIndex = index - floor;
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
            int floor = staticInputCount + (_isFunc ? 1 : 0);
            for (int i = floor; i < Params.Input.Count; i++)
            {
                Params.Input[i].Name = $"Argument {i - floor + 1}";
                Params.Input[i].NickName = $"Arg{i - floor + 1}";
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
                OnPingDocument()?.ScheduleSolution(5, d => ExpireSolution(true));
                return;
            }

            // Reconcile the Return Type parameter against the actual desired state on every solve.
            // We compare wantsFunc to whether RT actually exists in the layout — not to a stored
            // flag — so the component self-corrects regardless of how any inconsistency arose
            // (type switch, file load, undo, etc.).
            // DA was initialised with the current layout, so we must not read from the RT slot
            // in the same solve where we add/remove it; bail out and let the scheduled solution
            // run with the corrected layout.
            int typeProbe = 0;
            DA.GetData(1, ref typeProbe);
            bool wantsFunc = typeProbe == (int)RoutineType.FUNC;
            bool hasReturnTypeParam = Params.Input.Any(p => p.Name == ReturnTypeParamName);
            bool hasReturnValueOutput = Params.Output.Any(p => p.Name == ReturnValueOutputName);

            if (wantsFunc != hasReturnTypeParam || wantsFunc != hasReturnValueOutput)
            {
                _isFunc = wantsFunc;
                bool capturedIsFunc = wantsFunc;

                OnPingDocument()?.ScheduleSolution(5, d =>
                {
                    // Reconcile Return Type input
                    IGH_Param existingReturnType = Params.Input.FirstOrDefault(p => p.Name == ReturnTypeParamName);
                    if (capturedIsFunc && existingReturnType == null)
                    {
                        Params.RegisterInputParam(new Param_String()
                        {
                            Name = ReturnTypeParamName,
                            NickName = "RT",
                            Description = "Return type of the function. E.g. num, bool, etc.",
                            Access = GH_ParamAccess.item
                        }, staticInputCount);
                    }
                    else if (!capturedIsFunc && existingReturnType != null)
                    {
                        Params.UnregisterInputParameter(existingReturnType, true);
                    }

                    // Reconcile Return Value output
                    IGH_Param existingReturnValue = Params.Output.FirstOrDefault(p => p.Name == ReturnValueOutputName);
                    if (capturedIsFunc && existingReturnValue == null)
                    {
                        Params.RegisterOutputParam(new Param_RAPIDExpression()
                        {
                            Name = ReturnValueOutputName,
                            NickName = "RV",
                            Description = "The FUNC call as a RAPID expression, usable as a value in other instructions.",
                            Access = GH_ParamAccess.item
                        });
                    }
                    else if (!capturedIsFunc && existingReturnValue != null)
                    {
                        Params.UnregisterOutputParameter(existingReturnValue, true);
                    }

                    Params.OnParametersChanged();
                });
                return;
            }

            _isFunc = wantsFunc;

            // Input variables
            List<IAction> actions = new List<IAction>();
            int type = 0;
            string name = "";
            int scope = 0;
            string returnType = null;
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
                    "It can only be set to 0, 1, or 2. Use 0 for PROC, 1 for FUNC and 2 for TRAP.");
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

            // Read the Return Type if FUNC. The Return Type slot lives at staticInputCount; the
            // FUNC param-state branch above guarantees the param exists when _isFunc is true.
            int variableArgsStart = staticInputCount + (_isFunc ? 1 : 0);
            if (_isFunc)
            {
                DA.GetData(staticInputCount, ref returnType);
            }

            if (!_isTrap)
            {
                // Get routine arguments from variable input parameters
                for (int i = variableArgsStart; i < Params.Input.Count; i++)
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
            Routine method;
            if (_isFunc)
                method = new Routine(actions, (RoutineType)type, returnType, name, (Scope)scope, arguments);
            else
                method = new Routine(actions, (RoutineType)type, name, (Scope)scope, arguments);

            //Generates Routine Call if PROC or FUNC
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
            else if (method.Type == RoutineType.FUNC)
            {
                string call = method.Name + "(";

                if (arguments != null && arguments.Count > 0)
                {
                    List<string> argValues = new List<string>();
                    foreach (RoutineArgument arg in arguments)
                    {
                        argValues.Add(arg.ToCallString());
                    }

                    call += string.Join(", ", argValues);
                }

                call += ");";

                routineCall.Add(new CodeLine(call, CodeType.Instruction));
            }

            // Sets Output
            DA.SetData(0, method);
            DA.SetDataList(1, routineCall);

            // For FUNC routines, output the call expression so it can be wired into value inputs
            if (_isFunc && Params.Output.Count > 2)
            {
                string argList = arguments.Count > 0 ? string.Join(", ", arguments.Select(a => a.ToCallString())) : "";
                DA.SetData(2, new GH_RAPIDExpression(new RAPIDExpression($"{method.Name}({argList})")));
            }

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

        #region serialization
        /// <summary>
        /// Serializes the component state. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="writer"> Provides access to a subset of GH_Chunk methods used for writing archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("IsFunc", _isFunc);
            return base.Write(writer);
        }

        /// <summary>
        /// Deserializes the component state. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="reader"> Provides access to a subset of GH_Chunk methods used for reading archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Read(GH_IReader reader)
        {
            _isFunc = reader.GetBoolean("IsFunc");
            return base.Read(reader);
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
