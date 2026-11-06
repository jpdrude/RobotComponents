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
// Rhino Libs
using Rhino.Geometry;
// Grasshopper Libs
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using GH_IO.Serialization;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Parameters.Actions.Instructions;
using RobotComponents.ABB.Gh.Parameters.Actions.Declarations;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
using RobotComponents.ABB.Gh.Parameters.Actions.Dynamic;
using RobotComponents.ABB.Actions.Dynamic;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Action : Connect Interrupt Component.
    /// Right-click → "Override Interrupt Variable Name" adds an optional input to override the
    /// generated intnum variable name, needed when connecting more than one interrupt to the same
    /// TRAP routine (the default int_&lt;TrapRoutineName&gt; naming would otherwise collide).
    /// </summary>
    public class ConnectInterruptComponent : GH_RobotComponent, IGH_VariableParameterComponent
    {
        #region fields
        private bool _expire = false;
        private bool _interruptNameInputParam = false;
        private const string _interruptNameParamName = "Interrupt Variable Name";

        // Analog signals (AI, AO): a Condition input appears, and a Low Value input for Between/Outside.
        private const string _conditionParamName = "Condition";
        private const string _lowValueParamName = "Low Value";
        private const string _toleranceParamName = "Tolerance";
        private static readonly List<string> _conditionNames = new List<string> { "Always", "Above", "Below", "Between", "Outside" };
        private static readonly string[] _conditionRapid = { "AIO_ALWAYS", "AIO_ABOVE_HIGH", "AIO_BELOW_HIGH", "AIO_BETWEEN", "AIO_OUTSIDE" };
        private const int _conditionAlways = 0, _conditionAbove = 1, _conditionBetween = 3, _conditionOutside = 4;
        private bool _analogParamsPending = false;
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, subcategory the panel.
        /// If you use non-existing tab or panel names new tabs/panels will automatically be created.
        /// </summary>
        public ConnectInterruptComponent() : base("Connect Interrupt", "CI", "Advanced RAPID Features",
              "Connects a TRAP routine to a signal change. Right-click to override the generated " +
              "interrupt variable name, e.g. to connect more than one interrupt to the same TRAP routine.")

        {
        }

        /// <inheritdoc/>
        public override IReadOnlyList<(string Name, string NickName)> OptionalParameterDefaults => new[]
        {
            (_interruptNameParamName, "IVN"),
            (_conditionParamName, "C"),
            (_lowValueParamName, "LV"),
            (_toleranceParamName, "TOL"),
        };

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TRAP Routine Name", "TR", "Name of the TRAP routine to be called when the signal change occurs.", GH_ParamAccess.item);
            pManager.AddParameter(new Param_GenericObject(), "Signal Name", "SN",
                "Name of the signal that is monitored for changes. In Persistent Data mode, this is instead the " +
                "RAPID persistent (PERS) variable to monitor: connect a RAPID Variable component, or type its name.",
                GH_ParamAccess.item);
            pManager.AddNumberParameter("Signal Value", "SV",
                "Value of the signal that triggers the interrupt when the signal changes to this value. " +
                "For analog signals (AI, AO) it is the threshold for Condition Above/Below, or the upper limit for Between/Outside. " +
                "If not connected, a digital interrupt fires on every change of the signal. " +
                "Not used for group signals (GI, GO) and in Persistent Data mode: the RAPID ISignalGI, " +
                "ISignalGO and IPers instructions have no triggering value and always fire on every change.",
                GH_ParamAccess.item);
            pManager.AddIntegerParameter("Signal Type", "ST", "Type of Signal to be monitored (DI, DO, AI, AO, GI, GO, or Persistent Data)", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_CodeLine(), "Connect Code", "CC", "Code to connect interrupt. Both Declaration and Instruction Code is generated");
            pManager.RegisterParam(new Param_CodeLine(), "Enable Interrupts", "IE",
                "RAPID IEnable; instruction, (re-)enabling interrupts. Interrupts that were registered while " +
                "interrupts were disabled are queued and executed once interrupts are re-enabled with this instruction.",
                GH_ParamAccess.item);
            pManager.RegisterParam(new Param_CodeLine(), "Disable Interrupts", "ID",
                "RAPID IDisable; instruction, disabling interrupts. Interrupts that are registered while interrupts " +
                "are disabled are not discarded: they are queued to be executed once interrupts are re-enabled.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Creates the input value list and attachs it to the input parameter
            if (this.Params.Input[3].SourceCount == 0)
            {
                _expire = true;
                HelperMethods.CreateValueList(this, typeof(SignalType), 3);
            }

            // Expire solution of this component
            if (_expire == true)
            {
                _expire = false;
                this.ExpireSolution(true);
                return;
            }

            // The Enable/Disable Interrupts outputs are static instructions, independent of the
            // connect-interrupt inputs, so they are always available.
            DA.SetData(1, new CodeLine("IEnable;", CodeType.Instruction));
            DA.SetData(2, new CodeLine("IDisable;", CodeType.Instruction));

            // Declare variables to store input data
            string interruptName = string.Empty;
            string trapRoutineName = string.Empty;
            object rawSignalName = null;
            double signalValue = 1;
            int signalTypeInt = 1;

            // Get data from input parameters
            if (!DA.GetData(0, ref trapRoutineName)) return;
            if (!DA.GetData(1, ref rawSignalName)) return;
            bool hasSignalValue = DA.GetData(2, ref signalValue);
            if (!DA.GetData(3, ref signalTypeInt)) signalTypeInt = 1;

            // Resolves to the signal name (plain text) or, in Persistent Data mode, the declared
            // name of the connected RAPID Variable -- same handling used for any other input that
            // accepts either a literal or a RAPID declaration/variable/expression.
            string signalName = HelperMethods.ResolveRAPIDValueExpression(rawSignalName);

            if ((SignalType)signalTypeInt == SignalType.PersistentData && hasSignalValue)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Signal Value is connected but is not used in Persistent Data mode: the RAPID IPers " +
                    "instruction has no triggering value. Disconnect it to remove this warning.");
            }

            if (((SignalType)signalTypeInt == SignalType.GroupInput || (SignalType)signalTypeInt == SignalType.GroupOutput) && hasSignalValue)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Signal Value is connected but is not used for group signals: the RAPID ISignalGI and " +
                    "ISignalGO instructions have no triggering value and fire on every change of the signal. " +
                    "Disconnect it to remove this warning.");
            }

            // Without a Signal Value a digital interrupt fires on every change of the signal (trigger value 2, both edges).
            string digitalTrigger = hasSignalValue ? ((int)signalValue).ToString() : "2";

            // Analog signals get a Condition input (and a Low Value input for Between/Outside). The inputs are
            // added/removed in a scheduled solution, as the Message Box does with its button inputs.
            bool analog = signalTypeInt == (int)SignalType.AnalogInput || signalTypeInt == (int)SignalType.AnalogOutput;
            int conditionIndex = Params.Input.FindIndex(x => x.Name == _conditionParamName);
            int condition = -1;
            if (conditionIndex != -1 && !DA.GetData(conditionIndex, ref condition)) { condition = -1; }
            bool wantLowValue = analog && (condition == _conditionBetween || condition == _conditionOutside);
            int lowValueIndex = Params.Input.FindIndex(x => x.Name == _lowValueParamName);
            int toleranceIndex = Params.Input.FindIndex(x => x.Name == _toleranceParamName);

            if ((conditionIndex != -1) != analog || (lowValueIndex != -1) != wantLowValue || (toleranceIndex != -1) != analog)
            {
                ScheduleAnalogParameters(analog, wantLowValue, hasSignalValue);
                return;
            }

            // ISignalAI/ISignalAO take Condition, HighValue, LowValue, DeltaValue.
            string analogTrigger = "AIO_ALWAYS, 0, 0, 0";
            if (analog)
            {
                string Num(double value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // DeltaValue: the signal must differ by more than this from the value at the last
                // interrupt before a new interrupt is generated. 0 when not connected.
                double tolerance = 0;
                if (!DA.GetData(toleranceIndex, ref tolerance)) { tolerance = 0; }
                if (tolerance < 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Tolerance can't be negative.");
                    return;
                }

                if (condition < 0 || condition >= _conditionRapid.Length)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Condition must be 0 to 4: Always, Above, Below, Between or Outside.");
                    return;
                }

                if (condition == _conditionAlways)
                {
                    if (hasSignalValue)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Signal Value isn't used with Condition Always: the interrupt fires on every change.");
                    }
                }
                else if (!hasSignalValue)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Condition {_conditionNames[condition]} needs a Signal Value.");
                    return;
                }
                else if (condition == _conditionBetween || condition == _conditionOutside)
                {
                    double lowValue = 0;
                    if (!DA.GetData(lowValueIndex, ref lowValue))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Condition {_conditionNames[condition]} needs a Low Value (the Signal Value is the upper limit).");
                        return;
                    }

                    if (lowValue >= signalValue)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Low Value should be below the Signal Value (the upper limit).");
                    }

                    analogTrigger = $"{_conditionRapid[condition]}, {Num(signalValue)}, {Num(lowValue)}, {Num(tolerance)}";
                }
                else
                {
                    analogTrigger = $"{_conditionRapid[condition]}, {Num(signalValue)}, 0, {Num(tolerance)}";
                }

                if (condition == _conditionAlways)
                {
                    analogTrigger = $"AIO_ALWAYS, 0, 0, {Num(tolerance)}";
                }
            }

            // Optional override for the generated intnum variable name. Needed to connect more
            // than one interrupt to the same TRAP routine: the default int_<TrapRoutineName>
            // naming is purely a function of the trap name, so two components targeting the same
            // trap would otherwise both try to declare the same intnum variable.
            string interruptNameOverride = null;
            if (_interruptNameInputParam)
            {
                int interruptNameParamIndex = Params.Input.FindIndex(x => x.Name == _interruptNameParamName);
                if (interruptNameParamIndex != -1)
                {
                    DA.GetData(interruptNameParamIndex, ref interruptNameOverride);
                }
            }

            if (!string.IsNullOrWhiteSpace(interruptNameOverride))
            {
                interruptName = interruptNameOverride.Trim();

                if (!RobotComponents.ABB.Utils.HelperMethods.IsValidRapidIdentifier(interruptName))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        $"Interrupt Variable Name \"{interruptName}\" is not a valid RAPID identifier.");
                }
            }
            else
            {
                interruptName = "int_" + trapRoutineName;
            }

            //Create CodeLine Container
            List<CodeLine> codeLines = new List<CodeLine>();

            //Define declaration and connect code
            codeLines.Add(new CodeLine("VAR intNum " + interruptName + ";", CodeType.Declaration));
            codeLines.Add(new CodeLine("CONNECT " + interruptName + " WITH " + trapRoutineName + ";", CodeType.Instruction));

            switch (signalTypeInt)
            {
                case 0:
                    codeLines.Add(new CodeLine("ISignalDI " + signalName + ", " + digitalTrigger + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 1:
                    codeLines.Add(new CodeLine("ISignalDO " + signalName + ", " + digitalTrigger + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 2:
                    codeLines.Add(new CodeLine("ISignalAI " + signalName + ", " + analogTrigger + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 3:
                    codeLines.Add(new CodeLine("ISignalAO " + signalName + ", " + analogTrigger + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 4:
                    // Group signals: ISignalGI/ISignalGO take no triggering value and fire on every change.
                    codeLines.Add(new CodeLine("ISignalGI " + signalName + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 5:
                    codeLines.Add(new CodeLine("ISignalGO " + signalName + ", " + interruptName + ";", CodeType.Instruction));
                    break;
                case 6:
                    // Persistent data: IPers takes the PERS name and the interrupt variable, no
                    // triggering value (unlike the ISignalXX instructions above).
                    codeLines.Add(new CodeLine("IPers " + signalName + ", " + interruptName + ";", CodeType.Instruction));
                    break;
            }

            //Set output data
            HelperMethods.SetActionList(this, DA, 0, codeLines);
        }

        #region analog condition inputs
        /// <summary>
        /// Adds or removes the Condition, Low Value and Tolerance inputs in a scheduled solution (inputs can't change
        /// while this component is being solved). A new Condition input gets a value list, starting at
        /// Above when a Signal Value is connected and at Always otherwise, so files from before keep
        /// generating the same RAPID.
        /// </summary>
        private void ScheduleAnalogParameters(bool wantCondition, bool wantLowValue, bool hasSignalValue)
        {
            if (_analogParamsPending) { return; }

            GH_Document document = OnPingDocument();
            if (document == null) { return; }

            _analogParamsPending = true;

            document.ScheduleSolution(5, d =>
            {
                _analogParamsPending = false;

                IGH_Param conditionParam = Params.Input.FirstOrDefault(x => x.Name == _conditionParamName);
                IGH_Param lowValueParam = Params.Input.FirstOrDefault(x => x.Name == _lowValueParamName);
                IGH_Param toleranceParam = Params.Input.FirstOrDefault(x => x.Name == _toleranceParamName);
                bool createdCondition = false;

                if (!wantCondition && toleranceParam != null)
                {
                    Params.UnregisterInputParameter(toleranceParam, true);
                }

                if (!wantLowValue && lowValueParam != null)
                {
                    Params.UnregisterInputParameter(lowValueParam, true);
                }

                if (!wantCondition && conditionParam != null)
                {
                    RemoveOwnValueLists(conditionParam, d);
                    Params.UnregisterInputParameter(conditionParam, true);
                    conditionParam = null;
                }

                if (wantCondition && conditionParam == null)
                {
                    conditionParam = new Param_Integer
                    {
                        Name = _conditionParamName,
                        NickName = "C",
                        Description = "When the analog interrupt fires: 0 Always (every change), 1 Above the Signal Value, " +
                                      "2 Below the Signal Value, 3 Between Low Value and Signal Value, 4 Outside Low Value and Signal Value.",
                        Access = GH_ParamAccess.item,
                        Optional = true
                    };
                    HelperMethods.ApplyFullNamesPreference(conditionParam);
                    Params.RegisterInputParam(conditionParam, 4);
                    createdCondition = true;
                }

                if (wantLowValue && Params.Input.All(x => x.Name != _lowValueParamName))
                {
                    Param_Number newLowValue = new Param_Number
                    {
                        Name = _lowValueParamName,
                        NickName = "LV",
                        Description = "Lower limit for Condition Between and Outside (the Signal Value is the upper limit).",
                        Access = GH_ParamAccess.item,
                        Optional = true
                    };
                    HelperMethods.ApplyFullNamesPreference(newLowValue);
                    Params.RegisterInputParam(newLowValue, Params.Input.IndexOf(conditionParam) + 1);
                }

                if (wantCondition && Params.Input.All(x => x.Name != _toleranceParamName))
                {
                    Param_Number newTolerance = new Param_Number
                    {
                        Name = _toleranceParamName,
                        NickName = "TOL",
                        Description = "DeltaValue of ISignalAI/ISignalAO: the signal must differ by more than this from its value " +
                                      "at the last interrupt before a new interrupt is generated. 0 (default) = any change.",
                        Access = GH_ParamAccess.item,
                        Optional = true
                    };
                    HelperMethods.ApplyFullNamesPreference(newTolerance);
                    int lastAnalog = Math.Max(Params.Input.IndexOf(conditionParam), Params.Input.FindIndex(x => x.Name == _lowValueParamName));
                    Params.RegisterInputParam(newTolerance, lastAnalog + 1);
                }

                Params.OnParametersChanged();

                if (createdCondition && conditionParam.SourceCount == 0)
                {
                    int index = Params.Input.IndexOf(conditionParam);
                    if (HelperMethods.CreateValueList(this, _conditionNames, index) && conditionParam.Sources.FirstOrDefault() is GH_ValueList valueList)
                    {
                        valueList.NickName = "Analog Condition";
                        valueList.SelectItem(hasSignalValue ? _conditionAbove : _conditionAlways);
                    }
                }

                ExpireSolution(false);
            });
        }

        /// <summary>
        /// Deletes value lists that feed only this input (the one created with the Condition input), so
        /// switching back to a digital signal doesn't leave an unconnected list on the canvas.
        /// </summary>
        private static void RemoveOwnValueLists(IGH_Param param, GH_Document document)
        {
            foreach (IGH_Param source in param.Sources.ToList())
            {
                if (source is GH_ValueList valueList && valueList.Recipients.Count == 1)
                {
                    document.RemoveObject(valueList, false);
                }
            }
        }
        #endregion

        #region menu items
        /// <summary>
        /// Appends "Override Interrupt Variable Name" toggle to the right-click context menu.
        /// </summary>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Override Interrupt Variable Name", MenuItemClickInterruptName, true, _interruptNameInputParam);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void MenuItemClickInterruptName(object sender, EventArgs e)
        {
            RecordUndoEvent("Override Interrupt Variable Name");
            _interruptNameInputParam = !_interruptNameInputParam;
            ToggleInterruptNameParam();
        }

        /// <summary>
        /// Adds or removes the optional Interrupt Variable Name input parameter.
        /// </summary>
        private void ToggleInterruptNameParam()
        {
            if (_interruptNameInputParam)
            {
                Param_String interruptNameParam = new Param_String
                {
                    Name        = _interruptNameParamName,
                    NickName    = "IVN",
                    Description = "Overrides the generated interrupt (intnum) variable name, instead of the " +
                                  "default \"int_\" + TRAP Routine Name. Needed when connecting more than one " +
                                  "interrupt to the same TRAP routine, since RAPID requires each intnum to have " +
                                  "a distinct name.",
                    Access      = GH_ParamAccess.item,
                    Optional    = true
                };

                HelperMethods.ApplyFullNamesPreference(interruptNameParam);
                Params.RegisterInputParam(interruptNameParam);
            }
            else
            {
                var param = Params.Input.FirstOrDefault(x => x.Name == _interruptNameParamName);
                if (param != null)
                    Params.UnregisterInputParameter(param, true);
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        #endregion

        #region serialization
        /// <summary>
        /// Add our own fields. Needed for (de)serialization of the variable input parameter.
        /// </summary>
        /// <param name="writer"> Provides access to a subset of GH_Chunk methods used for writing archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("InterruptNameInputParam", _interruptNameInputParam);
            return base.Write(writer);
        }

        /// <summary>
        /// Read our own fields. Needed for (de)serialization of the variable input parameter.
        /// </summary>
        /// <param name="reader"> Provides access to a subset of GH_Chunk methods used for reading archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("InterruptNameInputParam"))
                _interruptNameInputParam = reader.GetBoolean("InterruptNameInputParam");
            return base.Read(reader);
        }
        #endregion

        #region IGH_VariableParameterComponent
        // Menu-driven only — no + / - buttons.
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index) => false;
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index) => false;
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index) => null;
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index) => false;
        void IGH_VariableParameterComponent.VariableParameterMaintenance() { }
        #endregion

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
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
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ConnectInterrupt_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("774F2525-1176-49B6-9C8D-887EC696896A"); }
        }
        #endregion


     
    }
}