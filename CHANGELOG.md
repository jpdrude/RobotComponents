# Changelog

## All notable changes to this modified version of Robot Components are documented here.

### Changelog 
 Generated on: 2026-02-10 14:33 
 --- 
 - Add RAPID WaitRob instruction and GH component support - Implement WaitRob class for RAPID WaitRob instruction (InPos/ZeroSpeed) - Add GH_WaitRob Goo, WaitRobComponent, and Param_WaitRob - Register new icons and update resources for WaitRob 
  
   **Commit:** `39531f5` | **Date:** 2026-02-10 
 
 --- 
 
 - Add RAPID system module support and module loading tools - Added UploadSystemModule to Controller for .SYS module upload, config, and warm restart - UploadModule now detects and delegates system modules - Added LoadModuleComponent for RAPID module load/unload code generation - RoutineCallComponent: support for cross-module routine calls - Fixed routine scope input index in RAPIDGeneratorComponent - Added new icons and registered in resources - Updated copyright and CHANGELOG - Minor codegen warnings and documentation improvements 
  
   **Commit:** `55cc675` | **Date:** 2026-01-29 
 
 --- 
 
 - Add WaitAO/DO/GI/GO instructions and GH components and parameters. 
 	 - Introduced WaitAO, WaitDO, WaitGI, and WaitGO instruction classes with serialization and RAPID code generation. 
 	 - Added corresponding Grasshopper components, parameter types, and Goo wrappers for each wait instruction. 
 	 - Updated icons and resources for new types. 
 	 - Updated CHANGELOG. 
  
   **Commit:** `dcbc844` | **Date:** 2026-01-27 
 
 --- 
 
 - Adds support for RAPID System Modules and Option for loading additional modules into the task. 
 	 - RAPIDGenerator and controller methods now support system modules (.SYS), generating correct RAPID headers and file extensions. 
 	 - Grasshopper RAPIDGeneratorComponent exposes "Is System Module" input and UI. 
 	 - UploadHelperModulesComponent adds "Load To Task" input. 
 	 - Improved module name extraction and file cleanup. 
 	 - Warnings for invalid system module/routine names. 
  
   **Commit:** `b716218` | **Date:** 2026-01-22 
 
 --- 
 
 - Adds support for group signals. Group signals are bitmasks used to communicate groups of digital signals. 
 - Comprehensive support for group inputs/outputs: - Controller class now manages group signals with new retrieval methods and properties. 
 	 - Added Grasshopper components for getting/setting group inputs/outputs, including bitmask support and signal picking UI. 
 	 - Introduced SetGroupOutput instruction, Goo, and parameter classes for RAPID code generation. 
 	 - New DeconstructGroupSignal component for bitwise signal analysis. 
 	 - Updated resources and icons for new components. 
  
   **Commit:** `e3f491e` | **Date:** 2026-01-22 
 
 --- 
 
 - Adds support for user-defined RAPID routine arguments. 
 	 - Introduces RoutineArgument class and Grasshopper components for defining and calling routines with arguments. 
 	 - Updates code generation, serialization, and UI to support variable arguments in PROC routines, with new icons and changelog entries. 
  
   **Commit:** `4da12b6` | **Date:** 2026-01-21 
 
 --- 
 
 - Add simple RAPID routine definition. Procedures (PROC) and Interrupts (TRAP) can now be defined. No functions or arguments are supported. 
 	 - Introduce Routine class for user-defined PROC/TRAP routines with scope (GLOBAL/LOCAL/TASK). 
 	 - Add Param_Routine and GH_Routine for Grasshopper integration. 
 	 - Implement AdditionalRoutineComponent for custom routines. 
 	 - Update RAPIDGenerator and RAPIDGeneratorComponent to handle additional routines and routine scope. 
 	 - Add ScopeValueList and RoutineTypeValueList components for easy selection. 
 	 - Update icons/resources for new features. 
 	 - Update Scope declaration in RAPID Code generation. 
  
   **Commit:** `772002b` | **Date:** 2026-01-08 
 
 --- 
 
 - Fixed helper module upload & modular RAPID code support - Controller now clears local additional directory before writing new files to prevent stale files. 
 	 - Removed legacy GLOBAL declaration parsing from RAPIDGenerator, as a GLOBAL keyword doesn't exist in RAPID. 
  
   **Commit:** `4c36ed8` | **Date:** 2026-01-07 
 
 --- 
 
 - Add helper module upload & modular RAPID code support - Added Controller.UploadHelperModules for uploading additional RAPID modules to controller storage without overwriting the main program. 
 	 - Introduced UploadHelperModulesComponent for Grasshopper, enabling users to upload helper modules. 
 	 - Enhanced RAPIDGenerator and RAPIDGeneratorComponent to support a "Superordinate Main Method" input, filtering out duplicate global declarations in helper modules. 
 	 - Improved input parameter management and context menu in RAPIDGeneratorComponent. 
 	 - Updated changelog and project file for new features and dependencies. 
  
   **Commit:** `7bf7d6e` | **Date:** 2026-01-03 
 
 --- 
 
 - Add LOCAL routine option to RAPIDGeneratorComponent - Added context menu option to declare RAPID routines as LOCAL - Updated RAPIDGenerator to support LOCAL keyword in code output - Preserved LOCAL setting in serialization and duplication - Improved multi-iteration handling in RAPIDGeneratorComponent - Fixed minor documentation and comment issues 
  
   **Commit:** `39ce824` | **Date:** 2025-12-09 
 
 --- 
 
 - Update Changelog. 
  
   **Commit:** `a2b0718` | **Date:** 2025-11-20 
 
 --- 
 
 - Changed Rhino Common version to 7.36. 
  
   **Commit:** `a2d199b` | **Date:** 2025-11-20 
 
 --- 
 
 - Included dependency license files. 
  
   **Commit:** `ec3cb54` | **Date:** 2025-11-17 
 
 --- 
 
 - Updated Changelog Builder and Changelog. 
  
   **Commit:** `a6e3393` | **Date:** 2025-11-14 
 
 --- 
 
 - Implemented Changelog Generator 
  
   **Commit:** `5782669` | **Date:** 2025-11-14 
 
 --- 
 
 - Updated documentation and acknowledgments in AUTHORS.md and README.md to reflect the modified version of the project. Added Jan Philipp Drude and Johannes Pfleging as contributors. 
 - Updated SPDX license headers across all modified files to acknowledge the original and modified projects. Updated copyright information to include "2025 EDEK Uni Kassel." 
  
   **Commit:** `5e1bf3d` | **Date:** 2025-11-14 
 
 --- 
 
 - Add CheckActionsComponent and automate release process Introduced a new Grasshopper component, `CheckActionsComponent`, to validate robot actions and provide feedback. Added a corresponding icon (`CheckActions_Icon.png`) and localized resource for the UI. 
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
 
 - implements constructing configuration data from quadrant data inputs. 
 - Builds configuration data (cfx) from quarant data (cf1, cf4, cf6) as bitmask, if no cfx is provided. Also deconstructs cfx into quadrant data. 
  
   **Commit:** `67900ef` | **Date:** 2025-11-05 
 
 --- 
 
 - add singularity detection for CRB15000 robots using Jacobian analysis Implemented comprehensive singularity detection in the IkGeo solver for CRB15000 (GoFa) robots. Added fields for tracking wrist, elbow, shoulder singularities and missing solver results across all eight Cfx configurations. Introduced MathNet.Numerics dependency for Jacobian matrix operations. 
 - Implemented `ComputeSingularities` method to detect near-singular configurations using SVD-based Jacobian analysis with geometric alignment checks. Added `BuildJacobian` and `CheckJacobianSingularity` helper methods to construct the 6x6 manipulator Jacobian and evaluate singularity conditions using a relative tolerance threshold. 
 - Updated `InverseKinematics` class to retrieve and propagate singularity data from the IkGeo solver, including a sentinel value system (9e9) for missing solutions. Modified `CheckInternalAxisLimits` to skip validation for missing joint values and report when the solver returns no result. Added public properties `WristSingularities`, `ElbowSingularities`, `ShoulderSingularities`, and `NoSolverResults` to expose singularity information. 
  
   **Commit:** `3df7784` | **Date:** 2025-11-05 
 
 --- 
 
 - Fixed issues with Robot Components GH UI. 
 - Added the PosedInternalPlanes parameter to the variable output parameters array in the IK component. 
 - Added the CRB15000 robots to the enumeration of available robot presets. 
  
   **Commit:** `1b2e812` | **Date:** 2025-11-05 
 
 --- 
 
 - Add support for outputting posed planes in IK component (optional) Introduce a new feature to output posed planes in the `InverseKinematicsComponent` class. Added a `_outputPosedPlanesParameter` field to manage this functionality and updated the context menu with a new "Output Posed Axis Planes" option. 
 - Implemented the `MenuItemClickOutputPlanes` event handler to toggle the posed planes output and added serialization/deserialization support for this parameter in the `Write` and `Read` methods. 
 - Created the `GetPosedPlanesDataTree` method to transform posed axis planes into a structured data tree for output. Updated the `SolveInstance` method to populate the posed planes output parameter when enabled. 
  
   **Commit:** `0f14375` | **Date:** 2025-11-05 
 
 --- 
 
 - Implemented Axis Configuration sorting into IkGeoSolver and posed internal axis planes into InverseKinematics. 
 - Added `_posedInternalAxisPlanes` to `ForwardKinematics` to store posed internal axis planes and updated calculations to compute and expose these planes via a new public property. 
 - Introduced `_missingJointValue` sentinel in `IkGeoSolver` and implemented `ArrangeJointPositions` to sort IK solutions into RAPID's 8-slot Cfx ordering. This method uses forward kinematics and geometric tests to compute Cf1, Cf4, and Cf6 bitmask values. 
  
   **Commit:** `5d16dcf` | **Date:** 2025-11-05 
 
 --- 
 
 - Add IkGeoSolver for GoFa CRB15000 robots to InverseKinematcs class Introduce IkGeoSolver to handle inverse kinematics for GoFa CRB15000 robots, while retaining OPW/Wrist Offset solvers for other robots. 
 	 - Reorganize and update namespace imports, adding `IkGeoSolver`. 
 	 - Update `CalculateRobotJointPosition` to use IkGeoSolver for CRB15000 robots. 
 	 - Retain OPW/Wrist Offset solvers for other robot types. 
 	 - Initialize singularity arrays for compatibility with IkGeoSolver. 
  
   **Commit:** `c631edf` | **Date:** 2025-11-05 
 
 --- 
 
 - Add IK solver for CRB15000 robots and supporting structs Introduced an inverse kinematics solver (`IkGeoSolver`) for CRB15000 (GoFa) robots, wrapping the native `ik-geo` library. Added binary dependencies (`ikgeoInterface_GoFa.dll`, `libgcc_s_seh-1.dll`, `libstdc++-6.dll`, `libwinpthread-1.dll`) required for the solver. 
 - Added supporting geometry structs: - `Quaternion`: Represents quaternions with conversion methods. 
 	 - `Vector3d`: Represents 3D vectors with Rhino type conversions. 
 	 - `Vector6d`: Represents 6D robot joint positions with utility methods. 
 - Implemented `Compute_CRB15000` to calculate IK solutions, handle singularities, and convert results to robot configurations. Added detailed documentation for all new components. 
  
   **Commit:** `63751d6` | **Date:** 2025-11-05 
 
 --- 
 


