// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// Copyright (c) 2023-2024 Arjen Deetman
//
// Authors:
//   - Arjen Deetman (2023-2024)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.Collections.Generic;
// Eto libs
using Eto.Forms;
using Eto.Drawing;

namespace RobotComponents.ABB.Controllers.Forms
{
    /// <summary>
    /// Represents the pick task form class.
    /// </summary>
    public class PickTaskForm : Dialog<bool>
    {
        #region fields
        private readonly Controller _controller = new Controller();
        private string _taskName = "-";
        private bool _allTasksSelected = false;
        private readonly List<string> _taskNames;
        private readonly List<string> _boxItems;
        private readonly Label _labelName = new Label() { Text = "-", TextAlignment = TextAlignment.Right, Height = _height };
        private readonly Label _labelType = new Label() { Text = "-", TextAlignment = TextAlignment.Right, Height = _height };
        private readonly Label _labelEnabled = new Label() { Text = "-", TextAlignment = TextAlignment.Right, Height = _height };
        private readonly ComboBox _box = new ComboBox() { Height = _height };
        private readonly bool _allowAllTasksOption;

        private const int _height = 21;
        private const string _allTasksItem = "All Tasks";
        #endregion

        #region constructors
        /// <summary>
        /// Constructs the form.
        /// </summary>
        /// <param name="controller"> The controller to pick a task from. </param>
        /// <param name="allowAllTasksOption">
        /// If true, adds an "All Tasks" entry at the bottom of the task list, after every real
        /// task. Picking it sets <see cref="AllTasksSelected"/>; <see cref="TaskName"/> still
        /// returns the first real task on the controller in that case, for callers that need a
        /// concrete single task regardless (e.g. for validation, or anything that can only ever
        /// target one task).
        /// </param>
        public PickTaskForm(Controller controller, bool allowAllTasksOption = false)
        {
            // Main layout
            Title = "Controller task";
            MinimumSize = new Size(600, 420);
            Resizable = false;
            Padding = 20;

            // Task names
            _controller = controller;
            _taskNames = _controller.TasksABB.ConvertAll(item => item.Name);
            _allowAllTasksOption = allowAllTasksOption;

            // The combo box shows every real task name followed by "All Tasks" (if allowed);
            // _boxItems is index-aligned with the combo box, _taskNames stays just the real names,
            // so a real task's index is identical in both lists.
            _boxItems = new List<string>(_taskNames);
            if (_allowAllTasksOption) { _boxItems.Add(_allTasksItem); }

            // Controls
            Button button = new Button() { Text = "OK" };
            _box = new ComboBox() { DataStore = _boxItems, Height = _height };

            // Assign events
            button.Click += ButtonClick;
            _box.SelectedIndexChanged += IndexChanged;

            // Select the first real task by default, not "All Tasks", even when it's available:
            // opting into every task should take a deliberate pick, not be the default.
            _box.SelectedIndex = 0;

            // Labels
            Label selectLabel = new Label() { Text = "Select a task", Font = new Font(SystemFont.Bold), Height = _height };
            Label infoLabel = new Label() { Text = "Task info", Font = new Font(SystemFont.Bold), Height = _height };

            // Layout
            DynamicLayout layout = new DynamicLayout() { Padding = 0, Spacing = new Size(8, 4) };
            layout.AddSeparateRow(selectLabel);
            layout.AddSeparateRow(_box);
            layout.AddSeparateRow(new Label() { Text = " ", Height = _height });
            layout.AddSeparateRow(infoLabel);
            layout.AddSeparateRow(new Label() { Text = "Name", Height = _height }, _labelName);
            layout.AddSeparateRow(new Label() { Text = "Type", Height = _height }, _labelType);
            layout.AddSeparateRow(new Label() { Text = "Enabled", Height = _height }, _labelEnabled);
            layout.AddSeparateRow(new Label() { Text = " ", Height = _height });
            layout.AddSeparateRow(new Label() { Text = " ", Height = _height });
            layout.AddSeparateRow(new Label() { Text = " ", Height = _height });
            layout.AddSeparateRow(new Label() { Text = " ", Height = _height });
            layout.AddSeparateRow(new Label() { Text = " ", Height = _height });
            layout.AddSeparateRow(button);

            Content = layout;
        }
        #endregion

        #region methods
        /// <summary>
        /// True while the combo box's current selection is the "All Tasks" entry (only ever
        /// possible when this form was constructed with allowAllTasksOption true).
        /// </summary>
        private bool IsAllTasksIndex(int index)
        {
            return _allowAllTasksOption && index == _taskNames.Count;
        }

        private void IndexChanged(object sender, EventArgs e)
        {
            if (IsAllTasksIndex(_box.SelectedIndex))
            {
                _labelName.Text = _allTasksItem;
                _labelType.Text = "-";
                _labelEnabled.Text = "-";
                return;
            }

            int taskIndex = _box.SelectedIndex;
            _labelName.Text = _controller.TasksABB[taskIndex].Name;
            _labelType.Text = _controller.TasksABB[taskIndex].Type.ToString();
            _labelEnabled.Text = _controller.TasksABB[taskIndex].Enabled.ToString();
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            if (IsAllTasksIndex(_box.SelectedIndex))
            {
                _allTasksSelected = true;
                _taskName = _taskNames.Count > 0 ? _taskNames[0] : "-";
            }
            else
            {
                _allTasksSelected = false;
                _taskName = _taskNames[_box.SelectedIndex];
            }

            Close(true);
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets the picked task name. When "All Tasks" was picked, this is the first real task on
        /// the controller -- callers that need a single concrete task regardless (e.g. for
        /// validation, or anything that can only ever target one task) can still use it as-is;
        /// callers that support uploading to every task should check <see cref="AllTasksSelected"/>
        /// first instead.
        /// </summary>
        public string TaskName
        {
            get { return _taskName; }
        }

        /// <summary>
        /// Gets whether the "All Tasks" entry was picked. Always false when this form was
        /// constructed with allowAllTasksOption left false.
        /// </summary>
        public bool AllTasksSelected
        {
            get { return _allTasksSelected; }
        }
        #endregion
    }
}
