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
using System.Linq;
// Grasshopper Libs
using Grasshopper.Kernel;

namespace RobotComponents.ABB.Gh.Upgraders
{
    /// <summary>
    /// Small shared helpers for the IGH_UpgradeObject implementations in this folder.
    /// </summary>
    /// <remarks>
    /// All wire migration here goes through GH_UpgradeUtil.MigrateSources / MigrateRecipients
    /// (verified via IL decompilation of Grasshopper.dll to do pure wire-only migration: they
    /// move Sources/Recipients list entries and update both ends' bidirectional references,
    /// without touching either parameter's type/identity). This is deliberately used everywhere
    /// here, even for parameters whose type did NOT change, instead of GH_UpgradeUtil's
    /// Migrate/ReplaceInputParameters bulk helpers — those transplant the actual param OBJECT
    /// (unregister from source, register on target), which would silently carry a stale param
    /// type onto the new component for any parameter whose type did change. Using the same
    /// wire-only primitive uniformly for every parameter keeps that distinction from having to
    /// be re-decided per component.
    /// </remarks>
    internal static class UpgradeHelpers
    {
        /// <summary>
        /// Migrates wires feeding into old.Params.Input[oldIndex] onto new.Params.Input[newIndex],
        /// if both indices exist.
        /// </summary>
        internal static void MigrateInputByIndex(IGH_Component oldComponent, int oldIndex, IGH_Component newComponent, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= oldComponent.Params.Input.Count) return;
            if (newIndex < 0 || newIndex >= newComponent.Params.Input.Count) return;

            GH_UpgradeUtil.MigrateSources(oldComponent.Params.Input[oldIndex], newComponent.Params.Input[newIndex]);
        }

        /// <summary>
        /// Migrates wires feeding into the old input param named <paramref name="oldName"/> onto
        /// the new input param named <paramref name="newName"/>, if both are currently registered.
        /// Used for the mode-dependent inputs on the array/index-toggle components, where the
        /// live index of the param depends on which optional inputs are enabled.
        /// </summary>
        internal static void MigrateInputByName(IGH_Component oldComponent, string oldName, IGH_Component newComponent, string newName)
        {
            IGH_Param oldParam = oldComponent.Params.Input.FirstOrDefault(p => p.Name == oldName);
            IGH_Param newParam = newComponent.Params.Input.FirstOrDefault(p => p.Name == newName);
            if (oldParam == null || newParam == null) return;

            GH_UpgradeUtil.MigrateSources(oldParam, newParam);
        }

        /// <summary>
        /// Migrates wires going out of old.Params.Output[oldIndex] onto new.Params.Output[newIndex],
        /// if both indices exist.
        /// </summary>
        internal static void MigrateOutputByIndex(IGH_Component oldComponent, int oldIndex, IGH_Component newComponent, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= oldComponent.Params.Output.Count) return;
            if (newIndex < 0 || newIndex >= newComponent.Params.Output.Count) return;

            GH_UpgradeUtil.MigrateRecipients(oldComponent.Params.Output[oldIndex], newComponent.Params.Output[newIndex]);
        }
    }
}
