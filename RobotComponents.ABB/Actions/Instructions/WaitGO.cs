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
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
// RobotComponents Libs
using RobotComponents.ABB.Definitions;

namespace RobotComponents.ABB.Actions.Instructions
{
    /// <summary>
    /// Represents a Wait for Digital Output instruction.
    /// </summary>
    /// <remarks>
    /// This action is used to wait until a digital output is set.
    /// </remarks>
    [Serializable()]
    public class WaitGO : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        private int _value;
        private double _maxTime;
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.  
        /// </summary>
        /// <param name="info"> The SerializationInfo to extract the data from. </param>
        /// <param name="context"> The context of this deserialization. </param>
        protected WaitGO(SerializationInfo info, StreamingContext context)
        {
            //Version version = (Version)info.GetValue("Version", typeof(Version)); // <-- use this if the (de)serialization changes
            _name = (string)info.GetValue("Name", typeof(string));
            _value = (int)info.GetValue("Value", typeof(int));
            _maxTime = (double)info.GetValue("Max Time", typeof(double));
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
            info.AddValue("Max Time", _maxTime, typeof(double));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes an empty instance of the Wait GO class.
        /// </summary>
        public WaitGO()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Wait DI class.
        /// </summary>
        /// <param name="name"> The name of the signal. </param>
        /// <param name="value"> Specifies the desired value of the Group Output. </param>
        /// <param name="maxTime"> The maximum time to wait in seconds. </param>
        public WaitGO(string name, int value, double maxTime = -1)
        {
            _name = name;
            _value = value;
            _maxTime = maxTime;
        }

        /// <summary>
        /// Initializes a new instance of the Wait GO class by duplicating an existing Wait GO instance. 
        /// </summary>
        /// <param name="waitGO"> The Wait GO instance to duplicate. </param>
        public WaitGO(WaitGO waitGO)
        {
            _name = waitGO.Name;
            _value = waitGO.Value;
            _maxTime = waitGO.MaxTime;
        }

        /// <summary>
        /// Returns an exact duplicate of this Wait GO instance.
        /// </summary>
        /// <returns> 
        /// A deep copy of the Wait GO instance.
        /// </returns>
        public WaitGO Duplicate()
        {
            return new WaitGO(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Wait GO instance as IInstruction.
        /// </summary>
        /// <returns> 
        /// A deep copy of the Wait GO instance as an IInstruction. 
        /// </returns>
        public IInstruction DuplicateInstruction()
        {
            return new WaitGO(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Wait GO instance as an Action. 
        /// </summary>
        /// <returns> 
        /// A deep copy of the Wait GO instance as an Action. 
        /// </returns>
        public IAction DuplicateAction()
        {
            return new WaitGO(this);
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
                return "Empty Wait for Group Output";
            }
            if (!IsValid)
            {
                return "Invalid Wait for Group Output";
            }
            else
            {
                return $"Wait for Group Output ({_name}\\{_value})";
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
            if (_maxTime > 0)
            {
                string result = $"WaitDI {_name}, {_value} ";
                result += $"\\MaxTime:={_maxTime:0.###}";
                //result += $"{(_timeFlag ? $"\\TimeFlag:=TRUE" : $"\\TimeFlag:=FALSE")}";
                result += ";";

                return result;
            }
            else
            {
                return $"WaitDI {_name}, {_value};";
            }
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
        /// Gets or sets the desired state of the group output signal.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Gets or sets the name of the digital output signal.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets te max. time to wait in seconds.
        /// </summary>
        /// <remarks>
        /// Set a negative value to wait forever (default is -1).
        /// </remarks>
        public double MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = value; }
        }
        #endregion
    }
}