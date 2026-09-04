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
    /// Upgrades RoutineArgumentComponent_OBSOLETE (guid 5F92D4A8-B1E7-4C63-8D2F-7A3E9B6C1D5F)
    /// instances to the live RoutineArgumentComponent (guid 7972F652-B35B-4DD1-8AC1-2A1FAE633694).
    /// </summary>
    /// <remarks>
    /// Reference list — old -&gt; new parameter mapping:
    ///   Inputs (unchanged: same names, types, order):
    ///     0  Keyword  (text, item)  -&gt; 0  Keyword
    ///     1  Type     (text, item)  -&gt; 1  Type
    ///     2  Name     (text, item)  -&gt; 2  Name
    ///     3  Value    (generic, item) -&gt; 3  Value
    ///   Outputs:
    ///     0  Argument (Param_RoutineArgument) -&gt; 0  Argument
    ///     (none)                              -&gt; 1  Variable (Param_RAPIDVariable, new — nothing to migrate)
    /// </remarks>
    public class RoutineArgumentComponentUpgrader : IGH_UpgradeObject
    {
        /// <inheritdoc/>
        public DateTime Version => new DateTime(2026, 9, 3);

        /// <inheritdoc/>
        public Guid UpgradeFrom => new Guid("5F92D4A8-B1E7-4C63-8D2F-7A3E9B6C1D5F");

        /// <inheritdoc/>
        public Guid UpgradeTo => new Guid("7972F652-B35B-4DD1-8AC1-2A1FAE633694");

        /// <inheritdoc/>
        public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
        {
            if (!(target is IGH_Component oldComponent)) { return null; }

            RoutineArgumentComponent newComponent = new RoutineArgumentComponent();

            UpgradeHelpers.MigrateInputByIndex(oldComponent, 0, newComponent, 0);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 1, newComponent, 1);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 2, newComponent, 2);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 3, newComponent, 3);

            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 0, newComponent, 0);

            return GH_UpgradeUtil.SwapComponents(oldComponent, newComponent, false) ? newComponent : null;
        }
    }
}
