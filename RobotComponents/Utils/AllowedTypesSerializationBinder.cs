// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2025 Arjen Deetman
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.Runtime.Serialization;

namespace RobotComponents.Utils
{
    /// <summary>
    /// A restricted SerializationBinder that only allows deserialization of known,
    /// trusted types. This mitigates CWE-502 (deserialization of untrusted data)
    /// by blocking instantiation of arbitrary types during BinaryFormatter.Deserialize.
    /// </summary>
    internal sealed class AllowedTypesSerializationBinder : SerializationBinder
    {
        // Namespace prefixes for types that are allowed to be deserialized.
        private static readonly string[] _allowedNamespacePrefixes = new string[]
        {
            "RobotComponents.",       // All RobotComponents domain types
            "Rhino.Geometry.",        // RhinoCommon geometry types (Mesh, Plane, Point3d, etc.)
        };

        // Fully-qualified type names for System types used in serialization.
        private static readonly string[] _allowedSystemTypes = new string[]
        {
            "System.String",
            "System.Double",
            "System.Int32",
            "System.Int64",
            "System.Boolean",
            "System.Byte",
            "System.Version",
            "System.Guid",
            "System.DateTime",
            "System.Drawing.Color",
        };

        // Prefixes for generic collection types and arrays that may appear
        // in serialized object graphs.
        private static readonly string[] _allowedGenericPrefixes = new string[]
        {
            "System.Collections.Generic.List`1",
            "System.Collections.Generic.Dictionary`2",
        };

        /// <summary>
        /// Controls the binding of a serialized object to a type.
        /// Returns null to allow default resolution for permitted types,
        /// or throws SerializationException for disallowed types.
        /// </summary>
        /// <param name="assemblyName"> The assembly name of the serialized object. </param>
        /// <param name="typeName"> The full type name of the serialized object. </param>
        /// <returns> Null to use default type resolution. </returns>
        /// <exception cref="SerializationException"> Thrown when the type is not in the allowlist. </exception>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (IsAllowed(typeName))
            {
                return null; // Default resolution for allowed types
            }

            throw new SerializationException(
                $"Deserialization of type '{typeName}' from assembly '{assemblyName}' " +
                $"is not allowed. Only known RobotComponents and Rhino.Geometry types " +
                $"are permitted.");
        }

        private static bool IsAllowed(string typeName)
        {
            // Check domain namespace prefixes (RobotComponents.*, Rhino.Geometry.*)
            for (int i = 0; i < _allowedNamespacePrefixes.Length; i++)
            {
                if (typeName.StartsWith(_allowedNamespacePrefixes[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            // Check explicit System type allowlist
            for (int i = 0; i < _allowedSystemTypes.Length; i++)
            {
                if (typeName == _allowedSystemTypes[i])
                {
                    return true;
                }
            }

            // Check array types — element type must itself be allowed
            if (typeName.EndsWith("[]", StringComparison.Ordinal))
            {
                string elementType = typeName.Substring(0, typeName.Length - 2);
                return IsAllowed(elementType);
            }

            // Check generic collections — the full typeName includes the type arguments,
            // so we verify the outer generic is allowed and that the inner arguments
            // reference only allowed namespaces.
            for (int i = 0; i < _allowedGenericPrefixes.Length; i++)
            {
                if (typeName.StartsWith(_allowedGenericPrefixes[i], StringComparison.Ordinal))
                {
                    return ContainsOnlyAllowedTypeArguments(typeName);
                }
            }

            return false;
        }

        private static bool ContainsOnlyAllowedTypeArguments(string typeName)
        {
            // BinaryFormatter encodes generics like:
            //   System.Collections.Generic.List`1[[Namespace.Type, Assembly, ...]]
            // Verify all embedded type references are in allowed namespaces.
            int bracketStart = typeName.IndexOf("[[", StringComparison.Ordinal);
            if (bracketStart < 0)
            {
                return false;
            }

            string args = typeName.Substring(bracketStart);

            for (int i = 0; i < _allowedNamespacePrefixes.Length; i++)
            {
                if (args.Contains(_allowedNamespacePrefixes[i]))
                {
                    return true;
                }
            }

            for (int i = 0; i < _allowedSystemTypes.Length; i++)
            {
                if (args.Contains(_allowedSystemTypes[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
