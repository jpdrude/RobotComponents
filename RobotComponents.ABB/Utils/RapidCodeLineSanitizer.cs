// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RobotComponents.ABB.Utils
{
    /// <summary>
    /// Provides sanitization for user-supplied RAPID code lines to prevent
    /// injection of structural keywords that could break the generated module.
    /// </summary>
    public static class RapidCodeLineSanitizer
    {
        /// <summary>
        /// RAPID structural keywords that open or close blocks. If injected into
        /// a CodeLine they can break the MODULE / PROC structure produced by
        /// <see cref="RobotComponents.ABB.Actions.RAPIDGenerator"/>.
        /// Checked case-insensitively at word boundaries.
        /// </summary>
        private static readonly string[] _structuralKeywords = new string[]
        {
            // Block closers
            "ENDPROC", "ENDMODULE", "ENDFUNC", "ENDTRAP", "ENDRECORD",
            // Block openers
            "PROC", "MODULE", "FUNC", "TRAP", "RECORD"
        };

        /// <summary>
        /// Sanitizes a user-provided RAPID code string.
        /// </summary>
        /// <param name="code"> The raw code string. </param>
        /// <returns>
        /// A <see cref="SanitizeResult"/> containing the cleaned code and any warnings.
        /// </returns>
        public static SanitizeResult Sanitize(string code)
        {
            List<string> warnings = new List<string>();

            if (string.IsNullOrEmpty(code))
            {
                return new SanitizeResult(code, warnings);
            }

            // Strip newlines — a CodeLine must be a single line
            if (code.Contains("\r") || code.Contains("\n"))
            {
                code = code.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
                warnings.Add("Newlines were removed from the code line. Each CodeLine should be a single RAPID statement.");
            }

            // Detect structural keywords that could break the RAPID module structure
            for (int i = 0; i < _structuralKeywords.Length; i++)
            {
                string keyword = _structuralKeywords[i];
                string pattern = @"\b" + keyword + @"\b";

                if (Regex.IsMatch(code, pattern))
                {
                    warnings.Add($"Code line contains the structural RAPID keyword \"{keyword}\" which could break the generated module structure.");
                }
            }

            return new SanitizeResult(code, warnings);
        }

        /// <summary>
        /// Contains the result of sanitizing a RAPID code line.
        /// </summary>
        public class SanitizeResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SanitizeResult"/> class.
            /// </summary>
            /// <param name="code"> The sanitized code string. </param>
            /// <param name="warnings"> The list of warning messages. </param>
            public SanitizeResult(string code, List<string> warnings)
            {
                Code = code;
                Warnings = warnings;
            }

            /// <summary>
            /// Gets the sanitized code string.
            /// </summary>
            public string Code { get; }

            /// <summary>
            /// Gets the list of warning messages generated during sanitization.
            /// </summary>
            public List<string> Warnings { get; }

            /// <summary>
            /// Gets a value indicating whether any warnings were generated.
            /// </summary>
            public bool HasWarnings
            {
                get { return Warnings.Count > 0; }
            }
        }
    }
}
