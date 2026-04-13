// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2023-2024 Arjen Deetman
//
// Authors:
//   - Arjen Deetman (2023-2024)
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
    /// Represents a Pulse Digital Output instruction.
    /// </summary>
    [Serializable()]
    public class PulseDigitalOutput : IAction, IInstruction, ISerializable
    {
        #region fields
        private bool _high = false;
        private string _lengthExpr = "0.2";
        private string _name;
        #endregion

        #region (de)serialization
        protected PulseDigitalOutput(SerializationInfo info, StreamingContext context)
        {
            _high = (bool)info.GetValue("High", typeof(bool));
            try { _lengthExpr = (string)info.GetValue("LengthExpr", typeof(string)); }
            catch (SerializationException) { _lengthExpr = ((double)info.GetValue("Length", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
            _name = (string)info.GetValue("Name", typeof(string));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("High", _high, typeof(bool));
            info.AddValue("LengthExpr", _lengthExpr, typeof(string));
            info.AddValue("Name", _name, typeof(string));
        }
        #endregion

        #region constructors
        public PulseDigitalOutput() { }

        /// <summary>Creates a Pulse Digital Output instruction with a RAPID expression for the pulse length.</summary>
        public PulseDigitalOutput(bool high, string length, string name)
        {
            _high = high;
            _lengthExpr = length;
            _name = name;
        }

        /// <summary>Creates a Pulse Digital Output instruction with a double pulse length (backward compat).</summary>
        public PulseDigitalOutput(bool high, double length, string name)
            : this(high, length.ToString("0.######", CultureInfo.InvariantCulture), name) { }

        public PulseDigitalOutput(PulseDigitalOutput pulseDigitalOutput)
        {
            _high = pulseDigitalOutput._high;
            _lengthExpr = pulseDigitalOutput._lengthExpr;
            _name = pulseDigitalOutput._name;
        }

        public PulseDigitalOutput Duplicate() => new PulseDigitalOutput(this);
        public IInstruction DuplicateInstruction() => new PulseDigitalOutput(this);
        public IAction DuplicateAction() => new PulseDigitalOutput(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (_name == null) return "Empty Pulse Digital Output";
            if (!IsValid) return "Invalid Pulse Digital Output";
            return $"Pulse Digital Output ({_name})";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            HelperMethods.ThrowIfInvalidRapidIdentifier(_name);
            if (!_high)
                return $"PulseDO \\PLength:={_lengthExpr}, {_name};";
            return $"PulseDO \\High, \\PLength:={_lengthExpr}, {_name};";
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
                if (string.IsNullOrEmpty(_lengthExpr)) return false;
                // Only validate bounds when the expression is a plain number
                if (double.TryParse(_lengthExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double l))
                    return l >= 0.001 && l <= 2000;
                return true;
            }
        }

        public bool High { get { return _high; } set { _high = value; } }
        public string Name { get { return _name; } set { _name = value; } }

        /// <summary>Gets or sets the pulse length as a RAPID expression string.</summary>
        public string LengthExpression { get { return _lengthExpr; } set { _lengthExpr = value; } }

        /// <summary>Gets or sets the pulse length as a double (backward compat wrapper).</summary>
        public double Length
        {
            get { return double.TryParse(_lengthExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : 0.2; }
            set { _lengthExpr = value.ToString("0.######", CultureInfo.InvariantCulture); }
        }
        #endregion
    }
}
