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
using GH_IO;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
// RobotComponentsLibs
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Definitions;

namespace RobotComponents.ABB.Gh.Goos.Definitions
{
    /// <summary>
    /// RoutineArgument Goo wrapper class, makes sure the RoutineArgument class can be used in Grasshopper.
    /// </summary>
    public class GH_RoutineArgument : GH_Goo<RoutineArgument>, GH_ISerializable
    {
        #region constructors
        /// <summary>
        /// Blank constructor
        /// </summary>
        public GH_RoutineArgument()
        {
            Value = null;
        }

        /// <summary>
        /// Data constructor: Create a RoutineArgument Goo instance from a RoutineArgument instance.
        /// </summary>
        /// <param name="argument"> Argument Value to store inside this Goo instance. </param>
        public GH_RoutineArgument(RoutineArgument argument)
        {
            Value = argument;
        }

        /// <summary>
        /// Data constructor: Creates a RoutineArgument Goo instance from another RoutineArgument Goo instance.
        /// This creates a shallow copy of the passed RoutineArgument Goo instance. 
        /// </summary>
        /// <param name="argumentGoo"> RoutineArgument Goo instance to copy. </param>
        public GH_RoutineArgument(GH_RoutineArgument argumentGoo)
        {
            if (argumentGoo == null)
            {
                argumentGoo = new GH_RoutineArgument();
            }

            Value = argumentGoo.Value;
        }

        /// <summary>
        /// Make a complete duplicate of this Goo instance. No shallow copies.
        /// </summary>
        /// <returns> A duplicate of the RoutineArgumentGoo. </returns>
        public override IGH_Goo Duplicate()
        {
            return new GH_RoutineArgument(Value == null ? new RoutineArgument() : Value.Duplicate());
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
                if (Value == null) { return "No internal RoutineArgument instance"; }
                if (Value.IsValid) { return string.Empty; }
                return "Invalid Routine Argument";
            }
        }

        /// <summary>
        /// Creates a string description of the current instance value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Value == null) { return "Null Argument"; }
            else { return Value.ToString(); }
        }

        /// <summary>
        /// Gets the name of the type of the implementation.
        /// </summary>
        public override string TypeName
        {
            get { return "Routine Argument"; }
        }

        /// <summary>
        /// Gets a description of the type of the implementation.
        /// </summary>
        public override string TypeDescription
        {
            get { return "Defines a Routine Argument (type name = value)"; }
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
            //Cast to Method Goo
            if (typeof(Q).IsAssignableFrom(typeof(GH_RoutineArgument)))
            {
                if (Value == null) { target = (Q)(object)new GH_RoutineArgument(); }
                else { target = (Q)(object)new GH_RoutineArgument(Value); }
                return true;
            }

            //Cast to Method
            if (typeof(Q).IsAssignableFrom(typeof(RoutineArgument)))
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

            //Cast from RoutineArgument
            if (typeof(RoutineArgument).IsAssignableFrom(source.GetType()))
            {
                Value = source as RoutineArgument;
                return true;
            }

            return false;
        }
        #endregion

        #region (de)serialisation
        /// <summary>
        /// IO key for (de)serialisation of the value inside this Goo.
        /// </summary>
        private const string IoKey = "RoutineArgument";

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
            Value = (RoutineArgument)RobotComponents.Utils.Serialization.ByteArrayToObject(array);

            return true;
        }
        #endregion
    }
}
