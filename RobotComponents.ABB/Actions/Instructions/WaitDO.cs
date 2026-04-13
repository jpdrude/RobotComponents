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
    /// Represents a Wait for Digital Output instruction.
    /// </summary>
    [Serializable()]
    public class WaitDO : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _name;
        private string _valueExpr;
        private double _maxTime;
        private bool _timeFlag;
        #endregion

        #region (de)serialization
        protected WaitDO(SerializationInfo info, StreamingContext context)
        {
            _name = (string)info.GetValue("Name", typeof(string));
            try { _valueExpr = (string)info.GetValue("ValueExpr", typeof(string)); }
            catch (SerializationException) { _valueExpr = ((bool)info.GetValue("Value", typeof(bool))) ? "1" : "0"; }
            _maxTime = (double)info.GetValue("Max Time", typeof(double));
            _timeFlag = (bool)info.GetValue("Time Flag", typeof(bool));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("ValueExpr", _valueExpr, typeof(string));
            info.AddValue("Max Time", _maxTime, typeof(double));
            info.AddValue("Time Flag", _timeFlag, typeof(bool));
        }
        #endregion

        #region constructors
        public WaitDO() { }

        /// <summary>Creates a Wait DO instruction with a RAPID expression for the value.</summary>
        public WaitDO(string name, string value, double maxTime = -1, bool timeFlag = false)
        {
            _name = name;
            _valueExpr = value;
            _maxTime = maxTime;
            _timeFlag = timeFlag;
        }

        /// <summary>Creates a Wait DO instruction with a boolean value (backward compat).</summary>
        public WaitDO(string name, bool value, double maxTime = -1, bool timeFlag = false)
            : this(name, value ? "1" : "0", maxTime, timeFlag) { }

        public WaitDO(WaitDO waitDO)
        {
            _name = waitDO._name;
            _valueExpr = waitDO._valueExpr;
            _maxTime = waitDO._maxTime;
            _timeFlag = waitDO._timeFlag;
        }

        public WaitDO Duplicate() => new WaitDO(this);
        public IInstruction DuplicateInstruction() => new WaitDO(this);
        public IAction DuplicateAction() => new WaitDO(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (_name == null) return "Empty Wait for Digital Output";
            if (!IsValid) return "Invalid Wait for Digital Output";
            return $"Wait for Digital Output ({_name}\\{_valueExpr})";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            HelperMethods.ThrowIfInvalidRapidIdentifier(_name);
            if (_maxTime > 0)
            {
                return $"WaitDO {_name}, {_valueExpr} \\MaxTime:={_maxTime:0.###} " +
                       $"{(_timeFlag ? "\\TimeFlag:=TRUE" : "\\TimeFlag:=FALSE")};";
            }
            return $"WaitDO {_name}, {_valueExpr};";
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
        public bool TimeFlag { get { return _timeFlag; } set { _timeFlag = value; } }

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
