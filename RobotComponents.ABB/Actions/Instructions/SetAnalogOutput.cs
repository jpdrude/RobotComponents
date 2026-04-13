// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2020-2024 Arjen Deetman
//
// Authors:
//   - Arjen Deetman (2020-2024)
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
    /// Represents a Set Analog Output instruction.
    /// </summary>
    [Serializable()]
    public class SetAnalogOutput : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        private string _valueExpr;
        #endregion

        #region (de)serialization
        protected SetAnalogOutput(SerializationInfo info, StreamingContext context)
        {
            _name = (string)info.GetValue("Name", typeof(string));
            try { _valueExpr = (string)info.GetValue("ValueExpr", typeof(string)); }
            catch (SerializationException) { _valueExpr = ((double)info.GetValue("Value", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
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
        public SetAnalogOutput() { }

        /// <summary>Creates a Set Analog Output instruction with a RAPID expression for the value.</summary>
        public SetAnalogOutput(string name, string value)
        {
            _name = name;
            _valueExpr = value;
        }

        /// <summary>Creates a Set Analog Output instruction with a double value (backward compat).</summary>
        public SetAnalogOutput(string name, double value)
            : this(name, value.ToString("0.######", CultureInfo.InvariantCulture)) { }

        public SetAnalogOutput(SetAnalogOutput setAnalogOutput)
        {
            _name = setAnalogOutput._name;
            _valueExpr = setAnalogOutput._valueExpr;
        }

        public SetAnalogOutput Duplicate() => new SetAnalogOutput(this);
        public IInstruction DuplicateInstruction() => new SetAnalogOutput(this);
        public IAction DuplicateAction() => new SetAnalogOutput(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (_name == null) return "Empty Set Analog Output";
            if (!IsValid) return "Invalid Set Analog Output";
            return $"Set Analog Output ({_name}\\{_valueExpr})";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            HelperMethods.ThrowIfInvalidRapidIdentifier(_name);
            return $"SetAO {_name}, {_valueExpr};";
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

        /// <summary>Gets or sets the value as a double (backward compat wrapper).</summary>
        public double Value
        {
            get { return double.TryParse(_valueExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : 0.0; }
            set { _valueExpr = value.ToString("0.######", CultureInfo.InvariantCulture); }
        }
        #endregion
    }
}
