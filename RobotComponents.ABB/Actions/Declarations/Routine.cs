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
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

// RobotComponents Libs
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Actions.Declarations
{
    /// <summary>
    /// Represents a custom (user definied) RAPID Routine.
    /// </summary>
    [Serializable()]
    public class Routine : ISerializable
    {
        #region fields
        private List<IAction> _actions;
        private RoutineType _type;
        private string _name;
        private Scope _scope;
        private List<RoutineArgument> _arguments;
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.  
        /// </summary>
        /// <param name="info"> The SerializationInfo to extract the data from. </param>
        /// <param name="context"> The context of this deserialization. </param>
        protected Routine(SerializationInfo info, StreamingContext context)
        {
            // Version version = (int)info.GetValue("Version", typeof(Version)); // <-- use this if the (de)serialization changes
            _actions = (List<IAction>)info.GetValue("Routine Content", typeof(List<IAction>));
            _type = (RoutineType)info.GetValue("Routine Type", typeof(RoutineType));
            _name = (string)info.GetValue("Routine Name", typeof(string));
            _scope = (Scope)info.GetValue("Routine Scope", typeof(Scope));
            _arguments = (List<RoutineArgument>)info.GetValue("Routine Arguments", typeof(List<RoutineArgument>));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the object.
        /// </summary>
        /// <param name="info"> The SerializationInfo to populate with data. </param>
        /// <param name="context"> The destination for this serialization. </param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Routine Content", _actions, typeof(List<IAction>));
            info.AddValue("Routine Type", _type, typeof(RoutineType));
            info.AddValue("Routine Name", _name, typeof(string));
            info.AddValue("Routine Scope", _scope, typeof(Scope));
            info.AddValue("Routine Arguments", _arguments, typeof(List<RoutineArgument>));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a empty instance of the Routine class.
        /// </summary>
        public Routine()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Routine class with the Routine Type set as PROC.
        /// </summary>
        /// <param name="actions"> The content of the routine. </param>
        /// <param name="name"> The identifier of the routine. </param>
        public Routine(List<IAction> actions, string name)
        {
            _actions = actions;
            _type = RoutineType.PROC;
            _name = name;
            _scope = Scope.GLOBAL;
        }

        /// <summary>
        /// Initializes a new instance of the Routine class
        /// </summary>
        /// <param name="actions"> The content of the routine </param>
        /// <param name="type"> The routine Type (PROC, TRAP). </param>
        /// <param name="name"> The identifier of the routine. </param>
        /// <param name="scope"> The scope of the routine. </param>
        /// <param name="arguments"> Optional arguments for the routine. </param>
        public Routine(List<IAction> actions, RoutineType type, string name, Scope scope = Scope.GLOBAL, List<RoutineArgument> arguments = null)
        {
            _actions = actions;
            _type = type;
            _name = name;
            _scope = scope;
            _arguments = arguments;

            if (arguments != null && arguments.Count == 0)
                arguments = null;
        }


        /// <summary>
        /// Initializes a new instance of the Routine class by duplicating an existing Routine instance. 
        /// </summary>
        /// <param name="routine"> The Routine instance to duplicate. </param>
        public Routine(Routine routine)
        {
            _actions = new List<IAction>();

            foreach (IAction action in routine._actions)
            {
                _actions.Add(action.DuplicateAction());
            }

            _type = routine._type;
            _name = routine._name;
            _scope = routine._scope;
            _arguments = null;

            if (routine._arguments != null)
            {
                _arguments = new List<RoutineArgument>();
                foreach (RoutineArgument arg in routine._arguments)
                {
                    _arguments.Add(arg.Duplicate());
                }
            }
        }

        /// <summary>
        /// Returns an exact duplicate of this Routine instance.
        /// </summary>
        /// <returns> 
        /// A deep copy of the Routine instance. 
        /// </returns>
        public Routine Duplicate()
        {
            return new Routine(this);
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
            string emptyCheck = "";

            if (_actions == null || _actions.Count == 0)
            {
                emptyCheck = "Empty ";
            }

            if (!IsValid)
            {
               return "Invalid Routine";
            }

            if (_type == RoutineType.PROC)
            {
                return emptyCheck + "Procedure";
            }
            else if (_type == RoutineType.TRAP)
            {
                return emptyCheck + "Trap";
            }
            else
            {
                return "Invalid Routine";
            }
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
                if (_name == null || _name == "")
                {
                    return false;
                }
                if (_type < RoutineType.PROC || _type > RoutineType.TRAP)
                {
                    return false;
                }
                if (_scope < Scope.GLOBAL || _scope > Scope.TASK)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the custom RAPID Routine content.
        /// </summary>
        public List<IAction> Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }

        /// <summary>
        /// Gets or sets the Routine Type.
        /// </summary>
        public RoutineType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Gets or sets the Routine Name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the Routine Scope.
        /// </summary>
        public Scope RoutineScope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        /// <summary>
        /// Gets the Routine Arguments.
        /// </summary>
        public List<RoutineArgument> Arguments
        { 
            get { return _arguments; } 
        }
        #endregion
    }
}