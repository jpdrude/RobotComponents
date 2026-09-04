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
    /// Upgrades NumEntryBoxComponent_OBSOLETE (guid E7B3C9F2-4A18-4D3B-A8E6-1C5D2F7B4E93)
    /// instances to the live NumEntryBoxComponent (guid 803742EC-9493-453C-BD24-02C5304AD8F2).
    /// </summary>
    /// <remarks>
    /// Reference list — old -&gt; new parameter mapping (all indices unchanged, only the type at
    /// index 3 changed):
    ///   Inputs:
    ///     0  Variable      (Param_RAPIDVariable, item)      -&gt; 0  Variable      (unchanged)
    ///     1  Header        (text, item)                     -&gt; 1  Header        (unchanged)
    ///     2  Message       (text, list)                     -&gt; 2  Message       (unchanged)
    ///     3  Initial Value (Param_Number, item)              -&gt; 3  Initial Value (Param_RAPIDExpression — TYPE CHANGED)
    ///     4  Range         (Param_Interval, item)            -&gt; 4  Range         (unchanged)
    ///     5  As Integer    (Param_Boolean, item)              -&gt; 5  As Integer    (unchanged)
    ///   Outputs:
    ///     0  Variable             (Param_RAPIDVariable) -&gt; 0  Variable
    ///     1  Numeric Entry Box    (Param_Action)         -&gt; 1  Numeric Entry Box
    /// </remarks>
    public class NumEntryBoxComponentUpgrader : IGH_UpgradeObject
    {
        /// <inheritdoc/>
        public DateTime Version => new DateTime(2026, 9, 5);

        /// <inheritdoc/>
        public Guid UpgradeFrom => new Guid("E7B3C9F2-4A18-4D3B-A8E6-1C5D2F7B4E93");

        /// <inheritdoc/>
        public Guid UpgradeTo => new Guid("803742EC-9493-453C-BD24-02C5304AD8F2");

        /// <inheritdoc/>
        public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
        {
            if (!(target is IGH_Component oldComponent)) { return null; }

            NumEntryBoxComponent newComponent = new NumEntryBoxComponent();

            UpgradeHelpers.MigrateInputByIndex(oldComponent, 0, newComponent, 0);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 1, newComponent, 1);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 2, newComponent, 2);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 3, newComponent, 3);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 4, newComponent, 4);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 5, newComponent, 5);

            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 0, newComponent, 0);
            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 1, newComponent, 1);

            if (!GH_UpgradeUtil.SwapComponents(oldComponent, newComponent, false)) { return null; }
            UpgradeHelpers.MigrateGroupMembership(oldComponent, newComponent, document);
            return newComponent;
        }
    }
}
