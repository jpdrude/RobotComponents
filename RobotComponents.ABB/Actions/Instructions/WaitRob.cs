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
    /// Represent the Wait Time instruction.
    /// </summary>
    /// <remarks>
    /// This action is used to wait for a robot to finish a movement.
    /// </remarks>
    [Serializable()]
    public class WaitRob : IAction, IInstruction, ISerializable
    {
        #region fields
        private bool _inPosition;
        private bool _zeroSpeed;
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.  
        /// </summary>
        /// <param name="info"> The SerializationInfo to extract the data from. </param>
        /// <param name="context"> The context of this deserialization. </param>
        protected WaitRob(SerializationInfo info, StreamingContext context)
        {
            //Version version = (Version)info.GetValue("Version", typeof(Version)); // <-- use this if the (de)serialization changes
            _inPosition = (bool)info.GetValue("In Position", typeof(bool));
            _zeroSpeed = (bool)info.GetValue("Zero Speed", typeof(bool));
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
            info.AddValue("In Position", _inPosition, typeof(bool));
            info.AddValue("Zero Speed", _zeroSpeed, typeof(bool));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes an empty instance of the Wait Rob class.
        /// </summary>
        public WaitRob()
        {
        }

        /// <summary>
        /// Initializes an empty instance of the Wait Rob class.
        /// </summary>
        /// <param name="zeroSpeed"> Specifies whether or not the mechanial units must have come to a complete standstill before execution continues. </param>
        public WaitRob(bool zeroSpeed = false)
        {
            if (zeroSpeed)
            {
                _zeroSpeed = true;
                _inPosition = false;
            }
            else
            {
                _zeroSpeed = false;
                _inPosition = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Wait Rob class by duplicating an existing Wait Rob instance. 
        /// </summary>
        /// <param name="waitRob"> The Wait Rob instance to duplicate. </param>
        public WaitRob(WaitRob waitRob)
        {
            _inPosition = waitRob.InPosition;
            _zeroSpeed = waitRob.ZeroSpeed;
        }

        /// <summary>
        /// Returns an exact duplicate of this Wait Rob instance.
        /// </summary>
        /// <returns> 
        /// A deep copy of the Wait Rob instance. 
        /// </returns>
        public WaitRob Duplicate()
        {
            return new WaitRob(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Wait Rob instance as IInstruction.
        /// </summary>
        /// <returns> 
        /// A deep copy of the Wait Rob instance as an IInstruction. 
        /// </returns>
        public IInstruction DuplicateInstruction()
        {
            return new WaitRob(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Wait Rob instance as an Action. 
        /// </summary>
        /// <returns> 
        /// A deep copy of the Wait Rob instance as an Action. 
        /// </returns>
        public IAction DuplicateAction()
        {
            return new WaitRob(this);
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
                return "Invalid Wait Rob";
            }
            else
            {
                if (_inPosition)
                    return "Wait Rob (In Position)";
                else
                    return "Wait Rob (Zero Speed)";
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
            if (_inPosition)
                return "WaitRob \\InPos;";
            else
                return "WaitRob \\ZeroSpeed;";
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
                if ((_inPosition && _zeroSpeed) || (!_inPosition && !_zeroSpeed))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether or not the robot and external axes must have arrived at their target positions,
        /// before execution continues.
        /// </summary>
        public bool InPosition
        {
            get { return _inPosition; }
            set { _inPosition = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the robot and external axes must have come to a complete standstill,
        /// before execution continues.
        /// </summary>
        public bool ZeroSpeed
        {
            get { return _zeroSpeed; }
            set { _zeroSpeed = value; }
        }
        #endregion
    }
}