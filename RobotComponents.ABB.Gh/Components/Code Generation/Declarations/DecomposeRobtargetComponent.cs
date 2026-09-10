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
using System.Windows.Forms;
// Grasshopper Libs
using GH_IO.Serialization;
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Definitions;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Decompose Robtarget component.
    /// Takes a robtarget RAPID Variable and produces RAPID expressions that access individual
    /// members of its struct (position, and optionally rotation/configuration/external axes),
    /// each usable directly wherever a RAPID Expression input is accepted.
    /// </summary>
    public class DecomposeRobtargetComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        // Which optional output blocks are currently switched on via the right-click menu. The
        // position block (X/Y/Z) is always present and isn't tracked here.
        private bool _rotationEnabled = false;
        private bool _configEnabled = false;
        private bool _externalEnabled = false;

        /// <summary>
        /// One output slot: its param Name/NickName/Description, and the robtarget struct member
        /// access to append after the variable name (e.g. "trans.x", for myVariable.trans.x).
        /// </summary>
        private readonly struct OutputSpec
        {
            public OutputSpec(string name, string nickName, string suffix, string description)
            {
                Name = name;
                NickName = nickName;
                Suffix = suffix;
                Description = description;
            }

            public string Name { get; }
            public string NickName { get; }
            public string Suffix { get; }
            public string Description { get; }
        }

        // Fixed output order, regardless of the order the menu items happen to be toggled in:
        // Position (always on) -> Rotation -> Config -> External. Keeping this order stable
        // makes a saved file's output layout predictable from its three toggle booleans alone.
        private static readonly OutputSpec[] _positionOutputSpecs =
        {
            new OutputSpec("X", "X", "trans.x", "X coordinate of the position (<variable>.trans.x) as a RAPID expression."),
            new OutputSpec("Y", "Y", "trans.y", "Y coordinate of the position (<variable>.trans.y) as a RAPID expression."),
            new OutputSpec("Z", "Z", "trans.z", "Z coordinate of the position (<variable>.trans.z) as a RAPID expression."),
        };

        private static readonly OutputSpec[] _rotationOutputSpecs =
        {
            new OutputSpec("Q1", "Q1", "rot.q1", "First quaternion value of the rotation (<variable>.rot.q1) as a RAPID expression."),
            new OutputSpec("Q2", "Q2", "rot.q2", "Second quaternion value of the rotation (<variable>.rot.q2) as a RAPID expression."),
            new OutputSpec("Q3", "Q3", "rot.q3", "Third quaternion value of the rotation (<variable>.rot.q3) as a RAPID expression."),
            new OutputSpec("Q4", "Q4", "rot.q4", "Fourth quaternion value of the rotation (<variable>.rot.q4) as a RAPID expression."),
        };

        private static readonly OutputSpec[] _configOutputSpecs =
        {
            new OutputSpec("Cf1", "Cf1", "robconf.cf1", "First axis configuration value (<variable>.robconf.cf1) as a RAPID expression."),
            new OutputSpec("Cf4", "Cf4", "robconf.cf4", "Fourth axis configuration value (<variable>.robconf.cf4) as a RAPID expression."),
            new OutputSpec("Cf6", "Cf6", "robconf.cf6", "Sixth axis configuration value (<variable>.robconf.cf6) as a RAPID expression."),
            new OutputSpec("Cfx", "Cfx", "robconf.cfx", "Configuration extension value (<variable>.robconf.cfx) as a RAPID expression."),
        };

        private static readonly OutputSpec[] _externalOutputSpecs =
        {
            new OutputSpec("EaxA", "EaxA", "extax.eax_a", "External axis A value (<variable>.extax.eax_a) as a RAPID expression."),
            new OutputSpec("EaxB", "EaxB", "extax.eax_b", "External axis B value (<variable>.extax.eax_b) as a RAPID expression."),
            new OutputSpec("EaxC", "EaxC", "extax.eax_c", "External axis C value (<variable>.extax.eax_c) as a RAPID expression."),
            new OutputSpec("EaxD", "EaxD", "extax.eax_d", "External axis D value (<variable>.extax.eax_d) as a RAPID expression."),
            new OutputSpec("EaxE", "EaxE", "extax.eax_e", "External axis E value (<variable>.extax.eax_e) as a RAPID expression."),
            new OutputSpec("EaxF", "EaxF", "extax.eax_f", "External axis F value (<variable>.extax.eax_f) as a RAPID expression."),
        };
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public DecomposeRobtargetComponent() : base("Decompose Robtarget", "DecRT", "Advanced RAPID Features",
            "Decomposes a robtarget RAPID Variable into RAPID expressions that access its struct members, " +
            "usable wherever a RAPID Expression input is accepted. Outputs the position (X/Y/Z) by default. " +
            "Right-click to also add the rotation (quaternion), configuration data, or external axis values.")
        {
            Message = "EXTENDABLE";
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_RAPIDVariable(), "Variable", "V",
                "Robtarget RAPID Variable to decompose into RAPID expressions.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // Only the position block by default; Get Rotation/Config/External add the rest via
            // the right-click menu (see ApplyOutputConfiguration()).
            foreach (OutputSpec spec in GetActiveOutputSpecs())
            {
                pManager.RegisterParam(new Param_RAPIDExpression(), spec.Name, spec.NickName, spec.Description, GH_ParamAccess.item);
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RAPIDVariable variable = null;
            if (!DA.GetData(0, ref variable)) { return; }

            if (variable == null || string.IsNullOrWhiteSpace(variable.Name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Variable does not have a name.");
                return;
            }

            if (!string.IsNullOrEmpty(variable.Type) && !variable.Type.Equals("robtarget", StringComparison.OrdinalIgnoreCase))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Variable's declared type is \"{variable.Type}\", not \"robtarget\" -- the generated expressions may not compile.");
            }

            List<OutputSpec> specs = GetActiveOutputSpecs();

            for (int i = 0; i < specs.Count; i++)
            {
                RAPIDExpression expression = RAPIDExpression.FromString($"{variable.Name}.{specs[i].Suffix}");
                DA.SetData(i, new GH_RAPIDExpression(expression));
            }
        }

        #region output configuration
        /// <summary>
        /// The full, ordered list of output specs matching the current toggle state: position
        /// always first, followed by rotation/config/external, each only if switched on -- in
        /// that fixed order regardless of which order the menu items were actually clicked in.
        /// </summary>
        private List<OutputSpec> GetActiveOutputSpecs()
        {
            List<OutputSpec> specs = new List<OutputSpec>(_positionOutputSpecs);
            if (_rotationEnabled) { specs.AddRange(_rotationOutputSpecs); }
            if (_configEnabled) { specs.AddRange(_configOutputSpecs); }
            if (_externalEnabled) { specs.AddRange(_externalOutputSpecs); }
            return specs;
        }

        /// <summary>
        /// Reconciles Params.Output against GetActiveOutputSpecs(), adding/removing whichever
        /// output block just got toggled while leaving every other output's wire connections
        /// untouched. Full reconciliation rather than "insert/remove exactly this one block" is
        /// deliberate: it's idempotent and order-independent, so it stays correct regardless of
        /// what combination of blocks is already on when another one gets toggled.
        /// </summary>
        private void ApplyOutputConfiguration()
        {
            List<OutputSpec> desired = GetActiveOutputSpecs();

            // Remove anything no longer wanted first. Walk backwards so removing one doesn't
            // shift the index of an output not yet visited.
            for (int i = Params.Output.Count - 1; i >= 0; i--)
            {
                string name = Params.Output[i].Name;
                if (!desired.Exists(spec => spec.Name == name))
                {
                    Params.UnregisterOutputParameter(Params.Output[i], true);
                }
            }

            // What's left is exactly the subsequence of `desired` still registered, in the same
            // relative order (nothing above ever reorders, only removes) -- so walking `desired`
            // left to right and inserting whatever doesn't match at that position lands each new
            // param exactly where it belongs, and can never produce a duplicate: a mismatch at
            // index i means desired[i] isn't registered at i yet (either missing outright, or --
            // being a genuine subsequence -- still further along in Params.Output, in which case
            // inserting here simply pushes it one slot later, which is exactly where it belongs).
            for (int i = 0; i < desired.Count; i++)
            {
                if (i >= Params.Output.Count || Params.Output[i].Name != desired[i].Name)
                {
                    Params.RegisterOutputParam(CreateOutputParam(desired[i]), i);
                }
            }
        }

        /// <summary>
        /// Creates a fresh output parameter matching the given spec.
        /// </summary>
        private static Param_RAPIDExpression CreateOutputParam(OutputSpec spec)
        {
            return new Param_RAPIDExpression
            {
                Name = spec.Name,
                NickName = spec.NickName,
                Description = spec.Description,
                Access = GH_ParamAccess.item
            };
        }
        #endregion

        #region menu items
        /// <summary>
        /// Appends right-click context menu items.
        /// </summary>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Get Rotation", MenuItemClickRotation, true, _rotationEnabled);
            Menu_AppendItem(menu, "Get Config", MenuItemClickConfig, true, _configEnabled);
            Menu_AppendItem(menu, "Get External", MenuItemClickExternal, true, _externalEnabled);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void MenuItemClickRotation(object sender, EventArgs e)
        {
            RecordUndoEvent("Get Rotation");
            _rotationEnabled = !_rotationEnabled;
            ToggleOutputs();
        }

        private void MenuItemClickConfig(object sender, EventArgs e)
        {
            RecordUndoEvent("Get Config");
            _configEnabled = !_configEnabled;
            ToggleOutputs();
        }

        private void MenuItemClickExternal(object sender, EventArgs e)
        {
            RecordUndoEvent("Get External");
            _externalEnabled = !_externalEnabled;
            ToggleOutputs();
        }

        private void ToggleOutputs()
        {
            ApplyOutputConfiguration();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        #endregion

        #region serialization
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("RotationOutputs", _rotationEnabled);
            writer.SetBoolean("ConfigOutputs", _configEnabled);
            writer.SetBoolean("ExternalOutputs", _externalEnabled);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("RotationOutputs")) { _rotationEnabled = reader.GetBoolean("RotationOutputs"); }
            if (reader.ItemExists("ConfigOutputs")) { _configEnabled = reader.GetBoolean("ConfigOutputs"); }
            if (reader.ItemExists("ExternalOutputs")) { _externalEnabled = reader.GetBoolean("ExternalOutputs"); }

            // base.Read() restores Params.Output from the archive itself (see
            // GH_ComponentParamServer.ReadAllParameterData), which was written from this exact
            // same state in the matching Write() call above -- so Params.Output already matches
            // GetActiveOutputSpecs() by the time this returns, with no reconciliation needed.
            return base.Read(reader);
        }
        #endregion

        #region IGH_VariableParameterComponent
        // Menu-driven only — no + / - zui buttons.
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index) => false;
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index) => false;
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index) => null;
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index) => false;
        void IGH_VariableParameterComponent.VariableParameterMaintenance() { }
        #endregion

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
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
            get { return Properties.Resources.DecomposeRobtarget_Icon; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("48E8C9AE-26A9-4ACD-B5A1-2C87A78B98F6"); }
        }
        #endregion
    }
}
