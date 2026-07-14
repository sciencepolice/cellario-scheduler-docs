---
title: Library Management
description: Learn how the Script Library Manager in Cellario empowers users to effortlessly organize and manage a collection of reusable scripts. Easily create, edit, copy, and delete scripts within customizable groups. Compile scripts for syntax errors, and seamless
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

The library of reusable Cellario scripts can be managed from the Script Library Manager.
The Script Library Manager can be accessed via **Cellario > Maintenance > Script Library**.

![Script Library in Cellario](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/scriptlibrarymanager.png)

# Library Manager Methods

| **Add Group**  | Master scripts can be organized into groups.<br />To create a new group click **Add Group**.                                                                                                |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Add Script** | To create a new script, select a group from the tree on the left and then click **Add Script**.                                                                                             |
| **Save**       | After editing a script click **Save&#x20;**&#x74;o save the changes.<br />The *Example* scripts cannot be edited. To edit an example script, copy the script first, and then edit the copy. |
| **Copy**       | To copy a script, select the script from the list on the left and then click **Copy**.                                                                                                      |
| **Delete**     | To delete a script, select the script from the list on the left and then click **Delete**.                                                                                                  |
| **Compile**    | To check for syntax error within the script, click **Compile**.<br />If there are problems within the code, a message window shows the error information.                                   |
| **Export**     | If you want to reuse a Cellario script on a different system, select the script from the tree on the left, and then click **Export** to export the script to a file.                        |
| **Import**     | Click **Import** to import and create a new script from a file.                                                                                                                             |

***

# Script Editing

## Intellisense

Intellisense is a context-aware code completion feature. When editing code, Intellisense will display List Members, Parameter Info, Quick Info, and Complete Word.

After typing `api.`, Intellisense displays all of the methods and properties associated with IScriptingApi.

![Intellisense displaying methods](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/intellisense1.jpg)

Intellisense is intelligently linked to the Cellario database. After typing `api.Resources[`
, intellisense displays a list of all the resource names on the current system.

![Intellisense displaying resources](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/intellisense2.jpg)

After typing `api.Resources[“resource name”].Operations[` , intellisense displays a list of all the operations associated with the resource.
In this example, the operations for resource *Bravo 1* are shown.

![Intellisense displaying operation resources](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/intellisense3.jpg)

After typing `api.Resources[“resource name”].Operations[“operation name”]` , Intellisense  displays a list of all the operation parameters associated with the operation.

![Intellisense displaying operation parameters](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/intellisense4.jpg)
