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
using RobotComponents.ABB.Utils;

namespace RobotComponents.ABB.Actions.Instructions
{
    /// <summary>
    /// Represents an Invert Digital Output instruction.
    /// </summary>
    [Serializable()]
    public class InvertDigitalOutput : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        #endregion

        #region (de)serialization
        protected InvertDigitalOutput(SerializationInfo info, StreamingContext context)
        {
            _name = (string)info.GetValue("Name", typeof(string));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Name", _name, typeof(string));
        }
        #endregion

        #region constructors
        public InvertDigitalOutput() { }

        /// <summary>Creates an Invert Digital Output instruction.</summary>
        public InvertDigitalOutput(string name)
        {
            _name = name;
        }

        public InvertDigitalOutput(InvertDigitalOutput invertDigitalOutput)
        {
            _name = invertDigitalOutput._name;
        }

        public InvertDigitalOutput Duplicate() => new InvertDigitalOutput(this);
        public IInstruction DuplicateInstruction() => new InvertDigitalOutput(this);
        public IAction DuplicateAction() => new InvertDigitalOutput(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (_name == null) return "Empty Invert Digital Output";
            if (!IsValid) return "Invalid Invert Digital Output";
            return $"Invert Digital Output ({_name})";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            HelperMethods.ThrowIfInvalidRapidIdentifier(_name);
            return $"InvertDO {_name};";
        }

        public void ToRAPIDGenerator(RAPIDGenerator RAPIDGenerator)
        {
            RAPIDGenerator.ProgramInstructions.Add("    " + "    " + new string(' ', IndentationLevel * 4) + ToRAPIDInstruction(RAPIDGenerator.Robot));
        }
        #endregion

        #region properties
        /// <inheritdoc/>
        public int IndentationLevel { get; set; }

        public bool IsValid
        {
            get
            {
                if (_name == null || _name == "") return false;
                if (!HelperMethods.IsValidRapidIdentifier(_name)) return false;
                return true;
            }
        }

        public string Name { get { return _name; } set { _name = value; } }
        #endregion
    }
}
