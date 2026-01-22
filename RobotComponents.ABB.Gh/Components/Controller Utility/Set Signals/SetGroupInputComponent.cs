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
using Grasshopper.Kernel.Special;
// Robot Components Libs
using RobotComponents.ABB.Controllers;
using RobotComponents.ABB.Controllers.Forms;
using RobotComponents.ABB.Gh.Parameters.Controllers;
using RobotComponents.ABB.Gh.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RobotComponents.ABB.Gh.Components.ControllerUtility
{
    /// <summary>
    /// Represents the component that gets and sets group inputs on a defined controller.
    /// </summary>
    public class SetGroupInputComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private Controller _controller;
        private const int staticInputCount = 4;
        #endregion

        /// <summary>
        /// Initializes a new instance of the SetGroupIntputComponent class.
        /// </summary>
        public SetGroupInputComponent() : base("Set Group Input", "SetGI", "Controller Utility",
              "Changes the state of a defined group input from an ABB controller in realtime."
                + System.Environment.NewLine + System.Environment.NewLine +
                "This component uses the ABB PC SDK.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Controller", "C", "Controller to connected to as Controller", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Name of the Group Intput Signal as text", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Value", "V", "State of the Group Input as a number", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Update", "U", "Updates the Group Input as bool", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Signal(), "Signal", "S", "Group Input Signal", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Value", "V", "Group Input Signal Value", GH_ParamAccess.item);
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
            var param = new Param_Boolean()
            {
                Name = $"bit{varIndex}",
                NickName = $"bit{varIndex}",
                Description = "BitMask bit",
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
                Params.Input[i].Name = $"bit{i - staticInputCount}";
                Params.Input[i].NickName = $"bit{i - staticInputCount}";
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Check the operating system
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "This component is only supported on Windows operating systems.");
                return;
            }

            // Input variables      
            string name = "";
            int value = 0;
            bool update = false;
            List<int> bitValues = new List<int>();
            bool bitInput = false;

            // Catch input data
            if (!DA.GetData(0, ref _controller)) { return; }
            if (!DA.GetData(1, ref name)) { return; }
            if (!DA.GetData(2, ref value)) { return; }
            if (!DA.GetData(3, ref update)) { return; }

            // Define an empty signal
            Signal signal = new Signal();

            // Get the signal
            try
            {
                signal = _controller.GetGroupInput(name, out int index);

                if (index == -1)
                {
                    if (_controller.IsEmpty == true)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Could not get the signal {name}. The controller is empty.");
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Could not get the signal {name}. Signal not found.");
                    }
                }
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }

            // Gets bitmask values from variable input parameters
            for (int i = staticInputCount; i < Params.Input.Count; i++)
            {
                bool bitValue = false;
                if (DA.GetData(i, ref bitValue))
                {
                    bitInput = true;
                    if (bitValue)
                        bitValues.Add(1 << (i - staticInputCount));
                    else
                        bitValues.Add(0);
                }
                else
                {
                    bitValues.Add(0);
                }
            }

            if (bitInput)
            {
                value = 0;

                foreach (int bit in bitValues)
                {
                    value = value | bit;
                }
            }


            // Update the signal
            if (update == true)
            {
                bool success = signal.SetValue(Convert.ToSingle(value), out string msg);

                if (success == false)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                }
            }

            // Output
            DA.SetData(0, signal);
            DA.SetData(1, value);
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.senary | GH_Exposure.obscure; }
        }

        /// <summary>
        /// Gets whether this object is obsolete.
        /// </summary>
        public override bool Obsolete
        {
            get { return false; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.SetGroupInput_Icon; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A4D8F6E2-9B3C-4E7A-81D5-6F2A9C1E3B7D"); }
        }
        #endregion

        #region menu-items
        /// <summary>
        /// Adds the additional item "Pick Signal" to the context menu of the component. 
        /// </summary>
        /// <param name="menu"> The context menu of the component. </param>
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Pick Signal", MenuItemClick);

            base.AppendAdditionalComponentMenuItems(menu);
        }

        /// <summary>
        /// Registers the event when the custom menu item is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClick(object sender, EventArgs e)
        {
            if (this.GetSignal(out string name) == true)
            {
                if (this.Params.Input[1].Sources.Count == 1 && this.Params.Input[1].Sources[0] is GH_Panel panel)
                {
                    panel.SetUserText(name);
                }
                else
                {
                    HelperMethods.CreatePanel(this, name, 1);
                }

                this.ExpireSolution(true);
            }
        }
        #endregion

        #region addtional methods
        /// <summary>
        /// Get the signal
        /// </summary>
        /// <returns> Indicates whether or not the signal was picked successfully. </returns>
        private bool GetSignal(out string name)
        {
            List<Signal> signals = _controller.GroupInputs;
            name = "";

            if (signals.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No group input signals found!");
                return false;
            }

            else if (signals.Count == 1)
            {
                name = signals[0].Name;
                return true;
            }

            else if (signals.Count > 1)
            {
                PickSignalForm form = new PickSignalForm(signals);
                bool result = form.ShowModal(Grasshopper.Instances.EtoDocumentEditor);

                if (result)
                {
                    name = form.Signal.Name;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
        #endregion
    }
}