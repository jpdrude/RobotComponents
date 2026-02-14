// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.Utils;

namespace RobotComponents.Tests.Utils
{
    public class SerializationBinderTests
    {
        #region round-trip tests
        [Fact]
        public void RoundTrip_SpeedData_PreservesValues()
        {
            var original = new SpeedData(250, 500, 5000, 1000);

            byte[] bytes = Serialization.ObjectToByteArray(original);
            var result = (SpeedData)Serialization.ByteArrayToObject(bytes);

            Assert.Equal(original.Name, result.Name);
            Assert.Equal(original.V_TCP, result.V_TCP);
            Assert.Equal(original.V_ORI, result.V_ORI);
            Assert.Equal(original.V_LEAX, result.V_LEAX);
            Assert.Equal(original.V_REAX, result.V_REAX);
        }

        [Fact]
        public void RoundTrip_RobotJointPosition_PreservesValues()
        {
            var original = new RobotJointPosition(10, 20, 30, 40, 50, 60);

            byte[] bytes = Serialization.ObjectToByteArray(original);
            var result = (RobotJointPosition)Serialization.ByteArrayToObject(bytes);

            Assert.Equal(original[0], result[0]);
            Assert.Equal(original[1], result[1]);
            Assert.Equal(original[2], result[2]);
            Assert.Equal(original[3], result[3]);
            Assert.Equal(original[4], result[4]);
            Assert.Equal(original[5], result[5]);
        }

        [Fact]
        public void RoundTrip_RobotKinematicParameters_PreservesValues()
        {
            var original = new RobotKinematicParameters(150, 0, 0, 0, 680, 1142.5, 200, 0);

            byte[] bytes = Serialization.ObjectToByteArray(original);
            var result = (RobotKinematicParameters)Serialization.ByteArrayToObject(bytes);

            Assert.Equal(original.A1, result.A1);
            Assert.Equal(original.A2, result.A2);
            Assert.Equal(original.A3, result.A3);
            Assert.Equal(original.B, result.B);
            Assert.Equal(original.C1, result.C1);
            Assert.Equal(original.C2, result.C2);
            Assert.Equal(original.C3, result.C3);
            Assert.Equal(original.C4, result.C4);
        }

        [Fact]
        public void RoundTrip_ZoneData_PreservesValues()
        {
            var original = new ZoneData(5);

            byte[] bytes = Serialization.ObjectToByteArray(original);
            var result = (ZoneData)Serialization.ByteArrayToObject(bytes);

            Assert.Equal(original.Name, result.Name);
        }

        [Fact]
        public void RoundTrip_ExternalJointPosition_PreservesValues()
        {
            var original = new ExternalJointPosition(100, 200, 300, 400, 500, 600);

            byte[] bytes = Serialization.ObjectToByteArray(original);
            var result = (ExternalJointPosition)Serialization.ByteArrayToObject(bytes);

            Assert.Equal(original[0], result[0]);
            Assert.Equal(original[1], result[1]);
            Assert.Equal(original[2], result[2]);
            Assert.Equal(original[3], result[3]);
            Assert.Equal(original[4], result[4]);
            Assert.Equal(original[5], result[5]);
        }

        [Fact]
        public void RoundTrip_ConfigurationData_PreservesValues()
        {
            var original = new ConfigurationData(1, 0, -1, 0);

            byte[] bytes = Serialization.ObjectToByteArray(original);
            var result = (ConfigurationData)Serialization.ByteArrayToObject(bytes);

            Assert.Equal(original.Cf1, result.Cf1);
            Assert.Equal(original.Cf4, result.Cf4);
            Assert.Equal(original.Cf6, result.Cf6);
            Assert.Equal(original.Cfx, result.Cfx);
        }
        #endregion

        #region blocked type tests
        [Fact]
        public void ByteArrayToObject_BlocksDisallowedType()
        {
            // Serialize a System.Collections.Hashtable — a type NOT in the allowlist.
            // In a real attack, the payload would be a gadget type like
            // System.Runtime.Remoting.Channels.SoapServerFormatterSinkProvider.
            byte[] maliciousBytes;
            var hashtable = new System.Collections.Hashtable { { "key", "value" } };

            BinaryFormatter rawFormatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                rawFormatter.Serialize(stream, hashtable);
                maliciousBytes = stream.ToArray();
            }

            // Attempting to deserialize through the protected method should throw
            Assert.Throws<SerializationException>(() =>
                Serialization.ByteArrayToObject(maliciousBytes));
        }
        #endregion

        #region null and edge case tests
        [Fact]
        public void ByteArrayToObject_NullInput_ReturnsNull()
        {
            Assert.Null(Serialization.ByteArrayToObject(null));
        }

        [Fact]
        public void ObjectToByteArray_NullInput_ReturnsNull()
        {
            Assert.Null(Serialization.ObjectToByteArray(null));
        }
        #endregion
    }
}
