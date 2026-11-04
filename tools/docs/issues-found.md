# Issues found while building the wiki

Running list of problems found in Robot Components (and the docs tooling) while building
and verifying wiki examples. Each entry has evidence and a repro so it can be fixed later.
Not published on the wiki site.

**Severity:** 🔴 bug, wrong or unusable output · 🟠 bug, cosmetic/UI · 🟡 usability ·
⚪ tooling/info

| # | Sev | Area | Title | Status |
|---|---|---|---|---|
| 1 | 🔴 | Code generation | Joint Target with a named Robot Joint Position produces RAPID that does not load | open |
| 2 | 🟠 | GH UI | "Draw Full Names" Tier 2 sweep does not rename Move's dynamic inputs | open |
| 3 | 🟡 | Move | *Type* silently defaults to MoveAbsJ when unconnected | open |
| 4 | 🟡 | RAPID Generator | Actions on several branches silently produce several modules | open |
| 5 | 🟡 | Robot Preset | Modal picker dialog on first solve blocks scripted/automated use | open |
| 6 | ⚪ | rapid-verifier | First Virtual Controller cold boot hung past the 210 s deadline | open |
| 7 | ⚪ | Docs tooling | Material for MkDocs reaches end of life in May 2027 | open |
| 8 | 🔴 | Flow control / RAPID Generator | Program order follows data-tree branches, not wire order, once loops/IFs are nested | open |
| 9 | 🟠 | RAPID Generator | Wiring a RAPID Variable's *Variable* output into *Actions* throws a null-reference solution exception | open |
| 10 | 🟡 | For Loop | Auto counter is always `i`, so nested loops without explicit counters shadow each other | open |
| 11 | ⚪ | RAPID Generator | Typo in generated comment: "User definied code lines" | open |
| 12 | 🔴 | Routine Argument | A Robot Target as argument *Value* is written as its display text (`visit Robot Target (pickA_0);`) and never declared | open |
| 13 | 🔴 | Routine Argument / Routine | A list on argument *Value* duplicates the routine body and declares the routine twice; module won't load | open |
| 14 | 🔴 | Message Box | Buttons without wired actions are skipped when numbering `CASE`, so actions run for the wrong button | open |
| 15 | 🔴 | Move / Robot Target / Work Object | A Work Object on a Move does not convert its targets: world coordinates are written and then offset again by the work object | open |

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

---

## 3 🟡 Move *Type* silently defaults to MoveAbsJ when unconnected

**Found:** 2026-11-04. `MoveComponent.SolveInstance`: `if (!DA.GetData(0, ref movementType)) { movementType = 0; }`.

With nothing on *Type*, every movement is `MoveAbsJ`. The only hint is the warning *"An
Absolute Joint Movement instruction is combined with a Robot Target"*, and only if the
target is a Robot Target. Beginners expect MoveL/MoveJ. Upstream behavior.

**Ideas:** a remark when *Type* is unconnected, or an explicit default shown in the
component message.

---

## 4 🟡 Actions on several branches silently produce several modules

**Found:** 2026-11-04. Discontinuity outputs `{0;0}`; a Move with one target outputs `{0}`.
Wiring both into the RAPID Generator gives **two** `MODULE MainModule` texts on the *Module*
output, with no message. Users see one long panel and may upload the wrong module.
Upstream behavior (one module per branch may be intended).

**Ideas:** a remark when *Actions* has more than one branch ("N modules generated, one per
branch; flatten Actions for a single module").

---

## 5 🟡 Robot Preset opens a modal dialog on first solve

**Found:** 2026-11-04 while scripting. `RobotPresetComponent.SolveInstance` calls
`GetRobotPreset()` (a modal form) when the preset is `EMPTY`. Adding the component from code
(or any headless context) freezes Rhino until someone clicks. Worked around by setting the
private `_robotPreset` field via reflection before adding.

**Ideas:** only show the form from the canvas/UI path (e.g. on placement via the toolbar),
or expose a settable preset property for scripting.

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

---

## 9 🟠 RAPID Variable *Variable* output into *Actions* crashes the RAPID Generator

**Found:** 2026-11-04. RAPID Variable → *Variable* (type RAPID Variable) wired into RAPID
Generator → *Actions*: messages "Data conversion failed from RAPID Variable to Action" and
**"Solution exception: Object reference not set to an instance of an object."**, no module.
The correct wire is *Variable Declaration* (Code Line). Easy mistake, since *Variable* is the
first output.

**Fix ideas:** skip null actions in the generator with a clear warning ("RAPID Variable: wire its
*Variable Declaration* output into Actions"), or let `GH_Action` cast a RAPIDVariable to its declaration.

---

## 10 🟡 For Loop auto counter is always `i`

**Found:** 2026-11-04. Two For Loops without a *Counter*, one inside the other, generate
`FOR i … DO FOR i … DO`. It loads (verified), but the inner `i` hides the outer one, so the
outer counter can't be used inside the inner loop, which is the usual reason for nesting.

**Fix ideas:** pick the first free name (`i`, `j`, `k`, …) when nesting is detected, or warn.
Also: `VAR num i;` is declared in the routine, but RAPID declares FOR counters implicitly; harmless.

---

## 11 ⚪ Typo in generated comment

`! User definied code lines` in every module with user declarations (also "definied" in
`Routine.cs` / `RoutineArgument.cs` doc comments).

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
