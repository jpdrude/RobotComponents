// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Actions.Declarations;

namespace RobotComponents.Tests.Actions
{
    public class ExternalJointPositionTests
    {
        #region Constructor
        [Fact]
        public void Constructor_Default_AllAxes9E9()
        {
            ExternalJointPosition ejp = new ExternalJointPosition();

            Assert.Equal(9e9, ejp[0]);
            Assert.Equal(9e9, ejp[1]);
            Assert.Equal(9e9, ejp[2]);
            Assert.Equal(9e9, ejp[3]);
            Assert.Equal(9e9, ejp[4]);
            Assert.Equal(9e9, ejp[5]);
        }

        [Fact]
        public void Constructor_SingleAxis_OthersDefault()
        {
            ExternalJointPosition ejp = new ExternalJointPosition(100);

            Assert.Equal(100, ejp[0]);
            Assert.Equal(9e9, ejp[1]);
            Assert.Equal(9e9, ejp[2]);
            Assert.Equal(9e9, ejp[3]);
            Assert.Equal(9e9, ejp[4]);
            Assert.Equal(9e9, ejp[5]);
        }

        [Fact]
        public void Constructor_TwoAxes_RestDefault()
        {
            ExternalJointPosition ejp = new ExternalJointPosition(100, 200);

            Assert.Equal(100, ejp[0]);
            Assert.Equal(200, ejp[1]);
            Assert.Equal(9e9, ejp[2]);
        }

        [Fact]
        public void Constructor_Named_SetsNameAndValues()
        {
            ExternalJointPosition ejp = new ExternalJointPosition("ej1", 100, 200);

            Assert.Equal("ej1", ejp.Name);
            Assert.Equal(100, ejp[0]);
            Assert.Equal(200, ejp[1]);
            Assert.Equal(9e9, ejp[2]);
        }

        [Fact]
        public void Constructor_NaN_ReplacedWithDefault()
        {
            ExternalJointPosition ejp = new ExternalJointPosition(double.NaN);

            Assert.Equal(9e9, ejp[0]);
        }
        #endregion

        #region ToRAPID
        [Fact]
        public void ToRAPID_AllDefault_ReturnsAll9E9()
        {
            ExternalJointPosition ejp = new ExternalJointPosition();

            Assert.Equal("[9E9, 9E9, 9E9, 9E9, 9E9, 9E9]", ejp.ToRAPID());
        }

        [Fact]
        public void ToRAPID_FirstAxisSet_Others9E9()
        {
            ExternalJointPosition ejp = new ExternalJointPosition(100);

            Assert.Equal("[100, 9E9, 9E9, 9E9, 9E9, 9E9]", ejp.ToRAPID());
        }

        [Fact]
        public void ToRAPID_DecimalValue_FormatsCorrectly()
        {
            // Format is {val:0.##} so 100.5 → "100.5", 100.456 → "100.46"
            ExternalJointPosition ejp = new ExternalJointPosition(100.5);

            Assert.Equal("[100.5, 9E9, 9E9, 9E9, 9E9, 9E9]", ejp.ToRAPID());
        }
        #endregion

        #region ToRAPIDDeclaration
        [Fact]
        public void ToRAPIDDeclaration_Named_ReturnsExtjointDeclaration()
        {
            ExternalJointPosition ejp = new ExternalJointPosition("ej1", 100);

            Assert.Equal("VAR extjoint ej1 := [100, 9E9, 9E9, 9E9, 9E9, 9E9];", ejp.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_Unnamed_ReturnsEmpty()
        {
            ExternalJointPosition ejp = new ExternalJointPosition(100);

            Assert.Equal(string.Empty, ejp.ToRAPIDDeclaration(null));
        }
        #endregion

        #region ToRAPIDInstruction
        [Fact]
        public void ToRAPIDInstruction_ReturnsEmpty()
        {
            ExternalJointPosition ejp = new ExternalJointPosition("ej1", 100);

            Assert.Equal(string.Empty, ejp.ToRAPIDInstruction(null));
        }
        #endregion

        #region Indexer
        [Fact]
        public void Indexer_SetAndGet_RoundTrips()
        {
            ExternalJointPosition ejp = new ExternalJointPosition();
            ejp[0] = 42;

            Assert.Equal(42, ejp[0]);
        }

        [Fact]
        public void Indexer_CharAccess_MatchesIntAccess()
        {
            ExternalJointPosition ejp = new ExternalJointPosition(10, 20, 30, 40, 50, 60);

            Assert.Equal(ejp[0], ejp['a']);
            Assert.Equal(ejp[1], ejp['b']);
            Assert.Equal(ejp[5], ejp['f']);
        }
        #endregion

        #region IsValid
        [Fact]
        public void IsValid_AllDefault_ReturnsTrue()
        {
            ExternalJointPosition ejp = new ExternalJointPosition();

            Assert.True(ejp.IsValid);
        }

        [Fact]
        public void IsValid_WithValues_ReturnsTrue()
        {
            ExternalJointPosition ejp = new ExternalJointPosition(100, 200);

            Assert.True(ejp.IsValid);
        }
        #endregion

        #region Duplicate
        [Fact]
        public void Duplicate_ReturnsMatchingValues()
        {
            ExternalJointPosition original = new ExternalJointPosition("ej1", 10, 20, 30, 40, 50, 60);

            ExternalJointPosition copy = original.Duplicate();

            Assert.Equal(original.Name, copy.Name);
            for (int i = 0; i < 6; i++)
            {
                Assert.Equal(original[i], copy[i]);
            }
        }
        #endregion

        #region Length
        [Fact]
        public void Length_AlwaysSix()
        {
            ExternalJointPosition ejp = new ExternalJointPosition();

            Assert.Equal(6, ejp.Length);
        }
        #endregion
    }
}
