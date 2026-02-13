// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
// Xunit Libs
using Xunit;

namespace RobotComponents.Tests
{
    public class VersionNumberingTests
    {
        [Fact]
        public void CurrentVersion_ParsesAsValidVersion()
        {
            bool parsed = Version.TryParse(VersionNumbering.CurrentVersion, out Version version);

            Assert.True(parsed, $"CurrentVersion \"{VersionNumbering.CurrentVersion}\" should parse as a valid System.Version");
            Assert.NotNull(version);
        }

        [Fact]
        public void CurrentVersion_MatchesVersionProperty()
        {
            string fromConst = VersionNumbering.CurrentVersion;
            string fromVersion = VersionNumbering.Version.ToString();

            Assert.Equal(fromConst, fromVersion);
        }

        [Fact]
        public void Version_HasExpectedComponents()
        {
            Version version = VersionNumbering.Version;

            Assert.True(version.Major >= 0, "Major version should be non-negative");
            Assert.True(version.Minor >= 0, "Minor version should be non-negative");
            Assert.True(version.Build >= 0, "Build version should be non-negative");
        }
    }
}
