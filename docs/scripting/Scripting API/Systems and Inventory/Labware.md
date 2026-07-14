---
title: Labware
description: This document provides information about the properties of labware including ThicknessTopToWellBottom, StackedHeight, Name, FillVolume, Height, GripHeight, TotalVolume, and WellCount in the api.AllLabware class. Learn about the distance between the well b
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

`api.AllLabware`

# ThicknessTopToWellBottom Property

## Description

The ThicknessTopToWellBottom property is the distance between the bottom surface of the plate's wells and the top of the plate.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/thicknesstoptowellbottomproperty.jpg" size="30" width="163" height="69" position="flex-start" alt="ThicknessTopToWellBottom, illustrated" showCaption="false"}

## Syntax Example

In the following example, “Matrix\_384\_Lidded” is selected from the list of labware and stored in the variable called *labware*. If the “Matrix\_384\_Lidded” was present within the AllLabware list, the property value is displayed; otherwise, an error is displayed.

```csharp
public override void Execute(IScriptingApi api)
{
    string labwareName = "Matrix_384_Lidded";
    var labware = api.AllLabware.SingleOrDefault(x => x.Name == labwareName);
    if (labware != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.None, string.Format("ThicknessTopToWellBottom = {0}mm", labware.ThicknessTopToWellBottom));
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Labware with name {0} not found", labwareName));
    }
}
```

During the Cellario run, the ThicknessTopToWellBottom for the Matrix\_384\_lidded plate is displayed in the Cellario message window.

![](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/thicknesstoptowellbottomproperty2.jpg)

## Property Type

Decimal number, measured in mm

***

# StackedHeight Property

## Description

Stack height is the distance from the bottom of one plate to the bottom of the next when stacked on top of each other.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/stackedheightproperty.jpg" size="40" width="294" height="166" position="flex-start" showCaption="false"}

## Syntax Example

In the following example, “Matrix\_384\_Lidded” is selected from the list of labware and stored in the variable called *labware*. If the “Matrix\_384\_Lidded” was present within the AllLabware list, the property value is displayed; otherwise, an error is displayed.

```csharp
public override void Execute(IScriptingApi api)
{
    string labwareName = "Matrix_384_Lidded";
    var labware = api.AllLabware.SingleOrDefault(x => x.Name == labwareName);
    if (labware != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.None, string.Format("Stacked Height = {0}mm", labware.StackedHeight));
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Labware with name {0} not found", labwareName));
    }
}
```

During the Cellario run, the stacked height for the Matrix\_384\_lidded plate is displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/stackedheightproperty2.png" size="90" width="620" height="63" position="center" showCaption="false"}

## Property Type

Decimal number, measured in mm

***

# Name Property

## Description

The Name property is the name of the labware.

## Syntax Example

In this example, the names of all labware with fill volumes greater than 80 ml are displayed.

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Labware with Fill Volume greater than 80 ml");
    foreach(var labware in api.AllLabware.Where(x => x.FillVolume > 80))
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Minimal, labware.Name);
    }
}
```

During the Cellario run, all labware with fill volumes greater than 80 ml is displayed.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/labware-nameproperty.png" size="84" width="568" height="101" position="center" showCaption="false"}

## Property Type

String

***

# FillVolume Property

## Description

The FillVolume property is the volume of liquid within the well, measured in ml.

## Syntax Example

In this example, “Matrix\_384\_Lidded” is selected from the list of labware and stored in the variable called *labware*. If the “Matrix\_384\_Lidded” was present within the AllLabware list, the property value is displayed; otherwise, an error is displayed.

```csharp
public override void Execute(IScriptingApi api)
{
    string labwareName = "Matrix_384_Lidded";
    var labware = api.AllLabware.SingleOrDefault(x => x.Name == labwareName);
    if (labware != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.None, string.Format("Fill Volume = {0}ml", labware.FillVolume));
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Labware with name {0} not found", labwareName));
    }
}
```

During the Cellario run, the fill volume for the Matrix\_384\_lidded plate is displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/fillvolumeproperty.png" size="82" width="576" height="65" position="center" showCaption="false"}

## Property Type

Decimal number, measured in ml

***

# Height Property

## Description

The Height property is the height of the plate.

Plate height is measured from the base of the plate to the highest point. If a plate were set on a flat surface and a flat object set on top of it, the distance between the surface and object corresponds to the plate height.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/heightproperty.jpg" size="40" width="234" height="128" position="flex-start" alt="Height illustrated" showCaption="false"}

## Syntax Example

In this example, “Matrix\_384\_Lidded” is selected from the list of labware and stored in the variable called *labware*. If the “Matrix\_384\_Lidded” was present within the AllLabware list, the property value is displayed; otherwise, an error is displayed.

```csharp
public class MyScript : AbstractScript
{
    public override void Execute(IScriptingApi api)
    {
        string labwareName = "Matrix_384_Lidded";
        var labware = api.AllLabware.SingleOrDefault(x => x.Name == labwareName);
        if (labware != null)
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.None, string.Format("Plate Height = {0}mm", labware.Height));
        }
        else
        {
            api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Labware with name {0} not found", labwareName));
        }
    }
}
```

During the Cellario run,  the Plate Height for the Matrix_384_lidded plate is displayed in the Cellario message window.

#### Property Type

Property type decimal number, measured in mm

## GripHeight Property

#### Description

The GripHeight is the height at which the robot grips the microplate, measured in millimeters from the bottom of the plate.

#### Syntax Example

In this example “Matrix_384_Lidded” is be selected from the list of labware and stored in the variable called *labware*. 
If the “Matrix_384_Lidded” was present within the AllLabware list, the property value is displayed; otherwise, an error is displayed.

```csharp
public override void Execute(IScriptingApi api)
{
    string labwareName = "Matrix_384_Lidded";
    var labware = api.AllLabware.SingleOrDefault(x => x.Name == labwareName);
    if (labware != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.None, string.Format("Grip Height = {0}mm", labware.GripHeight));
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Labware with name {0} not found", labwareName));
    }
}
```

During the Cellario run, the Grip Height for the Matrix\_384\_lidded plate is displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/heightproperty2.png" size="86" width="585" height="64" position="center" showCaption="false"}

## Property Type

Decimal number, measured in mm

***

# GripHeight Property

## Description

The GripHeight is the height at which the robot grips the microplate, measured in millimeters from the bottom of the plate.

## Syntax Example

In this example “Matrix\_384\_Lidded” is be selected from the list of labware and stored in the variable called labware. If the “Matrix\_384\_Lidded” was present within the AllLabware list, the property value is displayed; otherwise, an error is displayed.

```csharp
public override void Execute(IScriptingApi api)
{
    string labwareName = "Matrix_384_Lidded";
    var labware = api.AllLabware.SingleOrDefault(x => x.Name == labwareName);
    if (labware != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.None, string.Format("Grip Height = {0}mm", labware.GripHeight));
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Labware with name {0} not found", labwareName));
    }
}
```

During the Cellario run, the GripHeight for the Matrix\_384\_lidded plate is displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/gripheightproperty.png" size="84" width="578" height="64" position="center" showCaption="false"}

## Property Type

Decimal number, measured in mm

***

# TotalVolume Property

## Description

The TotalVolume property is the maximum volume of liquid that can be stored within a well, measured in ml.

## Syntax Example

In this example “Matrix\_384\_Lidded” is be selected from the list of labware and stored in the variable called *labware*. If the “Matrix\_384\_Lidded” was present within the AllLabware list, the property value is displayed; otherwise, an error is displayed.

```csharp
public override void Execute(IScriptingApi api)
{
    string labwareName = "Matrix_384_Lidded";
    var labware = api.AllLabware.SingleOrDefault(x => x.Name == labwareName);
    if (labware != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.None, string.Format("Total Volume = {0}ml", labware.TotalVolume));
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Labware with name {0} not found", labwareName));
    }
}
```

During the Cellario run, the Total Volume for the Matrix\_384\_lidded plate is displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/totalvolumeproperty.png" size="84" width="572" height="64" position="center" showCaption="false"}

## Property Type

Decimal number, measured in ml

***

# WellCount Property

## Description

The WellCount property is the number of well within the plate, for example: 96, 384, 1536.

## Syntax Example

In this example “Matrix\_384\_Lidded” is selected from the list of labware and stored in the variable called *labware*. If the “Matrix\_384\_Lidded” was present within the AllLabware list, the property value is displayed; otherwise, an error is displayed.

```csharp
public override void Execute(IScriptingApi api)
{
    string labwareName = "Matrix_384_Lidded";
    var labware = api.AllLabware.SingleOrDefault(x => x.Name == labwareName);
    if (labware != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.None, string.Format("WellCount = {0} wells", labware.WellCount));
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Labware with name {0} not found", labwareName));
    }
}
```

During the Cellario run, the Well Count for the Matrix\_384\_lidded plate is displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/wellcountproperty.png" size="86" width="569" height="64" position="center" showCaption="false"}

## Property Type

Integer
