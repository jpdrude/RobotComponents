// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2018-2020 EDEK Uni Kassel
// Copyright (c) 2020-2024 Arjen Deetman
//
// Authors:
//   - Gabriel Rumph (2018-2020)
//   - Benedikt Wannemacher (2018-2020)
//   - Arjen Deetman (2019-2024)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Drawing;
// Grasshopper Libs
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Gh.Goos.Actions;
using RobotComponents.ABB.Gh.Goos.Definitions;

namespace RobotComponents.ABB.Gh.Utils
{
    /// <summary>
    /// Helper methods and properties
    /// </summary>
    internal static class HelperMethods
    {
        #region path generator hashing
        /// <summary>
        /// Returns a hash of everything in an action that can change a calculated path or its timing.
        /// </summary>
        /// <remarks>
        /// Used by the (Timed) Path Generator to decide whether a cached path is still valid. For a
        /// Movement: type, target, circle point, speed, time, zone, work object, tool and digital output.
        /// Any other instruction is hashed by its RAPID line, so new or changed instructions (e.g. Velocity
        /// Set, Acceleration Set) also cause a recalculation.
        /// </remarks>
        /// <param name="action"> The action. </param>
        /// <param name="robot"> The robot the path is calculated for. </param>
        /// <returns> The hash. </returns>
        public static int GetPathRelevantHash(RobotComponents.ABB.Actions.IAction action, RobotComponents.ABB.Definitions.Robot robot)
        {
            if (action == null) { return 0; }

            string text;

            if (action is RobotComponents.ABB.Actions.Instructions.Movement movement)
            {
                text = string.Join("|",
                    movement.MovementType.ToString(),
                    TargetValues(movement.Target),
                    movement.CircularPoint != null && movement.CircularPoint.Plane.IsValid ? movement.CircularPoint.ToRAPID(false) : "",
                    movement.SpeedData?.Name ?? "", movement.SpeedData?.ToRAPID() ?? "",
                    movement.Time.ToString(CultureInfo.InvariantCulture),
                    movement.ZoneData?.Name ?? "", movement.ZoneData?.ToRAPID() ?? "",
                    movement.WorkObject?.Name ?? "", movement.WorkObject?.ToRAPID() ?? "",
                    movement.RobotTool?.Name ?? "", movement.RobotTool?.ToRAPID() ?? "",
                    movement.SetDigitalOutput != null && movement.SetDigitalOutput.IsValid ? movement.SetDigitalOutput.ToRAPIDInstruction(robot) : "");
            }
            else if (action is RobotComponents.ABB.Actions.Instructions.OverrideRobotTool overrideRobotTool)
            {
                // The RAPID line only names the tool; the path depends on its data (TCP).
                text = "OverrideRobotTool|" + (overrideRobotTool.RobotTool?.Name ?? "") + "|" + (overrideRobotTool.RobotTool?.ToRAPID() ?? "");
            }
            else if (action is RobotComponents.ABB.Actions.Instructions.IInstruction instruction)
            {
                try { text = action.GetType().Name + "|" + instruction.ToRAPIDInstruction(robot); }
                catch { text = action.GetType().Name + "|" + action.ToString(); }
            }
            else
            {
                text = action.GetType().Name + "|" + action.ToString();
            }

            return text.GetHashCode();
        }

        /// <summary>
        /// The RAPID values of a target for hashing: named parts (Robot Joint Position, Configuration
        /// Data, External Joint Position) as their values, not their names, so that changing a joint
        /// value or a quadrant also changes the hash.
        /// </summary>
        private static string TargetValues(RobotComponents.ABB.Actions.Declarations.ITarget target)
        {
            switch (target)
            {
                case null: return "";
                case RobotTarget robotTarget: return robotTarget.ToRAPID(false);
                case JointTarget jointTarget: return jointTarget.ToRAPID(false);
                // Offs(pickA, ...) names its base target; include the base target's values too.
                case OffsetTarget offsetTarget: return offsetTarget.ToRAPID() + "|" + TargetValues(offsetTarget.Target);
                default: return target.ToRAPID();
            }
        }
        #endregion

        #region typed action trees
        /// <summary>
        /// Rebuilds a typed data tree from an output that carries actions (Param_Action), keeping
        /// the paths and the very same action objects.
        /// </summary>
        /// <remarks>
        /// Components whose output became Param_Action still keep a typed tree for their
        /// after-solve bookkeeping (unique names, object manager). Casting the volatile data to
        /// that typed structure returns null, because the output now holds GH_Action items.
        /// The wrapped objects are the output's own, so renaming them (e.g. SyncID) changes the
        /// output data as before.
        /// </remarks>
        /// <typeparam name="T"> The action type, e.g. SyncMoveOn. </typeparam>
        /// <typeparam name="TGoo"> The goo type of the typed tree, e.g. GH_SyncMoveOn. </typeparam>
        /// <param name="output"> The output parameter. </param>
        /// <param name="wrap"> Wraps an action in its goo without copying it. </param>
        /// <returns> The typed tree. </returns>
        public static GH_Structure<TGoo> GetTypedActionTree<T, TGoo>(IGH_Param output, Func<T, TGoo> wrap)
            where T : class
            where TGoo : IGH_Goo
        {
            GH_Structure<TGoo> tree = new GH_Structure<TGoo>();

            foreach (GH_Path path in output.VolatileData.Paths)
            {
                foreach (object item in output.VolatileData.get_Branch(path))
                {
                    if (item is GH_Action action && action.Value is T value)
                    {
                        tree.Append(wrap(value), path);
                    }
                }
            }

            return tree;
        }
        #endregion

        #region action output
        /// <summary>
        /// Sets a list of actions on an output without the extra data-tree level Grasshopper
        /// adds on its own.
        /// </summary>
        /// <remarks>
        /// Grasshopper appends the iteration index to the path of every list output of a
        /// component that has at least one item-access input ({0} becomes {0;0}, {0;0} becomes
        /// {0;0;0}). For components that produce a block of RAPID actions (loops, IF, Code, ...)
        /// that extra level is harmful: a Move outputs on {0}, an IF on {0;0}, and merged action
        /// lists are then ordered by branch path instead of by wire order (and list-access
        /// consumers run once per branch, e.g. one module or one loop per branch).
        /// The index is appended inside SetDataList; ParameterTargetPath still returns the input
        /// branch's path. Setting the list as a tree on that path keeps the actions on the same
        /// branch as every other action. Several iterations on the same branch are appended in
        /// iteration order.
        /// </remarks>
        /// <para>
        /// SetDataTree appends to the output. If a component's solve runs again inside the running
        /// solve (e.g. when it creates a value list on its first solve), both runs would write and the
        /// actions would appear twice. So the output is emptied on the first iteration: a nested run's
        /// data is discarded, and the following iterations append as usual.
        /// </para>
        /// <typeparam name="T"> The item type. </typeparam>
        /// <param name="component"> The component that sets the data. </param>
        /// <param name="DA"> The data access object of the current iteration. </param>
        /// <param name="index"> The output index. </param>
        /// <param name="data"> The actions to output. </param>
        public static void SetActionList<T>(GH_Component component, IGH_DataAccess DA, int index, IEnumerable<T> data)
        {
            if (DA.Iteration == 0)
            {
                component.Params.Output[index].VolatileData.Clear();
            }

            DataTree<T> tree = new DataTree<T>();
            tree.AddRange(data, DA.ParameterTargetPath(index));
            DA.SetDataTree(index, tree);
        }
        #endregion

        #region fields
        /// <summary>
        /// The RAPID comparison operator symbols, in the exact order of the ComparisonOperator
        /// enum's underlying values (LT=0, GT=1, LE=2, GE=3, EQ=4, NE=5) -- the single source of
        /// truth for any value list that lets a user pick one, so a dropped-down "Comparison
        /// Operators" value list always shows the actual RAPID symbols (&lt;, &gt;, ...) rather
        /// than the enum's C# member names (LT, GT, ...).
        /// </summary>
        /// <remarks>
        /// A literal '=' can't be used as a GH value list item's display name -- GH_ValueListItem
        /// stores each item as a "name=expression" string pair, so a literal '=' inside the name
        /// half corrupts that parsing. EQ uses the visually identical fullwidth equals sign
        /// (U+FF1D, "＝") instead, display-only: the RAPID code actually generated for EQ is still
        /// the plain ASCII "=" (see ComparerExpressionComponent.OperatorToRAPID), since RAPID has
        /// no notion of the fullwidth character.
        /// </remarks>
        public static readonly List<string> ComparisonOperatorSymbols =
            new List<string>() { "<", ">", "<=", ">=", "\uFF1D", "<>" };
        #endregion

        #region methods
        /// <summary>
        /// Method to check if a string is longer then 32 characters. 
        /// </summary>
        /// <param name="variable"> The string to check for its length. </param>
        /// <returns> Returns true if the string longer then 32 characters. Otherwise the return value is false. </returns>
        public static bool StringExeedsCharacterLimit32(string variable)
        {
            int length = variable.Length;

            if (length < 32)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// A method to check if a string starts with a digit. 
        /// </summary>
        /// <param name="variable"> The string to check for. </param>
        /// <returns> Returns a boolean that defines if the string starts with a digit. </returns>
        public static bool StringStartsWithNumber(string variable)
        {
            if (variable.Length > 0)
            {
                bool isDigit = char.IsNumber(variable[0]); ;
                return isDigit;
            }

            return false;
        }

        /// <summary>
        /// A method to check if a string contains special characters.
        /// </summary>
        /// <param name="variable"> The string to check for. </param>
        /// <returns> Returns a boolean that defines if the string has special characters. </returns>
        public static bool StringHasSpecialCharacters(string variable)
        {
            if (variable.Length > 0)
            {
                Regex rgx = new Regex("[^A-Za-z0-9:_]");
                return rgx.IsMatch(variable);
            }

            return false;
        }

        /// <summary>
        /// Replaces the spaces with an underscore and removes new lines in a list with strings. 
        /// Typically used to edit variable names of declarations.
        /// </summary>
        /// <param name="strings"> The list with strings. </param>
        /// <returns> The list with strings. </returns>
        public static List<string> ReplaceSpacesAndRemoveNewLines(List<string> strings)
        {
            List<string> result = new List<string>() { };

            for (int i = 0; i < strings.Count; i++)
            {
                result.Add(ReplaceSpacesAndRemoveNewLines(strings[i]));
            }

            return result;
        }

        /// <summary>
        /// Replaces the spaces with an underscore and removes new lines in a string. 
        /// Typically used to edit variable names of declarations.
        /// </summary>
        /// <param name="text"> The string. </param>
        /// <returns> The new string. </returns>
        public static string ReplaceSpacesAndRemoveNewLines(string text)
        {
            string result = text.Replace(" ", "_");
            result = result.Replace("\n", "");
            result = result.Replace("\r", "");

            return result;
        }

        /// <summary>
        /// Compares are string with an other string and checks if these are equal. 
        /// </summary>
        /// <param name="text1"> The first string to compare with. </param>
        /// <param name="text2"> The second string to compare with. </param>
        /// <returns> Specifies wethers the two strings are equal </returns>
        public static bool Equality(this string text1, string text2)
        {
            char[] chars1 = text1.ToCharArray();
            char[] chars2 = text2.ToCharArray();

            if (chars1.Length != chars2.Length)
            {
                return false;
            }

            for (int i = 0; i < chars1.Length; i++)
            {
                if (chars1[i] != chars2[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Extracts the RAPID expression string from a <see cref="GH_RAPIDExpression"/> and emits a
        /// Grasshopper warning when the expression fails the lightweight validity check.
        /// Accepts both <see cref="GH_RAPIDExpression"/> inputs wired from a parameter and plain
        /// strings, because <see cref="GH_RAPIDExpression.CastFrom"/> already handles string casting.
        /// </summary>
        /// <param name="component"> Component that owns the input (used to attach the warning). </param>
        /// <param name="goo"> The goo object to read; may be null. </param>
        /// <param name="paramName"> Human-readable parameter name used in the warning message. </param>
        /// <param name="fallback"> Value returned when <paramref name="goo"/> is null or carries an empty expression. </param>
        /// <returns> The expression string, or <paramref name="fallback"/> when the goo is null/empty. </returns>
        public static string CheckRAPIDExpression(GH_Component component, GH_RAPIDExpression goo, string paramName, string fallback = "")
        {
            string expr = goo?.Value?.Expression ?? fallback;
            if (!RAPIDExpression.IsValidExpression(expr))
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"{paramName} \"{expr}\" is not a valid RAPID expression.");
            return expr;
        }

        /// <summary>
        /// Resolves a routine argument value to RAPID text. Like <see cref="ResolveRAPIDValueExpression"/>,
        /// except that robot targets, joint targets, robot/external joint positions and configuration
        /// data are always written as their values, never by name.
        /// </summary>
        /// <param name="raw"> The raw value, either an <see cref="IGH_Goo"/> or an already-unwrapped object. </param>
        /// <returns> The RAPID text for <paramref name="raw"/>, or null if <paramref name="raw"/> is null. </returns>
        public static string ResolveRAPIDArgumentValue(object raw)
        {
            // Target data passed as a routine argument is written as its value. By name it would
            // need a declaration, and nothing declares a target that is only used as an argument
            // (the call is plain text). A value is always valid for a by-value argument.
            object value = raw is IGH_Goo goo ? goo.ScriptVariable() : raw;

            switch (value)
            {
                case RobotTarget robotTarget:
                    return robotTarget.ToRAPID(false);
                case JointTarget jointTarget:
                    return jointTarget.ToRAPID(false);
                case RobotJointPosition robotJointPosition:
                    return robotJointPosition.ToRAPID();
                case ExternalJointPosition externalJointPosition:
                    return externalJointPosition.ToRAPID();
                case ConfigurationData configurationData:
                    return configurationData.ToRAPID();
                default:
                    return ResolveRAPIDValueExpression(raw);
            }
        }

        /// <summary>
        /// Resolves an arbitrary input value (typically read via <c>DA.GetData(i, ref object)</c>) to the
        /// RAPID text it should contribute to generated code, e.g. an argument value or the right-hand side
        /// of an assignment. A RAPID declaration/variable (Robot Target, Speed Data, RAPID Variable, ...) is
        /// resolved to its declared name so the generated code references the existing declaration, falling
        /// back to its inline RAPID value if it was not given a name. A RAPID Expression is resolved to its
        /// raw expression text. Anything else (bool, number, string, ...) is converted with invariant
        /// formatting so it produces valid RAPID syntax regardless of the current culture.
        /// </summary>
        /// <param name="raw"> The raw value, either an <see cref="IGH_Goo"/> or an already-unwrapped object. </param>
        /// <returns> The RAPID text for <paramref name="raw"/>, or null if <paramref name="raw"/> is null. </returns>
        public static string ResolveRAPIDValueExpression(object raw)
        {
            object value = raw is IGH_Goo goo ? goo.ScriptVariable() : raw;

            switch (value)
            {
                case null:
                    return null;
                case RAPIDVariable rapidVariable:
                    return rapidVariable.Name;
                case RAPIDExpression rapidExpression:
                    return rapidExpression.Expression;
                case RoutineArgument routineArgument:
                    return routineArgument.ToCallString();
                case IDeclaration declaration:
                    return !string.IsNullOrEmpty(declaration.Name) ? declaration.Name : declaration.ToRAPID();
                case bool boolean:
                    return boolean ? "TRUE" : "FALSE";
                case double number:
                    return number.ToString("0.######", CultureInfo.InvariantCulture);
                case float number:
                    return number.ToString("0.######", CultureInfo.InvariantCulture);
                case int integer:
                    return integer.ToString(CultureInfo.InvariantCulture);
                case string text:
                    return text;
                default:
                    return value.ToString();
            }
        }

        /// <summary>
        /// Creates the panel with the signal name and connects it to the input parameter
        /// </summary>
        /// <param name="component"> Component to connect to. </param>
        /// <param name="text"> Text to place inside the panel. </param>
        /// <param name="index"> Index of the input to connect the panel to. </param>
        /// <returns> True, if created. </returns>
        public static bool CreatePanel(GH_Component component, string text, int index)
        {
            try
            {
                // Parameter
                var parameter = component.Params.Input[index];

                // Create the  panel
                GH_Panel panel = new GH_Panel();
                panel.SetUserText(text);
                panel.Properties.Colour = Color.White;

                // Add the panel to the active canvas
                Instances.ActiveCanvas.Document.AddObject(panel, false);
                panel.Attributes.ExpireLayout();
                panel.Attributes.PerformLayout();

                // Set the location and size of the panel
                panel.Attributes.Bounds = new RectangleF(0.0f, 0.0f, 160.0f, 20.0f);
                float x = component.Attributes.DocObject.Attributes.Bounds.Left - panel.Attributes.Bounds.Width - 30;
                float y = parameter.Attributes.Bounds.Y;
                panel.Attributes.Pivot = new System.Drawing.PointF(x, y);

                // Connect the panel to the input parameter
                parameter.RemoveAllSources();
                parameter.AddSource(panel);

                // Return that it's created
                return true;
            }
            catch
            {
                // Return that it isn't created
                return false;
            }
        }

        /// <summary>
        /// Creates a Grasshopper value list from a list with strings and connects it to an input parameter.  
        /// Returns true if it's created.
        /// </summary>
        /// <param name="component"> Component to connect to. </param>
        /// <param name="names"> List with strings. </param>
        /// <param name="index"> Index of the input to connect the value list to. </param>
        /// <returns> True, if created. </returns>
        public static bool CreateValueList(GH_Component component, List<string> names, int index)
        {
            try
            {
                // Lay out the component first: a parameter registered a moment ago (e.g. a dynamically
                // added input) still has its grip at the origin, which put the value list near (0, 0).
                component.Attributes.ExpireLayout();
                component.Attributes.PerformLayout();

                var parameter = component.Params.Input[index];

                // Create the value list
                GH_ValueList obj = CreateValueList(names);

                // Add the value list to the active canvas
                Instances.ActiveCanvas.Document.AddObject(obj, false);
                obj.Attributes.ExpireLayout();
                obj.Attributes.PerformLayout();

                // Make point where the valuelist should be created on the canvas                
                float dx = obj.Attributes.OutputGrip.X - parameter.Attributes.InputGrip.X;
                float dy = obj.Attributes.OutputGrip.Y - parameter.Attributes.InputGrip.Y;
                obj.Attributes.Pivot = new PointF(obj.Attributes.Pivot.X - dx - 25, obj.Attributes.Pivot.Y - dy);
                obj.Attributes.ExpireLayout();

                // Connect the value list to the input parameter
                parameter.AddSource(obj);

                // Collect data
                parameter.CollectData();

                // Expire value list
                obj.ExpireSolution(true);

                //Return that it's created
                return true;
            }
            catch
            {
                //Return that it isn't created
                return false;
            }
        }

        /// <summary>
        /// Return a value list populated with names.
        /// </summary>
        /// <param name="names"> List with strings to take the names from. </param>
        /// <returns> The value list with data. </returns>
        private static GH_ValueList CreateValueList(List<string> names)
        {
            // Creates the empty value list
            GH_ValueList obj = new GH_ValueList();
            obj.CreateAttributes();
            obj.ListMode = GH_ValueListMode.DropDown;
            obj.ListItems.Clear();

            for (int i = 0; i < names.Count; i++)
            {
                obj.ListItems.Add(new GH_ValueListItem(names[i], i.ToString()));
            }

            return obj;
        }

        /// <summary>
        /// Creates a Grasshopper value list from an enum and connects it to an input parameter.  
        /// Returns true if it's created.
        /// </summary>
        /// <param name="component"> Component to connect to. </param>
        /// <param name="enumType"> Enumeration to take values from. </param>
        /// <param name="index"> Index of the input to connect the value list to. </param>
        /// <returns> True, if created. </returns>
        public static bool CreateValueList(GH_Component component, Type enumType, int index)
        {
            try
            {
                // Force a layout pass so newly (dynamically) registered input parameters
                // have valid grip positions before we read them below. Without this, a
                // parameter that was just added to Params.Input in the same call stack
                // (e.g. from a right-click menu handler) still has its default, unlaid-out
                // Attributes, which reads as (0,0) and places the value list at the origin.
                component.Attributes.ExpireLayout();
                component.Attributes.PerformLayout();

                var parameter = component.Params.Input[index];

                // Create the value list
                GH_ValueList obj = CreateValueList(enumType);

                // Add the value list to the active canvas
                Instances.ActiveCanvas.Document.AddObject(obj, false);
                obj.Attributes.ExpireLayout();
                obj.Attributes.PerformLayout();

                // Make point where the valuelist should be created on the canvas
                float dx = obj.Attributes.OutputGrip.X - parameter.Attributes.InputGrip.X;
                float dy = obj.Attributes.OutputGrip.Y - parameter.Attributes.InputGrip.Y;
                obj.Attributes.Pivot = new PointF(obj.Attributes.Pivot.X - dx - 25, obj.Attributes.Pivot.Y - dy);
                obj.Attributes.ExpireLayout();

                // Connect the value list to the input parameter
                parameter.AddSource(obj);

                // Collect data
                parameter.CollectData();

                // Expire value list
                obj.ExpireSolution(true);

                //Return that it's created
                return true;
            }
            catch
            {
                //Return that it isn't created
                return false;
            }
        }

        /// <summary>
        /// Return a value list populated with data from an enum.
        /// </summary>
        /// <param name="enumType"> Enumeration to take values from. </param>
        /// <returns> The value list with data. </returns>
        private static GH_ValueList CreateValueList(Type enumType)
        {
            // Creates the empty value list
            GH_ValueList obj = new GH_ValueList();
            obj.CreateAttributes();
            obj.ListMode = GH_ValueListMode.DropDown;
            obj.ListItems.Clear();

            // Add the items to the value list
            string[] names = Enum.GetNames(enumType);
            int[] values = (int[])Enum.GetValues(enumType);

            for (int i = 0; i < names.Length; i++)
            {
                obj.ListItems.Add(new GH_ValueListItem(names[i], values[i].ToString()));
            }

            return obj;
        }

        /// <summary>
        /// Places a value list on a given location on the canvas. 
        /// Returns true if it's created.
        /// </summary>
        /// <param name="obj"> Value list to place on the canvas. </param>
        /// <param name="location"> Location on the canvas. </param>
        /// <returns> True, if created. </returns>
        private static bool CreateValueList(GH_ValueList obj, PointF location)
        {
            try
            {
                obj.Attributes.Pivot = new PointF(location.X - (obj.Attributes.Bounds.Width / 4), location.Y - (obj.Attributes.Bounds.Height / 2));
                Instances.ActiveCanvas.Document.AddObject(obj, false);
                obj.Attributes.ExpireLayout();
                obj.Attributes.PerformLayout();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a value list populated with data from a dictionary.
        /// </summary>
        /// <param name="data"> Data to populate the valuelist with. </param>
        /// <returns> The value list with data. </returns>
        private static GH_ValueList CreateValueList(Dictionary<string, double> data)
        {
            // Creates the empty value list
            GH_ValueList obj = new GH_ValueList();
            obj.CreateAttributes();
            obj.ListMode = GH_ValueListMode.DropDown;
            obj.ListItems.Clear();

            // Add the items to the value list
            foreach (KeyValuePair<string, double> entry in data)
            {
                obj.ListItems.Add(new GH_ValueListItem(entry.Key, entry.Value.ToString()));
            }

            return obj;
        }

        /// <summary>
        /// Creates a Grasshopper value list from an enum and places it on the given location on the canvas. 
        /// Returns true if it's created.
        /// </summary>
        /// <param name="enumType"> Enumeration to take values from. </param>
        /// <param name="location"> Location on the canvas. </param>
        /// <returns> True, if created. </returns>
        public static bool CreateValueList(Type enumType, PointF location)
        {
            return CreateValueList(CreateValueList(enumType), location);
        }

        /// <summary>
        /// Creates a Grasshopper value list from a dictionary and places it on the given location on the canvas.
        /// Returns true if it's created.
        /// </summary>
        /// <param name="data"> Data to populate the valuelist with. </param>
        /// <param name="location"> Location on the canvas. </param>
        /// <returns> True, if created. </returns>
        public static bool CreateValueList(Dictionary<string, double> data, PointF location)
        {
            return CreateValueList(CreateValueList(data), location);
        }

        /// <summary>
        /// Creates a Grasshopper value list from a list with strings and places it on the given
        /// location on the canvas. Returns true if it's created.
        /// </summary>
        /// <param name="names"> List with strings. </param>
        /// <param name="location"> Location on the canvas. </param>
        /// <returns> True, if created. </returns>
        public static bool CreateValueList(List<string> names, PointF location)
        {
            return CreateValueList(CreateValueList(names), location);
        }

        /// <summary>
        /// If GH's "Draw Full Names" canvas preference is currently on, sets the given parameter's
        /// NickName to its Name. Meant to be called right after constructing a parameter at
        /// runtime -- i.e. anywhere a component registers a new IGH_Param outside of its own
        /// RegisterInputParams/RegisterOutputParams (a menu/zui-added optional parameter, or a
        /// fresh component built by an IGH_UpgradeObject.Upgrade()).
        /// </summary>
        /// <remarks>
        /// "Draw Full Names" is not a live rendering switch -- toggling it calls
        /// GH_Document.ConvertNickNamesToFullNames()/ConvertFullNamesToNickNames() once, which
        /// walks every object's parameters as of that moment and copies Name into NickName (or
        /// back). A parameter that gets added later never goes through that pass and is
        /// permanently stuck showing its short NickName, even while the preference stays on --
        /// this reproduces that same one-time copy for a parameter that didn't exist yet, at the
        /// one point (construction) where doing so unconditionally can never overwrite a name the
        /// user customized by hand, since there's nothing to customize yet.
        /// </remarks>
        /// <param name="param"> The freshly constructed parameter. </param>
        public static void ApplyFullNamesPreference(IGH_Param param)
        {
            if (CentralSettings.CanvasFullNames)
            {
                param.NickName = param.Name;
            }
        }

        /// <summary>
        /// Applies <see cref="ApplyFullNamesPreference(IGH_Param)"/> to every current input and
        /// output parameter of the given component. Meant to be called once, right after building
        /// a brand new component instance from an IGH_UpgradeObject.Upgrade() -- every one of its
        /// parameters is freshly constructed (via its own RegisterInputParams/RegisterOutputParams
        /// and, where applicable, whatever ConfigureForUpgrade(...) hook put it into the right
        /// optional-parameter mode before wire migration), so there's nothing to protect against
        /// overwriting there either.
        /// </summary>
        /// <param name="component"> The freshly constructed replacement component. </param>
        public static void ApplyFullNamesPreference(IGH_Component component)
        {
            if (!CentralSettings.CanvasFullNames) { return; }

            for (int i = 0; i < component.Params.Input.Count; i++)
            {
                component.Params.Input[i].NickName = component.Params.Input[i].Name;
            }
            for (int i = 0; i < component.Params.Output.Count; i++)
            {
                component.Params.Output[i].NickName = component.Params.Output[i].Name;
            }
        }
        #endregion

        #region properties
        #endregion
    }
}