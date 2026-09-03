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

**When this pattern is applied, remind the user to bump the MINOR (second) version number** in
`RobotComponents/VersionNumbering.cs` (`CurrentVersion` and `Version`) — e.g. `1.3.8` → `1.4.0` — per the
versioning rule already documented in that file ("MINOR version when you add functionality in a
backwards compatible manner"). Do **not** change the version number yourself — only flag that it's due;
the user bumps it.
