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
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
// RobotComponents Libs
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Utils;

namespace RobotComponents.ABB.Actions.Instructions
{
    /// <summary>
    /// Represents a Set Group Output instruction.
    /// </summary>
    [Serializable()]
    public class SetGroupOutput : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        private string _valueExpr;
        #endregion

        #region (de)serialization
        protected SetGroupOutput(SerializationInfo info, StreamingContext context)
        {
            _name = (string)info.GetValue("Name", typeof(string));
            try { _valueExpr = (string)info.GetValue("ValueExpr", typeof(string)); }
            catch (SerializationException) { _valueExpr = ((int)info.GetValue("Value", typeof(int))).ToString(CultureInfo.InvariantCulture); }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("ValueExpr", _valueExpr, typeof(string));
        }
        #endregion

        #region constructors
        public SetGroupOutput() { }

        /// <summary>Creates a Set Group Output instruction with a RAPID expression for the value.</summary>
        public SetGroupOutput(string name, string value)
        {
            _name = name;
            _valueExpr = value;
        }

        /// <summary>Creates a Set Group Output instruction with an integer value (backward compat).</summary>
        public SetGroupOutput(string name, int value)
            : this(name, value.ToString(CultureInfo.InvariantCulture)) { }

        public SetGroupOutput(SetGroupOutput setGroupOutput)
        {
            _name = setGroupOutput._name;
            _valueExpr = setGroupOutput._valueExpr;
        }

        public SetGroupOutput Duplicate() => new SetGroupOutput(this);
        public IInstruction DuplicateInstruction() => new SetGroupOutput(this);
        public IAction DuplicateAction() => new SetGroupOutput(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (_name == null) return "Empty Set Group Output";
            if (!IsValid) return "Invalid Set Group Output";
            return $"Set Group Output ({_name}\\{_valueExpr})";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            HelperMethods.ThrowIfInvalidRapidIdentifier(_name);
            return $"SetGO {_name}, {_valueExpr};";
        }

        public void ToRAPIDGenerator(RAPIDGenerator RAPIDGenerator)
        {
            RAPIDGenerator.ProgramInstructions.Add("    " + "    " + ToRAPIDInstruction(RAPIDGenerator.Robot));
        }
        #endregion

        #region properties
        public bool IsValid
        {
            get
            {
                if (_name == null || _name == "") return false;
                if (!HelperMethods.IsValidRapidIdentifier(_name)) return false;
                if (string.IsNullOrEmpty(_valueExpr)) return false;
                return true;
            }
        }

        public string Name { get { return _name; } set { _name = value; } }

        /// <summary>Gets or sets the value as a RAPID expression string.</summary>
        public string ValueExpression { get { return _valueExpr; } set { _valueExpr = value; } }

        /// <summary>Gets or sets the value as an integer (backward compat wrapper).</summary>
        public int Value
        {
            get { return int.TryParse(_valueExpr, out int i) ? i : 0; }
            set { _valueExpr = value.ToString(CultureInfo.InvariantCulture); }
        }
        #endregion
    }
}
