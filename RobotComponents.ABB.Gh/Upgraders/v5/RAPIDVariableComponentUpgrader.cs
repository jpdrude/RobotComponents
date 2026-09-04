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
using System.Linq;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents
using RobotComponents.ABB.Gh.Components.CodeGeneration;

namespace RobotComponents.ABB.Gh.Upgraders
{
    /// <summary>
    /// Upgrades RAPIDVariableComponent_OBSOLETE (guid 3B7E1F4A-9C82-4D16-A05B-6E3D2C8F7B94)
    /// instances to the live RAPIDVariableComponent (guid 9E34F983-96A8-495D-B0C4-AC607CE805BB).
    /// </summary>
    /// <remarks>
    /// Reference list — old -&gt; new parameter mapping:
    ///   Inputs (0-4 fixed, unchanged types/order):
    ///     0  Level   (integer, item) -&gt; 0  Level
    ///     1  Scope   (integer, item) -&gt; 1  Scope
    ///     2  Keyword (integer, item) -&gt; 2  Keyword
    ///     3  Type    (text, item)    -&gt; 3  Type
    ///     4  Name    (text, item)    -&gt; 4  Name
    ///   Inputs (5+, mode-dependent):
    ///     5  Value      (Param_String, item, scalar mode) -&gt; 5  Value      (Param_GenericObject — TYPE CHANGED)
    ///     5  Array Size (Param_Integer, item, array mode)  -&gt; 5  Array Size (unchanged type)
    ///     6  Values     (Param_String, list, array mode)   -&gt; 6  Values     (Param_GenericObject — TYPE CHANGED)
    ///   Outputs:
    ///     0  Variable             (Param_RAPIDVariable) -&gt; 0  Variable
    ///     1  Variable Declaration (Param_CodeLine)       -&gt; 1  Variable Declaration
    ///
    ///   The old component's array mode is read off whether its "Array Size" input is currently
    ///   registered (name-based, same as how the component itself decides its mode in
    ///   SolveInstance). The new instance is put into the matching mode via
    ///   RAPIDVariableComponent.ConfigureForUpgrade(...) *before* wires are migrated, so the
    ///   "Value"/"Array Size"/"Values" parameters actually exist on the new instance to migrate onto.
    /// </remarks>
    public class RAPIDVariableComponentUpgrader : IGH_UpgradeObject
    {
        /// <inheritdoc/>
        public DateTime Version => new DateTime(2026, 9, 3);

        /// <inheritdoc/>
        public Guid UpgradeFrom => new Guid("3B7E1F4A-9C82-4D16-A05B-6E3D2C8F7B94");

        /// <inheritdoc/>
        public Guid UpgradeTo => new Guid("9E34F983-96A8-495D-B0C4-AC607CE805BB");

        /// <inheritdoc/>
        public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
        {
            if (!(target is IGH_Component oldComponent)) { return null; }

            bool arrayMode = oldComponent.Params.Input.Any(p => p.Name == "Array Size");

            RAPIDVariableComponent newComponent = new RAPIDVariableComponent();
            newComponent.ConfigureForUpgrade(arrayMode);

            UpgradeHelpers.MigrateInputByIndex(oldComponent, 0, newComponent, 0);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 1, newComponent, 1);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 2, newComponent, 2);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 3, newComponent, 3);
            UpgradeHelpers.MigrateInputByIndex(oldComponent, 4, newComponent, 4);
            UpgradeHelpers.MigrateInputByName(oldComponent, "Value", newComponent, "Value");
            UpgradeHelpers.MigrateInputByName(oldComponent, "Array Size", newComponent, "Array Size");
            UpgradeHelpers.MigrateInputByName(oldComponent, "Values", newComponent, "Values");

            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 0, newComponent, 0);
            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 1, newComponent, 1);

            if (!GH_UpgradeUtil.SwapComponents(oldComponent, newComponent, false)) { return null; }
            UpgradeHelpers.MigrateGroupMembership(oldComponent, newComponent, document);
            return newComponent;
        }
    }
}
