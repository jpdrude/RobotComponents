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
using GH_IO.Serialization;
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Display;
// Rhino Libs
using Rhino.Geometry;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Actions.Declarations;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
using RobotComponents.ABB.Kinematics;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RobotComponents.ABB.Gh.Components.Simulation
{
    /// <summary>
    /// RobotComponents Forward Kinematics component.
    /// </summary>
    public class CheckActionsComponent : GH_RobotComponent
    {
        #region fields
        private List<Mesh> outMeshes = new List<Mesh>();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public CheckActionsComponent() : base("CheckActions", "CA", "Simulation",
              "Checks the valitity of the provided actions.")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Robot(), "Robot", "R", "Robot that is used as Robot.", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Action(), "Actions", "A", "Actions as list of instructive and declarative Actions.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Messages", "M", "Output Messages", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            Robot robot = null;
            List<RobotComponents.ABB.Actions.IAction> actions = new List<RobotComponents.ABB.Actions.IAction>();
            outMeshes.Clear();

            // Catch input data
            if (!DA.GetData(0, ref robot)) { return; }
            if (!DA.GetDataList(1, actions)) { return; }

            int actionIndex = 0;
            int firstError = -1;
            List<string> errorMessages = new List<string>();

            ForwardKinematics fk = new ForwardKinematics(robot);
            InverseKinematics ik = new InverseKinematics(robot);

            //Iterate over List of Actions
            foreach (RobotComponents.ABB.Actions.IAction action in actions)
            {
                switch (action)
                {
                    case JointTarget jt:
                        {
                            fk.Calculate(jt);
                            if (fk.ErrorText.Count > 0)
                            {
                                string compoundError = "Errors in action " + actionIndex + ": ";
                                for (int i = 0; i < fk.ErrorText.Count; ++i)
                                {
                                    string error = fk.ErrorText[i];
                                    compoundError += error;
                                    if (i < fk.ErrorText.Count - 1)
                                    {
                                        compoundError += "; ";
                                    }
                                }
                                errorMessages.Add(compoundError);
                                if (firstError == -1)
                                {
                                    firstError = actionIndex;
                                }
                            }
                            break;
                        }
                    case Movement m:
                        {
                            ik.Calculate(m);
                            if (ik.ErrorText.Count > 0)
                            {
                                string compoundError = "Errors in action " + actionIndex + ": ";
                                for (int i = 0; i < ik.ErrorText.Count; ++i)
                                {
                                    string error = ik.ErrorText[i];
                                    compoundError += error;
                                    if (i < ik.ErrorText.Count - 1)
                                    {
                                        compoundError += "; ";
                                    }
                                }
                                errorMessages.Add(compoundError);
                                if (firstError == -1)
                                {
                                    firstError = actionIndex;
                                }
                            }
                            break;
                        }
                    default:
                        {
                            if (!action.IsValid)
                            {
                                errorMessages.Add("Errors in action " + actionIndex + ": Action is not valid.");
                                if (firstError == -1)
                                {
                                    firstError = actionIndex;
                                }
                            }
                            break;
                        }
                }
                ++actionIndex;
            }

            //Draw last valid Mesh;
            if (firstError != -1)
            {
                for (int i = firstError - 1; i >= 0; --i)
                {
                    if (actions[i].GetType() == typeof(Movement))
                    {
                        Movement m = (Movement)actions[i];
                        ik.Calculate(m);
                        fk.Calculate(ik.RobotJointPosition);
                        outMeshes = fk.PosedRobotMeshes;
                        break;
                    }
                }

                if (outMeshes.Count == 0)
                    outMeshes = robot.PoseMeshes(new JointTarget("home", new RobotJointPosition(0, 0, 0, 0, 0, 0)));
            }

            if(errorMessages.Count == 0)
            {
                errorMessages.Add("All actions are valid.");
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid actions discovered. First invalid: Action " + firstError);
            }

            // Output
            DA.SetDataList(0, errorMessages);
            
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary, dropdown and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
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
            get { return Properties.Resources.CheckActions_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D8A42E91-7B5C-4F29-A3D2-8E4F1C9B6D70"); }
        }
        #endregion

        #region custom preview method
        /// <summary>
        /// Gets the clipping box for all preview geometry drawn by this component and all associated parameters.
        /// </summary>
        public override BoundingBox ClippingBox
        {
            get { return GetBoundingBox(); }
        }

        /// <summary>
        /// Returns the bounding box for all preview geometry drawn by this component.
        /// </summary>
        /// <returns></returns>
        private BoundingBox GetBoundingBox()
        {
            BoundingBox result = new BoundingBox();

            // Get bouding box of all the output parameters
            for (int i = 0; i < Params.Output.Count; i++)
            {
                if (Params.Output[i] is IGH_PreviewObject previewObject)
                {
                    result.Union(previewObject.ClippingBox);
                }
            }

            return result;
        }

        /// <summary>
        /// This method displays the robot pose for the given axis values. 
        /// </summary>
        /// <param name="args"> Preview display arguments for IGH_PreviewObjects. </param>
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            base.DrawViewportMeshes(args);

            // Initiate the display color and transparancy of the mechanical unit mesh
            DisplayMaterial displayMaterial = DisplaySettings.DisplayMaterialOutsideLimits;

            if (outMeshes.Count != 0)
                foreach (Mesh outMesh in outMeshes)
                    args.Display.DrawMeshShaded(outMesh, displayMaterial);

        }
        #endregion

        #region additional methods

        #endregion
    }
}
