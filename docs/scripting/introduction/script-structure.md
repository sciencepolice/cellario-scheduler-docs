---
title: Script Structure
description: Learn how to write a C# script for Cellario with this comprehensive document. Gain insights on using statements, namespaces, class names, and three essential methods: AllocateResources, Execute, and ReleaseResources. Discover the purpose and functionality
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

The default example script defines using statements, namespace, class name and three methods.

The script requires the *Execute&#x20;*&#x6D;ethod, the AllocateResources, and ReleaseResources method are optional.

:::CodeblockTabs
```csharp
using System;
using System.Linq;
using HRB.Cellario.Scripting.API;
namespace Customer.Scripting
{
    public class MyScript : AbstractScript
    {
        public override void AllocateResources(IScriptingApiAllocation api)
        {
        }
        public override void Execute(IScriptingApi api)
        {
        }
        public override void ReleaseResources(IScriptingApiPostExecute api)
        {
        }
    }
}
```

```python
#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


def AllocateResources(api : PythonScriptingApi):
    pass

def Execute(api : PythonScriptingApi):
    pass

def ReleaseResources(api : PythonScriptingApi):
    pass
```
:::

# Using Statements

The using directive states that the script will be using the names in the given namespace.
To use methods from another class, such as maths methods add the line.

```csharp
using System;
```

# Namespace

The namespace keyword declares the scope that contains a set of related objects.
Namespaces help to organize code elements and create globally unique types.HighRes suggest that you define your own namespace to avoid collisions with other predefined types.

# Class Name

Each script should have a unique class name, and inherits from the class AbstractScript.

# Scripting Methods

Cellario scripts have the method *Execute*, and two optional methods, *AllocateResources* and *ReleaseResources*.
AllocateResources and ReleaseResources functions are required if the script needs to claim resources.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/scriptingmethods.jpg" size="76" width="611" height="160" position="center" alt="Scripting methods flow" showCaption="false"}

## AllocateResources

If a script is to use a resource, the script must claim the resource before a script can use it.
A resource is claimed using the *Allocate&#x20;*&#x6D;ethod. The *Allocate&#x20;*&#x6D;ethod ensures that the resource is available and in a ready state.
If required, a script can allocate multiple devices. When all the allocated devices are available, Cellario will call the *Execute&#x20;*&#x6D;ethod.

To claim a resource within the *AllocateResources&#x20;*&#x6D;ethod, add a line to select the resource from the resources array and then call *Allocate*.

```csharp
api.Resources["resource name"].Allocate();
```

## Execute

Once all the allocated resources have been claimed, the *Execute&#x20;*&#x6D;ethod is called.
The main code for the script should be within the *Execute&#x20;*&#x6D;ethod.

## ReleaseResources

After the *Execute&#x20;*&#x6D;ethod is complete, Cellario calls the *ReleaseResources* method.
*ReleaseResources* can collect the results of an operation and release resources.

The claim to the resource should be removed after the script execution.
To remove the claim, select the resource from the resources array and then call the *Release* method.

```csharp
api.Resources["resource name"].Release();
```
