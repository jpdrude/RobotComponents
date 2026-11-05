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
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
// RobotComponents
using RobotComponents.ABB.Gh.Components.CodeGeneration;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Upgraders
{
    /// <summary>
    /// Upgrades ConfigurationDataComponent_OBSOLETE (guid F41C3E63-C063-4783-866D-566AF7E01C35)
    /// instances to the live ConfigurationDataComponent (guid d9d6d79d-e967-447e-b26e-fc6bd9aa0d42).
    /// </summary>
    /// <remarks>
    /// Reference list — old -&gt; new parameter mapping (shape and types unchanged; the defaults
    /// changed from -1 ("not set") to 0, and the values are no longer clamped or derived from
    /// each other):
    ///   Inputs:
    ///     0  Name  (text, item)     -&gt; 0  Name  (unchanged)
    ///     1  CF1   (integer, item)  -&gt; 1  CF1   (default -1 -&gt; 0)
    ///     2  CF4   (integer, item)  -&gt; 2  CF4   (default -1 -&gt; 0)
    ///     3  CF6   (integer, item)  -&gt; 3  CF6   (default -1 -&gt; 0)
    ///     4  CFX   (integer, item)  -&gt; 4  CFX   (default -1 -&gt; 0)
    ///   Outputs:
    ///     0  Configuration Data (Param_ConfigurationData) -&gt; 0  Configuration Data
    /// Values typed into an unwired input (persistent data) are carried over, except the old
    /// "not set" value -1 on CF1..CFX, which becomes the new default 0.
    /// </remarks>
    public class ConfigurationDataComponentUpgrader : IGH_UpgradeObject
    {
        /// <inheritdoc/>
        public DateTime Version => new DateTime(2026, 11, 5);

        /// <inheritdoc/>
        public Guid UpgradeFrom => new Guid("F41C3E63-C063-4783-866D-566AF7E01C35");

        /// <inheritdoc/>
        public Guid UpgradeTo => new Guid("d9d6d79d-e967-447e-b26e-fc6bd9aa0d42");

        /// <inheritdoc/>
        public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
        {
            if (!(target is IGH_Component oldComponent)) { return null; }

            ConfigurationDataComponent newComponent = new ConfigurationDataComponent();

            for (int i = 0; i < 5; i++)
            {
                UpgradeHelpers.MigrateInputByIndex(oldComponent, i, newComponent, i);
            }

            CopyPersistentText(oldComponent, newComponent, 0);
            for (int i = 1; i < 5; i++)
            {
                CopyPersistentInteger(oldComponent, newComponent, i);
            }

            UpgradeHelpers.MigrateOutputByIndex(oldComponent, 0, newComponent, 0);

            if (!GH_UpgradeUtil.SwapComponents(oldComponent, newComponent, false)) { return null; }
            UpgradeHelpers.MigrateGroupMembership(oldComponent, newComponent, document);
            HelperMethods.ApplyFullNamesPreference(newComponent);
            return newComponent;
        }

        private static void CopyPersistentText(IGH_Component oldComponent, IGH_Component newComponent, int index)
        {
            if (oldComponent.Params.Input[index] is Param_String oldParam && newComponent.Params.Input[index] is Param_String newParam)
            {
                newParam.PersistentData.Clear();
                newParam.PersistentData.AppendRange(oldParam.PersistentData.AllData(true).Cast<GH_String>());
            }
        }

        private static void CopyPersistentInteger(IGH_Component oldComponent, IGH_Component newComponent, int index)
        {
            if (!(oldComponent.Params.Input[index] is Param_Integer oldParam) || !(newComponent.Params.Input[index] is Param_Integer newParam)) { return; }

            bool onlyUnset = true;
            foreach (GH_Integer value in oldParam.PersistentData.AllData(true))
            {
                if (value == null || value.Value != -1) { onlyUnset = false; break; }
            }

            if (onlyUnset) { return; }

            newParam.PersistentData.Clear();
            foreach (GH_Integer value in oldParam.PersistentData.AllData(true))
            {
                newParam.PersistentData.Append(value == null ? null : new GH_Integer(value.Value == -1 ? 0 : value.Value));
            }
        }
    }
}
