// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2018-2020 EDEK Uni Kassel
// Copyright (c) 2020-2024 Arjen Deetman
//
// Authors:
//   - Gabriel Rumph (2018-2019)
//   - Benedikt Wannemacher (2018-2019)
//   - Arjen Deetman (2019-2024)
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
    /// Represents a Set Digital Output instruction.
    /// </summary>
    [Serializable()]
    public class SetDigitalOutput : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        private double _delay;
        private bool _sync;
        private string _valueExpr;
        #endregion

        #region (de)serialization
        protected SetDigitalOutput(SerializationInfo info, StreamingContext context)
        {
            _name = (string)info.GetValue("Name", typeof(string));
            _delay = (double)info.GetValue("Delay", typeof(double));
            _sync = (bool)info.GetValue("Sync", typeof(bool));
            try { _valueExpr = (string)info.GetValue("ValueExpr", typeof(string)); }
            catch (SerializationException) { _valueExpr = ((bool)info.GetValue("Value", typeof(bool))) ? "1" : "0"; }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("Delay", _delay, typeof(double));
            info.AddValue("Sync", _sync, typeof(bool));
            info.AddValue("ValueExpr", _valueExpr, typeof(string));
        }
        #endregion

        #region constructors
        public SetDigitalOutput() { }

        /// <summary>Creates a Set Digital Output instruction with a RAPID expression for the value.</summary>
        public SetDigitalOutput(string name, string value)
        {
            _name = name;
            _delay = 0;
            _sync = false;
            _valueExpr = value;
        }

        /// <summary>Creates a Set Digital Output instruction with a boolean value (backward compat).</summary>
        public SetDigitalOutput(string name, bool value) : this(name, value ? "1" : "0") { }

        public SetDigitalOutput(SetDigitalOutput setDigitalOutput)
        {
            _name = setDigitalOutput._name;
            _delay = setDigitalOutput._delay;
            _sync = setDigitalOutput._sync;
            _valueExpr = setDigitalOutput._valueExpr;
        }

        public SetDigitalOutput Duplicate() => new SetDigitalOutput(this);
        public IInstruction DuplicateInstruction() => new SetDigitalOutput(this);
        public IAction DuplicateAction() => new SetDigitalOutput(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (_name == null) return "Empty Set Digital Output";
            if (!IsValid) return "Invalid Set Digital Output";
            return $"Set Digital Output ({_name}\\{_valueExpr})";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            HelperMethods.ThrowIfInvalidRapidIdentifier(_name);
            string result = "SetDO ";
            result += _sync ? "\\Sync, " : (_delay > 0 ? $"\\SDelay:={_delay.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, " : "");
            result += $"{_name}, {_valueExpr};";
            return result;
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
        public double Delay { get { return _delay; } set { _delay = value; } }
        public bool Sync { get { return _sync; } set { _sync = value; } }

        /// <summary>Gets or sets the value as a RAPID expression string.</summary>
        public string ValueExpression { get { return _valueExpr; } set { _valueExpr = value; } }

        /// <summary>Gets or sets the value as a boolean (backward compat wrapper).</summary>
        public bool Value
        {
            get { return _valueExpr == "1" || string.Equals(_valueExpr, "TRUE", StringComparison.OrdinalIgnoreCase); }
            set { _valueExpr = value ? "1" : "0"; }
        }
        #endregion
    }
}
