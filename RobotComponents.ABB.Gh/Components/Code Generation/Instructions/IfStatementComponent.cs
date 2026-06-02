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
// RobotComponents Libs
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents IF Statement Component.
    /// Generates a RAPID IF / ELSEIF / ELSE / ENDIF block.
    ///
    /// Fixed inputs (indices 0–1, always present):
    ///   0  IF Condition  – RAPIDExpression
    ///   1  IF Actions    – Action list
    ///
    /// Variable inputs (added via the + button just before ELSE, in pairs):
    ///   …  ELSEIF N Condition  – RAPIDExpression
    ///   …  ELSEIF N Actions    – Action list
    ///
    /// Fixed last input (always the final param):
    ///   …  ELSE Actions  – Action list (optional)
    ///
    /// Clicking + inserts a new ELSEIF condition, then VariableParameterMaintenance
    /// schedules insertion of the matching ELSEIF actions immediately after.
    /// Clicking - on a condition removes both that condition and its actions partner.
    /// </summary>
    public class IfStatementComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        // Number of inputs that precede the variable ELSEIF section.
        // 0: IF Condition  |  1: IF Actions
        private const int _fixedInputCount = 2;

        // Set to true while a removal is being processed so VariableParameterMaintenance
        // does not try to re-add the partner of the condition that was just removed.
        private bool _removingPair = false;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public IfStatementComponent() : base("IF Statement", "IF", "Advanced RAPID Features",
            "Creates a RAPID IF/ELSEIF/ELSE conditional block. " +
            "Use the + button just above ELSE to add ELSEIF branches.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// Layout: IF Condition (0), IF Actions (1), [ELSEIF pairs…], ELSE Actions (last).
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_RAPIDExpression(), "IF Condition", "IC",
                "Condition for the IF branch as a RAPID expression (e.g. x = 1).",
                GH_ParamAccess.item);
            pManager.AddParameter(new Param_Action(), "IF Actions", "IA",
                "Actions to execute when the IF condition is true.",
                GH_ParamAccess.list);
            // ELSE is registered last so it always sits below any ELSEIF blocks.
            pManager.AddParameter(new Param_Action(), "ELSE Actions", "EA",
                "Actions to execute in the ELSE branch. Leave unconnected to omit ELSE.",
                GH_ParamAccess.list);

            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Action(), "IF Statement", "IF",
                "RAPID IF statement as a list of code lines and actions.",
                GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // ELSE is always the last registered parameter.
            int elseIndex = Params.Input.Count - 1;

            // Number of variable params between the fixed section and ELSE.
            // Must be even (each ELSEIF block is a condition + actions pair).
            int varCount = elseIndex - _fixedInputCount;
            int pairCount = varCount / 2;

            // --- Read fixed inputs ---
            GH_RAPIDExpression ifCondExpr = null;
            List<IAction> ifActions = new List<IAction>();

            if (!DA.GetData(0, ref ifCondExpr)) { return; }
            if (!DA.GetDataList(1, ifActions)) { return; }

            // --- Read ELSEIF pairs ---
            var elseifPairs = new List<(string condition, List<IAction> actions)>();
            for (int i = 0; i < pairCount; i++)
            {
                int condIdx = _fixedInputCount + i * 2;
                int actIdx  = _fixedInputCount + i * 2 + 1;

                GH_RAPIDExpression condExpr = null;
                DA.GetData(condIdx, ref condExpr);
                string cond = HelperMethods.CheckRAPIDExpression(this, condExpr, $"ELSEIF {i + 1} condition", "true");

                List<IAction> actions = new List<IAction>();
                DA.GetDataList(actIdx, actions);
                elseifPairs.Add((cond, actions));
            }

            // --- Read ELSE (last param, optional) ---
            List<IAction> elseActions = new List<IAction>();
            DA.GetDataList(elseIndex, elseActions);

            // --- Validate IF condition ---
            string ifCond = HelperMethods.CheckRAPIDExpression(this, ifCondExpr, "IF condition", "true");

            // --- Build RAPID code ---
            List<IAction> code = new List<IAction>();

            code.Add(new CodeLine($"IF {ifCond} THEN", CodeType.Instruction));
            foreach (IAction a in ifActions) { IAction d = a.DuplicateAction(); d.IndentationLevel = a.IndentationLevel + 1; code.Add(d); }

            foreach (var (cond, actions) in elseifPairs)
            {
                code.Add(new CodeLine($"ELSEIF {cond} THEN", CodeType.Instruction));
                foreach (IAction a in actions) { IAction d = a.DuplicateAction(); d.IndentationLevel = a.IndentationLevel + 1; code.Add(d); }
            }

            if (elseActions.Count > 0)
            {
                code.Add(new CodeLine("ELSE", CodeType.Instruction));
                foreach (IAction a in elseActions) { IAction d = a.DuplicateAction(); d.IndentationLevel = a.IndentationLevel + 1; code.Add(d); }
            }

            code.Add(new CodeLine("ENDIF", CodeType.Instruction));

            DA.SetDataList(0, code);
        }

        #region IGH_VariableParameterComponent
        /// <summary>
        /// Allows the + button only at the position just before ELSE (appending a new pair).
        /// </summary>
        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            // Only allow insertion right before ELSE (the last param).
            return side == GH_ParameterSide.Input
                && index == Params.Input.Count - 1;
        }

        /// <summary>
        /// Allows the - button only on ELSEIF condition params (even offsets from _fixedInputCount)
        /// and only when at least one complete pair exists.
        /// </summary>
        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side != GH_ParameterSide.Input) return false;
            // Must be in the variable section (not fixed, not ELSE).
            if (index < _fixedInputCount || index >= Params.Input.Count - 1) return false;
            // Only allow on condition params (even offset from floor).
            int offset = index - _fixedInputCount;
            if (offset % 2 != 0) return false;
            // Require at least one complete pair.
            int varCount = Params.Input.Count - 1 - _fixedInputCount;
            return varCount >= 2;
        }

        /// <summary>
        /// Creates the ELSEIF condition parameter when + is clicked.
        /// The matching actions parameter is added afterwards by VariableParameterMaintenance.
        /// </summary>
        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            // index is the insertion position (just before ELSE).
            // The new pair number = pairs so far + 1.
            int varCount = Params.Input.Count - 1 - _fixedInputCount;
            int pairNum  = varCount / 2 + 1;

            return new Param_RAPIDExpression
            {
                Name        = $"ELSEIF {pairNum} Condition",
                NickName    = $"EC{pairNum}",
                Description = $"Condition for ELSEIF branch {pairNum} as a RAPID expression.",
                Access      = GH_ParamAccess.item
            };
        }

        /// <summary>
        /// Called when a parameter is being removed.
        /// Schedules removal of the partner actions param via ScheduleSolution so that
        /// after the condition is removed both halves of the pair disappear together.
        /// </summary>
        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input
                && index >= _fixedInputCount
                && index < Params.Input.Count - 1               // not ELSE
                && (index - _fixedInputCount) % 2 == 0)         // condition slot
            {
                _removingPair = true;
                var doc = OnPingDocument();
                doc?.ScheduleSolution(5, d =>
                {
                    // After the condition at 'index' was removed, the matching actions
                    // param shifted down to 'index'.  Guard against index being out of
                    // range in case the user made a very rapid sequence of changes.
                    if (index < Params.Input.Count - 1 && index >= _fixedInputCount)
                    {
                        Params.UnregisterInputParameter(Params.Input[index], true);
                        Params.OnParametersChanged();
                    }
                    _removingPair = false;
                    RenumberElseIfs();
                });
            }
            return true;
        }

        /// <summary>
        /// Called by Grasshopper after every param change (insert, remove, open, undo, redo).
        /// On an insertion: detects the orphaned condition (odd variable count) and schedules
        /// addition of the matching actions param via ScheduleSolution.
        /// On a removal: skips add-logic because DestroyParameter already handles the partner.
        /// Otherwise: renumbers all ELSEIF labels.
        /// </summary>
        public void VariableParameterMaintenance()
        {
            // DestroyParameter is managing a removal — don't interfere.
            if (_removingPair) return;

            int varCount = Params.Input.Count - _fixedInputCount - 1; // exclude ELSE

            if (varCount % 2 == 1)
            {
                // Odd variable count: a condition was just added without its actions partner.
                // Find the orphan (a condition whose next slot is another condition or is ELSE).
                for (int i = _fixedInputCount; i < Params.Input.Count - 1; i++)
                {
                    int offset  = i - _fixedInputCount;
                    if (offset % 2 != 0) continue; // skip actions slots

                    int nextIdx    = i + 1;
                    bool nextIsElse      = nextIdx == Params.Input.Count - 1;
                    bool nextIsCondition = !nextIsElse && (nextIdx - _fixedInputCount) % 2 == 0;

                    if (nextIsElse || nextIsCondition)
                    {
                        // This condition has no actions partner — insert one after it.
                        int insertAt = nextIdx;
                        int pairNum  = offset / 2 + 1;

                        var doc = OnPingDocument();
                        doc?.ScheduleSolution(5, d =>
                        {
                            Params.RegisterInputParam(new Param_Action
                            {
                                Name        = $"ELSEIF {pairNum} Actions",
                                NickName    = $"EA{pairNum}",
                                Description = $"Actions to execute when ELSEIF {pairNum} condition is true.",
                                Access      = GH_ParamAccess.list,
                                Optional    = true
                            }, insertAt);
                            Params.OnParametersChanged();
                            RenumberElseIfs();
                        });
                        return;
                    }
                }
            }
            else
            {
                RenumberElseIfs();
            }
        }

        /// <summary>
        /// Renumbers all ELSEIF condition and actions params to keep names sequential.
        /// </summary>
        private void RenumberElseIfs()
        {
            int varCount = Params.Input.Count - _fixedInputCount - 1;
            int pairCount = varCount / 2;

            for (int i = 0; i < pairCount; i++)
            {
                int condIdx = _fixedInputCount + i * 2;
                int actIdx  = _fixedInputCount + i * 2 + 1;

                Params.Input[condIdx].Name    = $"ELSEIF {i + 1} Condition";
                Params.Input[condIdx].NickName = $"EC{i + 1}";

                Params.Input[actIdx].Name    = $"ELSEIF {i + 1} Actions";
                Params.Input[actIdx].NickName = $"EA{i + 1}";
            }
        }
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
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.IfStatement_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D6A4B8C3-5F19-4E2A-C7D5-3B0D4F9E1A62"); }
        }
        #endregion
    }
}
