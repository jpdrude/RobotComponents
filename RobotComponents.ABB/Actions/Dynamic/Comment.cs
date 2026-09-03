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
using System.Runtime.Serialization;
using System.Security.Permissions;
// RobotComponents Libs
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Actions.Dynamic
{
    /// <summary>
    /// Represents a comment in RAPID code.
    /// </summary>
    /// <remarks>
    /// This action is only used to make the program easier to understand. 
    /// It has no effect on the execution of the program.
    /// </remarks>
    [Serializable()]
    public class Comment : IAction, IDynamic, ISerializable
    {
        #region fields
        private string _comment;
        private CodeType _type;
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.  
        /// </summary>
        /// <param name="info"> The SerializationInfo to extract the data from. </param>
        /// <param name="context"> The context of this deserialization. </param>
        protected Comment(SerializationInfo info, StreamingContext context)
        {
            // // Version version = (int)info.GetValue("Version", typeof(Version)); // <-- use this if the (de)serialization changes
            _comment = StripNewlines((string)info.GetValue("Comment", typeof(string)));
            _type = (CodeType)info.GetValue("Code Type", typeof(CodeType));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the object.
        /// </summary>
        /// <param name="info"> The SerializationInfo to populate with data. </param>
        /// <param name="context"> The destination for this serialization. </param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.Version, typeof(Version));
            info.AddValue("Comment", _comment, typeof(string));
            info.AddValue("Code Type", _type, typeof(CodeType));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes an empty instance of the Comment class. 
        /// </summary>
        public Comment()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Comment class with the Code Type set as instruction.
        /// </summary>
        /// <param name="comment"> The comment. </param>
        public Comment(string comment)
        {
            _comment = StripNewlines(comment);
            _type = CodeType.Instruction;
        }

        /// <summary>
        /// Initializes a new instance of the Comment class.
        /// </summary>
        /// <param name="comment"> the comment. </param>
        /// <param name="type"> The Code Type. </param>
        public Comment(string comment, CodeType type)
        {
            _comment = StripNewlines(comment);
            _type = type;
        }

        /// <summary>
        /// Initializes a new instance of the Comment class by duplicating an existing Comment instance. 
        /// </summary>
        /// <param name="comment"> The Comment instance to duplicate. </param>
        public Comment(Comment comment)
        {
            _comment = StripNewlines(comment.Com);
            _type = comment.Type;
        }

        /// <summary>
        /// Returns an exact duplicate of this Comment instance.
        /// </summary>
        /// <returns> 
        /// A deep copy of the Comment instance. 
        /// </returns>
        public Comment Duplicate()
        {
            return new Comment(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Comment instance as IDynamic. 
        /// </summary>
        /// <returns> 
        /// A deep copy of the Comment instance as an IDynamic. 
        /// </returns>
        public IDynamic DuplicateDynamic()
        {
            return new Comment(this);
        }

        /// <summary>
        /// Returns an exact duplicate of this Comment instance as an Action. 
        /// </summary>
        /// <returns> 
        /// A deep copy of the Comment instance as an Action. 
        /// </returns>
        public IAction DuplicateAction()
        {
            return new Comment(this);
        }
        #endregion

        #region method
        /// <summary>
        /// Strips newline characters from the given string to prevent
        /// RAPID comment breakout injection.
        /// </summary>
        /// <param name="text"> The input text. </param>
        /// <returns> The text with newlines replaced by spaces, or null if input is null. </returns>
        private static string StripNewlines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (text.Contains("\r") || text.Contains("\n"))
            {
                return text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
            }

            return text;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns> 
        /// A string that represents the current object. 
        /// </returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(_comment))
            {
                return "Empty Comment";
            }
            else if (!IsValid)
            {
                return "Invalid Comment";
            }
            else
            {
                return "Comment";
            }
        }

        /// <summary>
        /// Returns the RAPID declaration code line of the this action.
        /// </summary>
        /// <param name="robot"> The Robot were the code is generated for. </param>
        /// <returns> 
        /// The RAPID code line. 
        /// </returns>
        public string ToRAPIDDeclaration(Robot robot)
        {
            return _type == CodeType.Declaration ? $"! {_comment}" : "";
        }

        /// <summary>
        /// Returns the RAPID instruction code line of the this action. 
        /// </summary>
        /// <param name="robot"> The Robot were the code is generated for. </param>
        /// <returns> 
        /// The RAPID code line. 
        /// </returns>
        public string ToRAPIDInstruction(Robot robot)
        {
            return _type == CodeType.Instruction ? $"! {_comment}" : "";
        }

        /// <summary>
        /// Creates declarations and instructions in the RAPID program module inside the RAPID Generator.
        /// </summary>
        /// <remarks>
        /// This method is called inside the RAPID generator.
        /// </remarks>
        /// <param name="RAPIDGenerator"> The RAPID Generator. </param>
        public void ToRAPIDGenerator(RAPIDGenerator RAPIDGenerator)
        {
            if (_type == CodeType.Declaration)
            {
                // Added to ProgramDeclarationCustomCodeLines (not ProgramDeclarations, and not
                // ProgramDeclarationComments): that's where a RAPID Variable's own declaration
                // (built as a declaration-type CodeLine) and CodeLineComponent's custom code lines
                // already land, in insertion order, under the "User defined code lines" section.
                // ProgramDeclarations is only for the implicit declarations Movement/Target objects
                // generate on their own (robtarget, speeddata, ...) and isn't where a user's own
                // RAPID Variable declarations end up, so a comment placed next to one of those in
                // the actions list needs to land in the same list to actually stay interleaved.
                if (_comment != "")
                {
                    RAPIDGenerator.ProgramDeclarationCustomCodeLines.Add("    " + $"! {_comment}");
                }
                else
                {
                    RAPIDGenerator.ProgramDeclarationCustomCodeLines.Add("    ");
                }
            }
            else if (_type == CodeType.Instruction)
            {
                if (_comment != "")
                {
                    RAPIDGenerator.ProgramInstructions.Add("    " + "    " + new string(' ', IndentationLevel * 4) + $"! {_comment}");
                }
                else
                {
                    RAPIDGenerator.ProgramInstructions.Add("    " + "    " + new string(' ', IndentationLevel * 4));
                }
            }
        }
        #endregion

        #region properties
        /// <summary>
        /// <inheritdoc/>
        public int IndentationLevel { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not the object is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (_comment == null) { return false; }
                if (_comment == "") { return false; }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the comment text.
        /// </summary>
        /// <remarks>
        /// Setting this property strips newlines to prevent RAPID comment breakout.
        /// </remarks>
        public string Com
        {
            get { return _comment; }
            set { _comment = StripNewlines(value); }
        }

        /// <summary>
        /// Gets or sets the comment Code Type.
        /// </summary>
        public CodeType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        #endregion
    }
}