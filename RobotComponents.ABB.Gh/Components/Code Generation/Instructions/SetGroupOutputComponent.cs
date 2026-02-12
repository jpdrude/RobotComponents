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
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using RobotComponents.ABB.Actions.Declarations;

// RobotComponents Libs
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Gh.Parameters.Actions.Instructions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Action : Set Analog Output component.
    /// </summary>
    public class SetGroupOutputComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private const int staticInputCount = 2;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public SetGroupOutputComponent() : base("Set Group Output", "SGO", "Code Generation",
                "Defines an instruction to change the state of a group output of the robot controller.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of the group output signal as text", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Value", "V", "Value of the group output signal as integer", GH_ParamAccess.item, 0);
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
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_SetGroupOutput(), "Set Group Output", "SGO", "Resulting Set Group Output instruction");
            pManager.RegisterParam(new Param_Integer(), "Value", "V", "Resulting Group Output Value", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            string name = "";
            int value = 0;
            List<int> bitValues = new List<int>();
            bool bitInput = false;

            // Catch the input data
            if (!DA.GetData(0, ref name)) { return; }
            if (!DA.GetData(1, ref value)) { return; }

            // Check name
            name = HelperMethods.ReplaceSpacesAndRemoveNewLines(name);

            if (HelperMethods.StringExeedsCharacterLimit32(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Group output name exceeds character limit of 32 characters.");
            }
            if (HelperMethods.StringStartsWithNumber(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Group output name starts with a number which is not allowed in RAPID code.");
            }
            if (HelperMethods.StringStartsWithNumber(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Group output name constains special characters which is not allowed in RAPID code.");
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

            // Create the action
            SetGroupOutput groupOutput = new SetGroupOutput(name, value);

            // Output
            DA.SetData(0, groupOutput);
            DA.SetData(1, value);
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
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
            get { return RobotComponents.ABB.Gh.Properties.Resources.GroupOutput_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F7A3E2D1-8B4C-4F6E-9A1D-3C8E5B2A7F90"); }
        }
        #endregion
    }

}
