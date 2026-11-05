# Robot Components Learning Wiki

Robot Components is a Grasshopper plugin for programming **ABB industrial robots** from
Rhino: you model targets and tool paths as geometry, simulate the robot, and get a RAPID
program you can upload to a real or virtual controller.

This wiki teaches it in two parts:

<div class="grid cards" markdown>

-   **[Part A: Essentials](part-a/index.md)**

    Everything you need to build, simulate and run robot programs. No RAPID knowledge
    needed; the generated code is shown, but you never have to touch it.

-   **[Part B: Advanced RAPID](part-b/index.md)**

    Variables, routines, loops, conditions, operator dialogs and interrupts. These
    components generate real RAPID logic, so each page also explains the RAPID behind it.

</div>

New here? Start with **[Getting started](getting-started/index.md)**.

## How the pages work

Every component page has the same parts: what it does, its inputs and outputs, a
downloadable example, the RAPID it generates, **Try it** tasks with solutions, and
gotchas. Examples marked
<span class="rc-badge rc-badge--verified">RAPID checked</span> were loaded and run on an
ABB Virtual Controller.
Components that exist only in this fork
([jpdrude/RobotComponents](https://github.com/jpdrude/RobotComponents)) carry a
<span class="rc-badge rc-badge--fork">Fork addition</span> badge.

!!! note "About ABB documentation"
    ABB's manuals are copyrighted and may not be copied. This wiki explains everything in
    its own words and links to the official manuals; see [ABB manuals](reference/abb-manuals.md).
