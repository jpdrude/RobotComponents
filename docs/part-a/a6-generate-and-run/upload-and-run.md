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

Set *Upload* to `True` to send the *Module* text to the controller. *Status* reports what
happened. Use a Grasshopper [[gh:Button]] (it is `True` only while you click it), not a
[[gh:Boolean Toggle]] that stays `True`, or every change in the definition uploads again.

## Run Program

[[io:Run Program]]

*Run* starts, *Stop* pauses, *Reset* puts the program pointer of all tasks back to the start
(*PP to Main*).

## Upload Helper Modules ★

Uploads **additional** modules (e.g. a library of your own routines, one module per branch of
*Modules*) to the controller's storage, next to the main program. With *Load To Task* they are
also loaded into the task right away; otherwise the program loads them itself with
[[Load Module]]. Example: [A routine library](../../part-b/b8-tutorials/routine-library.md).

[[io:Upload Helper Modules]]

★ <span class="rc-badge rc-badge--fork">Fork addition</span>

## Get Task Names

Lists the controller's tasks (`T_ROB1`, and `T_ROB2`, … in a [Multi Move](../a7-multi-move/index.md)
cell), e.g. to check the names a [[Task List]] needs.

[[io:Get Task Names]]

!!! danger "The robot really moves"
    Before *Run* on real hardware: test the program in RobotStudio first, reduce the speed,
    clear the cell, and keep the emergency stop within reach. The robot needs to be in automatic
    mode with motors on for remote start.

## See also

[[Get Controller]] · [[RAPID Generator]] · [[Get Log]]
