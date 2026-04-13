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
    /// Represents the Path Acceleration Limitation instruction.
    /// </summary>
    [Serializable()]
    public class PathAccelerationLimitation : IAction, IInstruction, ISerializable
    {
        #region fields
        private bool _accelerationLimitation;
        private string _accelerationMaxExpr;
        private bool _decelerationLimitation;
        private string _decelerationMaxExpr;
        #endregion

        #region (de)serialization
        protected PathAccelerationLimitation(SerializationInfo info, StreamingContext context)
        {
            _accelerationLimitation = (bool)info.GetValue("Acceleration Limitation", typeof(bool));
            try { _accelerationMaxExpr = (string)info.GetValue("AccelerationMaxExpr", typeof(string)); }
            catch (SerializationException) { _accelerationMaxExpr = ((double)info.GetValue("Acceleration Max", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
            _decelerationLimitation = (bool)info.GetValue("Deceleration Limitation", typeof(bool));
            try { _decelerationMaxExpr = (string)info.GetValue("DecelerationMaxExpr", typeof(string)); }
            catch (SerializationException) { _decelerationMaxExpr = ((double)info.GetValue("Deceleration Max", typeof(double))).ToString("0.######", CultureInfo.InvariantCulture); }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Acceleration Limitation", _accelerationLimitation, typeof(bool));
            info.AddValue("AccelerationMaxExpr", _accelerationMaxExpr, typeof(string));
            info.AddValue("Deceleration Limitation", _decelerationLimitation, typeof(bool));
            info.AddValue("DecelerationMaxExpr", _decelerationMaxExpr, typeof(string));
        }
        #endregion

        #region constructors
        public PathAccelerationLimitation() { }

        /// <summary>Creates a Path Acceleration Limitation instruction with RAPID expressions for the max values.</summary>
        public PathAccelerationLimitation(bool accelerationLimitation, string accelerationMax,
                                          bool decelerationLimitation, string decelerationMax)
        {
            _accelerationLimitation = accelerationLimitation;
            _accelerationMaxExpr = accelerationMax;
            _decelerationLimitation = decelerationLimitation;
            _decelerationMaxExpr = decelerationMax;
        }

        /// <summary>Creates a Path Acceleration Limitation instruction with double max values (backward compat).</summary>
        public PathAccelerationLimitation(bool accelerationLimitation, double accelerationMax,
                                          bool decelerationLimitation, double decelerationMax)
            : this(accelerationLimitation,
                   accelerationMax.ToString("0.######", CultureInfo.InvariantCulture),
                   decelerationLimitation,
                   decelerationMax.ToString("0.######", CultureInfo.InvariantCulture)) { }

        public PathAccelerationLimitation(PathAccelerationLimitation p)
        {
            _accelerationLimitation = p._accelerationLimitation;
            _accelerationMaxExpr = p._accelerationMaxExpr;
            _decelerationLimitation = p._decelerationLimitation;
            _decelerationMaxExpr = p._decelerationMaxExpr;
        }

        public PathAccelerationLimitation Duplicate() => new PathAccelerationLimitation(this);
        public IInstruction DuplicateInstruction() => new PathAccelerationLimitation(this);
        public IAction DuplicateAction() => new PathAccelerationLimitation(this);
        #endregion

        #region methods
        public override string ToString()
        {
            if (!IsValid) return "Invalid Path Acceleration Limitation";
            return "Path Acceleration Limitation";
        }

        public string ToRAPIDDeclaration(Robot robot) => string.Empty;

        public string ToRAPIDInstruction(Robot robot)
        {
            string acceleration = !_accelerationLimitation ? "FALSE" : $"TRUE\\AccMax:={_accelerationMaxExpr}";
            string deceleration = !_decelerationLimitation ? "FALSE" : $"TRUE\\DecelMax:={_decelerationMaxExpr}";
            return $"PathAccelerationLimitation {acceleration}, {deceleration};";
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
                if (string.IsNullOrEmpty(_accelerationMaxExpr) || string.IsNullOrEmpty(_decelerationMaxExpr)) return false;
                if (double.TryParse(_accelerationMaxExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double a) && a < 0) return false;
                if (double.TryParse(_decelerationMaxExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) && d < 0) return false;
                return true;
            }
        }

        public bool AccelerationLimitation { get { return _accelerationLimitation; } set { _accelerationLimitation = value; } }
        public bool DecelerationLimitation { get { return _decelerationLimitation; } set { _decelerationLimitation = value; } }

        /// <summary>Gets or sets the acceleration max as a RAPID expression string.</summary>
        public string AccelerationMaxExpression { get { return _accelerationMaxExpr; } set { _accelerationMaxExpr = value; } }

        /// <summary>Gets or sets the deceleration max as a RAPID expression string.</summary>
        public string DecelerationMaxExpression { get { return _decelerationMaxExpr; } set { _decelerationMaxExpr = value; } }

        /// <summary>Gets or sets the acceleration max as a double (backward compat wrapper).</summary>
        public double AccelerationMax
        {
            get { return double.TryParse(_accelerationMaxExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : 0.0; }
            set { _accelerationMaxExpr = value.ToString("0.######", CultureInfo.InvariantCulture); }
        }

        /// <summary>Gets or sets the deceleration max as a double (backward compat wrapper).</summary>
        public double DecelerationMax
        {
            get { return double.TryParse(_decelerationMaxExpr, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : 0.0; }
            set { _decelerationMaxExpr = value.ToString("0.######", CultureInfo.InvariantCulture); }
        }
        #endregion
    }
}
