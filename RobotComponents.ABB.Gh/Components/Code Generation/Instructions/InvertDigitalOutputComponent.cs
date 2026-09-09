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
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Gh.Parameters.Actions.Instructions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Action : Invert Digital Output component.
    /// </summary>
    public class InvertDigitalOutputComponent : GH_RobotComponent
    {
        public InvertDigitalOutputComponent() : base("Invert Digital Output", "IDO", "Code Generation",
              "Defines an instruction to invert (toggle) the state of a digital output of the robot controller.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of the digital output as text", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_InvertDigitalOutput(), "Invert Digital Output", "IDO", "Resulting Invert Digital Output instruction");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";

            if (!DA.GetData(0, ref name)) { return; }

            name = HelperMethods.ReplaceSpacesAndRemoveNewLines(name);
            if (HelperMethods.StringExeedsCharacterLimit32(name))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Digital output name exceeds character limit of 32 characters.");
            if (HelperMethods.StringStartsWithNumber(name))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Digital output name starts with a number which is not allowed in RAPID code.");
            if (HelperMethods.StringHasSpecialCharacters(name))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Digital output name contains special characters which is not allowed in RAPID code.");

            DA.SetData(0, new InvertDigitalOutput(name));
        }

        #region properties
        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }
        public override bool Obsolete { get { return false; } }
        protected override System.Drawing.Bitmap Icon { get { return RobotComponents.ABB.Gh.Properties.Resources.InvertDigitalOutput_Icon; } }
        public override Guid ComponentGuid { get { return new Guid("453DB3A2-710A-46C3-AAFD-AA638A4BCA3D"); } }
        #endregion
    }
}
