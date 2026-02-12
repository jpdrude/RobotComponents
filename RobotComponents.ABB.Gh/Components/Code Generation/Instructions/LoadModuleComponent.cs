// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2025-2026 EDEK Uni Kassel
//
// Author:
//   - Jan Philipp Drude (2026)
//
// For license details, see the LICENSE file in the project root.

// System Libs
// Grasshopper Libs
using Grasshopper.Kernel;
using RobotComponents.ABB.Actions.Dynamic;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Parameters.Actions.Dynamic;
using RobotComponents.ABB.Gh.Parameters.Actions.Instructions;
using RobotComponents.ABB.Gh.Utils;
using System;
using System.IO;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Action : Load Module instruction.
    /// </summary>
    public class LoadModuleComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public LoadModuleComponent() : base("Load Module", "LM", "Code Generation",
              "Defines code to load and unload RAPID modules.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Module Name", "N", "Name of the module as text.", GH_ParamAccess.item);
            pManager.AddTextParameter("Load Session Name", "LS", "Name of the load session for async loading.", GH_ParamAccess.item, "");

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_CodeLine(), "Load Module", "L", "RAPID code to load RAPID module.");
            pManager.RegisterParam(new Param_CodeLine(), "Unload Module", "U", "RAPID code to unload RAPID module.");
            pManager.RegisterParam(new Param_CodeLine(), "Load Session Declaration", "LS", "RAPID declaration to reference code session. Use for async loading.");
            pManager.RegisterParam(new Param_CodeLine(), "Start Load Module", "SL", "RAPID code to start asynchronous loading of RAPID module.");
            pManager.RegisterParam(new Param_CodeLine(), "Wait Load Module", "WL", "RAPID code to wait until RAPID module has been loaded asynchronously.");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            string name = "";
            string loadSessionName = "";

            // Catch the input data
            if (!DA.GetData(0, ref name)) { return; }
            if (!DA.GetData(1, ref loadSessionName)) { loadSessionName = "loadSession01"; }

            // Check name
            name = HelperMethods.ReplaceSpacesAndRemoveNewLines(name);

            if (HelperMethods.StringExeedsCharacterLimit32(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Module name exceeds character limit of 32 characters.");
            }
            if (HelperMethods.StringStartsWithNumber(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Module name starts with a number which is not allowed in RAPID code.");
            }
            if (HelperMethods.StringStartsWithNumber(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Module name constains special characters which is not allowed in RAPID code.");
            }

            string remoteAdditionalDirectory = "HOME:/Robot Components/Additional Modules/";
            CodeLine load = new CodeLine($"LOAD \\Dynamic, \"{remoteAdditionalDirectory}\" \\FILE:=\"{name}.MOD\";", CodeType.Instruction);
            CodeLine unload = new CodeLine($"UNLOAD \"{remoteAdditionalDirectory}{name}.MOD\";", CodeType.Instruction);

            CodeLine loadSessionDeclaration = new CodeLine($"VAR loadsession {loadSessionName};", CodeType.Declaration);
            CodeLine startLoad = new CodeLine($"STARTLOAD \\Dynamic, \"{remoteAdditionalDirectory}\" \\FILE:=\"{name}.MOD\", {loadSessionName};", CodeType.Instruction);
            CodeLine waitLoad = new CodeLine($"WAITLOAD {loadSessionName};", CodeType.Instruction);


            // Sets Output
            DA.SetData(0, load);
            DA.SetData(1, unload);
            DA.SetData(2, loadSessionDeclaration);
            DA.SetData(3, startLoad);
            DA.SetData(4, waitLoad);
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
            get { return Properties.Resources.LoadModule_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A7F3E2B1-8C4D-4E6F-9A2B-1D5C8E7F3A9B"); }
        }
        #endregion
    }

}
