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
using System.Collections.Generic;
using System.Windows.Forms;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components
{
    /// <summary>
    /// Component abstract class.
    /// </summary>
    public abstract class GH_RobotComponent : GH_Component
    {
        #region constructors
        /// <summary>
        /// Empty constructor. 
        /// </summary>
        protected GH_RobotComponent() : base()
        {
        }

        /// <summary>
        /// Constructs a generic Robot Components component. 
        /// </summary>
        /// <param name="name"> Name of the component. </param>
        /// <param name="nickname"> Nickname of the component. </param>
        /// <param name="category"> Category in which this component belongs. </param>
        /// <param name="description"> Description of the component. </param>
        protected GH_RobotComponent(string name, string nickname, string category, string description) :
            base(name, nickname, description + System.Environment.NewLine + System.Environment.NewLine +
                "Robot Components: v" + RobotComponents.VersionNumbering.CurrentVersion, "Robot Components ABB", category)
        {
        }
        #endregion

        #region draw full names
        /// <summary>
        /// The (Name, NickName) pair for every parameter this component can add dynamically at
        /// runtime -- via the +/- zui, a right-click "add optional parameter" toggle, or any other
        /// IGH_VariableParameterComponent mechanism -- at the values each one starts out with when
        /// first created. Empty by default; only components that actually add parameters this way
        /// need to override it.
        /// </summary>
        /// <remarks>
        /// GH's own "Draw Full Names" canvas preference is not a live rendering switch: toggling it
        /// walks the document once and copies each parameter's Name into its NickName (or back),
        /// comparing every real object against a freshly constructed reference instance of the same
        /// type to skip any parameter the user has already renamed by hand. A parameter added later
        /// -- after that walk, or on a component reconstructed by an IGH_UpgradeObject.Upgrade() --
        /// was never part of it and is stuck showing its short NickName forever, since the reference
        /// instance never has it either (it doesn't exist until a user or an upgrader adds it).
        /// <para>
        /// This list is this project's own record of what those defaults were, so a piece of code
        /// completely separate from every individual component (see
        /// RobotComponentsPriority.OnCanvasFullNamesChanged) can safely apply the same "expand to
        /// full name" / "collapse back to nickname" treatment retroactively, to a parameter that's
        /// already sitting on the canvas: only a parameter whose current (Name, NickName) still
        /// exactly matches an entry here is touched, so a name the user customized by hand is left
        /// alone, the same guarantee GH's own conversion gives the parameters it does reach.
        /// </para>
        /// <para>
        /// Whichever component overrides this should build each entry from the exact same field(s)
        /// its own parameter-construction code uses for that parameter's Name/NickName -- one
        /// literal per parameter, read from both places -- rather than retyping the strings here,
        /// so this list can never quietly drift out of sync with what the component actually builds.
        /// </para>
        /// </remarks>
        public virtual IReadOnlyList<(string Name, string NickName)> OptionalParameterDefaults
            => Array.Empty<(string, string)>();
        #endregion

        #region menu item
        /// <summary>
        /// Adds the additional items to the context menu of the component. 
        /// </summary>
        /// <param name="menu"> The context menu of the component. </param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Documentation", MenuItemClickComponentDoc, Properties.Resources.WikiPage_MenuItem_Icon);
            //Menu_AppendSeparator(menu);
            //Menu_AppendItem(menu, "Support me on Ko-fi", MenuItemClickComponentKofi, Properties.Resources.kofi_symbol);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Documentation" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        internal void MenuItemClickComponentDoc(object sender, EventArgs e)
        {
            if (Documentation.ComponentWeblinks.TryGetValue(this.GetType(), out string url))
                Documentation.OpenBrowser(url);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Support me on Ko-fi" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        internal void MenuItemClickComponentKofi(object sender, EventArgs e)
        {
            string url = "https://ko-fi.com/arjendeetman";
            Documentation.OpenBrowser(url);
        }
        #endregion
    }
}

