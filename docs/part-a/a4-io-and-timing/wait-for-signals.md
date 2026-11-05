---
title: Wait for signals
description: Pause the program until an input or output signal has a certain value.
---

# ![](../../assets/icons/rc/wait-for-digital-input.png){ .rc-icon--title } Wait for signals

<span class="rc-badge">Code Generation tab</span>
<span class="rc-badge rc-badge--verified">RAPID syntax checked on IRB 1200 · RobotWare 6.10</span>

These components pause the program until a signal has the value you want: a sensor reports
a part, a machine reports it's done, a door is closed. The robot stands still while it waits.

| Component | Waits for | RAPID |
|---|---|---|
| [[Wait for Digital Input]] | an on/off input | `WaitDI diPartReady, 1;` |
| [[Wait for Analog Input]] | an analog input above/below a value | `WaitAI aiPressure, \GT, 2.5;` |
| [[Wait for Group Input]] ★ | a group input to equal a number | `WaitGI giProgram, 3;` |
| [[Wait for Digital Output]] ★ | an output (e.g. set by another task) | `WaitDO doClamp, 1;` |
| [[Wait for Analog Output]] ★ / [[Wait for Group Output]] ★ | the same for analog / group outputs | `WaitAO …` / `WaitGO …` |

★ <span class="rc-badge rc-badge--fork">Fork addition</span>

Example: the gripper sequence on [Set outputs](set-outputs.md) starts with
`WaitDI diPartReady, 1;`.

## Inputs and outputs

### Wait for Digital Input
[[io:Wait for Digital Input]]

### Wait for Analog Input
[[io:Wait for Analog Input]]

### Wait for Group Input
[[io:Wait for Group Input]]

### Wait for Digital Output
[[io:Wait for Digital Output]]

### Wait for Analog Output
[[io:Wait for Analog Output]]

### Wait for Group Output
[[io:Wait for Group Output]]

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd]
([[abb:WaitDI]], [[abb:WaitAI]], [[abb:WaitGI]], [[abb:WaitDO]], [[abb:WaitAO]], [[abb:WaitGO]]).

!!! warning "Waiting forever"
    Without a time limit, the program waits as long as it takes. If the signal never comes,
    the robot just stands there. To react instead, use an interrupt ([[Connect Interrupt]]) or
    check the signal in an [[IF Statement]].

## See also

[[Set Digital Output]] · [[Wait Time]] · [[Wait for Robot]] · [[Connect Interrupt]]
