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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
// Grasshopper Libs
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
// RobotComponents Libs
using RobotComponents.ABB.Gh.Components;

namespace RobotComponents.ABB.Gh
{
    /// <summary>
    /// Runs once when the plugin loads, to wire up session-wide event subscriptions and a single
    /// "Fix RC Parameter Names" menu item that don't belong to any single component. GH
    /// auto-discovers a GH_AssemblyPriority the same way it discovers components -- no
    /// registration needed, just a public parameterless constructor.
    /// </summary>
    public class RobotComponentsPriority : GH_AssemblyPriority
    {
        private bool _menuAdded = false;

        /// <inheritdoc/>
        public override GH_LoadingInstruction PriorityLoad()
        {
            CentralSettings.CanvasFullNamesChanged += OnCanvasFullNamesChanged;
            Instances.CanvasCreated += OnCanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        #region menu
        /// <summary>
        /// The known-native GH_ValueList component guid -- see FixRCParameterNames' comparison
        /// operator half and IsApplicable below.
        /// </summary>
        private static readonly Guid _valueListGuid = new Guid("00027467-0D24-4fa7-B178-8DC0AC5F42EC");

        private static HashSet<Guid> _relevantGuids;

        /// <summary>
        /// The set of guids "Fix RC Parameter Names" can ever do anything for: every
        /// GH_RobotComponent type in this assembly whose OptionalParameterDefaults is non-empty,
        /// plus GH_ValueList's own guid. Computed once (via reflection over this assembly, each
        /// candidate type instantiated once through its required public parameterless
        /// constructor -- the same precondition GH's own component discovery already relies on)
        /// and cached; used only for the cheap, guid-based menu-enablement check in
        /// IsApplicable -- the actual fix still content-matches each object it touches before
        /// changing anything.
        /// </summary>
        private static HashSet<Guid> RelevantGuids
        {
            get
            {
                if (_relevantGuids == null)
                {
                    HashSet<Guid> guids = new HashSet<Guid> { _valueListGuid };

                    foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                    {
                        if (type.IsAbstract || !typeof(GH_RobotComponent).IsAssignableFrom(type)) { continue; }

                        try
                        {
                            if (Activator.CreateInstance(type) is GH_RobotComponent component
                                && component.OptionalParameterDefaults.Count > 0)
                            {
                                guids.Add(component.ComponentGuid);
                            }
                        }
                        catch
                        {
                            // Skip defensively rather than let one type that doesn't tolerate a
                            // bare construct-then-discard break menu enablement for everything else.
                        }
                    }

                    _relevantGuids = guids;
                }

                return _relevantGuids;
            }
        }

        /// <summary>
        /// Adds "Fix RC Parameter Names" to GH's own Solution menu, right after "Upgrade
        /// Components", the first time a canvas is created (Instances.DocumentEditor / its main
        /// menu aren't valid any earlier than this -- PriorityLoad() itself runs before any editor
        /// window exists). Guarded by _menuAdded since CanvasCreated could in principle fire more
        /// than once in a session.
        /// </summary>
        private void OnCanvasCreated(GH_Canvas canvas)
        {
            if (_menuAdded) { return; }

            // GH_DocumentEditor.MainMenu (and .mnuSolution/.mnuUpgradeComponents) all exist but are
            // internal to Grasshopper.dll (confirmed via IL: "assembly" visibility). GH_DocumentEditor
            // is an ordinary System.Windows.Forms.Form though, and the standard, publicly inherited
            // Form.MainMenuStrip property points at the exact same control once a form has one, so
            // it's used here instead, with a Controls-search fallback in case a future GH version
            // stops assigning MainMenuStrip for whatever reason.
            Form editor = Instances.DocumentEditor;
            if (editor == null) { return; }

            MenuStrip mainMenu = editor.MainMenuStrip
                ?? editor.Controls.OfType<MenuStrip>().FirstOrDefault();
            if (mainMenu == null) { return; }

            ToolStripMenuItem fixItem = new ToolStripMenuItem("Fix RC Parameter Names", null, (s, e) => FixRCParameterNames());

            // Find "Upgrade Components" by its WinForms Name, not its (localizable, GH-version-
            // dependent) displayed Text -- confirmed via IL that GH's own designer code sets
            // ToolStripItem.Name to the literal, hardcoded string "mnuUpgradeComponents", which is
            // far more stable to match against than display text ever would be.
            // ToolStripItemCollection.Find(..., searchAllChildren: true) finds it regardless of
            // which submenu it's nested in.
            ToolStripMenuItem upgradeItem = mainMenu.Items.Find("mnuUpgradeComponents", true)
                .OfType<ToolStripMenuItem>().FirstOrDefault();

            if (upgradeItem?.OwnerItem is ToolStripMenuItem solutionMenu)
            {
                int index = solutionMenu.DropDownItems.IndexOf(upgradeItem);
                solutionMenu.DropDownItems.Insert(index + 1, fixItem);
                solutionMenu.DropDownOpening += (s, e) => fixItem.Enabled = IsApplicable(Instances.ActiveCanvas?.Document);
            }
            else
            {
                // Fallback: couldn't find "Upgrade Components" under that name (a future GH version
                // renamed/restructured it) -- add a small top-level menu instead, so the feature
                // stays reachable either way.
                ToolStripMenuItem root = new ToolStripMenuItem("Robot Components");
                root.DropDownItems.Add(fixItem);
                root.DropDownOpening += (s, e) => fixItem.Enabled = IsApplicable(Instances.ActiveCanvas?.Document);
                mainMenu.Items.Add(root);
            }

            _menuAdded = true;
        }

        /// <summary>
        /// Cheap, guid-only presence check backing the menu item's Enabled state -- deliberately
        /// not the actual (more expensive) content-matching each fix performs on click, just
        /// "is there anything in this document this could conceivably apply to at all".
        /// </summary>
        private static bool IsApplicable(GH_Document document)
        {
            if (document == null) { return false; }

            return document.Objects.Any(o => RelevantGuids.Contains(o.ComponentGuid));
        }

        /// <summary>
        /// Menu action: runs both retroactive fixes on demand, against the currently active
        /// document -- covers a file that already has optional parameters showing the wrong Draw
        /// Full Names form, or a Comparison Operators value list still showing the old enum-name
        /// labels, from before either fix existed or from a session where the relevant event never
        /// fired for them.
        /// </summary>
        private static void FixRCParameterNames()
        {
            GH_Canvas canvas = Instances.ActiveCanvas;
            GH_Document document = canvas?.Document;
            if (document == null) { return; }

            bool changed = SweepDrawFullNames(document, CentralSettings.CanvasFullNames);
            changed |= SweepComparisonOperatorSymbols(document);

            // ExpireLayout()/ExpireSolution() (used by the two sweeps) only mark the affected
            // objects' own layout/state stale for whenever they're next drawn -- neither repaints
            // anything itself. GH's own "Draw Full Names" menu handler follows its equivalent
            // conversion with exactly this same canvas.Invalidate() call (confirmed via IL
            // decompilation); nothing else does that for a standalone menu action like this one.
            if (changed) { canvas.Invalidate(); }
        }

        /// <summary>
        /// Relabels every native Grasshopper Value List in the given document whose items still
        /// exactly match the old, pre-fix "Comparison Operators" shape (enum member names
        /// LT/GT/LE/GE/EQ/NE, with expressions "0".."5") to the actual RAPID comparison symbols
        /// instead -- see HelperMethods.ComparisonOperatorSymbols. Formerly implemented as an
        /// IGH_UpgradeObject hooked to GH_ValueList's own (shared, native) guid; moved here since
        /// that guid is shared by every value list in every file from any plugin, so GH's own
        /// "Upgrade Components" would have flagged every value list, everywhere, forever, as a
        /// candidate, even ones already fixed or entirely unrelated to this project.
        /// </summary>
        private static bool SweepComparisonOperatorSymbols(GH_Document document)
        {
            string[] oldNames = { "LT", "GT", "LE", "GE", "EQ", "NE" };
            bool changed = false;

            foreach (GH_ValueList valueList in document.Objects.OfType<GH_ValueList>())
            {
                List<GH_ValueListItem> items = valueList.ListItems;
                if (items.Count != oldNames.Length) { continue; }

                bool matches = true;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Name != oldNames[i] || items[i].Expression != i.ToString())
                    {
                        matches = false;
                        break;
                    }
                }
                if (!matches) { continue; }

                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Name = Utils.HelperMethods.ComparisonOperatorSymbols[i];
                }

                valueList.ExpireSolution(true);
                changed = true;
            }

            return changed;
        }
        #endregion

        #region draw full names
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
        /// Doesn't cover a file opened while "Draw Full Names" is already on from an earlier
        /// session (nothing changes this session, so nothing fires here) -- see
        /// FixRCParameterNames above, the on-demand menu action for exactly that case.
        /// </para>
        /// </remarks>
        private static void OnCanvasFullNamesChanged()
        {
            GH_Document document = Instances.ActiveCanvas?.Document;
            if (document == null) { return; }

            SweepDrawFullNames(document, CentralSettings.CanvasFullNames);
        }

        /// <summary>
        /// The actual document-wide sweep shared by the live toggle handler above and the
        /// on-demand menu action. Returns true if anything actually changed.
        /// </summary>
        private static bool SweepDrawFullNames(GH_Document document, bool full)
        {
            bool anyChanged = false;

            foreach (GH_RobotComponent component in document.Objects.OfType<GH_RobotComponent>())
            {
                IReadOnlyList<(string Name, string NickName)> defaults = component.OptionalParameterDefaults;
                if (defaults.Count == 0) { continue; }

                bool changed = ApplyToSide(component.Params.Input, defaults, full);
                changed |= ApplyToSide(component.Params.Output, defaults, full);

                if (changed)
                {
                    component.Attributes?.ExpireLayout();
                    anyChanged = true;
                }
            }

            return anyChanged;
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
        #endregion
    }
}
