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

// Grasshopper Libs
using GH_IO;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Declarations;

namespace RobotComponents.ABB.Gh.Goos.Definitions
{
    /// <summary>
    /// GH_RAPIDVariable Goo wrapper class, makes sure the RAPIDVariable class can be used in Grasshopper.
    /// </summary>
    public class GH_RAPIDVariable : GH_Goo<RAPIDVariable>, GH_ISerializable
    {
        #region constructors
        /// <summary>
        /// Blank constructor.
        /// </summary>
        public GH_RAPIDVariable()
        {
            Value = null;
        }

        /// <summary>
        /// Data constructor: Creates a GH_RAPIDVariable instance from a RAPIDVariable instance.
        /// </summary>
        /// <param name="variable"> RAPIDVariable value to store inside this Goo instance. </param>
        public GH_RAPIDVariable(RAPIDVariable variable)
        {
            Value = variable;
        }

        /// <summary>
        /// Data constructor: Creates a GH_RAPIDVariable instance from another GH_RAPIDVariable instance.
        /// This creates a shallow copy of the passed GH_RAPIDVariable instance.
        /// </summary>
        /// <param name="variableGoo"> GH_RAPIDVariable Goo instance to copy. </param>
        public GH_RAPIDVariable(GH_RAPIDVariable variableGoo)
        {
            if (variableGoo == null)
            {
                variableGoo = new GH_RAPIDVariable();
            }

            Value = variableGoo.Value;
        }

        /// <summary>
        /// Make a complete duplicate of this Goo instance. No shallow copies.
        /// </summary>
        /// <returns> A duplicate of the GH_RAPIDVariable. </returns>
        public override IGH_Goo Duplicate()
        {
            return new GH_RAPIDVariable(Value == null ? new RAPIDVariable() : Value.Duplicate());
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
                if (Value == null) { return "No internal RAPIDVariable instance"; }
                if (Value.IsValid) { return string.Empty; }
                return "Invalid RAPID Variable: Type and Name must not be empty.";
            }
        }

        /// <summary>
        /// Creates a string description of the current instance value.
        /// </summary>
        /// <returns> String description. </returns>
        public override string ToString()
        {
            if (Value == null) { return "Null RAPID Variable"; }
            else { return Value.ToString(); }
        }

        /// <summary>
        /// Gets the name of the type of the implementation.
        /// </summary>
        public override string TypeName
        {
            get { return "RAPID Variable"; }
        }

        /// <summary>
        /// Gets a description of the type of the implementation.
        /// </summary>
        public override string TypeDescription
        {
            get { return "Defines a RAPID variable declaration (VAR, PERS, or INOUT)."; }
        }
        #endregion

        #region casting methods
        /// <summary>
        /// Attempt a cast to type Q.
        /// </summary>
        /// <typeparam name="Q"> Type to cast to. </typeparam>
        /// <param name="target"> Pointer to target of cast. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool CastTo<Q>(ref Q target)
        {
            // Cast to GH_RAPIDVariable
            if (typeof(Q).IsAssignableFrom(typeof(GH_RAPIDVariable)))
            {
                if (Value == null) { target = (Q)(object)new GH_RAPIDVariable(); }
                else { target = (Q)(object)new GH_RAPIDVariable(Value); }
                return true;
            }

            // Cast to RAPIDVariable
            if (typeof(Q).IsAssignableFrom(typeof(RAPIDVariable)))
            {
                if (Value == null) { target = (Q)(object)null; }
                else { target = (Q)(object)Value; }
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

            // Cast from RAPIDVariable
            if (typeof(RAPIDVariable).IsAssignableFrom(source.GetType()))
            {
                Value = source as RAPIDVariable;
                return true;
            }

            return false;
        }
        #endregion

        #region (de)serialization
        /// <summary>
        /// IO key for (de)serialization of the value inside this Goo.
        /// </summary>
        private const string IoKey = "RAPIDVariable";

        /// <summary>
        /// This method is called whenever the instance is required to serialize itself.
        /// </summary>
        /// <param name="writer"> Writer object to serialize with. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Write(GH_IWriter writer)
        {
            if (Value != null)
            {
                byte[] array = RobotComponents.Utils.Serialization.ObjectToByteArray(Value);
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
                Value = null;
                return true;
            }

            byte[] array = reader.GetByteArray(IoKey);
            Value = (RAPIDVariable)RobotComponents.Utils.Serialization.ByteArrayToObject(array);

            return true;
        }
        #endregion
    }
}
