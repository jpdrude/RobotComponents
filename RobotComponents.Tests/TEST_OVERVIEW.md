# Test Overview — RobotComponents.Tests

## CI Behaviour

Tests tagged `[Trait("Category", "RequiresRhino")]` are excluded from the CI test filter and only run locally inside Rhino/Grasshopper. All other tests run in CI.

| Group | Requires Rhino | Runs in CI |
|---|---|---|
| CodeLine / Comment / Sanitizer | No | ✅ |
| HelperMethods (string / math) | Partial | ✅ / ⛔ |
| RAPID Declarations (SpeedData, ZoneData, etc.) | No | ✅ |
| RAPID Instructions (SetDO, WaitDI, etc.) | No | ✅ |
| VersionNumbering | No | ✅ |
| SerializationBinder | No | ✅ |
| RobotKinematicParameters | No | ✅ |
| Controller grant tests | No | ✅ |
| Movement / RAPIDGenerator | Yes | ⛔ |
| Robot / RobotTool / WorkObject | Yes | ⛔ |
| Kinematics (OPW) | Yes | ⛔ |
| Presets / RobotPresets | Yes | ⛔ |

---

## Test Infrastructure

### `TestHelpers.cs` — shared fixtures (not a test class)

Provides reusable factory methods used across multiple test classes. All methods require the Rhino native library.

| Method | Description |
|---|---|
| `CreateIRB120Robot()` | Builds an IRB120-3/0.58 `Robot` with real kinematic parameters and empty meshes |
| `CreateIRB120Parameters()` | Returns `RobotKinematicParameters` for the IRB120 |
| `CreateIRB120Limits()` | Returns the 6 axis limit `Interval[]` for the IRB120 |
| `CreateIRB120OPW()` | Builds an `OPWKinematics` solver. A2 must be set before C3 due to triggered recalculation — documented in the method |
| `AssertAnySolutionMatches()` | Asserts at least one of the 8 IK solutions matches expected joint angles within tolerance |

---

## RAPID Code — Code Lines & Comments

### `Actions/CodeLineAndCommentTests.cs`

#### `CodeLineTests` — no Rhino

Tests `CodeLine` routing between instruction and declaration sections.

| Test | What it checks |
|---|---|
| `InstructionType_ToRAPIDInstruction_ReturnsCode` | Instruction-type `CodeLine` returns code via `ToRAPIDInstruction()` |
| `InstructionType_ToRAPIDDeclaration_ReturnsEmpty` | Instruction-type returns empty via `ToRAPIDDeclaration()` |
| `DeclarationType_ToRAPIDDeclaration_ReturnsCode` | Declaration-type returns code via `ToRAPIDDeclaration()` |
| `DeclarationType_ToRAPIDInstruction_ReturnsEmpty` | Declaration-type returns empty via `ToRAPIDInstruction()` |
| `DefaultType_IsInstruction` | Default `CodeType` is `Instruction` |
| `IsValid_NonEmpty_ReturnsTrue` | Non-empty code string is valid |
| `IsValid_Empty_ReturnsFalse` | Empty string is invalid |
| `IsValid_Null_ReturnsFalse` | Null code is invalid |

#### `CodeLineSanitizationTests` — no Rhino

Tests `RapidCodeLineSanitizer` directly and via the `CodeLine` constructor and property setter.

| Test | What it checks |
|---|---|
| `Sanitize_NormalCode_NoWarnings` | Clean RAPID code passes through without warnings |
| `Sanitize_StripsNewlines` | `\n` replaced with space, warning issued |
| `Sanitize_StripsCarriageReturnNewline` | `\r\n` replaced with space, warning issued |
| `Sanitize_DetectsEndProc` | `ENDPROC` triggers a warning |
| `Sanitize_DetectsEndModule` | `ENDMODULE` triggers a warning |
| `Sanitize_DetectsProc` | `PROC` triggers a warning |
| `Sanitize_DetectsModule` | `MODULE` triggers a warning |
| `Sanitize_DetectsTrap` | `TRAP` triggers a warning |
| `Sanitize_LowercaseKeyword_NoWarning` | `"endproc"` (lowercase) does NOT trigger a warning — detection is case-sensitive |
| `Sanitize_AllowsProcInContext` | `"proc_target"` is not flagged — word-boundary matching prevents false positives |
| `Sanitize_MultipleIssues` | Newline + two keywords produces ≥ 3 warnings |
| `Sanitize_NullInput_NoWarnings` | Null input returns null code with no warnings |
| `Sanitize_EmptyInput_NoWarnings` | Empty string returns empty code with no warnings |
| `CodeLine_WithStructuralKeyword_IsInvalid` | `CodeLine` with `ENDPROC` is invalid and has warnings |
| `CodeLine_WithSafeCode_IsValid` | `CodeLine` with normal code is valid and has no warnings |
| `CodeLine_SetterSanitizes` | Assigning to `Code` property re-runs sanitization |
| `CodeLine_NewlinesSanitizedInOutput` | Newlines in constructor input are collapsed to spaces |
| `CodeLine_VarDeclaration_IsValid` | `VAR num x := 5;` as declaration type is valid |
| `CodeLine_InjectionWithSemicolon_NoStructuralWarning` | Semicolons alone do not trigger warnings |
| `CodeLine_FlaggedCode_ToRAPIDInstruction_StillReturnsCode` | Raw string still returned even when flagged — the guard is in `ToRAPIDGenerator`, not `ToRAPIDInstruction` |
| `CodeLine_FlaggedCode_ToRAPIDDeclaration_StillReturnsCode` | Same as above for declaration type |

#### `CommentTests` — no Rhino

Tests `Comment` routing and newline stripping.

| Test | What it checks |
|---|---|
| `InstructionType_ToRAPIDInstruction_ReturnsBangPrefix` | Instruction comment outputs `! text` |
| `InstructionType_ToRAPIDDeclaration_ReturnsEmpty` | Instruction comment returns empty for declarations |
| `DeclarationType_ToRAPIDDeclaration_ReturnsBangPrefix` | Declaration comment outputs `! text` |
| `DeclarationType_ToRAPIDInstruction_ReturnsEmpty` | Declaration comment returns empty for instructions |
| `DefaultType_IsInstruction` | Default `CodeType` is `Instruction` |
| `IsValid_NonEmpty_ReturnsTrue` / `_Empty_ReturnsFalse` / `_Null_ReturnsFalse` | Validity edge cases |
| `NewlineStripped_SingleArg` | `\n` in text is collapsed to space |
| `NewlineStripped_TwoArg` | `\r\n` is collapsed to space |
| `SetterStripsNewlines` | `Com` property setter strips newlines |
| `NullComment_NoException` | Setting `Com = null` does not throw |
| `EmptyComment_NoException` | Empty string in constructor does not throw |
| `NoNewline_Unchanged` | Clean text is unchanged |
| `CarriageReturn_Stripped` | Bare `\r` is stripped |

---

## RAPID Code — Movement & Code Generation

### `Actions/MovementTests.cs` — all `RequiresRhino`

Tests `Movement` → `RAPIDGenerator.CreateModule()` output using an IRB120 preset robot.

| Test | What it checks |
|---|---|
| `MoveAbsJ_JointTarget_ProducesCorrectInstruction` | Output contains `MoveAbsJ`, `v100`, `z10` |
| `MoveAbsJ_NamedTarget_UsesVariableName` | Target variable name appears in output |
| `MoveL_RobotTarget_ProducesCorrectInstruction` | Output contains `MoveL`, `v100` |
| `MoveJ_RobotTarget_ProducesCorrectInstruction` | Output contains `MoveJ` |
| `MoveAbsJ_PredefinedSpeed_UsesSpeedName` | `SpeedData(100)` renders as `v100`, not inline values |
| `MoveAbsJ_CustomNamedSpeed_ProducesDeclaration` | Custom speed produces `VAR speeddata` declaration |
| `JointTarget_WithMoveL_ThrowsInvalidOperationException` | Using `JointTarget` with `MoveL` throws |
| `JointTarget_WithMoveJ_ThrowsInvalidOperationException` | Using `JointTarget` with `MoveJ` throws |
| `MoveC_RobotTarget_ProducesCorrectInstruction` | Output contains `MoveC` and circular via-point name |
| `MoveC_UnsetCircularPoint_ThrowsException` | Missing circular point throws on generation |
| `MoveL_WithDigitalOutput_ProducesMoveLDO` | Combined instruction renders as `MoveLDO` with `DO_1, 1` |
| `MoveJ_WithDigitalOutput_ProducesMoveJDO` | Combined instruction renders as `MoveJDO` |
| `MoveAbsJ_WithDigitalOutput_ProducesSeparateSetDO` | `MoveAbsJ` cannot embed DO, generates separate `SetDO` |
| `MoveC_WithDigitalOutput_ProducesMoveCDO` | Combined instruction renders as `MoveCDO` |
| `MoveL_WithSyncID_AppendsBackslashID` | `SyncID = 5` appends `\ID:=5` |
| `MoveL_DefaultSyncID_NoBackslashID` | No `\ID` in output by default |
| `MoveL_WithTime_AppendsBackslashT` | `Time = 2.5` appends `\T:=2.5` |
| `MoveL_DefaultTime_NoBackslashT` | No `\T` in output by default |
| `MoveAbsJ_RobotTargetConvertedViaIK_ChecksAxisLimits` | Target behind the robot causes IK to violate axis 1 limit, `ErrorText` populated |
| `IsValid_ValidMovement_ReturnsTrue` | Movement with valid target and speed is valid |
| `IsValid_NullTarget_ReturnsFalse` | Default empty movement is invalid |

### `Actions/RAPIDGeneratorTests.cs` — all `RequiresRhino`

Tests `RAPIDGenerator.CreateModule()` structure and options.

| Test | What it checks |
|---|---|
| `CreateModule_EmptyActions_ProducesModuleStructure` | Output contains `MODULE MainModule` and `ENDMODULE` |
| `CreateModule_WithAction_ProducesProcAndEndproc` | A movement action produces `PROC main()` and `ENDPROC` |
| `CreateModule_CustomModuleName_UsesCustomName` | Custom module and proc names respected |
| `CreateModule_VersionComment_IncludesRCVersion` | Generator comment present in output |
| `CreateModule_SpeedDataDeduplication_OnlyOneDeclaration` | Same custom speed used twice produces exactly one `VAR speeddata` |
| `CreateModule_ZoneDataDeduplication_OnlyOneDeclaration` | Same custom zone used twice produces exactly one `VAR zonedata` |
| `CreateModule_InstructionOrder_MatchesInputOrder` | Three movements appear in input order |
| `CreateModule_DeclarationsSorted_Alphabetically` | `VAR jointtarget` declarations are sorted alphabetically |
| `CreateModule_AddTooldataFalse_OmitsTooldata` | Tooldata section suppressed when flag is false |
| `CreateModule_AddWobjdataFalse_OmitsWobjdata` | Wobjdata section suppressed when flag is false |
| `CreateModule_AddLoaddataFalse_OmitsLoaddata` | Loaddata section suppressed when flag is false |
| `CreateModule_MixedActions_CorrectSectionLayout` | Ordering: MODULE → declarations → PROC → instructions → ENDPROC → ENDMODULE |
| `CreateModule_AxisViolation_EnforceTrue_ReturnsEmptyModule` | Out-of-range axis with enforcement on returns empty module and populates `ErrorText` |
| `CreateModule_AxisViolation_EnforceFalse_ReturnsFullModule` | Enforcement off still populates `ErrorText` but returns full module |
| `CreateModule_NoViolation_EnforceTrue_ReturnsFullModule` | Valid values produce full module with empty `ErrorText` |
| `Duplicate_PreservesEnforceAxisLimits_True` | `EnforceAxisLimits = true` preserved on `Duplicate()` |
| `Duplicate_PreservesEnforceAxisLimits_False` | `EnforceAxisLimits = false` preserved on `Duplicate()` |

---

## RAPID Declarations

### `Actions/SpeedDataTests.cs` — no Rhino

Tests `SpeedData` RAPID output and recommended limit checks.

- Predefined speeds (`v100`, `v200` etc.) render as their name, not inline values
- Custom named speeds produce `VAR speeddata name := [...]`
- `ExceedsRecommendedLimits` returns `true` above max constants, `false` below
- `IsValid` checks for null/empty name and zero/negative TCP speed

### `Actions/ZoneDataTests.cs` — no Rhino

Tests `ZoneData` RAPID output and recommended limit checks.

- Predefined zones (`z10`, `fine` etc.) render as their name
- Custom named zones produce `VAR zonedata name := [...]`
- `ExceedsRecommendedLimits` returns `true` above max constants
- Per-parameter and `IsValid` checks

### `Actions/RobotTargetTests.cs` — `RequiresRhino`

| Test | What it checks |
|---|---|
| `Parse_ValidRapidString_RoundTrip` | Parses a `robtarget` RAPID string and re-serializes to the same string |
| `Parse_InvalidRapidString_ThrowsException` | Malformed string throws |

### `Actions/JointTargetTests.cs` — no Rhino

- Constructor and `ToRAPIDDeclaration()` produce correct `VAR jointtarget` strings
- `IsValid` checks

### `Actions/RobotJointPositionTests.cs` — no Rhino

- All 6 axis values assigned correctly by constructor
- `ToRAPID()` produces correct bracket notation
- `IsValid` and default value checks

### `Actions/ExternalJointPositionTests.cs` — no Rhino

- `9E9` sentinel value used for undefined axes renders correctly in `ToRAPID()` output
- Constructor and `IsValid` checks

### `Actions/ConfigurationDataTests.cs` — no Rhino

- cf1/cf4/cf6/cfx assigned correctly by constructor
- `ToRAPID()` produces correct string
- `Parse` round-trip and `IsValid` checks

### `Actions/WaitRobTests.cs` — no Rhino

- `ToRAPIDInstruction()` produces correct `WaitRob` statement
- `IsValid` checks

---

## RAPID Instructions — Signal I/O & Timing

All of these test `ToRAPIDInstruction()` output, `IsValid`, and signal name validation via `ThrowIfInvalidRapidIdentifier`. None require Rhino.

### `Actions/SetDigitalOutputTests.cs`
- Valid/invalid signal names; `true` → `1`, `false` → `0` in output

### `Actions/SetAnalogOutputTests.cs`
- Numeric value formats correctly; invalid name throws

### `Actions/SetGroupOutputTests.cs`
- Integer value formats correctly; invalid name throws

### `Actions/PulseDigitalOutputTests.cs`
- Default pulse (no length) vs. specified length — `\PLength:=x` appended only when set

### `Actions/WaitTimeTests.cs`
- `InPos` flag appends `\InPos`; zero/negative time validity checks

### `Actions/WaitDigitalIOTests.cs`
- `WaitDI signal, value` format; signal name validation

### `Actions/WaitAnalogIOTests.cs`
- `WaitAI signal \relop value` format; signal name validation

### `Actions/WaitGroupIOTests.cs`
- `WaitGI signal, value` format; signal name validation

---

## Definitions

### `Definitions/RobotTests.cs` — mostly `RequiresRhino`

Tests `Robot` construction and properties using `TestHelpers.CreateIRB120Robot()`.

| Test | Rhino | What it checks |
|---|---|---|
| `Constructor_WithKinematicParameters_ProducesValidRobot` | ✅ | Robot is valid, name is set |
| `Constructor_Empty_ProducesInvalidRobot` | ✅ | `new Robot()` is invalid, name is `"Empty Robot"` |
| `Constructor_WorldXYBase_SetsBasePlane` | ✅ | Base plane is WorldXY |
| `Constructor_OffsetBase_SetsBasePlane` | ✅ | Offset base plane set correctly |
| `InternalAxisPlanes_FromKinematicParameters_HasSixPlanes` | ✅ | 6 axis planes produced |
| `InternalAxisPlanes_Axis1_AtBaseOrigin` | ✅ | Axis 1 origin at (0,0,0) |
| `InternalAxisPlanes_Axis2_AtC1Height` | ✅ | Axis 2 origin at (0,0,290) |
| `InternalAxisPlanes_Axis3_AtC1PlusC2Height` | ✅ | Axis 3 origin at (0,0,560) |
| `MountingFrame_IRB120Parameters_CorrectPosition` | ✅ | Mounting frame at (374,0,630) |
| `Tool_DefaultTool0_IsValid` | ✅ | Default tool is valid with name `tool0` |
| `ToolPlane_DefaultTool0_AtMountingFrame` | ✅ | TCP at mounting frame position |
| `NumberOfAxes_Always6` | ✅ | 6 axes |
| `InternalAxisLimits_SetCorrectly` | ✅ | All 6 axis limits match IRB120 spec |
| `RobotKinematicParameters_Accessible` | ✅ | All kinematic parameters match IRB120 |
| `ExternalAxes_DefaultEmpty` | ✅ | No external axes by default |
| `Meshes_Contains8Elements` | ✅ | 7 body + 1 tool mesh = 8 |
| `InverseKinematics_Initialized` | ✅ | IK object not null |
| `ForwardKinematics_Initialized` | ✅ | FK object not null |
| `Duplicate_CreatesIndependentCopy` | ✅ | Modifying copy does not affect original |
| `Duplicate_PreservesKinematicParameters` | ✅ | Kinematic parameters copied correctly |
| `Duplicate_PreservesAxisLimits` | ✅ | All 6 axis limits copied correctly |
| `DuplicateMechanicalUnit_ReturnsValidRobot` | ✅ | Returns 6-axis mechanical unit |
| `ToString_ValidRobot_IncludesName` | ✅ | Returns `"Robot (TestRobot)"` |
| `ToString_InvalidRobot_ReturnsInvalid` | ✅ | Returns `"Invalid Robot"` for empty robot |

### `Definitions/RobotToolTests.cs` — `RequiresRhino`

| Test | What it checks |
|---|---|
| `Parse_ValidRapidString_RoundTrip` | Parses a `tooldata` RAPID string and re-serializes to the same string |
| `Parse_InvalidRapidString_ThrowsException` | Malformed string throws |

### `Definitions/WorkObjectTests.cs` — `RequiresRhino`

| Test | What it checks |
|---|---|
| `Parse_ValidRapidString_RoundTrip` | Parses a `wobjdata` RAPID string and re-serializes to the same string |
| `Parse_InvalidRapidString_ThrowsException` | Malformed string throws |

---

## Kinematics

### `Kinematics/KinematicsTests.cs` — all `RequiresRhino`

Tests `OPWKinematics` forward and inverse kinematics using `TestHelpers.CreateIRB120OPW()`.

| Test | What it checks |
|---|---|
| `OPWForward_AllZeroAngles_ReturnsExpectedPose` | FK at zero angles matches known IRB120 TCP position and orientation |
| `OPWForward_TooFewJoints_ThrowsException` | Fewer than 6 joint values throws |
| `OPWInverse_KnownPose_ContainsCorrectSolution` | IK of a known pose contains a solution matching the expected joint angles within tolerance |
| `OPWForwardInverseRoundTrip_ReturnsOriginalAngles` | FK then IK returns the original joint angles |

### `Kinematics/RobotKinematicParametersTests.cs` — no Rhino

| Test | What it checks |
|---|---|
| Constructor tests | A1/A2/A3/B/C1/C2/C3/C4 assigned correctly |
| `IsValid` checks | Valid parameters pass, degenerate values fail |
| `Duplicate` checks | All parameters copied correctly |

---

## Controllers

### `Controllers/ControllerGrantTests.cs` — no Rhino, requires `RobotComponents.ABB.Controllers`

Tests the fail-closed behaviour of `Controller` methods when the controller is empty (not connected to a real ABB controller). The `DemandGrant` failure path itself cannot be tested because ABB SDK types are sealed.

| Test | What it checks |
|---|---|
| `UploadModule_EmptyController_ReturnsFalse` | Returns `false`, status message contains `"empty"` |
| `UploadSystemModule_EmptyController_ReturnsFalse` | Returns `false`, status message contains `"empty"` |
| `ResetProgramPointers_EmptyController_ReturnsFalse` | Returns `false`, status message contains `"empty"` |
| `ResetProgramPointer_EmptyController_ReturnsFalse` | Returns `false`, status message contains `"empty"` |

---

## Utilities & Security

### `HelperMethodTests.cs` — partial Rhino dependency

The largest test class. Tests `RobotComponents.ABB.Utils.HelperMethods` and `RobotComponents.ABB.Presets.Utils.HelperMethods`.

#### Quaternion / Plane round-trip — `RequiresRhino`

| Test | What it checks |
|---|---|
| `PlaneToQuaternion_ThenQuaternionToPlane_ReturnsOriginalPlane` | WorldXY-aligned plane survives round-trip within tolerance |
| `PlaneToQuaternion_ThenQuaternionToPlane_RotatedPlane_ReturnsOriginalPlane` | 45° rotated plane survives round-trip |
| `IdentityQuaternion_ProducesWorldXYPlane` | q=(1,0,0,0) produces WorldXY plane |
| `QuaternionToPlane_WithComponentOverload_MatchesQuaternionOverload` | Both overloads agree |
| `QuaternionToPlane_XYZOverload_SetsCorrectOrigin` | Origin set correctly from x/y/z scalar parameters |

#### FlipPlane — `RequiresRhino`

| Test | What it checks |
|---|---|
| `FlipPlaneX_NegatesXAxis_OriginUnchanged` | X axis negated, origin and Y axis unchanged |
| `FlipPlaneY_NegatesYAxis_OriginUnchanged` | Y axis negated, origin and X axis unchanged |
| `FlipPlaneX_FlipsNormal` | Normal reverses when X axis is flipped |

#### Slerp — 3 `RequiresRhino`, 2 plain

| Test | Rhino | What it checks |
|---|---|---|
| `Slerp_AtZero_ReturnsFirstQuaternion` | ✅ | Result equals q1 at t=0 |
| `Slerp_AtOne_ReturnsSecondQuaternion` | ✅ | Result equals q2 (or -q2) at t=1 |
| `Slerp_AtHalf_ReturnsMidpointRotation` | ✅ | Result is 45° rotation for 0°→90° interpolation at t=0.5 |
| `Slerp_ClampsNegativeT_ToZero` | | t < 0 clamped to t=0 |
| `Slerp_ClampsAboveOneT_ToOne` | | t > 1 clamped to t=1 |

#### Lerp — all plain

| Test | What it checks |
|---|---|
| `Lerp_AtZero_ReturnsFirstQuaternion` | Result equals q1 at t=0 |
| `Lerp_AtOne_ReturnsSecondQuaternion` | Result equals q2 (or -q2) at t=1 |
| `Lerp_ClampsNegativeT_ToZero` | t < 0 clamped to t=0 |
| `Lerp_ClampsAboveOneT_ToOne` | t > 1 clamped to t=1 |

#### DotProduct — `RequiresRhino`

| Test | What it checks |
|---|---|
| `DotProduct_IdenticalQuaternions_ReturnsOne` | Dot product of identical unit quaternions is 1.0 |
| `DotProduct_OrthogonalQuaternions_ReturnsZero` | Dot product of orthogonal quaternions is 0.0 |
| `DotProduct_OppositeQuaternions_ReturnsNegativeOne` | Dot product of opposite quaternions is -1.0 |

#### ReplaceFirst — plain

| Test | What it checks |
|---|---|
| `ReplaceFirst_ReplacesOnlyFirstOccurrence` | Only the first match is replaced |
| `ReplaceFirst_NoMatch_ReturnsOriginal` | No match returns original string |
| `ReplaceFirst_EmptySearch_ReplacesAtStart` | Empty search string inserts replacement at position 0 |
| `ReplaceFirst_ReplaceWithEmpty_RemovesFirstOccurrence` | Replacing with empty string removes first occurrence |

#### SetRapidDataFromString — plain

Tests RAPID declaration string parsing into `IDeclaration` objects via a `DeclarationStub` helper.

| Test | What it checks |
|---|---|
| `SetRapidDataFromString_ParsesVarSpeedData` | Parses `VAR speeddata v100 := [...]` — scope, type, name, and 4 values |
| `SetRapidDataFromString_ParsesLocalScope` | `LOCAL VAR` sets scope to `LOCAL` |
| `SetRapidDataFromString_ParsesTaskScope` | `TASK PERS` sets scope to `TASK`, type to `PERS` |
| `SetRapidDataFromString_ParsesConstVariableType` | `CONST` sets type to `CONST` |
| `SetRapidDataFromString_MultipleEqualSigns_ThrowsInvalidCastException` | Multiple `=` in string throws |
| `SetRapidDataFromString_WrongDatatype_ThrowsInvalidCastException` | Wrong datatype in string throws |

#### IsValidRapidIdentifier — plain (security)

| Test | What it checks |
|---|---|
| `IsValidRapidIdentifier_ClassifiesCorrectly` | `[Theory]` with 12 cases — valid identifiers pass; null, empty, leading-digit, whitespace, semicolon, comma, newline, quote fail |
| `IsValidRapidIdentifier_Exactly32Chars_ReturnsTrue` | Exactly 32 characters is valid |
| `IsValidRapidIdentifier_33Chars_ReturnsFalse` | 33 characters is invalid |
| `IsValidRapidIdentifier_InjectionPayload_ReturnsFalse` | Signal injection payload rejected |
| `IsValidRapidIdentifier_PathTraversalPayload_ReturnsFalse` | `[Theory]` — `../../../test`, `..`, `.` etc. rejected (issue #36) |
| `IsValidRapidIdentifier_ValidModuleName_ReturnsTrue` | `[Theory]` — `MainModule`, `T_ROB1`, `my_module_01` pass |
| `ThrowIfInvalidRapidIdentifier_ValidName_DoesNotThrow` | Valid name does not throw |
| `ThrowIfInvalidRapidIdentifier_InvalidName_ThrowsInvalidOperationException` | Invalid name throws with identifier in message |
| `ThrowIfInvalidRapidIdentifier_Null_ThrowsInvalidOperationException` | Null throws |

#### IsSafeFilePath — plain (security)

| Test | What it checks |
|---|---|
| `IsSafeFilePath_ClassifiesCorrectly` | `[Theory]` with 6 cases — normal paths pass, null/empty fail |
| `IsSafeFilePath_PathWithQuotes_ReturnsFalse` | Quotes in path rejected |
| `IsSafeFilePath_InjectionPayload_ReturnsFalse` | Exact attack from issue #35 rejected |
| `ThrowIfUnsafeFilePath_ValidPath_DoesNotThrow` | Valid path does not throw |
| `ThrowIfUnsafeFilePath_PathWithQuotes_ThrowsArgumentException` | Quotes throw with `"unsafe"` in message |
| `ThrowIfUnsafeFilePath_Null_ThrowsArgumentException` | Null throws |

#### IsPathWithinDirectory / ThrowIfPathEscapesDirectory — plain (security)

| Test | What it checks |
|---|---|
| `IsPathWithinDirectory_SafePath_ReturnsTrue` | Combined path inside base returns `true` |
| `IsPathWithinDirectory_TraversalPayload_ReturnsFalse` | `../../` traversal returns `false` |
| `IsPathWithinDirectory_AbsolutePathOutside_ReturnsFalse` | Absolute path outside base returns `false` |
| `IsPathWithinDirectory_NullBase_ReturnsFalse` | Null base returns `false` |
| `IsPathWithinDirectory_NullPath_ReturnsFalse` | Null path returns `false` |
| `ThrowIfPathEscapesDirectory_SafePath_DoesNotThrow` | Safe path does not throw |
| `ThrowIfPathEscapesDirectory_TraversalPayload_ThrowsArgumentException` | `../../` traversal throws |
| `ThrowIfPathEscapesDirectory_AbsolutePathOutside_ThrowsArgumentException` | Absolute path outside base throws |

### `Utils/SerializationBinderTests.cs` — no Rhino

Tests the custom `SerializationBinder` used for deserializing older saved data (backwards compatibility).

- Known types resolve to the correct `Type`
- Unknown type names throw or return `null` as appropriate

---

## Presets

### `PresetHelperTests.cs` — `RequiresRhino`

Tests `RobotComponents.ABB.Presets.Utils.HelperMethods`:
- Mesh loading from file paths passes through `ThrowIfUnsafeFilePath` before use
- Valid paths do not throw; paths with quotes throw `ArgumentException`

### `RobotPresetTests.cs` — `RequiresRhino`

Tests `Factory.GetRobotPreset()` for a selection of robot models:
- Each preset returns a valid `Robot` (`IsValid == true`)
- Name, axis count, and presence of meshes verified for spot-checked presets (IRB120, IRB1200, etc.)

### `VersionNumberingTests.cs` — no Rhino

| Test | What it checks |
|---|---|
| `CurrentVersion_IsNotEmpty` | `CurrentVersion` string is not null or empty |
| `Version_MajorIsNonNegative` | `Version.Major >= 0` |
| `Version_MinorIsNonNegative` | `Version.Minor >= 0` |
| `Version_BuildIsNonNegative` | `Version.Build >= 0` |
| `CurrentVersion_MatchesVersionObject` | `CurrentVersion` string parses to the same value as the `Version` object |
