---
title: Timed Path Generator
description: Like the Path Generator, but every step along the path lasts the same time, for animations and time-based checks.
---

# ![](../../assets/icons/rc/timed-path-generator.png){ .rc-icon--title } Timed Path Generator

<span class="rc-badge">Simulation tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>

**Timed Path Generator** does what the [[Path Generator]] does, with one difference: it
splits every movement into steps of **equal time** (*Time Interval*, in seconds), using the
path length and the programmed TCP speed. Each step of the animation slider then
represents the same amount of time: a slow `MoveL` gets many steps, a fast `MoveJ` few. That
makes animations play at a realistic pace, and lets you ask "where is the robot after 3
seconds?".

[[io:Timed Path Generator]]

The right-click outputs are the same as on the [[Path Generator]].

!!! info "An estimate"
    The step times come from path length ÷ programmed speed. Acceleration, corner zones and the
    controller's motion planning aren't modelled, so the real robot takes somewhat longer. In the
    [[Move]] example the [[Path Generator]] estimated 5.8 s; the Virtual Controller measured 6.2 s.

## See also

[[Path Generator]] · [[Speed Data]] · [[Check Actions]]
