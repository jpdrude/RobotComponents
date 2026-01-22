// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2025 EDEK Uni Kassel
//
// Author:
//   - Jan Philipp Drude (2025)
//
// For license details, see the LICENSE file in the project root.

// Grasshopper Libs
using Grasshopper.Kernel.Types;
using GH_IO;
using GH_IO.Serialization;
// RobotComponents Libs
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Instructions;

namespace RobotComponents.ABB.Gh.Goos.Actions.Instructions
{
    /// <summary>
    /// Set Group Output Goo wrapper class, makes sure the Set Group Output class can be used in Grasshopper.
    /// </summary>
    public class GH_SetGroupOutput : GH_Goo<SetGroupOutput>, GH_ISerializable
    {
        #region constructors
        /// <summary>
        /// Blank constructor
        /// </summary>
        public GH_SetGroupOutput()
        {
            this.Value = null;
        }

        /// <summary>
        /// Data constructor: Creates a Set Group Output Goo instance from Group Ouput instance.
        /// </summary>
        /// <param name="groupOutput"> Set Group Output Value to store inside this Goo instance. </param>
        public GH_SetGroupOutput(SetGroupOutput groupOutput)
        {
            this.Value = groupOutput;
        }

        /// <summary>
        /// Data constructor: Creates a Set Group Output Goo instance from another Set Group Output Goo instance.
        /// This creates a shallow copy of the passed Group Output Goo instance. 
        /// </summary>
        /// <param name="groupOutputGoo"> Set Group Output Goo instance to copy. </param>
        public GH_SetGroupOutput(GH_SetGroupOutput groupOutputGoo)
        {
            if (groupOutputGoo == null)
            {
                groupOutputGoo = new GH_SetGroupOutput();
            }

            this.Value = groupOutputGoo.Value;
        }

        /// <summary>
        /// Make a complete duplicate of this Goo instance. No shallow copies.
        /// </summary>
        /// <returns> A duplicate of the Group Output Goo. </returns>
        public override IGH_Goo Duplicate()
        {
            return new GH_SetGroupOutput(Value == null ? new SetGroupOutput() : Value.Duplicate());
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets a value indicating whether or not the current value is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                if (Value == null) { return false; }
                return Value.IsValid;
            }
        }

        /// <summary>
        /// Gets a string describing the state of "invalidness". 
        /// If the instance is valid, then this property should return Nothing or string.Empty.
        /// </summary>
        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null) { return "No internal Set Group Output instance"; }
                if (Value.IsValid) { return string.Empty; }
                return "Invalid Set Group Output instance: Did you define the group output name and value?";
            }
        }

        /// <summary>
        /// Creates a string description of the current instance value.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Value == null) { return "Null Set Group Output"; }
            else { return Value.ToString(); }
        }

        /// <summary>
        /// Gets the name of the type of the implementation.
        /// </summary>
        public override string TypeName
        {
            get { return "Set Group Output"; }
        }

        /// <summary>
        /// Gets a description of the type of the implementation.
        /// </summary>
        public override string TypeDescription
        {
            get { return "Defines a Set Group Output"; }
        }
        #endregion

        #region casting methods
        /// <summary>
        /// Attempt a cast to type Q.
        /// </summary>
        /// <typeparam name="Q"> Type to cast to.  </typeparam>
        /// <param name="target"> Pointer to target of cast. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool CastTo<Q>(ref Q target)
        {
            //Cast to Set Group Output Goo
            if (typeof(Q).IsAssignableFrom(typeof(GH_SetGroupOutput)))
            {
                if (Value == null) { target = (Q)(object)new GH_SetGroupOutput(); }
                else { target = (Q)(object)new GH_SetGroupOutput(Value); }
                return true;
            }

            //Cast to Set Group Output
            if (typeof(Q).IsAssignableFrom(typeof(SetGroupOutput)))
            {
                if (Value == null) { target = (Q)(object)null; }
                else { target = (Q)(object)Value; }
                return true;
            }

            //Cast to Action Goo
            if (typeof(Q).IsAssignableFrom(typeof(GH_Action)))
            {
                if (Value == null) { target = (Q)(object)new GH_Action(); }
                else { target = (Q)(object)new GH_Action(Value); }
                return true;
            }

            //Cast to Action
            if (typeof(Q).IsAssignableFrom(typeof(IAction)))
            {
                if (Value == null) { target = (Q)(object)null; }
                else { target = (Q)(object)Value; }
                return true;
            }

            //Cast to Instruction Goo
            if (typeof(Q).IsAssignableFrom(typeof(GH_Instruction)))
            {
                if (Value == null) { target = (Q)(object)new GH_Instruction(); }
                else { target = (Q)(object)new GH_Instruction(Value); }
                return true;
            }


            //Cast to Instruction
            if (typeof(Q).IsAssignableFrom(typeof(IInstruction)))
            {
                if (Value == null) { target = (Q)(object)null; }
                else { target = (Q)(object)Value; }
                return true;
            }

            //Cast to Integer
            if (typeof(Q).IsAssignableFrom(typeof(GH_Integer)))
            {
                if (Value == null) { target = default; }
                else { target = (Q)(object)new GH_Integer(Value.Value); }
                return true;
            }

            //Cast to Group Output
            if (typeof(Q).IsAssignableFrom(typeof(SetGroupOutput)))
            {
                if (Value == null) { target = (Q)(object)null; }
                else { target = (Q)(object)new SetGroupOutput(Value.Name, Value.Value); }
                return true;
            }

            target = default;
            return false;
        }

        /// <summary>
        /// Attempt a cast from generic object.
        /// </summary>
        /// <param name="source"> Reference to source of cast. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool CastFrom(object source)
        {
            if (source == null) { return false; }

            //Cast from Set Group Output Goo
            if (typeof(GH_SetGroupOutput).IsAssignableFrom(source.GetType()))
            {
                GH_SetGroupOutput setGroupOutputGoo = source as GH_SetGroupOutput;
                Value = setGroupOutputGoo.Value;
                return true;
            }

            //Cast from Set Group Output
            if (typeof(SetGroupOutput).IsAssignableFrom(source.GetType()))
            {
                Value = source as SetGroupOutput;
                return true;
            }

            //Cast from Action
            if (typeof(IAction).IsAssignableFrom(source.GetType()))
            {
                if (source is SetGroupOutput action)
                {
                    Value = action;
                    return true;
                }
            }

            //Cast from Action Goo
            if (typeof(GH_Action).IsAssignableFrom(source.GetType()))
            {
                GH_Action actionGoo = source as GH_Action;
                if (actionGoo.Value is SetGroupOutput action)
                {
                    Value = action;
                    return true;
                }
            }

            //Cast from Instruction
            if (typeof(IInstruction).IsAssignableFrom(source.GetType()))
            {
                if (source is SetGroupOutput instruction)
                {
                    Value = instruction;
                    return true;
                }
            }

            //Cast from Instruction Goo
            if (typeof(GH_Instruction).IsAssignableFrom(source.GetType()))
            {
                GH_Instruction instructionGoo = source as GH_Instruction;
                if (instructionGoo.Value is SetGroupOutput instruction)
                {
                    Value = instruction;
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region (de)serialisation
        /// <summary>
        /// IO key for (de)serialisation of the value inside this Goo.
        /// </summary>
        private const string IoKey = "Set Group Output";

        /// <summary>
        /// This method is called whenever the instance is required to serialize itself.
        /// </summary>
        /// <param name="writer"> Writer object to serialize with. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Write(GH_IWriter writer)
        {
            if (this.Value != null)
            {
                byte[] array = RobotComponents.Utils.Serialization.ObjectToByteArray(this.Value);
                writer.SetByteArray(IoKey, array);
            }

            return true;
        }

        /// <summary>
        /// This method is called whenever the instance is required to deserialize itself.
        /// </summary>
        /// <param name="reader"> Reader object to deserialize from. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Read(GH_IReader reader)
        {
            if (!reader.ItemExists(IoKey))
            {
                this.Value = null;
                return true;
            }

            byte[] array = reader.GetByteArray(IoKey);
            this.Value = (SetGroupOutput)RobotComponents.Utils.Serialization.ByteArrayToObject(array);

            return true;
        }
        #endregion
    }
}
