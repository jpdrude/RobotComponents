# Part B: Advanced RAPID

The components on the **Advanced RAPID Features** toolbar tab write real RAPID logic:
data, routines, decisions, loops, operator dialogs and interrupts. All of them are
<span class="rc-badge rc-badge--fork">Fork additions</span>. Each page explains the RAPID
behind the component, and every RAPID snippet is checked on an ABB Virtual Controller.

| Chapter | Components and types | RAPID behind it |
|---|---|---|
| [B0 RAPID primer](b0-rapid-primer.md) | — | Modules, routines, data types ([[abb:num]], [[abb:bool]], [[abb:string]], [[abb:robtarget]], [[abb:jointtarget]]), `VAR`/`PERS`/`CONST`, scope; reading a generated module line by line |
| [B1 Routines](b1-routines/index.md) | [[Routine]] · [[Routine Argument]] · [[Routine Call]] · [[Return]] · [[Load Module]] · [[Routine Types]] · [[Predefined Scopes]]; [[RAPID Generator]] inputs for additional routines, scope and error handling | `PROC`, `FUNC`, `TRAP`, arguments, [[abb:RETURN]], [[abb:Load]] |
| [B2 Data and expressions](b2-data/index.md) | [[RAPID Variable]] · [[Assign Variable Value]] · [[Get Array At Index]] · [[Comparison Expression]] · [[Comparison Operators]] · [[Inequality Symbols]] · the [[type:RAPID Expression]] type | declarations, `:=`, arrays, expressions and operators |
| [B3 Flow control](b3-flow-control/index.md) | [[IF Statement]] · [[While Loop]] · [[For Loop]] | [[abb:IF]]/`ELSEIF`/`ELSE`, [[abb:WHILE]], [[abb:FOR]] |
| [B4 Target arithmetic](b4-target-arithmetic/index.md) | [[Current Robot Target]] · [[Offset Target]] · [[Get Robtarget Members]]; variables as [[Move]] targets | [[abb:CRobT]], [[abb:Offs]], robtarget components; what simulation can and cannot preview |
| [B5 Operator interaction](b5-operator-interaction/index.md) | [[Message Box]] · [[Numeric Entry Box]] | [[abb:TPReadFK]] + [[abb:TEST]]/`CASE`, [[abb:TPReadNum]] |
| [B6 Interrupts](b6-interrupts/index.md) | [[Connect Interrupt]] · [[Signal Types]]; a [[Routine]] of type TRAP | `CONNECT … WITH`, `ISignalDI/DO/AI/AO/GI/GO` |
| [B7 Raw code](b7-raw-code/index.md) | [[Code]] · [[Code Types]] · [[Comment]] · [[Empty Line]] | hand-written RAPID lines, comments; the code-line sanitizer |
| [B8 Tutorials](b8-tutorials/index.md) | a row of points with a loop · one routine, two places · a function returning the current height · an operator-driven loop · counting sensor hits · looping over an array · a routine library | — |
| [B9 Pitfalls and limits](b9-pitfalls.md) | naming rules, scope conflicts, what the [[Path Generator]] ignores, upgrading old files | — |
