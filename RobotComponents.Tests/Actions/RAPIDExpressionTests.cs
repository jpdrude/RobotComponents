// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2026 EDEK Uni Kassel
//
// For license details, see the LICENSE file in the project root.

// Xunit Libs
using Xunit;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Enumerations; // for RAPIDDataType

namespace RobotComponents.Tests.Actions
{
    public class RAPIDExpressionTests
    {
        // ── IsValidExpression (static validator) ──────────────────────────────

        [Fact]
        public void IsValidExpression_NonEmptyIdentifier_ReturnsTrue()
        {
            Assert.True(RAPIDExpression.IsValidExpression("myVar"));
        }

        [Fact]
        public void IsValidExpression_ArithmeticExpression_ReturnsTrue()
        {
            Assert.True(RAPIDExpression.IsValidExpression("a + b * 2"));
        }

        [Fact]
        public void IsValidExpression_FunctionCall_ReturnsTrue()
        {
            Assert.True(RAPIDExpression.IsValidExpression("Abs(-5)"));
        }

        [Fact]
        public void IsValidExpression_NestedFunctionCall_ReturnsTrue()
        {
            Assert.True(RAPIDExpression.IsValidExpression("Sqrt(x * x + y * y)"));
        }

        [Fact]
        public void IsValidExpression_BooleanLiteral_ReturnsTrue()
        {
            Assert.True(RAPIDExpression.IsValidExpression("TRUE"));
            Assert.True(RAPIDExpression.IsValidExpression("FALSE"));
        }

        [Fact]
        public void IsValidExpression_NumericLiteral_ReturnsTrue()
        {
            Assert.True(RAPIDExpression.IsValidExpression("42"));
            Assert.True(RAPIDExpression.IsValidExpression("3.14"));
        }

        [Fact]
        public void IsValidExpression_EmptyString_ReturnsFalse()
        {
            Assert.False(RAPIDExpression.IsValidExpression(""));
        }

        [Fact]
        public void IsValidExpression_WhitespaceOnly_ReturnsFalse()
        {
            Assert.False(RAPIDExpression.IsValidExpression("   "));
        }

        [Fact]
        public void IsValidExpression_ContainsSemicolon_ReturnsFalse()
        {
            Assert.False(RAPIDExpression.IsValidExpression("myVar; GOTO hack"));
        }

        [Fact]
        public void IsValidExpression_ContainsLineComment_ReturnsFalse()
        {
            Assert.False(RAPIDExpression.IsValidExpression("myVar // comment"));
        }

        [Fact]
        public void IsValidExpression_ContainsNewline_ReturnsFalse()
        {
            Assert.False(RAPIDExpression.IsValidExpression("myVar\nother"));
        }

        // ── Factory methods ───────────────────────────────────────────────────

        [Fact]
        public void FromLiteralInt_ProducesInvariantCultureString()
        {
            RAPIDExpression expr = RAPIDExpression.FromLiteral(42);

            Assert.Equal("42", expr.Expression);
        }

        [Fact]
        public void FromLiteralDouble_ProducesCorrectString()
        {
            RAPIDExpression expr = RAPIDExpression.FromLiteral(3.14);

            Assert.Equal("3.14", expr.Expression);
        }

        [Fact]
        public void FromLiteralDouble_WholeNumber_OmitsTrailingDecimal()
        {
            RAPIDExpression expr = RAPIDExpression.FromLiteral(5.0);

            Assert.Equal("5", expr.Expression);
        }

        [Fact]
        public void FromLiteralDouble_UsesInvariantCulture()
        {
            // Ensure no locale-dependent comma separator
            RAPIDExpression expr = RAPIDExpression.FromLiteral(1.5);

            Assert.Equal("1.5", expr.Expression);
            Assert.DoesNotContain(",", expr.Expression);
        }

        [Fact]
        public void FromLiteralBoolTrue_ProducesOne()
        {
            RAPIDExpression expr = RAPIDExpression.FromLiteral(true);

            Assert.Equal("1", expr.Expression);
        }

        [Fact]
        public void FromLiteralBoolFalse_ProducesZero()
        {
            RAPIDExpression expr = RAPIDExpression.FromLiteral(false);

            Assert.Equal("0", expr.Expression);
        }

        [Fact]
        public void FromVariable_UsesVariableName()
        {
            RAPIDVariable variable = new RAPIDVariable();
            variable.Name = "myCounter";

            RAPIDExpression expr = RAPIDExpression.FromVariable(variable);

            Assert.Equal("myCounter", expr.Expression);
        }

        [Fact]
        public void FromString_StoresVerbatim()
        {
            RAPIDExpression expr = RAPIDExpression.FromString("someVar + 1");

            Assert.Equal("someVar + 1", expr.Expression);
        }

        [Fact]
        public void FromFunctionCall_NoArgs_ProducesEmptyParens()
        {
            RAPIDExpression expr = RAPIDExpression.FromFunctionCall("GetTime",
                new RAPIDExpression[0]);

            Assert.Equal("GetTime()", expr.Expression);
        }

        [Fact]
        public void FromFunctionCall_WithArgs_ProducesCommaSeparatedArgs()
        {
            RAPIDExpression expr = RAPIDExpression.FromFunctionCall("Max", new[]
            {
                RAPIDExpression.FromLiteral(3.0),
                RAPIDExpression.FromString("myVar")
            });

            Assert.Equal("Max(3, myVar)", expr.Expression);
        }

        [Fact]
        public void FromFunctionCall_Nested_ProducesCorrectExpression()
        {
            RAPIDExpression inner = RAPIDExpression.FromFunctionCall("Abs",
                new[] { RAPIDExpression.FromString("x") });
            RAPIDExpression outer = RAPIDExpression.FromFunctionCall("Sqrt",
                new[] { inner });

            Assert.Equal("Sqrt(Abs(x))", outer.Expression);
        }

        // ── IsValid property ─────────────────────────────────────────────────

        [Fact]
        public void IsValid_NonEmptyExpression_ReturnsTrue()
        {
            Assert.True(new RAPIDExpression("myVar").IsValid);
        }

        [Fact]
        public void IsValid_EmptyExpression_ReturnsFalse()
        {
            Assert.False(new RAPIDExpression("").IsValid);
        }

        [Fact]
        public void IsValid_NullConstructorArg_ReturnsFalse()
        {
            Assert.False(new RAPIDExpression(null).IsValid);
        }

        // ── DataType hint ─────────────────────────────────────────────────────

        [Fact]
        public void FromLiteralDouble_DataType_IsNum()
        {
            Assert.Equal(RAPIDDataType.Num, RAPIDExpression.FromLiteral(1.0).DataType);
        }

        [Fact]
        public void FromLiteralBool_DataType_IsBool()
        {
            Assert.Equal(RAPIDDataType.Bool, RAPIDExpression.FromLiteral(true).DataType);
        }

        [Fact]
        public void FromVariable_DataType_IsAny()
        {
            RAPIDVariable v = new RAPIDVariable();
            v.Name = "x";
            Assert.Equal(RAPIDDataType.Any, RAPIDExpression.FromVariable(v).DataType);
        }

        [Fact]
        public void DefaultConstructor_DataType_IsAny()
        {
            Assert.Equal(RAPIDDataType.Any, new RAPIDExpression("x").DataType);
        }

        [Fact]
        public void ExplicitDataType_RoundTrips()
        {
            RAPIDExpression expr = new RAPIDExpression("x", RAPIDDataType.Num);

            Assert.Equal(RAPIDDataType.Num, expr.DataType);
        }

        // ── ToString ─────────────────────────────────────────────────────────

        [Fact]
        public void ToString_ReturnsExpression()
        {
            Assert.Equal("myVar", new RAPIDExpression("myVar").ToString());
        }

        [Fact]
        public void ToString_EmptyExpression_ReturnsMessage()
        {
            string result = new RAPIDExpression("").ToString();

            Assert.Equal("Empty RAPID Expression", result);
        }

        // ── Duplicate ────────────────────────────────────────────────────────

        [Fact]
        public void Duplicate_ProducesEqualExpression()
        {
            RAPIDExpression original = new RAPIDExpression("myVar", RAPIDDataType.Num);
            RAPIDExpression copy = original.Duplicate();

            Assert.Equal(original.Expression, copy.Expression);
            Assert.Equal(original.DataType, copy.DataType);
        }
    }
}
