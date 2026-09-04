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
    /// Upgrades EmptyLineComponent_OBSOLETE (guid F8C2A4E1-6B37-4D5A-B9F3-2E1C8D5A7B40)
    /// instances to the live EmptyLineComponent (guid 2BDB6D70-99CB-4509-BCEB-67031D588450).
    /// </summary>
    /// <remarks>
    /// Reference list — old -&gt; new parameter mapping:
    ///   Inputs: old had none (always instruction-type); the new component also has no inputs
    ///     by default (right-click "Add Type Input" adds the optional "Type" selector) — the
    ///     default shape already matches the old always-instruction behaviour, so nothing needs
    ///     enabling here and there are no wires to migrate.
    ///   Outputs:
    ///     0  Empty Line (Param_Action) -&gt; 0  Empty Line
    /// </remarks>
    public class EmptyLineComponentUpgrader : IGH_UpgradeObject
    {
        /// <inheritdoc/>
        public DateTime Version => new DateTime(2026, 9, 3);

        /// <inheritdoc/>
        public Guid UpgradeFrom => new Guid("F8C2A4E1-6B37-4D5A-B9F3-2E1C8D5A7B40");

        /// <inheritdoc/>
        public Guid UpgradeTo => new Guid("2BDB6D70-99CB-4509-BCEB-67031D588450");

        /// <inheritdoc/>
        public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
        {
            if (!(target is IGH_Component oldComponent)) { return null; }

            EmptyLineComponent newComponent = new EmptyLineComponent();

            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 0, newComponent, 0);

            if (!GH_UpgradeUtil.SwapComponents(oldComponent, newComponent, false)) { return null; }
            UpgradeHelpers.MigrateGroupMembership(oldComponent, newComponent, document);
            return newComponent;
        }
    }
}
