// This file is part of Robot Components. Robot Components is licensed 
// under the terms of GNU General Public License version 3.0 (GPL v3.0)
// as published by the Free Software Foundation. For more information and 
// the LICENSE file, see <https://github.com/RobotComponents/RobotComponents>.

// System Libs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
// Rhino Libs
using Rhino;
using Rhino.Geometry;
using Rhino.Runtime;
// Robot Components Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Kinematics.IkGeo.Geometry;

namespace RobotComponents.ABB.Kinematics.IkGeo
{
    /// <summary>
    /// Inverse kinematics solver for CRB15000 robots (GoFa) using the ik-geo native library.
    /// </summary>
    /// <remarks>
    /// - Wraps the native ik-geo implementation for CRB15000 (GoFa) robots by Alex Elias (https://github.com/rpiRobotics/ik-geo/tree/cpp). 
    /// - All positional inputs are interpreted in millimeters when using Rhino types in the code but converted to meters for the native solver.
    /// - The solver calls unmanaged code via DllImport; the native DLL("ikgeoInterface_GoFa.dll") and dependencies 
    /// ("libgcc_s_seh-1.dll", "libstdc++-6.dll", and "libwinpthread-1.dll") must be available at runtime.
    /// </remarks>
    public class IkGeoSolver
    {
        #region fields
        private int _numSolutions = 0;
        private List<RobotJointPosition> _robotJointPositions = new List<RobotJointPosition>();
        private List<RobotJointPosition> _robotJointPositionsArranged = new List<RobotJointPosition>();
        private readonly Plane _base = Plane.WorldYZ;
        private Robot _robot;

        // Constants
        private const double _pi = Math.PI;
        private const double _rad2deg = 180.0 / _pi;
        private const double _deg2rad = _pi / 180.0;

        // sentinel for missing IK solutions
        private const double _missingJointValue = 9e9;
        #endregion

        #region constructor
        /// <summary>
        /// Initializes a new instance of the IkGeo Solver class.
        /// </summary>
        /// <param name="robot">Robot for which to compute inverse kinematics.</param>
        public IkGeoSolver(Robot robot)
        {
            _robot = robot;
        }
        #endregion

        #region methods
        /// <summary>
        /// Resets the solver state, clearing previous solutions.
        /// </summary>
        private void Reset()
        {
            _numSolutions = 0;
            _robotJointPositions.Clear();
        }

        /// <summary>
        /// Computes inverse kinematics for CRB15000 (GoFa) robots for the given end effector plane.
        /// </summary>
        /// <param name="endPlane">Target end effector plane in world coordinates. The method converts Rhino units to meters for the native solver.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown when the process is not running as 64-bit and the native solver cannot be loaded.</exception>
        /// <exception cref="DllNotFoundException">Thrown when the ikgeo native DLL cannot be found at runtime.</exception>
        /// <exception cref="Exception">Thrown when no wrist intersection is found during singularity analysis.</exception>
        /// <exception cref="BadImageFormatException">Thrown if loaded into x86 environment.</exception>
        /// <remarks>
        /// This method:
        /// - Slightly perturbs targets aligned with world Z to avoid degeneracies.
        /// - Rotates the target by 90° about the tool X axis to match the native ik-geo reference frame.
        /// - Converts pose information to the native geometry types and calls the unmanaged solver.
        /// - Releases unmanaged memory after copying solutions.
        /// - After receiving solutions (in radians) it converts them to degrees and arranges them according to the robot's Cfx configuration.
        /// - Computes singularities for each solution based on Jacobian analysis and alignment angles.
        /// - Relies on an active Rhino document to determine unit conversion factors. Falls back to mm.
        /// </remarks>
        public void Compute_CRB15000(Plane endPlane)
        {
            // Check if system is 64-bit
            if (!System.Environment.Is64BitOperatingSystem)
            {
                throw new PlatformNotSupportedException("The IkGeo Kinematics Solver cannot be used. The operating system is not 64-bit.");
            }

            // Reset the solution
            Reset();

            Plane targetPlane = endPlane;

            //To meters conversion factor from Rhino Doc settings (Needs active Rhino Docs to run)
            double toMeters = 0.001;
            if (RhinoDoc.ActiveDoc != null)
                toMeters = RhinoMath.UnitScale(RhinoDoc.ActiveDoc.ModelUnitSystem, Rhino.UnitSystem.Meters);

            // Check if target z-axis is co-linear to world z-axis, rotate slightly if true (this is due to a bug in the native IkGeo implementation)
            if (Rhino.Geometry.Vector3d.VectorAngle(targetPlane.ZAxis, new Rhino.Geometry.Vector3d(0, 0, 1)) * _rad2deg < 0.01 ||
                Rhino.Geometry.Vector3d.VectorAngle(targetPlane.ZAxis, new Rhino.Geometry.Vector3d(0, 0, -1)) * _rad2deg < 0.01)
                targetPlane.Transform(Transform.Rotation(0.01 * _deg2rad, targetPlane.XAxis, targetPlane.Origin));

            // Target rotated 90 degrees (probably due to inconsistent end effector definition)
            targetPlane.Rotate(0.5 * _pi, targetPlane.XAxis, targetPlane.Origin);

            // Position (in meters!)
            IkGeo.Geometry.Vector3d position = new IkGeo.Geometry.Vector3d(targetPlane.Origin * toMeters);

            // Orientation as quaternion
            Rhino.Geometry.Quaternion quaternion = Rhino.Geometry.Quaternion.Rotation(_base, targetPlane);
            IkGeo.Geometry.Quaternion orientation = new IkGeo.Geometry.Quaternion(quaternion);

            //collect robot dimensions from planes
            double[] robot_dimensions = new double[7];
            robot_dimensions[0] = (_robot.InternalAxisPlanes[1].OriginZ - _robot.InternalAxisPlanes[0].OriginZ) * toMeters;
            robot_dimensions[1] = (_robot.InternalAxisPlanes[1].OriginX - _robot.InternalAxisPlanes[0].OriginX) * toMeters;
            robot_dimensions[2] = (_robot.InternalAxisPlanes[2].OriginZ - _robot.InternalAxisPlanes[1].OriginZ) * toMeters;
            robot_dimensions[3] = (_robot.InternalAxisPlanes[3].OriginZ - _robot.InternalAxisPlanes[2].OriginZ) * toMeters;
            robot_dimensions[4] = (_robot.InternalAxisPlanes[4].OriginX - _robot.InternalAxisPlanes[2].OriginX) * toMeters;
            robot_dimensions[5] = (_robot.InternalAxisPlanes[5].OriginZ - _robot.InternalAxisPlanes[4].OriginZ) * toMeters;
            robot_dimensions[6] = (_robot.InternalAxisPlanes[5].OriginX - _robot.InternalAxisPlanes[4].OriginX) * toMeters;

            RobotJointPosition robotJointPosition;

            // Call native IK solver, collect solutions and free memory
            IntPtr jointsPtr = computeIK_CRB15000(ref position, ref orientation, out _numSolutions, robot_dimensions);

            // Marshal solutions into managed structures and convert to RobotJointPosition
            if (jointsPtr != IntPtr.Zero && _numSolutions > 0)
            {
                int structSize = Marshal.SizeOf(typeof(Vector6d));
                for (int i = 0; i < _numSolutions; i++)
                {
                    IntPtr elemPtr = IntPtr.Add(jointsPtr, i * structSize);
                    Vector6d v = (Vector6d)Marshal.PtrToStructure(elemPtr, typeof(Vector6d));

                    robotJointPosition = v.ToRobotJointPosition();
                    robotJointPosition.Multiply(_rad2deg); // Radians to degrees

                    _robotJointPositions.Add(robotJointPosition);
                }
                //Free unmanaged memory
                freeIKMemory_CRB15000(jointsPtr);
            }

            //Arrange joint positions into Cfx ordering
            ArrangeJointPositions();
        }

        /// <summary>
        /// Arranges IK solutions into the 8-slot Cfx ordering used by RAPID.
        /// </summary>
        /// <remarks>
        /// Cfx is a 3-bit bitmask composed from Cf1 (shoulder side), Cf4 (wrist-center side) and Cf6 (sign of joint 5).
        /// This method:
        /// - Initializes an array of eight default positions (_missingJointValue = 9e9) and fills slots according to computed Cf1/Cf4/Cf6.
        /// - Uses ForwardKinematics to determine Cf bits by geometric tests.
        /// Missing solver solutions remain as the default sentinel values so callers can detect absent configurations.
        /// </remarks>
        private void ArrangeJointPositions()
        {
            // Cfx parameter defintion:
            //
            //Cfx is a bitmask of the axis related values Cf1, Cf4, and Cf6
            //
            //Cf1 relies on a line BT from the robot base to the TCP
            //      Cf1 = 0: the shoulder is on the right side of BT
            //      Cf1 = 1: the shoulder is on the left side of BT
            //
            //Cf4 relies in a line SE from the robots second to third joints     
            //      Cf4 = 0: the TCP is on the right side of SE
            //      Cf4 = 1: the TCP is on the left side of SE
            //
            //Cf6 relies on the angle of axis 5
            //      Cf6 = 0: angle5 > 0
            //      Cf6 = 1: angle5 < 0

            ForwardKinematics fk = new ForwardKinematics(_robot, true);

            //Millimeters to File Units
            double mm2FileUnits = 1;
            if (RhinoDoc.ActiveDoc != null)
                mm2FileUnits = RhinoMath.UnitScale(UnitSystem.Millimeters, RhinoDoc.ActiveDoc.ModelUnitSystem);

            // Initialize 8 robot joint positions with default values
            _robotJointPositionsArranged.Clear();

            for (int i = 0; i < 8; i++)
            {
                _robotJointPositionsArranged.Add(new RobotJointPosition(Enumerable.Repeat(_missingJointValue, 6).ToList()));
            }

            RobotJointPosition jointPos;

            for (int i = 0; i < NumSolutions; i++)
            {
                //initialize Cf1, Cf4, and Cf6
                int Cf1 = 0;
                int Cf4 = 0;
                int Cf6 = 0;

                //get posed robot
                jointPos = _robotJointPositions[i];
                fk.Calculate(jointPos);

                //determine value of Cf1
                Point3d robotBase = fk.PosedInternalAxisPlanes[0].Origin;
                Point3d TCP = fk.TCPPlane.Origin;
                Point3d shoulderPos = fk.PosedInternalAxisPlanes[1].Origin;
                //Move shoulder position outwards
                shoulderPos.Transform(Transform.Translation(fk.PosedInternalAxisPlanes[1].ZAxis * -100 * mm2FileUnits));
                //Project to 2D space
                robotBase.Z = 0;
                TCP.Z = 0;
                shoulderPos.Z = 0;

                //define related vectors
                Rhino.Geometry.Vector3d base2tcp = new Rhino.Geometry.Vector3d(TCP - robotBase);
                Rhino.Geometry.Vector3d base2shoulder = new Rhino.Geometry.Vector3d(shoulderPos - robotBase);

                //if cross product of these vectors is larger than 0 -> Cf1 = 1
                if (Rhino.Geometry.Vector3d.CrossProduct(base2tcp, base2shoulder).Z > 0)
                    Cf1 = 1;


                //determine value of Cf4
                shoulderPos = fk.PosedInternalAxisPlanes[1].Origin;
                Point3d elbowPos = fk.PosedInternalAxisPlanes[2].Origin;
                TCP = fk.TCPPlane.Origin;
                //Transform points into 2D space
                shoulderPos.Transform(Transform.PlaneToPlane(fk.PosedInternalAxisPlanes[1], Plane.WorldXY));
                elbowPos.Transform(Transform.PlaneToPlane(fk.PosedInternalAxisPlanes[1], Plane.WorldXY));
                TCP.Transform(Transform.PlaneToPlane(fk.PosedInternalAxisPlanes[1], Plane.WorldXY));
                shoulderPos.Z = 0;
                elbowPos.Z = 0;
                TCP.Z = 0;

                //define related vectors
                Rhino.Geometry.Vector3d shoulder2elbow = new Rhino.Geometry.Vector3d(elbowPos - shoulderPos);
                Rhino.Geometry.Vector3d shoulder2tcp = new Rhino.Geometry.Vector3d(TCP - shoulderPos);

                //if cross product of these vectors is smaller than 0 -> Cf1 = 1
                if (Rhino.Geometry.Vector3d.CrossProduct(shoulder2elbow, shoulder2tcp).Z < 0)
                    Cf4 = 1;

                //if Cf1: invert Cf4, due to plane orientations
                if (Cf1 == 1)
                    Cf4 = Math.Abs(Cf4 - 1);


                //determine value of Cf6
                if (jointPos[4] < 0)
                    Cf6 = 1;


                //define Cfx through bitshifting into a bitmask
                int Cfx = Cf1 << 2 | Cf4 << 1 | Cf6;

                //Sort result into correct array position in regards to Cfx
                _robotJointPositionsArranged[Cfx] = jointPos;
            }

        }
        #endregion

        #region properties
        /// <summary>
        /// Gets all calculated Robot Joint Positions.
        /// </summary>
        public List<RobotJointPosition> RobotJointPositions
        {
            get { return _robotJointPositionsArranged; }
        }

        /// <summary>
        /// Get number of computed inverse kinematics solutions.
        /// </summary>
        public int NumSolutions
        {
            get { return _numSolutions; }
        }
        #endregion

        #region DllImports
        /// <summary>
        /// Computes the inverse kinematics solutions for the CRB15000 robot given end-effector position and orientation.
        /// </summary>
        /// <remarks>
        /// This method is a P/Invoke wrapper for the native function
        /// <c>computeInverseKinematics_GoFa</c> in the "ikgeoInterface_GoFa.dll" library. Ensure that the native
        /// library is available and properly loaded at runtime. Make sure the following dependencies are available likewise:
        /// "libgcc_s_seh-1.dll", "libstdc++-6.dll", and "libwinpthread-1.dll".
        /// The caller is responsible for managing the memory of the returned pointer.
        /// </remarks>
        /// <param name="eePos">The position of the end-effector in 3D space, represented as a <see cref="IkGeo.Geometry.Vector3d"/>.</param>
        /// <param name="eeOri">The orientation of the end-effector, represented as a <see cref="IkGeo.Geometry.Quaternion"/>.</param>
        /// <param name="n_sol">Outputs the number of valid inverse kinematics solutions found.</param>
        /// <param name="robot_dimensions">An array of doubles representing the physical dimensions of the robot, used to calculate the solutions.</param>
        /// <returns>A pointer to an array of <see cref="Vector6d"/> structures, where each structure represents a valid joint
        /// configuration for the robot. Returns <c>null</c> if no solutions are found.</returns>
        [DllImport("ikgeoInterface_GoFa.dll", EntryPoint = "computeInverseKinematics_GoFa")]
        private static extern IntPtr computeIK_CRB15000(ref IkGeo.Geometry.Vector3d eePos, ref IkGeo.Geometry.Quaternion eeOri, out int n_sol, double[] robot_dimensions);

        /// <summary>
        /// Releases the memory allocated for inverse kinematics calculations.
        /// </summary>
        /// <remarks>
        /// This method is intended for use with the CRB15000 robot model and should only be
        /// called after the memory is no longer needed. Ensure that the pointer is valid and not used after this method
        /// is called.
        /// </remarks>
        /// <param name="ptr">A pointer to the memory block to be freed. This pointer must have been previously allocated by the inverse
        /// kinematics library. Passing an invalid or null pointer may result in undefined behavior.</param>
        [DllImport("ikgeoInterface_GoFa.dll", EntryPoint = "freeIKMemory")]
        private static extern void freeIKMemory_CRB15000(IntPtr ptr);
        #endregion
    }
}
