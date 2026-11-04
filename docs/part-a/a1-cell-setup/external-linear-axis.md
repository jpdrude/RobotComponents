---
title: External Linear Axis
description: A track that moves the robot (or a part) in a straight line.
---

# ![](../../assets/icons/rc/external-linear-axis.png){ .rc-icon--title } External Linear Axis

<span class="rc-badge">Definitions tab</span>

An **external linear axis** is a track: the robot rides on a carriage along a straight
line, extending its reach. Define it with its direction (*Axis*), travel (*Axis Limits*, in
mm) and meshes, and wire it into [[Robot Preset]] (*External Axis*). Targets then need to say
where the track should be; see [[External Joint Position]].

[[io:External Linear Axis]]

Right-click → *Axis Logic Number* sets which external axis (`eax_a` … `eax_f`) this is on
the controller; *Moves Robot* says whether the axis carries the robot (a track) or the part.

## See also

[[External Rotational Axis]] · [[External Joint Position]] · [[Robot Preset]] · [[Deconstruct External Linear Axis]]
