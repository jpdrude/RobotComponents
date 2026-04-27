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
using System.Runtime.Serialization;
using System.Security.Permissions;
// RobotComponents Libs
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Actions.Declarations
{
    /// <summary>
    /// Represents a RAPID variable declaration (VAR, PERS, or INOUT).
    /// </summary>
    [Serializable()]
    public class RAPIDVariable : ISerializable
    {
        #region fields
        private RAPIDVariableLevel _level;
        private Scope _scope;
        private RAPIDVariableKeyword _keyword;
        private string _type;
        private string _name;
        private string _value;
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.
        /// </summary>
        /// <param name="info"> The SerializationInfo to extract the data from. </param>
        /// <param name="context"> The context of this deserialization. </param>
        protected RAPIDVariable(SerializationInfo info, StreamingContext context)
        {
            _level = (RAPIDVariableLevel)info.GetValue("Level", typeof(RAPIDVariableLevel));
            _scope = (Scope)info.GetValue("Scope", typeof(Scope));
            _keyword = (RAPIDVariableKeyword)info.GetValue("Keyword", typeof(RAPIDVariableKeyword));
            _type = (string)info.GetValue("Type", typeof(string));
            _name = (string)info.GetValue("Name", typeof(string));
            _value = (string)info.GetValue("Value", typeof(string));
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
            info.AddValue("Level", _level, typeof(RAPIDVariableLevel));
            info.AddValue("Scope", _scope, typeof(Scope));
            info.AddValue("Keyword", _keyword, typeof(RAPIDVariableKeyword));
            info.AddValue("Type", _type, typeof(string));
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("Value", _value, typeof(string));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes an empty instance of the RAPIDVariable class.
        /// </summary>
        public RAPIDVariable()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RAPIDVariable class without an initial value.
        /// </summary>
        /// <param name="level"> The declaration level (Module or Routine). </param>
        /// <param name="scope"> The variable scope (LOCAL or GLOBAL). </param>
        /// <param name="keyword"> The storage keyword (VAR, PERS, or INOUT). </param>
        /// <param name="type"> The RAPID data type (e.g. num, bool). </param>
        /// <param name="name"> The variable name. </param>
        public RAPIDVariable(RAPIDVariableLevel level, Scope scope, RAPIDVariableKeyword keyword, string type, string name)
        {
            _level = level;
            _scope = scope;
            _keyword = keyword;
            _type = type;
            _name = name;
            _value = null;
        }

        /// <summary>
        /// Initializes a new instance of the RAPIDVariable class with an initial value.
        /// </summary>
        /// <param name="level"> The declaration level (Module or Routine). </param>
        /// <param name="scope"> The variable scope (LOCAL or GLOBAL). </param>
        /// <param name="keyword"> The storage keyword (VAR, PERS, or INOUT). </param>
        /// <param name="type"> The RAPID data type (e.g. num, bool). </param>
        /// <param name="name"> The variable name. </param>
        /// <param name="value"> The initial value. </param>
        public RAPIDVariable(RAPIDVariableLevel level, Scope scope, RAPIDVariableKeyword keyword, string type, string name, string value)
        {
            _level = level;
            _scope = scope;
            _keyword = keyword;
            _type = type;
            _name = name;
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the RAPIDVariable class by duplicating an existing instance.
        /// </summary>
        /// <param name="variable"> The RAPIDVariable instance to duplicate. </param>
        public RAPIDVariable(RAPIDVariable variable)
        {
            _level = variable._level;
            _scope = variable._scope;
            _keyword = variable._keyword;
            _type = variable._type;
            _name = variable._name;
            _value = variable._value;
        }

        /// <summary>
        /// Returns an exact duplicate of this RAPIDVariable instance.
        /// </summary>
        /// <returns> A deep copy of the RAPIDVariable instance. </returns>
        public RAPIDVariable Duplicate()
        {
            return new RAPIDVariable(this);
        }
        #endregion

        #region methods
        /// <summary>
        /// Returns the RAPID declaration string for this variable.
        /// </summary>
        /// <returns> A string in the form <c>KEYWORD type name := value;</c>. </returns>
        public string ToRAPID()
        {
            string decl = $"{_keyword} {_type} {_name}";

            if (!string.IsNullOrEmpty(_value))
            {
                decl += $" := {_value}";
            }

            return decl + ";";
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
        {
            return ToRAPID();
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
                return !string.IsNullOrEmpty(_type) && !string.IsNullOrEmpty(_name);
            }
        }

        /// <summary>
        /// Gets or sets the declaration level (Module or Routine).
        /// </summary>
        public RAPIDVariableLevel Level
        {
            get { return _level; }
            set { _level = value; }
        }

        /// <summary>
        /// Gets or sets the variable scope (LOCAL or GLOBAL).
        /// </summary>
        public Scope Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        /// <summary>
        /// Gets or sets the storage keyword (VAR, PERS, or INOUT).
        /// </summary>
        public RAPIDVariableKeyword Keyword
        {
            get { return _keyword; }
            set { _keyword = value; }
        }

        /// <summary>
        /// Gets or sets the RAPID data type.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Gets or sets the variable name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the initial value. Null or empty means no assignment.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        #endregion
    }
}
