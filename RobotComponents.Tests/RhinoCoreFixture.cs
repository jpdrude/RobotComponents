// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

using System;
using Xunit;

namespace RobotComponents.Tests
{
    /// <summary>
    /// Initializes the Rhino runtime once for all tests that require RhinoCommon native libraries.
    /// The Rhino 7 (or 8) System directory must be on PATH before running dotnet test.
    /// </summary>
    public class RhinoCoreFixture : IDisposable
    {
        private Rhino.Runtime.InProcess.RhinoCore _rhinoCore;

        /// <summary>
        /// True when RhinoCore initialized successfully. False when the Rhino System
        /// directory is not on PATH or the license could not be acquired.
        /// Tests that actually call into RhinoCommon will still fail on their own;
        /// tests that don't need Rhino are not penalized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        public RhinoCoreFixture()
        {
            try
            {
                _rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);
                IsInitialized = true;
            }
            catch
            {
                IsInitialized = false;
            }
        }

        public void Dispose()
        {
            _rhinoCore?.Dispose();
        }
    }

    /// <summary>
    /// xUnit collection that shares a single <see cref="RhinoCoreFixture"/> instance across
    /// all test classes that require the Rhino native runtime.
    /// Tests within this collection run sequentially (Rhino's native layer is not thread-safe).
    /// </summary>
    [CollectionDefinition("RequiresRhino", DisableParallelization = true)]
    public class RhinoCollection : ICollectionFixture<RhinoCoreFixture> { }
}
