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
    /// Upgrades AssignVariableValueComponent_OBSOLETE (guid A1F4C2E7-8D53-4B96-B0A3-7C2E5F9D3A81)
    /// instances to the live AssignVariableValueComponent (guid 3699B646-6765-42CA-8E54-DB2363AA4CED).
    /// </summary>
    /// <remarks>
    /// Reference list — old -&gt; new parameter mapping:
    ///   Inputs:
    ///     0  Variable (Param_RAPIDVariable, item)         -&gt; 0  Variable (unchanged type)
    ///     1  Value    (Param_String, item, scalar mode)   -&gt; 1  Value  (Param_GenericObject — TYPE CHANGED)
    ///     1  Values   (Param_String, list, array mode)    -&gt; 1  Values (Param_GenericObject — TYPE CHANGED)
    ///     2  Index    (Param_GenericObject, item, optional, index mode only) -&gt; 2  Index (unchanged type)
    ///   Outputs:
    ///     0  Variable   (Param_RAPIDVariable, pass-through) -&gt; 0  Variable
    ///     1  Assignment (Param_CodeLine)                    -&gt; 1  Assignment
    ///
    ///   The old component's array/index mode is read off which of its inputs are currently
    ///   registered (name-based, since that is exactly how the component itself decides its mode
    ///   in SolveInstance). The new instance is put into the matching mode via
    ///   AssignVariableValueComponent.ConfigureForUpgrade(...) *before* wires are migrated, so the
    ///   "Value"/"Values"/"Index" parameters actually exist on the new instance to migrate onto.
    /// </remarks>
    public class AssignVariableValueComponentUpgrader : IGH_UpgradeObject
    {
        /// <inheritdoc/>
        public DateTime Version => new DateTime(2026, 9, 3);

        /// <inheritdoc/>
        public Guid UpgradeFrom => new Guid("A1F4C2E7-8D53-4B96-B0A3-7C2E5F9D3A81");

        /// <inheritdoc/>
        public Guid UpgradeTo => new Guid("3699B646-6765-42CA-8E54-DB2363AA4CED");

        /// <inheritdoc/>
        public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
        {
            if (!(target is IGH_Component oldComponent)) { return null; }

            bool arrayMode = oldComponent.Params.Input.Any(p => p.Name == "Values");
            bool indexEnabled = oldComponent.Params.Input.Any(p => p.Name == "Index");

            AssignVariableValueComponent newComponent = new AssignVariableValueComponent();
            newComponent.ConfigureForUpgrade(arrayMode, indexEnabled);

            UpgradeHelpers.MigrateInputByIndex(oldComponent, 0, newComponent, 0);
            UpgradeHelpers.MigrateInputByName(oldComponent, "Value", newComponent, "Value");
            UpgradeHelpers.MigrateInputByName(oldComponent, "Values", newComponent, "Values");
            UpgradeHelpers.MigrateInputByName(oldComponent, "Index", newComponent, "Index");

            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 0, newComponent, 0);
            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 1, newComponent, 1);

            return GH_UpgradeUtil.SwapComponents(oldComponent, newComponent, false) ? newComponent : null;
        }
    }
}
