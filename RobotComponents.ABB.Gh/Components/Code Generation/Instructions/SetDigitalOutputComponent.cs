// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2018-2020 EDEK Uni Kassel
// Copyright (c) 2020-2026 Arjen Deetman
//
// Authors:
//   - Gabriel Rumph (2018-2020)
//   - Benedikt Wannemacher (2018-2020)
//   - Arjen Deetman (2019-2026)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions.Instructions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Action : Set Digital Output component.
    /// </summary>
    public class SetDigitalOutputComponent : GH_RobotComponent
    {
        public SetDigitalOutputComponent() : base("Set Digital Output", "SDO", "Code Generation",
              "Defines an instruction to change the state of a digital output of the robot controller.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of the digital output as text", GH_ParamAccess.item);
            pManager.AddParameter(new Param_RAPIDExpression(), "State", "S",
                "State of the digital output. Accepts a boolean (0/1), integer, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_SetDigitalOutput(), "Set Digital Output", "SDO", "Resulting Set Digital Output instruction");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            GH_RAPIDExpression valueExpr = null;

            if (!DA.GetData(0, ref name)) { return; }
            if (!DA.GetData(1, ref valueExpr)) { return; }

            name = HelperMethods.ReplaceSpacesAndRemoveNewLines(name);
            if (HelperMethods.StringExeedsCharacterLimit32(name))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Digital output name exceeds character limit of 32 characters.");
            if (HelperMethods.StringStartsWithNumber(name))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Digital output name starts with a number which is not allowed in RAPID code.");
            if (HelperMethods.StringHasSpecialCharacters(name))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Digital output name contains special characters which is not allowed in RAPID code.");

            string value = valueExpr?.Value?.Expression ?? "0";
            if (!RAPIDExpression.IsValidExpression(value))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"State value \"{value}\" does not appear to be a valid RAPID expression.");

            DA.SetData(0, new SetDigitalOutput(name, value));
        }

        #region properties
        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }
        public override bool Obsolete { get { return false; } }
        protected override System.Drawing.Bitmap Icon { get { return RobotComponents.ABB.Gh.Properties.Resources.DigitalOutput_Icon; } }
        public override Guid ComponentGuid { get { return new Guid("75014DB2-351F-4A79-9917-DEA7DAD89BEF"); } }
        #endregion
    }
}
