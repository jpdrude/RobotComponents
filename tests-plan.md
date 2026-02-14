# Comprehensive Test Suite Plan for RobotComponents

## Context

The project originally had **one test** (`RobotPresetTests.cs`) covering preset enum-to-class consistency. This plan adds structured tests across all layers, prioritized by risk and value. Phases 1, 2, 2b, and 3 are complete with **410 tests** across 25 test classes.

## Test Infrastructure

**Framework**: xUnit (already in use), Pester for PowerShell scripts
**Project**: `RobotComponents.Tests/` (existing)
**Pipeline scripts**: `.github/scripts/tests/` (new, Pester)

### Project reference additions needed in `RobotComponents.Tests.csproj`

Currently references: `RobotComponents`, `RobotComponents.ABB`, `RobotComponents.ABB.Presets`
Add: `RobotComponents.ABB.Gh.Goos` (for serialization round-trip tests later, not in this phase)

---

## Phase 1: Utility & Helper Tests — COMPLETE (42 tests)

### `HelperMethodTests.cs` — 30 tests

- [x] **Quaternion / Plane round-trip**: `PlaneToQuaternion` then `QuaternionToPlane` returns original plane (within tolerance), including rotated planes
- [x] **Known quaternion values**: identity quaternion (1,0,0,0) produces WorldXY plane; component overload matches Quaternion overload; XYZ overload sets correct origin
- [x] **FlipPlaneX / FlipPlaneY**: verify axis negation, origin unchanged, normal flip on X
- [x] **Slerp**: t=0 returns q1, t=1 returns q2, t=0.5 returns midpoint; clamping t<0 to 0, t>1 to 1
- [x] **Lerp**: same boundary and clamping tests
- [x] **DotProduct**: orthogonal quaternions produce 0, identical produce 1, opposite produce -1
- [x] **ReplaceFirst**: replaces only first occurrence, no match returns original, empty search, replace with empty
- [x] **SetRapidDataFromString**: parse VAR/LOCAL/TASK/CONST declarations; malformed input produces exception

### `PresetHelperTests.cs` — 9 tests

- [x] **GetRobotNameFromPresetName**: standard, CRB, IRB1010 special case, LID suffix
- [x] **GetRobotClassNameFromName**: reverse of above, single-digit reach padding
- [x] **Round-trip**: all presets name → className → name matches enum name

### `VersionNumberingTests.cs` — 3 tests

- [x] `CurrentVersion` string parses as valid `System.Version`
- [x] `CurrentVersion` matches `Version.ToString()`
- [x] Version has expected components (non-negative Major, Minor, Build)

---

## Phase 2: RAPID Code Generation Tests — COMPLETE (246 tests)

### `SpeedDataTests.cs` — 34 tests

- [x] Predefined speed mapping: exact (v5, v100) and nearest-snap (25→v30, 7→v5)
- [x] Int overload matches double overload
- [x] Custom constructor: 4-arg and 5-arg variants, default optional args
- [x] `ToRAPID()` bracket format for custom and predefined
- [x] `ToRAPIDDeclaration`: custom named → VAR declaration, predefined → empty, unnamed → empty, LOCAL/TASK scope, PERS variable type
- [x] `ToRAPIDInstruction` returns empty
- [x] `IsValid`: positive → true, zero TCP → false, negative ORI → false, zero LEAX → false
- [x] `Duplicate` returns matching values
- [x] `Parse` / `TryParse` round-trip and invalid input
- [x] Static helpers: predefined names, values, data, enum-to-constructor mapping

### `ZoneDataTests.cs` — 32 tests

- [x] Predefined zones: fine point, z10, z0, nearest-snap (8→z10)
- [x] Int overload matches double overload
- [x] Custom constructor: named, unnamed, fine point
- [x] `ToRAPID()`: fine vs fly-by point, 7-element arrays for fine/z10/z0
- [x] `ToRAPIDDeclaration`: custom named, predefined, unnamed, LOCAL scope
- [x] `ToRAPIDInstruction` returns empty
- [x] `IsValid`: predefined, fine point, zero path zone, negative path zone
- [x] `Duplicate`, `Parse` / `TryParse`, static helpers

### `ConfigurationDataTests.cs` — 17 tests

- [x] Constructor: 4-int, default all-zero, named
- [x] `ToRAPID`: bracket format, zeros, negative values
- [x] `ToRAPIDDeclaration`: named (CONST confdata), unnamed, VAR keyword
- [x] `ToRAPIDInstruction` returns empty
- [x] `Duplicate`, `Parse` / `TryParse`

### `ExternalJointPositionTests.cs` — 17 tests

- [x] Constructor: default all-9E9, single axis, two axes, named, NaN→default
- [x] `ToRAPID`: all default, first axis set, decimal formatting
- [x] `ToRAPIDDeclaration`: named (extjoint), unnamed
- [x] `ToRAPIDInstruction` returns empty
- [x] Indexer: int and char ('a'…'f') access
- [x] `IsValid`, `Duplicate`, `Length` always 6

### `JointTargetTests.cs` — 17 tests

- [x] Constructor: with RobotJointPosition, named with all fields, named with RJP only
- [x] `ToRAPID`: nested array format `[[rjp], [ejp]]`
- [x] `ToRAPIDDeclaration`: named (jointtarget), unnamed
- [x] `ToRAPIDInstruction` returns empty
- [x] `IsValid`: valid positions true, default constructor false
- [x] `Duplicate`, `Parse` / `TryParse`

### `RobotTargetTests.cs` — 19 tests

- [x] Plane-to-quaternion embedded in declaration (WorldXY → identity, translated plane)
- [x] `ToRAPID` produces 4-element nested array with config data and external axes
- [x] Named ConfigurationData uses variable name in output
- [x] `ToRAPIDDeclaration`: named (robtarget), unnamed
- [x] `ToRAPIDInstruction` returns empty
- [x] `IsValid`: valid plane true, Unset plane false
- [x] `Duplicate`, `Parse` / `TryParse`

### Instruction Tests — 57 tests (split into individual files)

Originally `InstructionTests.cs`, split into per-type test files for maintainability:

**`SetDigitalOutputTests.cs`** — 9 tests
- [x] Basic true/false, delay, sync, sync overrides delay, IsValid (name, null, empty), declaration empty

**`WaitTimeTests.cs`** — 6 tests
- [x] Basic, integer trailing zeros, InPos flag, IsValid (zero, negative), declaration empty

**`WaitDigitalIOTests.cs`** — 8 tests
- [x] WaitDI: basic true/false, MaxTime with TimeFlag true/false, IsValid (name, null)
- [x] WaitDO: basic, MaxTime

**`WaitAnalogIOTests.cs`** — 6 tests
- [x] WaitAI: LessThan, GreaterThan, MaxTime, IsValid
- [x] WaitAO: LessThan, MaxTime

**`WaitGroupIOTests.cs`** — 5 tests
- [x] WaitGI: basic, MaxTime, IsValid
- [x] WaitGO: basic, MaxTime

**`WaitRobTests.cs`** — 7 tests
- [x] InPos, ZeroSpeed, default not valid, InPos valid, ZeroSpeed valid, both true not valid, declaration empty

**`CodeLineAndCommentTests.cs`** — 16 tests
- [x] CodeLine: instruction/declaration types, default type, IsValid (non-empty, empty, null)
- [x] Comment: instruction/declaration types, default type, IsValid (non-empty, empty, null)

### `MovementTests.cs` — 20 tests

- [x] MoveAbsJ with JointTarget: correct instruction format, named target uses variable name
- [x] MoveL with RobotTarget: correct instruction format
- [x] MoveJ with RobotTarget: correct instruction format
- [x] Predefined speed uses name (v100), custom named speed produces declaration
- [x] Invalid combinations: JointTarget with MoveL/MoveJ throws
- [x] IsValid: valid movement true, null target false
- [x] MoveC with circular via-point: correct instruction format, unset circular point throws
- [x] MoveLDO / MoveJDO / MoveCDO: Movement with SetDigitalOutput produces correct DO variant
- [x] MoveAbsJ with DO produces separate SetDO (no MoveAbsJDO variant)
- [x] `\ID` parameter: SyncID appended to instruction, default SyncID omitted
- [x] `\T` parameter: Time appended to instruction, default Time omitted

### `RobotJointPositionTests.cs` — 21 tests

- [x] Constructor: default all-zero, 6-arg, named, NaN replaced with zero, list-of-doubles
- [x] `ToRAPID`: 6-element bracket format, decimal formatting
- [x] `ToRAPIDDeclaration`: named (robjoint), unnamed → empty, LOCAL/TASK scope
- [x] `ToRAPIDInstruction` returns empty
- [x] Indexer: int access [0]–[5], out-of-range throws
- [x] `IsValid`, `Duplicate`
- [x] `Parse` / `TryParse` round-trip, invalid input, CONST variable type

### `RAPIDGeneratorTests.cs` — 12 tests

- [x] Module structure: empty actions → MODULE/ENDMODULE, with action → PROC/ENDPROC, custom module name, RC version comment
- [x] Deduplication: SpeedData and ZoneData with same name produce one declaration each
- [x] Instruction ordering matches input order
- [x] Declaration sorting alphabetical
- [x] Optional sections: tooldata/wobjdata/loaddata inclusion flags
- [x] Mixed actions: correct section layout (MODULE → declarations → PROC → instructions → ENDPROC → ENDMODULE)

---

## Phase 3: Definitions & Kinematics Tests — COMPLETE (121 tests)

### `RobotTests.cs` — 21 tests

- [x] Construct from kinematic parameters produces IsValid = true; empty → invalid
- [x] WorldXY and offset base planes set correctly
- [x] Axis planes: 6 planes, axis 1 at origin, axis 2 at C1 height, axis 3 at C1+C2
- [x] Mounting frame at correct position (374, 0, 630) for IRB120
- [x] Tool: tool0 valid, ToolPlane at mounting frame
- [x] Properties: NumberOfAxes=6, limits, kinematic parameters, external axes empty, 8 meshes, IK/FK initialized
- [x] Duplicate: independent copy, preserves parameters and limits, DuplicateMechanicalUnit
- [x] ToString valid/invalid

### `RobotToolTests.cs` — 25 tests

- [x] Default tool: name "tool0", WorldXY planes, RobotHold=true
- [x] Custom tool: attachment/tool plane, with LoadData
- [x] `ToRAPID`: default format, custom position, robotHold false
- [x] `ToRAPIDDeclaration`: default/LOCAL/TASK scope, tool0→empty, custom→declaration, empty name→empty
- [x] IsValid: valid, empty name, unset attachment plane
- [x] Duplicate: independent copy, without mesh
- [x] Properties: datatype, scope, variableType, toString
- [x] Parse/TryParse: valid, invalid, throws on bad input

### `WorkObjectTests.cs` — 27 tests

- [x] Default wobj: name "wobj0", WorldXY, FixedFrame=true
- [x] Custom frame: name+objectFrame, with userFrame
- [x] `ToRAPID`: default format, custom objectFrame, with userFrame quaternion
- [x] `ToRAPIDDeclaration`: default/LOCAL scope, wobj0→empty, custom→declaration, empty name→empty
- [x] IsValid: valid, empty name, null name
- [x] FixedFrame: no external axis → true
- [x] GlobalWorkObjectPlane: default→WorldXY, with userFrame→combined (150,0,0)
- [x] Duplicate: independent copy, preserves all properties
- [x] Properties: datatype, scope, variableType, toString
- [x] Parse/TryParse: valid, invalid, throws on bad input

### `KinematicsTests.cs` — 30 tests

- [x] **OPW Forward**: all-zero angles → valid plane, correct position, wrist output, joint1 rotation, different angles, too few joints → exception
- [x] **OPW Inverse**: 8 solutions, singularity flag arrays
- [x] **OPW Round-trip**: all-zero, non-trivial pose, negative angles all recover original
- [x] **Singularity detection**: wrist J5≈0, shoulder target at origin, normal → no singularity
- [x] **Angle normalization**: no NaN values
- [x] **ForwardKinematics with Robot**: all-zero TCP, joint1 rotation, in-limits check, 6 posed planes, 7 transforms, different positions
- [x] **FK validity**: empty→invalid, before calculate→invalid, toString

### `RobotKinematicParametersTests.cs` — 18 tests

- [x] Construct from values then `GetAxisPlanes()` then construct from planes produces matching parameters
- [x] Large robot (IRB6700-like) round-trip
- [x] Empty constructor → NaN, invalid; value constructor → correct, valid
- [x] IRB120 parameters match expected values, zero wrist offset
- [x] GetAxisPlanes: 6 planes, plane0 at origin, plane1 at A1/C1, plane5 at wrist, mounting frame, offset base
- [x] Duplicate: preserves values, preserves NaN
- [x] ToString valid/invalid

**Files created:**
- `RobotComponents.Tests/Definitions/RobotTests.cs`
- `RobotComponents.Tests/Definitions/RobotToolTests.cs`
- `RobotComponents.Tests/Definitions/WorkObjectTests.cs`
- `RobotComponents.Tests/Kinematics/KinematicsTests.cs`
- `RobotComponents.Tests/Kinematics/RobotKinematicParametersTests.cs`

---

## Phase 4: CI/CD Pipeline Script Tests (low priority)

Extract inline PowerShell from workflows into `.github/scripts/`, test with Pester.

### Scripts to extract

| Script | Source workflow | Logic |
|--------|---------------|-------|
| `Validate-Version.ps1` | `release.yml` | Parse tag + VersionNumbering.cs, compare |
| `Collect-ReleaseFiles.ps1` | `release.yml`, `artifact-build.yml` | Gather .gha + DLLs into staging dir |
| `Extract-Changelog.ps1` | `release.yml` | Parse CHANGELOG.md for release notes |

### Pester tests

- [ ] **`Validate-Version.Tests.ps1`**: matching/mismatching versions, malformed file, missing file
- [ ] **`Collect-ReleaseFiles.Tests.ps1`**: all files present, missing .gha fails, missing DLL fails
- [ ] **`Extract-Changelog.Tests.ps1`**: normal changelog, empty file, truncation at 10K chars

**Files to create:**
- `.github/scripts/Validate-Version.ps1`
- `.github/scripts/Collect-ReleaseFiles.ps1`
- `.github/scripts/Extract-Changelog.ps1`
- `.github/scripts/tests/Validate-Version.Tests.ps1`
- `.github/scripts/tests/Collect-ReleaseFiles.Tests.ps1`
- `.github/scripts/tests/Extract-Changelog.Tests.ps1`

**Files to modify:**
- `.github/workflows/ci.yml` — add Pester test step
- `.github/workflows/release.yml` — call extracted scripts
- `.github/workflows/artifact-build.yml` — call extracted scripts

---

## Verification

1. Build: `msbuild /t:Build /p:Configuration=Release`
2. Run xUnit: `vstest.console.exe RobotComponents.Tests/bin/Release/net48/RobotComponents.Tests.dll`
3. Run Pester locally: `Invoke-Pester .github/scripts/tests/ -Output Detailed`
4. Push to branch and verify CI runs both xUnit and Pester steps
