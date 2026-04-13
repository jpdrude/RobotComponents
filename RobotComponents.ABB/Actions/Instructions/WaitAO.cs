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
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Utils;

namespace RobotComponents.ABB.Actions.Instructions
{
    /// <summary>
    /// Represents a Wait for Analog Output instruction.
    /// </summary>
    [Serializable()]
    public class WaitAO : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        private string _valueExpr;
        private InequalitySymbol _inequalitySymbol;
        private double _maxTime;
        #endregion

        #region (de)serialization
        protected WaitAO(SerializationInfo info, StreamingContext context)
        {
            _name = (string)info.GetValue("Name", typeof(string));
            try { _valueExpr = (string)info.GetValue("ValueExpr", typeof(string)); }
            catch (SerializationException) { _valueExpr = ((double)info.GetValue("Value", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
            _inequalitySymbol = (InequalitySymbol)info.GetValue("Inequality Symbol", typeof(InequalitySymbol));
            _maxTime = (double)info.GetValue("Max Time", typeof(double));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("ValueExpr", _valueExpr, typeof(string));
            info.AddValue("Inequality Symbol", _inequalitySymbol, typeof(InequalitySymbol));
            info.AddValue("Max Time", _maxTime, typeof(double));
        }
        #endregion

        #region constructors
        public WaitAO() { }

        /// <summary>Creates a Wait AO instruction with a RAPID expression for the value.</summary>
        public WaitAO(string name, string value, InequalitySymbol inequalitySymbol, double maxTime = -1)
        {
            _name = name;
            _valueExpr = value;
            _inequalitySymbol = inequalitySymbol;
            _maxTime = maxTime;
        }

        /// <summary>Creates a Wait AO instruction with a double value (backward compat).</summary>
        public WaitAO(string name, double value, InequalitySymbol inequalitySymbol, double maxTime = -1)
            : this(name, value.ToString("0.######", CultureInfo.InvariantCulture), inequalitySymbol, maxTime) { }

        public WaitAO(WaitAO waitAO)
        {
            _name = waitAO._name;
            _valueExpr = waitAO._valueExpr;
            _inequalitySymbol = waitAO._inequalitySymbol;
            _maxTime = waitAO._maxTime;
        }

        public WaitAO Duplicate() => new WaitAO(this);
        public IInstruction DuplicateInstruction() => new WaitAO(this);
        public IAction DuplicateAction() => new WaitAO(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (_name == null) return "Empty Wait for Analog Output";
            if (!IsValid) return "Invalid Wait for Analog Output";
            return $"Wait for Analog Output ({_name}\\{_valueExpr})";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            HelperMethods.ThrowIfInvalidRapidIdentifier(_name);
            return $"WaitAO {_name}, \\{Enum.GetName(typeof(InequalitySymbol), _inequalitySymbol)}, {_valueExpr}" +
                   $"{(_maxTime > 0 ? $"\\MaxTime:={_maxTime:0.###}" : "")};";
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
        public InequalitySymbol InequalitySymbol { get { return _inequalitySymbol; } set { _inequalitySymbol = value; } }
        public double MaxTime { get { return _maxTime; } set { _maxTime = value; } }

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
