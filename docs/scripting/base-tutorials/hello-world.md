---
title: Hello World
description: Learn how to create and execute a Cellario script with this comprehensive guide. From creating a new protocol and naming it "Hello World," to adding a storage resource to the plate, editing the script to include imports, namespaces, classes, and method de
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

This section instructs you on how to create and execute a Cellario script.

:::::WorkflowBlock
:::WorkflowBlockItem
Create a new protocol in Cellario and name it "Hello World."

For instructions on creating a protocol, see the *Cellario Operator Guide* (for v3.6 and earlier) or the *Cellario Online Help* (for v4.0 and later).&#x20;
:::

:::WorkflowBlockItem
After you assign a storage resource to the plate, drag and drop a New Script script onto the thread after the plate.

![Dragging a new script to a thread](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep5.png)
:::

:::WorkflowBlockItem
Click **Save** to save the protocol.
:::

:::WorkflowBlockItem
Double-click the *New Script* operation icon within the thread to open the code editor.

![New script opened in code editor](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep7.png)
:::

:::WorkflowBlockItem
Edit the Cellario script to contain the following code:

![Edited code](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep8.png)

**Each region of code is explained below**

**i.**  Imports the classes required by the script

**ii.**   Creates a new namespace called *Hello*

**iii.**  Creates a new class called *World*, which inherits from the base class *AbstractScript*

**iv.** Creates a new method called *Execute*, which overrides the *Execute* method defined in the base class *AbstractScript*
The main code for the script should be defined in the *Execute* method. An argument of type IScriptingApi is passed to the Execute function. The argument gives access to all of the Cellario methods and properties.

**v.**  In standard C# the methods *Console.WriteLine&#x20;*&#x61;nd *Debug.WriteLine* can be used to print log information to the output console. The *WriteDiagnostic* method is the Cellario equivalent of these methods. *WriteDiagnostic* writes messages to the Cellario Message Log. In this example, *WriteDiagnostic* writes the words "Hello World" to the message log.

**vi.**  CLosing braces for the *Execute* method, *Hello* class, and *World* namespace.
:::

:::WorkflowBlockItem
Click **Compile** in the C# Script window to check the syntax for the script.&#x20;

A message window confirms that the script compiled successfully. If there are syntax errors, the message displays details about the type of error and its location.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep10.jpg" size="70" width="520" height="248" position="flex-start" alt="Compile Script confirmation message window" showCaption="false"}
:::

:::WorkflowBlockItem
Click **OK**.
:::

:::WorkflowBlockItem
Click **Save** in the *C# Script* window to save the script, and then close the window.
:::

:::WorkflowBlockItem
Click **Save** in the Protocol Designer to save the protocol.
:::

:::WorkflowBlockItem
Click the *Order* tab, and then click **Add** to add a new order.

![Order Add button](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep13.png)
:::

:::WorkflowBlockItem
Select the *Hello World* protocol you created, and then click **Order**.

![Selecting the protocol](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep14.png)
:::

:::WorkflowBlockItem
Add plates to the order by right-clicking in the storage area and selecting **Add Samples**.

![Adding samples to a storage location](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep15.png)
:::

::::WorkflowBlockItem
Click **Save** to save the order.

:::hint{type="info"}
In this example, we do not want to execute any real operation on the resources or have the robot physically move the plate.
:::
::::

:::WorkflowBlockItem
Click the *Run* tab, and then right-click in the resource tree and select **Simulate/Un-Simulate All** to simulate all the devices.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep17.png" size="40" width="371" height="380" position="flex-start" alt="Selecting Simulate/Un-Simlate Al for a resource" showCaption="false"}
:::

:::WorkflowBlockItem
Click the Debug icon below the log pane to turn on the Debug message filter.

![Debug icon of Cellario Message pane](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep18.png)
:::

:::WorkflowBlockItem
Click the Start icon to initialize the system.
:::

:::WorkflowBlockItem
Click **Start Run** to start the Cellario run.

During the run, Cellario writes "Hello World" to the message log.

!["Hello World" written to Message log](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/helloworldstep21.png)
:::
:::::
