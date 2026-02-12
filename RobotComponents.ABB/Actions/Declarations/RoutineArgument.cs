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
using System.Runtime.Serialization;
using System.Security.Permissions;

// RobotComponents Libs
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Actions.Declarations
{
    /// <summary>
    /// Represents an argument for a custom (user definied) RAPID Routine.
    /// </summary>
    [Serializable()]
    public class RoutineArgument : ISerializable
    {
        #region fields
        private string _keyword;
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
        protected RoutineArgument(SerializationInfo info, StreamingContext context)
        {
            // Version version = (int)info.GetValue("Version", typeof(Version)); // <-- use this if the (de)serialization changes
            _keyword = (string)info.GetValue("Argument Keyword", typeof(string));
            _type = (string)info.GetValue("Argument Data Type", typeof(string));
            _name = (string)info.GetValue("Argument Name", typeof(string));
            _value = (string)info.GetValue("Argument Value", typeof(string));
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
            info.AddValue("Argument Keyword", _keyword, typeof(string));
            info.AddValue("Argument Data Type", _type, typeof(string));
            info.AddValue("Argument Name", _name, typeof(string));
            info.AddValue("Argument Value", _value, typeof(string));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a empty instance of the RoutineArgument class.
        /// </summary>
        public RoutineArgument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RoutineArgument class with the value set to null.
        /// </summary>
        /// <param name="type"> The data type of the argument. </param>
        /// <param name="name"> The identifier of the argument. </param>
        public RoutineArgument(string type, string name)
        {
            _type = type;
            _name = name;
            _value = "null";
            _keyword = null;
        }

        /// <summary>
        /// Initializes a new instance of the RoutineArgument class.
        /// </summary>
        /// <param name="type"> The data type of the argument. </param>
        /// <param name="name"> The identifier of the argument. </param>
        /// <param name="value"> The value of the argument. </param>
        /// <param name="keyword"> The keyword of the argument (e.g. INOUT, VAR, PERS). </param>
        public RoutineArgument(string type, string name, string value, string keyword = null)
        {
            _type = type;
            _name = name;
            _value = value;
            _keyword = keyword;
        }


        /// <summary>
        /// Initializes a new instance of the RoutineArgument class by duplicating an existing RoutineArgument instance. 
        /// </summary>
        /// <param name="routineArgument"> The Routine instance to duplicate. </param>
        public RoutineArgument(RoutineArgument routineArgument)
        {
            _type = routineArgument._type;
            _name = routineArgument._name;
            _value = routineArgument._value;
            _keyword = routineArgument._keyword;
        }

        /// <summary>
        /// Returns an exact duplicate of this RoutineArgument instance.
        /// </summary>
        /// <returns> 
        /// A deep copy of the RoutineArgument instance. 
        /// </returns>
        public RoutineArgument Duplicate()
        {
            return new RoutineArgument(this);
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
            string str = $"{_keyword} {_type} {_name} = {_value}";
            str = str.Trim();
            return str;
        }

        /// <summary>
        /// Returns a string used for Routine Declarations.
        /// </summary>
        /// <returns> String used for Routine Declarations. </returns>
        public string ToDeclString()
        {
            string str = $"{_keyword} {_type} {_name}";
            str = str.Trim();
            return str;
        }

        /// <summary>
        /// Returns a string used for Routine Call.
        /// </summary>
        /// <returns> String used for Routine Call. </returns>
        public string ToCallString()
        {
            string str = $"{_keyword} {_value}";
            str = str.Trim();
            return str;
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
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the Argument Keyword.
        /// </summary>
        public string Keyword
        {
            get { return _keyword; }
            set { _keyword = value; }
        }

        /// <summary>
        /// Gets or sets the Argument value.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        #endregion
    }
}