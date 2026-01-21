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
using System;
using System.Collections.Generic;
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Gh.Goos.Actions.Declarations;
using RobotComponents.ABB.Gh.Parameters.Actions.Declarations;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Action : Speed Data component.
    /// </summary>
    public class RoutineArgumentComponent : GH_RobotComponent
    {
        #region fields
        private GH_Structure<GH_SpeedData> _tree = new GH_Structure<GH_SpeedData>();
        private List<string> _registered = new List<string>();
        private readonly List<string> _toRegister = new List<string>();
        private ObjectManager _objectManager;
        private string _lastName = "";
        private bool _isUnique = true;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public RoutineArgumentComponent() : base("Routine Argument", "RA", "Code Generation",
              "Defines an argument to be used by an additional routine.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Keyword", "K", "Argument keyword (INOUT, VAR, PERS)", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "T", "Argument Data Type", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Argument Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Value", "V", "Argument Value", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RoutineArgument(), "Argument", "Arg", "Resulting Routine Argument");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Sets inputs 
            string keyword = null;
            string type = "";
            string name = "";
            string value = "null";
            object valueObject = null;

            // Catch the input data
            if (!DA.GetData(0, ref keyword)) {}
            if (!DA.GetData(1, ref type)) { return; }
            if (!DA.GetData(2, ref name)) { return; }
            if (!DA.GetData(3, ref valueObject)) {}

            type = type.Trim();
            name = name.Trim();

            if (!string.IsNullOrEmpty(keyword))
                keyword = keyword.ToUpper().Trim();

            if (valueObject != null)
                value = valueObject.ToString();

            // Sets Output
            DA.SetData(0, new RoutineArgument(type, name, value, keyword));
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
            get { return Properties.Resources.RoutineArgument_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5F92D4A8-B1E7-4C63-8D2F-7A3E9B6C1D5F"); }
        }
        #endregion
    }
}

