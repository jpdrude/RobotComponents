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
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Actions.Declarations
{
    /// <summary>
    /// Represents a symbolic reference to a robtarget or jointtarget: a RAPID expression
    /// (typically a RAPID variable name) that is assumed to already resolve to a target
    /// value in the generated module. No declaration is emitted for this target; it is
    /// the user's responsibility to ensure the referenced variable exists in RAPID.
    /// </summary>
    [Serializable()]
    public class ReferenceTarget : ITarget, ISerializable
    {
        #region fields
        private string _expression;
        private VariableType _variableType = VariableType.VAR;
        private ExternalJointPosition _externalJointPosition = new ExternalJointPosition();
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.
        /// </summary>
        /// <param name="info"> The SerializationInfo to extract the data from. </param>
        /// <param name="context"> The context of this deserialization. </param>
        protected ReferenceTarget(SerializationInfo info, StreamingContext context)
        {
            _expression = (string)info.GetValue("Expression", typeof(string));
            _variableType = (VariableType)info.GetValue("Variable Type", typeof(VariableType));
            _externalJointPosition = (ExternalJointPosition)info.GetValue("External Joint Position", typeof(ExternalJointPosition));
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
            info.AddValue("Expression", _expression, typeof(string));
            info.AddValue("Variable Type", _variableType, typeof(VariableType));
            info.AddValue("External Joint Position", _externalJointPosition, typeof(ExternalJointPosition));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the Reference Target class.
        /// </summary>
        /// <param name="expression"> The RAPID expression, typically a RAPID variable name, that resolves to a robtarget or jointtarget. </param>
        public ReferenceTarget(string expression)
        {
            _expression = expression ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the Reference Target class by duplicating an existing instance.
        /// </summary>
        /// <param name="target"> The Reference Target instance to duplicate. </param>
        public ReferenceTarget(ReferenceTarget target)
        {
            _expression = target._expression;
            _variableType = target._variableType;
            _externalJointPosition = target._externalJointPosition.Duplicate();
        }

        /// <summary>
        /// Returns an exact duplicate of this Reference Target instance.
        /// </summary>
        /// <returns> A deep copy of the Reference Target instance. </returns>
        public ReferenceTarget Duplicate()
        {
            return new ReferenceTarget(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Reference Target instance as an ITarget.
        /// </summary>
        /// <returns> A deep copy of the Reference Target instance as an ITarget. </returns>
        public ITarget DuplicateTarget()
        {
            return new ReferenceTarget(this);
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
                return "Invalid Reference Target";
            }

            return $"Reference Target ({_expression})";
        }

        /// <summary>
        /// Returns the Reference Target in RAPID code format.
        /// </summary>
        /// <returns> The RAPID expression that is assumed to resolve to a robtarget or jointtarget. </returns>
        public string ToRAPID()
        {
            return _expression;
        }

        /// <summary>
        /// Returns the RAPID declaration code line of this action.
        /// </summary>
        /// <remarks>
        /// A Reference Target never declares anything: it references a robtarget or jointtarget
        /// that is assumed to already be declared elsewhere in the RAPID module.
        /// </remarks>
        /// <param name="robot"> The Robot were the code is generated for. </param>
        /// <returns> An empty string. </returns>
        public string ToRAPIDDeclaration(Robot robot)
        {
            return string.Empty;
        }

        /// <summary>
        /// Creates declarations and instructions in the RAPID program module inside the RAPID Generator.
        /// </summary>
        /// <remarks>
        /// This method is called inside the RAPID generator. A Reference Target does not add any
        /// declarations or instructions since it only emits its expression inline.
        /// </remarks>
        /// <param name="RAPIDGenerator"> The RAPID Generator. </param>
        public void ToRAPIDGenerator(RAPIDGenerator RAPIDGenerator)
        {
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets a value indicating whether or not the object is valid.
        /// </summary>
        public bool IsValid
        {
            get { return RAPIDExpression.IsValidExpression(_expression); }
        }

        /// <summary>
        /// Gets or sets the Variable Type.
        /// </summary>
        /// <remarks>
        /// Not used, since a Reference Target does not emit a declaration.
        /// </remarks>
        public VariableType VariableType
        {
            get { return _variableType; }
            set { _variableType = value; }
        }

        /// <summary>
        /// Gets or sets the RAPID expression that resolves to a robtarget or jointtarget.
        /// </summary>
        /// <remarks>
        /// This is emitted verbatim as the target argument of the move instruction.
        /// </remarks>
        public string Name
        {
            get { return _expression; }
            set { _expression = value ?? string.Empty; }
        }

        /// <summary>
        /// Gets or sets the External Joint Position.
        /// </summary>
        /// <remarks>
        /// Not used, since the actual target value referenced by the expression is unknown at
        /// Grasshopper-authoring time.
        /// </remarks>
        public ExternalJointPosition ExternalJointPosition
        {
            get { return _externalJointPosition; }
            set { _externalJointPosition = value; }
        }
        #endregion
    }
}
