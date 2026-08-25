// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2018-2020 EDEK Uni Kassel
// Copyright (c) 2020-2026 Arjen Deetman
//
// Authors:
//   - Gabriel Rumph (2018-2020)
//   - Benedikt Wannemacher (2018-2020)
//   - Arjen Deetman (2019-2026)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using GH_IO.Serialization;
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Display;
// Rhino Libs
using Rhino.Geometry;
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Actions.Declarations;
using RobotComponents.ABB.Gh.Parameters.Actions.Instructions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
// RobotComponents Libs
using RobotComponents.ABB.Kinematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RobotComponents.ABB.Gh.Components.Simulation
{
    /// <summary>
    /// RobotComponents Timed Path Generator component.
    /// </summary>
    public class TimedPathGeneratorComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private readonly List<TimedPathGenerator> _pathGenerators = new List<TimedPathGenerator>();
        private readonly List<ForwardKinematics> _forwardKinematics = new List<ForwardKinematics>();
        private readonly List<bool> _calculated = new List<bool>();
        private readonly List<double> _timeIntervals = new List<double>();
        private readonly List<int> _lastInputHash = new List<int>();
        private bool _outputPath = true;
        private bool _outputMovement = false;
        private bool _outputMovements = false;
        private bool _outputRobotEndPlane = false;
        private bool _outputRobotEndPlanes = false;
        private bool _outputRobotJointPosition = false;
        private bool _outputRobotJointPositions = false;
        private bool _outputConfigurationData = false;
        private bool _outputConfigurationDatas = false;
        private bool _outputExternalAxisPlanes = false;
        private bool _outputExternalJointPosition = false;
        private bool _outputExternalJointPositions = false;
        private bool _outputProgramTime = false;
        private bool _outputErrorMessages = false;
        private bool _outputMesh = false;
        private bool _previewPath = true;
        private bool _previewMesh = true;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel.
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public TimedPathGeneratorComponent() : base("Timed Path Generator", "TPG", "Simulation",
              "Generates and displays an approximation of the movement path for a defined ABB robot based on a list of Actions. " +
              "Each movement is subdivided by the time it takes to execute, computed from the path length and TCP speed, " +
              "so that every step represents the given time interval in seconds.")
        {
            // Create the component label with a message
            Message = "EXTENDABLE";
        }

        /// <summary>
        /// Stores the variable output parameters in an array.
        /// </summary>
        private readonly IGH_Param[] _variableOutputParameters = new IGH_Param[15]
        {
            new Param_Curve() { Name = "Path", NickName = "P", Description = "The whole tool path as list with curves", Access = GH_ParamAccess.list},
            new Param_Movement() { Name = "Movement", NickName = "M", Description = "The current move object.", Access = GH_ParamAccess.item},
            new Param_Movement() { Name = "Movements", NickName = "Ms", Description = "The move objects of the whole path.", Access = GH_ParamAccess.list},
            new Param_Plane() { Name = "Robot End Plane", NickName = "EP", Description = "The current position and orientation of tool TCP", Access = GH_ParamAccess.item},
            new Param_Plane() { Name = "Robot End Planes", NickName = "EPs", Description = "The positions and orientations of the tool TCP of the whole path", Access = GH_ParamAccess.list},
            new Param_RobotJointPosition() { Name = "Robot Joint Position", NickName = "RJ", Description = "The current Robot Joint Position", Access = GH_ParamAccess.item},
            new Param_RobotJointPosition() { Name = "Robot Joint Positions", NickName = "RJs", Description = "The Robot Joint Positions of the whole path", Access = GH_ParamAccess.list},
            new Param_ConfigurationData() { Name = "Configuration Data", NickName = "CD", Description = "The current Configuration Data", Access = GH_ParamAccess.item},
            new Param_ConfigurationData() { Name = "Configurations Datas", NickName = "CDs", Description = "The Configuration Datas of the whole path", Access = GH_ParamAccess.list},
            new Param_Plane() { Name = "External Axis Planes", NickName = "EAP", Description = "The current position and orientation of the external axes", Access = GH_ParamAccess.list},
            new Param_ExternalJointPosition() { Name = "External Joint Position", NickName = "EJ", Description = "The current External Joint Position", Access = GH_ParamAccess.item},
            new Param_ExternalJointPosition() { Name = "External Joint Positions", NickName = "EJs", Description = "The External Joint Positions of the whole path", Access = GH_ParamAccess.list},
            new Param_Number() { Name = "Time", NickName = "T", Description = "An estimation of the program time in minutes", Access = GH_ParamAccess.item},
            new Param_String() { Name = "Error messages", NickName = "E", Description = "The error messages collected during the generation of the path", Access = GH_ParamAccess.list},
            new Param_Mesh() { Name = "Posed Meshes", NickName = "PM", Description = "Posed Robot and External Axis meshes.", Access = GH_ParamAccess.tree},
        };

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Robot(), "Robot", "R", "Robot as Robot", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Action(), "Actions", "A", "Actions as a list with Actions", GH_ParamAccess.list);
            pManager.AddNumberParameter("Time Interval", "TI", "Time interval in seconds as Number. Each movement is divided into steps that each approximate this duration.", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Animation Slider", "AS", "Animation Slider as number (0.0 - 1.0)", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Update", "U", "If set to true, path will be constantly recalculated.", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            AddOutputParameter(0);
        }

        /// <summary>
        /// Override this method if you want to be called before the first call to SolveInstance.
        /// </summary>
        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();

            _forwardKinematics.Clear();
            _timeIntervals.Clear();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            Robot robot = new Robot();
            List<RobotComponents.ABB.Actions.IAction> actions = new List<RobotComponents.ABB.Actions.IAction>();
            double timeInterval = 0.1;
            double interpolationSlider = 0;
            bool update = false;

            // Catch the input data
            if (!DA.GetData(0, ref robot)) { return; }
            if (!DA.GetDataList(1, actions)) { return; }
            if (!DA.GetData(2, ref timeInterval)) { return; }
            if (!DA.GetData(3, ref interpolationSlider)) { return; }
            if (!DA.GetData(4, ref update)) { return; }

            // Generate Input Hash
            int inputHash = CreateInputHash(actions, timeInterval, robot);

            // Animation slider
            _timeIntervals.Add(interpolationSlider);

            // Create forward kinematics for mesh display
            ForwardKinematics forwardKinematics = new ForwardKinematics(robot);

            // Fill list if needed
            while (_calculated.Count <= DA.Iteration)
            {
                _calculated.Add(false);
            }
            while (_pathGenerators.Count <= DA.Iteration)
            {
                _pathGenerators.Add(null);
            }
            while (_lastInputHash.Count <= DA.Iteration)
            {
                _lastInputHash.Add(0);
            }

            // Update the path
            if ((update == true && inputHash != _lastInputHash[DA.Iteration]) | _calculated[DA.Iteration] == false)
            {
                // Create the path generator
                _pathGenerators[DA.Iteration] = new TimedPathGenerator(robot);

                // Re-calculate the path
                _pathGenerators[DA.Iteration].Calculate(actions, timeInterval);

                // Makes sure that there is always a calculated solution
                _calculated[DA.Iteration] = true;

                // Sets the last input hash
                _lastInputHash[DA.Iteration] = inputHash;
            }

            // Get the index number of the current target
            int index = (int)((_pathGenerators[DA.Iteration].Planes.Count - 1) * interpolationSlider);

            // Calculate forward kinematics
            if (_previewMesh == true | _outputMesh == true)
            {
                forwardKinematics.HideMesh = false;
            }
            else
            {
                forwardKinematics.HideMesh = true;
            }

            forwardKinematics.Calculate(_pathGenerators[DA.Iteration].RobotJointPositions[index], _pathGenerators[DA.Iteration].ExternalJointPositions[index]);

            // Show error messages
            if (_pathGenerators[DA.Iteration].ErrorText.Count != 0)
            {
                for (int i = 0; i < _pathGenerators[DA.Iteration].ErrorText.Count; i++)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, _pathGenerators[DA.Iteration].ErrorText[i]);
                    if (i == 30) { break; }
                }
            }

            // Add to list with FK
            _forwardKinematics.Add(forwardKinematics);

            // Output Parameters
            int ind;
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[0].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[0].NickName));
                DA.SetDataList(ind, _pathGenerators[DA.Iteration].Paths);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[1].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[1].NickName));
                DA.SetData(ind, _pathGenerators[DA.Iteration].Movements[index]);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[2].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[2].NickName));
                DA.SetDataList(ind, _pathGenerators[DA.Iteration].Movements);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[3].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[3].NickName));
                DA.SetData(ind, _pathGenerators[DA.Iteration].Planes[index]);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[4].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[4].NickName));
                DA.SetDataList(ind, _pathGenerators[DA.Iteration].Planes);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[5].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[5].NickName));
                DA.SetData(ind, _pathGenerators[DA.Iteration].RobotJointPositions[index]);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[6].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[6].NickName));
                DA.SetDataList(ind, _pathGenerators[DA.Iteration].RobotJointPositions);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[7].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[7].NickName));
                DA.SetData(ind, _pathGenerators[DA.Iteration].ConfigurationDatas[index]);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[8].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[8].NickName));
                DA.SetDataList(ind, _pathGenerators[DA.Iteration].ConfigurationDatas);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[9].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[9].NickName));
                DA.SetDataList(ind, forwardKinematics.PosedExternalAxisPlanes);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[10].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[10].NickName));
                DA.SetData(ind, _pathGenerators[DA.Iteration].ExternalJointPositions[index]);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[11].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[11].NickName));
                DA.SetDataList(ind, _pathGenerators[DA.Iteration].ExternalJointPositions);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[12].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[12].NickName));
                DA.SetData(ind, _pathGenerators[DA.Iteration].ProgramTime / 60);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[13].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[13].NickName));
                DA.SetDataList(ind, _pathGenerators[DA.Iteration].ErrorText);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[14].NickName)))
            {
                ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[14].NickName));
                DA.SetDataTree(ind, this.GetPosedMeshesDataTree(DA.Iteration));
            }
        }

        /// <summary>
        /// Override this method if you want to be called after the last call to SolveInstance.
        /// </summary>
        protected override void AfterSolveInstance()
        {
            base.AfterSolveInstance();

            if (RunCount != -1)
            {
                if (_pathGenerators.Count > RunCount)
                {
                    _pathGenerators.RemoveRange(RunCount, _pathGenerators.Count - RunCount);
                }

                if (_calculated.Count > RunCount)
                {
                    _calculated.RemoveRange(RunCount, _calculated.Count - RunCount);
                }
            }
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
            get { return Properties.Resources.TimedPathGen_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3A7F2C1D-8B4E-4F9A-B6C5-D2E1F0A39847"); }
        }
        #endregion

        #region menu item
        /// <summary>
        /// Adds the additional items to the context menu of the component.
        /// </summary>
        /// <param name="menu"> The context menu of the component. </param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Preview Path", MenuItemClickPreviewPath, true, _previewPath);
            Menu_AppendItem(menu, "Preview Posed Meshes", MenuItemClickPreviewMesh, true, _previewMesh);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Output current Movement", MenuItemClickOutputMovement, true, _outputMovement);
            Menu_AppendItem(menu, "Output all Movements", MenuItemClickOutputMovements, true, _outputMovements);
            Menu_AppendItem(menu, "Output current Robot End Plane", MenuItemClickOutputRobotEndPlane, true, _outputRobotEndPlane);
            Menu_AppendItem(menu, "Output all Robot End Planes", MenuItemClickOutputRobotEndPlanes, true, _outputRobotEndPlanes);
            Menu_AppendItem(menu, "Output current Robot Joint Position", MenuItemClickOutputRobotJointPosition, true, _outputRobotJointPosition);
            Menu_AppendItem(menu, "Output all Robot Joint Positions", MenuItemClickOutputRobotJointPositions, true, _outputRobotJointPositions);
            Menu_AppendItem(menu, "Output current Configuration Data", MenuItemClickOutputConfigurationData, true, _outputConfigurationData);
            Menu_AppendItem(menu, "Output all Configuration Datas", MenuItemClickOutputConfigurationDatas, true, _outputConfigurationDatas);
            Menu_AppendItem(menu, "Output current External Axis Planes", MenuItemClickOutputExternalAxisPlanes, true, _outputExternalAxisPlanes);
            Menu_AppendItem(menu, "Output current External Joint Position", MenuItemClickOutputExternalJointPosition, true, _outputExternalJointPosition);
            Menu_AppendItem(menu, "Output all External Joint Positions", MenuItemClickOutputExternalJointPositions, true, _outputExternalJointPositions);
            Menu_AppendItem(menu, "Output Program Time", MenuItemClickOutputProgramTime, true, _outputProgramTime);
            Menu_AppendItem(menu, "Output all Error Messages", MenuItemClickOutputErrorMessages, true, _outputErrorMessages);
            Menu_AppendItem(menu, "Output Posed Meshes", MenuItemClickOutputMesh, true, _outputMesh);

            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void MenuItemClickPreviewMesh(object sender, EventArgs e)
        {
            RecordUndoEvent("Preview Posed Meshes");
            _previewMesh = !_previewMesh;
            ExpireSolution(true);
        }

        private void MenuItemClickPreviewPath(object sender, EventArgs e)
        {
            RecordUndoEvent("Preview Path");
            _previewPath = !_previewPath;

            IGH_Param param = Params.Output.Find(x => x.NickName.Equality(_variableOutputParameters[0].NickName));

            if (param is IGH_PreviewObject previewObject)
            {
                previewObject.Hidden = !_previewPath;
            }

            ExpireSolution(true);
        }

        private void MenuItemClickOutputMovement(object sender, EventArgs e)
        {
            RecordUndoEvent("Output current Movement");
            _outputMovement = !_outputMovement;
            AddOutputParameter(1);
        }

        private void MenuItemClickOutputMovements(object sender, EventArgs e)
        {
            RecordUndoEvent("Output all Movements");
            _outputMovements = !_outputMovements;
            AddOutputParameter(2);
        }

        private void MenuItemClickOutputRobotEndPlane(object sender, EventArgs e)
        {
            RecordUndoEvent("Output current Robot End Plane");
            _outputRobotEndPlane = !_outputRobotEndPlane;
            AddOutputParameter(3);
        }

        private void MenuItemClickOutputRobotEndPlanes(object sender, EventArgs e)
        {
            RecordUndoEvent("Output all Robot End Planes");
            _outputRobotEndPlanes = !_outputRobotEndPlanes;
            AddOutputParameter(4);
        }

        private void MenuItemClickOutputRobotJointPosition(object sender, EventArgs e)
        {
            RecordUndoEvent("Output current Robot Joint Position");
            _outputRobotJointPosition = !_outputRobotJointPosition;
            AddOutputParameter(5);
        }

        private void MenuItemClickOutputRobotJointPositions(object sender, EventArgs e)
        {
            RecordUndoEvent("Output all Robot Joint Positions");
            _outputRobotJointPositions = !_outputRobotJointPositions;
            AddOutputParameter(6);
        }

        private void MenuItemClickOutputConfigurationData(object sender, EventArgs e)
        {
            RecordUndoEvent("Output current Configuration Data");
            _outputConfigurationData = !_outputConfigurationData;
            AddOutputParameter(7);
        }

        private void MenuItemClickOutputConfigurationDatas(object sender, EventArgs e)
        {
            RecordUndoEvent("Output all Configuration Datas");
            _outputConfigurationDatas = !_outputConfigurationDatas;
            AddOutputParameter(8);
        }

        private void MenuItemClickOutputExternalAxisPlanes(object sender, EventArgs e)
        {
            RecordUndoEvent("Output current External Axis Planes");
            _outputExternalAxisPlanes = !_outputExternalAxisPlanes;
            AddOutputParameter(9);
        }

        private void MenuItemClickOutputExternalJointPosition(object sender, EventArgs e)
        {
            RecordUndoEvent("Output current External Joint Position");
            _outputExternalJointPosition = !_outputExternalJointPosition;
            AddOutputParameter(10);
        }

        private void MenuItemClickOutputExternalJointPositions(object sender, EventArgs e)
        {
            RecordUndoEvent("Output all External Joint Positions");
            _outputExternalJointPositions = !_outputExternalJointPositions;
            AddOutputParameter(11);
        }

        private void MenuItemClickOutputProgramTime(object sender, EventArgs e)
        {
            RecordUndoEvent("Output Program Time");
            _outputProgramTime = !_outputProgramTime;
            AddOutputParameter(12);
        }

        private void MenuItemClickOutputErrorMessages(object sender, EventArgs e)
        {
            RecordUndoEvent("Output all Error Messages");
            _outputErrorMessages = !_outputErrorMessages;
            AddOutputParameter(13);
        }

        private void MenuItemClickOutputMesh(object sender, EventArgs e)
        {
            RecordUndoEvent("Output Posed Meshes");
            _outputMesh = !_outputMesh;
            AddOutputParameter(14);

            // Disable default mesh preview
            if (_outputMesh == true)
            {
                IGH_Param param = Params.Output.Find(x => x.NickName.Equality(_variableOutputParameters[14].NickName));

                if (param is IGH_PreviewObject previewObject)
                {
                    previewObject.Hidden = true;
                }
            }
        }

        /// <summary>
        /// Add our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("Preview Path", _previewPath);
            writer.SetBoolean("Preview Posed Meshes", _previewMesh);
            writer.SetBoolean("Output Path", _outputPath);
            writer.SetBoolean("Output Movement", _outputMovement);
            writer.SetBoolean("Output Movements", _outputMovements);
            writer.SetBoolean("Output Robot End Plane", _outputRobotEndPlane);
            writer.SetBoolean("Output Robot End Planes", _outputRobotEndPlanes);
            writer.SetBoolean("Output Robot Joint Position", _outputRobotJointPosition);
            writer.SetBoolean("Output Robot Joint Positions", _outputRobotJointPositions);
            writer.SetBoolean("Output Configuration Data", _outputConfigurationData);
            writer.SetBoolean("Output Configuration Datas", _outputConfigurationDatas);
            writer.SetBoolean("Output External Axis Planes", _outputExternalAxisPlanes);
            writer.SetBoolean("Output External Joint Position", _outputExternalJointPosition);
            writer.SetBoolean("Output External Joint Positions", _outputExternalJointPositions);
            writer.SetBoolean("Output Program Time", _outputProgramTime);
            writer.SetBoolean("Output Error Messages", _outputErrorMessages);
            writer.SetBoolean("Output Posed Meshes", _outputMesh);
            return base.Write(writer);
        }

        /// <summary>
        /// Read our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        public override bool Read(GH_IReader reader)
        {
            _previewPath = reader.GetBoolean("Preview Path");
            _previewMesh = reader.GetBoolean("Preview Posed Meshes");
            _outputPath = reader.GetBoolean("Output Path");
            _outputMovement = reader.GetBoolean("Output Movement");
            _outputMovements = reader.GetBoolean("Output Movements");
            _outputRobotEndPlane = reader.GetBoolean("Output Robot End Plane");
            _outputRobotEndPlanes = reader.GetBoolean("Output Robot End Planes");
            _outputRobotJointPosition = reader.GetBoolean("Output Robot Joint Position");
            _outputRobotJointPositions = reader.GetBoolean("Output Robot Joint Positions");
            _outputConfigurationData = reader.GetBoolean("Output Configuration Data");
            _outputConfigurationDatas = reader.GetBoolean("Output Configuration Datas");
            _outputExternalAxisPlanes = reader.GetBoolean("Output External Axis Planes");
            _outputExternalJointPosition = reader.GetBoolean("Output External Joint Position");
            _outputExternalJointPositions = reader.GetBoolean("Output External Joint Positions");
            _outputProgramTime = reader.GetBoolean("Output Program Time");
            _outputErrorMessages = reader.GetBoolean("Output Error Messages");
            _outputMesh = reader.GetBoolean("Output Posed Meshes");
            return base.Read(reader);
        }

        /// <summary>
        /// Adds or destroys the output parameter to the component.
        /// </summary>
        /// <param name="index"> The index number of the parameter that needs to be added. </param>
        private void AddOutputParameter(int index)
        {
            // Pick the parameter
            IGH_Param parameter = _variableOutputParameters[index];

            // If the parameter already exists: remove it
            if (Params.Output.Any(x => x.NickName.Equality(parameter.NickName)))
            {
                Params.UnregisterOutputParameter(Params.Output.First(x => x.NickName.Equality(parameter.NickName)), true);
            }
            else
            {
                // The index where the parameter should be added
                int insertIndex = 0;

                // Check if other parameters are already added and correct the insert index
                for (int i = 0; i < index; i++)
                {
                    if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[i].NickName)))
                    {
                        insertIndex += 1;
                    }
                }

                // Register the input parameter
                Params.RegisterOutputParam(parameter, insertIndex);
            }

            // Expire solution and refresh parameters since they changed
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        #endregion

        #region variable input parameters
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            return null;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
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

        private BoundingBox GetBoundingBox()
        {
            BoundingBox result = new BoundingBox();

            for (int i = 0; i < Params.Output.Count; i++)
            {
                if (Params.Output[i] is IGH_PreviewObject previewObject)
                {
                    result.Union(previewObject.ClippingBox);
                }
            }

            if (_previewMesh)
            {
                for (int i = 0; i < _forwardKinematics.Count; i++)
                {
                    result.Union(_forwardKinematics[i].GetBoundingBox(false));
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

            if (_previewMesh)
            {
                for (int i = 0; i < _forwardKinematics.Count; i++)
                {
                    DisplayMaterial displayMaterial;

                    if (_forwardKinematics[i].IsInLimits == false)
                    {
                        displayMaterial = DisplaySettings.DisplayMaterialOutsideLimits;
                    }
                    else
                    {
                        displayMaterial = DisplaySettings.DisplayMaterialInLimits;
                    }

                    for (int j = 0; j < _forwardKinematics[i].PosedRobotMeshes.Count; j++)
                    {
                        if (_forwardKinematics[i].PosedRobotMeshes[j] != null)
                        {
                            if (_forwardKinematics[i].PosedRobotMeshes[j].IsValid)
                            {
                                args.Display.DrawMeshShaded(_forwardKinematics[i].PosedRobotMeshes[j], displayMaterial);
                            }
                        }
                    }

                    for (int j = 0; j < _forwardKinematics[i].PosedExternalAxisMeshes.Count; j++)
                    {
                        for (int k = 0; k < _forwardKinematics[i].PosedExternalAxisMeshes[j].Count; k++)
                        {
                            if (_forwardKinematics[i].PosedExternalAxisMeshes[j][k] != null)
                            {
                                if (_forwardKinematics[i].PosedExternalAxisMeshes[j][k].IsValid)
                                {
                                    args.Display.DrawMeshShaded(_forwardKinematics[i].PosedExternalAxisMeshes[j][k], displayMaterial);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region additional methods
        /// <summary>
        /// Transforms the posed meshes from the forward kinematics to a data tree.
        /// </summary>
        private GH_Structure<GH_Mesh> GetPosedMeshesDataTree(int iteration)
        {
            GH_Structure<GH_Mesh> meshes = new GH_Structure<GH_Mesh>();

            List<Mesh> posedInternalAxisMeshes = _forwardKinematics[iteration].PosedRobotMeshes;

            GH_Path path = new GH_Path(new int[2] { iteration, 0 });

            for (int i = 0; i < posedInternalAxisMeshes.Count; i++)
            {
                meshes.Append(new GH_Mesh(posedInternalAxisMeshes[i]), path);
            }

            List<List<Mesh>> posedExternalAxisMeshes = _forwardKinematics[iteration].PosedExternalAxisMeshes;

            for (int i = 0; i < posedExternalAxisMeshes.Count; i++)
            {
                path = new GH_Path(new int[2] { iteration, i + 1 });

                for (int j = 0; j < posedExternalAxisMeshes[i].Count; j++)
                {
                    meshes.Append(new GH_Mesh(posedExternalAxisMeshes[i][j]), path);
                }
            }

            return meshes;
        }

        /// <summary>
        /// Computes a unique input hash based on robot, tool, base, external axes, and action parameters.
        /// </summary>
        int CreateInputHash(List<Actions.IAction> actions, double timeInterval, Robot robot)
        {
            IList<Actions.IAction> ungrouped = UngroupActions(actions);

            int hash = (int)(timeInterval * 1000);
            hash = hash * 31 + robot.Name.GetHashCode();
            hash = hash * 31 + robot.Tool.Name.GetHashCode();
            hash = hash * 31 + robot.Tool.ToRAPID().GetHashCode();
            hash = hash * 31 + robot.BasePlane.GetHashCode();
            hash = hash * 31 + robot.MountingFrame.GetHashCode();

            foreach (Plane plane in robot.InternalAxisPlanes)
            {
                hash = hash * 31 + plane.GetHashCode();
            }

            foreach (Interval limit in robot.InternalAxisLimits)
            {
                hash = hash * 31 + limit.GetHashCode();
            }

            hash = hash * 31 + robot.ExternalAxes.Count.GetHashCode();

            foreach (IExternalAxis axis in robot.ExternalAxes)
            {
                hash = hash * 31 + axis.Name.GetHashCode();
                hash = hash * 31 + axis.AxisLimits.GetHashCode();
                hash = hash * 31 + axis.AxisPlane.GetHashCode();
                hash = hash * 31 + axis.AttachmentPlane.GetHashCode();
            }

            foreach (Actions.IAction action in ungrouped)
            {
                if (action == null)
                {
                    continue;
                }

                if (action is OverrideRobotTool overrideRobotTool)
                {
                    hash = hash * 31 + overrideRobotTool.RobotTool.Name.GetHashCode();
                    hash = hash * 31 + overrideRobotTool.RobotTool.ToRAPID().GetHashCode();
                }
                else if (action is JointConfigurationControl jointConfigurationControl)
                {
                    hash = hash * 31 + jointConfigurationControl.IsActive.GetHashCode();
                }
                else if (action is LinearConfigurationControl linearConfigurationControl)
                {
                    hash = hash * 31 + linearConfigurationControl.IsActive.GetHashCode();
                }
                else if (action is CirclePathMode circlePathMode)
                {
                    hash = hash * 31 + circlePathMode.Mode.GetHashCode();
                }
                else if (action is WaitTime waitTime)
                {
                    hash = hash * 31 + waitTime.Duration.GetHashCode();
                }
                else if (action is Movement movement)
                {
                    if (movement.Target != null)
                    {
                        hash = hash * 31 + movement.Target.ToRAPID().GetHashCode();
                    }
                }
            }
            return hash;
        }

        /// <summary>
        /// Returns a list with ungrouped actions.
        /// </summary>
        private IList<Actions.IAction> UngroupActions(IList<Actions.IAction> actions)
        {
            List<Actions.IAction> ungrouped = new List<Actions.IAction>() { };

            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] is ActionGroup group)
                {
                    ungrouped.AddRange(group.Ungroup());
                }
                else
                {
                    ungrouped.Add(actions[i]);
                }
            }

            return ungrouped;
        }
        #endregion
    }
}
