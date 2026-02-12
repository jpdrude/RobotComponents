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
using RobotComponents.ABB.Definitions;

namespace RobotComponents.ABB.Actions.Instructions
{
    /// <summary>
    /// Represents a Set Group Output instruction. 
    /// </summary>
    /// <remarks>
    /// This action is used to set the value of an analog output signal.
    /// </remarks>
    [Serializable()]
    public class SetGroupOutput : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        private int _value;
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.  
        /// </summary>
        /// <param name="info"> The SerializationInfo to extract the data from. </param>
        /// <param name="context"> The context of this deserialization. </param>
        protected SetGroupOutput(SerializationInfo info, StreamingContext context)
        {
            // // Version version = (int)info.GetValue("Version", typeof(Version)); // <-- use this if the (de)serialization changes
            _name = (string)info.GetValue("Name", typeof(string));
            _value = (int)info.GetValue("Value", typeof(int));
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
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("Value", _value, typeof(int));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes an empty instance of the Set Group Output class.
        /// </summary>
        public SetGroupOutput()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Set Group Output class.
        /// </summary>
        /// <param name="name"> The name of the Group Output signal. </param>
        /// <param name="value"> The desired value of the signal. </param>
        public SetGroupOutput(string name, int value)
        {
            _name = name;
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the Set Group Output class by duplicating an existing Set Group Output instance. 
        /// </summary>
        /// <param name="setGroupOutput"> The Set Group Output instance to duplicate. </param>
        public SetGroupOutput(SetGroupOutput setGroupOutput)
        {
            _name = setGroupOutput.Name;
            _value = setGroupOutput.Value;
        }

        /// <summary>
        /// Returns an exact duplicate of this Set Group Output instance.
        /// </summary>
        /// <returns> 
        /// A deep copy of the Set Group Output instance. 
        /// </returns>
        public SetGroupOutput Duplicate()
        {
            return new SetGroupOutput(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Set Group Output instance as IInstruction.
        /// </summary>
        /// <returns> 
        /// A deep copy of the Set Group Output instance as an IInstruction. 
        /// </returns>
        public IInstruction DuplicateInstruction()
        {
            return new SetGroupOutput(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Set Group Output instance as an Action. 
        /// </summary>
        /// <returns> 
        /// A deep copy of the Set Group Output instance as an Action. 
        /// </returns>
        public IAction DuplicateAction()
        {
            return new SetGroupOutput(this);
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
            if (_name == null)
            {
                return "Empty Set Group Output";
            }
            else if (!IsValid)
            {
                return "Invalid Set Group Output";
            }
            else
            {
                return $"Set Group Output ({_name}\\{_value})";
            }
        }

        /// <summary>
        /// Returns the RAPID declaration code line of the this action.
        /// </summary>
        /// <param name="robot"> The Robot were the code is generated for. </param>
        /// <returns> 
        /// An empty string. 
        /// </returns>
        public string ToRAPIDDeclaration(Robot robot)
        {
            return string.Empty;
        }

        /// <summary>
        /// Returns the RAPID instruction code line of the this action. 
        /// </summary>
        /// <param name="robot"> The Robot were the code is generated for. </param>
        /// <returns> 
        /// The RAPID code line. 
        /// </returns>
        public string ToRAPIDInstruction(Robot robot)
        {
            return $"SetGO {_name}, {_value};";
        }

        /// <summary>
        /// Creates declarations and instructions in the RAPID program module inside the RAPID Generator.
        /// </summary>
        /// <remarks>
        /// This method is called inside the RAPID generator.
        /// </remarks>
        /// <param name="RAPIDGenerator"> The RAPID Generator. </param>
        public void ToRAPIDGenerator(RAPIDGenerator RAPIDGenerator)
        {
            RAPIDGenerator.ProgramInstructions.Add("    " + "    " + ToRAPIDInstruction(RAPIDGenerator.Robot));
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
                if (_name == null) { return false; }
                if (_name == "") { return false; }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the name of the group output signal.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the value of the group output signal.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }
        #endregion
    }
}