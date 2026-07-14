---
title: Adding Scripts to the Script Library
description: Learn how to add and manage scripts in Cellario's Script Library with this comprehensive guide. Discover how to create new groups, add and edit scripts, compile for error checks, and ensure scripts can be used multiple times without altering the original 
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

The library of Cellario scripts can form the basis for a protocol script. Using a script within a protocol or auto-recovery creates a copy of the script. Any edits to the protocol script do not affect the library script.

If a script will be used multiple times, you should add it to the Script Library. The following procedure instructs how to add a script to the Script Library.

::::WorkflowBlock
:::WorkflowBlockItem
Open the Script Library. (**Settings > Maintenance > Script Library**).
:::

:::WorkflowBlockItem
Click **Add Group** to add a new group.

![Adding a group to the Script Library](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/addingscriptssteps2-3.png)
:::

:::WorkflowBlockItem
Enter a name for the new group in the **Group Name** box, and then click **OK**.&#x20;

In this example, the group is called *Test*.
:::

:::WorkflowBlockItem
Select the new group from the list in the left pane, and then click **Add Script** to add a new script.

![Adding a new script to the library](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/addingscriptssteps4-5.png)
:::

:::WorkflowBlockItem
Enter a name for the script, and then click **OK**. In this example, the script is called *Test1*.

A new script is created with some template code.

![Template code of new script](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/addingscriptsstep52.png)
:::

:::WorkflowBlockItem
Edit the code as needed. In this example, the code is added to the *Execute* method.

The code will display the message "Test1."

![Code line to write "Test1"](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/addingscriptsstep6.png)
:::

:::WorkflowBlockItem
Click **Compile** to check the syntax and compile the script.

- If there are no problems in the script, a message window confirms that it compiled successfully.
- If there are problems with the script, a warning message window appears and provides information about the error. For example, if the semicolon was missing from the end of the `WriteDiagnostic` line, the message window provides information about the error.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/addingscriptsstep7.png" size="72" width="418" height="199" position="center" alt="Failed compilation message window" showCaption="false"}
:::

:::WorkflowBlockItem
Click **Save** to save the script, and then close the *Script Library* window.
:::
::::

