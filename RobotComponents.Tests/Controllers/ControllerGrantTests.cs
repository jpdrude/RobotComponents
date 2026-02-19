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
    /// Tests that Controller methods fail-closed when the controller is empty
    /// (and therefore cannot acquire grants). Methods under test: UploadModule,
    /// UploadSystemModule, ResetProgramPointers, ResetProgramPointer.
    /// </summary>
    public class ControllerGrantTests
    {
        [Fact]
        public void UploadModule_EmptyController_ReturnsFalse()
        {
            Controller controller = new Controller();

            bool result = controller.UploadModule("T_ROB1", new List<string>(), out string status);

            Assert.False(result);
            Assert.Contains("empty", status, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UploadSystemModule_EmptyController_ReturnsFalse()
        {
            Controller controller = new Controller();

            bool result = controller.UploadSystemModule("T_ROB1", new List<string>(), out string status, false);

            Assert.False(result);
            Assert.Contains("empty", status, System.StringComparison.OrdinalIgnoreCase);
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
