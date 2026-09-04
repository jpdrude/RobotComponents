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
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

namespace RobotComponents.ABB.Gh.Components.Utilities
{
    /// <summary>
    /// RobotComponents Multi Relais Component.
    /// A generic pass-through utility with variable inputs (+/- zui, like Merge): each input gets
    /// a matching output that simply relays its tree through unchanged. Meant purely to tidy up a
    /// GH script's canvas by collapsing a bundle of otherwise-crossing wires through one component.
    /// </summary>
    public class MultiRelaisComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        // For each currently-registered input (keyed by its InstanceGuid, stable for the life of
        // the param object), the Name/NickName we ourselves last assigned it -- either the "Input N"
        // placeholder given at creation, or a type name detected from what got wired into it. As
        // long as the param's current Name still matches this, it's still "ours" to auto-rename; the
        // moment a user renames it to anything else, it falls out of sync here and we leave it alone
        // from then on. Persisted across save/reload so that distinction survives too.
        private readonly Dictionary<Guid, string> _autoNames = new Dictionary<Guid, string>();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public MultiRelaisComponent() : base("Multi Relais", "MR", "Utility",
            "Relays any number of data trees straight through, one matching output per input. " +
            "Right-click, or use the +/- zui like Merge, to add or remove input/output pairs. " +
            "Each input is auto-named after whatever type first gets wired into it (still renameable " +
            "by hand), and its output mirrors that name. Purely a canvas tidy-up utility: it does not " +
            "touch the data at all.")
        {
            Message = "+/-";
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // Starts with none; add pairs via the +/- zui.
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // Starts with none; kept in sync with the inputs from EnsureConsistentState().
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
        /// it, and mirrors each input's current Name/NickName onto its matching output. Called by
        /// GH after every input add/remove via the +/- zui, and defensively again at the start of
        /// every solve.
        /// </summary>
        private void EnsureConsistentState()
        {
            SyncOutputCount();

            for (int i = 0; i < Params.Input.Count; i++)
            {
                IGH_Param input = Params.Input[i];
                Guid id = input.InstanceGuid;

                if (!_autoNames.TryGetValue(id, out string lastAuto))
                {
                    // First time seeing this param: it was just created by the zui. Give it a
                    // placeholder name until something is wired into it.
                    lastAuto = $"Input {i + 1}";
                    input.Name = lastAuto;
                    input.NickName = lastAuto;
                    _autoNames[id] = lastAuto;
                }

                bool stillOurs = input.Name == lastAuto;

                if (stillOurs && input.SourceCount > 0)
                {
                    string typeName = input.Sources[0].TypeName;

                    if (!string.IsNullOrEmpty(typeName) && typeName != input.Name)
                    {
                        input.Name = typeName;
                        input.NickName = typeName;
                        _autoNames[id] = typeName;
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

            // Drop bookkeeping for params that were removed via the zui since the last pass.
            HashSet<Guid> current = new HashSet<Guid>(Params.Input.Select(p => p.InstanceGuid));
            foreach (Guid stale in _autoNames.Keys.Where(g => !current.Contains(g)).ToList())
            {
                _autoNames.Remove(stale);
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
            return side == GH_ParameterSide.Input;
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
            writer.SetInt32("AutoNameCount", _autoNames.Count);

            int i = 0;
            foreach (KeyValuePair<Guid, string> entry in _autoNames)
            {
                writer.SetGuid("AutoNameGuid", i, entry.Key);
                writer.SetString("AutoNameValue", i, entry.Value);
                i++;
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
            _autoNames.Clear();

            if (reader.ItemExists("AutoNameCount"))
            {
                int count = reader.GetInt32("AutoNameCount");

                for (int i = 0; i < count; i++)
                {
                    Guid guid = reader.GetGuid("AutoNameGuid", i);
                    string value = reader.GetString("AutoNameValue", i);
                    _autoNames[guid] = value;
                }
            }

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
            get { return Properties.Resources.MultiRelais_Icon; }
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
