"""MkDocs hook: icon + link shortcodes for Robot Components and Grasshopper.

Write in any page:

    [[Move]]                    component  -> component icon + link to its page
    [[type:Robot Target]]       data type  -> parameter icon + link to the page that defines it
    [[gh:Rectangle]]            native Grasshopper component -> icon + link to grasshopperdocs.com
    [[Move|two Moves]]          any of the above with a custom label
    [[Move\|two Moves]]         same, inside a Markdown table cell
    [[io:Move]]                 inputs/outputs tables from docs/data/io.json (extracted from
                                the live components, incl. right-click extra parameters)

Targets live in docs/data/registry.yml. An unknown name is a build warning, so
`mkdocs build --strict` fails on typos. Every page listed in the registry that does not
exist yet is generated as a "planned" stub, so links never break before a page is written.
Shortcodes inside inline code or fenced code blocks are left alone.
"""

import json
import logging
import posixpath
import re
from pathlib import Path

import yaml
from mkdocs.structure.files import File

log = logging.getLogger("mkdocs.hooks.rc_links")

# Label separator is "|", or "\|" inside Markdown tables.
SHORTCODE = re.compile(r"\[\[(?:(type|gh):)?([^\]|\\]+?)(?:\\?\|([^\]]+))?\]\]")
CODE = re.compile(r"(^[ \t]*```.*?^[ \t]*```|`[^`\n]+`)", re.DOTALL | re.MULTILINE)

IO = re.compile(r"\[\[io:([^\]]+)\]\]")

_registry: dict = {}
_io: dict = {}
_type_by_guid: dict = {}


def _chapter_title(page: str) -> str:
    folder = posixpath.basename(posixpath.dirname(page))  # e.g. "a2-targets"
    code, _, rest = folder.partition("-")
    return f"{code.upper()} {rest.replace('-', ' ').capitalize()}"


def on_config(config):
    path = Path(config["docs_dir"]) / "data" / "registry.yml"
    _registry.clear()
    _registry.update(yaml.safe_load(path.read_text(encoding="utf-8")))
    docs = Path(config["docs_dir"])
    _io.clear()
    _io.update(json.loads((docs / "data" / "io.json").read_text(encoding="utf-8")))
    _type_by_guid.clear()
    for entry in json.loads((docs / "assets" / "icons" / "manifest.json").read_text(encoding="utf-8")):
        if entry["subcategory"] == "Parameters" and entry["name"] in _registry["types"]:
            _type_by_guid[entry["guid"]] = entry["name"]
    return config


def _cell(text: str) -> str:
    return " ".join((text or "").split()).replace("|", "\\|")


def _type_cell(p: dict) -> str:
    name = _type_by_guid.get(p["typeGuid"])
    shown = f"[[type:{name}]]" if name else _cell(p["type"])
    return f"list of {shown}" if p["access"] == "list" else (f"tree of {shown}" if p["access"] == "tree" else shown)


def _io_table(rows: list[dict], first: str) -> str:
    lines = [f"| {first} | Nick | Type | Description |", "|---|---|---|---|"]
    for p in rows:
        name = f"{p['name']} *(optional)*" if p["optional"] and not p.get("menu") else p["name"]
        lines.append(f"| {_cell(name)} | `{_cell(p['nick'])}` | {_type_cell(p)} | {_cell(p['description'])} |")
    return "\n".join(lines)


def _render_io(match, page_src: str) -> str:
    name = match.group(1).strip()
    entry = _io.get(name)
    if entry is None:
        log.warning("Unknown shortcode [[io:%s]] in %s", name, page_src)
        return match.group(0)
    fixed_in = [p for p in entry["inputs"] if not p["menu"]]
    menu_in = [p for p in entry["inputs"] if p["menu"]]
    fixed_out = [p for p in entry["outputs"] if not p["menu"]]
    menu_out = [p for p in entry["outputs"] if p["menu"]]
    parts = []
    if fixed_in:
        parts += ["**Inputs**", "", _io_table(fixed_in, "Input"), ""]
    if menu_in:
        parts += ["**Extra inputs** (right-click the component to add them)", "", _io_table(menu_in, "Input"), ""]
    if fixed_out:
        parts += ["**Outputs**", "", _io_table(fixed_out, "Output"), ""]
    if menu_out:
        parts += ["**Extra outputs** (right-click the component to add them)", "", _io_table(menu_out, "Output"), ""]
    return "\n".join(parts)


def on_files(files, config):
    existing = {f.src_uri for f in files}
    pages: dict[str, list[tuple[str, str]]] = {}
    for section, prefix in (("components", ""), ("types", "type:")):
        for name, entry in _registry[section].items():
            pages.setdefault(entry["page"], []).append((prefix, name))
    for page, entries in sorted(pages.items()):
        if page in existing:
            continue
        if page.endswith("/index.md"):
            title = _chapter_title(page)
            heading = f"# {title}"
        else:
            components = [n for p, n in entries if p == ""] or [n for _, n in entries]
            title = " · ".join(dict.fromkeys(components))
            first = next((p + n for p, n in entries if p == ""), entries[0][0] + entries[0][1])
            heading = f"# {_icon_markdown(first, page, title_size=True)} {title}"
        items = "\n".join(f"    - [[{p}{n}]]" for p, n in entries)
        content = (
            f"---\ntitle: {title}\n---\n\n{heading}\n\n"
            f'!!! abstract "Planned page"\n    The tutorial for this page is not written yet. It will cover:\n\n{items}\n'
        )
        if not page.endswith("/index.md"):
            components = list(dict.fromkeys(n for p, n in entries if p == "" and n in _io))
            for n in components:
                if len(components) > 1:
                    content += f"\n## {n}\n"
                content += f"\n{_cell(_io[n]['description'])}\n\n[[io:{n}]]\n"
        files.append(File.generated(config, page, content=content))
    return files


def _lookup(kind: str | None, name: str):
    section = {"type": "types", "gh": "grasshopper", None: "components"}[kind]
    return _registry[section].get(name)


def _icon_markdown(code: str, page_src: str, title_size: bool = False) -> str:
    kind, _, name = code.partition(":") if ":" in code else (None, "", code)
    entry = _lookup(kind or None, name)
    rel = posixpath.relpath(entry["icon"], posixpath.dirname(page_src) or ".")
    css = ".rc-icon--title" if title_size else ".rc-icon"
    return f"![]({rel}){{ {css} }}"


def _replace(match, page_src: str) -> str:
    kind, name, label = match.group(1), match.group(2).strip(), match.group(3)
    entry = _lookup(kind, name)
    if entry is None:
        log.warning("Unknown shortcode [[%s%s]] in %s", f"{kind}:" if kind else "", name, page_src)
        return match.group(0)
    base = posixpath.dirname(page_src) or "."
    icon = posixpath.relpath(entry["icon"], base)
    target = entry["url"] if kind == "gh" else posixpath.relpath(entry["page"], base)
    return f"[![]({icon}){{ .rc-icon }} {label or name}]({target}){{ .rc-ref }}"


def on_page_markdown(markdown, page, config, files):
    src = page.file.src_uri
    parts = CODE.split(markdown)
    for i in range(0, len(parts), 2):  # even indices are outside code
        parts[i] = IO.sub(lambda m: _render_io(m, src), parts[i])
        parts[i] = SHORTCODE.sub(lambda m: _replace(m, src), parts[i])
    return "".join(parts)
