// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2024 Arjen Deetman
//
// Authors:
//   - Arjen Deetman (2024)
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
    /// Represents the Acceleration Set instruction.
    /// </summary>
    [Serializable()]
    public class AccelerationSet : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _accelerationExpr;
        private string _rampExpr;
        #endregion

        #region (de)serialization
        protected AccelerationSet(SerializationInfo info, StreamingContext context)
        {
            try { _accelerationExpr = (string)info.GetValue("AccelerationExpr", typeof(string)); }
            catch (SerializationException) { _accelerationExpr = ((double)info.GetValue("Acceleration", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
            try { _rampExpr = (string)info.GetValue("RampExpr", typeof(string)); }
            catch (SerializationException) { _rampExpr = ((double)info.GetValue("Ramp", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("AccelerationExpr", _accelerationExpr, typeof(string));
            info.AddValue("RampExpr", _rampExpr, typeof(string));
        }
        #endregion

        #region constructors
        public AccelerationSet() { }

        /// <summary>Creates an Acceleration Set instruction with RAPID expressions for both values.</summary>
        public AccelerationSet(string acceleration, string ramp)
        {
            _accelerationExpr = acceleration;
            _rampExpr = ramp;
        }

        /// <summary>Creates an Acceleration Set instruction with double values (backward compat).</summary>
        public AccelerationSet(double acceleration, double ramp)
            : this(acceleration.ToString("0.######", CultureInfo.InvariantCulture),
                   ramp.ToString("0.######", CultureInfo.InvariantCulture)) { }

        public AccelerationSet(AccelerationSet accelerationSet)
        {
            _accelerationExpr = accelerationSet._accelerationExpr;
            _rampExpr = accelerationSet._rampExpr;
        }

        public AccelerationSet Duplicate() => new AccelerationSet(this);
        public IInstruction DuplicateInstruction() => new AccelerationSet(this);
        public IAction DuplicateAction() => new AccelerationSet(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (!IsValid) return "Invalid Acceleration Set";
            return "Acceleration Set";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
            => $"AccSet {_accelerationExpr}, {_rampExpr};";

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
                if (string.IsNullOrEmpty(_accelerationExpr) || string.IsNullOrEmpty(_rampExpr)) return false;
                // Only validate numeric bounds when the expression is a plain number
                if (double.TryParse(_accelerationExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double a))
                    if (a < 20 || a > 100) return false;
                if (double.TryParse(_rampExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double r))
                    if (r < 10 || r > 100) return false;
                return true;
            }
        }

        /// <summary>Gets or sets the acceleration as a RAPID expression string.</summary>
        public string AccelerationExpression { get { return _accelerationExpr; } set { _accelerationExpr = value; } }

        /// <summary>Gets or sets the ramp as a RAPID expression string.</summary>
        public string RampExpression { get { return _rampExpr; } set { _rampExpr = value; } }

        /// <summary>Gets or sets the acceleration as a double (backward compat wrapper).</summary>
        public double Acceleration
        {
            get { return double.TryParse(_accelerationExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : 0.0; }
            set { _accelerationExpr = value.ToString("0.######", CultureInfo.InvariantCulture); }
        }

        /// <summary>Gets or sets the ramp as a double (backward compat wrapper).</summary>
        public double Ramp
        {
            get { return double.TryParse(_rampExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : 0.0; }
            set { _rampExpr = value.ToString("0.######", CultureInfo.InvariantCulture); }
        }
        #endregion
    }
}
