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
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents
using RobotComponents.ABB.Gh.Components.CodeGeneration;

namespace RobotComponents.ABB.Gh.Upgraders
{
    /// <summary>
    /// Upgrades ConnectInterruptComponent_OBSOLETE (guid F7A3C8E1-9D42-4B6F-A158-2E7D0C9B34F6)
    /// instances to the live ConnectInterruptComponent (guid F77FEF07-D879-436A-AC00-B63FE3820BCD).
    /// </summary>
    /// <remarks>
    /// Reference list — old -&gt; new parameter mapping:
    ///   Inputs (unchanged types/order; index 3 was renamed "SignalType" -&gt; "Signal Type", display only):
    ///     0  TRAP Routine Name (text, item)   -&gt; 0  TRAP Routine Name
    ///     1  Signal Name       (text, item)   -&gt; 1  Signal Name
    ///     2  Signal Value      (number, item) -&gt; 2  Signal Value
    ///     3  SignalType        (integer, item) -&gt; 3  Signal Type
    ///   Outputs:
    ///     0  Connect Code (Param_CodeLine) -&gt; 0  Connect Code
    ///     (none)                            -&gt; 1  Enable Interrupts  (Param_CodeLine, new — nothing to migrate)
    ///     (none)                            -&gt; 2  Disable Interrupts (Param_CodeLine, new — nothing to migrate)
    /// </remarks>
    public class ConnectInterruptComponentUpgrader : IGH_UpgradeObject
    {
        /// <inheritdoc/>
        public DateTime Version => new DateTime(2026, 9, 3);

        /// <inheritdoc/>
        public Guid UpgradeFrom => new Guid("F7A3C8E1-9D42-4B6F-A158-2E7D0C9B34F6");

        /// <inheritdoc/>
        public Guid UpgradeTo => new Guid("F77FEF07-D879-436A-AC00-B63FE3820BCD");

        /// <inheritdoc/>
        public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
        {
            if (!(target is IGH_Component oldComponent)) { return null; }

            ConnectInterruptComponent newComponent = new ConnectInterruptComponent();

            UpgradeHelpers.MigrateInputByIndex(oldComponent, 0, newComponent, 0);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 1, newComponent, 1);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 2, newComponent, 2);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 3, newComponent, 3);

            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 0, newComponent, 0);

            return GH_UpgradeUtil.SwapComponents(oldComponent, newComponent, false) ? newComponent : null;
        }
    }
}
