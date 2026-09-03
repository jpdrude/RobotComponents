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
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Comparer Expression Component.
    /// Combines two RAPID operands with a comparison operator into a single RAPIDExpression
    /// that can be wired into IF, WHILE, or other expression inputs.
    /// </summary>
    public class ComparerExpressionComponent : GH_RobotComponent
    {
        #region fields
        private bool _expire = false;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public ComparerExpressionComponent() : base("Comparison Expression", "CE", "Advanced RAPID Features",
            "Combines two RAPID operands (A and B) with a comparison operator into a boolean RAPID expression " +
            "(e.g. counter < 10). The result can be wired into IF condition or WHILE condition inputs.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_RAPIDExpression(), "A", "A",
                "Left operand. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);
            pManager.AddIntegerParameter("Operator", "O",
                "Comparison operator. Use the Comparison Operators value list to select one.",
                GH_ParamAccess.item, 0);
            pManager.AddParameter(new Param_RAPIDExpression(), "B", "B",
                "Right operand. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RAPIDExpression(), "Expression", "E",
                "Boolean RAPID expression composed from A, the operator, and B.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Auto-create the comparison operator value list on first use
            if (this.Params.Input[1].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, new List<string>() { "<", ">", "<=", ">=", "\uFF1D", "<>" }, 1);
            }

            if (_expire)
            {
                _expire = false;
                this.ExpireSolution(true);
                return;
            }

            GH_RAPIDExpression exprA = null;
            int operatorInt = 0;
            GH_RAPIDExpression exprB = null;

            if (!DA.GetData(0, ref exprA)) { return; }
            if (!DA.GetData(1, ref operatorInt)) { return; }
            if (!DA.GetData(2, ref exprB)) { return; }

            if (!Enum.IsDefined(typeof(ComparisonOperator), operatorInt))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    $"Operator value {operatorInt} is not a valid ComparisonOperator. Use the Comparison Operators value list.");
                return;
            }

            string a = HelperMethods.CheckRAPIDExpression(this, exprA, "A", "");
            string b = HelperMethods.CheckRAPIDExpression(this, exprB, "B", "");

            string op = OperatorToRAPID((ComparisonOperator)operatorInt);
            DA.SetData(0, new GH_RAPIDExpression(RAPIDExpression.FromString($"{a} {op} {b}")));
        }

        /// <summary>
        /// Returns the RAPID operator symbol for the given <see cref="ComparisonOperator"/>.
        /// </summary>
        private static string OperatorToRAPID(ComparisonOperator op)
        {
            switch (op)
            {
                case ComparisonOperator.LT: return "<";
                case ComparisonOperator.GT: return ">";
                case ComparisonOperator.LE: return "<=";
                case ComparisonOperator.GE: return ">=";
                case ComparisonOperator.EQ: return "=";
                case ComparisonOperator.NE: return "<>";
                default:                    return "<";
            }
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
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
            get { return Properties.Resources.ComparerExpression_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F2A3E1B4-7C96-4D08-B5E4-2A9A4F8D1C63"); }
        }
        #endregion
    }
}
