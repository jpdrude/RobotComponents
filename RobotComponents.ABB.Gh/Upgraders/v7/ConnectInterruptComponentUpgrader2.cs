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
    /// Upgrades ConnectInterruptComponent_OBSOLETE2 (guid F77FEF07-D879-436A-AC00-B63FE3820BCD --
    /// the shape actually shipped on main before this branch's changes) instances to the live
    /// ConnectInterruptComponent (guid 774F2525-1176-49B6-9C8D-887EC696896A).
    /// </summary>
    /// <remarks>
    /// Second upgrader for this component -- see also ConnectInterruptComponentUpgrader (v5),
    /// which upgrades the original shipped shape to ConnectInterruptComponent_OBSOLETE2's shape.
    /// GH only ever needs one upgrade hop per placed instance (an OBSOLETE instance's guid
    /// already identifies exactly which of the two obsolete shapes it has), so the two upgraders
    /// don't chain.
    ///
    /// This upgrader jumps straight from the shipped v5-era shape to the current live shape (both
    /// the Signal Name type change/Persistent Data signal type, and the optional Interrupt
    /// Variable Name override, landed together here) rather than through an intermediate obsolete
    /// snapshot for each: no saved .gh file was ever able to reference a guid from that
    /// in-between shape, since it only ever existed on this branch's own commits before the two
    /// changes were combined into this single guid transition.
    ///
    /// Reference list — old -&gt; new parameter mapping (all indices unchanged, only the type at
    /// index 1 changed):
    ///   Inputs:
    ///     0  TRAP Routine Name (text, item)   -&gt; 0  TRAP Routine Name (unchanged)
    ///     1  Signal Name        (text, item)  -&gt; 1  Signal Name (Param_GenericObject — TYPE CHANGED)
    ///     2  Signal Value       (number, item) -&gt; 2  Signal Value (unchanged)
    ///     3  Signal Type        (integer, item) -&gt; 3  Signal Type (unchanged; enum gained PersistentData = 6)
    ///     (none)                                 -&gt; 4  Interrupt Variable Name (new, optional, off
    ///                                                  by default -- see ConnectInterruptComponent's
    ///                                                  "Override Interrupt Variable Name" right-click
    ///                                                  toggle -- nothing to migrate onto it)
    ///   Outputs:
    ///     0  Connect Code      (Param_CodeLine) -&gt; 0  Connect Code
    ///     1  Enable Interrupts (Param_CodeLine) -&gt; 1  Enable Interrupts
    ///     2  Disable Interrupts (Param_CodeLine) -&gt; 2  Disable Interrupts
    /// </remarks>
    public class ConnectInterruptComponentUpgrader2 : IGH_UpgradeObject
    {
        /// <inheritdoc/>
        public DateTime Version => new DateTime(2026, 9, 6);

        /// <inheritdoc/>
        public Guid UpgradeFrom => new Guid("F77FEF07-D879-436A-AC00-B63FE3820BCD");

        /// <inheritdoc/>
        public Guid UpgradeTo => new Guid("774F2525-1176-49B6-9C8D-887EC696896A");

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
            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 1, newComponent, 1);
            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 2, newComponent, 2);

            if (!GH_UpgradeUtil.SwapComponents(oldComponent, newComponent, false)) { return null; }
            UpgradeHelpers.MigrateGroupMembership(oldComponent, newComponent, document);
            return newComponent;
        }
    }
}
