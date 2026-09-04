// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// Xunit Libs
using Xunit;
// System Libs
using System.Collections.Generic;
// Robot Components Libs
using RobotComponents.ABB.Controllers;

namespace RobotComponents.Tests.Controllers
{
    /// <summary>
    /// Tests that Controller grant-protected methods fail-closed when the
    /// controller is empty. Methods under test: UploadModule,
    /// UploadSystemModule, ResetProgramPointers, ResetProgramPointer.
    ///
    /// UploadHelperModules is omitted because its DataTree parameter
    /// requires the Grasshopper assembly which is unavailable in CI.
    ///
    /// Note: These tests exercise the empty-controller early-return path
    /// (_isEmpty == true), not the DemandGrant failure path itself. The
    /// ABB PC SDK uses sealed types that cannot be mocked, so the grant
    /// failure behavior is verified by code review. These tests serve as
    /// regression anchors ensuring the methods return false with a
    /// descriptive status message.
    /// </summary>
    public class ControllerGrantTests
    {
        [Fact]
        public void UploadModule_EmptyController_ReturnsFalse()
        {
            Controller controller = new Controller();

            bool result = controller.UploadModule("T_ROB1", new List<string>(), out string status, out List<string> warnings);

            Assert.False(result);
            Assert.Contains("empty", status, System.StringComparison.OrdinalIgnoreCase);
            Assert.Empty(warnings);
        }

        [Fact]
        public void UploadSystemModule_EmptyController_ReturnsFalse()
        {
            Controller controller = new Controller();

            bool result = controller.UploadSystemModule(new List<string>(), out string status, out List<string> warnings);

            Assert.False(result);
            Assert.Contains("empty", status, System.StringComparison.OrdinalIgnoreCase);
            Assert.Empty(warnings);
        }

        [Fact]
        public void ResetProgramPointers_EmptyController_ReturnsFalse()
        {
            Controller controller = new Controller();

            bool result = controller.ResetProgramPointers(out string status);

            Assert.False(result);
            Assert.Contains("empty", status, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ResetProgramPointer_EmptyController_ReturnsFalse()
        {
            Controller controller = new Controller();

            bool result = controller.ResetProgramPointer("T_ROB1", out string status);

            Assert.False(result);
            Assert.Contains("empty", status, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
