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
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

namespace RobotComponents.ABB.Gh.Components.Utilities
{
    /// <summary>
    /// RobotComponents Multi Relay Component.
    /// A generic pass-through utility with variable inputs (+/- zui, like Merge): each input gets
    /// a matching output that simply relays its tree through unchanged. Meant purely to tidy up a
    /// GH script's canvas by collapsing a bundle of otherwise-crossing wires through one component.
    /// Always keeps at least one input/output pair; the "-" zui stops working once just one is left.
    /// </summary>
    public class MultiRelayComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        // For each CURRENT input, by position (index-aligned with Params.Input -- not keyed by
        // InstanceGuid: that isn't reliably stable across copy/paste/duplicate, which was the
        // root cause of two earlier attempts at this each failing a different way), the
        // Name/NickName we ourselves last assigned it -- either the "Input N" placeholder given
        // at creation, or a type name detected from what got wired into it. As long as the
        // param's current NickName still matches this, it's still "ours" to auto-rename; the
        // moment a user renames it to anything else, it falls out of sync here and we leave it
        // alone from then on. Checked against NickName, not Name: a user can only ever rename a
        // parameter's NickName from the canvas -- there is no "NameAccepted" GH_ObjectEventType at
        // all (confirmed via IL decompilation of Grasshopper.dll), so Name is purely a property
        // this component itself sets in code, never user-editable, and comparing it alone could
        // never actually detect a real rename. A null entry means "not yet assigned" (a genuinely
        // new slot); "" is used as a baseline the param's real NickName can never equal, for a
        // slot that's been permanently excluded from auto-renaming (see EnsureConsistentState()).
        //
        // Persisted directly via Write/Read (see #region serialization below), the same one
        // mechanism GH itself uses for both a plain save/reload *and* copy/paste/duplicate -- so
        // fixing this to survive one fixes it for the other too, and there's no separate
        // guid-remapping case to reason about at all.
        private readonly List<string> _lastAutoNames = new List<string>();

        // Same idea as _lastAutoNames above, but for the OUTPUT side of each pair: the NickName we
        // ourselves last mirrored onto output i from its matching input. As long as the output's
        // current NickName still matches this, it's still ours to keep mirroring; the moment a
        // user renames an output directly, it falls out of sync here and -- exactly like a custom
        // input rename -- is left alone forever after. Without this, EnsureConsistentState()
        // used to force output.Name = input.Name unconditionally on every solve, with no way for
        // a user-set output name to ever survive past the next solve.
        private readonly List<string> _lastMirroredOutputNames = new List<string>();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public MultiRelayComponent() : base("Multi Relay", "MR", "Utility",
            "Relays any number of data trees straight through, one matching output per input. " +
            "Right-click, or use the +/- zui like Merge, to add or remove input/output pairs (always " +
            "keeps at least one). Each input is auto-named after whatever type first gets wired into " +
            "it (still renameable by hand), and its output mirrors that name. Purely a canvas tidy-up " +
            "utility: it does not touch the data at all.")
        {
            Message = "+/-";

            // React to a rename on EITHER side the moment it's committed, rather than waiting for
            // the next unrelated solve: an input rename should mirror onto its output right away,
            // and an output rename should immediately freeze that output out of further mirroring
            // (see _lastMirroredOutputNames) instead of only taking effect retroactively next
            // solve. Params.ParameterNickNameChanged fires only once a rename is actually accepted
            // (interactive edit committed, or undo/redo of one) -- confirmed via IL decompilation
            // of GH_ComponentParamServer.LocalParameterChanged, which is what raises it, gated on
            // GH_ObjectEventType.NickNameAccepted -- never merely from code assigning .NickName,
            // so this can't re-fire itself from EnsureConsistentState()'s own renaming below.
            Params.ParameterNickNameChanged += OnParameterNickNameChanged;
        }

        /// <summary>
        /// Fires once a parameter rename on this component (either side) is accepted. Immediately
        /// re-syncs and expires so the result -- a renamed input's matching output picking up the
        /// new name, or a renamed output freezing out of further mirroring -- takes effect right
        /// away instead of waiting for the next unrelated solve.
        /// </summary>
        private void OnParameterNickNameChanged(object sender, GH_ParamServerEventArgs e)
        {
            EnsureConsistentState();
            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // Always starts with one; further pairs are added via the +/- zui. Named directly here
            // (rather than leaving it to EnsureConsistentState() on first solve) so the seeded
            // input has its final identity from construction on, with no dependency on solve timing.
            IGH_Param input = CreateInputParam();
            input.Name = "Input 1";
            input.NickName = "Input 1";
            pManager.AddParameter(input);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // Mirrors the seeded input above; further pairs are added/removed in lockstep by
            // CreateParameter/DestroyParameter below. Named directly here for the same reason as
            // the input.
            Param_GenericObject output = CreateRelayParam();
            output.Name = "Input 1";
            output.NickName = "Input 1";
            pManager.RegisterParam(output);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Belt-and-suspenders: VariableParameterMaintenance() should already have synced
            // outputs to inputs and applied naming by the time a solve runs, but guarantee the
            // invariant holds here too rather than risk an index mismatch on, say, the very first
            // solve after a file load.
            EnsureConsistentState();

            for (int i = 0; i < Params.Input.Count; i++)
            {
                if (DA.GetDataTree(i, out GH_Structure<IGH_Goo> tree))
                {
                    DA.SetDataTree(i, tree);
                }
            }
        }

        #region variable parameters
        /// <summary>
        /// Auto-names any input that still has its original/auto-assigned name once something
        /// gets wired into it, and mirrors each input's current Name/NickName onto its matching
        /// output -- but only for as long as neither side has been custom-renamed by the user.
        /// Also leaves an already-named input's name alone the first time it's seen -- whether
        /// that name arrived via GH's own Read() (a plain file reload or a copy/paste/duplicate:
        /// both go through the identical IGH_DocumentObject.Write/Read mechanism) racing ahead of
        /// our own bookkeeping list catching up, or any other way a param could show up already
        /// named but untracked. Tracking is by position in Params.Input/Params.Output, not by
        /// InstanceGuid: a guid isn't stable across copy/paste/duplicate (a pasted param gets a
        /// fresh one), which was the root cause of two earlier, guid-keyed attempts at this each
        /// failing a different way. Called by GH after every input add/remove via the +/- zui,
        /// after a rename on either side is accepted, and defensively again at the start of every
        /// solve. Adding/removing the output list itself to match an input add/remove is NOT done
        /// here -- CreateParameter/DestroyParameter below already do that precisely, at the exact
        /// index of the change; see EnsureOutputCountFallback() for why a count-only fallback
        /// still exists here too.
        /// </summary>
        private void EnsureConsistentState()
        {
            EnsureOutputCountFallback();

            // Fallback only: CreateParameter/DestroyParameter above already insert/remove at the
            // exact index GH gives them, so by the time this runs after an ordinary zui add/remove
            // or mid-list insert, the counts here already match and neither loop below does
            // anything. What's left for these to catch is a count arriving out of step some other
            // way it can't be positionally reasoned about -- notably right after Read(), before
            // this list has necessarily settled to match Params.Input.Count (e.g. the legacy-format
            // fallback in Read() below, which can leave _lastAutoNames shorter than Params.Input
            // with no positional information to recover at all). Trimming/padding at the end is the
            // best available default for that case, not a claim that it's positionally correct.
            while (_lastAutoNames.Count > Params.Input.Count)
            {
                _lastAutoNames.RemoveAt(_lastAutoNames.Count - 1);
            }

            while (_lastAutoNames.Count < Params.Input.Count)
            {
                _lastAutoNames.Add(null);
            }

            // Same fallback reasoning as _lastAutoNames above, mirrored for the output-side
            // tracking list.
            while (_lastMirroredOutputNames.Count > Params.Output.Count)
            {
                _lastMirroredOutputNames.RemoveAt(_lastMirroredOutputNames.Count - 1);
            }

            while (_lastMirroredOutputNames.Count < Params.Output.Count)
            {
                _lastMirroredOutputNames.Add(null);
            }

            for (int i = 0; i < Params.Input.Count; i++)
            {
                IGH_Param input = Params.Input[i];
                string lastAuto = _lastAutoNames[i];

                if (lastAuto == null)
                {
                    // Not tracked for this slot. Under normal operation this only happens for a
                    // genuinely brand new input (zui '+', or the initial RegisterInputParams call):
                    // a round-tripped reload or copy/paste goes through this same component's own
                    // Write/Read, so _lastAutoNames is already correctly populated for every
                    // existing input by the time this runs, and never lands here.
                    //
                    // The one other way to get here is an archive Read() couldn't recover tracking
                    // from at all -- e.g. a file saved by an earlier version of this component that
                    // used a different, no-longer-understood serialization shape (see Read() below,
                    // which deliberately leaves a slot untracked rather than throw when it can't make
                    // sense of what's on disk for it). There's no reliable history to fall back on
                    // either way, so: if the param has no name of its own yet, treat it exactly like
                    // a brand new input (placeholder name, hidden wire display). If it already has
                    // some name, freeze it in place instead -- deliberately NOT touching it even if
                    // something's wired in. This intentionally does not try to be clever about
                    // recovering a "fresher" name from the connected type: an untracked-but-named
                    // param almost always means archive tracking that's out of sync with an otherwise
                    // perfectly good, possibly user-set Name (e.g. this exact file, mid-migration
                    // across a couple of serialization-format changes to this component) -- silently
                    // overwriting that with the wired type was tried and is worse: it clobbers a
                    // legitimate custom name the instant this branch is hit for any reason, which is
                    // far more likely in practice than genuinely wanting a re-derived type name here.
                    if (string.IsNullOrEmpty(input.Name))
                    {
                        lastAuto = $"Input {i + 1}";
                        input.Name = lastAuto;
                        input.NickName = lastAuto;

                        // Re-assert hidden wire display here too: GH's own +/- zui insert handler
                        // overwrites whatever WireDisplay CreateParameter() set on a freshly-inserted
                        // param with its own "implied" style right after calling it (verified via IL
                        // decompilation of GH_ComponentAttributes' insert-click handler), so setting
                        // it only in CreateInputParam() is silently clobbered for every zui-added
                        // input. This runs from VariableParameterMaintenance(), which fires right
                        // after that clobber, so it's the last word. Only reached for a param with no
                        // name of its own yet, so this can't re-hide a recovered-but-untracked param's
                        // wire display, which is ordinary serialized state independent of our own
                        // tracking.
                        input.WireDisplay = GH_ParamWireDisplay.hidden;
                    }
                    else
                    {
                        // Freeze via a baseline ("") the param's real Name can never equal, which
                        // makes "stillOurs" below permanently false for this slot from here on.
                        lastAuto = "";
                    }

                    _lastAutoNames[i] = lastAuto;
                }

                // A user can only ever rename a parameter's NickName from the canvas -- there is
                // no "NameAccepted" GH_ObjectEventType at all (confirmed via IL decompilation of
                // Grasshopper.dll: GH_ObjectEventType only has NickName/NickNameAccepted), so Name
                // is purely a property this component itself sets in code and a user genuinely
                // cannot edit interactively. Comparing input.Name alone therefore can never detect
                // a real rename -- it stays exactly as this component last set it regardless of
                // what the user does on canvas. Checking NickName here is what actually catches
                // it: the moment it diverges from what we last assigned, resync Name to match (so
                // this component's own invariant -- Name and NickName always equal for a slot it's
                // still managing -- holds again) and freeze this slot for good, the same one-way
                // freeze already used for every other "no longer ours" case.
                if (lastAuto.Length > 0 && input.NickName != lastAuto)
                {
                    input.Name = input.NickName;
                    lastAuto = "";
                    _lastAutoNames[i] = lastAuto;
                }

                bool stillOurs = lastAuto.Length > 0 && input.Name == lastAuto;

                if (stillOurs && input.SourceCount > 0)
                {
                    string typeName = input.Sources[0].TypeName;

                    if (!string.IsNullOrEmpty(typeName) && typeName != input.Name)
                    {
                        input.Name = typeName;
                        input.NickName = typeName;
                        _lastAutoNames[i] = typeName;
                    }
                }

                // Mirror onto the matching output -- but only while the output itself hasn't
                // been custom-renamed by the user. _lastMirroredOutputNames[i] is the Name we
                // ourselves last wrote to output i; a null entry means this slot has never been
                // mirrored yet (a genuinely fresh pair), which is just as safe to mirror as a
                // match. The moment output.NickName diverges from that, it's a custom rename --
                // stop touching this output's Name/NickName for good, the same one-way freeze
                // already used for a custom input rename above. Checked against NickName, not
                // Name, for the same reason as the input side above: a user can only ever rename
                // NickName from the canvas, so comparing Name alone would never actually catch it.
                IGH_Param output = Params.Output[i];
                string lastMirrored = _lastMirroredOutputNames[i];

                if (lastMirrored == null || output.NickName == lastMirrored)
                {
                    if (output.Name != input.Name || output.NickName != input.NickName)
                    {
                        output.Name = input.Name;
                        output.NickName = input.NickName;
                        output.Attributes?.ExpireLayout();
                    }

                    // Stored as NickName, matching what this slot is compared against above.
                    _lastMirroredOutputNames[i] = input.NickName;
                }
            }

            Attributes?.ExpireLayout();
        }

        /// <summary>
        /// Last-resort safety net only: pads/trims Params.Output at the *end* so its count matches
        /// Params.Input, with no attempt at positional correctness. Under normal operation this
        /// never has anything to do -- CreateParameter/DestroyParameter already add/remove the
        /// matching output at the exact index of the input change (see below), which is the whole
        /// fix for the "wires get disconnected on add/remove" bug this method used to cause: a
        /// mid-list input removal used to leave the count-only version of this method blindly
        /// deleting whatever output happened to be *last*, destroying that output's wire
        /// connections while the output that actually corresponded to the removed input survived
        /// untouched -- permanently misaligned with every input after it. What's left for this
        /// fallback to catch is Params.Output ever arriving out of step some other way this
        /// component can't positionally reason about at all (e.g. an externally corrupted
        /// archive) -- an extremely defensive last resort, not a claim of correctness.
        /// </summary>
        private void EnsureOutputCountFallback()
        {
            while (Params.Output.Count < Params.Input.Count)
            {
                Params.RegisterOutputParam(CreateRelayParam(), Params.Output.Count);
            }

            while (Params.Output.Count > Params.Input.Count)
            {
                Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1], true);
            }
        }

        /// <summary>
        /// Creates a fresh generic, tree-access, optional relay parameter (used for both new
        /// inputs and their matching outputs).
        /// </summary>
        private static Param_GenericObject CreateRelayParam()
        {
            return new Param_GenericObject
            {
                Access = GH_ParamAccess.tree,
                Optional = true
            };
        }

        /// <summary>
        /// Creates a fresh input parameter for the +/- zui: generic/tree/optional, and with its
        /// wire display hidden by default (this component exists purely to tidy up a canvas, so
        /// the wires feeding into it shouldn't add back the clutter it's meant to remove). Users
        /// can still turn wire display back on per-input via that input's own right-click menu;
        /// nothing here re-hides it afterwards.
        /// </summary>
        private static IGH_Param CreateInputParam()
        {
            Param_GenericObject param = CreateRelayParam();
            param.WireDisplay = GH_ParamWireDisplay.hidden;

            // Param_GenericObject's own default constructor already sets Name/NickName to "Data"/
            // "D" -- that's GH's standard generic-object-parameter default, not an absence of a
            // name. EnsureConsistentState() tells a genuinely new, not-yet-auto-named slot apart
            // from an already-named one purely by checking string.IsNullOrEmpty(input.Name), so
            // leaving that default in place made every zui-added input look "already named" and
            // get frozen on "Data" forever instead of ever receiving its "Input N" placeholder.
            // Blank both out here so that check actually sees an unnamed param, same as the very
            // first input RegisterInputParams sets up explicitly.
            param.Name = string.Empty;
            param.NickName = string.Empty;

            return param;
        }

        // Menu-driven components elsewhere in this project return false/null here and manage
        // their optional inputs via a right-click toggle instead; this one is deliberately the
        // other, zui-driven kind (+/- buttons drawn on the component, like Merge/Entwine), so it
        // implements these for real rather than stubbing them out.
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input;
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            // Never remove the last remaining pair.
            return side == GH_ParameterSide.Input && Params.Input.Count > 1;
        }

        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            // Only ever invoked for side == Input, per CanInsertParameter above.
            //
            // GH splices the returned param into Params.Input at exactly this index right after
            // this call returns -- and index is NOT always Params.Input.Count: dropping a wire on
            // the joint between two existing inputs (or the canvas's own "Insert parameter"
            // context menu) inserts in the middle, shifting every later input up by one. Insert
            // a matching "not yet assigned" slot into _lastAutoNames at that same index now, so
            // it lines back up with Params.Input immediately -- rather than leaving it to
            // EnsureConsistentState()'s grow loop below, which only ever appends at the *end* and
            // so is only correct for a plain append. Left uncorrected here, a mid-list insert
            // desynced this list from Params.Input by one slot from that point on: the new input
            // inherited whatever tracked auto-name used to belong to the input now one slot further
            // along, and that shifted input in turn got treated as freshly untracked -- which is
            // exactly the "inserted pair, and an old pair's identity moved back onto the new one"
            // bug reported after this fix went in without it.
            if (index >= 0 && index <= _lastAutoNames.Count)
            {
                _lastAutoNames.Insert(index, null);
            }

            // Create and insert the matching OUTPUT at this exact same index, right now -- rather
            // than leaving it to EnsureOutputCountFallback(), which only ever appends at the
            // *end* and has no idea which input index actually changed. Left to the fallback, a
            // mid-list insert landed the new output at the end of Params.Output instead of
            // alongside its actual input, silently shifting every later output's data by one
            // position from that point on (each wire stayed attached to the same, now-misaligned
            // param object, so nothing looked disconnected on canvas -- the data flowing through
            // it was simply for the wrong input). Params.Output.Count still equals the
            // pre-insert Params.Input.Count here (GH hasn't spliced the new input in yet), which
            // is exactly the valid range for `index`, so no clamping is needed.
            Params.RegisterOutputParam(CreateRelayParam(), index);

            if (index >= 0 && index <= _lastMirroredOutputNames.Count)
            {
                _lastMirroredOutputNames.Insert(index, null);
            }

            return CreateInputParam();
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            // Only ever invoked for side == Input, per CanRemoveParameter above.
            //
            // Mirrors CreateParameter above: CanRemoveParameter permits removing any input, not
            // just the last one (e.g. via right-click "Remove parameter"), so remove tracking for
            // exactly the slot being destroyed here rather than relying on EnsureConsistentState()'s
            // shrink loop, which only ever trims from the *end* and so is only correct when the
            // removed input happens to be the last one.
            if (index >= 0 && index < _lastAutoNames.Count)
            {
                _lastAutoNames.RemoveAt(index);
            }

            // Destroy the matching OUTPUT at this exact same index, right now -- see
            // CreateParameter above for why. Left to EnsureOutputCountFallback() instead, a
            // mid-list removal deleted whatever output happened to be *last* (destroying that
            // output's wire connections in the process), while the output that actually
            // corresponded to the removed input survived untouched -- permanently misaligned
            // with every input after it. Params.Output.Count still equals the pre-removal
            // Params.Input.Count here (GH removes the input itself separately), so `index` is
            // valid for Params.Output too by the same invariant as CreateParameter above.
            if (index >= 0 && index < Params.Output.Count)
            {
                Params.UnregisterOutputParameter(Params.Output[index], true);
            }

            if (index >= 0 && index < _lastMirroredOutputNames.Count)
            {
                _lastMirroredOutputNames.RemoveAt(index);
            }

            return true;
        }

        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            EnsureConsistentState();
        }
        #endregion

        #region serialization
        /// <summary>
        /// Add our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="writer"> Provides access to a subset of GH_Chunk methods used for writing archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Write(GH_IWriter writer)
        {
            // Positional, not guid-keyed: index i here lines up with Params.Input[i] both now and
            // (since input order/count is itself part of the archive, restored before
            // VariableParameterMaintenance() ever runs) after a subsequent Read(). A null entry is
            // written as an empty string with a companion bool, since GH_IWriter has no native
            // "null string" chunk item.
            writer.SetInt32("AutoNameCount", _lastAutoNames.Count);

            for (int i = 0; i < _lastAutoNames.Count; i++)
            {
                string value = _lastAutoNames[i];
                writer.SetBoolean("AutoNameIsNull", i, value == null);
                writer.SetString("AutoNameValue", i, value ?? string.Empty);
            }

            // Same shape, for the output-side mirroring tracking. Must be persisted too: without
            // this, a custom output rename's protection would be forgotten on every reload, and
            // the very next solve would mirror the input's name straight back over it.
            writer.SetInt32("MirrorNameCount", _lastMirroredOutputNames.Count);

            for (int i = 0; i < _lastMirroredOutputNames.Count; i++)
            {
                string value = _lastMirroredOutputNames[i];
                writer.SetBoolean("MirrorNameIsNull", i, value == null);
                writer.SetString("MirrorNameValue", i, value ?? string.Empty);
            }

            return base.Write(writer);
        }

        /// <summary>
        /// Read our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="reader"> Provides access to a subset of GH_Chunk methods used for reading archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Read(GH_IReader reader)
        {
            _lastAutoNames.Clear();

            if (reader.ItemExists("AutoNameCount"))
            {
                int count = reader.GetInt32("AutoNameCount");

                for (int i = 0; i < count; i++)
                {
                    // Only trust an entry actually written in this (current) format. An archive
                    // saved by the earlier, InstanceGuid-keyed version of this component wrote
                    // "AutoNameCount" under this same name, but never wrote "AutoNameIsNull" at
                    // all -- GetBoolean/GetString on a chunk item that doesn't exist throws (GH_IO's
                    // GetXxx(name, index) looks the item up and calls straight into it with no null
                    // check), which is exactly what broke loading a file saved before this rewrite.
                    // Leave a slot unresolved (null) instead for anything that doesn't match the
                    // current shape; EnsureConsistentState() treats an unresolved slot as having no
                    // reliable history and freezes whatever name is already on the param rather than
                    // guess at one, which is the right behavior for recovering from a format it no
                    // longer has a way to actually read back.
                    if (reader.ItemExists("AutoNameIsNull", i))
                    {
                        bool isNull = reader.GetBoolean("AutoNameIsNull", i);
                        _lastAutoNames.Add(isNull ? null : reader.GetString("AutoNameValue", i));
                    }
                    else
                    {
                        _lastAutoNames.Add(null);
                    }
                }
            }

            _lastMirroredOutputNames.Clear();

            if (reader.ItemExists("MirrorNameCount"))
            {
                int count = reader.GetInt32("MirrorNameCount");

                for (int i = 0; i < count; i++)
                {
                    if (reader.ItemExists("MirrorNameIsNull", i))
                    {
                        bool isNull = reader.GetBoolean("MirrorNameIsNull", i);
                        _lastMirroredOutputNames.Add(isNull ? null : reader.GetString("MirrorNameValue", i));
                    }
                    else
                    {
                        _lastMirroredOutputNames.Add(null);
                    }
                }
            }

            // A file saved before this output-mirroring protection existed has no "MirrorNameCount"
            // at all, so the list above stays empty here; EnsureConsistentState()'s fallback grow
            // loop then pads it with null for every output. A null entry means "never mirrored yet",
            // which is treated as safe to mirror -- correct for these old files, since the old
            // Write()-time code always force-mirrored output.Name = input.Name on every solve
            // before a save could ever happen, so no old archive could contain a genuinely-diverged
            // (and therefore worth protecting) output name to begin with.
            //
            // base.Read() restores Params (input/output params, including each one's own Name/
            // NickName/WireDisplay/...) before returning. EnsureConsistentState() -- called from
            // VariableParameterMaintenance() right after this, per GH's own documented IO sequence
            // -- then reconciles both tracking lists' lengths against the just-restored Params
            // counts (they should already match here, since both were saved together, but doesn't
            // assume it).
            return base.Read(reader);
        }
        #endregion

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary, dropdown and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

        /// <summary>
        /// Gets whether this object is obsolete.
        /// </summary>
        public override bool Obsolete
        {
            get { return false; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.MultiRelay_Icon; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0E24397A-08D7-40FB-8A33-EEA41F7CB121"); }
        }
        #endregion
    }
}
