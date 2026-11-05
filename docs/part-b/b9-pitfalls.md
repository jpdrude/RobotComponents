---
title: "B9 Pitfalls and limits"
description: Every trap found while building this wiki, in one checklist, with the fix for each.
---

# B9 Pitfalls and limits

Everything on this page was found while building and checking the examples in this wiki,
and each item links to the page that explains it. Read it once before you run Part B code on
a robot.

## Programs that are silently wrong

These produce code that loads and runs, but **does the wrong thing**, with no warning in
Grasshopper.

| Trap | Fix | Details |
|---|---|---|
| **Work Object on a Move doesn't convert targets.** World coordinates get offset by the work object a second time. The [[Move]] warns when a target looks like world coordinates. | Give the [[Robot Target]] the work object's plane as *Reference Plane*. | [[Work Object]] |
| **Nested loops share the counter `i`.** | Give the outer [[For Loop]] its own [[RAPID Variable]] counter. | [[For Loop]] |
| **Move *Type* unconnected = MoveAbsJ.** | Always connect [[Movement Types]]. | [[Move]] |

## Programs that don't load

The controller rejects the module, usually with load error 40322 and a line number.

| Trap | Fix | Details |
|---|---|---|
| A `VAR` declaration whose value refers to another `VAR`, e.g. a [[RAPID Variable]] whose *Value* is another variable. | Use a literal or a `CONST` as the initial value, and assign the other variable later with [[Assign Variable Value]]. | [Primer](b0-rapid-primer.md#data-three-kinds-of-storage) |
| A list of values on a [[Routine Argument]] declares the routine twice. | Leave *Value* empty; make the calls with [[Routine Call]]. | [[Routine]] |
| A direct call to a routine in a module that [[Load Module]] only loads later. | Give the [[Routine Call]] the *Module Name*: the call is then late-bound. | [[Routine Call]] |
| A tool switched with [[Override Robot Tool]] that isn't declared anywhere. | Declare the tool on the controller (system module) or in the module. | [[Override Robot Tool]] |
| Signal names the controller doesn't know. | Copy the exact names from the controller's I/O configuration. | [Set outputs](../part-a/a4-io-and-timing/set-outputs.md) |
| Names that aren't valid RAPID identifiers. | Letters, digits and `_`, starting with a letter, at most 32 characters, no RAPID keywords. | [Primer](b0-rapid-primer.md) |
| The same name used twice (two targets both called `p`). | [[Check Variable Names]] (on by default) warns *"The variable name … is already in use"*. | [[Check Variable Names]] |

## Things that fail in Grasshopper

| Trap | Fix | Details |
|---|---|---|
| Several `MODULE MainModule` texts instead of one. | Flatten (see above). | [[RAPID Generator]] |
| *"Data conversion failed from RAPID Variable to Action"* and a solution exception. | Wire *Variable Declaration*, not *Variable*, into *Actions*. | [[RAPID Variable]] |
| *"Recursive data stream found"* around a [[For Loop]]. | Don't wire the loop's *Counter* output into its own body; use a [[RAPID Variable]] counter. | [Array tutorial](b8-tutorials/array-offsets.md) |
| *"A Joint Target cannot be combined with a MoveL…"* on a [[Move]] that has no joint target. | The Move's *Target* got no data; fix whatever feeds it. | [[Move]] |
| *"Routine variables cannot be global. Scope set to LOCAL."* | Level `Routine` data is always local. Set Scope `LOCAL`. | [[RAPID Variable]] |

## What the simulation can't know

[[Path Generator]], [[Timed Path Generator]], [[Inverse Kinematics]] and [[Check Actions]]
compute everything in Grasshopper. They **can't** evaluate what only exists on the
controller:

- variables and expressions (`Offs(base, 0, i * 100, 0)`, `CRobT()`, values from
  [[Numeric Entry Box]]),
- which branch an [[IF Statement]] or [[Message Box]] takes, or how often a [[While Loop]] runs,
- what a TRAP routine does when an interrupt fires,
- **collisions** with anything, even in Part A.

Test such programs in RobotStudio or on a Virtual Controller, then on the real robot at reduced speed.

## Opening older files

When a component's inputs or outputs change between versions, older `.gh` files keep the old,
hidden version so they still open. To switch to the current one, use Grasshopper's
*Solution → Upgrade Components*. It replaces the old instances and reconnects the wires.

## See also

[B0 RAPID primer](b0-rapid-primer.md) · [B8 Tutorials](b8-tutorials/index.md)
