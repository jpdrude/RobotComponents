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
using Eto.Forms;
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
using System.Xml.Linq;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Message Box Component.
    /// </summary>
    public class MessageBoxComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private bool _expire = false;
        private const int _staticInputCount = 3;
        List<string> _buttonNames = new List<string>();
        int _numButtons = 0;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public MessageBoxComponent() : base("Message Box", "MB", "Code Generation", 
            "Calls a message box. Different buttons call different code segments.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Header of the message box.", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "M", "Message to be shown in the message box.", GH_ParamAccess.list);
            pManager.AddTextParameter("Buttons", "B", "Buttons to be shown in the message box. Each button will generate input to execute a set of actions specific to this button press.", GH_ParamAccess.list);
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
            return false;
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
            return false;
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
        /// Creates a new Code Line parameter To specify actions executen on a button click at the specified side and index.
        /// </summary>
        /// <param name="side">The parameter side (input or output) where the new parameter will be added.</param>
        /// <param name="index">The zero-based index indicating the position of the parameter on the specified side.</param>
        /// <returns>An <see cref="IGH_Param"/> representing a string argument at the given side and index.</returns>
        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var param = new Param_Action
            {
                Name = $"Actions {_buttonNames[index - _staticInputCount]}",
                NickName = $"btn_{index - _staticInputCount + 1}",
                Description = "Actions to execute when this button is clicked.",
                Access = GH_ParamAccess.list,
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
            for (int i = _staticInputCount; i < Params.Input.Count; i++)
            {
                if (i >= _buttonNames.Count) return;

                Params.Input[i].Name = $"Actions {_buttonNames[i - _staticInputCount]}";
                Params.Input[i].NickName = $"btn_{i - _staticInputCount + 1}";
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Action(), "Message Box", "MB", "Code to prompt message box and react to user input.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _numButtons = Params.Input.Count - _staticInputCount;

            //Input variables
            string header = string.Empty;
            List<string> message = new List<string>();
            List<string> buttons = new List<string>();
            List<List<IAction>> btnActions = new List<List<IAction>>();

            // Catch the input data
            if (!DA.GetData(0, ref header)) { return; }
            if (!DA.GetDataList(1, message)) { return; }
            if (!DA.GetDataList(2, buttons)) { return; }

            for (int i = _staticInputCount; i < Params.Input.Count; i++)
            {
                List<IAction> list = new List<IAction>();
                if (DA.GetDataList(i, list))
                {
                    btnActions.Add(list);
                }
            }

            //Generate/Mainain button-specific inputs
            _buttonNames = buttons;
            if (buttons.Count > _numButtons)
            {
                for (int i = _numButtons; i < buttons.Count; ++i)
                {
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, i + _staticInputCount));
                }

                Params.OnParametersChanged();
                VariableParameterMaintenance();
                _numButtons = buttons.Count;
                _expire = true;
                return;
            }
            else if (buttons.Count < _numButtons)
            {
                UpdateVariableParameters();

                _expire = true;
                return;
            }

            if (_expire)
            {
                _expire = false;
                ExpireSolution(true);
                return;
            }

            //Generate Code Lines
            List<IAction> msgBoxCode = new List<IAction>();
            string msgArray = $"\"{message[0]}\"";
            string btnArray = $"\"{buttons[0]}\"";

            for (int i = 1; i < message.Count; ++i)
                msgArray += $", \"{message[i]}\"";

            for (int i = 1; i < buttons.Count; ++i)
                btnArray += $", \"{buttons[i]}\"";

            msgBoxCode.Add(new CodeLine("LOCAL VAR btnRes msgBoxAnswer;", CodeType.Declaration));
            msgBoxCode.Add(new CodeLine($"msgBoxAnswer := UIMessageBox(\\Header:=\"{header}\" \\MsgArray:=[{msgArray}] \\BtnArray:=[{btnArray}]);"));

            int answerCounter = 1;
            foreach (List<IAction> btnCode in btnActions)
            {
                msgBoxCode.Add(new CodeLine($"IF msgBoxAnswer = {answerCounter} THEN", CodeType.Instruction));
                foreach (IAction action in btnCode)
                {
                    msgBoxCode.Add(action);
                }
                msgBoxCode.Add(new CodeLine("ENDIF", CodeType.Instruction));

                ++answerCounter;
            }

            //Set Output
            DA.SetDataList(0, msgBoxCode);
        }

        /// <summary>
        /// Updates the set of variable input parameters. Interrupts (Traps) do not support arguments.
        /// </summary>
        /// <remarks>This method schedules the update operation and triggers parameter change
        /// notifications. If the document context is unavailable, no update occurs.</remarks>
        private void UpdateVariableParameters()
        {
            var doc = OnPingDocument();
            if (doc == null) return;
            _numButtons = _buttonNames.Count;

            doc.ScheduleSolution(5, d =>
            {
                while (Params.Input.Count > _numButtons + _staticInputCount)
                {
                    Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], true);
                }

                VariableParameterMaintenance();
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
            get { return Properties.Resources.MessageBox_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A7B2D4E8-9F3C-4A1B-8E6D-2C5F8B3A9D7E"); }
        }
        #endregion
    }
}
