// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2025-2026 Arjen Deetman
//
// Authors:
//   - Arjen Deetman (2025-2026)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.IO;
using System.Text.RegularExpressions;
// Robot Components Libs
using RobotComponents.ABB.Presets.Enumerations;

namespace RobotComponents.ABB.Presets.Utils
{
    /// <summary>
    /// Helper methods and properties
    /// </summary>
    internal static class HelperMethods
    {
        #region fields
        #endregion

        #region methods
        /// <summary>
        /// Returns the Robot preset name as a human readable string.
        /// </summary>
        /// <param name="preset"> The enum of the robot preset. </param>
        /// <returns> The name of the robot preset. </returns>
        internal static string GetRobotNameFromPresetName(RobotPreset preset)
        {
            // Handle special cases first
            if (preset == RobotPreset.IRB1010_1_5_037)
            {
                return "IRB1010-1.5/0.37";
            }
            // Start from the enum name
            string name = Enum.GetName(typeof(RobotPreset), preset);

            // Apply replacements
            name = Regex.Replace(name, @"^([^_]+)_(\d+)_", "$1-$2/");   // IRB1100_4_0475 → IRB1100-4/0475
            name = name.Replace("_LID", "-LID");                        // Handle suffix

            // Find the index of '/'
            int slashIndex = name.IndexOf('/');

            if (slashIndex != -1 && slashIndex + 1 < name.Length)
            {
                // Insert '.' after the character following '/'
                name = name.Insert(slashIndex + 2, ".");
            }

            return name;
        }

        /// <summary>
        /// Generates a valid C# class name from a human-readable robot name.
        /// </summary>
        /// <param name="name">The human-readable robot name (e.g. "IRB5720-155/2.6-LID").</param>
        /// <returns> A sanitized class name string </returns>
        internal static string GetRobotClassNameFromName(string name)
        {
            // Handle special cases first
            if (name == "IRB1010-1.5/0.37")
            {
                return "IRB1010_1_5_037";
            }

            // keep a working copy
            string className = name;

            // Normalize some separators first
            className = className.Replace('-', '_').Replace('/', '_');

            // Pad single digit after a dot (e.g. ".2" -> ".20")
            className = Regex.Replace(className, @"\.(\d)(?!\d)", m => "." + m.Groups[1].Value + "0");

            // Remove dots now
            className = className.Replace(".", "");

            return className;
        }
        /// <summary>
        /// Checks whether a file path is safe to interpolate into a RhinoApp.RunScript command.
        /// </summary>
        /// <remarks>
        /// Rejects paths that contain double quotes (which would break out of the quoted argument
        /// in a RunScript command string) or characters from <see cref="Path.GetInvalidPathChars"/>.
        /// </remarks>
        /// <param name="filePath"> The file path to validate. </param>
        /// <returns> True if the path is safe; false otherwise. </returns>
        internal static bool IsSafeFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (filePath.IndexOf('"') >= 0)
            {
                return false;
            }

            char[] invalidChars = Path.GetInvalidPathChars();

            if (filePath.IndexOfAny(invalidChars) >= 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the file path is not safe for use
        /// in a RhinoApp.RunScript command.
        /// </summary>
        /// <param name="filePath"> The file path to validate. </param>
        /// <exception cref="ArgumentException"> Thrown when the path contains unsafe characters. </exception>
        internal static void ThrowIfUnsafeFilePath(string filePath)
        {
            if (!IsSafeFilePath(filePath))
            {
                throw new ArgumentException(
                    $"The file path '{filePath}' contains characters that are unsafe for script execution.");
            }
        }
        #endregion

        #region properties
        #endregion
    }
}