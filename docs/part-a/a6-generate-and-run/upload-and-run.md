---
title: Upload and run
description: Send the generated program to a real or virtual ABB controller, and start and stop it from Grasshopper.
---

# ![](../../assets/icons/rc/upload-program.png){ .rc-icon--title } Upload and run

<span class="rc-badge">Controller Utility tab</span>

Once the [[RAPID Generator]] has made a module, these components put it on a controller and
run it, without RobotStudio or a USB stick. They need the ABB PC SDK runtime (see
[Getting started → Install](../../getting-started/index.md#install)) and work on Windows only.

The chain: [[Get Controller]] → [[Upload Program]] → [[Run Program]].

## Upload Program

[[io:Upload Program]]

Set *Upload* to `True` (a [[gh:Panel]] or a Boolean Toggle) to send the *Module* text to the
controller. *Status* reports what happened. Use a button, not a toggle that stays `True`, so you
don't upload on every change.

## Run Program

[[io:Run Program]]

*Run* starts, *Stop* pauses, *Reset* puts the program pointer of all tasks back to the start
(*PP to Main*).

## Upload Helper Modules ★ and Get Task Names

[[io:Upload Helper Modules]]

[[io:Get Task Names]]

★ <span class="rc-badge rc-badge--fork">Fork addition</span>

!!! danger "The robot really moves"
    Before *Run* on real hardware: test the program in RobotStudio first, reduce the speed,
    clear the cell, and keep the emergency stop within reach. The robot needs to be in automatic
    mode with motors on for remote start.

## See also

[[Get Controller]] · [[RAPID Generator]] · [[Get Log]]
