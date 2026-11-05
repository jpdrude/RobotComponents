---
title: Deconstruct components
description: Take Robot Components objects apart into their values.
---

# ![](../../assets/icons/rc/deconstruct-robot-target.png){ .rc-icon--title } Deconstruct components

<span class="rc-badge">Deconstruct tab</span>

Every main Robot Components object has a **Deconstruct** component that takes it apart
again: the name, plane and configuration of a [[type:Robot Target]], the four speeds of a
[[type:Speed Data]], the meshes and kinematics of a [[type:Robot]]. Use them to inspect what
a component made, or to reuse parts of an object elsewhere in your definition.

| Code Generation | Definitions |
|---|---|
| [[Deconstruct Robot Target]] · [[Deconstruct Joint Target]] · [[Deconstruct Robot Joint Position]] · [[Deconstruct External Joint Position]] · [[Deconstruct Configuration Data]] · [[Deconstruct Speed Data]] · [[Deconstruct Zone Data]] · [[Deconstruct Move]] · [[Deconstruct Path Acceleration Limitation]] · [[Deconstruct Group Signal]] ★ | [[Deconstruct Robot]] · [[Deconstruct Robot Kinematic Parameters]] · [[Deconstruct Robot Tool]] · [[Deconstruct Work Object]] · [[Deconstruct Load Data]] · [[Deconstruct External Linear Axis]] · [[Deconstruct External Rotational Axis]] |

★ <span class="rc-badge rc-badge--fork">Fork addition</span>

## Inputs and outputs

### Deconstruct Robot Target
[[io:Deconstruct Robot Target]]

### Deconstruct Move
[[io:Deconstruct Move]]

### Deconstruct Robot
[[io:Deconstruct Robot]]

### Deconstruct Robot Tool
[[io:Deconstruct Robot Tool]]

### Deconstruct Work Object
[[io:Deconstruct Work Object]]

### Deconstruct Speed Data
[[io:Deconstruct Speed Data]]

### Deconstruct Zone Data
[[io:Deconstruct Zone Data]]

### Deconstruct Configuration Data
[[io:Deconstruct Configuration Data]]

### Deconstruct Joint Target
[[io:Deconstruct Joint Target]]

### Deconstruct Robot Joint Position
[[io:Deconstruct Robot Joint Position]]

### Deconstruct External Joint Position
[[io:Deconstruct External Joint Position]]

### Deconstruct Group Signal
[[io:Deconstruct Group Signal]]

### Deconstruct Path Acceleration Limitation
[[io:Deconstruct Path Acceleration Limitation]]

### Deconstruct Robot Kinematic Parameters
[[io:Deconstruct Robot Kinematic Parameters]]

### Deconstruct Load Data
[[io:Deconstruct Load Data]]

### Deconstruct External Linear Axis
[[io:Deconstruct External Linear Axis]]

### Deconstruct External Rotational Axis
[[io:Deconstruct External Rotational Axis]]

!!! info "Targets with a Reference Plane"
    A [[Robot Target]] with a *Reference Plane* stores its plane **relative** to that plane, so
    Deconstruct Robot Target gives the relative plane (e.g. `[0, 0, 0]` on the [[Work Object]]
    table), not the world position.
