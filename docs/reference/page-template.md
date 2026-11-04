# Page template

Every component page uses these sections, in this order. [Move](../part-a/a3-motion/move.md)
is the reference example.

1. **Title, icon and badges**: the component's toolbar icon (`{ .rc-icon--title }`) before the name; toolbar location; `Fork addition` if the component is not upstream; `RAPID checked` once the example ran on a Virtual Controller.
2. **What it does**: two or three plain sentences. No RAPID terms in Part A.
3. **Inputs and outputs**: name, nickname, type, default, meaning, plus right-click options.
4. **Example**: download button for the `.gh` file, a **Components used** strip with icons, canvas screenshot (Grasshopper set to *Draw Full Names*), numbered walkthrough with an icon before every component name, viewport screenshot.
5. **Generated RAPID**: the exact module the example produces. Part A: collapsed ("for the curious"). Part B: always open and annotated line by line. Note what the Virtual Controller reported.
6. **Try it**: 1–3 small tasks; at least one solution, collapsed.
7. **In RAPID** (Part A) / **RAPID background** (Part B): what happens on the controller, in our own words, with links to the ABB manuals.
8. **Gotchas**: real pitfalls found while building the example, including known bugs and workarounds.
9. **See also**

## Referencing components and types

**Every** mention of a Robot Components component or data type is written as a shortcode,
which renders as icon + name + link:

| Write | For | Renders as |
|---|---|---|
| `[[Move]]` | a component | component icon, links to its page |
| `[[type:Robot Target]]` | a data type (what flows through a wire) | parameter icon, links to the page that creates it |
| `[[gh:Rectangle]]` | a native Grasshopper component | Grasshopper icon, links to grasshopperdocs.com |
| `[[Move|two Moves]]` | any of these with a custom label | (inside a table cell, write `\|` instead of `|`) |

Names and link targets live in `docs/data/registry.yml`. An unknown name fails the strict
build. Pages that aren't written yet are generated as "planned" stubs, so links always work.
The hook is `tools/docs/rc_links.py`. The page's own title icon is the only hand-written image.

## How examples are produced

1. The definition is built in Grasshopper (scripted through the Rhino MCP server, or by hand).
2. The canvas image is rendered with `GH_Canvas.GenerateHiResImageTile` at 1.5× zoom; the viewport image with `RhinoView.CaptureToBitmap` (includes the Grasshopper preview).
3. The RAPID Generator output is copied verbatim and run with the `rapid-verifier` Virtual Controller (syntax check, then full simulation).
4. The `.gh` file goes to `docs/examples/<part>/`, images to `docs/assets/images/<part>/`.
5. Icons: `docs/assets/icons/rc/<name>.png` (Robot Components) and `docs/assets/icons/gh/<name>.png` (Grasshopper). `<name>` is the component name in lowercase with hyphens; `param-` prefixes parameters, and the toolbar tab prefixes name clashes (e.g. `controller-utility-set-digital-output`). `docs/assets/icons/manifest.json` lists them all.
