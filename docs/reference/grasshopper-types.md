---
title: Grasshopper data types
description: The basic Grasshopper data types that Robot Components' inputs and outputs use, with their icons.
---

# Grasshopper data types

Besides its own types ([[type:Robot Target]], [[type:Movement]], …), Robot Components uses
Grasshopper's built-in data types. Every input/output table in this wiki shows them with
the same icon as Grasshopper's *Params* tab. Hover over an input in Grasshopper to see its
type.

## ![](../assets/icons/gh/param-text.png){ .rc-icon } Text { #text }

A piece of text (a *string*). Used for **names**: target names, signal names, routine
names. In Part B also for RAPID expressions typed into a [[gh:Panel]] (`count + 1`). Names
become RAPID identifiers: letters, digits and `_`, starting with a letter.

## ![](../assets/icons/gh/param-number.png){ .rc-icon } Number { #number }

A decimal number. Used for joint angles (degrees), coordinates and offsets (mm), times
(seconds) and speeds. A [[gh:Number Slider]] or a [[gh:Panel]] provides it.

## ![](../assets/icons/gh/param-integer.png){ .rc-icon } Integer { #integer }

A whole number. Used for **choices** coded as numbers: the movement type (`0` MoveAbsJ, `1`
MoveL, `2` MoveJ, `3` MoveC), scopes, keywords, signal types. Use the drop-down lists
(e.g. [[Movement Types]]) instead of typing numbers.

## ![](../assets/icons/gh/param-boolean.png){ .rc-icon } Boolean { #boolean }

`True` or `False`. Used for switches: *Update*, *Upload*, *Run*, *Is Active*, *In Position*.
A *Boolean Toggle* or *Button* (Params tab) provides it; a button gives `True` only while
pressed.

## ![](../assets/icons/gh/param-plane.png){ .rc-icon } Plane { #plane }

A position with an orientation (origin + X, Y, Z axes). The main geometric input: robot
positions, target positions ([[Robot Target]]), tool and work object frames. Robot
Components reads planes in **millimetres**.

## ![](../assets/icons/gh/param-point.png){ .rc-icon } Point { #point }

A position without orientation, e.g. a centre of gravity in [[Load Data]] or the result of
[[Robot Tool Calibration]].

## ![](../assets/icons/gh/param-vector.png){ .rc-icon } Vector { #vector }

A direction, e.g. the travel direction of an [[External Linear Axis]] or the moments of
inertia in [[Load Data]].

## ![](../assets/icons/gh/param-curve.png){ .rc-icon } Curve { #curve }

A line or curve: the tool path drawn by the [[Path Generator]].

## ![](../assets/icons/gh/param-mesh.png){ .rc-icon } Mesh { #mesh }

Geometry made of triangles and quads: robot links, tools and external axes for display
and simulation.

## ![](../assets/icons/gh/param-domain.png){ .rc-icon } Domain { #domain }

A range from one number to another (`0 To 9900`): axis limits, the input range of a
[[Numeric Entry Box]]. *Construct Domain* (Maths tab) builds one; a [[gh:Panel]] with
`0 To 300` works too.

## ![](../assets/icons/gh/param-generic-data.png){ .rc-icon } Generic Data { #generic-data }

Anything. Inputs of this type accept several kinds of data and decide themselves what to do
with them, e.g. *Value* of [[RAPID Variable]] (a number, text, a target, …) or *Signal Name*
of [[Connect Interrupt]].

## Lists and trees

*list of …* in a table means the input takes **many** items at once (e.g. *Actions*).
Otherwise it takes one item, and Grasshopper repeats the component for each item you give it.
That's how one [[Move]] makes four movements from four targets. *tree of …* keeps
Grasshopper's branch structure; see the [flatten warning](../part-b/b8-tutorials/row-loop.md#step-by-step).
