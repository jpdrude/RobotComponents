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
using System.Collections.Generic;
using System.Linq;
// Grasshopper Libs
using Grasshopper;
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Gh.Components;

namespace RobotComponents.ABB.Gh
{
    /// <summary>
    /// Runs once when the plugin loads, to wire up session-wide event subscriptions that don't
    /// belong to any single component. GH auto-discovers a GH_AssemblyPriority the same way it
    /// discovers components -- no registration needed, just a public parameterless constructor.
    /// </summary>
    public class RobotComponentsPriority : GH_AssemblyPriority
    {
        /// <inheritdoc/>
        public override GH_LoadingInstruction PriorityLoad()
        {
            CentralSettings.CanvasFullNamesChanged += OnCanvasFullNamesChanged;
            return GH_LoadingInstruction.Proceed;
        }

        /// <summary>
        /// Retroactively applies GH's "Draw Full Names" canvas preference to every already-placed
        /// parameter this project itself knows a default (Name, NickName) pair for -- the ones
        /// GH's own GH_Document.ConvertNickNamesToFullNames()/ConvertFullNamesToNickNames() can
        /// never reach, since a parameter added dynamically after that walk (or on a component
        /// rebuilt by an IGH_UpgradeObject.Upgrade()) doesn't exist on the freshly constructed
        /// reference instance it diffs against. See GH_RobotComponent.OptionalParameterDefaults
        /// for the per-component data this reads.
        /// </summary>
        /// <remarks>
        /// CentralSettings.CanvasFullNames is already updated to its new value by the time this
        /// fires (the setter raises the event after assigning the field). This handler runs
        /// before GH's own document-wide conversion pass and before the canvas repaint that
        /// follows it (both happen later in the same menu-click handler), so any change made here
        /// is already reflected by the time the user sees anything redraw.
        /// <para>
        /// Only a parameter whose current (Name, NickName) still exactly matches one of its
        /// component's recorded defaults is touched -- anything the user has renamed by hand, on
        /// either side of the toggle, is left alone, the same guarantee GH's own conversion gives
        /// the parameters it does reach.
        /// </para>
        /// <para>
        /// Known gap: this only fires when the preference is actively toggled. A file opened while
        /// "Draw Full Names" is already on from an earlier session (nothing changes this session,
        /// so nothing fires) keeps whatever NickNames it was saved with.
        /// </para>
        /// </remarks>
        private static void OnCanvasFullNamesChanged()
        {
            GH_Document document = Instances.ActiveCanvas?.Document;
            if (document == null) { return; }

            bool full = CentralSettings.CanvasFullNames;

            foreach (GH_RobotComponent component in document.Objects.OfType<GH_RobotComponent>())
            {
                IReadOnlyList<(string Name, string NickName)> defaults = component.OptionalParameterDefaults;
                if (defaults.Count == 0) { continue; }

                bool changed = ApplyToSide(component.Params.Input, defaults, full);
                changed |= ApplyToSide(component.Params.Output, defaults, full);

                if (changed)
                {
                    component.Attributes?.ExpireLayout();
                }
            }
        }

        /// <summary>
        /// Applies the expand/collapse rule to one side (Params.Input or Params.Output) of a
        /// component. Returns true if anything actually changed, so the caller only needs to
        /// expire layout for components that were touched.
        /// </summary>
        private static bool ApplyToSide(IEnumerable<IGH_Param> parameters, IReadOnlyList<(string Name, string NickName)> defaults, bool full)
        {
            bool changed = false;

            foreach (IGH_Param param in parameters)
            {
                foreach ((string Name, string NickName) def in defaults)
                {
                    if (param.Name != def.Name || def.Name == def.NickName) { continue; }

                    if (full && param.NickName == def.NickName)
                    {
                        param.NickName = def.Name;
                        changed = true;
                    }
                    else if (!full && param.NickName == def.Name)
                    {
                        param.NickName = def.NickName;
                        changed = true;
                    }

                    break;
                }
            }

            return changed;
        }
    }
}
