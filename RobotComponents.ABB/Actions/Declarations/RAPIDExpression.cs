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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
// RobotComponents Libs
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Actions.Declarations
{
    /// <summary>
    /// Represents a RAPID expression: a literal value, variable reference, function call,
    /// or arbitrary arithmetic/logical expression that can be emitted verbatim into RAPID code.
    /// </summary>
    [Serializable()]
    public class RAPIDExpression : ISerializable
    {
        #region fields
        private readonly string _expression;
        private readonly RAPIDDataType _dataType;
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.
        /// </summary>
        protected RAPIDExpression(SerializationInfo info, StreamingContext context)
        {
            _expression = (string)info.GetValue("Expression", typeof(string));
            _dataType = (RAPIDDataType)info.GetValue("DataType", typeof(RAPIDDataType));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the object.
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Expression", _expression, typeof(string));
            info.AddValue("DataType", _dataType, typeof(RAPIDDataType));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new RAPIDExpression with the given expression string and optional data type hint.
        /// </summary>
        /// <param name="expression"> The RAPID expression string, emitted verbatim into generated code. </param>
        /// <param name="dataType"> Advisory RAPID data type hint. Default is Any. </param>
        public RAPIDExpression(string expression, RAPIDDataType dataType = RAPIDDataType.Any)
        {
            _expression = expression ?? string.Empty;
            _dataType = dataType;
        }

        /// <summary>
        /// Returns an exact duplicate of this RAPIDExpression.
        /// </summary>
        public RAPIDExpression Duplicate() => new RAPIDExpression(_expression, _dataType);

        // ── Factory methods ────────────────────────────────────────────────────

        /// <summary>Creates a RAPIDExpression from an integer literal.</summary>
        public static RAPIDExpression FromLiteral(int value)
            => new RAPIDExpression(value.ToString(CultureInfo.InvariantCulture), RAPIDDataType.Num);

        /// <summary>Creates a RAPIDExpression from a double literal.</summary>
        public static RAPIDExpression FromLiteral(double value)
            => new RAPIDExpression(value.ToString("0.######", CultureInfo.InvariantCulture), RAPIDDataType.Num);

        /// <summary>
        /// Creates a RAPIDExpression from a boolean literal.
        /// Emits <c>1</c> or <c>0</c>, matching the RAPID convention for digital signal values.
        /// </summary>
        public static RAPIDExpression FromLiteral(bool value)
            => new RAPIDExpression(value ? "1" : "0", RAPIDDataType.Bool);

        /// <summary>Creates a RAPIDExpression that references a RAPIDVariable by name.</summary>
        public static RAPIDExpression FromVariable(RAPIDVariable variable)
            => new RAPIDExpression(variable?.Name ?? string.Empty, RAPIDDataType.Any);

        /// <summary>
        /// Creates a RAPIDExpression from an arbitrary string.
        /// The string is used verbatim; call <see cref="IsValidExpression"/> first for advisory validation.
        /// </summary>
        public static RAPIDExpression FromString(string expression)
            => new RAPIDExpression(expression ?? string.Empty, RAPIDDataType.Any);

        /// <summary>
        /// Creates a RAPIDExpression representing a FUNC call: <c>name(arg1, arg2, ...)</c>.
        /// Each argument is itself a RAPIDExpression, enabling full composability.
        /// </summary>
        /// <param name="name"> The RAPID function name. </param>
        /// <param name="args"> The argument expressions. </param>
        public static RAPIDExpression FromFunctionCall(string name, IEnumerable<RAPIDExpression> args)
        {
            string argList = string.Join(", ", args.Select(a => a._expression));
            return new RAPIDExpression($"{name}({argList})", RAPIDDataType.Any);
        }
        #endregion

        #region methods
        /// <summary>
        /// Returns a string that represents the current object (the RAPID expression itself).
        /// </summary>
        public override string ToString() => string.IsNullOrEmpty(_expression) ? "Empty RAPID Expression" : _expression;

        /// <summary>
        /// Performs a lightweight check that the string could be a valid RAPID expression.
        /// Rejects empty strings, statement terminators (<c>;</c>), comment markers (<c>//</c>),
        /// and newlines. The RAPID compiler is the authoritative validator.
        /// </summary>
        /// <param name="expression"> The expression string to check. </param>
        /// <returns> <see langword="true"/> if the string passes all basic checks. </returns>
        public static bool IsValidExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) return false;
            if (expression.Contains(";")) return false;
            if (expression.Contains("//")) return false;
            if (expression.IndexOf('\n') >= 0 || expression.IndexOf('\r') >= 0) return false;
            return true;
        }
        #endregion

        #region properties
        /// <summary>Gets a value indicating whether the expression is non-empty.</summary>
        public bool IsValid => !string.IsNullOrEmpty(_expression);

        /// <summary>Gets the RAPID expression string. This is emitted verbatim into generated code.</summary>
        public string Expression => _expression;

        /// <summary>Gets the advisory RAPID data type hint.</summary>
        public RAPIDDataType DataType => _dataType;
        #endregion
    }
}
