// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2018-2020 EDEK Uni Kassel
// Copyright (c) 2020-2024 Arjen Deetman
//
// Authors:
//   - Gabriel Rumph (2018-2020)
//   - Benedikt Wannemacher (2018-2020)
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

namespace RobotComponents.ABB.Actions.Instructions
{
    /// <summary>
    /// Represents the Wait Time instruction.
    /// </summary>
    [Serializable()]
    public class WaitTime : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _durationExpr;
        private bool _inPosition;
        #endregion

        #region (de)serialization
        protected WaitTime(SerializationInfo info, StreamingContext context)
        {
            try { _durationExpr = (string)info.GetValue("DurationExpr", typeof(string)); }
            catch (SerializationException) { _durationExpr = ((double)info.GetValue("Duration", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
            _inPosition = (bool)info.GetValue("In Position", typeof(bool));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("DurationExpr", _durationExpr, typeof(string));
            info.AddValue("In Position", _inPosition, typeof(bool));
        }
        #endregion

        #region constructors
        public WaitTime() { }

        /// <summary>Creates a Wait Time instruction with a RAPID expression for the duration.</summary>
        public WaitTime(string duration, bool inPosition = false)
        {
            _durationExpr = duration;
            _inPosition = inPosition;
        }

        /// <summary>Creates a Wait Time instruction with a double duration (backward compat).</summary>
        public WaitTime(double duration, bool inPosition = false)
            : this(duration.ToString("0.######", CultureInfo.InvariantCulture), inPosition) { }

        public WaitTime(WaitTime waitTime)
        {
            _durationExpr = waitTime._durationExpr;
            _inPosition = waitTime._inPosition;
        }

        public WaitTime Duplicate() => new WaitTime(this);
        public IInstruction DuplicateInstruction() => new WaitTime(this);
        public IAction DuplicateAction() => new WaitTime(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (!IsValid) return "Invalid Wait Time";
            return $"Wait Time ({_durationExpr} sec.)";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
            => $"WaitTime {(_inPosition ? "\\InPos, " : "")}{_durationExpr};";

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
                if (string.IsNullOrEmpty(_durationExpr)) return false;
                // Only validate bounds when the expression is a plain number
                if (double.TryParse(_durationExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                    return d >= 0;
                return true; // Expression — assume valid; RAPID compiler is authoritative
            }
        }

        /// <summary>Gets or sets the duration as a RAPID expression string.</summary>
        public string DurationExpression { get { return _durationExpr; } set { _durationExpr = value; } }

        /// <summary>Gets or sets the duration as a double (backward compat wrapper).</summary>
        public double Duration
        {
            get { return double.TryParse(_durationExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : 0.0; }
            set { _durationExpr = value.ToString("0.######", CultureInfo.InvariantCulture); }
        }

        public bool InPosition { get { return _inPosition; } set { _inPosition = value; } }
        #endregion
    }
}
