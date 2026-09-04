# RobotComponents — Claude Code context

## Branch strategy

- **`main`** is the default branch. All pull requests target `main`.
- `ikgeo` is a legacy integration branch and is **not** the default merge target.
- `CodingFeatures` is the active development branch; PRs from it go to `main`.

## Repository

Fork of [RobotComponents/RobotComponents](https://github.com/RobotComponents/RobotComponents) maintained at [jpdrude/RobotComponents](https://github.com/jpdrude/RobotComponents).

- `origin` → `jpdrude/RobotComponents` (working fork)
- `upstream` → `RobotComponents/RobotComponents` (upstream)

## Changing a shipped GH component's parameter shape or type — REQUIRED pattern

Grasshopper components are matched to a saved `.gh`/`.ghx` file's data purely by `ComponentGuid`.
For components that do **not** implement `IGH_VariableParameterComponent`, `GH_ComponentParamServer.Read`
restores parameters *positionally*, reading exactly as many `param_input`/`param_output` chunks as the
**current, freshly-constructed** component now has. If a saved file has fewer chunks than the current
code expects (e.g. because an input/output was added), every missing index throws
`"... parameter chunk is missing. Archive is corrupt."` on load — one per missing chunk, per old
instance. This is true even for a purely *optional* new input/output; "safe to add an optional param to
an existing component" is **not** true in Grasshopper and must never be assumed.

Any change to an already-shipped component that adds/removes a parameter, or changes a parameter's
underlying concrete type (e.g. `Param_String` → `Param_GenericObject`), is a **breaking serialization
change** for every `.gh` file that already contains that component. **Never** try to work around this
with runtime reconciliation logic in `SolveInstance`/`Read` (detecting a stale param and swapping it via
`ScheduleSolution`/`ExpireSolution`) — that path is fragile (reentrant-solve risk) and was tried and
reverted in this project.

The correct, only-supported fix is the pattern already used ~150 times in `RobotComponents.ABB.Gh/Obsolete/`
(`v0` through `v4`, continue with the next `vN` folder for new changes):

1. Copy the component's file **exactly as it was before your change** into
   `RobotComponents.ABB.Gh/Obsolete/vN/<ComponentName>_OBSOLETE.cs` (append `2`, `3`, ... if that name is
   already taken by an earlier obsolete snapshot).
2. In that copy: move it to `namespace RobotComponents.ABB.Gh.Obsolete`, rename the class to
   `<ComponentName>_OBSOLETE`, add `[Obsolete("This component is OBSOLETE and will be removed in the
   future. Use <ComponentName> instead.", false)]` on the class, set `Exposure => GH_Exposure.hidden` and
   `Obsolete => true`, and — critically — **keep its `ComponentGuid` exactly as it was**. Nothing else
   about this file may ever change again.
2. In the live component, make your change freely (add/remove/retype parameters, whatever's needed), and
   give it a **brand new `ComponentGuid`** (`[guid]::NewGuid()`), since it's now a distinct component as
   far as GH is concerned. No self-check/reconciliation logic is needed in the live component — a fresh
   GUID has no legacy saved data to be compatible with, so it can assume its own current shape
   unconditionally, exactly like a brand-new component.

Existing `.gh` files keep loading the frozen `_OBSOLETE` class exactly as before (hidden from the
toolbar, so it can't be newly placed); new placements get the live component. Users with old files must
replace the component instance to pick up the change — same as swapping in any new component.

### Also add an `IGH_UpgradeObject` so GH's own "Upgrade Components" can do that replacement

Alongside the `_OBSOLETE` snapshot, add a matching upgrader class in
`RobotComponents.ABB.Gh/Upgraders/vN/<ComponentName>Upgrader.cs` (same `vN` folder name as the
`Obsolete` snapshot for that change) implementing `Grasshopper.Kernel.IGH_UpgradeObject`
(`Version`, `UpgradeFrom` = the old/obsolete guid, `UpgradeTo` = the new live guid, and
`Upgrade(IGH_DocumentObject target, GH_Document document)`). GH auto-discovers it the same way it
discovers components — no registration needed, just a public parameterless constructor. This lets a
user run GH's built-in Solution → "Upgrade Components" to swap every old instance in a file for the
live one, with wires reconnected automatically, instead of manually replacing each one.

Inside `Upgrade(...)`: construct a new live-component instance (not yet in a document), migrate each
input/output's wires from the old instance onto the matching new one via
`GH_UpgradeUtil.MigrateSources(oldParam, newParam)` (inputs) / `GH_UpgradeUtil.MigrateRecipients(oldParam, newParam)`
(outputs) — verified via IL decompilation to be **pure wire-only** migration (moves `Sources`/`Recipients`
list entries, safe regardless of whether the param's type changed) — then finish with
`GH_UpgradeUtil.SwapComponents(oldComponent, newComponent, false)` (the `false` is required: `true` would
call `ReplaceInputParameters`/`ReplaceOutputParameters`, which **transplant the param object itself**
rather than just its wires, silently carrying a stale param type onto the new component for any
parameter whose type changed). See `RobotComponents.ABB.Gh/Upgraders/v5/UpgradeHelpers.cs` and its
sibling upgrader classes for the established pattern, including how to read an old instance's
menu-toggled mode (array/index on or off) off which of its named params are currently registered, and
put a freshly-constructed new instance into the matching mode *before* migrating wires (via an
`internal ConfigureForUpgrade(...)` hook on the component, see `AssignVariableValueComponent`/
`RAPIDVariableComponent`) so the params to migrate onto actually exist.

After a successful swap, also call `UpgradeHelpers.MigrateGroupMembership(oldComponent, newComponent,
document)`. `GH_UpgradeUtil.SwapComponents` only removes/adds the two components themselves — it has no
notion of GH groups, which track membership separately as a list of member `InstanceGuid`s on each
`GH_Group` object (an ordinary document object, found by scanning `document.Objects`). Without this
step, upgrading a component that was in a group silently drops the new instance out of that group.
`MigrateGroupMembership` goes through `GH_Group.InstanceGuidsChanged(SortedDictionary<Guid,Guid>)` —
the same `IGH_InstanceGuidDependent` notification `GH_Document` itself sends to every group when object
instance guids are remapped (e.g. `GH_Document.MutateAllIds`, used on duplicate/paste) — rather than
editing each group's `ObjectIDs` list by hand.

Each upgrader's XML doc comment carries a "reference list" table of the old→new input/output name,
type and index mapping for that component — write one for every new upgrader; it's what makes the
wire-migration calls in `Upgrade(...)` auditable against the actual param shapes instead of having to
re-derive them from the two component files by hand.

### Check for shared-enum drift into already-frozen Obsolete snapshots

Several components build a right-click/auto-populated dropdown by reflecting directly off a live
enum: `HelperMethods.CreateValueList(this, typeof(SomeEnum), index)`. An `_OBSOLETE` snapshot that
does this (grep the `Obsolete/` tree for `CreateValueList(this, typeof(` to find them all) still
references that **same, live** enum type — it was never given its own frozen copy. If that enum
later gains a new member, every existing `_OBSOLETE` snapshot that reflects off it will silently
start offering that new option too, even though the snapshot's own `SolveInstance` switch/logic —
frozen at the time it was written — has no case for it. The result isn't a load-time crash; it's
worse: the component loads fine, the user picks the new-looking option, and it silently emits
incomplete/wrong RAPID code with no error or warning.

Check this in **both** directions, every time:
- **Adding a member to an existing enum**: grep `Obsolete/` for `CreateValueList(this, typeof(<ThatEnum>)`.
  For every match, replace it with `CreateValueList(this, new List<string> { ... }, index)` listing
  exactly the member names that enum had *before* your change (i.e. exactly the cases that
  snapshot's own switch already handles) — freezing the dropdown to match the frozen logic behind it.
- **Freezing a new `_OBSOLETE` snapshot** whose `SolveInstance` calls `CreateValueList(this, typeof(...), index)`:
  that reflection call is itself already a latent instance of this bug against *future* growth of
  that enum — pin it to a hardcoded name list the same way, in the same commit that creates the
  snapshot, rather than waiting to discover it later.

This was missed initially when `SignalType` gained `PersistentData` (both the v5 and v7 obsolete
Connect Interrupt snapshots drifted) and had to be fixed after the fact — do this check as a
standard part of every Obsolete/vN change, not just when something reminds you to.

**When this pattern is applied, remind the user to bump the MINOR (second) version number** in
`RobotComponents/VersionNumbering.cs` (`CurrentVersion` and `Version`) — e.g. `1.3.8` → `1.4.0` — per the
versioning rule already documented in that file ("MINOR version when you add functionality in a
backwards compatible manner"). Do **not** change the version number yourself — only flag that it's due;
the user bumps it.
