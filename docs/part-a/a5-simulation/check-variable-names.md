---
title: Check Variable Names
description: Switch the document-wide check for duplicate RAPID names on or off.
---

# ![](../../assets/icons/rc/check-variable-names.png){ .rc-icon--title } Check Variable Names

<span class="rc-badge">Utility tab</span>

Every named declaration (targets, speed data, variables, …) becomes a RAPID name, and RAPID
names must be unique. Robot Components checks this across the whole Grasshopper document.
When two components use the same name, they warn *"The variable name "p" is already in
use."*. **Check Variable Names** switches that check on or off for the document. It is on by
default.

[[io:Check Variable Names]]

!!! tip "Leave it on"
    The check costs a little time in very large definitions, which is the only reason to switch
    it off. Duplicate names make the controller reject the module.

## See also

[[Get Object Manager]] · [B9 Pitfalls](../../part-b/b9-pitfalls.md)
