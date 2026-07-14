---
title: Protocol Thread
description: This document provides valuable insights into the Cellario protocol's essential properties: ProtocolName, Threads, and Protocol Ports Properties. Learn about the ProtocolName property identifying the current protocol, the Threads property encompassing all
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

`api.CurrentPlate.CurrentProtocol`

# ProtocolName Property

## Description

The ProtocolName property describes the name of the current Cellario protocol.

## Syntax Example

```csharp
using System; using System.Linq;
using HRB.Cellario.Scripting.API;
namespace Customer.Scripting
{
    public class MyScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Protocol Name = {0}", api.CurrentPlate.CurrentProtocol.ProtocolName));
        }
    }
}
```

During the Cellario order, the script writes out the name of the current protocol to the
Cellario message log. In this example, the protocol is called *CurrentProtocol*.

![](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/protocolnameproperty.png)

## Property Type

String

***

# Threads Property

## Description

The Threads property provides a list of all the threads within the current protocol.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var thread in api.CurrentPlate.CurrentProtocol.Threads)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Thread name = {0}", thread.ThreadName));
    }
}
```

During the Cellario run, this script displays a list of all the thread names in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/threadsproperty.png" size="86" width="596" height="103" position="center" showCaption="false"}

## Property Type

Array of IScriptingProtocolThread

***

# Protocol Ports Properties

## Description

Protocol Ports are properties of a plate thread object.

## Syntax Example

```csharp
var thread = api.CurrentRun.Protocol.Threads.First();
var threadName = thread.ThreadName;
var inputPortName = thread.InputPortName;
var inputPortTag = thread.InputPortTag;
var outputPortName = thread.OutputPortName;
var outputPortTag = thread.OutputPortTag;
```

This example reads the values for the port properties in the first protocol thread.

## Properties

All Protocol Port property types are *string*.

- InputPortName
- InputPortTag
- OutputPortName
- OutputPortTag

