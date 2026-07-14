---
title: Referencing Dependencies
description: Learn how to reference an external assembly in a Cellario script using the css_reference comment in this comprehensive document. Discover how Cellario seamlessly detects references from using statements for GAC and local assemblies, while cautioning about
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

A Cellario script can reference an external assembly by adding a css\_reference comment to the script. For example:

```csharp
//css_reference .\ScriptDependencies\ScriptCustomLibrary.dll
```

For assemblies defined in the GAC and local (to Cellario) assemblies, where namespaces are the same as the assembly names, Cellario automatically picks up references from using statements. For example:

```csharp
using System.Windows.Forms;
```

:::hint{type="warning"}
External assemblies are **not** visible to Intellisense in the script editor and are not exported/imported with the script.
:::

