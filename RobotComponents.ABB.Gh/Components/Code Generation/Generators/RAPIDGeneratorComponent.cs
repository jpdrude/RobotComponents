// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2018-2020 EDEK Uni Kassel
// Copyright (c) 2020-2025 Arjen Deetman
//
// Authors:
//   - Gabriel Rumph (2018-2020)
//   - Benedikt Wannemacher (2018-2020)
//   - Arjen Deetman (2019-2025)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using GH_IO.Serialization;
// RobotComponents Libs
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Gh.Utils;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Actions.Declarations;
using Grasshopper;
using System.Drawing.Printing;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Rapid Generator component.
    /// </summary>
    public class RAPIDGeneratorComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private RAPIDGenerator _rapidGenerator = new RAPIDGenerator();
        private bool _firstMovementIsMoveAbsJ = true;
        private bool _isSystemModuleInputParam = false;
        private bool _moduleNameInputParam = false;
        private bool _routineNameInputParam = false;
        private bool _routineScopeParam = false;
        private bool _addMainModuleInputParam = false;
        private bool _addAdditionalRoutinesInputParam = false;
        private bool _addLoaddataInputParam = false;
        private bool _addTooldataInputParam = false;
        private bool _addWobjdataInputParam = false;
        private readonly int _fixedParamNumInput = 2;
        private bool _loaddataOutputParam = false;
        private bool _tooldataOutputParam = false;
        private bool _wobjdataOutputParam = false;
        private readonly int _fixedParamNumOutput = 1;

        private List<RAPIDGenerator> _rapidGenerators = new List<RAPIDGenerator>();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public RAPIDGeneratorComponent() : base("RAPID Generator", "RG", "Code Generation",
              "Generates the RAPID module for the ABB robot controller.")
        {
            // Create the component label with a message
            Message = "EXTENDABLE";
        }

        /// <summary>
        /// Stores the variable input parameters in an array.
        /// </summary>
        private readonly IGH_Param[] _variableInputParameters = new IGH_Param[10]
        {
            new Param_Boolean() { Name = "Is System Moudle", NickName = "SM", Description = "Indicates whether this module should be uploaded as a System Module.", Access = GH_ParamAccess.item, Optional = true},
            new Param_String() { Name = "Module Name", NickName = "MN", Description = "The name of the module as a text. The default name is MainModule.", Access = GH_ParamAccess.item, Optional = true},
            new Param_String() { Name = "Procedure Name", NickName = "RN", Description = "The name of the RAPID routine as a text. The default name is main.", Access = GH_ParamAccess.item, Optional = true},
            new Param_Integer() { Name = "Routine Scope", NickName = "RS", Description = "The scope of the RAPID routine. Use 0 for GLOBAL scope, 1 for LOCAL scope and 2 for TASK scope.", Access = GH_ParamAccess.item, Optional = true},
            new Param_String() { Name = "Superordinate Main Method", NickName = "MM", Description = "The contents of the superordinate main method if this routine is called from another routine.", Access = GH_ParamAccess.list, Optional = true},
            new Param_Routine() { Name = "Additional Routines", NickName = "AR", Description = "Additional routines to be added to the RAPID module.", Access = GH_ParamAccess.list, Optional = true},
            new Param_Boolean() { Name = "Add loaddata", NickName = "AL", Description = "Indicates if the loaddata should be added to the RAPID module.", Access = GH_ParamAccess.item, Optional = true},
            new Param_Boolean() { Name = "Add tooldata", NickName = "AT", Description = "Indicates if the tooldata should be added to the RAPID module.", Access = GH_ParamAccess.item, Optional = true},
            new Param_Boolean() { Name = "Add wobjdata", NickName = "AW", Description = "Indicates if the wobjdata should be added the RAPID module.", Access = GH_ParamAccess.item, Optional = true},
            new Param_Boolean() { Name = "Update", NickName = "U", Description = "Updates the RAPID module based on a boolean value. To increase performance, only update when changes were made.", Access = GH_ParamAccess.item, Optional = true }
        };

        /// <summary>
        /// Stores the variable output parameters in an array.
        /// </summary>
        private readonly IGH_Param[] _variableOutputParameters = new IGH_Param[3]
        {
            new Param_String() { Name = "Loaddata", NickName = "L", Description = "The RAPID loaddata as a list with text.", Access = GH_ParamAccess.list},
            new Param_String() { Name = "Tooldata", NickName = "T", Description = "The RAPID tooldata as a list with text.", Access = GH_ParamAccess.list},
            new Param_String() { Name = "Wobjdata", NickName = "W", Description = "The RAPID wobjdata as a list with text.", Access = GH_ParamAccess.list}
        };

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Robot(), "Robot", "R", "Robot that is used as Robot.", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Action(), "Actions", "A", "Actions as list of instructive and declarative Actions.", GH_ParamAccess.list);

            AddInputParameter(9);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Module", "M", "The RAPID Module as list with strings", GH_ParamAccess.list);
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
            bool isSystemModule = false;
            string moduleName = "MainModule";
            string routineName = "main";
            int scope = (int)Scope.GLOBAL;
            List<string> mainModule = new List<string>();
            List<Routine> additionalRoutines = new List<Routine>();
            bool addTooldata = true;
            bool addLoaddata = true;
            bool addWobjdata = true;
            bool update = true;

            // Catch the input data
            if (!DA.GetData(0, ref robot)) { return; }
            if (!DA.GetDataList(1, actions)) { return; }

            // Creates the input value list for the scope and attachs it to the input parameter
            int scopeParamIndex = Params.Input.FindIndex(x => x.Name == _variableInputParameters[2].Name);
            if (_routineScopeParam && scopeParamIndex != -1 && this.Params.Input[scopeParamIndex].SourceCount == 0)
            {
                HelperMethods.CreateValueList(this, typeof(Scope), scopeParamIndex);
                this.ExpireSolution(true);
            }

            // Catch the input data from the variable parameteres
            if (Params.Input.Any(x => x.Name == _variableInputParameters[0].Name))
            {
                if (!DA.GetData(_variableInputParameters[0].Name, ref isSystemModule))
                {
                    isSystemModule = false;
                }
            }
            if (Params.Input.Any(x => x.Name == _variableInputParameters[1].Name))
            {
                if (!DA.GetData(_variableInputParameters[1].Name, ref moduleName))
                {
                    moduleName = "MainModule";
                }
            }
            if (Params.Input.Any(x => x.Name == _variableInputParameters[2].Name))
            {
                if (!DA.GetData(_variableInputParameters[2].Name, ref routineName))
                {
                    routineName = "main";
                }
            }
            if (Params.Input.Any(x => x.Name == _variableInputParameters[3].Name))
            {
                if (!DA.GetData(_variableInputParameters[3].Name, ref scope))
                {
                    scope = (int)Scope.GLOBAL;
                }
            }
            if (Params.Input.Any(x => x.Name == _variableInputParameters[4].Name))
            {
                if (!DA.GetDataList(_variableInputParameters[4].Name, mainModule))
                {
                    mainModule = null;
                }
            }
            if (Params.Input.Any(x => x.Name == _variableInputParameters[5].Name))
            {
                if (!DA.GetDataList(_variableInputParameters[5].Name, additionalRoutines))
                {
                    additionalRoutines = new List<Routine>();
                }
            }
            if (Params.Input.Any(x => x.Name == _variableInputParameters[6].Name))
            {
                if (!DA.GetData(_variableInputParameters[6].Name, ref addLoaddata))
                {
                    addLoaddata = true;
                }
            }
            if (Params.Input.Any(x => x.Name == _variableInputParameters[7].Name))
            {
                if (!DA.GetData(_variableInputParameters[7].Name, ref addTooldata))
                {
                    addTooldata = true;
                }
            }
            if (Params.Input.Any(x => x.Name == _variableInputParameters[8].Name))
            {
                if (!DA.GetData(_variableInputParameters[8].Name, ref addWobjdata))
                {
                    addWobjdata = true;
                }
            }
            if (Params.Input.Any(x => x.Name == _variableInputParameters[9].Name))
            {
                if (!DA.GetData(_variableInputParameters[9].Name, ref update))
                {
                    update = true;
                }
            }

            // Checks if module name exceeds max character limit for RAPID Code
            if (HelperMethods.StringExeedsCharacterLimit32(moduleName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The module name exceeds the character limit of 32 characters.");
            }
            if (HelperMethods.StringExeedsCharacterLimit32(routineName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The routine name exceeds the character limit of 32 characters.");
            }

            // Checks if module name starts with a number
            if (HelperMethods.StringStartsWithNumber(moduleName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The module name starts with a number which is not allowed in RAPID code.");
            }
            if (HelperMethods.StringStartsWithNumber(routineName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The routine name starts with a number which is not allowed in RAPID code.");
            }

            // Check if module name contains special character
            if (HelperMethods.StringStartsWithNumber(moduleName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The module name contains special characters which is not allowed in RAPID code.");
            }
            if (HelperMethods.StringStartsWithNumber(routineName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The routine name contains special characters which is not allowed in RAPID code.");
            }

            //Checks for System Module
            if (isSystemModule && moduleName == "MainModule")
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The module name 'MainModule' is not allowed in System Modules. Please change the module name.");
            }
            if (isSystemModule && routineName == "main")
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The routine name 'main' is not allowed in System Modules. Please change the routine name.");
            }

            // Updates the rapid Progam and System code
            if (update == true)
            {

                if (DA.Iteration == 0)
                    _rapidGenerators.Clear();

                // Initiaties the rapidGenerator
                _rapidGenerator = new RAPIDGenerator(robot, moduleName, routineName, (Scope)scope, mainModule, additionalRoutines, isSystemModule);

                // Generator code
                _rapidGenerator.CreateModule(actions, addTooldata, addWobjdata, addLoaddata);

                // Check if the first movement is an absolute joint movement. 
                _firstMovementIsMoveAbsJ = _rapidGenerator.IsFirstMovementMoveAbsJ;

                //Add current generator for retreiving multiple results
                _rapidGenerators.Add(_rapidGenerator);
            }

            // Checks if first Movement is MoveAbsJ
            if (_firstMovementIsMoveAbsJ == false)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The first movement is not set as an absolute joint movement.");
            }

            // Show warning messages
            if (_rapidGenerator.ErrorText.Count != 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Only axis values of absolute joint movements are checked.");

                for (int i = 0; i < _rapidGenerator.ErrorText.Count; i++)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, _rapidGenerator.ErrorText[i]);
                    if (i == 30) { break; }
                }
            }

            // Fixed output parameter
            DA.SetDataList(0, _rapidGenerators[DA.Iteration].Module);

            // Variable output parameters
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[0].NickName)))
            {
                int ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[0].NickName));
                DA.SetDataList(ind, _rapidGenerators[DA.Iteration].ProgramDeclarationsLoadData);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[1].NickName)))
            {
                int ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[1].NickName));
                DA.SetDataList(ind, _rapidGenerators[DA.Iteration].ProgramDeclarationsToolData);
            }
            if (Params.Output.Any(x => x.NickName.Equality(_variableOutputParameters[2].NickName)))
            {
                int ind = Params.Output.FindIndex(x => x.NickName.Equality(_variableOutputParameters[2].NickName));
                DA.SetDataList(ind, _rapidGenerators[DA.Iteration].ProgramDeclarationsWorkObjectData);
            }
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            // Always place the RAPID generator in the last sub category
            get { return GH_Exposure.septenary; }
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
            get { return Properties.Resources.RAPID_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0BA943C5-D728-4FBC-861B-07F1EB184AFD"); }
        }
        #endregion

        #region menu items
        /// <summary>
        /// Adds the additional items to the context menu of the component. 
        /// </summary>
        /// <param name="menu"> The context menu of the component. </param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Is System Module", MenuItemClickIsSystemModule, true, _isSystemModuleInputParam);
            Menu_AppendItem(menu, "Overwrite Module Name", MenuItemClickModuleName, true, _moduleNameInputParam);
            Menu_AppendItem(menu, "Overwrite Routine Name", MenuItemClickRoutineName, true, _routineNameInputParam);
            Menu_AppendItem(menu, "Specify Routine Scope", MenuItemClickRoutineScope, true, _routineScopeParam);
            Menu_AppendItem(menu, "Add Superordinate Main Method", MenuItemClickMainModule, true, _addMainModuleInputParam);
            Menu_AppendItem(menu, "Add Additional Routines", MenuItemClickAdditionalRoutines, true, _addAdditionalRoutinesInputParam);
            Menu_AppendItem(menu, "Add Load Data", MenuItemClickLoaddata, true, _addLoaddataInputParam);
            Menu_AppendItem(menu, "Add Tool Data", MenuItemClickTooldata, true, _addTooldataInputParam);
            Menu_AppendItem(menu, "Add Work Object Data", MenuItemClickWobjdata, true, _addWobjdataInputParam);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Output Load Data", MenuItemClickOutputLoaddata, true, _loaddataOutputParam);
            Menu_AppendItem(menu, "Output Tool Data", MenuItemClickOutputTooldata, true, _tooldataOutputParam);
            Menu_AppendItem(menu, "Output Work Object Data", MenuItemClickOutputWobjdata, true, _wobjdataOutputParam);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Save RAPID module to file", MenuItemClickSaveModule);

            base.AppendAdditionalComponentMenuItems(menu);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Is System Module" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickIsSystemModule(object sender, EventArgs e)
        {
            RecordUndoEvent("Overwrite Is System Module");
            _isSystemModuleInputParam = !_isSystemModuleInputParam;
            AddInputParameter(0);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Overwrite Module Name" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickModuleName(object sender, EventArgs e)
        {
            RecordUndoEvent("Overwrite Module Name");
            _moduleNameInputParam = !_moduleNameInputParam;
            AddInputParameter(1);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Set Procedure Name" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickRoutineName(object sender, EventArgs e)
        {
            RecordUndoEvent("Overwrite Routine Name");
            _routineNameInputParam = !_routineNameInputParam;
            AddInputParameter(2);
        }

        private void MenuItemClickRoutineScope(object sender, EventArgs e)
        {
            RecordUndoEvent("Specify Routine Scope");
            _routineScopeParam = !_routineScopeParam;
            AddInputParameter(3);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Add Superordinate Main Method" is clicked.
        /// </summary>
        /// <param name="sender">The object that raises the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuItemClickMainModule(object sender, EventArgs e)
        {
            RecordUndoEvent("Add Superordinate Main Method");
            _addMainModuleInputParam = !_addMainModuleInputParam;
            AddInputParameter(4);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Add Additional Routines" is clicked.
        /// </summary>
        /// <param name="sender">The object that raises the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuItemClickAdditionalRoutines(object sender, EventArgs e)
        {
            RecordUndoEvent("Add Additional Routines");
            _addAdditionalRoutinesInputParam = !_addAdditionalRoutinesInputParam;
            AddInputParameter(5);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Add Load Data" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickLoaddata(object sender, EventArgs e)
        {
            RecordUndoEvent("Add Load Data");
            _addLoaddataInputParam = !_addLoaddataInputParam;
            AddInputParameter(6);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Add Tool Data" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickTooldata(object sender, EventArgs e)
        {
            RecordUndoEvent("Add Tool Data");
            _addTooldataInputParam = !_addTooldataInputParam;
            AddInputParameter(7);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Add Work Object Data" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickWobjdata(object sender, EventArgs e)
        {
            RecordUndoEvent("Add Work Object Data");
            _addWobjdataInputParam = !_addWobjdataInputParam;
            AddInputParameter(8);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Output Load Data" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickOutputLoaddata(object sender, EventArgs e)
        {
            RecordUndoEvent("Output Load Data");
            _loaddataOutputParam = !_loaddataOutputParam;
            AddOutputParameter(0);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Output Tool Data" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickOutputTooldata(object sender, EventArgs e)
        {
            RecordUndoEvent("Output Tool Data");
            _tooldataOutputParam = !_tooldataOutputParam;
            AddOutputParameter(1);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Output Work Object Data" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickOutputWobjdata(object sender, EventArgs e)
        {
            RecordUndoEvent("Output Work Object Data");
            _wobjdataOutputParam = !_wobjdataOutputParam;
            AddOutputParameter(2);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Save to Program module to file" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickSaveModule(object sender, EventArgs e)
        {
            SaveModule();
        }

        /// <summary>
        /// Save Program module to file
        /// </summary>
        private void SaveModule()
        {
            // Create save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt = "mod",
                Filter = "RAPID Module|*.mod",
                Title = "Save a RAPID Module"
            };

            // If result of dialog is OK the file can be saved
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Check the file name
                if (saveFileDialog.FileName != "")
                {
                    // Write RAPID code to file
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false))
                    {
                        for (int i = 0; i != _rapidGenerator.Module.Count; i++)
                        {
                            writer.WriteLine(_rapidGenerator.Module[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="writer"> Provides access to a subset of GH_Chunk methods used for writing archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("Is System Module", _isSystemModuleInputParam);
            writer.SetBoolean("Module Name", _moduleNameInputParam);
            writer.SetBoolean("Routine Name", _routineNameInputParam);
            writer.SetBoolean("Main Module", _addMainModuleInputParam);
            writer.SetBoolean("Additional Routines", _addAdditionalRoutinesInputParam);
            writer.SetBoolean("Routine Scope", _routineScopeParam);
            writer.SetBoolean("Add loaddata", _addLoaddataInputParam);
            writer.SetBoolean("Add tooldata", _addTooldataInputParam);
            writer.SetBoolean("Add wobjdata", _addWobjdataInputParam);
            writer.SetBoolean("Output loaddata", _loaddataOutputParam);
            writer.SetBoolean("Output tooldata", _tooldataOutputParam);
            writer.SetBoolean("Output wobjdata", _wobjdataOutputParam);
            return base.Write(writer);
        }

        /// <summary>
        /// Read our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="reader"> Provides access to a subset of GH_Chunk methods used for reading archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Read(GH_IReader reader)
        {
            _isSystemModuleInputParam = reader.GetBoolean("Is System Module");
            _moduleNameInputParam = reader.GetBoolean("Module Name");
            _routineNameInputParam = reader.GetBoolean("Routine Name");
            _addMainModuleInputParam = reader.GetBoolean("Main Module");
            _addAdditionalRoutinesInputParam = reader.GetBoolean("Additional Routines");
            _routineScopeParam = reader.GetBoolean("Routine Scope");
            _addLoaddataInputParam = reader.GetBoolean("Add loaddata");
            _addTooldataInputParam = reader.GetBoolean("Add tooldata");
            _addWobjdataInputParam = reader.GetBoolean("Add wobjdata");
            _loaddataOutputParam = reader.GetBoolean("Output loaddata");
            _tooldataOutputParam = reader.GetBoolean("Output tooldata");
            _wobjdataOutputParam = reader.GetBoolean("Output wobjdata");
            return base.Read(reader);
        }

        /// <summary>
        /// Adds or destroys the input parameter to the component.
        /// </summary>
        /// <param name="index"> The index number of the parameter that needs to be added. </param>
        private void AddInputParameter(int index)
        {
            // Pick the parameter
            IGH_Param parameter = _variableInputParameters[index];
            string name = _variableInputParameters[index].Name;

            // If the parameter already exist: remove it
            if (Params.Input.Any(x => x.Name == name))
            {
                Params.UnregisterInputParameter(Params.Input.First(x => x.Name == name), true);
            }

            // Else remove the variable input parameter
            else
            {
                // The index where the parameter should be added
                int insertIndex = _fixedParamNumInput;

                // Check if other parameters are already added and correct the insert index
                for (int i = 0; i < index; i++)
                {
                    if (Params.Input.Any(x => x.Name == _variableInputParameters[i].Name))
                    {
                        insertIndex += 1;
                    }
                }

                // Register the input parameter
                Params.RegisterInputParam(parameter, insertIndex);
            }

            // Expire solution and refresh parameters since they changed
            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        /// <summary>
        /// Adds or destroys the output parameter to the component.
        /// </summary>
        /// <param name="index"> The index number of the parameter that needs to be added. </param>
        private void AddOutputParameter(int index)
        {
            // Pick the parameter
            IGH_Param parameter = _variableOutputParameters[index];
            string name = _variableOutputParameters[index].NickName;

            // If the parameter already exist: remove it
            if (Params.Output.Any(x => x.NickName.Equality(parameter.NickName)))
            {
                Params.UnregisterOutputParameter(Params.Output.First(x => x.NickName.Equality(parameter.NickName)), true);
            }

            // Else remove the variable input parameter
            else
            {
                // The index where the parameter should be added
                int insertIndex = _fixedParamNumOutput;

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
        /// <summary>
        /// This function will get called before an attempt is made to insert a parameter. 
        /// Since this method is potentially called on Canvas redraws, it must be fast.
        /// </summary>
        /// <param name="side"> Parameter side (input or output). </param>
        /// <param name="index"> Insertion index of parameter. Index=0 means the parameter will be in the topmost spot. </param>
        /// <returns> Return True if your component supports a variable parameter at the given location </returns>
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        /// <summary>
        /// This function will get called before an attempt is made to insert a parameter. 
        /// Since this method is potentially called on Canvas redraws, it must be fast.
        /// </summary>
        /// <param name="side"> Parameter side (input or output). </param>
        /// <param name="index"> Insertion index of parameter. Index=0 means the parameter will be in the topmost spot. </param>
        /// <returns> Return True if your component supports a variable parameter at the given location. </returns>
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        /// <summary>
        /// This function will be called when a new parameter is about to be inserted. 
        /// You must provide a valid parameter or insertion will be skipped. 
        /// You do not, repeat not, need to insert the parameter yourself.
        /// </summary>
        /// <param name="side"> Parameter side (input or output). </param>
        /// <param name="index"> Insertion index of parameter. Index=0 means the parameter will be in the topmost spot. </param>
        /// <returns> A valid IGH_Param instance to be inserted. In our case a null value. </returns>
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            return null;
        }

        /// <summary>
        /// This function will be called when a parameter is about to be removed. 
        /// You do not need to do anything, but this would be a good time to remove 
        /// any event handlers that might be attached to the parameter in question.
        /// </summary>
        /// <param name="side"> Parameter side (input or output). </param>
        /// <param name="index"> Insertion index of parameter. Index=0 means the parameter will be in the topmost spot. </param>
        /// <returns> Return True if the parameter in question can indeed be removed. Note, this is only in emergencies, 
        /// typically the CanRemoveParameter function should return false if the parameter in question is not removable. </returns>
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        /// <summary>
        /// This method will be called when a closely related set of variable parameter operations completes. 
        /// This would be a good time to ensure all Nicknames and parameter properties are correct. 
        /// This method will also be called upon IO operations such as Open, Paste, Undo and Redo.
        /// </summary>
        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {

        }
        #endregion
    }
}
