# Part B: Advanced RAPID

The components on the **Advanced RAPID Features** toolbar tab write real RAPID logic:
data, routines, decisions, loops, operator dialogs and interrupts. All of them are
<span class="rc-badge rc-badge--fork">Fork additions</span>. Each page explains the RAPID
behind the component, and every RAPID snippet is checked on an ABB Virtual Controller.

| Chapter | Components and types | RAPID behind it |
|---|---|---|
| [B0 RAPID primer](b0-rapid-primer.md) | — | Modules, routines, data types (`num`, `bool`, `string`, `robtarget`, `jointtarget`), `VAR`/`PERS`/`CONST`, scope; reading a generated module line by line |
| B1 Routines | [[Routine]] · [[Routine Argument]] · [[Routine Call]] · [[Return]] · [[Load Module]] · [[Routine Types]] · [[Predefined Scopes]]; [[RAPID Generator]] inputs for additional routines, scope and error handling | `PROC`, `FUNC`, `TRAP`, arguments, `RETURN`, `Load` |
| B2 Data and expressions | [[RAPID Variable]] · [[Assign Variable Value]] · [[Get Array At Index]] · [[Comparison Expression]] · [[Comparison Operators]] · [[Inequality Symbols]] · the [[type:RAPID Expression]] type | declarations, `:=`, arrays, expressions and operators |
| B3 Flow control | [[IF Statement]] · [[While Loop]] · [[For Loop]] | `IF`/`ELSEIF`/`ELSE`, `WHILE`, `FOR` |
| B4 Target arithmetic | [[Current Robot Target]] · [[Offset Target]] · [[Get Robtarget Members]]; variables as [[Move]] targets | `CRobT`, `Offs`, robtarget components; what simulation can and cannot preview |
| B5 Operator interaction | [[Message Box]] · [[Numeric Entry Box]] | `TPReadFK` + `TEST`/`CASE`, `TPReadNum` |
| B6 Interrupts | [[Connect Interrupt]] · [[Signal Types]]; a [[Routine]] of type TRAP | `CONNECT … WITH`, `ISignalDI/DO/AI/AO/GI/GO` |
| B7 Raw code | [[Code]] · [[Code Types]] · [[Comment]] · [[Empty Line]] | hand-written RAPID lines, comments; the code-line sanitizer |
| B8 Tutorials | parametric routine library · operator-driven loop · sensor-triggered stop · looping over an array of targets · relative moves from the current position | — |
| B9 Pitfalls and limits | naming rules, scope conflicts, what the [[Path Generator]] ignores, upgrading old files | — |
