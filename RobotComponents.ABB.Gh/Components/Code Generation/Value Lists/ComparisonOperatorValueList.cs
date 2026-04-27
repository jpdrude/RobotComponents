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
using System.Drawing;
// Grasshopper Libs
using Grasshopper;
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration.ValueLists
{
    /// <summary>
    /// RobotComponents Comparison Operator Value List.
    /// Creates a value list with all RAPID comparison operators and removes itself from the canvas.
    /// </summary>
    public class ComparisonOperatorValueList : GH_RobotComponent
    {
        #region fields
        private bool _created = false;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public ComparisonOperatorValueList() : base("Comparison Operators", "CO", "Code Generation",
              "Defines a value list with comparison operators for use in RAPID conditional constructs.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (_created == false)
            {
                PointF location = new PointF(this.Attributes.Pivot.X, this.Attributes.Pivot.Y);
                _created = HelperMethods.CreateValueList(typeof(ComparisonOperator), location);
            }

            Instances.ActiveCanvas.Document.RemoveObject(this, true);
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quarternary | GH_Exposure.obscure; }
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
            get { return Properties.Resources.ComparisonSymbolValueList_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B4E2F6A1-3D97-4C08-A5E3-1F8B2D7C9E40"); }
        }
        #endregion
    }
}
