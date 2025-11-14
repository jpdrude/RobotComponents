# Changelog

## All notable changes to this modified version of Robot Components are documented here.

### Changelog 
 Generated on: 2025-11-14 16:08 
 --- 
 - Updated documentation and acknowledgments in AUTHORS.md and README.md to reflect the modified version of the project. Added Jan Philipp Drude and Johannes Pfleging as contributors.Updated SPDX license headers across all modified files to acknowledge the original and modified projects. Updated copyright information to include "2025 EDEK Uni Kassel." 
 
 	 **Commit:** `5e1bf3d` | **Date:** 2025-11-14  
 
 --- 
 
 - Add CheckActionsComponent and automate release processIntroduced a new Grasshopper component, `CheckActionsComponent`, to validate robot actions and provide feedback. Added a corresponding icon (`CheckActions_Icon.png`) and localized resource for the UI. 
 - Updated the build process to include a `PostBuild` target in the project file, executing a new PowerShell script (`CreateRelease.ps1`) to automate release packaging and GitHub release creation. The script generates a zip archive, an `INSTALL.md` file, and uploads the release. 
 
 	 **Commit:** `faae010` | **Date:** 2025-11-14  
 
 --- 
 
 - Ommited to meters and to fileUnits conversion as Robot Components is entirely in mm, no matter the Rhino file units. 
 
 	 **Commit:** `c71dd21` | **Date:** 2025-11-11  
 
 --- 
 
 - IkGeo doesnt reveal solutions when target x = 0. Targets are therefore slightly offset for this case in the IkGeo solver compute method. 
 
 	 **Commit:** `61f9866` | **Date:** 2025-11-11  
 
 --- 
 
 - Added my contact to contributors. 
 
 	 **Commit:** `6a182ba` | **Date:** 2025-11-05  
 
 --- 
 
 - implements constructing configuration data from quadrant data inputs.Builds configuration data (cfx) from quarant data (cf1, cf4, cf6) as bitmask, if no cfx is provided. Also deconstructs cfx into quadrant data. 
 
 	 **Commit:** `67900ef` | **Date:** 2025-11-05  
 
 --- 
 
 - add singularity detection for CRB15000 robots using Jacobian analysisImplemented comprehensive singularity detection in the IkGeo solver for CRB15000 (GoFa) robots. Added fields for tracking wrist, elbow, shoulder singularities and missing solver results across all eight Cfx configurations. Introduced MathNet.Numerics dependency for Jacobian matrix operations. 
 - Implemented `ComputeSingularities` method to detect near-singular configurations using SVD-based Jacobian analysis with geometric alignment checks. Added `BuildJacobian` and `CheckJacobianSingularity` helper methods to construct the 6x6 manipulator Jacobian and evaluate singularity conditions using a relative tolerance threshold. 
 - Updated `InverseKinematics` class to retrieve and propagate singularity data from the IkGeo solver, including a sentinel value system (9e9) for missing solutions. Modified `CheckInternalAxisLimits` to skip validation for missing joint values and report when the solver returns no result. Added public properties `WristSingularities`, `ElbowSingularities`, `ShoulderSingularities`, and `NoSolverResults` to expose singularity information. 
 
 	 **Commit:** `3df7784` | **Date:** 2025-11-05  
 
 --- 
 
 - Fixed issues with Robot Components GH UI.Added the PosedInternalPlanes parameter to the variable output parameters array in the IK component. 
 - Added the CRB15000 robots to the enumeration of available robot presets. 
 
 	 **Commit:** `1b2e812` | **Date:** 2025-11-05  
 
 --- 
 
 - Add support for outputting posed planes in IK component (optional)Introduce a new feature to output posed planes in the 
 - `InverseKinematicsComponent` class. Added a `_outputPosedPlanesParameter` 
 - field to manage this functionality and updated the context menu with a 
 - new "Output Posed Axis Planes" option. 
 - Implemented the `MenuItemClickOutputPlanes` event handler to toggle the 
 - posed planes output and added serialization/deserialization support for 
 - this parameter in the `Write` and `Read` methods. 
 - Created the `GetPosedPlanesDataTree` method to transform posed axis 
 - planes into a structured data tree for output. Updated the `SolveInstance` 
 - method to populate the posed planes output parameter when enabled. 
 
 	 **Commit:** `0f14375` | **Date:** 2025-11-05  
 
 --- 
 
 - Implemented Axis Configuration sorting into IkGeoSolver and posed internal axis planes into InverseKinematics.Added `_posedInternalAxisPlanes` to `ForwardKinematics` to store posed internal axis planes and updated calculations to compute and expose these planes via a new public property. 
 - Introduced `_missingJointValue` sentinel in `IkGeoSolver` and implemented `ArrangeJointPositions` to sort IK solutions into RAPID's 8-slot Cfx ordering. This method uses forward kinematics and geometric tests to compute Cf1, Cf4, and Cf6 bitmask values. 
 
 	 **Commit:** `5d16dcf` | **Date:** 2025-11-05  
 
 --- 
 
 - Add IkGeoSolver for GoFa CRB15000 robots to InverseKinematcs classIntroduce IkGeoSolver to handle inverse kinematics for GoFa CRB15000 robots, while retaining OPW/Wrist Offset solvers for other robots. 
 - - Reorganize and update namespace imports, adding `IkGeoSolver`. 
 - - Update `CalculateRobotJointPosition` to use IkGeoSolver for CRB15000 robots. 
 - - Retain OPW/Wrist Offset solvers for other robot types. 
 - - Initialize singularity arrays for compatibility with IkGeoSolver. 
 
 	 **Commit:** `c631edf` | **Date:** 2025-11-05  
 
 --- 
 
 - Add IK solver for CRB15000 robots and supporting structsIntroduced an inverse kinematics solver (`IkGeoSolver`) for CRB15000 (GoFa) robots, wrapping the native `ik-geo` library. Added binary dependencies (`ikgeoInterface_GoFa.dll`, `libgcc_s_seh-1.dll`, `libstdc++-6.dll`, `libwinpthread-1.dll`) required for the solver. 
 - Added supporting geometry structs: 
 - - `Quaternion`: Represents quaternions with conversion methods. 
 - - `Vector3d`: Represents 3D vectors with Rhino type conversions. 
 - - `Vector6d`: Represents 6D robot joint positions with utility methods. 
 - Implemented `Compute_CRB15000` to calculate IK solutions, handle singularities, and convert results to robot configurations. Added detailed documentation for all new components. 
 
 	 **Commit:** `63751d6` | **Date:** 2025-11-05  
 
 --- 
 


