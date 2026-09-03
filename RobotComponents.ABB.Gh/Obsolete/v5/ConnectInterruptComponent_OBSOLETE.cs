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

#pragma warning disable CS1591 // Missing XML comment — obsolete shim, kept for .ghx backwards compatibility.

// System Libs
using System;
using System.Collections.Generic;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Components;
using RobotComponents.ABB.Gh.Parameters.Actions.Dynamic;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Obsolete
{
    /// <summary>
    /// RobotComponents Action : Connect Interrupt Component.
    /// </summary>
    /// <remarks>
    /// Hidden from the menu since the Enable/Disable Interrupts outputs were added. Retained so
    /// older .gh files that placed this component before that change continue to load and resolve
    /// their saved param GUIDs unchanged; the live component now has a new GUID for its new
    /// output shape.
    /// </remarks>
    [Obsolete("This component is OBSOLETE and will be removed in the future. Use Connect Interrupt instead.", false)]
    public class ConnectInterruptComponent_OBSOLETE : GH_RobotComponent
    {
        #region fields
        private bool _expire = false;
        #endregion

        public ConnectInterruptComponent_OBSOLETE() : base("Connect Interrupt", "CI", "Advanced RAPID Features",
              "Connects a TRAP routine to a signal change.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TRAP Routine Name", "TR", "Name of the TRAP routine to be called when the signal change occurs.", GH_ParamAccess.item);
            pManager.AddTextParameter("Signal Name", "SN", "Name of the signal that is monitored for changes.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Signal Value", "SV", "Value of the signal that triggers the interrupt when the signal changes to this value.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("SignalType", "ST", "Type of Signal to be monitored (DI, DO, AI, AO, GI, GO)", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_CodeLine(), "Connect Code", "CC", "Code to connect interrupt. Both Declaration and Instruction Code is generated");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Creates the input value list and attachs it to the input parameter
            if (this.Params.Input[3].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, typeof(SignalType), 3);
            }

            // Expire solution of this component
            if (_expire == true)
            {
                _expire = false;
                this.ExpireSolution(true);
                return;
            }

            // Declare variables to store input data
            string interruptName = string.Empty;
            string trapRoutineName = string.Empty;
            string signalName = string.Empty;
            double signalValue = 1;
            int signalTypeInt = 1;

            // Get data from input parameters
            if (!DA.GetData(0, ref trapRoutineName)) return;
            if (!DA.GetData(1, ref signalName)) return;
            if (!DA.GetData(2, ref signalValue)) signalValue = 1;
            if (!DA.GetData(3, ref signalTypeInt)) signalTypeInt = 1;

            interruptName = "int_" + trapRoutineName;

            //Create CodeLine Container
            List<CodeLine> codeLines = new List<CodeLine>();

            //Define declaration and connect code
            codeLines.Add(new CodeLine("VAR intNum " + interruptName + ";", CodeType.Declaration));
            codeLines.Add(new CodeLine("CONNECT " + interruptName + " WITH " + trapRoutineName + ";", CodeType.Instruction));

            switch (signalTypeInt)
            {
                case 0:
                    codeLines.Add(new CodeLine("ISignalDI " + signalName + ", " + (int)signalValue + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 1:
                    codeLines.Add(new CodeLine("ISignalDO " + signalName + ", " + (int)signalValue + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 2:
                    codeLines.Add(new CodeLine("ISignalAI " + signalName + ", " + signalValue + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 3:
                    codeLines.Add(new CodeLine("ISignalAO " + signalName + ", " + signalValue + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 4:
                    codeLines.Add(new CodeLine("ISignalGI " + signalName + ", " + (int)signalValue + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 5:
                    codeLines.Add(new CodeLine("ISignalGO " + signalName + ", " + (int)signalValue + ", " + interruptName + ";", CodeType.Instruction));
                    break;
            }

            //Set output data
            DA.SetDataList(0, codeLines);
        }

        #region properties
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }

        public override bool Obsolete
        {
            get { return true; }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ConnectInterrupt_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F7A3C8E1-9D42-4B6F-A158-2E7D0C9B34F6"); }
        }
        #endregion
    }
}
