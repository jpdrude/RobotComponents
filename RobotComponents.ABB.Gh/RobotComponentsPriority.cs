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
    /// Runs once when the plugin loads, to wire up session-wide event subscriptions and a custom
    /// "Robot Components" menu that don't belong to any single component. GH auto-discovers a
    /// GH_AssemblyPriority the same way it discovers components -- no registration needed, just a
    /// public parameterless constructor.
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
        /// Adds a "Robot Components" entry to GH's own main menu bar the first time a canvas is
        /// created (Instances.DocumentEditor / its MainMenu aren't valid any earlier than this --
        /// PriorityLoad() itself runs before any editor window exists). Guarded by _menuAdded since
        /// CanvasCreated could in principle fire more than once in a session.
        /// </summary>
        /// <remarks>
        /// This exists instead of hooking GH's native "Upgrade Components" command. That command's
        /// applicability check (GH_ComponentServer.IsUpgrader) is a plain "is there an upgrader
        /// registered for this guid" lookup -- no per-instance "already fixed" tracking -- so
        /// registering one against any of this project's own, heavily-used live component guids
        /// would flag every instance of that component, in every file, as "upgradeable" forever,
        /// even ones already fixed. An ordinary menu action the user runs on demand has none of
        /// that permanent-nag downside, and is fully under this project's own naming/behavior.
        /// </remarks>
        private void OnCanvasCreated(GH_Canvas canvas)
        {
            if (_menuAdded) { return; }

            // GH_DocumentEditor.MainMenu exists but is internal to Grasshopper.dll; GH_DocumentEditor
            // is an ordinary System.Windows.Forms.Form though, and the standard, publicly inherited
            // Form.MainMenuStrip property points at the exact same control once a form has one, so
            // it's used here instead. Falls back to searching Controls directly, in case some future
            // GH version stops assigning MainMenuStrip for whatever reason.
            Form editor = Instances.DocumentEditor;
            if (editor == null) { return; }

            MenuStrip mainMenu = editor.MainMenuStrip
                ?? editor.Controls.OfType<MenuStrip>().FirstOrDefault();
            if (mainMenu == null) { return; }

            ToolStripMenuItem root = new ToolStripMenuItem("Robot Components");
            root.DropDownItems.Add("Fix Optional Parameter Names", null, (s, e) => FixOptionalParameterNames());
            root.DropDownItems.Add("Fix Comparison Operator Symbols", null, (s, e) => FixComparisonOperatorSymbols());
            mainMenu.Items.Add(root);

            _menuAdded = true;
        }

        /// <summary>
        /// Menu action: runs the same Draw Full Names sweep as OnCanvasFullNamesChanged below, on
        /// demand, against the currently active document -- for a file that already has optional
        /// parameters showing the wrong form (opened while the preference was already at its
        /// current value from an earlier session, so the toggle event never fired for them).
        /// </summary>
        private static void FixOptionalParameterNames()
        {
            GH_Canvas canvas = Instances.ActiveCanvas;
            GH_Document document = canvas?.Document;
            if (document == null) { return; }

            SweepDrawFullNames(document, CentralSettings.CanvasFullNames);

            // ExpireLayout() (inside SweepDrawFullNames) only marks each changed component's own
            // layout stale for whenever it next gets drawn -- it doesn't itself repaint anything.
            // GH's own "Draw Full Names" menu handler follows its equivalent conversion with
            // exactly this same canvas.Invalidate() call (confirmed via IL decompilation); nothing
            // else does that for a standalone menu action like this one, which is why the change
            // used to only become visible after some unrelated canvas interaction forced a repaint.
            canvas.Invalidate();
        }

        /// <summary>
        /// Menu action: relabels every native Grasshopper Value List in the active document whose
        /// items still exactly match the old, pre-fix "Comparison Operators" shape (enum member
        /// names LT/GT/LE/GE/EQ/NE, with expressions "0".."5") to the actual RAPID comparison
        /// symbols instead -- see HelperMethods.ComparisonOperatorSymbols. Formerly implemented as
        /// an IGH_UpgradeObject hooked to GH_ValueList's own (shared, native) guid; moved here for
        /// the same reason as FixOptionalParameterNames -- that guid is shared by every value list
        /// in every file from any plugin, so GH's own "Upgrade Components" would have flagged every
        /// value list, everywhere, forever, as a candidate, even ones already fixed or entirely
        /// unrelated to this project.
        /// </summary>
        private static void FixComparisonOperatorSymbols()
        {
            GH_Canvas canvas = Instances.ActiveCanvas;
            GH_Document document = canvas?.Document;
            if (document == null) { return; }

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

            // See the matching comment in FixOptionalParameterNames above -- ExpireSolution()
            // alone doesn't repaint the canvas, so without this the relabeled value list stayed
            // invisible until some unrelated canvas interaction forced a redraw.
            if (changed) { canvas.Invalidate(); }
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
        /// FixOptionalParameterNames above, the on-demand menu action for exactly that case.
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
        /// on-demand menu action.
        /// </summary>
        private static void SweepDrawFullNames(GH_Document document, bool full)
        {
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
        #endregion
    }
}
