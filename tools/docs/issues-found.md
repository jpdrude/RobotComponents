# Issues found while building the wiki

Running list of problems found in Robot Components (and the docs tooling) while building
and verifying wiki examples. Each entry has evidence and a repro so it can be fixed later.
Not published on the wiki site.

**Severity:** 🔴 bug, wrong or unusable output · 🟠 bug, cosmetic/UI · 🟡 usability ·
⚪ tooling/info

| # | Sev | Area | Title | Status |
|---|---|---|---|---|
| 1 | 🔴 | Code generation | Joint Target with a named Robot Joint Position produces RAPID that does not load | fixed (`fix/wiki-issues`) |
| 2 | 🟠 | GH UI | "Draw Full Names" Tier 2 sweep does not rename Move's dynamic inputs | fixed (`fix/wiki-issues`) |
| 3 | 🟡 | Move | *Type* silently defaults to MoveAbsJ when unconnected | by design (documented) |
| 4 | 🟡 | RAPID Generator | Actions on several branches silently produce several modules | by design (documented) |
| 5 | 🟡 | Robot Preset | Modal picker dialog on first solve blocks scripted/automated use | fixed (`fix/wiki-issues`) |
| 6 | ⚪ | rapid-verifier | First Virtual Controller cold boot hung past the 210 s deadline | open |
| 7 | ⚪ | Docs tooling | Material for MkDocs reaches end of life in May 2027 | open |
| 8 | 🔴 | Flow control / RAPID Generator | Program order follows data-tree branches, not wire order, once loops/IFs are nested | fixed (`fix/wiki-issues`) |
| 9 | 🟠 | RAPID Generator | Wiring a RAPID Variable's *Variable* output into *Actions* throws a null-reference solution exception | fixed (`fix/wiki-issues`) |
| 10 | 🟡 | For Loop | Auto counter is always `i`, so nested loops without explicit counters shadow each other | fixed (`fix/wiki-issues`) |
| 11 | ⚪ | RAPID Generator | Typo in generated comment: "User definied code lines" | fixed (`fix/wiki-issues`) |
| 12 | 🔴 | Routine Argument | A Robot Target as argument *Value* is written as its display text (`visit Robot Target (pickA_0);`) and never declared | fixed (`fix/wiki-issues`) |
| 13 | 🔴 | Routine Argument / Routine | A list on argument *Value* duplicates the routine body and declares the routine twice; module won't load | by design (documented) |
| 14 | 🔴 | Message Box | Buttons without wired actions are skipped when numbering `CASE`, so actions run for the wrong button | fixed (`fix/wiki-issues`) |
| 15 | 🔴 | Move / Robot Target / Work Object | A Work Object on a Move does not convert its targets: world coordinates are written and then offset again by the work object | fixed: warning (`fix/wiki-issues`) |
| 16 | 🟡 | For Loop | The *Counter* output can't be used inside the same loop's body (recursive data stream) | by design (documented) |
| 17 | 🟠 | Move | A missing/failed Target throws a misleading "Joint Target cannot be combined with MoveL" exception, and the null crashes the generator | fixed (`fix/wiki-issues`) |
| 18 | 🔴 | Path Acceleration Limitation | Generates `PathAccelerationLimitation …;`, which is not RAPID; the module doesn't load (upstream too) | fixed (`fix/wiki-issues`) |
| 19 | 🔴 | Configuration Data | Negative quadrant values (cf1/cf4/cf6 = -1, -2, …) are treated as "unset" and clamped to 0 | fixed (`fix/wiki-issues`) |
| 20 | 🟠 | Multi Move | Sync Move On/Off and Wait Sync Task throw "After Solution exception" on every solve; duplicate sync IDs are never checked | fixed (`fix/wiki-issues`) |
| 21 | ⚪ | README | Installation section contradicts the release notes (Rhino 7 / Package Manager vs. Rhino 8 only / GitHub zip) | fixed (`fix/wiki-issues`) |
| 22 | 🔴 | Connect Interrupt | Analog interrupt **with** a Signal Value writes `ISignalAI sig, 2.5, int;`, which doesn't load | fixed (`fix/wiki-issues`) |
| 23 | 🟠 | Simulation | Path Generator / Timed Path Generator cache ignores movement type, speed, zone, …: stale path and program time | fixed (`fix/wiki-issues`) |

---

## 1 🔴 Joint Target with a named Robot Joint Position produces RAPID that does not load

**Found:** 2026-11-04, Move page example. **Also in upstream** (`JointTarget.ToRAPID()` is identical in `upstream/main`).

**What happens:** Robot Joint Position (name `home_rj`) → Joint Target (name `home`) → Move (MoveAbsJ) generates

```
VAR jointtarget home := [home_rj, [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];
VAR robjoint home_rj := [0, 0, 0, 0, 30, 0];
```

The controller rejects the module: load error 40322, syntax/semantic error at the `home_rj`
reference (line 8, col 30). Swapping the declaration order does not help. A `VAR`
initializer must be a constant expression, so it cannot reference another `VAR`.

The Robot Joint Position *Name* input is required, so **every** Joint Target built from a
Robot Joint Position hits this.

**Repro:** `docs/examples/part-a/move-square.gh`: wire a Joint Target between Robot Joint
Position and the lower Move. Check the output with the rapid-verifier (`verify_syntax`) or load it on any controller.

**Workaround (documented on the Move page):** wire the Robot Joint Position straight into
Move → *Target*; it is then written inline: `MoveAbsJ [home_rj, [9E9, …]], …` loads and
runs (checked: ok, 6.2 s).

**Fix ideas:** in `JointTarget.ToRAPID()` (`RobotComponents.ABB/Actions/Declarations/JointTarget.cs:353`),
write the robjoint/extjoint *values* inline when the jointtarget itself is declared, instead of their names;
or declare referenced robjoint/extjoint as `CONST`.

**To check with the same fix:** a named External Joint Position inside a Joint Target, and
a Robot Target with a named Configuration Data (same pattern may apply).

**Fixed (2026-11-05, `fix/wiki-issues`):** confirmed for Robot Target too: `VAR robtarget p :=
[…, cfg, ej];` fails with 40322 at `cfg` (line 4, col 54). `JointTarget.ToRAPID(bool useNames)` and
`RobotTarget.ToRAPID(bool useNames)`: declarations (`ToRAPIDDeclaration`) write the values of named
Robot Joint Position / External Joint Position / Configuration Data; `ToRAPID()` (inline in
instructions, Offs, …) still uses the names, which is valid there. The named parts are still declared.
New output (`VAR jointtarget home := [[0, 0, 0, 0, 30, 0], [9E9, …]];`, `VAR robtarget p := [[400, 0,
400], [0, 0, 1, 0], [0, 0, 0, 0], [9E9, …]];`) plus `MoveAbsJ home` / `MoveJ p` and the inline-by-name
forms run on the VC. Unit tests added (JointTarget, RobotTarget; not run here, need Rhino).
Upstream has the same code.

---

## 2 🟠 "Draw Full Names" Tier 2 sweep does not rename Move's dynamic inputs

**Found:** 2026-11-04. Move declares `OptionalParameterDefaults`, so the Tier 2 listener
(`RobotComponentsPriority.OnCanvasFullNamesChanged`, commit 4965d3dd) should flip them.

**What happens:** after switching *Draw Full Names* on, Move's *Target*, *Speed Data* and
*Zone Data* inputs keep their nicknames `TA`, `SD`, `ZD`.

**Evidence:**
- The user toggled *Draw Full Names* on from the Grasshopper menu with two Moves on the
  canvas. Afterwards `TA/SD/ZD` were still short (seen in the canvas screenshot).
- Controlled repro by script: `CentralSettings.CanvasFullNames = false` → add a new Move
  (`TY,TA,SD,ZD`) → `CentralSettings.CanvasFullNames = true` → still `TY,TA,SD,ZD`.
  (`TY` is expected here: it is a fixed parameter that GH's own menu conversion handles, and
  setting the property doesn't run that.)

**Repro by hand (to confirm):** Draw Full Names off → place Move → Draw Full Names on →
check the *Target* input label.

**Fix ideas:** check that `CanvasFullNamesChanged` actually fires on toggle and that
`SweepDrawFullNames` reaches IGH_VariableParameterComponent params (Move registers them via
`Params.RegisterInputParam` at runtime). Compare with a component where the sweep does work.

**Fixed (2026-11-05, `fix/wiki-issues`):** the event fires and the sweep runs; it just never looked
at these parameters. Move's `OptionalParameterDefaults` left out Target, Speed Data and Zone Data
(and both Path Generators left out "Path") on the assumption that GH's own conversion covers them.
It doesn't: GH converts a parameter only if its NickName still matches a freshly constructed
reference instance, and these constructors apply the *current* preference (`ApplyFullNamesPreference`),
so the reference already says "Target" and the real "TA" looks user-renamed. Found by constructing
every GH_RobotComponent with the preference off and on: exactly these five parameters differ and
were missing from the lists. All are listed now. Checked in Rhino: built with full names off →
sweep(full) gives Target / Speed Data / Zone Data / Path, sweep(short) gives TA / SD / ZD / P back,
a hand-renamed "myTarget" is left alone. (The issue's script repro set the property only; it fires
the same event, so that wasn't the cause.)

---

## 3 🟡 Move *Type* silently defaults to MoveAbsJ when unconnected

**Found:** 2026-11-04. `MoveComponent.SolveInstance`: `if (!DA.GetData(0, ref movementType)) { movementType = 0; }`.

With nothing on *Type*, every movement is `MoveAbsJ`. The only hint is the warning *"An
Absolute Joint Movement instruction is combined with a Robot Target"*, and only if the
target is a Robot Target. Beginners expect MoveL/MoveJ. Upstream behavior.

**Ideas:** a remark when *Type* is unconnected, or an explicit default shown in the
component message.

**Decision (2026-11-05): by design.** On the canvas *Type* can't stay unconnected: after the first
solve `_add` is true, and every solve with `SourceCount == 0` creates and connects a *Movement Types*
value list (`MoveComponent.cs:143`), also after deleting it; files keep the wire. The list starts at
MoveAbsJ, which is intended. Only a Move in a document other than the active canvas stays unconnected
(`CreateValueList` adds to `Instances.ActiveCanvas.Document`): scripted/headless builds like the wiki
example scripts (where this was found), possibly clusters (not checked). No error added (it would only
flash on placement). Wiki: Move gotcha rewritten as a note, B9 row removed.

---

## 4 🟡 Actions on several branches silently produce several modules

**Found:** 2026-11-04. Discontinuity outputs `{0;0}`; a Move with one target outputs `{0}`.
Wiring both into the RAPID Generator gives **two** `MODULE MainModule` texts on the *Module*
output, with no message. Users see one long panel and may upload the wrong module.
Upstream behavior (one module per branch may be intended).

**Ideas:** a remark when *Actions* has more than one branch ("N modules generated, one per
branch; flatten Actions for a single module").

**Decision (2026-11-05): by design.** One module per branch is Grasshopper's normal data matching
(same reasoning as #13). Since #8 the flow-control components no longer add branches, so this only
happens with the user's own split data (e.g. Moves from Discontinuity points). Documented on the RAPID
Generator page ("Several modules instead of one") and in B9; no remark added.

---

## 5 🟡 Robot Preset opens a modal dialog on first solve

**Found:** 2026-11-04 while scripting. `RobotPresetComponent.SolveInstance` calls
`GetRobotPreset()` (a modal form) when the preset is `EMPTY`. Adding the component from code
(or any headless context) freezes Rhino until someone clicks. Worked around by setting the
private `_robotPreset` field via reflection before adding.

**Ideas:** only show the form from the canvas/UI path (e.g. on placement via the toolbar),
or expose a settable preset property for scripting.

**Fixed (2026-11-05, `fix/wiki-issues`):** the picker only opens when the component is in the active
canvas's document (placement from the toolbar, right-click → Pick Robot Preset). In any other
document it adds the remark "No robot preset picked yet. Right-click → Pick Robot Preset." and
outputs nothing. Checked in Rhino with a separate document: no dialog, remark shown. The canvas
path is unchanged (not testable from a script without blocking Rhino).

---

## 6 ⚪ rapid-verifier: first cold boot hung

**Found:** 2026-11-04. `start_controller` on `IRB1200_5_90_NoTool`: status stayed
`starting`, then `failed` with C0040402 "Controller boot exceeded the 210000 ms hard
deadline". A second `start_controller` was ready after 13 s. (RapidMCPServer project.)

---

## 7 ⚪ Material for MkDocs end of life

Material for MkDocs 9.x reaches end of life on **2027-05-05** (build prints a notice).
Its successor, Zensical, reads the existing `mkdocs.yml`. The custom hook
`tools/docs/rc_links.py` will need checking during the move.

---

## 8 🔴 Program order follows data-tree branches, not wire order, once loops/IFs are nested

**Found:** 2026-11-04, row-loop tutorial (`docs/examples/part-b/row-loop.gh`).

**What happens:** Move / RAPID Variable output on branch `{0}`; For Loop and IF Statement
output on `{0;0}`; a For Loop fed with another For Loop outputs `{0;0;0}`. The RAPID Generator's
*Actions* must be flattened to get one module (see #4), and Grasshopper then orders the
merged data **by branch path first, wire order second**. Wiring
`[declaration, MoveAbsJ, MoveJ, outer FOR, IF]` produced `… MoveJ, IF, FOR j (FOR i …)`: the
IF moved before the loop. That's silently the wrong program.

Also affected (found 2026-11-04, interrupt example): **Code** and **Connect Interrupt** output on `{0;0}`, so a Wait Time wired *after* Connect Interrupt was generated *before* `CONNECT`.

The flat example (one loop + one IF) only comes out right by coincidence: both are `{0;0}`
and keep their wire order.

**Workaround (verified):** flatten the *outputs* of For Loop / While Loop / IF Statement (or every
action source) instead of the generator's input; everything is then on `{0}` and wire order holds.

**Fix ideas:** have flow-control components output their action list on the same path as
their input branch (no extra level), or have the RAPID Generator ignore branch structure for
ordering (e.g. treat all *Actions* as one list in wire order unless multi-module is intended).

**Fixed (2026-11-05, `fix/wiki-issues`):** cause is Grasshopper itself: a list output of a
component with any item-access input gets the iteration index appended inside `SetDataList`
(tested with native Series / Shift List: `{0}` → `{0;0}`, `{0;1}` → `{0;1;0}`), while
`DA.ParameterTargetPath` still returns the un-appended path. New `HelperMethods.SetActionList`
sets the list with `SetDataTree` on `ParameterTargetPath`; used by For Loop, While Loop, IF
Statement, Message Box, Code, Comment, Connect Interrupt (*Connect Code*) and Routine (*Routine
Call*). Checked in Rhino (new build loaded from bytes): everything on `{0}`, also nested loops;
an unflattened RAPID Generator writes one module in wire order (`MoveAbsJ, MoveJ, FOR (FOR), IF,
code, comment, WHILE`; the old build put the IF before the FOR); a While Loop body with a Move
and an IF is one loop. Module passes `verify_syntax`. Several items on one branch (Code with 3
texts) stay on that branch in item order; separate input branches stay separate. Existing files
that flatten these outputs are unaffected (flattening `{0}` is a no-op). Behaviour change: a
producer fed several items on one branch no longer grafts them into separate branches.

**Follow-up (2026-11-05):** `SetDataTree` appends, so a component whose solve runs again inside the
running solve (Routine creating its value lists on first placement) wrote its actions twice or three
times (seen in the demo file: `go [...]` ×3 until F5). `SetActionList` now empties the output on
iteration 0 before writing. Checked on a canvas: Routine call once right after the first solve;
3 lines on one branch and two separate branches unchanged.

---

## 9 🟠 RAPID Variable *Variable* output into *Actions* crashes the RAPID Generator

**Found:** 2026-11-04. RAPID Variable → *Variable* (type RAPID Variable) wired into RAPID
Generator → *Actions*: messages "Data conversion failed from RAPID Variable to Action" and
**"Solution exception: Object reference not set to an instance of an object."**, no module.
The correct wire is *Variable Declaration* (Code Line). Easy mistake, since *Variable* is the
first output.

**Fix ideas:** skip null actions in the generator with a clear warning ("RAPID Variable: wire its
*Variable Declaration* output into Actions"), or let `GH_Action` cast a RAPIDVariable to its declaration.

**Fixed (2026-11-05, `fix/wiki-issues`):** the RAPID Generator removes null items from *Actions* and
warns ("Some actions are invalid and were skipped. Check the error and warning messages to resolve
them."; first version named this cause, shortened on request). Checked in Rhino:
RAPID Variable *Variable* + a Code line → warning, module with the Code line.

---

## 10 🟡 For Loop auto counter is always `i`

**Found:** 2026-11-04. Two For Loops without a *Counter*, one inside the other, generate
`FOR i … DO FOR i … DO`. It loads (verified), but the inner `i` hides the outer one, so the
outer counter can't be used inside the inner loop, which is the usual reason for nesting.

**Fix ideas:** pick the first free name (`i`, `j`, `k`, …) when nesting is detected, or warn.
Also: `VAR num i;` is declared in the routine, but RAPID declares FOR counters implicitly; harmless.

**Fixed (2026-11-05, `fix/wiki-issues`):** an auto counter takes the first of i, j, k, l, m, n that no
`FOR … FROM` line in the loop's own body uses. Nested loops are built inside out, so the outer loop sees
the inner's counter. Checked in Rhino: two levels → `FOR j … FOR i`, three → `k / j / i`, inner with
explicit `j` → outer `i`; the Counter output reports the chosen name; the generated module runs on the VC.

**Decision (2026-11-05):** kept as is, although it reverses the usual convention (outer `i`, inner `j`).
The inner loop is solved first and can't know it will be nested; renaming afterwards or late binding in
the generator were judged too fragile/large. Documented on the For Loop page ("the innermost loop counts
with `i`"), with explicit RAPID Variable counters for the conventional order.

---

## 11 ⚪ Typo in generated comment

`! User definied code lines` in every module with user declarations (also "definied" in
`Routine.cs` / `RoutineArgument.cs` doc comments).

**Fixed (2026-11-05, `fix/wiki-issues`):** "definied"/"undefinied" → "defined"/"undefined" in all 12
files: the generated comment (`! User defined code lines`), the error messages of Joint Target,
External Joint Position and Forward Kinematics, and doc comments.

---

## 12 🔴 Robot Target as Routine Argument *Value* becomes display text

**Found:** 2026-11-04 building the Routine page example.
`RoutineArgumentComponent.SolveInstance` (`RobotComponents.ABB.Gh/.../Declarations/RoutineArgumentComponent.cs`, around
`value = valueObject.ToString();`) turns the value into text with `ToString()`. For a Robot
Target goo that is `Robot Target (pickA_0)`, so the routine call becomes
`visit Robot Target (pickA_0);`, which is not RAPID. The target is also never declared, because
nothing registers it with the generator (a Move would).

**Repro:** Routine Argument (Type `robtarget`, Name `p`, Value ← Robot Target named `pickA`) on a
Routine; wire the Routine's *Routine Call* into the RAPID Generator.

**Fix ideas:** resolve the value the way RAPID Variable's *Value* does (declared name, or
inline RAPID value if unnamed) and make sure named declarations reach the generator.
**Workaround used in the wiki:** pass numbers (or text) and build targets inside the routine
with Offset Target.

**Fixed (2026-11-05, `fix/wiki-issues`):** new `HelperMethods.ResolveRAPIDArgumentValue`: like
`ResolveRAPIDValueExpression`, but Robot Target / Joint Target / Robot and External Joint Position /
Configuration Data are written as their values (Robot/Joint Target via `ToRAPID(false)` from #1), so
no declaration is needed. Used by Routine Argument (instead of `ToString()`) and Routine Call (which
had the related gap: a named target resolved to an undeclared name). `INOUT`/`PERS` parameters still
need a RAPID Variable (documented). `visit [[400, -100, 300], …];` into `PROC visit(robtarget p)`
runs on the VC. Component check in Rhino (new build installed): Routine Argument and Routine Call
with Robot Target `pickA` both give `go [[400, -100, 300], [0, 0, 1, 0], [0, 0, 0, 0], [9E9, …]];`, and
the generated module runs on the VC.
The shared resolver was deliberately not changed: tool/wobj in CRobT, Offs and Assign need names.

---

## 13 🔴 A list on Routine Argument *Value* duplicates the routine

**Found:** 2026-11-04. Routine Argument *Value* = two numbers (`-150`, `150`). The Routine's
*Routine Call* output correctly gives `visit -150;` and `visit 150;`, but the argument's
*Variable* output now has two items too. Everything in the body that uses it runs twice
(every Move appears twice), and **`PROC visit(num y)` is declared twice**. Verified: a module with
two identical `PROC visit` declarations fails to load (40322).

**Workaround used in the wiki:** leave *Value* empty on the Routine Argument (it only defines
the parameter) and make the calls with a separate **Routine Call** component whose
*Argument Value* gets the list.

**Fix ideas:** make *Variable* a single item independent of *Value* (it is the parameter, not
the call), or document that *Value* is only for a single self-call.

**Decision (2026-11-05): by design.** A list on an item input runs the component once per item;
one routine per value is the Grasshopper behaviour users expect. A fix (Value as list input, first
value + warning) was implemented and reverted for that reason. Documented on the Routine and Routine
Argument pages (and B9): for several calls use Routine Call. Note for later: Access is not stored in
.gh files (param_input chunk: Description, InstanceGuid, Name, NickName, Optional, SourceCount), and
skipping iterations with an item input still writes a null item per skipped iteration.

---

## 14 🔴 Message Box: `CASE` numbers shift when a button has no actions

**Found:** 2026-11-04, operator-loop example (`docs/examples/part-b/operator-loop.gh`).
Buttons `Yes`, `No`; only *Actions No* wired (`again := FALSE`). Generated:

```
msgBoxAnswer := UIMessageBox(... \BtnArray:=["Yes", "No"]);
TEST msgBoxAnswer
CASE 1:
    again := FALSE;     ! runs when the operator presses YES
ENDTEST
```

`UIMessageBox` returns 1 for the first button, 2 for the second. The *No* actions belong in
`CASE 2`. Cause: `MessageBoxComponent.SolveInstance` only adds a button's action list when
`DA.GetDataList(i, list)` succeeds, and `answerCounter` counts the collected lists, not the
button positions. Silent wrong behaviour on the robot.

**Workaround (documented on the Message Box page):** wire something (e.g. an Empty Line) into
every button's *Actions* input. Then `CASE 1:` (empty) / `CASE 2: again := FALSE;` is generated
(syntax checked on the VC).

**Fix:** add an empty list for unwired buttons (keep the index), or emit `CASE {i - _staticInputCount + 1}`
from the parameter index. Empty buttons can then be omitted from the TEST entirely.

**Fixed (2026-11-05, `fix/wiki-issues`):** every button input now gets a list (empty if unwired,
nulls removed), and the `CASE` number is the button's position. Buttons without actions get no
`CASE`; with no actions at all, no `TEST` is written. Checked in Rhino with the new build loaded
from bytes: Yes/No with only No wired → `CASE 2:` only; A/B/C with A and C wired → `CASE 1:` /
`CASE 3:`; none wired → only the `UIMessageBox` line. All three shapes pass `verify_syntax` (IRB
1200, RW 6.10). No parameter change, so no Obsolete snapshot needed.

---

## 15 🔴 Work Object on a Move doesn't convert the target coordinates

**Found:** 2026-11-04, tool & work object example (`docs/examples/part-a/pen-on-table.gh`).
Work Object `table` (plane at (350, −150, 150), rotated 30°), square drawn **in that plane**,
Robot Targets from those planes, Move with *Override Work Object* = `table`. Generated:

```
PERS wobjdata table := [..., [[350, -150, 150], [0.966, 0, 0, 0.259]]];
VAR robtarget p_0 := [[350, -150, 150], ...];          ! world coordinates
MoveL p_0, v100, fine, pen\WObj:=table;                ! ...interpreted in the table frame
```

On the controller `p_0` is then measured **from the table origin**, so the point lands at
at (728, -105, 300) world instead of (350, −150, 150): the table offset and rotation are applied a second time. Nothing warns. Only when the Robot Target gets
*Reference Plane* = the work object's plane are the coordinates written relative to it
(`[[0, 0, 0], ...]`, `[[100, 0, 0], ...]`), which is correct and verified on the VC.

Probably upstream behaviour (Reference Plane is the intended mechanism), but it's a very easy
and dangerous mistake: the simulation and the real robot disagree.

**Fix ideas:** when a Move has a Work Object and its Robot Target has no Reference Plane, either
convert using the work object's plane automatically, or warn ("target is in world coordinates
but used with work object X").

**Fixed with a warning (2026-11-05, `fix/wiki-issues`):** correction to the text above: the
simulation does **not** disagree with the robot. `Movement.GetGlobalTargetPlane` applies the work
object too, so the Path Generator shows the same doubled offset; the trap is that the points are
not where they were drawn. A `RobotTarget` doesn't keep whether a Reference Plane was used
(the plane is converted in the constructor), so the Move can only guess:
`MoveComponent.LooksLikeWorldCoordinates` flags a target that is closer to the work object's origin
than half its distance to the world origin (and than half the work object's offset); work objects
within 50 mm of the world origin are skipped. Checked with the pen-on-table geometry: all four
world-coordinate corners and a 50 mm approach point warn; the same points relative to the table, and
long local paths (500, 0, 0) / (600, 200, 0), don't; a real Move with a world target warns, with the
Reference Plane set it doesn't. Auto-converting was not done: it would change the RAPID of every
existing file that uses relative targets on purpose.

---

## 16 🟡 For Loop *Counter* output can't feed the loop's own body

**Found:** 2026-11-04, array example (`docs/examples/part-b/array-offsets.gh`). For Loop *Counter*
→ Get Array At Index *Index* → Offset Target → Move → back into the same For Loop's *Actions*:
Grasshopper reports *"Recursive data stream found, this component depends on itself"*, and
nothing is generated. The body of a loop is an **input** of the loop, so the loop's outputs can
never feed it. Reading the counter inside the body is the main reason to have it, so the
*Counter* output is only usable *after* the loop.

**Workaround (documented):** create the counter as a RAPID Variable (`num`, Level Routine) and
wire it into both the loop's *Counter* input and the body components, or type the counter
name (`i`) into a panel.

**Fix ideas:** document that the *Counter* output is for use after the loop, or drop it. The
auto-created `i` could be exposed differently (e.g. a separate "Loop Counter" component that
only outputs the name).

**Decision (2026-11-05): by design.** The body is an input of the loop, so any feedback from the loop's
own output is a cycle Grasshopper never allows. The For Loop is meant for sequences with a known number
of repetitions, using its counter by name; for everything else users create their own RAPID Variables
(counter used in several places, counts that persist, condition-based repetition with While Loop). The
For Loop page now has a "When to use it" section saying so; the Counter output is documented as "for
after the loop".

---

## 17 🟠 Move: misleading exception when the Target is missing

**Found:** 2026-11-04 (same session, twice). When the *Target* input fails to collect data (upstream
error, recursive stream, or not yet solved), Move falls back to a default joint target and then
throws *"Solution exception: Invalid Move instruction: A Joint Target cannot be combined with a
MoveL, MoveJ or MoveC instruction."*. The real problem (no target) isn't mentioned, and the
resulting null in the RAPID Generator gives a second *"Object reference not set…"* exception.

**Fix ideas:** if *Target* has no data, add an error "Target input has no data" and return early
(no output) instead of falling back to a joint target. Make the generator skip nulls with a warning (see #9).

**Fixed (2026-11-05, `fix/wiki-issues`):** Move: *Target* wired but without data → warning "The Target
input has no data. Check the component that feeds it.", no movement. Unconnected *Target* keeps the
default joint target for MoveAbsJ; MoveL/MoveJ/MoveC with a Joint Target (or none) → error
"MoveL, MoveJ and MoveC need a Robot Target…" instead of the exception. Generator skips the null (#9).
Checked in Rhino: all four cases (wired-empty, MoveL unconnected, MoveAbsJ unconnected → `MoveAbsJ
[[0, 0, 0, 0, 0, 0], …]` as before).

---

## 18 🔴 Path Acceleration Limitation writes an instruction that doesn't exist

**Found:** 2026-11-04. Acceleration limit TRUE / 2 m/s², deceleration FALSE generates

```
PathAccelerationLimitation TRUE\AccMax:=2, FALSE;
```

The controller rejects it: load error 40322, syntax/semantic error at that line. The RAPID
instruction is **`PathAccLim`**: `PathAccLim TRUE \AccMax:=2, FALSE;` loads (verified).
Source: `RobotComponents.ABB/Actions/Instructions/PathAccelerationLimitation.cs:104`,
`return $"PathAccelerationLimitation {acceleration}, {deceleration};";`. **Upstream has the same line**
(`upstream/main`, line 180).

**Workaround (documented):** a Code component with the `PathAccLim` line.
**Fix:** emit `PathAccLim`. Check whether the Deconstruct/serialization keeps working (the class name can stay).

**Fixed (2026-11-05, `fix/wiki-issues`):** `ToRAPIDInstruction` writes `PathAccLim`. The class,
component, Deconstruct and serialization are unchanged (only the RAPID text changed). All four
combinations as generated (`FALSE, FALSE` / `TRUE\AccMax:=2, FALSE` / `FALSE, TRUE\DecelMax:=3` /
`TRUE\AccMax:=2.5, TRUE\DecelMax:=4`) run on the VC (`simulate_program`: ok). Unit test
`BothDisabled_ProducesFalseFalse` updated. Worth reporting upstream.

---

## 19 🔴 Configuration Data clamps negative quadrants to 0

**Found:** 2026-11-04. Introduced in the fork by `bbe4c31b` (2025-11-05, "implements constructing
configuration data from quadrant data inputs"). `ConfigurationDataComponent.SolveInstance`:

```
cf1 = cf1 < 0 ? 0 : cf1;   (same for cf4, cf6)
```

and `-1` doubles as "not provided". Results (CFX unconnected):
`(1, 0, -1)` → `[1, 0, 0, 4]`, `(-1, 0, 1)` → `[0, 0, 1, 1]`, `(0, -1, 0)` → `[0, 0, 0, 0]`.
In RAPID, quadrant numbers are commonly negative (axis 1 at −45° is `cf1 = -1`). With this
component a target on the robot's −Y side can't get its correct configuration, and with
`ConfL\On` the move fails or the robot picks another configuration. Upstream passes all four
values through unchanged.

Also to check: the CFX "bitmask" built from `cf > 0` flags matches the GoFa/CRB 15000
convention it was written for, but not ABB's cfx definition for other robot families.

**Fix ideas:** use a nullable/optional input or a separate sentinel (e.g. leave the input empty)
for "not provided" instead of `< 0`; never clamp provided values; only compute CFX when the
robot is a CRB 15000 (or document which robots it's for).

**Fixed (2026-11-05, `fix/wiki-issues`):** the component mixed up two things. cf1/cf4/cf6 are
axis quadrants (`floor(angle / 90)`, negative is normal); cfx is the 3-bit posture (bit 2 wrist
center behind axis 1, bit 1 behind the lower arm, bit 0 axis 5 negative), the same order
`IkGeoSolver` sorts its solutions by (its local `Cf1/Cf4/Cf6` are those bits, not quadrants,
which is probably where the mix-up came from). Neither can be derived from the other.
Configuration Data now passes all four through (defaults 0, a remark if CFX is outside 0..7)
with a new GUID; the old one is frozen as `Obsolete/v8/ConfigurationDataComponent_OBSOLETE`
with `Upgraders/v8/ConfigurationDataComponentUpgrader` (wires + typed values, old `-1` "unset"
becomes 0). New component **Configuration Number** builds cfx from Shoulder/Elbow/Wrist
booleans. Bit meanings checked on the VC with `CRobT` (a5 −30° → cfx 1, a2 −90° → 4,
a3 −100° → 6; the four in-limit IK solutions of one target read back as cfx 0, 1, 6, 7 matching
Robot Components' IK order). Effect of the old clamping shown on the VC: from axis 6 at −309.8°,
`MoveJ` to the same target with `cf6 = 0` instead of `-4` turns axis 6 a full revolution; `MoveL`
stops with 50442. Wiki: new primer section *Arm configuration: confdata* with pose renders.

---

## 20 🟠 Multi Move components throw after every solve

**Found:** 2026-11-04. Sync Move On, Sync Move Off and Wait Sync Task each show
*"After Solution exception: Object reference not set to an instance of an object."*. Their RAPID
output is still correct (`SyncMoveOn sync2, allTasks;` etc.).

**Cause (verified in source):** the fork's refactor `9919b80e` ("unify RAPID outputs as
Param_Action") changed their output to `Param_Action`, but `AfterSolveInstance` still does
`_tree = this.Params.Output[0].VolatileData as GH_Structure<GH_SyncMoveOn>;` (resp.
`GH_SyncMoveOff`, `GH_WaitSyncTask`). The cast returns null, so `_tree.Branches` throws. As a
side effect the duplicate-name check for sync IDs (`UpdateVariableNames` /
`ObjectManager.CheckVariableNames`) never runs. Upstream still uses the typed params, so it's
fork-only. A grep over all `Param_Action` components found only these three.

**Fix:** cast to `GH_Structure<GH_Action>` and read the sync ID from
`(item.Value as SyncMoveOn)?.SyncID` (etc.), like the other refactored components do.

**Fixed (2026-11-05, `fix/wiki-issues`):** no other component had a typed `_tree` on a `Param_Action`
output, so there was no pattern to copy. New `HelperMethods.GetTypedActionTree<T, TGoo>` rebuilds the
typed tree from the `GH_Action` items (same paths, same objects, so the SyncID renaming still changes
the output), used by all three components. Checked in Rhino (new .gha from bytes): no exception,
RAPID unchanged; two Sync Move On with `sync1` → the second warns *"The variable name "sync1" is
already in use."* (the typo "aleady" in `ObjectManager` fixed on the way); one component with two IDs
→ `sync1_0`, `sync2_1` as before. The intended use is one shared component per sync point (as in the
Multi Move tutorial), so the warning doesn't fire on correct definitions.

---

## 21 ⚪ README installation text is out of date

**Found:** 2026-11-04 while aligning the wiki with the v1.4.5 release notes. `README.md` says
*"If you are using Rhino 7 or higher, you can install Robot Components via the Package Manager"*
and to copy files into the Components Folder. The release notes (`INSTALL.md`, generated by
`CreateRelease.ps1`) say: **Rhino 8 only**, uninstall any Package Manager version first, unblock
the zip, copy to `%appdata%\Grasshopper\Libraries`. The wiki's Getting Started now follows the
release notes.

**Fix:** update the README's *Installation* section to match `INSTALL.md` (or link to it).

**Fixed (2026-11-05, `fix/wiki-issues`):** the README no longer describes the installation; it points
to the releases page (each release carries its own instructions) and the wiki's Getting started.

---

## 22 🔴 Connect Interrupt: analog signal with a Signal Value doesn't load

**Found:** 2026-11-04 (wiki review). With *Signal Type* analog input/output and a *Signal Value*,
`ConnectInterruptComponent` writes `ISignalAI aiPressure, 2.5, int_p;` (`analogTrigger` is just
the value, line ~170). `ISignalAI`/`ISignalAO` need *Condition, HighValue, LowValue, DeltaValue*:
the Virtual Controller rejects the module with a syntax/semantic error at that line (RobotWare
6.10). Without a value the component writes `AIO_ALWAYS, 0, 0, 0`, which loads, and so does
`ISignalAI aiPressure, AIO_ABOVE_HIGH, 2.5, 0, 0, int_p;` (both checked).

**Fix ideas:** write `AIO_ABOVE_HIGH, <value>, 0, 0` for a single value, or add a condition input
(above high / below low / between / outside) with high and low values, like `WaitAI`'s
*Inequality*. Remember the obsolete-snapshot pattern if inputs change.

**Fixed (2026-11-05, `fix/wiki-issues`):** a single *Signal Value* on AI/AO now writes
`AIO_ABOVE_HIGH, <value>, 0, 0` ("rises above"); no inputs changed, so no obsolete snapshot. The
input description says so; other conditions via Code (documented). Output of the new build checked
in Rhino (AI with/without value, AO, DI unchanged); `ISignalAI … AIO_ABOVE_HIGH, 2.5, 0, 0, …` and the
AO variant pass `verify_syntax`. The frozen v5/v7 obsolete snapshots keep the old line (frozen by
design; Upgrade Components moves files to the live component).

---

## 23 🟠 Path Generator caches stale paths after a movement type or speed change

**Found:** 2026-11-05 while writing the Path Generator Try-it solutions. In the Move square,
switching the square's Move from `MoveL` to `MoveJ` left the path exactly straight (0.0 mm off
the edges) and *Program Time* at 0.096 min. Switching its speed to `v50` left the time at
0.099 min. Only after nudging *Interpolations* (5 → 6 → 5) did it recalculate: MoveJ edges 8.6–9.9
mm off the straight line, approach 73 mm, time 0.395 min at `v50`.

**Cause (verified in source):** `CreateInputHash` in `PathGeneratorComponent.cs` (added in
`78c2c321`, fork-only) hashes a `Movement` only by `movement.Target.ToRAPID()`. Movement type,
speed data, zone data, *Time*, work object, tool override and the DO of MoveLDO/MoveJDO are not in
the hash, so `(update && inputHash != _lastInputHash) | !_calculated` stays false and the old
`PathGenerator` result is reused. `TimedPathGeneratorComponent.CreateInputHash` has the same gap,
which is worse there because its step timing depends on the speed. Toggling *Update* or F5
(Recompute) doesn't help: `_calculated` stays true and the hash is unchanged.

**Fix:** hash `movement.ToRAPIDInstruction(robot)` (covers type, speed, zone, tool, work object,
time and DO) plus the target's RAPID, or simply `MovementType`, `SpeedData.ToRAPID()`,
`ZoneData.ToRAPID()`, `Time`, `WorkObject.ToRAPID()`, `RobotTool?.ToRAPID()`; same in the Timed
component.

**Fixed (2026-11-05, `fix/wiki-issues`):** shared `HelperMethods.GetPathRelevantHash(action, robot)`
used by both components. Movement: type, target, circle point, speed (name + data), time, zone,
work object, tool, DO. Override Robot Tool: tool name + data. Any other instruction: its RAPID line
(so Velocity Set, Acceleration Set, … now also trigger a recalculation). Checked in Rhino with a
square: installed (old) build keeps 0.0 mm / same time after MoveL → MoveJ; new build: MoveJ edges
12.3 mm off straight, time 5.56 → 5.80 s, v200 → v50 5.80 → 23.20 s; Timed Path Generator v200 → v50:
16 → 61 steps at 0.1 s, 4.56 → 18.24 s.
