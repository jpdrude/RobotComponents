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
using Grasshopper.Kernel.Parameters;
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
    /// Hidden from the menu since this component was converted to a menu-driven
    /// IGH_VariableParameterComponent to add the optional "Interrupt Variable Name" override
    /// input (right-click "Override Interrupt Variable Name"), needed to connect more than one
    /// interrupt to the same TRAP routine -- the default int_&lt;TrapRoutineName&gt; naming
    /// collides otherwise. Retained so older .gh files that placed this component before that
    /// change continue to load and resolve their saved param GUIDs unchanged; the live component
    /// now has a new GUID since its parameter-restore mechanism changed. This is the third
    /// obsolete snapshot for this component -- see also ConnectInterruptComponent_OBSOLETE (v5,
    /// frozen when the Enable/Disable Interrupts outputs were added) and
    /// ConnectInterruptComponent_OBSOLETE2 (v7, frozen when Signal Name was changed to accept a
    /// RAPID Variable for the new Persistent Data signal type).
    /// </remarks>
    [Obsolete("This component is OBSOLETE and will be removed in the future. Use Connect Interrupt instead.", false)]
    public class ConnectInterruptComponent_OBSOLETE3 : GH_RobotComponent
    {
        #region fields
        private bool _expire = false;
        #endregion

        public ConnectInterruptComponent_OBSOLETE3() : base("Connect Interrupt", "CI", "Advanced RAPID Features",
              "Connects a TRAP routine to a signal change.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TRAP Routine Name", "TR", "Name of the TRAP routine to be called when the signal change occurs.", GH_ParamAccess.item);
            pManager.AddParameter(new Param_GenericObject(), "Signal Name", "SN",
                "Name of the signal that is monitored for changes. In Persistent Data mode, this is instead the " +
                "RAPID persistent (PERS) variable to monitor: connect a RAPID Variable component, or type its name.",
                GH_ParamAccess.item);
            pManager.AddNumberParameter("Signal Value", "SV",
                "Value of the signal that triggers the interrupt when the signal changes to this value. " +
                "Not used in Persistent Data mode: the RAPID IPers instruction has no triggering value.",
                GH_ParamAccess.item);
            pManager.AddIntegerParameter("Signal Type", "ST", "Type of Signal to be monitored (DI, DO, AI, AO, GI, GO, or Persistent Data)", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_CodeLine(), "Connect Code", "CC", "Code to connect interrupt. Both Declaration and Instruction Code is generated");
            pManager.RegisterParam(new Param_CodeLine(), "Enable Interrupts", "IE",
                "RAPID IEnable; instruction, (re-)enabling interrupts. Interrupts that were registered while " +
                "interrupts were disabled are queued and executed once interrupts are re-enabled with this instruction.",
                GH_ParamAccess.item);
            pManager.RegisterParam(new Param_CodeLine(), "Disable Interrupts", "ID",
                "RAPID IDisable; instruction, disabling interrupts. Interrupts that are registered while interrupts " +
                "are disabled are not discarded: they are queued to be executed once interrupts are re-enabled.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Creates the input value list and attachs it to the input parameter. Uses a hardcoded
            // name list frozen to this component's own 6-case switch below (SolveInstance has no
            // case for PersistentData = 6, only 0-5), rather than reflecting off the live
            // SignalType enum (typeof(SignalType)) -- see CLAUDE.md's Obsolete/vN write-up on
            // shared-enum drift.
            if (this.Params.Input[3].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, new List<string>
                {
                    "DigitalInput", "DigitalOutput", "AnalogInput", "AnalogOutput", "GroupInput", "GroupOutput"
                }, 3);
            }

            // Expire solution of this component
            if (_expire == true)
            {
                _expire = false;
                this.ExpireSolution(true);
                return;
            }

            // The Enable/Disable Interrupts outputs are static instructions, independent of the
            // connect-interrupt inputs, so they are always available.
            DA.SetData(1, new CodeLine("IEnable;", CodeType.Instruction));
            DA.SetData(2, new CodeLine("IDisable;", CodeType.Instruction));

            // Declare variables to store input data
            string interruptName = string.Empty;
            string trapRoutineName = string.Empty;
            object rawSignalName = null;
            double signalValue = 1;
            int signalTypeInt = 1;

            // Get data from input parameters
            if (!DA.GetData(0, ref trapRoutineName)) return;
            if (!DA.GetData(1, ref rawSignalName)) return;
            bool hasSignalValue = DA.GetData(2, ref signalValue);
            if (!hasSignalValue) signalValue = 1;
            if (!DA.GetData(3, ref signalTypeInt)) signalTypeInt = 1;

            // Resolves to the signal name (plain text) or, in Persistent Data mode, the declared
            // name of the connected RAPID Variable -- same handling used for any other input that
            // accepts either a literal or a RAPID declaration/variable/expression.
            string signalName = HelperMethods.ResolveRAPIDValueExpression(rawSignalName);

            if ((SignalType)signalTypeInt == SignalType.PersistentData && hasSignalValue)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Signal Value is connected but is not used in Persistent Data mode: the RAPID IPers " +
                    "instruction has no triggering value. Disconnect it to remove this warning.");
            }

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
                case 6:
                    // Persistent data: IPers takes the PERS name and the interrupt variable, no
                    // triggering value (unlike the ISignalXX instructions above).
                    codeLines.Add(new CodeLine("IPers " + signalName + ", " + interruptName + ";", CodeType.Instruction));
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
            get { return new Guid("FB7FCD2B-9A1D-4213-BC52-66DBAF9E314F"); }
        }
        #endregion
    }
}
