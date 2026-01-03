// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2018-2020 EDEK Uni Kassel
// Copyright (c) 2020-2024 Arjen Deetman
//
// Authors:
//   - Gabriel Rumph (2018-2020)
//   - Benedikt Wannemacher (2018-2020)
//   - Arjen Deetman (2019-2024)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using Microsoft.SqlServer.Server;
using Rhino.Render;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Actions.Instructions;
// RobotComponents Libs
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using System.Collections.Generic;
using System.IO;

namespace RobotComponents.ABB.Actions
{
    /// <summary>
    /// Represents the RAPID Generator.
    /// </summary>
    /// <remarks>
    /// This is class is used to generate the RAPID module from a given set of actions.
    /// </remarks>
    public class RAPIDGenerator
    {
        #region fields
        private Robot _robot;

        // Dictionaries that collect all declarations that have a variable name defined
        private readonly Dictionary<string, SpeedData> _speedDatas = new Dictionary<string, SpeedData>();
        private readonly Dictionary<string, ConfigurationData> _configurationDatas = new Dictionary<string, ConfigurationData>();
        private readonly Dictionary<string, ZoneData> _zoneDatas = new Dictionary<string, ZoneData>();
        private readonly Dictionary<string, RobotTool> _robotTools = new Dictionary<string, RobotTool>();
        private readonly Dictionary<string, LoadData> _loadDatas = new Dictionary<string, LoadData>();
        private readonly Dictionary<string, WorkObject> _workObjects = new Dictionary<string, WorkObject>();
        private readonly Dictionary<string, TaskList> _taskLists = new Dictionary<string, TaskList>();
        private readonly Dictionary<string, IJointPosition> _jointPositions = new Dictionary<string, IJointPosition>();
        private readonly Dictionary<string, ITarget> _targets = new Dictionary<string, ITarget>();
        private readonly Dictionary<string, ISyncident> _syncidents = new Dictionary<string, ISyncident>();

        // Collections with different types of RAPID code lines
        private readonly List<string> _programDeclarations = new List<string>();
        private readonly List<string> _programDeclarationsLoadData = new List<string>();
        private readonly List<string> _programDeclarationsToolData = new List<string>();
        private readonly List<string> _programDeclarationsWorkObjectData = new List<string>();
        private readonly List<string> _programDeclarationsComments = new List<string>();
        private readonly List<string> _programDeclarationsCustom = new List<string>();
        private readonly List<string> _programDeclarationsMultiMove = new List<string>();
        private readonly List<string> _programInstructions = new List<string>();

        // The RAPID module
        private readonly List<string> _module = new List<string>();
        private string _moduleName;
        private string _procedureName;
        private string _localRoutine = "";
        private HashSet<string> _globalDeclarations = new HashSet<string>();

        // Checks
        private readonly List<string> _errorText = new List<string>();
        private bool _isFirstMovementMoveAbsJ;
        private bool _isSynchronized = false;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes an empty instance of the RAPID Generator class.
        /// </summary>
        public RAPIDGenerator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RAPID Generator class with a main routine.
        /// </summary>
        /// <param name="robot"> The robot info wherefore the code should be created. </param>
        public RAPIDGenerator(Robot robot)
        {
            _robot = robot.Duplicate(); // Since we might swap tools and therefore change the robot tool we make a deep copy
            _moduleName = "MainModule";
            _procedureName = "main";
        }

        /// <summary>
        /// Initializes a new instance of the RAPID Generator class with custom names.
        /// </summary>
        /// <param name="robot"> The robot info wherefore the code should be created. </param>
        /// <param name="moduleName"> The name of the program module </param>
        /// <param name="routineName"> The name of the RAPID procedure </param>
        /// <param name="localRoutine"> Specifies whether the RAPID procedure is declared as LOCAL. </param>
        /// <param name="mainModule"> Optionally provides a Main Module whose global declarations are skipped in helper modules.</param>
        public RAPIDGenerator(Robot robot, string moduleName, string routineName, bool localRoutine = false, List<string> mainModule = null)
        {
            _robot = robot.Duplicate(); // Since we might swap tools and therefore change the robot tool we make a deep copy
            _moduleName = moduleName;
            _procedureName = routineName;
            if (localRoutine) _localRoutine = "LOCAL";
            else _localRoutine = "";

            if (mainModule != null)
                _globalDeclarations = GetMainModuleDeclarations(mainModule);
        }

        /// <summary>
        /// Initializes a new instance of the RAPID Generator class by duplicating an existing RAPID Generator instance. 
        /// </summary>
        /// <param name="generator"> The RAPID Generator instance to duplicate. </param>
        public RAPIDGenerator(RAPIDGenerator generator)
        {
            _module = generator.Module.ConvertAll(line => line);
            _moduleName = generator.ModuleName;
            _procedureName = generator.ProcedureName;
            _robot = generator.Robot.Duplicate();
            _isFirstMovementMoveAbsJ = generator.IsFirstMovementMoveAbsJ;
            _localRoutine = generator.LocalRoutine;
            _globalDeclarations = generator.GlobalDeclarations;
        }

        /// <summary>
        /// Returns an exact duplicate of this RAPID Generator instance.
        /// </summary>
        /// <returns> 
        /// A deep copy of the RAPID Generator instance. 
        /// </returns>
        public RAPIDGenerator Duplicate()
        {
            return new RAPIDGenerator(this);
        }
        #endregion

        #region method
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns> 
        /// A string that represents the current object. 
        /// </returns>
        public override string ToString()
        {
            if (!IsValid)
            {
                return "Invalid RAPID Generator";
            }
            else
            {
                return "RAPID Generator";
            }
        }

        /// <summary>
        /// Returns the RAPID module.
        /// </summary>
        /// <param name="actions"> The list with robot actions wherefore the code will be created. </param>
        /// <param name="addTooldata"> Specifies if the tooldata should be added to the RAPID module. </param>
        /// <param name="addWobjdata"> Specifies if the wobjdata should be added to the RAPID module. </param>
        /// <param name="addLoaddata"> Specifies if the loaddata should be added to the RAPID module. </param>
        /// <returns> 
        /// The RAPID module as a list with code lines.
        /// </returns>
        public List<string> CreateModule(IList<IAction> actions, bool addTooldata = true, bool addWobjdata = true, bool addLoaddata = true)
        {
            // Reset the fields            
            _programDeclarations.Clear();
            _programDeclarationsToolData.Clear();
            _programDeclarationsLoadData.Clear();
            _programDeclarationsWorkObjectData.Clear();
            _programDeclarationsComments.Clear();
            _programDeclarationsCustom.Clear();
            _programDeclarationsMultiMove.Clear();
            _programInstructions.Clear();

            _configurationDatas.Clear();
            _speedDatas.Clear();
            _jointPositions.Clear();
            _targets.Clear();
            _zoneDatas.Clear();
            _robotTools.Clear();
            _loadDatas.Clear();
            _workObjects.Clear();
            _taskLists.Clear();
            _syncidents.Clear();

            _module.Clear();
            _errorText.Clear();
            _isSynchronized = false;
            _isFirstMovementMoveAbsJ = false;

            #region get data
            _robot.Tool.ToRAPIDGenerator(this);

            // Check if the first movement is an Absolute Joint Movement
            _isFirstMovementMoveAbsJ = CheckFirstMovement(actions);

            // Initial tool
            _robot.Tool.ToRAPIDGenerator(this);

            // Sync ID counter
            int syncID = 10;

            // Creates the code lines
            for (int i = 0; i != actions.Count; i++)
            {
                if (_isSynchronized == true && actions[i] is Movement movement)
                {
                    movement.SyncID = syncID;
                    actions[i].ToRAPIDGenerator(this);
                    movement.SyncID = -1;
                    syncID += 10;
                }
                else
                {
                    actions[i].ToRAPIDGenerator(this);
                }
            }
            #endregion

            #region write the module
            _module.Add($"MODULE {_moduleName}");
            _module.Add("    ");

            // Add comment lines for tracking which version of RC was used
            _module.Add("    " + $"! This RAPID code was generated with RobotComponents v{VersionNumbering.CurrentVersion} (GPL v3)");
            _module.Add("    " + "! Visit www.github.com/RobotComponents for more information");
            _module.Add("    ");

            // Add the comments
            if (_programDeclarationsComments.Count != 0)
            {
                _module.AddRange(_programDeclarationsComments);
                _module.Add("    ");
            }

            // Add loaddata
            if (addLoaddata == true && _programDeclarationsLoadData.Count != 0)
            {
                _programDeclarationsLoadData.Sort();

                List<string> uniqueDecls = new List<string>();

                foreach (string decl in _programDeclarationsLoadData)
                    if (DeclarationIsUnique(decl))
                        uniqueDecls.Add(decl);

                if (uniqueDecls.Count != 0)
                {
                    _module.Add("    " + "! User defined loaddata");
                    _module.AddRange(uniqueDecls);
                    _module.Add("    ");
                }
            }

            // Add the tooldata
            if (addTooldata == true && _programDeclarationsToolData.Count != 0)
            {
                _programDeclarationsToolData.Sort();

                List<string> uniqueDecls = new List<string>();

                foreach (string decl in _programDeclarationsToolData)
                    if (DeclarationIsUnique(decl))
                        uniqueDecls.Add(decl);

                if (uniqueDecls.Count != 0)
                {
                    _module.Add("    " + "! User defined tooldata");
                    _module.AddRange(uniqueDecls);
                    _module.Add("    ");
                }
            }

            // Add the wobjdata
            if (addWobjdata == true && _programDeclarationsWorkObjectData.Count != 0)
            {
                _programDeclarationsWorkObjectData.Sort();

                List<string> uniqueDecls = new List<string>();

                foreach (string decl in _programDeclarationsWorkObjectData)
                    if (DeclarationIsUnique(decl))
                        uniqueDecls.Add(decl);

                if (uniqueDecls.Count != 0)
                {
                    _module.Add("    " + "! User defined wobjdata");
                    _module.AddRange(uniqueDecls);
                    _module.Add("    ");
                }
            }

            // Add the custom Code Lines
            if (_programDeclarationsCustom.Count != 0)
            {
                List<string> uniqueDecls = new List<string>();

                foreach (string decl in _programDeclarationsCustom)
                    if (DeclarationIsUnique(decl))
                        uniqueDecls.Add(decl);

                if (uniqueDecls.Count != 0)
                {
                    _module.Add("    " + "! User definied code lines");
                    _module.AddRange(uniqueDecls);
                    _module.Add("    ");
                }
            }

            // Add the multi move declarations
            if (_programDeclarationsMultiMove.Count != 0)
            {
                _programDeclarationsMultiMove.Sort();

                List<string> uniqueDecls = new List<string>();

                foreach (string decl in _programDeclarationsMultiMove)
                    if (DeclarationIsUnique(decl))
                        uniqueDecls.Add(decl);

                if (uniqueDecls.Count != 0)
                {
                    _module.Add("    " + "! Declarations for multi move programming");
                    _module.AddRange(uniqueDecls);
                    _module.Add("    ");
                }
            }

            // Add all other declarations
            if (_programDeclarations.Count != 0)
            {
                _programDeclarations.Sort();

                List<string> uniqueDecls = new List<string>();

                foreach (string decl in _programDeclarations)
                    if (DeclarationIsUnique(decl))
                        uniqueDecls.Add(decl);

                if (uniqueDecls.Count != 0)
                {
                    _module.Add("    " + "! Declarations generated by Robot Components");
                    _module.AddRange(uniqueDecls);
                    _module.Add("    ");
                }
            }

            // Add the instructions
            if (_programInstructions.Count != 0)
            {
                // Create Program
                _module.Add("    " + $"{_localRoutine} PROC {_procedureName}()");

                // Add instructions
                _module.AddRange(_programInstructions);

                // Closes Program
                _module.Add("    " + "ENDPROC");
                _module.Add("    ");
            }

            // Close / end
            _module.Add("ENDMODULE");
            #endregion

            return _module;
        }

        /// <summary>
        /// Writes the RAPID module to a file.
        /// </summary>
        /// <param name="path"> The path. </param>
        /// <returns> 
        /// True on success, false on failure. 
        /// </returns>
        public bool WriteModuleToFile(string path)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter($"{path}\\{_moduleName}.mod", false))
                {
                    for (int i = 0; i != _module.Count; i++)
                    {
                        writer.WriteLine(_module[i]);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether the first movement type is an absolute joint movement.
        /// </summary>
        /// <remarks>
        /// If the no movements were defined the method returns true.
        /// </remarks>
        /// <param name="actions"> The list with actions to check. </param>
        /// <returns> 
        /// Specifies whether the first movement type is an absolute joint movement. 
        /// </returns>
        private bool CheckFirstMovement(IList<IAction> actions)
        {
            List<IAction> ungrouped = new List<IAction>() { };

            for (int i = 0; i != actions.Count; i++)
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

            for (int i = 0; i != ungrouped.Count; i++)
            {
                if (ungrouped[i] is Movement movement)
                {
                    if (movement.MovementType == MovementType.MoveAbsJ)
                    {
                        return true;
                    }
                    else
                    {
                        _errorText.Add("The first movement is not set as an absolute joint movement.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a collection of GLOBAL declararions from a mainModule.
        /// </summary>
        /// <param name="mainModule">Main Module as List of RAPID code lines.</param>
        /// <returns>HashSet of used global declaration names for quick lookup.</returns>
        private HashSet<string> GetMainModuleDeclarations(List<string> mainModule)
        {
            // Use case-insensitive set for RAPID identifiers
            HashSet<string> decl = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            foreach (string rawLine in mainModule)
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                string line = rawLine.Trim();

                // Split on whitespace and remove empty entries (handles multiple spaces/tabs)
                string[] parts = line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    continue;

                // Stop collecting when the first PROC is encountered (declarations section finished)
                if (parts[0].ToUpperInvariant() == "PROC")
                    break;

                // GLOBAL declarations: try to find the variable name robustly
                if (parts[0].ToUpperInvariant() == "GLOBAL")
                {
                    string name = null;

                    // Typical forms:
                    // GLOBAL <type> <name> ...
                    // GLOBAL VAR <type> <name> ...
                    // GLOBAL PERS <type> <name> ...
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string token = parts[i];

                        // skip RAPID storage keywords if present
                        if (token.Equals("VAR", System.StringComparison.OrdinalIgnoreCase) ||
                            token.Equals("PERS", System.StringComparison.OrdinalIgnoreCase) ||
                            token.Equals("CONST", System.StringComparison.OrdinalIgnoreCase))
                            continue;

                        // strip trailing punctuation like '[', ';', ':', '='
                        string candidate = token.Split(new char[] { '[', ';', ':', '=' })[0];

                        if (!string.IsNullOrEmpty(candidate))
                        {
                            name = candidate;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(name))
                        decl.Add(name);

                    continue;
                }

                // VAR / PERS / CONST declarations at module level
                string first = parts[0].ToUpperInvariant();
                if (first == "VAR" || first == "PERS" || first == "CONST")
                {
                    if (parts.Length >= 3)
                    {
                        string name = parts[2].Split(new char[] { '[', ';', ':', '=' })[0];
                        if (!string.IsNullOrEmpty(name))
                            decl.Add(name);
                    }
                }
            }

            return decl;
        }

        /// <summary>
        /// Checks whether a declaration is unique or already defined in the MainModule.
        /// </summary>
        /// <param name="decl">Declaration code line.</param>
        /// <returns>True if Unique.</returns>
        private bool DeclarationIsUnique(string decl)
        {
            string name = GetDeclarationName(decl);
            if (name == null) return true;

            // _globalDeclarations was created with case-insensitive comparer in GetMainModuleDeclarations
            return !_globalDeclarations.Contains(name);
        }

        /// <summary>
        /// Seperates the variable name from a declaration.
        /// </summary>
        /// <param name="codeLine">Code line to retreive name from.</param>
        /// <returns> Declaration name string.</returns>
        private string GetDeclarationName(string codeLine)
        {
            if (string.IsNullOrWhiteSpace(codeLine))
                return null;

            string line = codeLine.Trim();
            string[] parts = line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return null;

            string first = parts[0].ToUpperInvariant();

            if (first == "GLOBAL")
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    string token = parts[i];

                    if (token.Equals("VAR", System.StringComparison.OrdinalIgnoreCase) ||
                        token.Equals("PERS", System.StringComparison.OrdinalIgnoreCase) ||
                        token.Equals("CONST", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    string candidate = token.Split(new char[] { '[', ';', ':', '=' })[0];
                    if (!string.IsNullOrEmpty(candidate))
                        return candidate;
                }

                return null;
            }

            if (first == "VAR" || first == "PERS" || first == "CONST")
            {
                if (parts.Length >= 3)
                    return parts[2].Split(new char[] { '[', ';', ':', '=' })[0];
            }

            return null;
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets a value indicating whether or not the object is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (_robot == null) { return false; }
                if (_robot.IsValid == false) { return false; }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the Robot. 
        /// </summary>
        public Robot Robot
        {
            get { return _robot; }
            set { _robot = value; }
        }

        /// <summary>
        /// Gets or sets the name of the RAPID module.
        /// </summary>
        public string ModuleName
        {
            get { return _moduleName; }
            set { _moduleName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the RAPID procedure.
        /// </summary>
        public string ProcedureName
        {
            get { return _procedureName; }
            set { _procedureName = value; }
        }

        /// <summary>
        /// Gets or sets wether the RAPID procedure is declared as LOCAL.
        /// </summary>
        public string LocalRoutine
        {
            get { return _localRoutine; }
            set { _localRoutine = value; }
        }

        /// <summary>
        /// Gets the RAPID module as a list with code lines.
        /// </summary>
        public List<string> Module
        {
            get { return _module; }
        }

        /// <summary>
        /// Gets the global RAPID declarations provided in an optional MainModule.
        /// </summary>
        public HashSet<string> GlobalDeclarations
        {
            get { return _globalDeclarations; }
        }

        /// <summary>
        /// Gets the collected error messages. 
        /// </summary>
        public List<string> ErrorText
        {
            get { return _errorText; }
        }

        /// <summary>
        /// Gets a value indicating whether or not the first movement is an Absolute Joint Movement.
        /// </summary>
        public bool IsFirstMovementMoveAbsJ
        {
            get { return _isFirstMovementMoveAbsJ; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the movements are synchronized. 
        /// </summary>
        /// <remarks>
        /// Value is set inside the SyncMoveOn and SyncMoveOff instructions.
        /// </remarks>
        public bool IsSynchronized
        {
            get { return _isSynchronized; }
            internal set { _isSynchronized = value; }
        }

        /// <summary>
        /// Gets the collection with unique Configuration Datas used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, ConfigurationData> ConfigurationDatas
        {
            get { return _configurationDatas; }
        }

        /// <summary>
        /// Gets the collection with unique Speed Datas used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, SpeedData> SpeedDatas
        {
            get { return _speedDatas; }
        }

        /// <summary>
        /// Gets the collection with unique Zone Datas used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, ZoneData> ZoneDatas
        {
            get { return _zoneDatas; }
        }

        /// <summary>
        /// Gets the collection with unique Joint Positions used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, IJointPosition> JointPositions
        {
            get { return _jointPositions; }
        }

        /// <summary>
        /// Gets the collection with unique Targets used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, ITarget> Targets
        {
            get { return _targets; }
        }

        /// <summary>
        /// Gets the collection with unique Robot Tools used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, RobotTool> RobotTools
        {
            get { return _robotTools; }
        }

        /// <summary>
        /// Gets the collection with unique Load Datas used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, LoadData> LoadDatas
        {
            get { return _loadDatas; }
        }

        /// <summary>
        /// Gets the collection with unique Work Objects used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, WorkObject> WorkObjects
        {
            get { return _workObjects; }
        }

        /// <summary>
        /// Gets the collection with unique Task Lists used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, TaskList> TaskLists
        {
            get { return _taskLists; }
        }

        /// <summary>
        /// Gets the collection with unique syncidents used to create the RAPID program module. 
        /// </summary>
        public Dictionary<string, ISyncident> Syncidents
        {
            get { return _syncidents; }
        }

        /// <summary>
        /// Gets the program declarations as list with RAPID code lines.
        /// </summary>
        public List<string> ProgramDeclarations
        {
            get { return _programDeclarations; }
        }

        /// <summary>
        /// Gets the program declarations commments as list with RAPID code lines.
        /// </summary>
        public List<string> ProgramDeclarationComments
        {
            get { return _programDeclarationsComments; }
        }

        /// <summary>
        /// Gets the program declarations custom code lines as list with RAPID code lines.
        /// </summary>
        public List<string> ProgramDeclarationCustomCodeLines
        {
            get { return _programDeclarationsCustom; }
        }

        /// <summary>
        /// Gets the program declarations for multi move programming as a list with RAPID code lines.
        /// </summary>
        public List<string> ProgramDeclarationsMultiMove
        {
            get { return _programDeclarationsMultiMove; }
        }

        /// <summary>
        /// Gets the RAPID tooldata code lines as list with RAPID code lines.
        /// </summary>
        public List<string> ProgramDeclarationsToolData
        {
            get { return _programDeclarationsToolData; }
        }

        /// <summary>
        /// Gets the RAPID loaddata code lines as list with RAPID code lines.
        /// </summary>
        public List<string> ProgramDeclarationsLoadData
        {
            get { return _programDeclarationsLoadData; }
        }

        /// <summary>
        /// Gets the RAPID wobjdata code lines as list with RAPID code lines.
        /// </summary>
        public List<string> ProgramDeclarationsWorkObjectData
        {
            get { return _programDeclarationsWorkObjectData; }
        }

        /// <summary>
        /// Gets the program instructions as a list with RAPID code lines.
        /// </summary>
        public List<string> ProgramInstructions
        {
            get { return _programInstructions; }
        }
        #endregion
    }
}