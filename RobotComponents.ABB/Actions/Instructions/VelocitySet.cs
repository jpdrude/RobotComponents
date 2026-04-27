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
    /// Represents the Velocity Set instruction.
    /// </summary>
    [Serializable()]
    public class VelocitySet : IAction, IInstruction, ISerializable
    {
        #region fields
        private string _overrideExpr;
        private string _maxExpr;
        #endregion

        #region (de)serialization
        protected VelocitySet(SerializationInfo info, StreamingContext context)
        {
            try { _overrideExpr = (string)info.GetValue("OverrideExpr", typeof(string)); }
            catch (SerializationException) { _overrideExpr = ((double)info.GetValue("Override", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
            try { _maxExpr = (string)info.GetValue("MaxExpr", typeof(string)); }
            catch (SerializationException) { _maxExpr = ((double)info.GetValue("Max", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("OverrideExpr", _overrideExpr, typeof(string));
            info.AddValue("MaxExpr", _maxExpr, typeof(string));
        }
        #endregion

        #region constructors
        public VelocitySet() { }

        /// <summary>Creates a Velocity Set instruction with RAPID expressions for both values.</summary>
        public VelocitySet(string @override, string max)
        {
            _overrideExpr = @override;
            _maxExpr = max;
        }

        /// <summary>Creates a Velocity Set instruction with double values (backward compat).</summary>
        public VelocitySet(double @override, double max)
            : this(@override.ToString("0.######", CultureInfo.InvariantCulture),
                   max.ToString("0.######", CultureInfo.InvariantCulture)) { }

        public VelocitySet(VelocitySet velocitySet)
        {
            _overrideExpr = velocitySet._overrideExpr;
            _maxExpr = velocitySet._maxExpr;
        }

        public VelocitySet Duplicate() => new VelocitySet(this);
        public IInstruction DuplicateInstruction() => new VelocitySet(this);
        public IAction DuplicateAction() => new VelocitySet(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (!IsValid) return "Invalid Velocity Set";
            return "Velocity Set";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
            => $"VelSet {_overrideExpr}, {_maxExpr};";

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
                if (string.IsNullOrEmpty(_overrideExpr) || string.IsNullOrEmpty(_maxExpr)) return false;
                if (double.TryParse(_overrideExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double o))
                    if (o < 0 || o > 100) return false;
                if (double.TryParse(_maxExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double m))
                    if (m <= 0) return false;
                return true;
            }
        }

        /// <summary>Gets or sets the velocity override as a RAPID expression string.</summary>
        public string OverrideExpression { get { return _overrideExpr; } set { _overrideExpr = value; } }

        /// <summary>Gets or sets the max velocity as a RAPID expression string.</summary>
        public string MaxExpression { get { return _maxExpr; } set { _maxExpr = value; } }

        /// <summary>Gets or sets the velocity override as a double (backward compat wrapper).</summary>
        public double Override
        {
            get { return double.TryParse(_overrideExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : 0.0; }
            set { _overrideExpr = value.ToString("0.######", CultureInfo.InvariantCulture); }
        }

        /// <summary>Gets or sets the max velocity as a double (backward compat wrapper).</summary>
        public double Max
        {
            get { return double.TryParse(_maxExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : 0.0; }
            set { _maxExpr = value.ToString("0.######", CultureInfo.InvariantCulture); }
        }
        #endregion
    }
}
