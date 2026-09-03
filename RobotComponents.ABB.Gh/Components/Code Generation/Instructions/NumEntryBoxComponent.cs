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
using System.Globalization;
using System.Text;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Definitions;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Numeric Entry Box Component.
    /// Assigns the result of UINumEntry to a user-supplied RAPID variable.
    ///
    /// Generated code example:
    ///   answer := UINumEntry(\Header:="Enter value" \Message:="How many?" \InitValue:=5 \MinValue:=1 \MaxValue:=10 \AsInteger);
    /// </summary>
    public class NumEntryBoxComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public NumEntryBoxComponent() : base("Numeric Entry Box", "NEB", "Advanced RAPID Features",
            "Calls UINumEntry to prompt the user for a numeric input during robot program execution. " +
            "Assigns the result to the provided RAPID variable.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_RAPIDVariable(), "Variable", "V",
                "RAPID variable that receives the UINumEntry result. Connect a RAPID Variable component.",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Header", "H",
                "Header text shown at the top of the dialog.",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "M",
                "Message text. Connect a list for multiple lines (generates a RAPID MsgArray).",
                GH_ParamAccess.list);
            pManager.AddNumberParameter("Initial Value", "IV",
                "Initial value shown in the entry box (\\InitValue).",
                GH_ParamAccess.item);
            pManager.AddIntervalParameter("Range", "R",
                "Valid input range. T0 = minimum (\\MinValue), T1 = maximum (\\MaxValue).",
                GH_ParamAccess.item);
            pManager.AddBooleanParameter("As Integer", "AI",
                "When true, only integer values are accepted (\\AsInteger switch).",
                GH_ParamAccess.item, false);

            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RAPIDVariable(), "Variable", "V",
                "Pass-through of the input RAPID variable.",
                GH_ParamAccess.item);
            pManager.RegisterParam(new Param_Action(), "Numeric Entry Box", "NEB",
                "UINumEntry assignment as a RAPID code line.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RAPIDVariable variable = null;
            string header   = string.Empty;
            var    message  = new List<string>();
            double initVal  = 0.0;
            bool   hasInit  = false;
            var    range    = new Rhino.Geometry.Interval();
            bool   hasRange = false;
            bool   asInt    = false;

            if (!DA.GetData(0, ref variable))   { return; }
            if (!DA.GetData(1, ref header))     { return; }
            if (!DA.GetDataList(2, message))    { return; }
            hasInit  = DA.GetData(3, ref initVal);
            hasRange = DA.GetData(4, ref range);
            DA.GetData(5, ref asInt);

            if (string.IsNullOrWhiteSpace(variable.Name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Variable does not have a name.");
                return;
            }

            // --- Validate range ---
            if (hasRange && range.T0 > range.T1)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Range minimum (T0) is greater than maximum (T1).");

            if (hasRange && hasInit && (initVal < range.T0 || initVal > range.T1))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Initial value ({initVal}) is outside the specified range [{range.T0}, {range.T1}].");

            // --- Build UINumEntry assignment ---
            var sb = new StringBuilder();
            sb.Append($"{variable.Name} := UINumEntry(");
            sb.Append($"\\Header:=\"{header}\"");

            if (message.Count == 1)
            {
                sb.Append($" \\Message:=\"{message[0]}\"");
            }
            else if (message.Count > 1)
            {
                var quoted = new List<string>();
                foreach (string line in message)
                    quoted.Add($"\"{line}\"");
                sb.Append($" \\MsgArray:=[{string.Join(", ", quoted)}]");
            }

            if (hasInit)
                sb.Append($" \\InitValue:={initVal.ToString("0.######", CultureInfo.InvariantCulture)}");

            if (hasRange)
            {
                sb.Append($" \\MinValue:={range.T0.ToString("0.######", CultureInfo.InvariantCulture)}");
                sb.Append($" \\MaxValue:={range.T1.ToString("0.######", CultureInfo.InvariantCulture)}");
            }

            if (asInt)
                sb.Append(" \\AsInteger");

            sb.Append(");");

            DA.SetData(0, variable);
            DA.SetData(1, new CodeLine(sb.ToString(), CodeType.Instruction));
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
            get { return Properties.Resources.NumEntryBox_Icon; }
        }


        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E7B3C9F2-4A18-4D3B-A8E6-1C5D2F7B4E93"); }
        }
        #endregion
    }
}
