---
title: Get Object Manager
description: Access Robot Components' bookkeeping of named objects in this document (for scripting).
---

# ![](../../assets/icons/rc/get-object-manager.png){ .rc-icon--title } Get Object Manager

<span class="rc-badge">Utility tab</span>

Robot Components keeps track of every named object in a Grasshopper document, which is
how it can warn about duplicate names (see [[Check Variable Names]]). **Get Object Manager**
outputs that bookkeeping object. It is meant for scripting (C# or Python components) and
debugging; you don't need it to build programs.

[[io:Get Object Manager]]

## See also

[[Check Variable Names]] · [[Info]]
