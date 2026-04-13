// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2021-2024 Arjen Deetman
//
// Authors:
//   - Arjen Deetman (2021-2024)
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
    /// Represents a Wait for Analog Input instruction.
    /// </summary>
    [Serializable()]
    public class WaitAI : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        private string _valueExpr;
        private InequalitySymbol _inequalitySymbol;
        private double _maxTime;
        #endregion

        #region (de)serialization
        protected WaitAI(SerializationInfo info, StreamingContext context)
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
        public WaitAI() { }

        /// <summary>Creates a Wait AI instruction with a RAPID expression for the value.</summary>
        public WaitAI(string name, string value, InequalitySymbol inequalitySymbol, double maxTime = -1)
        {
            _name = name;
            _valueExpr = value;
            _inequalitySymbol = inequalitySymbol;
            _maxTime = maxTime;
        }

        /// <summary>Creates a Wait AI instruction with a double value (backward compat).</summary>
        public WaitAI(string name, double value, InequalitySymbol inequalitySymbol, double maxTime = -1)
            : this(name, value.ToString("0.######", CultureInfo.InvariantCulture), inequalitySymbol, maxTime) { }

        public WaitAI(WaitAI waitAI)
        {
            _name = waitAI._name;
            _valueExpr = waitAI._valueExpr;
            _inequalitySymbol = waitAI._inequalitySymbol;
            _maxTime = waitAI._maxTime;
        }

        public WaitAI Duplicate() => new WaitAI(this);
        public IInstruction DuplicateInstruction() => new WaitAI(this);
        public IAction DuplicateAction() => new WaitAI(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (_name == null) return "Empty Wait for Analog Input";
            if (!IsValid) return "Invalid Wait for Analog Input";
            return $"Wait for Analog Input ({_name}\\{_valueExpr})";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            HelperMethods.ThrowIfInvalidRapidIdentifier(_name);
            return $"WaitAI {_name}, \\{Enum.GetName(typeof(InequalitySymbol), _inequalitySymbol)}, {_valueExpr}" +
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
