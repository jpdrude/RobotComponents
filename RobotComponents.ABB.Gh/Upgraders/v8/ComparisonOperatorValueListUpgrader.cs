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
using System.Collections.Generic;
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
// RobotComponents
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Upgraders
{
    /// <summary>
    /// Relabels a native Grasshopper Value List whose items exactly match the old, pre-fix
    /// "Comparison Operators" shape (enum member names LT/GT/LE/GE/EQ/NE, with expressions
    /// "0".."5") to the actual RAPID comparison symbols instead (see
    /// <see cref="HelperMethods.ComparisonOperatorSymbols"/>).
    /// </summary>
    /// <remarks>
    /// This is deliberately unlike every other upgrader in this project. <see cref="GH_ValueList"/>
    /// is a native Grasshopper type shared by every value list anywhere, in any file, from any
    /// plugin -- there is no separate "old RobotComponents value list class" to key an upgrade
    /// off of by ComponentGuid the normal way. <see cref="UpgradeFrom"/> is therefore
    /// GH_ValueList's own (shared) guid, and <see cref="Upgrade"/> only ever acts on a value list
    /// whose current ListItems exactly match the old 6-item signature -- both Name and Expression,
    /// in order. For every other value list on the canvas, regardless of what it's wired to or
    /// which plugin created it, this returns <see langword="null"/> (not applicable) and touches
    /// nothing.
    /// <para>
    /// Also unlike a normal component-swap upgrader, this edits the SAME live object in place --
    /// a value list's item text is just data, not a type, so there is nothing to construct or
    /// swap via GH_UpgradeUtil.SwapComponents. The object, its wires, and its currently selected
    /// item are all left exactly as they were; only the six items' display Name strings change.
    /// </para>
    /// <para>
    /// Caveat: GH_ComponentServer keeps its upgraders in a dictionary keyed by UpgradeFrom, one
    /// entry per guid (last-registered/newest Version wins) -- so if any other plugin ever
    /// registers its own upgrader against GH_ValueList's guid for an unrelated purpose, only one
    /// of the two can ever be active at a time. Given how narrow and content-specific the match
    /// above is, the practical risk is low, but it's a real limitation of piggy-backing on a
    /// shared native type's guid rather than one this project owns.
    /// </para>
    /// </remarks>
    public class ComparisonOperatorValueListUpgrader : IGH_UpgradeObject
    {
        // The pre-fix auto-created shape: enum member names, in ComparisonOperator's declared
        // order (LT=0, GT=1, LE=2, GE=3, EQ=4, NE=5) -- see the now-corrected
        // HelperMethods.ComparisonOperatorSymbols / CreateValueList(..., typeof(ComparisonOperator), ...).
        private static readonly string[] _oldNames = { "LT", "GT", "LE", "GE", "EQ", "NE" };

        /// <inheritdoc/>
        public DateTime Version => new DateTime(2026, 9, 10);

        /// <inheritdoc/>
        public Guid UpgradeFrom => new Guid("00027467-0D24-4fa7-B178-8DC0AC5F42EC"); // Grasshopper.Kernel.Special.GH_ValueList

        /// <inheritdoc/>
        public Guid UpgradeTo => new Guid("00027467-0D24-4fa7-B178-8DC0AC5F42EC"); // same object, edited in place -- see remarks

        /// <inheritdoc/>
        public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
        {
            if (!(target is GH_ValueList valueList)) { return null; }

            List<GH_ValueListItem> items = valueList.ListItems;

            if (items.Count != _oldNames.Length) { return null; }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name != _oldNames[i] || items[i].Expression != i.ToString())
                {
                    return null;
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Name = HelperMethods.ComparisonOperatorSymbols[i];
            }

            valueList.ExpireSolution(true);
            return valueList;
        }
    }
}
