# Changelog

## All notable changes to this modified version of Robot Components are documented here.

### Changelog 
 Generated on: 2026-09-11 11:55 
 --- 
 - MessageBoxComponent: generate TEST/CASE/ENDTEST instead of chained IF/ENDIF Per explicit request, with the exact RAPID syntax to match: TEST msgBoxAnswer CASE 1: <button 1 actions> CASE 2: <button 2 actions> ENDTEST Replaces the previous per-button "IF msgBoxAnswer = n THEN ... ENDIF" chain with a single TEST msgBoxAnswer / CASE n: / ENDTEST block. msgBoxAnswer only ever holds one value at a time, so this is both the more idiomatic RAPID construct for the pattern and matches conventional RAPID code for UIMessageBox/msgBoxAnswer. RAPID's TEST has no fall-through between cases (unlike a C-style switch), so no BREAK statement is needed or emitted. 
 - TEST/CASE/ENDTEST sit at the same indentation level (matching the sample RAPID exactly); each button's actions are indented one level in, same as the bodies of the old IF blocks were. 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged; no test coverage exists for this component's generated code (GH_Component classes aren't unit tested in this project; verification is manual, in Grasshopper itself). 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `54462e9` | **Date:** 2026-09-11 
 
 --- 
 
 - Collapse RC fix actions into one Solution-menu item, gated by guid presence Per explicit request: a whole top-level "Robot Components" menu was too much for two related actions. Collapsed FixOptionalParameterNames and FixComparisonOperatorSymbols into a single "Fix RC Parameter Names" menu item that runs both sweeps and invalidates the canvas once at the end, and moved it into GH's own Solution menu, directly under Upgrade Components, instead of a standalone top-level menu. 
 - Found "Upgrade Components" by its WinForms Name, not its displayed Text -- confirmed via IL that GH's own designer code sets ToolStripItem.Name to the literal, hardcoded string "mnuUpgradeComponents" (GH_DocumentEditor's own mnuSolution/mnuUpgradeComponents accessor properties exist but are, like MainMenu, internal to Grasshopper.dll -- "assembly" visibility, confirmed via IL), which is far more stable to match against than localizable, GH-version-dependent display text would be. ToolStripItemCollection.Find(..., searchAllChildren: true) locates it regardless of nesting. Falls back to adding a small top-level "Robot Components" menu instead, so the feature stays reachable even if some future GH version renames/restructures that item. 
 - The item is only enabled when the active document actually contains something it could apply to: a HashSet<Guid> of "every GH_RobotComponent type in this assembly whose OptionalParameterDefaults is non-empty" (built once via reflection, each candidate type instantiated through its required public parameterless constructor -- the same precondition GH's own component discovery already relies on) plus GH_ValueList's own guid, checked against document.Objects on the Solution menu's DropDownOpening. 
 - Deliberately a cheap guid-presence check, not the fuller content-matching each fix performs on an object before actually changing it. 
 - Build: 0 errors, no new warnings. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged; pure GH-UI behavior with no automated coverage in this project, verification is manual, in Grasshopper itself. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `07a5aec` | **Date:** 2026-09-11 
 
 --- 
 
 - Fix Robot Components menu actions not repainting until an unrelated redraw Reported: after running "Fix Optional Parameter Names", the relabeled parameters only actually appeared on canvas after clicking into it afterward. 
 - ExpireLayout()/ExpireSolution() (used by the sweep and the value-list relabeling respectively) only mark the affected objects' own layout/state stale for whenever they're next drawn -- neither repaints anything itself. 
 - GH's own "Draw Full Names" menu handler follows its equivalent conversion with an explicit canvas.Invalidate() call (confirmed via IL decompilation during the original investigation); the live CentralSettings. 
 - CanvasFullNamesChanged listener gets that for free since it runs *before* GH's own conversion and Invalidate() in that same click handler, but the two standalone "Robot Components" menu actions have nothing else to trigger a repaint afterward. 
 - Fixed by calling Instances.ActiveCanvas.Invalidate() at the end of both menu actions (only when something actually changed, for FixComparisonOperatorSymbols). 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged; pure GH-UI behavior with no automated coverage in this project, verification is manual, in Grasshopper itself. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `3d9d31d` | **Date:** 2026-09-11 
 
 --- 
 
 - Fix Multi Relay: custom renames only actually protected NickName, not Name Reported: rename an empty input by hand, then wire something into it -- the type-derived auto-rename fires anyway and clobbers the custom name. 
 - Root cause, confirmed via IL decompilation of Grasshopper.dll: GH_ObjectEventType has no "NameAccepted" member at all -- only NickName and NickNameAccepted. A parameter's canvas-interactive rename can only ever change its NickName; Name is purely a property this component itself assigns in code and a user cannot edit it directly, ever. Both the input "is this still ours to auto-rename" check and the output "is this still ours to mirror" check compared against .Name -- a property that, by construction, never actually changes from user interaction. So neither check could ever detect a real rename: the user renames NickName, Name stays exactly as this component last set it, "stillOurs"/"lastMirrored" both stay (wrongly) true, and the next wire connect or input rename overwrites the NickName the user just set. 
 - Fixed by comparing against NickName instead (on both the input auto-name and output mirror tracking), and, the moment a divergence is detected, resyncing Name to match NickName before freezing the slot -- keeping this component's own invariant (Name == NickName for any slot it still manages) intact, rather than leaving Name silently stuck at a stale value forever. 
 - Updated the field-level doc comments (which described the old, incorrect Name-based mechanism) to match. 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged; no automated coverage exists for GH_Component classes in this project (requires a live Rhino host), verification is manual, in Grasshopper itself. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `d20f196` | **Date:** 2026-09-11 
 
 --- 
 
 - Replace native-Upgrade-Components hooks with a custom Robot Components menu Both retroactive-fix mechanisms shipped so far piggy-backed on GH's native "Upgrade Components" command by registering an IGH_UpgradeObject with UpgradeFrom == UpgradeTo (an in-place edit, no swap): first ComparisonOperatorValueListUpgrader (targeting GH_ValueList's own shared native guid), then a proposed batch of ~15 more for Draw Full Names (targeting this project's own live component guids). Decompiled GH_ComponentServer.IsUpgrader -- the check that decides what "Upgrade Components" offers -- and confirmed it's a plain "is an upgrader registered for this guid" lookup, with no per-instance "already fixed" tracking. That means either approach would have permanently flagged every instance of the affected type, in every file, as "upgradeable" forever, even ones already fixed -- for ComparisonOperatorValueList a single native, off-the-beaten-path guid; for Draw Full Names, ~15 of this project's own heavily-used live component guids. Not an acceptable trade for a cosmetic fix. 
 - Replaced with a "Robot Components" entry in GH's own main menu bar instead, holding both fix actions as ordinary on-demand commands with none of that downside: - "Fix Optional Parameter Names" -- runs the same Draw Full Names sweep the CentralSettings.CanvasFullNamesChanged live listener already runs on toggle, but on demand, against the active document. Covers the one documented gap in the live listener: a file opened while the preference is already on/off from an earlier session, where nothing changes this session so the toggle event never fires for it. 
 	 - "Fix Comparison Operator Symbols" -- the same content-matched relabeling ComparisonOperatorValueListUpgrader used to do, now run on demand instead of GH offering it unprompted, forever, on every value list. 
 - Added via Grasshopper.Instances.CanvasCreated (public static event, fires once the first canvas/editor window exists -- confirmed via IL, since PriorityLoad() itself runs before any editor window exists and Instances.DocumentEditor is not valid yet at that point). The menu is attached to Instances.DocumentEditor.MainMenuStrip -- GH_DocumentEditor's own MainMenu property returns the identical control but is internal to Grasshopper.dll (confirmed via IL: "assembly" visibility); MainMenuStrip is the standard, publicly inherited System.Windows.Forms.Form property GH assigns the same control to, with a Controls-search fallback in case a future GH version stops doing that. 
 - The live CentralSettings.CanvasFullNamesChanged listener from the previous commit is unchanged and still fires automatically on an active toggle -- this only adds the on-demand path for everything that isn't. 
 - Removed RobotComponents.ABB.Gh/Upgraders/v8/ComparisonOperatorValueListUpgrader.cs (and the now-empty v8 folder) entirely, superseded by the menu action above. 
 - Build: 0 errors, no new warnings. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged; this is pure GH-UI behavior with no automated coverage in this project, verification is manual, in Grasshopper itself. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `87bc9eb` | **Date:** 2026-09-11 
 
 --- 
 
 - Add Draw Full Names Tier 2: retroactive fix via GH_AssemblyPriority listener Complements the Tier 1 fix already shipped (apply the preference at the moment a dynamic parameter is constructed). Tier 1 can't help a parameter that already exists when the user toggles "Draw Full Names" -- there's no generic "a parameter was just created" event to hook, and construction-time logic only runs at construction time. This adds the missing half: a document-wide sweep that runs exactly when the preference is toggled. 
 	 - GH_RobotComponent gains a virtual instance property, OptionalParameterDefaults, listing the (Name, NickName) pair each dynamically-addable parameter started out with. Defaults to empty; only overridden by components that actually add parameters at runtime. 
 - Deliberately an instance property, not static -- the listener always has a live component instance in hand while walking the document, so there's no need for (and, on .NET Framework 4.8, no language support for) a virtual static member. 
 	 - RobotComponentsPriority (new, GH_AssemblyPriority) subscribes to Grasshopper.CentralSettings.CanvasFullNamesChanged once at plugin load. 
 - On toggle, it walks the active document's GH_RobotComponent instances and, for every currently-registered parameter whose (Name, NickName) still exactly matches an entry in that component's OptionalParameterDefaults, flips NickName between the short and full form -- exactly the guarantee GH's own conversion gives the parameters it can reach, extended to the ones it can't. A name the user has customized, on either side of the toggle, no longer matches its recorded default and is left alone. 
 - Overrides were added to every component with a *finite* set of dynamic parameter defaults (~14 components): the ones built from a fixed pool array (PathGenerator, TimedPathGenerator, ForwardKinematics, InverseKinematics, RAPIDGenerator, ExternalLinearAxis, ExternalRotationalAxis, Move, RobotTarget, ExternalJointPosition -- excluding whichever pool entries are already registered by RegisterInputParams/RegisterOutputParams itself, since those are already covered by GH's native conversion) and the ones built from named constants (AssignVariableValue, RAPIDVariable, ConnectInterrupt, EmptyLine, AdditionalRoutine's Return Type/Return Value). Left at the inherited empty default: components whose dynamic parameters already keep Name and NickName identical (MultiRelay, DecomposeRobtarget, the "bit{n}" params in Set Group Input/Output), where the preference has nothing to expand either way; and components whose dynamic names are an unbounded, parameterized series (IfStatement's ELSEIF pairs, RoutineCall's/ AdditionalRoutine's "Argument N", MessageBox's per-button actions) -- there's no finite list to give for those, so they rely on Tier 1 alone. 
 - For the pool-array components, each override reads (Name, NickName) straight off the live pool objects rather than retyping the literals a second time -- but captured into a snapshot BEFORE that same constructor also runs the Tier 1 loop, since Tier 1 mutates those same live objects' NickName in place if Draw Full Names is already on at construction time; reading them live and unguarded would report the post-Tier-1 (already expanded) NickName as if it were the original default, breaking the retroactive match. 
 - Verified via the same GH_Document/CentralSettings IL decompilation as the Tier 1 investigation: CentralSettings.CanvasFullNamesChanged is public static (fires after the setter updates the field, before GH's own document conversion and canvas repaint in the same menu-click handler), and GH_AssemblyPriority.PriorityLoad() is GH's standard, auto-discovered plugin-load hook (same discovery mechanism as components and IGH_UpgradeObject upgraders -- no registration needed). 
 - Known gap (documented in RobotComponentsPriority's own remarks): this only fires on an active toggle. A file opened while the preference is already on from an earlier session never fires the event, so its parameters keep whatever NickNames they were saved with. 
 - Also verified per explicit request: every one of the 133 live (non-Obsolete) components in RobotComponents.ABB.Gh already inherits from GH_RobotComponent directly -- nothing needed changing there. 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged; this is pure GH-canvas display behavior with no automated coverage in this project, verification is manual, in Grasshopper itself. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `4965d3d` | **Date:** 2026-09-11 
 
 --- 
 
 - Apply Draw Full Names to dynamically-added parameters and upgraded components Root cause (verified via IL decompilation of Grasshopper.dll): "Draw Full Names" is not a live rendering switch. Clicking the canvas menu item toggles CentralSettings.CanvasFullNames and, in the same click, calls GH_Document.ConvertNickNamesToFullNames()/ConvertFullNamesToNickNames() -- a ONE-TIME pass that walks every object as of that moment, emits a bare `new SomeComponent()` reference instance per object via ComponentServer.EmitObject(guid), and copies Name into NickName (or back) for each matched parameter, skipping any whose current NickName no longer matches the reference's default (i.e. already user-customized). 
 - Two consequences, matching exactly what was reported: 1. Parameters registered at runtime (Params.RegisterInputParam/ RegisterOutputParam calls outside RegisterInputParams/RegisterOutputParams -- every right-click "add optional parameter" and zui +/- pattern in this project) don't exist on the bare reference instance used for comparison, so the conversion's parallel attribute-tree walk (capped at Math.Min(realCount, referenceCount) - 1) never reaches them. They stay stuck on their short NickName forever, no matter how many times "Draw Full Names" gets toggled. 
 - 2. A component swapped in via "Upgrade Components" is a freshly constructed `new NewComponent()`, built after the one-time conversion pass already ran (if ever). It starts at its own bare-default NickNames and never gets its own conversion pass. 
 - Fix: two new HelperMethods (ApplyFullNamesPreference(IGH_Param) and an IGH_Component overload) that check CentralSettings.CanvasFullNames and, if on, set NickName = Name -- safe to call unconditionally only at the moment of construction, since there's nothing to overwrite yet. 
 - Applied at every runtime parameter-construction site across ~20 components (folded into each component's own shared factory method where one already existed -- CreateScalarValueParam/CreateArrayValuesParam and similar -- otherwise added inline), and in each of the 7 real component-swap IGH_UpgradeObject implementations (Upgraders/v5-v7), applied to the whole freshly built replacement component right after GH_UpgradeUtil. 
 - SwapComponents succeeds. 
 - Left untouched, deliberately: components whose dynamically-added parameters already keep Name and NickName identical (MultiRelayComponent, DecomposeRobtargetComponent, the "bit{n}" params in Set Group Input/Output, Controller Utility and Code Generation variants) -- Draw Full Names has nothing to expand there, so adding the call would be a pure no-op. 
 - Scope note: this is Tier 1 from the investigation -- forward-looking only. 
 - It does not retroactively relabel optional parameters already sitting in files saved before this fix (their NickName is already baked into the archive); re-running "Upgrade Components" after this fix already handles the upgraded-component case for old files, since that reconstructs a fresh instance. 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged; this is pure GH-canvas display behavior with no automated coverage in this project, verification is manual, in Grasshopper itself. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `b782f61` | **Date:** 2026-09-11 
 
 --- 
 
 - Use actual RAPID comparison symbols in Comparison Operators value lists Both places that auto-create a "Comparison Operators" value list -- ComparerExpressionComponent (on first use) and the standalone ComparisonOperatorValueList dropper component -- built it by reflecting enum member names straight off ComparisonOperator via HelperMethods.CreateValueList(..., typeof(ComparisonOperator), ...), producing items literally named "LT", "GT", "LE", "GE", "EQ", "NE" instead of the actual RAPID symbols. ComparerExpressionComponent had already switched to a hardcoded symbol list for its own auto-created value list (<, >, <=, >=, <>, plus the fullwidth equals sign U+FF1D standing in for '=' -- a literal '=' can't be a GH value list item's display name, since GH_ValueListItem's own serialization uses '=' as a name/expression delimiter) -- but ComparisonOperatorValueList never got the same fix. 
 - Factored that hardcoded list out of ComparerExpressionComponent into a single shared HelperMethods.ComparisonOperatorSymbols, in ComparisonOperator's declared order (LT=0..NE=5), and pointed both components at it. Added a matching HelperMethods.CreateValueList(List <string>, PointF) overload (mirroring the existing Type/Dictionary variants) so ComparisonOperatorValueList's standalone, drop-on-canvas path can use it too. 
 - Also added ComparisonOperatorValueListUpgrader (Upgraders/v8) so files saved before this fix get their existing, already-placed value list relabeled via Grasshopper's own "Upgrade Components" command, per explicit request -- reloading a file does NOT fix this on its own, since the auto-create code only ever runs once (gated on the input having no source yet) and a value list's ListItems are that object's own, independently persisted state. This upgrader is unlike every other one in this project: GH_ValueList is a native Grasshopper type with one guid shared by every value list in existence, so UpgradeFrom targets that shared guid directly, and Upgrade() only actually touches a value list whose 6 items exactly match the old LT/GT/... signature (name AND expression, in order) -- a safe no-op for anything else. It edits the existing live object's item Names in place rather than swapping components (nothing to swap: same object, same wires, same selected item). See the class's own remarks for the one real caveat: GH_ComponentServer keeps upgraders in a dictionary keyed by UpgradeFrom, one per guid, so this could collide with some other plugin's own upgrader for GH_ValueList, if one ever exists. 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged, no automated coverage exists for GH_Component/native-GH-type interactions in this project. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `6d444e1` | **Date:** 2026-09-10 
 
 --- 
 
 - Downgrade PathGenerator/TimedPathGenerator's first-movement notice to a hint Path Generator and Timed Path Generator surfaced "The first movement is not set as an absolute joint movement." as a Warning, lumped in with genuine errors (axis limit violations, singularities, degenerate circular moves) via their shared ErrorText list/loop. RAPIDGenerator had the exact same message until it was split out into a separate RemarksText list and surfaced as a GH Remark (a "hint") in RAPIDGeneratorComponent, since it's advisory -- it doesn't affect the calculated path/generated code, just a heads-up that the robot may need to travel through a non-deterministic path to reach the first target. 
 - Applied the same split to PathGenerator and TimedPathGenerator: added a RemarksText list (field + Clear() + property, mirroring RAPIDGenerator's shape exactly), moved the first-movement message from _errorText.Add(...) to _remarksText.Add(...) in each class's CheckFirstMovement(), and added a matching Remark-level loop in PathGeneratorComponent/TimedPathGeneratorComponent alongside their existing Warning-level ErrorText loop. Every other ErrorText message (axis limits, wrist singularities, circular-movement degenerate cases) is untouched and still surfaces as a Warning. 
 - The optional "Errors" output list on both components (DA.SetDataList(..., ErrorText)) no longer includes this message, matching RAPIDGeneratorComponent -- which never exposed ErrorText/RemarksText as output data in the first place, only as runtime messages. 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged; no existing test asserts on this specific message. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `7a8908c` | **Date:** 2026-09-10 
 
 --- 
 
 - Fix Multi Relay: protect custom output names, fix output misalignment on add/remove Two bugs reported after the last round of Multi Relay fixes: 1. A custom name on an input or output pair didn't stick -- specifically, outputs had NO protection at all. EnsureConsistentState() force-wrote output.Name = input.Name unconditionally on every single solve, with no way for a user-set output name to survive past the next solve. (Inputs already had correct protection via _lastAutoNames/"stillOurs" -- this only affected the output side.) Fixed with a new _lastMirroredOutputNames tracking list, symmetric to the existing input-side one: as long as an output's current Name still matches what we ourselves last mirrored onto it, it's safe to keep mirroring; the moment a user renames it directly, mirroring stops for that slot for good. Persisted via Write/Read so the protection survives reload (old files have no data to protect here, since the old code could never have let a divergent output name reach a save in the first place). 
 - OnParameterNickNameChanged now also reacts to output-side renames immediately, instead of only input-side ones. 
 - 2. Wires got disconnected -- or worse, silently rewired to the wrong stream -- when adding/removing an input/output pair anywhere but the end of the list. Root cause: SyncOutputCount() only compared the COUNT of Params.Output against Params.Input; it had no idea which input index actually changed. Removing input #1 (not the last) caused it to delete the LAST output instead, destroying that output's wire connections, while the output that actually corresponded to input #1 survived untouched but now misaligned with every input after it. Inserting a pair in the middle appended the new output at the END instead of alongside its input, silently shifting every later output's data over by one position (each wire stayed technically "connected", just to the wrong data). This is the exact same class of bug already fixed for the internal _lastAutoNames list in a previous PR, just never applied to the actual Params.Output list itself. 
 - Fixed by having CreateParameter/DestroyParameter -- which already know the exact index GH is inserting/removing at, for the input side -- also create/destroy the matching OUTPUT at that same index, synchronously, right there. SyncOutputCount() is renamed EnsureOutputCountFallback() and kept only as a last-resort, no-positional-guarantee safety net for states this component can't otherwise reason about (e.g. a corrupted archive) -- under normal operation it no longer has anything to do. 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged, no automated coverage exists for GH_Component classes in this project (requires a live Rhino host); verification is manual, in Grasshopper itself. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `187086c` | **Date:** 2026-09-10 
 
 --- 
 
 - Merge pull request #31 from jpdrude/integration/combined-open-changes Add Decompose Robtarget component 
  
   **Commit:** `e007fe7` | **Date:** 2026-09-10 
 
 --- 
 
 - Add Decompose Robtarget component New "Decompose Robtarget" component under Advanced RAPID Features. Takes a robtarget RAPID Variable and produces RAPID expressions that access individual members of its struct, each usable directly wherever a RAPID Expression input is accepted (e.g. wired into Assign Variable Value, or any RAPID Expression parameter). 
 - By default outputs the position: X/Y/Z -> <variable>.trans.x/.trans.y/.trans.z. 
 - Right-click menu toggles add further output blocks, in a fixed order regardless of click order (Position -> Rotation -> Config -> External): - Get Rotation: Q1-Q4 -> <variable>.rot.q1 .. .rot.q4 - Get Config: Cf1, Cf4, Cf6, Cfx -> <variable>.robconf.cf1/.cf4/.cf6/.cfx - Get External: EaxA-EaxF -> <variable>.extax.eax_a .. .eax_f Implementation notes: - Input is a RAPID Variable (Param_RAPIDVariable), not a RAPID Expression -- decomposition only makes sense against a named variable, since the result is itself a member-access expression built from that name. 
 	 - Menu-driven IGH_VariableParameterComponent (no +/- zui), same pattern as AssignVariableValueComponent: three persisted bools track which optional blocks are on; toggling one calls ApplyOutputConfiguration(), which reconciles Params.Output against the full desired list (remove what's no longer wanted, then insert whatever's missing at its correct position). 
 - This stays correct and idempotent regardless of what combination of blocks is already enabled when another one gets toggled. 
 	 - New component, so no Obsolete/Upgrader pair needed (nothing to be backwards compatible with yet). 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests) -- unchanged, since (like GetArrayAtIndexComponent) the expression-building logic lives entirely in the GH component with no separate core-library class to unit test. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `9b5417d` | **Date:** 2026-09-10 
 
 --- 
 
 - Merge pull request #30 from jpdrude/integration/combined-open-changes Fix Multi Relay auto-name tracking desync on mid-list insert/remove 
  
   **Commit:** `9fa7197` | **Date:** 2026-09-10 
 
 --- 
 
 - Fix Multi Relay desyncing its auto-name tracking on a mid-list insert/remove Multi Relay keeps a positional List<string> (_lastAutoNames), index-aligned with Params.Input, recording the auto-assigned name it last gave each input so it knows which inputs are still safe to auto-rename. EnsureConsistentState() kept that list's length in sync with Params.Input.Count by only ever appending nulls at the end when growing, and trimming from the end when shrinking. 
 - That's only correct for a plain append/remove-the-last-one. GH's own zui lets a user insert a new input/output pair in the middle (dropping a wire on the joint between two existing pins, or the canvas's "Insert parameter" menu), and CanRemoveParameter permits removing any input, not just the last. In either case Params.Input changes at an arbitrary index, but _lastAutoNames kept being patched only at the end -- desyncing it from Params.Input by one slot from that point on. Reported symptom: inserting a new pair between two existing ones (0, 1) made the old pair 1 land at index 2 as expected, but on the next update its tracked identity effectively swapped with the new pair, looking like the insert had partially reverted. 
 - Fixed by using the exact index IGH_VariableParameterComponent.CreateParameter/ DestroyParameter already receive: CreateParameter now inserts a matching "not yet assigned" (null) slot into _lastAutoNames at that same index (GH splices the new param into Params.Input at that index right after this call returns), and DestroyParameter removes _lastAutoNames at that exact index instead of always the last one. EnsureConsistentState()'s grow/shrink-at-the- end loops are kept as a fallback for the one case with no positional information at all (recovering _lastAutoNames' length from an archive saved in the pre-rewrite format), and their comments updated to say so. 
 - Build: 0 errors. Tests: 679/679 passed (RobotComponents.Tests). 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_01NrcWFvZFd6RGcSK3ZhKTFh 
  
   **Commit:** `0fafa6f` | **Date:** 2026-09-10 
 
 --- 
 
 - Merge pull request #29 from jpdrude/integration/combined-open-changes Unify Set/Invert Digital Output outputs as Param_Action 
  
   **Commit:** `240d842` | **Date:** 2026-09-09 
 
 --- 
 
 - Unify Set/Invert Digital Output outputs as Param_Action Set Digital Output and Invert Digital Output were the last two Code Generation instruction components still publishing a dedicated output parameter type (Param_SetDigitalOutput, Param_InvertDigitalOutput) instead of the generic Param_Action every other Wait/Set/Pulse instruction component already uses, per the refactor in 9919b80e ("Refactor: unify RAPID outputs as Param_Action, obsolete old params"). 
 - Same pattern, applied identically: - Param_SetDigitalOutput and Param_InvertDigitalOutput move into Obsolete/v1.1/ (the same folder that refactor created) with their Exposure set to hidden and Obsolete to true. Their class name, namespace, and ComponentGuid are all left completely unchanged -- only the file location and those two properties change -- so any existing .gh file's archived output-param chunk still resolves via ComponentServer.EmitObject(thatGuid) to the exact same class, restoring exactly as it always has. 
 	 - SetDigitalOutputComponent and InvertDigitalOutputComponent now register Param_Action instead. Their own ComponentGuid is untouched, and SolveInstance needs no change: GH_Action.CastFrom already accepts any IAction, which SetDigitalOutput/InvertDigitalOutput already implement, so the existing DA.SetData(0, new SetDigitalOutput(...)) call keeps working unchanged. 
 - GH_ComponentParamServer.ReadAllParameterData reconstructs each output param from its own archived type GUID via EmitObject, independent of what the live component's current RegisterOutputParams() would set up -- confirmed via IL earlier this session and empirically true of the ~20 components 9919b80e already did this to (WaitDIComponent's diff from that commit, for one, touches only the RegisterOutputParams line and a using -- ComponentGuid untouched, no companion Obsolete/Upgrader pair for the component itself). Old files load unaffected; newly placed instances of either component get Param_Action. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `4b4145e` | **Date:** 2026-09-09 
 
 --- 
 
 - Merge pull request #28 from jpdrude/integration/combined-open-changes Add Invert Digital Output component 
  
   **Commit:** `f0bb184` | **Date:** 2026-09-09 
 
 --- 
 
 - Update Invert Digital Output icon Replaces the opposing-arrows invert glyph with a circular invert/toggle arrow, keeping the same toggle-switch glyph on the right unchanged. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `a911d6d` | **Date:** 2026-09-09 
 
 --- 
 
 - Add Invert Digital Output component New RAPID code-generation component, modeled on Set Digital Output: takes a digital output name and emits the RAPID InvertDO instruction, which toggles a digital output's current value instead of setting it to an explicit state. 
 - Full stack, following the established pattern: - RobotComponents.ABB.Actions.Instructions.InvertDigitalOutput: the core IAction/IInstruction, emitting "InvertDO {name};". No delay/sync options -- InvertDO takes only the signal name, unlike SetDO. 
 	 - GH_InvertDigitalOutput: Goo wrapper (Action/Instruction casting, byte-array (de)serialization, matching GH_SetDigitalOutput). 
 	 - Param_InvertDigitalOutput: the GH parameter type carrying the instruction between components. 
 	 - InvertDigitalOutputComponent (Code Generation > Instructions): single "Name" text input, one output. Reuses the same name-validation warnings (character limit, leading digit, special characters) as Set Digital Output's component. 
 	 - New 24x24 icons (component + parameter), matching the existing digital-I/O icon language (dark toggle-switch glyph identical to Set/Pulse Digital Output's, paired with a new opposing-arrows glyph for "invert", in place of Set's single arrow and Pulse's waveform). 
 	 - RobotComponents.Tests/Actions/InvertDigitalOutputTests.cs: RAPID output, validity, injection-payload rejection, RAPID declaration emptiness, and Duplicate() coverage, mirroring SetDigitalOutputTests. 
 	 - Documentation.cs: TODO-marked wiki link entries for both the component and its parameter, following the same placeholder pattern already used for Pulse Digital Output pending real doc pages. 
 - InvertDO's RAPID syntax ("InvertDO Signal;", no optional parameters) verified against the ABB RAPID reference; live verification via the rapid-verifier MCP server's virtual controller was inconclusive only because its default IO config has no user-writable DO signal at all -- confirmed by SetDO (an existing, shipped instruction) failing identically against the same signal names, so the failure is an environment/config gap, not a defect in this instruction's syntax. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `6352717` | **Date:** 2026-09-09 
 
 --- 
 
 - Merge pull request #27 from jpdrude/integration/combined-open-changes Merge open changes: Multi Relay component, Connect Interrupt name override, RAPID Generator optional robot, and more 
  
   **Commit:** `f20cd05` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge branch 'feature/multi-relais-component' into integration/combined-open-changes 
  
   **Commit:** `0c57151` | **Date:** 2026-09-04 
 
 --- 
 
 - Fix new zui-added Multi Relay inputs being named "Data" instead of "Input N" Param_GenericObject's own default constructor already sets Name/NickName to "Data"/"D" -- GH's standard default for a generic-object parameter, not an absence of a name. EnsureConsistentState() tells a genuinely new, not-yet-auto-named slot apart from an already-named one purely by checking string.IsNullOrEmpty(input.Name), so a freshly zui-added input always looked "already named" and got frozen on "Data" forever instead of ever receiving its "Input N" placeholder -- the same freeze-in-place default from the previous commit, just triggered by GH's own param default this time instead of an untracked archive entry. 
 - CreateInputParam() now blanks Name/NickName immediately after construction, so that check actually sees an unnamed param, same as the first input RegisterInputParams sets up explicitly. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `06b09b3` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge branch 'feature/multi-relais-component' into integration/combined-open-changes 
  
   **Commit:** `c2ffc28` | **Date:** 2026-09-04 
 
 --- 
 
 - Revert untracked-slot fallback from data-type default to freeze-in-place The previous change made EnsureConsistentState() default an untracked slot's name to whatever type is currently wired into it, on the theory that "no tracked history" meant either a genuinely new input or an unrecoverable legacy-format file, and either way a freshly-derived type name was a reasonable thing to show. 
 - That reasoning doesn't hold for an input that already carries some name: an untracked-but-named slot can also happen for entirely mundane reasons unrelated to a legacy file -- e.g. this component's tracking list still catching up after being moved through more than one serialization-format change, as one specific saved file already had by the time this was tested against it. In that situation, defaulting to the wired type silently clobbered a real, possibly user-set name the instant this branch was hit, which is a strictly worse outcome than the previous "leave it alone" default: reported as "I change the names, and these don't survive a save/load cycle" -- a regression from the one thing this whole line of fixes was actually supposed to guarantee. 
 - Reverted to freezing an already-named slot in place (never touching input.Name here, connected wire or not) -- the same default this component used for the copy/paste bug this session. Read()'s guard against throwing on a genuinely unreadable (pre-rewrite) archive format is unrelated and stays as-is; only what EnsureConsistentState() does with an untracked slot changes back. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `0fa4bd8` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge branch 'feature/multi-relais-component' into integration/combined-open-changes 
  
   **Commit:** `7a86251` | **Date:** 2026-09-04 
 
 --- 
 
 - Fix Multi Relay crash loading files saved before the Write/Read rewrite Read() threw a NullReferenceException on any file saved by the earlier, InstanceGuid-keyed version of this component's serialization. That version wrote "AutoNameCount" and "AutoNameValue" under the same names the current format still uses, but never wrote "AutoNameIsNull" (the current format's per-entry null flag). GH_IO's indexed GetBoolean/ GetString look the requested item up and call straight into it with no null check, so requesting an item that was never written throws -- confirmed via IL decompilation of GH_Chunk.GetBoolean(string,int32). 
 - Read() now checks ItemExists("AutoNameIsNull", i) per entry before trusting it, and leaves the slot unresolved (null) instead of throwing when an archive doesn't have it -- covering both a legacy-format file and any other archive this version can't make sense of. 
 - EnsureConsistentState() already had a branch for an unresolved slot, but it defaulted to freezing the name in place (the copy/paste fix from the previous change). That default no longer fits now that "unresolved" also means "we recovered nothing usable from disk": there's no reliable history to preserve, so instead it now defaults to whatever type is currently wired into that input, falling back to the existing name (if any) or the plain "Input N" placeholder when nothing's connected. A round-tripped reload or copy/paste under the current format still populates _lastAutoNames directly from the archive and never reaches this branch, so today's actual custom-name-preservation behavior is unaffected -- this only changes what happens when there is genuinely no tracked data to preserve. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `ef0eb6d` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/GoFaController' into integration/combined-open-changes 
  
   **Commit:** `1bdf6c1` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/current-robot-target-component' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `3d76fde` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/docs/release-notes-upgrade-hint' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `1803be5` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge branch 'feature/multi-relais-component' into integration/combined-open-changes 
  
   **Commit:** `89100a4` | **Date:** 2026-09-04 
 
 --- 
 
 - Fix Multi Relay custom-name persistence across reload and copy/paste Replace the InstanceGuid-keyed _autoNames dictionary with a positional List<string> (_lastAutoNames), index-aligned with Params.Input, and persist it directly via Write/Read. 
 - The guid-keyed approach broke in two different ways: - A pasted/duplicated/reloaded param gets a fresh InstanceGuid, so the previous fix (freezing via a "" sentinel keyed by that guid) worked for the guid it was written under, but the dictionary itself was never actually persisted to the archive -- only in-memory state -- so a real file reload lost all tracking outright and every restored name showed up untracked and unfrozen. 
 	 - Confirmed via IL decompilation of GH_ComponentParamServer. 
 - ReadAllParameterData / GH_Param<T>.Read(): a variable-parameter component's per-param state (Name, NickName, WireDisplay, ...) is restored straight from the archive for each param, independent of this component's own bookkeeping -- so the fix belongs in what this component persists, not in more guid-tracking logic. 
 - Tracking is now purely positional and, since input order/count is itself part of the archive and restored before VariableParameterMaintenance() runs, stays aligned with Params.Input across a save/reload the same way it already did across a zui +/-. 
 - Write/Read persist _lastAutoNames directly (AutoNameCount + per-index AutoNameIsNull/AutoNameValue), so copy/paste -- which goes through the same IGH_DocumentObject.Write/Read mechanism as a plain save/reload -- now recovers the same tracking state, fixing both reported symptoms (names reset to type on paste, "GUID" shown as a name, and custom names lost on file reload) in one change. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `42db7ca` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/rapid-generator-optional-robot' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `b3989d8` | **Date:** 2026-09-04 
 
 --- 
 
 - Fix optional Robot not actually applying to already-placed RAPID Generator instances The RAPIDGenerator.CreateModule movement-detection logic (including scanning additional routines' actions -- already correctly implemented and covered by 5 existing tests, re-verified while investigating this) was fine. The reported "still doesn't work without a robot" was a separate bug in the GH wrapper: RegisterInputParams sets the Robot input's Optional = true, but that only takes effect for a component placed fresh after this change. 
 - Verified via IL decompilation of GH_ComponentParamServer.ReadAllParameterData (the read path used by variable-parameter components, which RAPIDGeneratorComponent is): each input/output param is reconstructed from its own archived type guid and then has its *own* persisted properties -- Optional included -- restored from the archive verbatim, not re-derived from a fresh RegisterInputParams call. So any instance placed or saved before this branch's change still has Optional = false archived on its Robot input specifically, and GH's own required-input check blocks SolveInstance from ever running when it's unconnected -- regardless of whether Actions has any movements at all, since that check happens before SolveInstance, and my movement-aware error only ever gets a chance to run from inside it. 
 - Fixed by re-asserting Params.Input[0].Optional = true from VariableParameterMaintenance(), which -- per its own SDK doc comment -- fires on Open/Paste/Undo/Redo as well as after zui operations, i.e. before an already-placed instance ever gets a chance to solve. This corrects an old instance's stale archived flag the moment it loads, rather than only new ones. 
 - Build clean (MSBuild), 663/663 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `4becf5b` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/multi-relais-component' into integration/combined-open-changes 
  
   **Commit:** `e8a7788` | **Date:** 2026-09-04 
 
 --- 
 
 - Fix the copy/paste name fix itself: names were still getting overwritten The previous fix (adopt an untracked-but-already-named param's existing name as the new "lastAuto" baseline) had a bug that defeated its own purpose: right after "lastAuto = input.Name;", the very next line checks "stillOurs = input.Name == lastAuto" -- which is now trivially true, since lastAuto was just set to equal input.Name. That made the copied param look like it was still on an auto-assigned name eligible for further auto-renaming, so the immediately following type-detection block (which does fire, since a copied param keeps its wire connected) overwrote the just-preserved name with the connected source's type name on that same pass. Exactly the reported symptom: names reset to the input's type. 
 - Fixed by recording a baseline the param's real Name can never actually equal ("" -- Name is never legitimately set to that anywhere else in this component), rather than the Name itself. That makes "stillOurs" permanently false for a copied/pasted param, so its preserved name -- custom or otherwise -- is left alone from then on. There's no way to tell after the fact whether that name had been a deliberate user rename or an untouched auto-detected one on the original before the copy (the very distinction _autoNames exists to track was itself lost along with the old guid), so this freezes it either way -- the only cost is an untouched auto-detected name no longer following a later type change after a copy, which is a minor, rare tradeoff next to actually preserving renames, which was the point. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `aa178df` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/multi-relais-component' into integration/combined-open-changes 
  
   **Commit:** `6130f30` | **Date:** 2026-09-04 
 
 --- 
 
 - Preserve custom input names across copy/paste on Multi Relay Copying (or duplicating) a Multi Relay instance lost every input's custom rename, resetting them back to "Input N" placeholders. Root cause: the "has this input been manually renamed" tracking is a Dictionary<Guid, string> keyed by each input param's InstanceGuid, persisted through Write/Read -- but a copy/paste gives the copied param a fresh InstanceGuid (so it can coexist with the original) while the rest of its serialized state, including a user's rename, carries over faithfully. The dictionary lookup by the new guid then always missed, and the old "first time seeing this param" branch responded by unconditionally resetting it to a fresh placeholder, discarding the name that was actually still sitting right there on the param. 
 - Fixed by distinguishing the two cases that land in "first time seeing this guid": if the param's Name is empty, it's genuinely brand new (zui-created or the initial one), so it gets the placeholder as before -- including the hidden-wire-display default, which only makes sense for a truly fresh input. 
 - If it already has a name, it's a copy/paste/duplicate whose tracking just hasn't caught up yet: adopt that existing name as the new tracked baseline instead of overwriting it, which also means a wire display a user had deliberately turned back on before copying survives too (it's ordinary serialized state, just like Name). 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `1f0e115` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/multi-relais-component' into integration/combined-open-changes 
  
   **Commit:** `897b50c` | **Date:** 2026-09-04 
 
 --- 
 
 - Mirror an input rename onto its output immediately on Multi Relay Previously a renamed input's matching output only picked up the new name on the next unrelated solve, since nothing prompted a recompute right when the rename itself happened. 
 - GH_ComponentParamServer exposes a ParameterNickNameChanged event, confirmed via IL decompilation to be raised specifically when a parameter rename is *accepted* (GH_ObjectEventType.NickNameAccepted) -- fired only by GH's own interactive-rename-commit and undo/redo machinery, never by code merely assigning .NickName (that setter doesn't raise it at all). Subscribed to it in the constructor; on an accepted input-side rename, immediately calls the existing EnsureConsistentState() (mirrors the name, same as any solve would) followed by ExpireSolution(true), instead of waiting for whatever the next solve happens to be triggered by. 
 - Because the event genuinely can't be re-triggered by our own programmatic renames (verified: not raised by the property setter), there's no reentrancy risk with EnsureConsistentState()'s own input.NickName assignments. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `9e28763` | **Date:** 2026-09-04 
 
 --- 
 
 - Hint at Upgrade Components in the release install instructions INSTALL.md (generated by Generate-InstallInstructions.ps1, prepended to every release's notes by Extract-Changelog.ps1) had no mention of how to move an existing .gh file's old component instances onto the current versions after updating -- the project has ~150 Obsolete/vN + IGH_UpgradeObject pairs set up specifically to make Grasshopper's own Solution -> Upgrade Components do this automatically, but nothing in the release notes ever pointed users at it. 
 - Added an "Updating Existing Definitions" section after the install steps pointing at Solution -> Upgrade Components, and mentioning the obsolete-icon overlay GH already draws on old instances so a user can spot what's affected before running it. Used a plain "->" rather than a Unicode arrow: Windows PowerShell 5.1 misreads non-ASCII characters in .ps1 source without a BOM, which corrupted a literal "->" arrow character in local testing. 
 - Added a matching Pester test asserting the hint is present. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `8677abb` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/multi-relais-component' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `b9f4c57` | **Date:** 2026-09-04 
 
 --- 
 
 - Fix Multi Relay's hidden wire display and harden the minimum-1 seeding; update icon Wire display: GH's own +/- zui insert handler overwrites whatever WireDisplay CreateParameter() sets on a freshly-inserted param with its own "implied" style, right after calling it (verified via IL decompilation of GH_ComponentAttributes' insert-click handler) -- so setting it only inside CreateInputParam() was silently clobbered for every input added via the zui. 
 - Re-asserted it from EnsureConsistentState(), which runs (via VariableParameterMaintenance()) right after that clobber, so it's the last word; only for a param not seen before, so a user who deliberately turns display back on for one input later keeps it. 
 - Minimum 1 input/output: the structural fix (RegisterInputParams/ RegisterOutputParams seeding one pair, CanRemoveParameter refusing to remove the last one) was already correct, but the seeded pair had no Name/NickName set until EnsureConsistentState() backfilled it on the first solve. Set "Input 1" directly at registration time instead, removing any dependency on solve timing for the initial pair's identity. 
 - Also pushed the updated MultiRelay_Icon.png. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `0e2f69a` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/connect-interrupt-name-override' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `8dcdddf` | **Date:** 2026-09-04 
 
 --- 
 
 - Collapse Connect Interrupt's v7/v8 upgrade chain into a single hop The v8 obsolete snapshot (ConnectInterruptComponent_OBSOLETE3, freezing guid FB7FCD2B) and its upgrader existed purely to protect an intermediate shape between the Persistent Data change and the Interrupt Variable Name override -- but that guid never made it to main; it only ever existed transiently across this session's own unmerged branches. No saved .gh file could ever reference it, so freezing it was unnecessary churn. 
 - Deleted v8 entirely. ConnectInterruptComponentUpgrader2 (v7) now upgrades directly from the shape actually shipped on main (guid F77FEF07, unchanged -- verified it still matches main's live component byte-for-byte on the parts that matter) straight to the current live component (guid 774F2525, carrying both changes together). No code changes needed inside Upgrade() itself: it already migrated the 4 original inputs/3 outputs by index, which is exactly right for a direct jump too -- just updated UpgradeTo and the doc comments. 
 - v7's obsolete snapshot already had its Signal Type dropdown pinned to a hardcoded list (done when Persistent Data was added), so nothing needed there. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `7b510e5` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/multi-relais-component' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `632fcc5` | **Date:** 2026-09-04 
 
 --- 
 
 - Always keep at least 1 input/output on Multi Relais; rename to Multi Relay - RegisterInputParams/RegisterOutputParams now seed one pair up front instead of starting empty, and CanRemoveParameter refuses to remove the last remaining input, so the component can never be reduced to 0/0. 
 	 - Renamed MultiRelaisComponent -> MultiRelayComponent throughout (class, file, display Name, icon file + resx/Designer.cs entries): "Relais" is the German/French spelling, "Relay" is the correct English word. Component was never merged/shipped, so this needed no GUID change or Obsolete/vN handling -- same ComponentGuid, purely a naming fix. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `98533fb` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/connect-interrupt-name-override' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `4d0a2a6` | **Date:** 2026-09-04 
 
 --- 
 
 - Add optional interrupt variable name override to Connect Interrupt The generated intnum variable is always named "int_" + TRAP Routine Name, so connecting more than one interrupt to the same TRAP routine (fully legal in RAPID -- multiple distinct intnum variables can each CONNECT to the same trap) produced two colliding VAR intnum declarations for the same name -- a compile error on the controller. 
 - Right-click "Override Interrupt Variable Name" adds an optional text input that, when supplied, is used verbatim as the intnum name instead of the derived default; unconnected/off, behavior is unchanged. The override is validated with the same HelperMethods.IsValidRapidIdentifier check used elsewhere in this project (Controller module/task names), warning rather than silently emitting broken RAPID if it isn't a legal identifier. 
 - Went with an explicit override rather than having RAPIDGenerator auto-detect and rename colliding declarations: the three related lines (VAR intnum, the CONNECT, and the ISignalXX/IPers call) are only linked in ConnectInterrupt's own SolveInstance, not in any structure RAPIDGenerator understands, so coordinated auto-renaming across them would mean fragile text-pattern rewriting or a much larger refactor into a real structured Interrupt action; auto-picked suffixes would also be liable to shift between recomputes depending on solve order, which is exactly the kind of instability you don't want in RAPID variable names you're deploying to a controller. An explicit, user-controlled name matches how every other named RAPID declaration in this project already works. 
 - This turns the component into a (menu-driven only, no +/- zui) variable parameter component to add the optional input without disturbing the 4 fixed ones, which is itself a parameter-restore-mechanism change for an already-shipped component, so it needs the project's Obsolete/vN + IGH_UpgradeObject treatment a third time for this component: - RobotComponents.ABB.Gh/Obsolete/v8/ConnectInterruptComponent_OBSOLETE3.cs: frozen pre-change snapshot of the v7 shape, same guid, hidden + Obsolete = true. 
 - Also pinned its Signal Type dropdown to a hardcoded name list rather than reflecting off the live SignalType enum, per the shared-enum-drift check documented in CLAUDE.md -- doing it now rather than needing to fix it later. 
 	 - Live component: new guid. 
 	 - RobotComponents.ABB.Gh/Upgraders/v8/ConnectInterruptComponentUpgrader3.cs: wires all 4 existing inputs and all 3 outputs across by index (wire-only migration throughout); the new 5th input has nothing to migrate onto it and simply starts off. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `cc1633d` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/multi-relais-component' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `e92eb83` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/docs/claude-md-enum-drift-check' into integration/combined-open-changes 
  
   **Commit:** `1dfe382` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/fix/connect-interrupt-obsolete-valuelist-drift' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `0649aa7` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/rapid-generator-optional-robot' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `026863f` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/connect-interrupt-persistent-data' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `c190967` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge remote-tracking branch 'origin/feature/numentrybox-rapid-expression-initvalue' into integration/combined-open-changes # Conflicts: # CHANGELOG.md 
  
   **Commit:** `c73a0bb` | **Date:** 2026-09-04 
 
 --- 
 
 - Add Multi Relais component New GH component (Utility > Multi Relais, nickname MR): a generic pass-through utility with variable inputs, added/removed via the native +/- zui (same mechanism as Merge/Entwine), where each input gets a matching output that simply relays its tree through unchanged. Purely a canvas tidy-up tool for collapsing a bundle of otherwise-crossing wires through one component; it never touches the data itself. 
 	 - Each new input defaults to a placeholder name ("Input N") and hidden wire display (declutters the inbound wires this component exists to tidy up; users can still turn display back on per-wire, nothing re-hides it). 
 	 - The first time something is wired into an input that still has its placeholder (or a previously auto-detected) name, it's renamed to that source's type name (IGH_Param.TypeName, e.g. "Number", "Brep"); reconnecting a different-typed source later updates it again the same way. 
 	 - The moment a user renames an input by hand, it's excluded from further auto-renaming for good (tracked via a persisted Guid->name dictionary recording what name was last auto-assigned; a mismatch means the user changed it). The matching output always mirrors whatever the input's current name is, auto-detected or manual. 
 	 - Outputs are added/removed by the component itself (SyncOutputCount, from VariableParameterMaintenance) to stay 1:1 with the inputs; the +/- zui only applies directly to the input side. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `35ffebf` | **Date:** 2026-09-04 
 
 --- 
 
 - Document the shared-enum-drift check for Obsolete/vN snapshots Add a required checklist item to the Obsolete/vN pattern write-up: an _OBSOLETE snapshot that builds a dropdown via CreateValueList(this, typeof(SomeEnum), index) references the live enum directly, not a frozen copy. If that enum later gains a member, every snapshot reflecting off it silently starts offering an option its own frozen switch has no case for -- no crash, just silently incomplete/wrong generated code. Document checking both directions (extending an enum; freezing a new snapshot that reflects off one) so this gets caught as a matter of course. 
 - Found and fixed after the fact for SignalType gaining PersistentData (both the v5 and v7 obsolete Connect Interrupt snapshots had drifted); this documents it so it happens automatically going forward instead of needing to be asked for. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `8f2fe0b` | **Date:** 2026-09-04 
 
 --- 
 
 - Freeze the v7 obsolete Connect Interrupt's Signal Type dropdown too Same issue as the v5 snapshot, introduced in this same branch: reflecting off the live SignalType enum (typeof(SignalType)) for the auto-generated dropdown means this component's own Persistent Data addition leaks into this frozen component's dropdown as well, even though its switch statement is (correctly) still frozen at cases 0-5. Replaced with the hardcoded 6-name list the enum had before this branch's change, matching the same fix just applied to the v5 snapshot on fix/connect-interrupt-obsolete-valuelist-drift. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `4996dc1` | **Date:** 2026-09-04 
 
 --- 
 
 - Freeze the obsolete Connect Interrupt's Signal Type dropdown ConnectInterruptComponent_OBSOLETE (v5) builds its Signal Type value list by reflecting off the live SignalType enum (typeof(SignalType)). Today's Persistent Data addition to that enum (PersistentData = 6) leaked straight into this frozen component's dropdown too, since it shares the enum -- but its own switch statement is (correctly, per the freeze rule) still frozen at cases 0-5. Picking the new option on an old, obsolete instance would silently emit incomplete RAPID code (missing the third instruction line) with no warning. 
 - Replaced the typeof(SignalType) reflection with the hardcoded 6-name list the enum had at freeze time, so the dropdown can no longer drift out from under this component's own switch statement regardless of what the live enum grows into later. Same fix needed (and applied separately) in ConnectInterruptComponent_OBSOLETE2 (v7), on the yet-unmerged feature/connect-interrupt-persistent-data branch that introduced the enum member in the first place. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `95cda73` | **Date:** 2026-09-04 
 
 --- 
 
 - Make Robot optional on the RAPID Generator; error only if movements need one Robot is now an optional input on RAPID Generator and on RAPIDGenerator's constructors (a null robot is fine, e.g. for a declaration-only module). 
 - CreateModule scans the actions -- including inside ActionGroups and additional routines -- for a Movement instruction before doing anything else; if it finds one and no Robot was provided, it throws InvalidOperationException with a clear message instead of the NullReferenceException that would otherwise come from dereferencing an absent robot's tool/kinematics deep inside code generation. Movement instructions genuinely cannot be resolved to RAPID code (tool/workobject declarations, robtargets, ...) without a Robot, so this is a hard failure, not a toggleable warning like axis-limit enforcement. 
 	 - RAPIDGenerator: constructors now do robot?.Duplicate() instead of robot.Duplicate(); added ContainsMovement(actions) (recursing into ActionGroups, matching CheckFirstMovement's own ungrouping) and the robot-required check in CreateModule; guarded the two _robot.Tool declaration call sites. 
 	 - RAPIDGeneratorComponent: Robot input now Optional; SolveInstance no longer short-circuits when it's unconnected; CreateModule's call is wrapped in a try/catch that surfaces the exception as a GH runtime Error. 
 	 - 5 new RAPIDGeneratorTests: no robot + no movement succeeds, no robot + top-level movement throws, no robot + movement inside an ActionGroup throws, no robot + movement inside an additional routine throws, and a previously-set Robot cleared back to null behaves the same as never having had one. 
 - No GH component parameter shape changed (Robot stays the same Param_Robot, same index -- Optional is a per-parameter flag restored from each saved component instance's own archived state on load, not a shape change), so this doesn't need the Obsolete/vN treatment. 
 - Build clean (MSBuild), 663/663 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `fb890ea` | **Date:** 2026-09-04 
 
 --- 
 
 - Add Persistent Data signal type to Connect Interrupt Signal Type gains a "Persistent Data" entry (SignalType.PersistentData), which connects the interrupt via RAPID's IPers instead of an ISignalXX instruction: CONNECT pers1int WITH iroutine1; IPers counter, pers1int; - Signal Name now accepts either plain text (for the existing DI/DO/AI/AO/GI/GO modes) or a RAPID Variable (for the PERS variable to monitor in Persistent Data mode), resolved via HelperMethods.ResolveRAPIDValueExpression -- the same handling used everywhere else a value can be either a literal or a RAPID declaration/variable/expression. 
 	 - Signal Value has no equivalent in IPers (it takes no triggering value), so it's now flagged with a runtime warning when connected in Persistent Data mode instead of being silently ignored. 
 - Signal Name's input param type change (Param_String -> Param_GenericObject) is a breaking serialization change for an already-shipped component, so this follows the project's Obsolete/vN + IGH_UpgradeObject pattern a second time for this component (the first was v5, when the Enable/Disable Interrupts outputs were added): - RobotComponents.ABB.Gh/Obsolete/v7/ConnectInterruptComponent_OBSOLETE2.cs: frozen pre-change snapshot of the v5 shape, same guid, hidden + Obsolete = true. 
 	 - Live component: new guid. 
 	 - RobotComponents.ABB.Gh/Upgraders/v7/ConnectInterruptComponentUpgrader2.cs: wires every input/output across by index (wire-only migration throughout). 
 - This is a second upgrade hop after the v5 upgrader -- an instance saved with the original shipped guid needs "Upgrade Components" run twice to reach the current live shape. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `ea2d94b` | **Date:** 2026-09-04 
 
 --- 
 
 - Accept RAPID Variables/Expressions for Numeric Entry Box's Initial Value The Initial Value input only took a literal double, unlike the RAPID-expression handling used elsewhere for numeric inputs (Offs' X/Y/Z, Get Array At Index's Index): changed it from Param_Number to Param_RAPIDExpression, resolved via HelperMethods.CheckRAPIDExpression like those. The range-vs-initial-value sanity-check warning still fires for a literal numeric value, and is skipped (rather than throwing) when Initial Value is a variable/expression that can't be range-checked at solve time. 
 - This changes an already-shipped component's input param type, so it needs the project's Obsolete/vN + IGH_UpgradeObject treatment: - RobotComponents.ABB.Gh/Obsolete/v6/NumEntryBoxComponent_OBSOLETE.cs: frozen pre-change snapshot, original GUID, hidden + Obsolete = true. 
 	 - Live component: new GUID. 
 	 - RobotComponents.ABB.Gh/Upgraders/v6/NumEntryBoxComponentUpgrader.cs: wires every input/output across by index (wire-only migration throughout, per the established convention -- see UpgradeHelpers), so GH's own "Upgrade Components" can swap old instances for the live one automatically. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `5afa661` | **Date:** 2026-09-04 
 
 --- 
 
 - Load system modules into every task directly instead of via AllTask config + restart The previous UploadSystemModule wrote the module to HOME:/Robot Components/System Modules, registered it in the controller's SYS configuration domain for automatic AllTask-shared loading, and warm-restarted the controller on every upload. That restart-on-every-upload has proven unreliable and is disruptive during iteration. 
 - UploadSystemModule now uploads the module the same way UploadModule uploads a regular module (write to the regular temp directory, PutDirectory, then LoadModuleFromFile) and loops that load step over every task on the controller, skipping (not aborting on) any task that's currently running. No restart, no SYS config edits, no HOME: placement. 
 - The module still carries the SYSMODULE attribute, so PERS/CONST data declared before its first routine still resolves to a single shared instance across every task that loads it -- that part of RAPID's behavior is tied to the SYSMODULE attribute itself, not to how the module got loaded. VAR data declared there does NOT get this treatment: each task gets its own independent copy, which silently defeats the point of putting it there since it reads as shared (declared once, before any routine). 
 	 - HelperMethods.FindNonSharedModuleData(module): scans a module's shared-data section (MODULE header up to its first PROC/FUNC/TRAP) and returns the declarations that use a keyword other than PERS or CONST. TASK PERS is deliberately excluded (it's per-task by design, not accidentally unshared). 
 - 7 new regression tests in HelperMethodTests.cs. 
 	 - UploadModule/UploadHelperModules/UploadSystemModule gain an `out List<string> warnings` parameter carrying any such findings; UploadProgramComponent and UploadHelperModulesComponent now surface each as a GH runtime warning even when the upload itself succeeds. 
 	 - UploadSystemModule dropped its now-unused taskName/shared parameters (every system module now always loads into every task) and the dead _localSystemDirectory/_remoteSystemDirectory fields + ConfigurationDomain using it depended on. 
 	 - Updated the 2 existing ControllerGrantTests call sites for the new signatures. 
 - Build clean (MSBuild), 665/665 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `6c520dc` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge pull request #26 from jpdrude/fix/current-robot-target-icon Update Current Robot Target icon 
  
   **Commit:** `077f67f` | **Date:** 2026-09-04 
 
 --- 
 
 - Update Current Robot Target icon Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `3338dbd` | **Date:** 2026-09-04 
 
 --- 
 
 - Update Current Robot Target icon Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `ed0ade8` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge pull request #25 from jpdrude/feature/current-robot-target-component Add Current Robot Target component wrapping RAPID's CRobT 
  
   **Commit:** `738fe8c` | **Date:** 2026-09-04 
 
 --- 
 
 - Add Current Robot Target component wrapping RAPID's CRobT New GH component (Advanced RAPID Features > Current Robot Target, nickname CRobT) that wraps CRobT([\TaskRef]|[\TaskName] [\Tool] [\WObj]) into a RAPID expression, returning the robot's current TCP position as a robtarget. 
 	 - Two optional generic inputs, Tool and Work Object, each resolved via HelperMethods.ResolveRAPIDValueExpression (accepts a Robot Tool/Work Object declaration, a RAPID Variable, a RAPID Expression, or plain text). Leaving either unconnected omits its \Tool / \WObj switch; with neither connected the output is plain CRobT(). 
 	 - Built directly via RAPIDExpression.FromString rather than FromFunctionCall, since CRobT's optional switch arguments are space-separated (\Tool:=t1 \WObj:=w1), not comma-separated like a regular RAPID function call. 
 	 - New 24x24 icon (CurrentRobotTarget_Icon.png, red target-reticle glyph, matching the existing icon color/style) registered in Resources.resx/ Designer.cs. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `5dad276` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge pull request #24 from jpdrude/fix/upgrader-group-membership Restore group membership when the v5 upgraders swap a component 
  
   **Commit:** `45ff357` | **Date:** 2026-09-04 
 
 --- 
 
 - Restore group membership when the upgraders swap a component GH_UpgradeUtil.SwapComponents only removes/adds the two components themselves; it has no notion of GH groups, which track membership separately as a list of member InstanceGuids on each GH_Group document object. Without this, running Upgrade Components on an old instance that was inside a group silently dropped the new instance out of that group. 
 - Added UpgradeHelpers.MigrateGroupMembership(old, new, document), called after a successful swap in all 5 v5 upgraders: scans document.Objects for GH_Group instances containing the old component's InstanceGuid, and for each one calls GH_Group.InstanceGuidsChanged(...) -- the same IGH_InstanceGuidDependent notification GH_Document itself sends to every group when object instance guids are remapped (e.g. GH_Document.MutateAllIds, used on duplicate/paste) -- rather than editing each group's ObjectIDs list by hand. 
 - Documented in CLAUDE.md alongside the rest of the upgrader write-up. 
 - Build clean (MSBuild), 658/658 tests passing. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `dc4e0ca` | **Date:** 2026-09-04 
 
 --- 
 
 - version number bump 
  
   **Commit:** `af79f61` | **Date:** 2026-09-04 
 
 --- 
 
 - Merge pull request #23 from jpdrude/fix/backward-compat-and-declaration-ordering Fix backward compatibility, RAPID expression parsing, declaration ordering, and add upgrade mechanics 
  
   **Commit:** `0ffa8c6` | **Date:** 2026-09-04 
 
 --- 
 
 - Update auto-generated CHANGELOG.md Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `10c8fd1` | **Date:** 2026-09-04 
 
 --- 
 
 - Add IGH_UpgradeObject upgraders for the v5 Obsolete/live component pairs Lets GH's built-in Solution -> "Upgrade Components" swap RoutineArgumentComponent, ConnectInterruptComponent, EmptyLineComponent, AssignVariableValueComponent and RAPIDVariableComponent old instances for the live ones automatically, with wires reconnected, instead of requiring manual replacement per instance. 
 	 - New RobotComponents.ABB.Gh/Upgraders/v5/ folder: one IGH_UpgradeObject per pair plus a shared UpgradeHelpers (MigrateInputByIndex/ByName, MigrateOutputByIndex), all built on GH_UpgradeUtil.MigrateSources/MigrateRecipients (verified via IL decompilation to be pure wire-only migration, safe even where a param's type changed), followed by GH_UpgradeUtil.SwapComponents(old, new, migrateParameters: false) -- false is required, since true would additionally transplant param objects via Replace{Input,Output}Parameters and silently carry a stale param type onto the new component wherever a param's type changed. 
 	 - Each upgrader's XML doc carries the old->new input/output name/type/index reference mapping for that component. 
 	 - AssignVariableValueComponent/RAPIDVariableComponent needed a new internal ConfigureForUpgrade(...) hook so a freshly-constructed instance can be put into the old instance's array/index mode *before* wires are migrated onto it; refactored their existing Toggle*Params methods into Apply*/Toggle* so the pure param-registration logic is reusable without the solve-triggering ExpireSolution side effect (which would be reentrant here, since the new instance isn't attached to a document yet). 
 	 - Documented the required companion-upgrader step in CLAUDE.md alongside the existing Obsolete/vN pattern write-up. 
 - Build clean (MSBuild), 658/658 tests passing. 
  
   **Commit:** `01fa18a` | **Date:** 2026-09-03 
 
 --- 
 
 - Fix backward compatibility for changed components, RAPID Expression text parsing, and declaration/comment ordering Backward compatibility (Archive corrupted on load): - Non-IGH_VariableParameterComponent components restore parameters positionally against the *current* code's param count on load, not the archive's. Adding/removing a param on a shipped component breaks every pre-existing .gh file with 'archive is corrupt' per missing chunk. Applies the project's established Obsolete/vN pattern: froze the pre-change shape of RoutineArgumentComponent, ConnectInterruptComponent, EmptyLineComponent, AssignVariableValueComponent and RAPIDVariableComponent as hidden _OBSOLETE classes (original GUIDs, Obsolete=true) under Obsolete/v5/, and gave each live component a new GUID. Removed the fragile SolveInstance-based param-type reconciliation this replaces. 
 	 - Documented the pattern and its rationale in CLAUDE.md as required going forward. 
 - RAPID Expression text parsing (GH_RAPIDExpression.CastFrom): - Text wired into a Param_RAPIDExpression input (e.g. via a Panel) was stored verbatim, never parsed as a number, unlike a native Number/Integer input. Added invariant-culture int/double parsing with verbatim fallback, centralized so it fixes every Param_RAPIDExpression input at once. 
 	 - Assign Variable Value's Index input has its own resolution path; added the same int-parsing fallback there. 
 - Declaration/comment ordering: - Comment (CodeType.Declaration) now writes into ProgramDeclarationCustomCodeLines instead of ProgramDeclarations: that's the list a RAPID Variable's own declaration (CodeLine output) and CodeLineComponent's custom code lines already use, in insertion order. ProgramDeclarations is only for the implicit declarations Movement/Target objects generate on their own, so a comment placed next to a user's own declaration wasn't landing in the same section. Added a regression test (CreateModule_DeclarationComments_InterleaveWithCustomCodeLineDeclarations). 
 - Empty Line component: - Type input (Instruction/Declaration) is now optional and menu-toggled (right-click 'Add Type Input'), off by default, matching Assign Variable Value's toggle pattern. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `9eb7a99` | **Date:** 2026-09-03 
 
 --- 
 
 - Merge pull request #22 from jpdrude/feature/get-array-at-index-component Add Get Array At Index component 
  
   **Commit:** `b2769d3` | **Date:** 2026-09-03 
 
 --- 
 
 - Add Get Array At Index component New GH component that wraps RAPID array element access (arrayName{index}) into a RAPIDExpression, so it can be wired into Assign Variable Value, a Move target, or any other RAPID Expression input. 
 	 - Variable: a Param_RAPIDVariable, expected to be an array-declared RAPID Variable (RAPID Variable component, 'Set Array Size'). 
 	 - Index: a Param_RAPIDExpression (default 1), so a plain integer, a RAPID variable, or a RAPID expression can all be wired in. The input hint reminds that RAPID arrays are 1-based. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `00266f8` | **Date:** 2026-09-03 
 
 --- 
 
 - Merge pull request #21 from jpdrude/fix/parameter-full-names Fix input/output parameter full names for 'Draw Full Names' display 
  
   **Commit:** `410e609` | **Date:** 2026-09-03 
 
 --- 
 
 - Fix input/output parameter full names for 'Draw Full Names' display Audited every component's RegisterInputParams/RegisterOutputParams (and dynamically-registered parameters) for the Name shown when 'Draw Full Names' is enabled. Fixed: - Missing spaces: 'ExternalAxis' -> 'External Axis' (DeconstructWorkObjectComponent), 'SignalType' -> 'Signal Type' (ConnectInterruptComponent) - Abbreviated output names that didn't match their own component's full name: Wait AI/AO/DI/DO/GI/GO/Robot outputs now read 'Wait for Analog Input' etc.; NumEntryBoxComponent's output now matches its renamed 'Numeric Entry Box' component - Typos: 'Inequalty' -> 'Inequality' (WaitAI/WaitAO), 'Moudle' -> 'Module', 'Add loaddata/tooldata/wobjdata' -> Title Case (RAPIDGeneratorComponent), 'Configurations Datas' -> 'Configuration Datas' (Path/TimedPathGeneratorComponent) - Capitalization consistency: 'Robot/External joint position N' -> 'Robot/External Joint Position N' across RobotJointPositionComponent, ExternalJointPositionComponent, and their Deconstruct counterparts; 'Error messages' -> 'Error Messages'; 'Attachment plane' -> 'Attachment Plane' (ExternalLinearAxisComponent) All other parameters already had correctly set, descriptive full names. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `cf3e05f` | **Date:** 2026-09-03 
 
 --- 
 
 - Merge pull request #20 from jpdrude/fix/component-full-names Fix component full names for 'Draw Full Names' display 
  
   **Commit:** `486fe41` | **Date:** 2026-09-03 
 
 --- 
 
 - Fix component full names for 'Draw Full Names' display Audited all GH components' Name (shown on canvas when Draw Full Names is enabled). Fixed: - CheckActionsComponent: 'CheckActions' -> 'Check Actions' - RoutineCallComponent: 'RoutineCall' -> 'Routine Call' - DeconstructGroupSignalComponent: 'Deconstruct GroupSignal' -> 'Deconstruct Group Signal' - OverrideRobotToolComponent: nickname was accidentally the full sentence 'Overrides the current Robot Tool' instead of a short nickname (the real description was already correct in the 4th constructor argument) -> nickname is now 'ORT' - OffsComponent: 'Offs' -> 'Offset Target' (nickname stays 'Offs') - ComparerExpressionComponent: 'Comparer Expression' -> 'Comparison Expression' - NumEntryBoxComponent: 'Num Entry Box' -> 'Numeric Entry Box' All other ~120 components already had correctly set, descriptive full names. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `c915727` | **Date:** 2026-09-03 
 
 --- 
 
 - Merge pull request #19 from jpdrude/feature/connect-interrupt-enable-disable Add Enable/Disable Interrupts outputs to Connect Interrupt component 
  
   **Commit:** `ef79570` | **Date:** 2026-09-03 
 
 --- 
 
 - Add Enable/Disable Interrupts outputs to Connect Interrupt component Adds two static outputs to ConnectInterruptComponent: - Enable Interrupts: RAPID IEnable; instruction - Disable Interrupts: RAPID IDisable; instruction Both are independent of the connect-interrupt inputs and always available. The output hints note that interrupts registered while disabled are queued and executed once interrupts are re-enabled, not discarded. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `e981ed6` | **Date:** 2026-09-03 
 
 --- 
 
 - Merge pull request #18 from jpdrude/fix/rapid-variable-initial-value Fix RAPID Variable initial value printing full declaration 
  
   **Commit:** `aae37bb` | **Date:** 2026-09-02 
 
 --- 
 
 - Fix RAPID Variable initial value printing full declaration The RAPID Variable component's initial Value/Values inputs were still plain text parameters (the same bug already fixed in Assign Variable Value and Routine Call). Wiring another RAPID Variable in as the initial value got stringified by Grasshopper's default ToString() before SolveInstance ran, producing e.g. 'VAR num a := VAR num b;;' instead of 'VAR num a := b;'. 
 - Both inputs are now generic parameters, resolved through the shared HelperMethods.ResolveRAPIDValueExpression() used by the other two components. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `3cd1d97` | **Date:** 2026-09-02 
 
 --- 
 
 - Version Number update 
  
   **Commit:** `08cc05c` | **Date:** 2026-09-02 
 
 --- 
 
 - Merge pull request #17 from jpdrude/feature/offs-expression-component Add Offs Expression component wrapping RAPID's Offs() function 
  
   **Commit:** `f893d3f` | **Date:** 2026-09-02 
 
 --- 
 
 - Add Offs Expression component wrapping RAPID's Offs() function New GH component that wraps the RAPID built-in Offs(Target, X, Y, Z) function into a RAPIDExpression, so it can be wired into a Move target, Assign Variable Value, or any other RAPID Expression input. 
 	 - Target accepts a Robot Target, a RAPID Variable (e.g. an INOUT robtarget routine argument), or a RAPID Expression, resolved via HelperMethods.ResolveRAPIDValueExpression (same resolution used by Assign Variable Value and Routine Call). 
 	 - X/Y/Z are Param_RAPIDExpression inputs (default 0.0), matching the existing convention used by Acceleration Set/Velocity Set/etc., so plain numbers, RAPID variables, or expressions can all be wired in. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `2584017` | **Date:** 2026-09-02 
 
 --- 
 
 - Merge pull request #16 from jpdrude/fix/routine-call-rapid-variable-argument Fix Routine Call printing full declaration for RAPID Variable arguments 
  
   **Commit:** `b9dd15a` | **Date:** 2026-09-02 
 
 --- 
 
 - Fix Routine Call printing full declaration for RAPID Variable arguments RoutineCallComponent resolved a RAPIDVariable argument via ToString() on the raw Goo, which prints the full declaration (e.g. 'VAR num x := 5;') instead of just the variable name to reference in the call. 
 - Extracted the RAPID value-resolution logic already added to AssignVariableValueComponent into a shared HelperMethods. 
 - ResolveRAPIDValueExpression(), and use it in both components so a RAPIDVariable resolves to its Name, an IDeclaration (Robot Target, Speed Data, ...) to its Name (or inline RAPID value if unnamed), a RAPIDExpression to its expression text, and RoutineArgument via ToCallString(), consistently wherever an argument/value is resolved. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `d843a7e` | **Date:** 2026-09-02 
 
 --- 
 
 - Merge pull request #15 from jpdrude/fix/assign-variable-value-rapid-values Fix Assign Variable Value assigning type names instead of values 
  
   **Commit:** `75aca09` | **Date:** 2026-09-02 
 
 --- 
 
 - Fix Assign Variable Value assigning type names instead of values The Value/Values inputs were text parameters, so wiring in a RAPID declaration (Robot Target, Speed Data, RAPID Variable, ...) got silently stringified by Grasshopper's default ToString() before SolveInstance ever ran, producing text like "Robot Target" instead of a usable RAPID value. 
 - Both inputs are now generic parameters, and the component resolves the underlying value itself: RAPID declarations resolve to their declared name (or inline RAPID value if unnamed), RAPID Expressions resolve to their expression text, and plain values (bool/number/ string) are formatted with invariant culture so decimal points don't turn into commas. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `583cf5c` | **Date:** 2026-09-02 
 
 --- 
 
 - Merge pull request #14 from jpdrude/feature/routine-argument-variable-output Add RAPIDVariable output to Routine Argument component 
  
   **Commit:** `80fb390` | **Date:** 2026-09-02 
 
 --- 
 
 - Add RAPIDVariable output to Routine Argument component Routine Argument now also outputs the argument as a RAPIDVariable (Routine level, LOCAL scope) so it can be referenced inside the routine body, e.g. as input for Assign Variable Value. The Keyword text input is parsed into RAPIDVariableKeyword (VAR/PERS/INOUT/CONST), defaulting to VAR for unmarked arguments. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `7fb6048` | **Date:** 2026-09-02 
 
 --- 
 
 - Minor fix in error handling. ErrWrite was called with two ';' in GetErrorHandlerCodeLines 
  
   **Commit:** `2cdd0b5` | **Date:** 2026-08-25 
 
 --- 
 
 - Update version number before new version 1.3.6 
  
   **Commit:** `9cf1f30` | **Date:** 2026-08-25 
 
 --- 
 
 - Merge pull request #13 from jpdrude/fix/path-generator-stale-cache Fix Path/Timed Path Generator caching to include tool and robot changes 
  
   **Commit:** `349a9ca` | **Date:** 2026-08-25 
 
 --- 
 
 - Fix Path/Timed Path Generator caching to include tool and robot changes PathGeneratorComponent and TimedPathGeneratorComponent cache the generated path per iteration and only recompute it when an input hash changes. That hash previously only accounted for the tool's Name, so changing the tool's TCP plane, attachment plane, robot-hold flag, or load data (without renaming it) silently kept the stale cached path. The same gap existed for OverrideRobotTool actions. 
 - The hash also omitted robot-model data that can change independently of the robot Name: mounting frame, internal axis planes, and internal axis limits. 
 - Include RobotTool.ToRAPID() (which already encodes the TCP transform relative to the attachment plane, robot-hold, and load data) in the hash, and add the robot's mounting frame, internal axis planes, and internal axis limits. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> Claude-Session: https://claude.ai/code/session_012FWAydD7Dwp39BFS6E7bxy 
  
   **Commit:** `0336107` | **Date:** 2026-08-25 
 
 --- 
 
 - Fixed Syntax Error in Error-Handling in RAPIDGenerator. 
  
   **Commit:** `805ebe6` | **Date:** 2026-07-14 
 
 --- 
 
 - Merge pull request #12 from jpdrude/RAPIDVariable_Compatibility Add ReferenceTarget and enhance RAPID Generator 
  
   **Commit:** `9a8e755` | **Date:** 2026-07-13 
 
 --- 
 
 - Add ReferenceTarget and enhance RAPID Generator Introduced the `ReferenceTarget` class for symbolic RAPID target references, including serialization, duplication, and validation. Enhanced `RAPIDGeneratorComponent` with reordered input parameters, asynchronous solution scheduling, and improved error handling. 
 - Added new Git commands in `settings.local.json` to expand repository interaction capabilities. Updated `GH_Target.cs` to support casting for `RAPIDVariable` and `RAPIDExpression` types. Incremented version to 1.3.4 in `VersionNumbering.cs`. 
 - Updated `CHANGELOG.md` with recent changes and fixed parameter indexing issues. Added SPDX license headers and author information for better compliance. 
  
   **Commit:** `d67f88e` | **Date:** 2026-07-13 
 
 --- 
 
 - Merge pull request #11 from jpdrude/fix/error-handling-menu Add configurable error handling to RAPID Generator 
  
   **Commit:** `a79a234` | **Date:** 2026-07-13 
 
 --- 
 
 - Fix stale RAPID version-comment assertion in RAPIDGeneratorTests The generated comment text was changed to "a modified version of RobotComponents" in e9b58501 but the test still asserted the old wording, breaking CI. 
  
   **Commit:** `4a3d77b` | **Date:** 2026-07-13 
 
 --- 
 
 - Add configurable error handling to RAPID Generator Adds a right-click "Specify Error Handling" option to the RAPID Generator component, backed by a new ErrorHandling enum (No Error Handling / Pause on Error / Skip all Errors). When enabled, an ERROR handler is appended to the end of the generated procedure: Pause on Error logs the error and stops the task, Skip all Errors adds a TRYNEXT. Also fixes the value-list-attached-to-dynamic-input helper placing new dropdowns at canvas (0,0) instead of next to their input. 
 - Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com> 
  
   **Commit:** `cd8068f` | **Date:** 2026-07-11 
 
 --- 
 
 - New Version Commit 
  
   **Commit:** `a952410` | **Date:** 2026-07-13 
 
 --- 
 
 - Merge pull request #10 from jpdrude/comparisonSigns Refactor value list creation for comparison operators 
  
   **Commit:** `c4d42e0` | **Date:** 2026-07-13 
 
 --- 
 
 - Refactor value list creation for comparison operators Updated `ComparerExpressionComponent`, `WaitAIComponent`, and `WaitAOComponent` to use explicit `List<string>` for operator value lists instead of relying on enumerations. Added `System.Collections.Generic` to enable generic collections. 
 - Modified `HelperMethods` to remove sorting of names and set `GH_ValueListItem` values to indices instead of string representations. These changes improve clarity, consistency, and alignment with new requirements. 
  
   **Commit:** `e6d53e0` | **Date:** 2026-07-13 
 
 --- 
 
 - New Version Commit 
  
   **Commit:** `8c59c86` | **Date:** 2026-07-11 
 
 --- 
 
 - Version Number Update 
  
   **Commit:** `07137a3` | **Date:** 2026-06-30 
 
 --- 
 
 - Changed comment lines in RAPID script to include modified version info and script author 
  
   **Commit:** `e9b5850` | **Date:** 2026-06-30 
 
 --- 
 
 - Merge pull request #8 from jpdrude/fix/upload-helper-modules-issues Fix bugs in UploadHelperModules 
  
   **Commit:** `ab328df` | **Date:** 2026-06-03 
 
 --- 
 
 - Fix bugs in UploadHelperModules and UploadHelperModulesComponent - Add empty-branch guard (Count < 2) before indexing into module list to prevent IndexOutOfRangeException on empty DataTree branches - Redirect SYSMODULE branches to UploadSystemModule instead of storing them in the wrong directory without updating SYS configuration - Return false with a descriptive message on invalid branches instead of silently skipping them (consistent with UploadModule behaviour) - Remove duplicate `using Grasshopper.Kernel.Types` and move misplaced `using Grasshopper.Kernel.Data` into the Grasshopper usings block - Add missing PickItem_Icon to the Pick Task context menu item Co-Authored-By: Claude Sonnet 4.6 (1M context) <noreply@anthropic.com> 
  
   **Commit:** `0268122` | **Date:** 2026-06-03 
 
 --- 
 
 - Updated Version Number 
  
   **Commit:** `5ffc68a` | **Date:** 2026-06-02 
 
 --- 
 
 - Update Changelog and minor fix to GH_RobotComponent 
  
   **Commit:** `e6e4c8c` | **Date:** 2026-06-02 
 
 --- 
 
 - Changes to loops and addition of ComparerExpression component. While loop now accepts expression, for loops accept from to input with variable names and automatically create i as a counter variable if nothing else is provided. 
  
   **Commit:** `5933951` | **Date:** 2026-06-02 
 
 --- 
 
 - Bug fix to include Routine Code in RAPIDGenerator. Minor changes to variables including addition of CONST keyword. 
  
   **Commit:** `4c8e463` | **Date:** 2026-06-02 
 
 --- 
 
 - Update CI Pipeline to correctly create build. 
  
   **Commit:** `d1e3af4` | **Date:** 2026-04-28 
 
 --- 
 
 - Generate new Changelog before release. 
  
   **Commit:** `1049c05` | **Date:** 2026-04-27 
 
 --- 
 
 - Expand Bash commands, bump version, add CLAUDE.md - Bumped RobotComponents version to 1.2.0. 
 	 - Added CLAUDE.md with branch strategy and repository structure documentation. 
  
   **Commit:** `2f34c1d` | **Date:** 2026-04-27 
 
 --- 
 
 - Resolving Merge conflicts introduced by PR #7 
  
   **Commit:** `ff582a5` | **Date:** 2026-04-27 
 
 --- 
 
 - Finalize PR #7 
  
   **Commit:** `2b9ec0b` | **Date:** 2026-04-27 
 
 --- 
 
 - Merge pull request #7 from jpdrude/ikgeo CodingFeatures: RAPID language extensions, param consolidation, backwards compat, CI overhaul 
  
   **Commit:** `bceeb8d` | **Date:** 2026-04-27 
 
 --- 
 
 - New Icons - Assign Variable - Comparison Symbol Value list - Times Path Generator 
  
   **Commit:** `f57dd0f` | **Date:** 2026-04-27 
 
 --- 
 
 - Expand allowed Bash command patterns in settings.local.json Added support for additional Bash command patterns: find, dotnet test, dotnet build, gh pr, and git add. This update enables these commands to be recognized or executed according to the configuration. 
  
   **Commit:** `952d7c4` | **Date:** 2026-04-27 
 
 --- 
 
 - Merge pull request #6 from jpdrude/CodingFeatures CodingFeatures: RAPID language extensions, param consolidation, backwards compat, CI overhaul 
  
   **Commit:** `4888ed5` | **Date:** 2026-04-27 
 
 --- 
 
 - Changelog update before Release. 
  
   **Commit:** `d8ad556` | **Date:** 2026-04-27 
 
 --- 
 
 - Enhance deserialization and legacy migration support. Improve backward compatibility for ABB RAPID components by: - Migrating legacy parameter data in GH_RAPIDExpression.Read to support old GH_Number/GH_Boolean types. - Adding parameter existence checks in RAPIDGeneratorComponent and InverseKinematicsComponent to prevent errors with older files. - Updating changelog and settings.local.json with new tests and diagnostics. 
  
   **Commit:** `fe6916f` | **Date:** 2026-04-27 
 
 --- 
 
 - Enhance changelog extraction and add CI test scripts - Added CI-Tests and CI Solution Items folders to the solution, referencing test scripts and GitHub workflow files. 
 	 - Improved Extract-Changelog.ps1: added MaxSections/MaxLength params, better handling of missing/empty changelogs, section limiting, and output truncation. 
 	 - Enhanced logging and diagnostics in changelog extraction. 
 	 - Updated Extract-Changelog.Tests.ps1 to verify fallback behavior. 
 	 - Extended settings.local.json with new Bash and PowerShell Pester test commands. 
  
   **Commit:** `0e350cf` | **Date:** 2026-04-27 
 
 --- 
 
 - Refactor: unify RAPID outputs as Param_Action, obsolete old params - All ABB RAPID instruction components now output Param_Action instead of instruction-specific parameter types. 
 	 - Old parameter classes are replaced with hidden, obsolete shims to ensure backward compatibility with existing .ghx files. 
 	 - Updated all component files to use the new parameter namespace. - Added new icon for Timed Path Generator and updated resources. 
  
   **Commit:** `9919b80` | **Date:** 2026-04-27 
 
 --- 
 
 - Ensure invariant float formatting, Rhino test infra, RAPID fixes - Use CultureInfo.InvariantCulture for all RAPID float output to ensure locale-independent decimal formatting. 
 	 - Add RhinoCoreFixture and RequiresRhino xUnit collection for reliable, single-initialization RhinoCommon test setup. 
 	 - Mark all Rhino-dependent tests with RequiresRhino collection and trait. 
 	 - Add rhino.runsettings to set PATH for Rhino native dependencies during test runs. 
 	 - RAPIDGenerator: enforce axis limits before codegen, always emit PROC/ENDPROC block. 
 	 - Minor fixes in Movement.cs for initialization order and float formatting. 
 	 - Update settings.local.json for Rhino System directory access and CI scripting. 
  
   **Commit:** `a1b5e10` | **Date:** 2026-04-15 
 
 --- 
 
 - Add Timed Path Generator & new icons for RAPID components - Introduce TimedPathGenerator and Grasshopper UI component for time-based robot path simulation. 
 	 - Add new icons for EmptyLine and ComparisonSymbolValueList. 
  
   **Commit:** `e303c36` | **Date:** 2026-04-14 
 
 --- 
 
 - Add consistent indentation for generated RAPID code Introduces IndentationLevel to IAction and all action classes, enabling proper indentation of generated RAPID code, especially for nested control-flow constructs (FOR, WHILE, IF, MessageBox). Control-flow components now increment IndentationLevel for their body actions, and all ToRAPIDGenerator methods apply the correct indentation. Improves code readability and maintainability. 
  
   **Commit:** `3e9f81b` | **Date:** 2026-04-14 
 
 --- 
 
 - Add advanced RAPID flow control and array support - Introduce For, While, and If statement components for RAPID code generation, including dynamic ELSEIF/ELSE branching and comparison operator value lists. 
 	 - Add NumEntryBox and EmptyLine components for user input and code formatting. 
 	 - Enhance RAPIDVariable and AssignVariableValue components with right-click array mode, improved validation, and new icons. 
 	 - Move code generation tools to "Advanced RAPID Features" category and adjust UI exposure. 
 	 - Add new icons for variables, expressions, loops, and assignments. 
 	 - Add ComparisonOperator enum for conditional constructs. 
 	 - Update changelog and resources for new features and improved UX. 
  
   **Commit:** `dc06fb1` | **Date:** 2026-04-14 
 
 --- 
 
 - Add RAPIDExpression support for IO and wait components - Introduced RAPIDExpression type for literals, variables, and expressions - Added GH_RAPIDExpression Goo and Param_RAPIDExpression parameter - Refactored IO/wait/action classes to accept expressions as input - Components now allow variables/functions as input, not just numbers - Added validation and warnings for invalid RAPID expressions - Updated RoutineCall/AdditionalRoutine to output FUNC expressions - Added comprehensive unit tests for RAPIDExpression and affected actions - Updated changelog and CI for CodingFeatures branch 
  
   **Commit:** `0ccd3d6` | **Date:** 2026-04-13 
 
 --- 
 
 - Changed Variable assignment to only work on RAPIDVariables, not on variable names. 
  
   **Commit:** `25fa6ad` | **Date:** 2026-04-13 
 
 --- 
 
 - Add RAPID variable and FUNC support to code generation - Add RAPIDVariable class, Grasshopper Goo, and parameter type - New RAPIDVariableComponent for variable declarations (VAR/PERS/INOUT) - Add AssignVariableValueComponent for assignments - Support FUNC routines: RoutineType.FUNC, return type, serialization - RoutineCallComponent now supports PROC and FUNC calls - Add enums for variable level, keyword, and callable routine type - Dynamic value lists and input management for better UX 
  
   **Commit:** `7965100` | **Date:** 2026-04-13 
 
 --- 
 
 - Update year in header 
  
   **Commit:** `c8a969c` | **Date:** 2026-03-31 
 
 --- 
 
 - Enhance geometry preview in PathGenerator component; use ExpirePreview instead of ExpireSolution for more targeted preview updates and ensure path preview is refreshed after component deserialization 
  
   **Commit:** `2dfd659` | **Date:** 2026-03-31 
 
 --- 
 
 - Replace ObjectsDeleted event with RemovedFromDocument in components that use an ObjectManager 
  
   **Commit:** `32c448e` | **Date:** 2026-03-31 
 
 --- 
 
 - Add check for special characters in signal name; fix incorrect implementation 
  
   **Commit:** `cb225ec` | **Date:** 2026-03-30 
 
 --- 
 
 - Change year in header 
  
   **Commit:** `1c3fdb3` | **Date:** 2026-03-30 
 
 --- 
 
 - Bug fix in serialization of WaitTime class 
  
   **Commit:** `0cf31a5` | **Date:** 2026-03-30 
 
 --- 
 
 - Ensure mastership release with try-finally block 
  
   **Commit:** `913c324` | **Date:** 2026-03-30 
 
 --- 
 
 - Add return when empty controller is initialized 
  
   **Commit:** `3b469de` | **Date:** 2026-03-30 
 
 --- 
 
 - Add return in signal class when an empty signal is set 
  
   **Commit:** `cc0a476` | **Date:** 2026-03-30 
 
 --- 
 
 - Add example files for v4.1.x 
  
   **Commit:** `b72e193` | **Date:** 2026-03-30 
 
 --- 
 
 - Update the version number 
  
   **Commit:** `06e7bf5` | **Date:** 2026-03-28 
 
 --- 
 
 - Merge pull request #201 from RobotComponents/gofa Gofa 
  
   **Commit:** `af65df8` | **Date:** 2026-03-28 
 
 --- 
 
 - Add a Pick Item icon 
  
   **Commit:** `8eaaf1a` | **Date:** 2026-03-20 
 
 --- 
 
 - Integration of the GoFa series 
  
   **Commit:** `873e111` | **Date:** 2026-03-20 
 
 --- 
 
 - Update the info component 
  
   **Commit:** `140c05d` | **Date:** 2026-03-16 
 
 --- 
 
 - Update README.md 
  
   **Commit:** `fbcf64c` | **Date:** 2026-03-16 
 
 --- 
 
 - Update zenodo.json 
  
   **Commit:** `e17e1be` | **Date:** 2026-03-16 
 
 --- 
 
 - Update RobotComponents.Tests.csproj 
  
   **Commit:** `4abe729` | **Date:** 2026-03-16 
 
 --- 
 
 - Add Component Button class 
  
   **Commit:** `b818692` | **Date:** 2026-03-16 
 
 --- 
 
 - Add link to ko-fi 
  
   **Commit:** `8f84f16` | **Date:** 2026-03-16 
 
 --- 
 
 - Adds current ABB PC SDK assembly. 
  
   **Commit:** `84297ee` | **Date:** 2026-03-09 
 
 --- 
 
 - Merge pull request #5 from jpdrude/WorkshopBugFixes Workshop Bug Fixes 
  
   **Commit:** `236fee8` | **Date:** 2026-03-09 
 
 --- 
 
 - Rotated ikGeo target plane around z-Axis to provide correct tool orientation. 
 - The problem wasn't apparent before, because it wasnt tested with assymetric tool geometry. 
  
   **Commit:** `bc8ab69` | **Date:** 2026-03-09 
 
 --- 
 
 - Adds missing assemblies and corresponding license files to release. 
 - Added assemblies and license locations to Collect-ReleaseFiles.ps1 to accomodate them in release. 
  
   **Commit:** `d69efd0` | **Date:** 2026-03-09 
 
 --- 
 
 - Update parameter naming, which caused problems when drawing full names in GH. 
 - Refactored parameter class display names to remove "Parameter" suffix and updated constructors for consistency across the ABB Grasshopper plugin. Changed Name property overrides to use the base property for improved flexibility. 
  
   **Commit:** `c25b8e6` | **Date:** 2026-03-09 
 
 --- 
 
 - Pushed changelog before release. 
  
   **Commit:** `103d5f2` | **Date:** 2026-02-26 
 
 --- 
 
 - Minor fixes in release pipeline and explanation of Tests. 
  
   **Commit:** `eb77d7f` | **Date:** 2026-02-26 
 
 --- 
 
 - Fixed some bugs in Test scripts, which were hampering the build. 
  
   **Commit:** `e6d7b50` | **Date:** 2026-02-26 
 
 --- 
 
 - Included Reference to RobotComponents.ABB.Controllers in tests project. 
  
   **Commit:** `960283b` | **Date:** 2026-02-25 
 
 --- 
 
 - Updated changelog before release. 
  
   **Commit:** `2facb14` | **Date:** 2026-02-25 
 
 --- 
 
 - Overhaul release pipeline: CI-driven releases with full changelog and assets - Remove post-build CreateRelease.ps1 in favour of the existing CI release workflow as the single release mechanism. 
 	 - Release notes now include install instructions followed by all commits since the previous tag. 
 	 - RobotComponentsEDEK.gha added as an optional separate release asset from DLLs/. 
 	 - UpdateChangeLog.ps1 now skipsregeneration if the latest commit is already recorded in CHANGELOG.md. 
  
   **Commit:** `1656cb8` | **Date:** 2026-02-25 
 
 --- 
 
 - Overhaul release pipeline: CI-driven releases with full changelog and assets - Remove post-build CreateRelease.ps1 in favour of the existing CI release workflow as the single release mechanism. 
 	 - Release notes now include install instructions followed by all commits since the previous tag. 
 	 - RobotComponentsEDEK.gha added as an optional separate release asset from DLLs/. 
 	 - UpdateChangeLog.ps1 now skipsregeneration if the latest commit is already recorded in CHANGELOG.md. 
  
   **Commit:** `8d170b6` | **Date:** 2026-02-25 
 
 --- 
 
 - Updated VersionNumbering to my current style at v1.1.0 
  
   **Commit:** `937246c` | **Date:** 2026-02-25 
 
 --- 
 
 - Separate non-critical RAPID generator messages into RemarksText - Introduce a RemarksText list in RAPIDGenerator for informational messages that should not block code generation. Move the "first movement is not MoveAbsJ" notice from ErrorText to RemarksText and surface it as a GH Remark in RAPIDGeneratorComponent. 
 	 - Make structural RAPID keyword detection in RapidCodeLineSanitizer case-sensitive, reducing false positives on lowercase user code. 
  
   **Commit:** `5c4afa1` | **Date:** 2026-02-25 
 
 --- 
 
 - Merge remote-tracking branch 'origin/main' into ikgeo 
  
   **Commit:** `2060c9d` | **Date:** 2026-02-24 
 
 --- 
 
 - Refine structural keyword checks in RapidCodeLineSanitizer Removed ENDTEST, ENDWHILE, ENDIF, ENDFOR from the list of structural RAPID keywords checked for code injection. Only block boundary keywords relevant to RAPID modules and routines are now detected, reducing false positives and focusing sanitization on critical structure. 
  
   **Commit:** `0e31914` | **Date:** 2026-02-24 
 
 --- 
 
 - Remove UploadHelperModules test to fix CI build DataTree<string> requires the Grasshopper assembly which is not available in the CI environment. Remove the test and Grasshopper package reference; document the omission in the class docstring. 
  
   **Commit:** `66bdea7` | **Date:** 2026-02-19 
 
 --- 
 
 - Separate DemandGrant try-catch in ResetProgramPointer(s) Split the try block so ExecuteRapid grant failures produce a grant-specific status message rather than being conflated with GetTasks/ResetProgramPointer operation failures. 
  
   **Commit:** `f21cf9f` | **Date:** 2026-02-19 
 
 --- 
 
 - Replace bare catch with catch(Exception e) on WriteFtp grants Bare catch clauses swallow all exceptions including critical ones like OutOfMemoryException. Use catch(Exception e) and include e.Message in the status string for diagnostic clarity. 
  
   **Commit:** `89d3dc8` | **Date:** 2026-02-19 
 
 --- 
 
 - Add UploadHelperModules test and clarify test coverage limits Add missing test for UploadHelperModules empty-controller path. Update class docstring to acknowledge that tests exercise the _isEmpty early return, not the DemandGrant failure path (ABB SDK types are sealed). 
 - Add Grasshopper package reference to test project for DataTree<string>. 
  
   **Commit:** `f4961f9` | **Date:** 2026-02-19 
 
 --- 
 
 - Separate DemandGrant from privileged operations in upload methods Split the try blocks in UploadModule and UploadHelperModules so that LoadRapidProgram grant acquisition has its own try-catch with a grant-specific error message, distinct from LoadModuleFromFile failures. 
 - Also moves DemandGrant out of the foreach loop in UploadHelperModules since the grant only needs to be acquired once per method call. 
  
   **Commit:** `11e699c` | **Date:** 2026-02-19 
 
 --- 
 
 - Add tests for Controller grant-protected methods Verify that UploadModule, UploadSystemModule, ResetProgramPointers, and ResetProgramPointer return false with descriptive status when the controller is empty. The DemandGrant fail-closed behavior itself is verified by code review since the ABB SDK cannot be mocked. 
  
   **Commit:** `4654f42` | **Date:** 2026-02-19 
 
 --- 
 
 - Handle ExecuteRapid grant failures in ResetProgramPointer(s) DemandGrant(ExecuteRapid) was called outside the try block in both ResetProgramPointers and ResetProgramPointer, so grant failures propagated as unhandled exceptions with no status message. Move DemandGrant inside the try block for proper error handling. 
 - Fixes #40 
  
   **Commit:** `156c205` | **Date:** 2026-02-19 
 
 --- 
 
 - Fail-closed on WriteFtp grant failures in Controller upload methods DemandGrant(WriteFtp) catch blocks in UploadModule, UploadHelperModules, and UploadSystemModule logged the failure but continued to PutDirectory() without authorization. Add return false to abort on grant failure. 
 - Fixes part of #40 
  
   **Commit:** `8ea2d92` | **Date:** 2026-02-19 
 
 --- 
 
 - Clarify EnforceAxisLimits scope in XML docs and test helper Update the EnforceAxisLimits XML doc to mention that it also covers other errors detected during target conversion (e.g. IK failures), not only axis limit violations. Add a note to the GenerateModule test helper about the implicit enforcement default. 
  
   **Commit:** `a649df3` | **Date:** 2026-02-18 
 
 --- 
 
 - Fix off-by-one in error message display limit The loop break condition `i == 30` displayed 31 messages (indices 0ÔÇô30). 
 - Changed to `i == 29` to correctly limit output to 30 error messages. 
  
   **Commit:** `b6603fc` | **Date:** 2026-02-18 
 
 --- 
 
 - Clean up cast in Movement and separate menu item in GH component Use a typed local variable instead of casting _convertedTarget back to JointTarget when checking axis limits after IK conversion. Place the Enforce Axis Limits menu item in its own visual section with separators. 
  
   **Commit:** `5e6f359` | **Date:** 2026-02-18 
 
 --- 
 
 - Add missing tests for IK-converted axis limits and Duplicate propagation Adds a test in MovementTests verifying that axis limits are checked on JointTargets produced by inverse kinematics conversion (the gap fix). 
 - Adds two tests in RAPIDGeneratorTests verifying that EnforceAxisLimits is correctly propagated through Duplicate() for both true and false. 
  
   **Commit:** `2d1c83f` | **Date:** 2026-02-18 
 
 --- 
 
 - Expose EnforceAxisLimits in RAPID Generator GH component Adds an optional "Enforce Axis Limits" input parameter (default true) to the RAPID Generator component, accessible via the context menu. 
 - When enforcement is active and axis limit violations are detected, messages are surfaced as red errors instead of yellow warnings and the output module is empty. Backward-compatible deserialization handles old .ghx files that lack this setting. 
 - Part of #38 
  
   **Commit:** `1b6de1e` | **Date:** 2026-02-18 
 
 --- 
 
 - Abort RAPID module generation on axis limit violations when enforced When EnforceAxisLimits is true (default) and ErrorText contains any violations after processing all actions, CreateModule now returns an empty module. Errors from additional routine sub-generators are also collected into the parent before the enforcement check. 
 - Adds three tests covering: enforce-on with violation (empty module), enforce-off with violation (full module), and enforce-on without violation (full module). 
 - Fixes #38 
  
   **Commit:** `6201014` | **Date:** 2026-02-18 
 
 --- 
 
 - Add EnforceAxisLimits property to RAPIDGenerator Introduces a boolean property (default true) that controls whether axis limit violations should prevent RAPID module generation. The property is propagated through the duplication constructor so sub-generators for additional routines inherit the parent's setting. 
 - Part of #38 
  
   **Commit:** `af67414` | **Date:** 2026-02-18 
 
 --- 
 
 - Check axis limits on IK-converted JointTargets in Movement When a RobotTarget is converted to a JointTarget via inverse kinematics (MoveAbsJ path), the resulting joint values were never validated against the robot's axis limits. This adds the missing CheckAxisLimits call on the converted target, closing the coverage gap. 
 - Fixes part of #38 
  
   **Commit:** `ef8dae1` | **Date:** 2026-02-18 
 
 --- 
 
 - Derive recommended limits from predefined arrays and show specific warnings Change RecommendedMax* fields from const to static readonly, derived from the predefined value arrays so they stay in sync automatically. 
 - Add GetExceededLimitWarnings() that returns a message identifying only the specific parameters that exceeded (e.g. "V_TCP (8000) exceeds recommended maximum (7000 mm/s)") instead of dumping all limits. 
  
   **Commit:** `09722bf` | **Date:** 2026-02-18 
 
 --- 
 
 - Add missing ZoneData per-parameter tests and MoveComponent limit warnings Address review feedback: add individual exceeds-limit tests for PathZoneEAX, ZoneLEAX, and ZoneREAX parameters. Also add ExceedsRecommendedLimits checks in MoveComponent so warnings are shown when speed/zone data is wired directly into Move components. 
  
   **Commit:** `4f680a2` | **Date:** 2026-02-18 
 
 --- 
 
 - Emit GH warnings when speed or zone values exceed recommended limits SpeedDataComponent and ZoneDataComponent now display Grasshopper runtime warnings when user-supplied values exceed the recommended maximums, alerting users to potentially unsafe robot parameters. 
  
   **Commit:** `d4ad6f6` | **Date:** 2026-02-18 
 
 --- 
 
 - Add ExceedsRecommendedLimits to ZoneData with recommended max constants Add soft upper bound checks for zone parameters based on ABB predefined maximums (PathZoneTCP=200 mm, PathZoneORI=300 mm, PathZoneEAX=300 mm, ZoneORI=30 deg, ZoneLEAX=300 mm, ZoneREAX=30 deg). Values exceeding these limits are flagged via ExceedsRecommendedLimits(). 
  
   **Commit:** `2818643` | **Date:** 2026-02-18 
 
 --- 
 
 - Add ExceedsRecommendedLimits to SpeedData with recommended max constants Add soft upper bound checks for speed parameters based on ABB predefined maximums (V_TCP=7000 mm/s, V_ORI=1000 deg/s, V_LEAX=5000 mm/s, V_REAX=1000 deg/s). Values exceeding these limits are not rejected but flagged via ExceedsRecommendedLimits() for downstream warning display. 
  
   **Commit:** `b6a3af7` | **Date:** 2026-02-18 
 
 --- 
 
 - Add path containment checks on remote file paths Validate the remote file paths in UploadModule and UploadHelperModules using IsPathWithinDirectory for consistency with the local path checks. 
 - (OWASP A03, issue #36) 
  
   **Commit:** `b8d3a03` | **Date:** 2026-02-18 
 
 --- 
 
 - Use graceful return false pattern for path containment checks Add IsPathWithinDirectory bool check and use it with the return false + status message pattern in Controller.cs, matching the surrounding error handling style. ThrowIfPathEscapesDirectory is retained but now delegates to the bool method. Adds null guard and tests for IsPathWithinDirectory. 
  
   **Commit:** `7913aa4` | **Date:** 2026-02-18 
 
 --- 
 
 - Add unit tests for path traversal validation Test ThrowIfPathEscapesDirectory with safe paths, traversal payloads, and absolute paths outside the base directory. Also verify that IsValidRapidIdentifier rejects path traversal sequences like "../../../test". (OWASP A03, issue #36) 
  
   **Commit:** `7761f4a` | **Date:** 2026-02-18 
 
 --- 
 
 - Validate module names in UploadHelperModules and UploadSystemModule Apply the same RAPID identifier validation and path containment checks to the remaining two upload methods. (OWASP A03, issue #36) 
  
   **Commit:** `bdd2aa9` | **Date:** 2026-02-18 
 
 --- 
 
 - Validate module name in UploadModule to prevent path traversal Reject module names that are not valid RAPID identifiers before using them in Path.Combine. Also add defense-in-depth path containment check to verify the resolved path stays within _localDirectory. 
 - (OWASP A03, issue #36) 
  
   **Commit:** `88fb350` | **Date:** 2026-02-18 
 
 --- 
 
 - Add ThrowIfPathEscapesDirectory helper to validate resolved paths Defense-in-depth method that resolves both paths via Path.GetFullPath and verifies the combined path stays within the base directory. Prevents path traversal attacks using ".." sequences. (OWASP A03, issue #36) 
  
   **Commit:** `be5e080` | **Date:** 2026-02-18 
 
 --- 
 
 - Remove user input from error message and add review polish - Strip file path from ThrowIfUnsafeFilePath exception to prevent information leakage of potentially malicious input - Add blank line between methods for consistent style - Comment hardcoded RunScript calls as safe from injection Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `8f2cda2` | **Date:** 2026-02-18 
 
 --- 
 
 - Add unit tests for file path validation Tests cover valid paths, null/empty input, the exact injection payload from issue #35, and the ThrowIfUnsafeFilePath exception behavior. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `946fd71` | **Date:** 2026-02-18 
 
 --- 
 
 - Validate file path before RhinoApp.RunScript in Preperation Calls ThrowIfUnsafeFilePath before interpolating user-selected file paths into RunScript commands, blocking the injection vector described in #35. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `39cf5b8` | **Date:** 2026-02-18 
 
 --- 
 
 - Add ThrowIfUnsafeFilePath helper to validate file paths Prevents command injection (OWASP A03) by rejecting file paths that contain double quotes or invalid path characters before they are interpolated into RhinoApp.RunScript command strings. 
 - Addresses #35 Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `2c678ea` | **Date:** 2026-02-18 
 
 --- 
 
 - Extract ThrowIfInvalidRapidIdentifier helper to reduce duplication Replace inline 5-line throw blocks in all 10 signal instruction classes with a single call to HelperMethods.ThrowIfInvalidRapidIdentifier(_name). 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `3d59bc5` | **Date:** 2026-02-18 
 
 --- 
 
 - Validate signal name in WaitGI and WaitGO Apply RAPID identifier validation: extend IsValid and guard ToRAPIDInstruction with InvalidOperationException. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `8cd0b56` | **Date:** 2026-02-18 
 
 --- 
 
 - Validate signal name in WaitAI and WaitAO Apply RAPID identifier validation: extend IsValid and guard ToRAPIDInstruction with InvalidOperationException. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `236d319` | **Date:** 2026-02-17 
 
 --- 
 
 - Validate signal name in WaitDI and WaitDO Apply RAPID identifier validation: extend IsValid and guard ToRAPIDInstruction with InvalidOperationException. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `fb38e78` | **Date:** 2026-02-17 
 
 --- 
 
 - Validate signal name in PulseDigitalOutput Apply RAPID identifier validation: extend IsValid (after null/empty, before length checks) and guard ToRAPIDInstruction. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `1c0ed0b` | **Date:** 2026-02-17 
 
 --- 
 
 - Validate signal name in SetAnalogOutput and SetGroupOutput Apply the same RAPID identifier validation pattern: extend IsValid and guard ToRAPIDInstruction with InvalidOperationException. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `3c81d70` | **Date:** 2026-02-17 
 
 --- 
 
 - Validate signal name in SetDigitalOutput to prevent RAPID injection Extend IsValid to reject names that are not valid RAPID identifiers. 
 - Guard ToRAPIDInstruction with InvalidOperationException to prevent injection even when callers skip the IsValid check. Closes #34. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `966eaf9` | **Date:** 2026-02-17 
 
 --- 
 
 - Add tests for IsValidRapidIdentifier Cover valid identifiers (letters, underscores, digits, boundary lengths) and invalid ones (null, empty, digit-start, spaces, semicolons, commas, newlines, quotes, over-length, and a full injection payload). 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `6d0e74c` | **Date:** 2026-02-17 
 
 --- 
 
 - Add IsValidRapidIdentifier to core HelperMethods Add a shared RAPID identifier validator using a compiled regex that enforces: letter/underscore start, alphanumeric/underscore body, max 32 characters. This will be used by all signal instruction classes to prevent RAPID code injection via crafted signal names (issue #34). 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `44f8904` | **Date:** 2026-02-17 
 
 --- 
 
 - Apply StripNewlines in copy constructor for defense-in-depth Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `e80e54c` | **Date:** 2026-02-15 
 
 --- 
 
 - Change Warnings property to IReadOnlyList<string> for API hygiene Prevents external callers from mutating the internal warnings list. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `cc509bd` | **Date:** 2026-02-15 
 
 --- 
 
 - Strip newlines from Comment text to prevent RAPID breakout injection A newline in a Comment string breaks out of the `! ` prefix, causing subsequent text to execute as real RAPID instructions. All constructors, the Com setter, and the deserialization constructor now strip \r\n, \r, and \n characters, replacing them with spaces. Closes #33. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `9ac6f2b` | **Date:** 2026-02-14 
 
 --- 
 
 - Fix review findings: sanitize on deserialization, guard ToRAPIDGenerator Deserialization constructor now runs sanitization on the code read from SerializationInfo, closing the bypass via malicious .ghx files. 
 - ToRAPIDGenerator skips emitting code when warnings are present, so flagged structural keywords are actually blocked from RAPID output. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `52f759f` | **Date:** 2026-02-14 
 
 --- 
 
 - Add RAPID code line sanitizer to mitigate code injection (OWASP A03) Introduces RapidCodeLineSanitizer that strips newlines and detects structural RAPID keywords (ENDPROC, ENDMODULE, PROC, MODULE, TRAP, etc.) which could break the generated module structure. CodeLine now sanitizes all input and exposes warnings; the Grasshopper component surfaces them in the UI. Closes #32. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `61e737e` | **Date:** 2026-02-14 
 
 --- 
 
 - Fix review findings: validate all generic type args, add List<T> test - Rewrite ContainsOnlyAllowedTypeArguments to parse each type argument individually instead of checking if any allowed string appears anywhere - Add List<SpeedData> round-trip test to cover the generic collection path - Remove unused using directive Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `ad97f37` | **Date:** 2026-02-14 
 
 --- 
 
 - Add AllowedTypesSerializationBinder to mitigate CWE-502 deserialization vulnerability BinaryFormatter.Deserialize in Serialization.cs could instantiate arbitrary types from maliciously crafted .gh files, enabling remote code execution. 
 - Add a SerializationBinder that restricts deserialization to known types (RobotComponents.*, Rhino.Geometry.*, and explicit System primitives). 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `93ef09a` | **Date:** 2026-02-14 
 
 --- 
 
 - Mark test plan Phase 4 as complete Update tests-plan.md: all phases complete (410 xUnit + 21 Pester), fix file paths to .github/tests/, add Generate-InstallInstructions. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `46b2168` | **Date:** 2026-02-14 
 
 --- 
 
 - Fix Collect-ReleaseFiles tests: clean TestDrive between runs TestDrive persists between It blocks in Pester 5. Files from the first test leaked into subsequent tests, causing count mismatches. Add cleanup at the start of BeforeEach. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `c601d29` | **Date:** 2026-02-14 
 
 --- 
 
 - Fix 4 failing Pester tests - Extract-Changelog: use IsNullOrWhiteSpace instead of -not for empty changelog detection (file with only newline was truthy) - Collect-ReleaseFiles tests: remove Should -Not -Throw wrappers that interfered with ErrorActionPreference in the script Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `c1adf1a` | **Date:** 2026-02-14 
 
 --- 
 
 - Add Extract-Changelog and Generate-InstallInstructions scripts with tests Extract changelog parsing and install-instructions generation from release.yml into standalone scripts. Extract-Changelog supports configurable MaxSections and MaxLength with fallback for missing files. Includes 9 Pester test cases total. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `5735ef8` | **Date:** 2026-02-14 
 
 --- 
 
 - Add Collect-ReleaseFiles.ps1 script and Pester test Extract and unify the file-collection logic from release.yml and artifact-build.yml into a single parameterized script. Accepts Configuration, OutputDir, RepoRoot, and optional CreateZip/Version. 
 - Includes 7 test cases using TestDrive fixtures. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `e4234cc` | **Date:** 2026-02-14 
 
 --- 
 
 - Add Validate-Version.ps1 script and Pester test Extract inline PowerShell from release.yml that validates git tag matches VersionNumbering.cs. Uses throw instead of exit 1 for Pester testability. Includes 5 test cases covering match, mismatch, missing regex, missing file, and no v-prefix scenarios. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `cfd964f` | **Date:** 2026-02-14 
 
 --- 
 
 - Refactor release workflow to use PowerShell scripts Refactor release workflow to use scripts for version validation, file collection, changelog extraction, and installation instructions. 
  
   **Commit:** `b8928d0` | **Date:** 2026-02-14 
 
 --- 
 
 - Add Pester tests to CI workflow Added a step to run Pester tests with detailed output and NUnit XML format. 
  
   **Commit:** `7218c7e` | **Date:** 2026-02-14 
 
 --- 
 
 - Refactor artifact collection in workflow 
  
   **Commit:** `ca09771` | **Date:** 2026-02-14 
 
 --- 
 
 - Address review findings: shared helpers, dedup, consistency - Extract CreateIRB120Robot(), CreateIRB120OPW(), and AssertAnySolutionMatches() into shared TestHelpers class - Remove duplicated CreateTestRobot() from RobotTests and KinematicsTests - Replace 3 duplicated round-trip match loops with shared helper - Use class-level Tolerance constant in WorkObjectTests - Add comments explaining why Parse exception tests skip RequiresRhino Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `afb689e` | **Date:** 2026-02-14 
 
 --- 
 
 - Tag Rhino-native-dependent tests with RequiresRhino trait Tests that require rhcommon_c (Mesh, Plane constructors, Quaternion operations) are tagged so CI can exclude them with Category!=RequiresRhino filter. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `4388ca6` | **Date:** 2026-02-14 
 
 --- 
 
 - Add Phase 3 unit tests for Definitions and Kinematics (121 tests) New test files: - RobotToolTests.cs (25 tests): constructor, ToRAPID, declarations, Parse - WorkObjectTests.cs (27 tests): constructor, ToRAPID, declarations, Parse - RobotTests.cs (21 tests): constructor, axis planes, mounting frame, tool, duplicate - RobotKinematicParametersTests.cs (18 tests): constructor, GetAxisPlanes, round-trip, duplicate - KinematicsTests.cs (30 tests): OPW forward/inverse, round-trip, singularity, ForwardKinematics Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `9196666` | **Date:** 2026-02-14 
 
 --- 
 
 - Address review findings: precision, coverage gaps - MoveC assertion uses trailing space to avoid matching MoveCDO - MoveLDO test asserts DO value (DO_1, 1) - Add MoveCDO test (MoveC + SetDigitalOutput combined) - Add IList<double> constructor test for RobotJointPosition - Add LOCAL and TASK scope tests for RobotJointPosition declarations Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `35e9d8e` | **Date:** 2026-02-14 
 
 --- 
 
 - Add RobotJointPosition unit tests for RAPID code generation Tests constructors, ToRAPID format, ToRAPIDDeclaration, indexer, IsValid, Duplicate, and Parse/TryParse for the RobotJointPosition declaration type. 18 tests covering full public API surface. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `67dd544` | **Date:** 2026-02-14 
 
 --- 
 
 - Add Time parameter unit tests for Movement RAPID code generation Tests that setting Time > 0 appends \T:=N to the speed data in the instruction and that the default Time (-1) produces no \T parameter. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `7d56f84` | **Date:** 2026-02-14 
 
 --- 
 
 - Add SyncID unit tests for Movement RAPID code generation Tests that setting SyncID appends \ID:=N to the instruction and that the default SyncID (-1) produces no \ID parameter. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `f92d0cc` | **Date:** 2026-02-14 
 
 --- 
 
 - Add MoveLDO/MoveJDO unit tests for Movement RAPID code generation Tests MoveLDO, MoveJDO instruction variants when a SetDigitalOutput is combined with MoveL/MoveJ, and verifies MoveAbsJ with DO produces separate MoveAbsJ + SetDO instructions. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `e821694` | **Date:** 2026-02-14 
 
 --- 
 
 - Add MoveC unit tests for Movement RAPID code generation Tests MoveC instruction with circular via-point and verifies that an unset circular point correctly throws an exception. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `62ae822` | **Date:** 2026-02-14 
 
 --- 
 
 - Improve test suite: split InstructionTests, add Parse boundary tests Split the 527-line InstructionTests.cs into 7 focused files per instruction type for better discoverability. Add 21 Parse/TryParse boundary tests covering wrong datatype, too few values, and CONST/PERS/LOCAL scope variants across all declaration types. 
 - Strengthen WaitDI assertion to check both MaxTime and TimeFlag. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `6d5826b` | **Date:** 2026-02-13 
 
 --- 
 
 - Fix SpeedData nearest-snap test: tie at 25 goes to v30, not v20 Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `0dab905` | **Date:** 2026-02-13 
 
 --- 
 
 - Add RAPIDGenerator tests for RAPID module assembly Tests module structure (MODULE/ENDMODULE, PROC/ENDPROC), version comments, SpeedData/ZoneData deduplication, instruction ordering, declaration sorting, optional section flags, and mixed action layout. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `976e7b6` | **Date:** 2026-02-13 
 
 --- 
 
 - Add Movement unit tests for RAPID code generation Integration tests using RAPIDGenerator.CreateModule() since Movement's _convertedTarget is private and only set during the generator pipeline. 
 - Tests cover MoveAbsJ/MoveL/MoveJ instructions, predefined vs custom named speed data, target variable names, invalid JointTarget+MoveL/MoveJ combinations, and IsValid checks. All tests marked RequiresRhino. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `af3dc33` | **Date:** 2026-02-13 
 
 --- 
 
 - Add RobotTarget unit tests for RAPID code generation Tests cover constructors (plane-only, named, full args), ToRAPID() nested array format with quaternion/position/config/external axes, named ConfigurationData substitution, declarations, IsValid, Duplicate, and Parse/TryParse. All tests marked RequiresRhino. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `3c07aba` | **Date:** 2026-02-13 
 
 --- 
 
 - Add instruction unit tests for RAPID code generation Tests cover all simple instruction types: SetDigitalOutput (basic, delay, sync override), WaitTime (duration format, InPos), WaitDI/WaitDO (MaxTime, TimeFlag), WaitAI/WaitAO (InequalitySymbol LT/GT, MaxTime), WaitGI/WaitGO (int value, MaxTime), WaitRob (InPos/ZeroSpeed validity), CodeLine (Instruction/Declaration types), Comment (bang prefix format). 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `6645fed` | **Date:** 2026-02-13 
 
 --- 
 
 - Add JointTarget unit tests for RAPID code generation Tests cover constructors with RobotJointPosition/ExternalJointPosition, ToRAPID() nested array format, VAR jointtarget declarations, IsValid, Duplicate, and Parse/TryParse round-trip. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `0f0898c` | **Date:** 2026-02-13 
 
 --- 
 
 - Add ExternalJointPosition unit tests for RAPID code generation Tests cover constructors (default 9E9, single/multi axis, named, NaN handling), ToRAPID() format with 0.## specifier, declarations, indexer access (int and char), IsValid, Duplicate, and Length property. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `6a36363` | **Date:** 2026-02-13 
 
 --- 
 
 - Add ConfigurationData unit tests for RAPID code generation Tests cover constructors, ToRAPID() bracket format, CONST confdata declarations, scope/variable type variants, Duplicate, and Parse/TryParse. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `ffe9f6f` | **Date:** 2026-02-13 
 
 --- 
 
 - Add ZoneData unit tests for RAPID code generation Tests cover predefined zones (fine, z0, z10), nearest-snap behavior, custom constructors, ToRAPID() 7-element array format, declaration generation, IsValid boundary checks, Parse/TryParse, and static helpers. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `164a4aa` | **Date:** 2026-02-13 
 
 --- 
 
 - Add SpeedData unit tests for RAPID code generation Tests cover predefined/custom constructors, ToRAPID() bracket format, ToRAPIDDeclaration with scope/variable type variants, IsValid checks, Duplicate, Parse/TryParse, and static helper properties. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `2c956c0` | **Date:** 2026-02-13 
 
 --- 
 
 - Update test command to exclude 'RequiresRhino' category 
  
   **Commit:** `ca508da` | **Date:** 2026-02-13 
 
 --- 
 
 - Update test run command to exclude 'RequiresRhino' category 
  
   **Commit:** `658a5f7` | **Date:** 2026-02-13 
 
 --- 
 
 - Modify test command to filter out specific tests Updated test command to exclude tests with 'RequiresRhino' category. 
  
   **Commit:** `f7889c6` | **Date:** 2026-02-13 
 
 --- 
 
 - Tag native Rhino-dependent tests with RequiresRhino trait 14 tests that call rhcommon_c native code (Plane constructors, Quaternion.Unitize, QuaternionToPlane) are tagged with [Trait("Category", "RequiresRhino")] so CI can filter them out. 
 - Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `f8ca072` | **Date:** 2026-02-13 
 
 --- 
 
 - Add InternalsVisibleTo for test project access to internal APIs Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `af1c89b` | **Date:** 2026-02-13 
 
 --- 
 
 - Add Phase 1 unit tests for helper methods, preset helpers, and version numbering Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `5a588bf` | **Date:** 2026-02-13 
 
 --- 
 
 - Copy RhinoCommon DLLs to test output for CI runtime resolution Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com> 
  
   **Commit:** `9c2c852` | **Date:** 2026-02-13 
 
 --- 
 
 - Skip CreateRelease.ps1 in non-interactive environments Add check to skip execution in non-interactive environments. 
  
   **Commit:** `490ce44` | **Date:** 2026-02-13 
 
 --- 
 
 - Change branch from 'ikgeo' to 'HEAD' 
  
   **Commit:** `e0ac8c5` | **Date:** 2026-02-13 
 
 --- 
 
 - Add fetch-depth option to checkout step 
  
   **Commit:** `d8f0114` | **Date:** 2026-02-13 
 
 --- 
 
 - Add fetch-depth option to checkout step 
  
   **Commit:** `4fcac49` | **Date:** 2026-02-13 
 
 --- 
 
 - Update fetch-depth for checkout action in CI Set fetch-depth to 0 for the checkout action. 
  
   **Commit:** `2446f55` | **Date:** 2026-02-13 
 
 --- 
 
 - Integrate VSTest setup into release workflow Added setup step for VSTest and simplified test execution. 
  
   **Commit:** `cc62c2b` | **Date:** 2026-02-13 
 
 --- 
 
 - Add VSTest setup step and simplify test execution 
  
   **Commit:** `e0360c2` | **Date:** 2026-02-13 
 
 --- 
 
 - Add VSTest setup and simplify test execution 
  
   **Commit:** `f6351e8` | **Date:** 2026-02-13 
 
 --- 
 
 - Add GitHub Actions workflow for release process 
  
   **Commit:** `73a13b8` | **Date:** 2026-02-13 
 
 --- 
 
 - Add CI workflow for build and test on Windows 
  
   **Commit:** `3b1a17a` | **Date:** 2026-02-13 
 
 --- 
 
 - Also fix UploadHelperModules missing try/finally for master.Release() The catch block inside the foreach had return false, which exited the method without calling master.Release(). Moved try/catch to wrap the entire foreach and added finally for Release(). 
  
   **Commit:** `ee03122` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix RunProgram/StopProgram missing try/finally for master.Release() If Rapid.Start() or Rapid.Stop() threw an exception, master.Release() was never called, leaving the controller mastership locked. Now wrapped in try/finally matching the pattern used in ResetProgramPointers(). 
  
   **Commit:** `8d8f98d` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix RobotComponent outputting empty robot on construction failure The catch block logged the error but didn't return, so an empty Robot() instance was output downstream, masking the actual failure. 
  
   **Commit:** `e23e4c4` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix RobotTool.DuplicateWithoutMesh leaving _mesh null The else branch had the mesh initialization commented out, leaving _mesh null and causing NullReferenceException in Transform() and other methods. 
  
   **Commit:** `152a501` | **Date:** 2026-02-12 
 
 --- 
 
 - Also fix WaitAI copy constructor dropping inequality symbol Same bug as WaitAO: the copy constructor omitted _inequalitySymbol. 
  
   **Commit:** `b906178` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix WaitAO copy constructor dropping inequality symbol The copy constructor omitted _inequalitySymbol, so Duplicate() would always reset it to the default enum value (0). 
  
   **Commit:** `8db2ba5` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix serialization key/type mismatches in Wait* instructions - WaitTime: fix key typo "In Postion" ÔåÆ "In Position" in GetObjectData - WaitDI/WaitDO: fix _timeFlag serialized as typeof(double) ÔåÆ typeof(bool) - WaitAO: fix "Inequality Symbol" serializing _value instead of _inequalitySymbol 
  
   **Commit:** `2824965` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix event handler leak in 12 additional components Same bug as RobotToolComponent/WorkObjectComponent: doc.ObjectsDeleted was subscribed every solve without unsubscribing. Covers LoadData, SyncMoveOn/Off, TaskList, WaitSyncTask, ZoneData, ConfigurationData, SpeedData, JointTarget, ExternalJointPosition, RobotTarget, and RobotJointPosition components. 
  
   **Commit:** `9d7506d` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix event handler leak in RobotTool and WorkObject components doc.ObjectsDeleted was subscribed every solve cycle without unsubscribing, stacking duplicate handlers. Now unsubscribes before subscribing to prevent the leak. 
  
   **Commit:** `ac39646` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix MenuItemClickOutputPlanes toggling wrong field MenuItemClickOutputPlanes was toggling _outputMeshParameter instead of _outputPosedPlanesParameter, causing both menu items to control the mesh toggle and the planes toggle to have no effect. 
  
   **Commit:** `1305585` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix special character validation calling wrong method in 11 components The second validation check duplicated StringStartsWithNumber() instead of calling StringHasSpecialCharacters(), so signal/module names with special characters were never flagged. Also fixes "constains" typo to "contains". 
  
   **Commit:** `8f14263` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix Signal.SetValue missing return on empty check When _isEmpty was true, the method set an error message but continued executing, causing a NullReferenceException when accessing _signal.Name and _limits on a null signal. 
  
   **Commit:** `cb30501` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix PoseMeshes mutating Robot internal mesh list PoseMeshes() was appending external axis meshes to the internal _meshes field instead of the local meshes variable. This caused the robot's permanent mesh list to grow on every call. 
  
   **Commit:** `f365525` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix WaitGO and WaitGI generating WaitDI RAPID instruction Both classes were copy-pasted from WaitDI but the RAPID instruction name was never updated. WaitGO now generates "WaitGO" and WaitGI generates "WaitGI". Also fixed XML doc comments. 
  
   **Commit:** `3e037d4` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix shoulder singularity detection no-op in OPWKinematics The LINQ .Select() result was never materialized, so the _shoulderSingularities array was never updated. Also fixed bitwise & to logical && in the condition. Use a for loop instead. 
  
   **Commit:** `c1cbb52` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix GetExternalJointPositions indexing wrong list The loop iterated over _externalAxes but read positions from _mechanicalUnits[i], which contains robots first then external axes. This returned robot positions instead of external axis positions when robots were present. 
  
   **Commit:** `9b8e43b` | **Date:** 2026-02-12 
 
 --- 
 
 - Add input caching to PathGeneratorComponent - Improves performance by introducing input change detection and caching in PathGeneratorComponent. 
 	 - The component now computes a unique input hash based on robot, tool, base, external axes, and action parameters, recalculating paths only when inputs change or update is triggered. 
 	 - Added helper methods for hashing and ungrouping actions, and ensured internal lists are properly sized per iteration. 
 	 - Also updated using directives and advanced the changelog script's commit reference. 
  
   **Commit:** `dade86d` | **Date:** 2026-02-18 
 
 --- 
 
 - Add input caching to PathGeneratorComponent - Improves performance by introducing input change detection and caching in PathGeneratorComponent. 
 	 - The component now computes a unique input hash based on robot, tool, base, external axes, and action parameters, recalculating paths only when inputs change or update is triggered. 
 	 - Added helper methods for hashing and ungrouping actions, and ensured internal lists are properly sized per iteration. 
 	 - Also updated using directives and advanced the changelog script's commit reference. 
  
   **Commit:** `78c2c32` | **Date:** 2026-02-18 
 
 --- 
 
 - Merge branch 'ikgeo' 
  
   **Commit:** `c428c76` | **Date:** 2026-02-17 
 
 --- 
 
 - Refactor user credential handling and NaN checks - Refactored the Controller class to remove redundant _userName and _password fields, using the _userInfo object directly for user credential management. 
 	 - Updated logging and property access to reference _userInfo.Name. 
 	 - Simplified SetUserInfo and SetDefaultUser methods. 
 	 - Fixed RobotKinematicParameters.IsValid to use double.IsNaN() for proper NaN checking. 
 	 - Added early returns after error logs to improve control flow. 
  
   **Commit:** `d3ccb5d` | **Date:** 2026-02-17 
 
 --- 
 
 - Fix missing early returns in Controller and remove redundant credential fields Initiliaze(), SetUserInfo(), and SetDefaultUser() all continued executing after detecting _isEmpty, causing null dereferences. Added early returns. 
 - Removed redundant _userName/_password fields that duplicated data already in _userInfo. SetDefaultUser() was updating the strings but not _userInfo, causing Logon() to use stale credentials. Now _userInfo is the single source of truth. 
 - Pull Request by FilipHae 
  
   **Commit:** `0e2b71d` | **Date:** 2026-02-17 
 
 --- 
 
 - Fix NaN comparison in RobotKinematicParameters.IsValid. 
 - Pull request by FilipHae. 
  
   **Commit:** `262a15c` | **Date:** 2026-02-17 
 
 --- 
 
 - Add Message Box component and improve declaration handling - Introduced MessageBoxComponent for RAPID message box code generation with customizable buttons and actions. 
 	 - Added MessageBox_Icon.png and registered it in resources. 
 	 - Enhanced uniqueness checks for program declarations in RAPIDGenerator.cs and refactored GetDeclarationName to handle local declarations. 
  
   **Commit:** `71ee1c7` | **Date:** 2026-02-17 
 
 --- 
 
 - Commits further Icons, which were previously not accounted for. 
 - Icons: - LoadModule - MessageBox 
  
   **Commit:** `0b2ed16` | **Date:** 2026-02-17 
 
 --- 
 
 - Changed Copyright notice in all files to current year. 
  
   **Commit:** `6e5d826` | **Date:** 2026-02-12 
 
 --- 
 
 - Add Connect Interrupt component and Signal Type value list - Introduced ConnectInterruptComponent for RAPID interrupt code generation. These connect Traps to signal changes implementing RAPID interrupt behaviour. 
 	 - Added SignalType enum and SignalTypeValueList for user-friendly signal type input. 
 	 - Updated resources and icons for new components. 
 	 - Minor cleanup in AdditionalRoutineComponent. 
  
   **Commit:** `816e2a8` | **Date:** 2026-02-12 
 
 --- 
 
 - Commits some Icons, which were previously not accounted for. 
 - Icons: - Check Actions - Interrupt Connection - Deconstruct Group Signal 
  
   **Commit:** `c9840e8` | **Date:** 2026-02-12 
 
 --- 
 
 - Updated CHANGELOG to reflect current status. Updated UpdateChangeLog script to actually generate correct changelogs... 
  
   **Commit:** `d10498b` | **Date:** 2026-02-10 
 
 --- 
 
 - Improve routine call arg handling - Clarify "Keyword" input description in RoutineArgumentComponent - Enhance RoutineCallComponent to call ToRAPID on objects implementing iDeclaration. 
  
   **Commit:** `d28066b` | **Date:** 2026-02-10 
 
 --- 
 
 - Add RAPID WaitRob instruction and GH component support - Implement WaitRob class for RAPID WaitRob instruction (InPos/ZeroSpeed) - Add GH_WaitRob Goo, WaitRobComponent, and Param_WaitRob - Register new icons and update resources for WaitRob 
  
   **Commit:** `4a394e5` | **Date:** 2026-02-10 
 
 --- 
 
 - Add RAPID system module support and module loading tools - Added UploadSystemModule to Controller for .SYS module upload, config, and warm restart - UploadModule now detects and delegates system modules - Added LoadModuleComponent for RAPID module load/unload code generation - RoutineCallComponent: support for cross-module routine calls - Fixed routine scope input index in RAPIDGeneratorComponent - Added new icons and registered in resources - Updated copyright and CHANGELOG - Minor codegen warnings and documentation improvements 
  
   **Commit:** `2dd9527` | **Date:** 2026-01-29 
 
 --- 
 
 - Add WaitAO/DO/GI/GO instructions and GH components and parameters. 
 	 - Introduced WaitAO, WaitDO, WaitGI, and WaitGO instruction classes with serialization and RAPID code generation. 
 	 - Added corresponding Grasshopper components, parameter types, and Goo wrappers for each wait instruction. 
 	 - Updated icons and resources for new types. 
 	 - Updated CHANGELOG. 
  
   **Commit:** `1c33b8f` | **Date:** 2026-01-27 
 
 --- 
 
 - Adds support for RAPID System Modules and Option for loading additional modules into the task. 
 	 - RAPIDGenerator and controller methods now support system modules (.SYS), generating correct RAPID headers and file extensions. 
 	 - Grasshopper RAPIDGeneratorComponent exposes "Is System Module" input and UI. 
 	 - UploadHelperModulesComponent adds "Load To Task" input. 
 	 - Improved module name extraction and file cleanup. 
 	 - Warnings for invalid system module/routine names. 
  
   **Commit:** `de67e89` | **Date:** 2026-01-22 
 
 --- 
 
 - Adds support for group signals. Group signals are bitmasks used to communicate groups of digital signals. 
 - Comprehensive support for group inputs/outputs: - Controller class now manages group signals with new retrieval methods and properties. 
 	 - Added Grasshopper components for getting/setting group inputs/outputs, including bitmask support and signal picking UI. 
 	 - Introduced SetGroupOutput instruction, Goo, and parameter classes for RAPID code generation. 
 	 - New DeconstructGroupSignal component for bitwise signal analysis. 
 	 - Updated resources and icons for new components. 
  
   **Commit:** `04296e1` | **Date:** 2026-01-22 
 
 --- 
 
 - Adds support for user-defined RAPID routine arguments. 
 	 - Introduces RoutineArgument class and Grasshopper components for defining and calling routines with arguments. 
 	 - Updates code generation, serialization, and UI to support variable arguments in PROC routines, with new icons and changelog entries. 
  
   **Commit:** `6ae908e` | **Date:** 2026-01-21 
 
 --- 
 
 - Add simple RAPID routine definition. Procedures (PROC) and Interrupts (TRAP) can now be defined. No functions or arguments are supported. 
 	 - Introduce Routine class for user-defined PROC/TRAP routines with scope (GLOBAL/LOCAL/TASK). 
 	 - Add Param_Routine and GH_Routine for Grasshopper integration. 
 	 - Implement AdditionalRoutineComponent for custom routines. 
 	 - Update RAPIDGenerator and RAPIDGeneratorComponent to handle additional routines and routine scope. 
 	 - Add ScopeValueList and RoutineTypeValueList components for easy selection. 
 	 - Update icons/resources for new features. 
 	 - Update Scope declaration in RAPID Code generation. 
  
   **Commit:** `d150e37` | **Date:** 2026-01-08 
 
 --- 
 
 - Fixed helper module upload & modular RAPID code support - Controller now clears local additional directory before writing new files to prevent stale files. 
 	 - Removed legacy GLOBAL declaration parsing from RAPIDGenerator, as a GLOBAL keyword doesn't exist in RAPID. 
  
   **Commit:** `d525057` | **Date:** 2026-01-07 
 
 --- 
 
 - Add helper module upload & modular RAPID code support - Added Controller.UploadHelperModules for uploading additional RAPID modules to controller storage without overwriting the main program. 
 	 - Introduced UploadHelperModulesComponent for Grasshopper, enabling users to upload helper modules. 
 	 - Enhanced RAPIDGenerator and RAPIDGeneratorComponent to support a "Superordinate Main Method" input, filtering out duplicate global declarations in helper modules. 
 	 - Improved input parameter management and context menu in RAPIDGeneratorComponent. 
 	 - Updated changelog and project file for new features and dependencies. 
  
   **Commit:** `9ef868c` | **Date:** 2026-01-03 
 
 --- 
 
 - Add LOCAL routine option to RAPIDGeneratorComponent - Added context menu option to declare RAPID routines as LOCAL - Updated RAPIDGenerator to support LOCAL keyword in code output - Preserved LOCAL setting in serialization and duplication - Improved multi-iteration handling in RAPIDGeneratorComponent - Fixed minor documentation and comment issues 
  
   **Commit:** `3da06c8` | **Date:** 2025-12-09 
 
 --- 
 
 - Update Changelog. 
  
   **Commit:** `5ea2596` | **Date:** 2025-11-20 
 
 --- 
 
 - Changed Rhino Common version to 7.36. 
  
   **Commit:** `ca4b890` | **Date:** 2025-11-20 
 
 --- 
 
 - Included dependency license files. 
  
   **Commit:** `13a62a1` | **Date:** 2025-11-17 
 
 --- 
 
 - Updated Changelog Builder and Changelog. 
  
   **Commit:** `4c96b6d` | **Date:** 2025-11-14 
 
 --- 
 
 - Implemented Changelog Generator 
  
   **Commit:** `44d50dc` | **Date:** 2025-11-14 
 
 --- 
 
 - Updated documentation and acknowledgments in AUTHORS.md and README.md to reflect the modified version of the project. Added Jan Philipp Drude and Johannes Pfleging as contributors. 
 - Updated SPDX license headers across all modified files to acknowledge the original and modified projects. Updated copyright information to include "2025 EDEK Uni Kassel." 
  
   **Commit:** `e6a5888` | **Date:** 2025-11-14 
 
 --- 
 
 - Add CheckActionsComponent and automate release process Introduced a new Grasshopper component, `CheckActionsComponent`, to validate robot actions and provide feedback. Added a corresponding icon (`CheckActions_Icon.png`) and localized resource for the UI. 
 - Updated the build process to include a `PostBuild` target in the project file, executing a new PowerShell script (`CreateRelease.ps1`) to automate release packaging and GitHub release creation. The script generates a zip archive, an `INSTALL.md` file, and uploads the release. 
  
   **Commit:** `be44cdb` | **Date:** 2025-11-14 
 
 --- 
 
 - Ommited to meters and to fileUnits conversion as Robot Components is entirely in mm, no matter the Rhino file units. 
  
   **Commit:** `178bdac` | **Date:** 2025-11-11 
 
 --- 
 
 - IkGeo doesnt reveal solutions when target x = 0. Targets are therefore slightly offset for this case in the IkGeo solver compute method. 
  
   **Commit:** `49e257a` | **Date:** 2025-11-11 
 
 --- 
 
 - Added my contact to contributors. 
  
   **Commit:** `6607b74` | **Date:** 2025-11-05 
 
 --- 
 
 - implements constructing configuration data from quadrant data inputs. 
 - Builds configuration data (cfx) from quarant data (cf1, cf4, cf6) as bitmask, if no cfx is provided. Also deconstructs cfx into quadrant data. 
  
   **Commit:** `36a348a` | **Date:** 2025-11-05 
 
 --- 
 
 - add singularity detection for CRB15000 robots using Jacobian analysis Implemented comprehensive singularity detection in the IkGeo solver for CRB15000 (GoFa) robots. Added fields for tracking wrist, elbow, shoulder singularities and missing solver results across all eight Cfx configurations. Introduced MathNet.Numerics dependency for Jacobian matrix operations. 
 - Implemented `ComputeSingularities` method to detect near-singular configurations using SVD-based Jacobian analysis with geometric alignment checks. Added `BuildJacobian` and `CheckJacobianSingularity` helper methods to construct the 6x6 manipulator Jacobian and evaluate singularity conditions using a relative tolerance threshold. 
 - Updated `InverseKinematics` class to retrieve and propagate singularity data from the IkGeo solver, including a sentinel value system (9e9) for missing solutions. Modified `CheckInternalAxisLimits` to skip validation for missing joint values and report when the solver returns no result. Added public properties `WristSingularities`, `ElbowSingularities`, `ShoulderSingularities`, and `NoSolverResults` to expose singularity information. 
  
   **Commit:** `ed4bbe7` | **Date:** 2025-11-05 
 
 --- 
 
 - Fixed issues with Robot Components GH UI. 
 - Added the PosedInternalPlanes parameter to the variable output parameters array in the IK component. 
 - Added the CRB15000 robots to the enumeration of available robot presets. 
  
   **Commit:** `22fc7bf` | **Date:** 2025-11-05 
 
 --- 
 
 - Add support for outputting posed planes in IK component (optional) Introduce a new feature to output posed planes in the `InverseKinematicsComponent` class. Added a `_outputPosedPlanesParameter` field to manage this functionality and updated the context menu with a new "Output Posed Axis Planes" option. 
 - Implemented the `MenuItemClickOutputPlanes` event handler to toggle the posed planes output and added serialization/deserialization support for this parameter in the `Write` and `Read` methods. 
 - Created the `GetPosedPlanesDataTree` method to transform posed axis planes into a structured data tree for output. Updated the `SolveInstance` method to populate the posed planes output parameter when enabled. 
  
   **Commit:** `f100270` | **Date:** 2025-11-05 
 
 --- 
 
 - Implemented Axis Configuration sorting into IkGeoSolver and posed internal axis planes into InverseKinematics. 
 - Added `_posedInternalAxisPlanes` to `ForwardKinematics` to store posed internal axis planes and updated calculations to compute and expose these planes via a new public property. 
 - Introduced `_missingJointValue` sentinel in `IkGeoSolver` and implemented `ArrangeJointPositions` to sort IK solutions into RAPID's 8-slot Cfx ordering. This method uses forward kinematics and geometric tests to compute Cf1, Cf4, and Cf6 bitmask values. 
  
   **Commit:** `021b87d` | **Date:** 2025-11-05 
 
 --- 
 
 - Add IkGeoSolver for GoFa CRB15000 robots to InverseKinematcs class Introduce IkGeoSolver to handle inverse kinematics for GoFa CRB15000 robots, while retaining OPW/Wrist Offset solvers for other robots. 
 	 - Reorganize and update namespace imports, adding `IkGeoSolver`. 
 	 - Update `CalculateRobotJointPosition` to use IkGeoSolver for CRB15000 robots. 
 	 - Retain OPW/Wrist Offset solvers for other robot types. 
 	 - Initialize singularity arrays for compatibility with IkGeoSolver. 
  
   **Commit:** `8a74478` | **Date:** 2025-11-05 
 
 --- 
 
 - Add IK solver for CRB15000 robots and supporting structs Introduced an inverse kinematics solver (`IkGeoSolver`) for CRB15000 (GoFa) robots, wrapping the native `ik-geo` library. Added binary dependencies (`ikgeoInterface_GoFa.dll`, `libgcc_s_seh-1.dll`, `libstdc++-6.dll`, `libwinpthread-1.dll`) required for the solver. 
 - Added supporting geometry structs: - `Quaternion`: Represents quaternions with conversion methods. 
 	 - `Vector3d`: Represents 3D vectors with Rhino type conversions. 
 	 - `Vector6d`: Represents 6D robot joint positions with utility methods. 
 - Implemented `Compute_CRB15000` to calculate IK solutions, handle singularities, and convert results to robot configurations. Added detailed documentation for all new components. 
  
   **Commit:** `2865df2` | **Date:** 2025-11-05 
 
 --- 
 
 - Add Message Box component and improve declaration handling - Introduced MessageBoxComponent for RAPID message box code generation with customizable buttons and actions. 
 	 - Added MessageBox_Icon.png and registered it in resources. 
 	 - Enhanced uniqueness checks for program declarations in RAPIDGenerator.cs and refactored GetDeclarationName to handle local declarations. 
  
   **Commit:** `752d314` | **Date:** 2026-02-17 
 
 --- 
 
 - Changed Copyright notice in all files to current year. 
  
   **Commit:** `fb9578d` | **Date:** 2026-02-12 
 
 --- 
 
 - Add Connect Interrupt component and Signal Type value list - Introduced ConnectInterruptComponent for RAPID interrupt code generation. These connect Traps to signal changes implementing RAPID interrupt behaviour. 
 	 - Added SignalType enum and SignalTypeValueList for user-friendly signal type input. 
 	 - Updated resources and icons for new components. 
 	 - Minor cleanup in AdditionalRoutineComponent. 
  
   **Commit:** `363ea4b` | **Date:** 2026-02-12 
 
 --- 
 
 - Updated CHANGELOG to reflect current status. Updated UpdateChangeLog script to actually generate correct changelogs... 
  
   **Commit:** `a9607e2` | **Date:** 2026-02-10 
 
 --- 
 
 - Improve routine call arg handling - Clarify "Keyword" input description in RoutineArgumentComponent - Enhance RoutineCallComponent to call ToRAPID on objects implementing iDeclaration. 
  
   **Commit:** `75607ab` | **Date:** 2026-02-10 
 
 --- 
 
 - Add RAPID WaitRob instruction and GH component support - Implement WaitRob class for RAPID WaitRob instruction (InPos/ZeroSpeed) - Add GH_WaitRob Goo, WaitRobComponent, and Param_WaitRob - Register new icons and update resources for WaitRob 
  
   **Commit:** `65f6409` | **Date:** 2026-02-10 
 
 --- 
 
 - Add RAPID system module support and module loading tools - Added UploadSystemModule to Controller for .SYS module upload, config, and warm restart - UploadModule now detects and delegates system modules - Added LoadModuleComponent for RAPID module load/unload code generation - RoutineCallComponent: support for cross-module routine calls - Fixed routine scope input index in RAPIDGeneratorComponent - Added new icons and registered in resources - Updated copyright and CHANGELOG - Minor codegen warnings and documentation improvements 
  
   **Commit:** `0b4c517` | **Date:** 2026-01-29 
 
 --- 
 
 - Add WaitAO/DO/GI/GO instructions and GH components and parameters. 
 	 - Introduced WaitAO, WaitDO, WaitGI, and WaitGO instruction classes with serialization and RAPID code generation. 
 	 - Added corresponding Grasshopper components, parameter types, and Goo wrappers for each wait instruction. 
 	 - Updated icons and resources for new types. 
 	 - Updated CHANGELOG. 
  
   **Commit:** `dab24ab` | **Date:** 2026-01-27 
 
 --- 
 
 - Adds support for RAPID System Modules and Option for loading additional modules into the task. 
 	 - RAPIDGenerator and controller methods now support system modules (.SYS), generating correct RAPID headers and file extensions. 
 	 - Grasshopper RAPIDGeneratorComponent exposes "Is System Module" input and UI. 
 	 - UploadHelperModulesComponent adds "Load To Task" input. 
 	 - Improved module name extraction and file cleanup. 
 	 - Warnings for invalid system module/routine names. 
  
   **Commit:** `2f0c850` | **Date:** 2026-01-22 
 
 --- 
 
 - Adds support for group signals. Group signals are bitmasks used to communicate groups of digital signals. 
 - Comprehensive support for group inputs/outputs: - Controller class now manages group signals with new retrieval methods and properties. 
 	 - Added Grasshopper components for getting/setting group inputs/outputs, including bitmask support and signal picking UI. 
 	 - Introduced SetGroupOutput instruction, Goo, and parameter classes for RAPID code generation. 
 	 - New DeconstructGroupSignal component for bitwise signal analysis. 
 	 - Updated resources and icons for new components. 
  
   **Commit:** `735cac5` | **Date:** 2026-01-22 
 
 --- 
 
 - Adds support for user-defined RAPID routine arguments. 
 	 - Introduces RoutineArgument class and Grasshopper components for defining and calling routines with arguments. 
 	 - Updates code generation, serialization, and UI to support variable arguments in PROC routines, with new icons and changelog entries. 
  
   **Commit:** `b2f1286` | **Date:** 2026-01-21 
 
 --- 
 
 - Add simple RAPID routine definition. Procedures (PROC) and Interrupts (TRAP) can now be defined. No functions or arguments are supported. 
 	 - Introduce Routine class for user-defined PROC/TRAP routines with scope (GLOBAL/LOCAL/TASK). 
 	 - Add Param_Routine and GH_Routine for Grasshopper integration. 
 	 - Implement AdditionalRoutineComponent for custom routines. 
 	 - Update RAPIDGenerator and RAPIDGeneratorComponent to handle additional routines and routine scope. 
 	 - Add ScopeValueList and RoutineTypeValueList components for easy selection. 
 	 - Update icons/resources for new features. 
 	 - Update Scope declaration in RAPID Code generation. 
  
   **Commit:** `339bbd0` | **Date:** 2026-01-08 
 
 --- 
 
 - Fixed helper module upload & modular RAPID code support - Controller now clears local additional directory before writing new files to prevent stale files. 
 	 - Removed legacy GLOBAL declaration parsing from RAPIDGenerator, as a GLOBAL keyword doesn't exist in RAPID. 
  
   **Commit:** `5eb4ddd` | **Date:** 2026-01-07 
 
 --- 
 
 - Add helper module upload & modular RAPID code support - Added Controller.UploadHelperModules for uploading additional RAPID modules to controller storage without overwriting the main program. 
 	 - Introduced UploadHelperModulesComponent for Grasshopper, enabling users to upload helper modules. 
 	 - Enhanced RAPIDGenerator and RAPIDGeneratorComponent to support a "Superordinate Main Method" input, filtering out duplicate global declarations in helper modules. 
 	 - Improved input parameter management and context menu in RAPIDGeneratorComponent. 
 	 - Updated changelog and project file for new features and dependencies. 
  
   **Commit:** `02c7755` | **Date:** 2026-01-03 
 
 --- 
 
 - Add LOCAL routine option to RAPIDGeneratorComponent - Added context menu option to declare RAPID routines as LOCAL - Updated RAPIDGenerator to support LOCAL keyword in code output - Preserved LOCAL setting in serialization and duplication - Improved multi-iteration handling in RAPIDGeneratorComponent - Fixed minor documentation and comment issues 
  
   **Commit:** `e1b8f81` | **Date:** 2025-12-09 
 
 --- 
 
 - Update Changelog. 
  
   **Commit:** `121548a` | **Date:** 2025-11-20 
 
 --- 
 
 - Changed Rhino Common version to 7.36. 
  
   **Commit:** `899fef5` | **Date:** 2025-11-20 
 
 --- 
 
 - Included dependency license files. 
  
   **Commit:** `1880f27` | **Date:** 2025-11-17 
 
 --- 
 
 - Updated Changelog Builder and Changelog. 
  
   **Commit:** `7596141` | **Date:** 2025-11-14 
 
 --- 
 
 - Implemented Changelog Generator 
  
   **Commit:** `c8e811e` | **Date:** 2025-11-14 
 
 --- 
 
 - Updated documentation and acknowledgments in AUTHORS.md and README.md to reflect the modified version of the project. Added Jan Philipp Drude and Johannes Pfleging as contributors. 
 - Updated SPDX license headers across all modified files to acknowledge the original and modified projects. Updated copyright information to include "2025 EDEK Uni Kassel." 
  
   **Commit:** `1ec4f4c` | **Date:** 2025-11-14 
 
 --- 
 
 - Add CheckActionsComponent and automate release process Introduced a new Grasshopper component, `CheckActionsComponent`, to validate robot actions and provide feedback. Added a corresponding icon (`CheckActions_Icon.png`) and localized resource for the UI. 
 - Updated the build process to include a `PostBuild` target in the project file, executing a new PowerShell script (`CreateRelease.ps1`) to automate release packaging and GitHub release creation. The script generates a zip archive, an `INSTALL.md` file, and uploads the release. 
  
   **Commit:** `00a671a` | **Date:** 2025-11-14 
 
 --- 
 
 - Ommited to meters and to fileUnits conversion as Robot Components is entirely in mm, no matter the Rhino file units. 
  
   **Commit:** `120528e` | **Date:** 2025-11-11 
 
 --- 
 
 - IkGeo doesnt reveal solutions when target x = 0. Targets are therefore slightly offset for this case in the IkGeo solver compute method. 
  
   **Commit:** `6a61c2e` | **Date:** 2025-11-11 
 
 --- 
 
 - Added my contact to contributors. 
  
   **Commit:** `2831f74` | **Date:** 2025-11-05 
 
 --- 
 
 - implements constructing configuration data from quadrant data inputs. 
 - Builds configuration data (cfx) from quarant data (cf1, cf4, cf6) as bitmask, if no cfx is provided. Also deconstructs cfx into quadrant data. 
  
   **Commit:** `bbe4c31` | **Date:** 2025-11-05 
 
 --- 
 
 - add singularity detection for CRB15000 robots using Jacobian analysis Implemented comprehensive singularity detection in the IkGeo solver for CRB15000 (GoFa) robots. Added fields for tracking wrist, elbow, shoulder singularities and missing solver results across all eight Cfx configurations. Introduced MathNet.Numerics dependency for Jacobian matrix operations. 
 - Implemented `ComputeSingularities` method to detect near-singular configurations using SVD-based Jacobian analysis with geometric alignment checks. Added `BuildJacobian` and `CheckJacobianSingularity` helper methods to construct the 6x6 manipulator Jacobian and evaluate singularity conditions using a relative tolerance threshold. 
 - Updated `InverseKinematics` class to retrieve and propagate singularity data from the IkGeo solver, including a sentinel value system (9e9) for missing solutions. Modified `CheckInternalAxisLimits` to skip validation for missing joint values and report when the solver returns no result. Added public properties `WristSingularities`, `ElbowSingularities`, `ShoulderSingularities`, and `NoSolverResults` to expose singularity information. 
  
   **Commit:** `c9e4f01` | **Date:** 2025-11-05 
 
 --- 
 
 - Fixed issues with Robot Components GH UI. 
 - Added the PosedInternalPlanes parameter to the variable output parameters array in the IK component. 
 - Added the CRB15000 robots to the enumeration of available robot presets. 
  
   **Commit:** `637ac7f` | **Date:** 2025-11-05 
 
 --- 
 
 - Add support for outputting posed planes in IK component (optional) Introduce a new feature to output posed planes in the `InverseKinematicsComponent` class. Added a `_outputPosedPlanesParameter` field to manage this functionality and updated the context menu with a new "Output Posed Axis Planes" option. 
 - Implemented the `MenuItemClickOutputPlanes` event handler to toggle the posed planes output and added serialization/deserialization support for this parameter in the `Write` and `Read` methods. 
 - Created the `GetPosedPlanesDataTree` method to transform posed axis planes into a structured data tree for output. Updated the `SolveInstance` method to populate the posed planes output parameter when enabled. 
  
   **Commit:** `1f4edd1` | **Date:** 2025-11-05 
 
 --- 
 
 - Implemented Axis Configuration sorting into IkGeoSolver and posed internal axis planes into InverseKinematics. 
 - Added `_posedInternalAxisPlanes` to `ForwardKinematics` to store posed internal axis planes and updated calculations to compute and expose these planes via a new public property. 
 - Introduced `_missingJointValue` sentinel in `IkGeoSolver` and implemented `ArrangeJointPositions` to sort IK solutions into RAPID's 8-slot Cfx ordering. This method uses forward kinematics and geometric tests to compute Cf1, Cf4, and Cf6 bitmask values. 
  
   **Commit:** `eeefebf` | **Date:** 2025-11-05 
 
 --- 
 
 - Add IkGeoSolver for GoFa CRB15000 robots to InverseKinematcs class Introduce IkGeoSolver to handle inverse kinematics for GoFa CRB15000 robots, while retaining OPW/Wrist Offset solvers for other robots. 
 	 - Reorganize and update namespace imports, adding `IkGeoSolver`. 
 	 - Update `CalculateRobotJointPosition` to use IkGeoSolver for CRB15000 robots. 
 	 - Retain OPW/Wrist Offset solvers for other robot types. 
 	 - Initialize singularity arrays for compatibility with IkGeoSolver. 
  
   **Commit:** `08733c3` | **Date:** 2025-11-05 
 
 --- 
 
 - Add IK solver for CRB15000 robots and supporting structs Introduced an inverse kinematics solver (`IkGeoSolver`) for CRB15000 (GoFa) robots, wrapping the native `ik-geo` library. Added binary dependencies (`ikgeoInterface_GoFa.dll`, `libgcc_s_seh-1.dll`, `libstdc++-6.dll`, `libwinpthread-1.dll`) required for the solver. 
 - Added supporting geometry structs: - `Quaternion`: Represents quaternions with conversion methods. 
 	 - `Vector3d`: Represents 3D vectors with Rhino type conversions. 
 	 - `Vector6d`: Represents 6D robot joint positions with utility methods. 
 - Implemented `Compute_CRB15000` to calculate IK solutions, handle singularities, and convert results to robot configurations. Added detailed documentation for all new components. 
  
   **Commit:** `5c784da` | **Date:** 2025-11-05 
 
 --- 
 
 - Add LoadModule and MessageBox Icons 
  
   **Commit:** `f4925c8` | **Date:** 2026-02-17 
 
 --- 
 
 - Add Message Box component and improve declaration handling - Introduced MessageBoxComponent for RAPID message box code generation with customizable buttons and actions. 
 	 - Added MessageBox_Icon.png and registered it in resources. 
 	 - Enhanced uniqueness checks for program declarations in RAPIDGenerator.cs and refactored GetDeclarationName to handle local declarations. 
  
   **Commit:** `198ebaa` | **Date:** 2026-02-17 
 
 --- 
 
 - Changed Copyright notice in all files to current year. 
  
   **Commit:** `e92ce29` | **Date:** 2026-02-12 
 
 --- 
 
 - New Icons Icons for: - Check Actions - Interrupt Connection - Deconstruct Group Signal 
  
   **Commit:** `d2fd732` | **Date:** 2026-02-12 
 
 --- 
 
 - Add Connect Interrupt component and Signal Type value list - Introduced ConnectInterruptComponent for RAPID interrupt code generation. These connect Traps to signal changes implementing RAPID interrupt behaviour. 
 	 - Added SignalType enum and SignalTypeValueList for user-friendly signal type input. 
 	 - Updated resources and icons for new components. 
 	 - Minor cleanup in AdditionalRoutineComponent. 
  
   **Commit:** `35782ae` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix missing early returns in Controller and remove redundant credential fields Initiliaze(), SetUserInfo(), and SetDefaultUser() all continued executing after detecting _isEmpty, causing null dereferences. 
 - Added early returns. 
 - Removed redundant _userName/_password fields that duplicated data already in _userInfo. SetDefaultUser() was updating the strings but not _userInfo, causing Logon() to use stale credentials. 
 - Now _userInfo is the single source of truth. 
  
   **Commit:** `7a1a814` | **Date:** 2026-02-12 
 
 --- 
 
 - Fix NaN comparison in RobotKinematicParameters.IsValid Per IEEE 754, NaN == NaN is always false, so the previous checks (_a1 == double.NaN) never triggered. IsValid always returned true, even for uninitialized instances. Use double.IsNaN() instead. 
  
   **Commit:** `f14c976` | **Date:** 2026-02-12 
 
 --- 
 
 - Updated CHANGELOG to reflect current status. Updated UpdateChangeLog script to actually generate correct changelogs... 
  
   **Commit:** `ad3ff92` | **Date:** 2026-02-10 
 
 --- 
 
 - Improve routine call arg handling - Clarify "Keyword" input description in RoutineArgumentComponent - Enhance RoutineCallComponent to call ToRAPID on objects implementing iDeclaration. 
  
   **Commit:** `b4517f2` | **Date:** 2026-02-10 
 
 --- 
 
 - Update copyright year to 2026 
  
   **Commit:** `1382290` | **Date:** 2026-02-10 
 
 --- 
 
 - Update active developer's end date for Arjen Deetman 
  
   **Commit:** `42d076c` | **Date:** 2026-02-10 
 
 --- 
 
 - Add RAPID WaitRob instruction and GH component support - Implement WaitRob class for RAPID WaitRob instruction (InPos/ZeroSpeed) - Add GH_WaitRob Goo, WaitRobComponent, and Param_WaitRob - Register new icons and update resources for WaitRob 
  
   **Commit:** `39531f5` | **Date:** 2026-02-10 
 
 --- 
 
 - Change year from 2025 to 2026 
  
   **Commit:** `7aee199` | **Date:** 2026-02-08 
 
 --- 
 
 - Update header of RobotKinematicParameters.cs 
  
   **Commit:** `0886a89` | **Date:** 2026-02-08 
 
 --- 
 
 - Update the version number 
  
   **Commit:** `b702c0d` | **Date:** 2026-02-08 
 
 --- 
 
 - Fix in conversion of kinematic parameters to axis planes and vice versa: changed the sign of kinematic parameter b 
  
   **Commit:** `deb6f35` | **Date:** 2026-02-08 
 
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
 
 - Update README.md 
  
   **Commit:** `b584408` | **Date:** 2025-12-21 
 
 --- 
 
 - Update example files 
  
   **Commit:** `b3726c0` | **Date:** 2025-12-19 
 
 --- 
 
 - Update version number 
  
   **Commit:** `908d45a` | **Date:** 2025-12-19 
 
 --- 
 
 - Merge pull request #198 from RobotComponents/v4 Initialized robot attributes 
  
   **Commit:** `5f02818` | **Date:** 2025-12-19 
 
 --- 
 
 - Initialized robot attributes 
  
   **Commit:** `a497208` | **Date:** 2025-12-19 
 
 --- 
 
 - Delete .github/workflows/dotnet-tests.yml 
  
   **Commit:** `b75a77b` | **Date:** 2025-12-19 
 
 --- 
 
 - Revise citation guidelines in README.md Updated citation instructions and added BibTeX entry. 
  
   **Commit:** `a04a651` | **Date:** 2025-12-19 
 
 --- 
 
 - Update section title from 'Cite' to 'How to cite' 
  
   **Commit:** `636024b` | **Date:** 2025-12-12 
 
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
 


