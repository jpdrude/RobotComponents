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
using Grasshopper.Kernel.Data;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Gh.Components;
using RobotComponents.ABB.Gh.Goos.Actions.Declarations;
using RobotComponents.ABB.Gh.Parameters.Actions.Declarations;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Obsolete
{
    /// <summary>
    /// RobotComponents Action : Routine Argument component.
    /// </summary>
    /// <remarks>
    /// Hidden from the menu since the "Variable" output was added. Retained so older .gh files
    /// that placed this component before that change continue to load and resolve their saved
    /// param GUIDs unchanged; the live component now has a new GUID for its new output shape.
    /// </remarks>
    [Obsolete("This component is OBSOLETE and will be removed in the future. Use Routine Argument instead.", false)]
    public class RoutineArgumentComponent_OBSOLETE : GH_RobotComponent
    {
        #region fields
        private GH_Structure<GH_SpeedData> _tree = new GH_Structure<GH_SpeedData>();
        private List<string> _registered = new List<string>();
        private readonly List<string> _toRegister = new List<string>();
        private ObjectManager _objectManager;
        private string _lastName = "";
        private bool _isUnique = true;
        #endregion

        public RoutineArgumentComponent_OBSOLETE() : base("Routine Argument", "RA", "Advanced RAPID Features",
              "Defines an argument to be used by an additional routine.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Keyword", "K", "Optional Argument keyword (INOUT, PERS)", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "T", "Argument Data Type", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Argument Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Value", "V", "Argument Value", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RoutineArgument(), "Argument", "Arg", "Resulting Routine Argument");
        }

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
