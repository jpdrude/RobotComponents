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
// Rhino Libs
using Rhino.Geometry;
// RobotComponents Libs
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Actions.Declarations
{
    /// <summary>
    /// Represents a Robot Target offset with the RAPID built-in Offs(Target, X, Y, Z) function.
    /// </summary>
    /// <remarks>
    /// Unlike a <see cref="ReferenceTarget"/>, the source target is kept as an object instead of being frozen
    /// into RAPID text. This lets the RAPID Generator calculate the source target's external axis values in the
    /// context of the movement that uses it (as it does for a direct move to a Robot Target), and lets the
    /// kinematics simulate the movement when the offsets are numbers.
    /// The source target is either a <see cref="RobotTarget"/> or another <see cref="OffsetTarget"/>.
    /// </remarks>
    [Serializable()]
    public class OffsetTarget : ITarget, ISerializable
    {
        #region fields
        private ITarget _target;
        private string _x;
        private string _y;
        private string _z;
        private VariableType _variableType = VariableType.VAR;
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.
        /// </summary>
        /// <param name="info"> The SerializationInfo to extract the data from. </param>
        /// <param name="context"> The context of this deserialization. </param>
        protected OffsetTarget(SerializationInfo info, StreamingContext context)
        {
            _target = (ITarget)info.GetValue("Target", typeof(ITarget));
            _x = (string)info.GetValue("X", typeof(string));
            _y = (string)info.GetValue("Y", typeof(string));
            _z = (string)info.GetValue("Z", typeof(string));
            _variableType = (VariableType)info.GetValue("Variable Type", typeof(VariableType));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the object.
        /// </summary>
        /// <param name="info"> The SerializationInfo to populate with data. </param>
        /// <param name="context"> The destination for this serialization. </param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Target", _target, typeof(ITarget));
            info.AddValue("X", _x, typeof(string));
            info.AddValue("Y", _y, typeof(string));
            info.AddValue("Z", _z, typeof(string));
            info.AddValue("Variable Type", _variableType, typeof(VariableType));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the Offset Target class.
        /// </summary>
        /// <param name="target"> The target to offset: a Robot Target or another Offset Target. </param>
        /// <param name="x"> The offset in the X direction of the work object coordinate system as a RAPID expression. </param>
        /// <param name="y"> The offset in the Y direction of the work object coordinate system as a RAPID expression. </param>
        /// <param name="z"> The offset in the Z direction of the work object coordinate system as a RAPID expression. </param>
        public OffsetTarget(ITarget target, string x, string y, string z)
        {
            if (!(target is RobotTarget) && !(target is OffsetTarget))
            {
                throw new ArgumentException("The target of an Offset Target must be a Robot Target or an Offset Target.", nameof(target));
            }

            _target = target.DuplicateTarget();
            _x = x ?? "0";
            _y = y ?? "0";
            _z = z ?? "0";
        }

        /// <summary>
        /// Initializes a new instance of the Offset Target class by duplicating an existing instance.
        /// </summary>
        /// <param name="target"> The Offset Target instance to duplicate. </param>
        public OffsetTarget(OffsetTarget target)
        {
            _target = target._target.DuplicateTarget();
            _x = target._x;
            _y = target._y;
            _z = target._z;
            _variableType = target._variableType;
        }

        /// <summary>
        /// Returns an exact duplicate of this Offset Target instance.
        /// </summary>
        /// <returns> A deep copy of the Offset Target instance. </returns>
        public OffsetTarget Duplicate()
        {
            return new OffsetTarget(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Offset Target instance as an ITarget.
        /// </summary>
        /// <returns> A deep copy of the Offset Target instance as an ITarget. </returns>
        public ITarget DuplicateTarget()
        {
            return new OffsetTarget(this);
        }
        #endregion

        #region methods
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
        {
            if (!IsValid)
            {
                return "Invalid Offset Target";
            }

            return $"Offset Target ({ToRAPID()})";
        }

        /// <summary>
        /// Returns the Offs(...) function call as RAPID code.
        /// </summary>
        /// <remarks>
        /// A named source Robot Target is referenced by its name, an unnamed one is written inline.
        /// </remarks>
        /// <returns> The RAPID code. </returns>
        public string ToRAPID()
        {
            string target = _target is RobotTarget robotTarget && robotTarget.Name != "" ? robotTarget.Name : _target.ToRAPID();

            return $"Offs({target}, {_x}, {_y}, {_z})";
        }

        /// <summary>
        /// Returns the RAPID declaration code line of this Offset Target.
        /// </summary>
        /// <param name="robot"> The Robot were the code is generated for. </param>
        /// <returns> An empty string: an Offset Target is never declared itself. </returns>
        public string ToRAPIDDeclaration(Robot robot)
        {
            return string.Empty;
        }

        /// <summary>
        /// Creates declarations in the RAPID program module inside the RAPID Generator.
        /// </summary>
        /// <remarks>
        /// Declares the source target if it has a name, exactly like a direct move to that Robot Target would.
        /// This method is called inside the RAPID generator.
        /// </remarks>
        /// <param name="RAPIDGenerator"> The RAPID Generator. </param>
        public void ToRAPIDGenerator(RAPIDGenerator RAPIDGenerator)
        {
            _target.ToRAPIDGenerator(RAPIDGenerator);
        }

        /// <summary>
        /// Tries to get the total offset (including the offsets of nested Offset Targets) as a vector.
        /// </summary>
        /// <param name="offset"> The total offset in the work object coordinate system. </param>
        /// <returns> True if all offsets are numbers, false otherwise. </returns>
        public bool TryGetOffset(out Vector3d offset)
        {
            offset = Vector3d.Unset;

            if (!TryParseNumber(_x, out double x)) { return false; }
            if (!TryParseNumber(_y, out double y)) { return false; }
            if (!TryParseNumber(_z, out double z)) { return false; }

            Vector3d total = new Vector3d(x, y, z);

            if (_target is OffsetTarget offsetTarget)
            {
                if (!offsetTarget.TryGetOffset(out Vector3d inner)) { return false; }
                total += inner;
            }

            offset = total;
            return true;
        }

        /// <summary>
        /// Returns the Robot Target this Offset Target evaluates to, if all offsets are numbers.
        /// </summary>
        /// <remarks>
        /// RAPID's Offs() adds the offset in the work object coordinate system, which is the coordinate system the
        /// Robot Target plane is defined in. The orientation, configuration and external axis values of the source
        /// target are kept. The returned target has no name.
        /// </remarks>
        /// <returns> The offset Robot Target, or null if an offset is not a number. </returns>
        public RobotTarget GetOffsetRobotTarget()
        {
            RobotTarget source = SourceRobotTarget;

            if (source == null || !TryGetOffset(out Vector3d offset)) { return null; }

            Plane plane = new Plane(source.Plane);
            plane.Origin += offset;

            return new RobotTarget("", plane, source.ConfigurationData.Duplicate(), source.ExternalJointPosition.Duplicate());
        }

        /// <summary>
        /// Parses a RAPID expression as a number, if it is a numeric literal.
        /// </summary>
        private static bool TryParseNumber(string expression, out double value)
        {
            return double.TryParse(expression, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets a value indicating whether or not the object is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (_target == null) { return false; }
                if (_target.IsValid == false) { return false; }
                if (!RAPIDExpression.IsValidExpression(_x)) { return false; }
                if (!RAPIDExpression.IsValidExpression(_y)) { return false; }
                if (!RAPIDExpression.IsValidExpression(_z)) { return false; }
                return true;
            }
        }

        /// <summary>
        /// Gets the target that is offset: a Robot Target or another Offset Target.
        /// </summary>
        public ITarget Target
        {
            get { return _target; }
        }

        /// <summary>
        /// Gets the Robot Target at the root of this (possibly nested) Offset Target.
        /// </summary>
        public RobotTarget SourceRobotTarget
        {
            get
            {
                if (_target is RobotTarget robotTarget) { return robotTarget; }
                if (_target is OffsetTarget offsetTarget) { return offsetTarget.SourceRobotTarget; }
                return null;
            }
        }

        /// <summary>
        /// Gets the offset in the X direction of the work object coordinate system as a RAPID expression.
        /// </summary>
        public string X
        {
            get { return _x; }
        }

        /// <summary>
        /// Gets the offset in the Y direction of the work object coordinate system as a RAPID expression.
        /// </summary>
        public string Y
        {
            get { return _y; }
        }

        /// <summary>
        /// Gets the offset in the Z direction of the work object coordinate system as a RAPID expression.
        /// </summary>
        public string Z
        {
            get { return _z; }
        }

        /// <summary>
        /// Gets or sets the Variable Type.
        /// </summary>
        /// <remarks>
        /// Not used, since an Offset Target does not emit a declaration.
        /// </remarks>
        public VariableType VariableType
        {
            get { return _variableType; }
            set { _variableType = value; }
        }

        /// <summary>
        /// Gets the Offs(...) function call. Setting it has no effect.
        /// </summary>
        /// <remarks>
        /// An Offset Target has no name of its own; the function call is emitted as the target argument of the
        /// move instruction.
        /// </remarks>
        public string Name
        {
            get { return ToRAPID(); }
            set { }
        }

        /// <summary>
        /// Gets or sets the External Joint Position of the source Robot Target.
        /// </summary>
        /// <remarks>
        /// RAPID's Offs() keeps the external axis values of the target it offsets.
        /// </remarks>
        public ExternalJointPosition ExternalJointPosition
        {
            get { return SourceRobotTarget.ExternalJointPosition; }
            set { SourceRobotTarget.ExternalJointPosition = value; }
        }
        #endregion
    }
}
