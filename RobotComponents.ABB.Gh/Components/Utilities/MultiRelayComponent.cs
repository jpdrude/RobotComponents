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
        // param's current Name still matches this, it's still "ours" to auto-rename; the moment a
        // user renames it to anything else, it falls out of sync here and we leave it alone from
        // then on. A null entry means "not yet assigned" (a genuinely new slot); "" is used as a
        // baseline the param's real Name can never equal, for a slot that's been permanently
        // excluded from auto-renaming (see EnsureConsistentState()).
        //
        // Persisted directly via Write/Read (see #region serialization below), the same one
        // mechanism GH itself uses for both a plain save/reload *and* copy/paste/duplicate -- so
        // fixing this to survive one fixes it for the other too, and there's no separate
        // guid-remapping case to reason about at all.
        private readonly List<string> _lastAutoNames = new List<string>();
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

            // Mirror an input's rename onto its matching output the moment the rename is
            // committed, rather than waiting for the next unrelated solve. Params.
            // ParameterNickNameChanged fires only once a rename is actually accepted (interactive
            // edit committed, or undo/redo of one) -- confirmed via IL decompilation of
            // GH_ComponentParamServer.LocalParameterChanged, which is what raises it, gated on
            // GH_ObjectEventType.NickNameAccepted -- never merely from code assigning .NickName,
            // so this can't re-fire itself from EnsureConsistentState()'s own renaming below.
            Params.ParameterNickNameChanged += OnParameterNickNameChanged;
        }

        /// <summary>
        /// Fires once a parameter rename on this component is accepted. Immediately re-syncs and
        /// expires so a renamed input's matching output picks up the new name right away.
        /// </summary>
        private void OnParameterNickNameChanged(object sender, GH_ParamServerEventArgs e)
        {
            if (e.ParameterSide != GH_ParameterSide.Input) { return; }

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
            // Mirrors the seeded input above; further pairs are kept in sync from
            // EnsureConsistentState(). Named directly here for the same reason as the input.
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
        /// Keeps the output list mirroring the input list 1:1 (same count, same order), auto-names
        /// any input that still has its original/auto-assigned name once something gets wired into
        /// it, and mirrors each input's current Name/NickName onto its matching output. Also leaves
        /// an already-named input's name alone the first time it's seen -- whether that name arrived
        /// via GH's own Read() (a plain file reload or a copy/paste/duplicate: both go through the
        /// identical IGH_DocumentObject.Write/Read mechanism) racing ahead of our own bookkeeping
        /// list catching up, or any other way a param could show up already named but untracked.
        /// Tracking is by position in Params.Input, not by InstanceGuid: a guid isn't stable across
        /// copy/paste/duplicate (a pasted param gets a fresh one), which was the root cause of two
        /// earlier, guid-keyed attempts at this each failing a different way. Called by GH after
        /// every input add/remove via the +/- zui, after an input rename is accepted, and
        /// defensively again at the start of every solve.
        /// </summary>
        private void EnsureConsistentState()
        {
            SyncOutputCount();

            // Keep the tracking list index-aligned with Params.Input. Shrinking only ever happens
            // right after an input removal (zui '-' always removes the last input), so trimming
            // from the end here stays aligned with what SyncOutputCount just did to Params.Output.
            // Growing covers both a zui '+' (new slot, no name assigned yet) and this component's
            // own tracking list not having caught up yet with a param count that arrived some other
            // way (notably: right after Read(), before this list has been resized to match).
            while (_lastAutoNames.Count > Params.Input.Count)
            {
                _lastAutoNames.RemoveAt(_lastAutoNames.Count - 1);
            }

            while (_lastAutoNames.Count < Params.Input.Count)
            {
                _lastAutoNames.Add(null);
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
                    // which deliberately leaves a slot untracked rather than throw when it can't
                    // make sense of what's on disk for it). There's no reliable history to fall back
                    // on either way, so both cases get the same treatment: default to whatever type
                    // is currently wired in, if anything is -- this is the one piece of fresh
                    // information actually available, and matches what a brand new input would
                    // settle on the moment something's connected anyway. If nothing's wired in, keep
                    // the param's current name if it has one (there's nothing to gain by discarding
                    // it), else fall back to the plain "Input N" placeholder.
                    bool wasUnnamed = string.IsNullOrEmpty(input.Name);
                    string typeName = input.SourceCount > 0 ? input.Sources[0].TypeName : null;

                    if (!string.IsNullOrEmpty(typeName))
                    {
                        lastAuto = typeName;
                    }
                    else if (wasUnnamed)
                    {
                        lastAuto = $"Input {i + 1}";
                    }
                    else
                    {
                        lastAuto = input.Name;
                    }

                    input.Name = lastAuto;
                    input.NickName = lastAuto;

                    if (wasUnnamed)
                    {
                        // Re-assert hidden wire display here too: GH's own +/- zui insert handler
                        // overwrites whatever WireDisplay CreateParameter() set on a freshly-inserted
                        // param with its own "implied" style right after calling it (verified via IL
                        // decompilation of GH_ComponentAttributes' insert-click handler), so setting
                        // it only in CreateInputParam() is silently clobbered for every zui-added
                        // input. This runs from VariableParameterMaintenance(), which fires right
                        // after that clobber, so it's the last word. Gated on wasUnnamed so this
                        // can't re-hide a recovered-but-untracked param's wire display, which is
                        // ordinary serialized state independent of our own tracking.
                        input.WireDisplay = GH_ParamWireDisplay.hidden;
                    }

                    _lastAutoNames[i] = lastAuto;
                }

                bool stillOurs = input.Name == lastAuto;

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

                // Mirror onto the matching output regardless of where the name came from.
                IGH_Param output = Params.Output[i];

                if (output.Name != input.Name || output.NickName != input.NickName)
                {
                    output.Name = input.Name;
                    output.NickName = input.NickName;
                    output.Attributes?.ExpireLayout();
                }
            }

            Attributes?.ExpireLayout();
        }

        /// <summary>
        /// Adds or removes outputs so their count matches the inputs, one-for-one, same order.
        /// </summary>
        private void SyncOutputCount()
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
            // Only ever invoked for side == Input, per CanInsertParameter above; the matching
            // output is created separately by SyncOutputCount() from VariableParameterMaintenance().
            return CreateInputParam();
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            // Only ever invoked for side == Input, per CanRemoveParameter above; the matching
            // output is destroyed separately by SyncOutputCount() from VariableParameterMaintenance().
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
                    // reliable history and defaults it to the connected data type (or its existing
                    // name, or a placeholder), which is the right behavior for recovering from a
                    // format it no longer has a way to actually read back.
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

            // base.Read() restores Params (input/output params, including each one's own Name/
            // NickName/WireDisplay/...) before returning. EnsureConsistentState() -- called from
            // VariableParameterMaintenance() right after this, per GH's own documented IO sequence
            // -- then reconciles _lastAutoNames' length against the just-restored Params.Input.Count
            // (they should already match here, since both were saved together, but doesn't assume it).
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
