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

// Grasshopper Libs
using GH_IO;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;

namespace RobotComponents.ABB.Gh.Goos.Definitions
{
    /// <summary>
    /// GH_RAPIDExpression Goo wrapper, allowing RAPIDExpression to flow through Grasshopper wires.
    /// Accepts integers, doubles, booleans, strings, and RAPIDVariable objects via casting.
    /// </summary>
    public class GH_RAPIDExpression : GH_Goo<RAPIDExpression>, GH_ISerializable
    {
        #region constructors
        /// <summary>Blank constructor.</summary>
        public GH_RAPIDExpression()
        {
            Value = null;
        }

        /// <summary>Data constructor from a RAPIDExpression instance.</summary>
        public GH_RAPIDExpression(RAPIDExpression expression)
        {
            Value = expression;
        }

        /// <summary>Copy constructor.</summary>
        public GH_RAPIDExpression(GH_RAPIDExpression goo)
        {
            Value = goo?.Value?.Duplicate();
        }

        /// <summary>Make a complete duplicate of this Goo instance.</summary>
        public override IGH_Goo Duplicate() => new GH_RAPIDExpression(Value?.Duplicate());
        #endregion

        #region properties
        /// <inheritdoc/>
        public override bool IsValid => Value != null && Value.IsValid;

        /// <inheritdoc/>
        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null) return "No internal RAPIDExpression instance.";
                if (Value.IsValid) return string.Empty;
                return "RAPID expression is empty.";
            }
        }

        /// <inheritdoc/>
        public override string ToString() => Value?.ToString() ?? "Null RAPID Expression";

        /// <inheritdoc/>
        public override string TypeName => "RAPID Expression";

        /// <inheritdoc/>
        public override string TypeDescription
            => "Defines a RAPID expression: a literal value, variable reference, function call, or arithmetic expression.";
        #endregion

        #region casting
        /// <inheritdoc/>
        public override bool CastTo<Q>(ref Q target)
        {
            if (typeof(Q).IsAssignableFrom(typeof(GH_RAPIDExpression)))
            {
                target = (Q)(object)new GH_RAPIDExpression(Value);
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(RAPIDExpression)))
            {
                target = (Q)(object)Value;
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(GH_String)))
            {
                target = (Q)(object)new GH_String(Value?.Expression ?? string.Empty);
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(string)))
            {
                target = (Q)(object)(Value?.Expression ?? string.Empty);
                return true;
            }
            target = default;
            return false;
        }

        /// <inheritdoc/>
        public override bool CastFrom(object source)
        {
            if (source == null) return false;

            // Direct RAPIDExpression
            if (source is RAPIDExpression expr)
            {
                Value = expr;
                return true;
            }
            // GH_Boolean → "1" / "0" (RAPID digital signal convention)
            if (source is GH_Boolean ghBool)
            {
                Value = RAPIDExpression.FromLiteral(ghBool.Value);
                return true;
            }
            // GH_Integer
            if (source is GH_Integer ghInt)
            {
                Value = RAPIDExpression.FromLiteral(ghInt.Value);
                return true;
            }
            // GH_Number (double)
            if (source is GH_Number ghNum)
            {
                Value = RAPIDExpression.FromLiteral(ghNum.Value);
                return true;
            }
            // GH_String — stored verbatim; component validates separately
            if (source is GH_String ghStr)
            {
                Value = RAPIDExpression.FromString(ghStr.Value);
                return true;
            }
            // raw string
            if (source is string str)
            {
                Value = RAPIDExpression.FromString(str);
                return true;
            }
            // GH_RAPIDVariable → use variable Name as expression
            if (source is GH_RAPIDVariable ghVar)
            {
                if (ghVar.Value == null || string.IsNullOrEmpty(ghVar.Value.Name)) return false;
                Value = RAPIDExpression.FromVariable(ghVar.Value);
                return true;
            }
            // RAPIDVariable
            if (source is RAPIDVariable rapid)
            {
                if (string.IsNullOrEmpty(rapid.Name)) return false;
                Value = RAPIDExpression.FromVariable(rapid);
                return true;
            }
            return false;
        }
        #endregion

        #region (de)serialization
        private const string IoKey = "RAPIDExpression";

        /// <inheritdoc/>
        public override bool Write(GH_IWriter writer)
        {
            if (Value != null)
            {
                byte[] array = RobotComponents.Utils.Serialization.ObjectToByteArray(Value);
                writer.SetByteArray(IoKey, array);
            }
            return true;
        }

        /// <inheritdoc/>
        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists(IoKey))
            {
                byte[] array = reader.GetByteArray(IoKey);
                Value = (RAPIDExpression)RobotComponents.Utils.Serialization.ByteArrayToObject(array);
                return true;
            }

            // Legacy migration: param type was changed from GH_Number / GH_Boolean to
            // GH_RAPIDExpression. GH serializes both primitive types under the key "data".
            // Try double first (covers Number inputs like Duration, Value, Override, Max),
            // then bool (covers Boolean inputs like State).
            if (reader.ItemExists("data"))
            {
                try
                {
                    Value = RAPIDExpression.FromLiteral(reader.GetDouble("data"));
                    return true;
                }
                catch { }
                try
                {
                    Value = RAPIDExpression.FromLiteral(reader.GetBoolean("data"));
                    return true;
                }
                catch { }
            }

            Value = null;
            return true;
        }
        #endregion
    }
}
