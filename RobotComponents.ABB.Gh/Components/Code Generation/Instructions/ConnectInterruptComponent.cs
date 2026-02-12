// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2025 EDEK Uni Kassel
//
// Author:
//   - Jan Philipp Drude (2026)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
// Rhino Libs
using Rhino.Geometry;
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using GH_IO.Serialization;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Parameters.Actions.Instructions;
using RobotComponents.ABB.Gh.Parameters.Actions.Declarations;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
using RobotComponents.ABB.Gh.Parameters.Actions.Dynamic;
using RobotComponents.ABB.Actions.Dynamic;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Action : Connect Interrupt Component.
    /// </summary>
    public class ConnectInterruptComponent : GH_RobotComponent
    {
        #region fields
        private bool _expire = false;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, subcategory the panel. 
        /// If you use non-existing tab or panel names new tabs/panels will automatically be created.
        /// </summary>
        public ConnectInterruptComponent() : base("Connect Interrupt", "CI", "Code Generation",
              "Connects a TRAP routine to a signal change.")

        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TRAP Routine Name", "TR", "Name of the TRAP routine to be called when the signal change occurs.", GH_ParamAccess.item);
            pManager.AddTextParameter("Signal Name", "SN", "Name of the signal that is monitored for changes.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Signal Value", "SV", "Value of the signal that triggers the interrupt when the signal changes to this value.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("SignalType", "ST", "Type of Signal to be monitored (DI, DO, AI, AO, GI, GO)", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;    
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_CodeLine(), "Connect Code", "CC", "Code to connect interrupt. Both Declaration and Instruction Code is generated");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
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