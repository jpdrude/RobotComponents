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
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Gh.Parameters.Actions.Declarations;
using RobotComponents.ABB.Controllers;
using RobotComponents.ABB.Gh.Goos.Controllers;
using Grasshopper.Kernel.Types;

namespace RobotComponents.ABB.Gh.Components.Deconstruct.CodeGeneration
{
    /// <summary>
    /// RobotComponents Deconstruct GroupSignal component.
    /// </summary>
    public class DeconstructGroupSignalComponent : GH_RobotComponent
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructZoneData class.
        /// </summary>
        public DeconstructGroupSignalComponent() : base("Deconstruct GroupSignal", "DeGS", "Deconstruct",
              "Deconstructs a Group Signal component into its bit parameters.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Group Signal", "GS", "Group Signal to be deconstructed. Integer value also accepted.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Signal Bits", "SB", "Bit values of the Group Signal", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            List<int> signalBits = new List<int>();
            object rawSignal = new object();
            int intSignal = 0;

            // Catch the input data
            if (!DA.GetData(0, ref rawSignal)) { return; }
            
            if (rawSignal is GH_Signal groupSignal)
            {               
                intSignal = (int)groupSignal.Value.Value;
            }
            else if (rawSignal is GH_Integer intValue)
            {
                intSignal = intValue.Value;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input must be a Group Signal or an integer value.");
                return;
            }

            for (int i = 0; i < 32; i++)
            {
                int bitValue = (intSignal >> i) & 1;
                signalBits.Add(bitValue);
            }

            DA.SetDataList(0, signalBits);
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary, dropdown and obscure
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
        /// Provides an Icon for the component
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.DeconstructGroupSignal_Icon; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F7E3A1B2-9C4D-4E5F-8A6B-3C2D1E0F9A8B"); }
        }
        #endregion
    }
}