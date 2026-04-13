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
    /// Represents a Wait for Group Input instruction.
    /// </summary>
    [Serializable()]
    public class WaitGI : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        private string _valueExpr;
        private double _maxTime;
        #endregion

        #region (de)serialization
        protected WaitGI(SerializationInfo info, StreamingContext context)
        {
            _name = (string)info.GetValue("Name", typeof(string));
            try { _valueExpr = (string)info.GetValue("ValueExpr", typeof(string)); }
            catch (SerializationException) { _valueExpr = ((int)info.GetValue("Value", typeof(int))).ToString(CultureInfo.InvariantCulture); }
            _maxTime = (double)info.GetValue("Max Time", typeof(double));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("ValueExpr", _valueExpr, typeof(string));
            info.AddValue("Max Time", _maxTime, typeof(double));
        }
        #endregion

        #region constructors
        public WaitGI() { }

        /// <summary>Creates a Wait GI instruction with a RAPID expression for the value.</summary>
        public WaitGI(string name, string value, double maxTime = -1)
        {
            _name = name;
            _valueExpr = value;
            _maxTime = maxTime;
        }

        /// <summary>Creates a Wait GI instruction with an integer value (backward compat).</summary>
        public WaitGI(string name, int value, double maxTime = -1)
            : this(name, value.ToString(CultureInfo.InvariantCulture), maxTime) { }

        public WaitGI(WaitGI waitGI)
        {
            _name = waitGI._name;
            _valueExpr = waitGI._valueExpr;
            _maxTime = waitGI._maxTime;
        }

        public WaitGI Duplicate() => new WaitGI(this);
        public IInstruction DuplicateInstruction() => new WaitGI(this);
        public IAction DuplicateAction() => new WaitGI(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (_name == null) return "Empty Wait for Group Input";
            if (!IsValid) return "Invalid Wait for Group Input";
            return $"Wait for Group Input ({_name}\\{_valueExpr})";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            HelperMethods.ThrowIfInvalidRapidIdentifier(_name);
            if (_maxTime > 0)
                return $"WaitGI {_name}, {_valueExpr} \\MaxTime:={_maxTime:0.###};";
            return $"WaitGI {_name}, {_valueExpr};";
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
        public double MaxTime { get { return _maxTime; } set { _maxTime = value; } }

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
