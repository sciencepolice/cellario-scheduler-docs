---
title: Data
description: Learn about the properties `PerPlateData`, `PerRunData`, and `PerStepData` in the `api.data` namespace, which serve as a storage and retrieval mechanism for sharing data across scripts in a Cellario run. Discover how to effectively save and retrieve data 
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

`api.data`

# PerPlateData Property

## Description

The PerPlateData property saves and retrieves per-plate data among all scripts executing in the current Cellario run.

Data is identified by a name string, and must have an associated type converter that can convert data to and from a string.

## Syntax Example

See script examples [Run Data](../../samples/csharp-scripts/examples/Run_Data.cs) and [Run Data Next](../../samples/csharp-scripts/examples/Run_Data_Next.cs).

## Property Type

IDataDictionary

***

# PerRunData Property

## Description

The PerRunData property saves and retrieves data among all scripts executing in the current Cellario run.

Data is identified by a name string and must have an associated type converter that can convert data to and from a string.

## Syntax Example

See script examples [Run Data](../../samples/csharp-scripts/examples/Run_Data.cs) and [Run Data Next](../../samples/csharp-scripts/examples/Run_Data_Next.cs).

## Property Type

IDataDictionary

***

# PerStepData Property

## Description

The PerStepData property saves and retrieves data between invocations of this script step in the current Cellario run.

Data is identified by a name string and must have an associated type converter that can convert data to and from a string.

## Syntax Example

See script examples [Run Data](../../samples/csharp-scripts/examples/Run_Data.cs) and [Run Data Next](../../samples/csharp-scripts/examples/Run_Data_Next.cs).

## Property Type

IDataDictionary

***

# Cellario Scripting Data Event

## Description

There is additional functionality for the Data event that does not affect the Scripting API.
When data is written to the Script Data API in a script, an event is fired that conveys the data to Cellario Services notifications. This lets you add a notification listener to catch the data and act on it, such as forwarding to a LIMS or altering a later workflow step.

## Syntax Example

```csharp
string myKey = "42";
object myData = "The answer to life, the universe, and everything is 42.";
// During runtime (only), this data update generates an event in cell controller, which
// may be listened to by the Services hook and broadcast to notification listeners.
api.Data.PerRunData[myKey] = myData;
api.Data.PerPlateData[myKey] = myData;
api.Data.PerStepData[myKey] =  myData;
```

