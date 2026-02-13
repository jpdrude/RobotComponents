// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.Collections.Generic;
using System.Linq;
// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Presets.Enumerations;
using RobotComponents.ABB.Presets.Utils;

namespace RobotComponents.Tests
{
    public class PresetHelperTests
    {
        #region GetRobotNameFromPresetName
        [Fact]
        public void GetRobotNameFromPresetName_IRB1100_4_058_ProducesCorrectName()
        {
            string name = HelperMethods.GetRobotNameFromPresetName(RobotPreset.IRB1100_4_058);

            Assert.Equal("IRB1100-4/0.58", name);
        }

        [Fact]
        public void GetRobotNameFromPresetName_CRB15000_10_152_ProducesCorrectName()
        {
            string name = HelperMethods.GetRobotNameFromPresetName(RobotPreset.CRB15000_10_152);

            Assert.Equal("CRB15000-10/1.52", name);
        }

        [Fact]
        public void GetRobotNameFromPresetName_IRB1010_SpecialCase_ProducesCorrectName()
        {
            string name = HelperMethods.GetRobotNameFromPresetName(RobotPreset.IRB1010_1_5_037);

            Assert.Equal("IRB1010-1.5/0.37", name);
        }

        [Fact]
        public void GetRobotNameFromPresetName_LID_Suffix_ProducesCorrectName()
        {
            string name = HelperMethods.GetRobotNameFromPresetName(RobotPreset.IRB5720_155_260_LID);

            Assert.Equal("IRB5720-155/2.60-LID", name);
        }
        #endregion

        #region GetRobotClassNameFromName
        [Fact]
        public void GetRobotClassNameFromName_StandardName_ProducesCorrectClassName()
        {
            string className = HelperMethods.GetRobotClassNameFromName("IRB1100-4/0.58");

            Assert.Equal("IRB1100_4_058", className);
        }

        [Fact]
        public void GetRobotClassNameFromName_IRB1010_SpecialCase_ProducesCorrectClassName()
        {
            string className = HelperMethods.GetRobotClassNameFromName("IRB1010-1.5/0.37");

            Assert.Equal("IRB1010_1_5_037", className);
        }

        [Fact]
        public void GetRobotClassNameFromName_LID_Suffix_ProducesCorrectClassName()
        {
            string className = HelperMethods.GetRobotClassNameFromName("IRB5720-155/2.60-LID");

            Assert.Equal("IRB5720_155_260_LID", className);
        }

        [Fact]
        public void GetRobotClassNameFromName_SingleDigitReach_PadsCorrectly()
        {
            // e.g., "IRB5710-110/2.3" has single digit after dot -> should pad to "230"
            // Wait: .3 -> .30 -> remove dot -> 230. But the enum is IRB5710_110_230.
            string className = HelperMethods.GetRobotClassNameFromName("IRB5710-110/2.30");

            Assert.Equal("IRB5710_110_230", className);
        }
        #endregion

        #region Round-trip
        [Fact]
        public void RoundTrip_AllPresets_NameToClassNameMatchesEnumName()
        {
            List<RobotPreset> presets = Enum.GetValues(typeof(RobotPreset))
                .Cast<RobotPreset>()
                .Where(p => p != RobotPreset.EMPTY)
                .ToList();

            List<string> failures = new List<string>();

            for (int i = 0; i < presets.Count; i++)
            {
                string enumName = presets[i].ToString();
                string humanName = HelperMethods.GetRobotNameFromPresetName(presets[i]);
                string className = HelperMethods.GetRobotClassNameFromName(humanName);

                if (className != enumName)
                {
                    failures.Add($"{enumName} -> \"{humanName}\" -> \"{className}\" (expected \"{enumName}\")");
                }
            }

            Assert.True(failures.Count == 0,
                "Round-trip failures (enum -> name -> className != enum):\n" + string.Join("\n", failures));
        }
        #endregion
    }
}
