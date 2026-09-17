# What's New

Release notes for Cellario Scheduler, newest release first. This is a living page —
each release is appended at the top, so the full history stays on one page.

Entries are grouped as **New** (capabilities that did not exist before), **Improved**
(existing behavior made better), and **Fixed** (defects resolved).

## 4.5: May 2026

**New**

- The user interface has been updated in key areas with new HighRes branding.
- A new feature, Global Event Scripts, lets you specify scripts to execute on order or operation state changes independent of protocol. There are also new endpoints to create, edit, and delete global event scripts.
- The **View Completed Orders** window now features a **Copy Order** button.
- You can now differentiate End steps of protocol threads by name to have the names appear when creating an order and in the Active Plates Editor. (This helps identify the multiple End steps in Decision operations.)
- A graphic has been added to the user interface that displays the real-time state of the system status (tower) lights.
- A new **Barcode Scanning** setting has been added in **System Settings**: **Scanner Not Available**.

**Improved**

- You can now use a parameter for the value of the Decision Value parameter.
- There are improvements to the Active Plates Editor: To use the editor, you now only need to pause the order instead of the entire system, and you can now reassign resources to a different resource type for an operation.
- There are improvements to the Cellario Scheduler API (see the HighRes Developers' Site for details):
  - You can now update a protocol (using POST and PUT), create a protocol, and delete a protocol (with the appropriate permissions for each).
  - You can now log API requests and response body in `cellario.exe.nlog`.
  - The GET/resources endpoints can now return driver error messages.
  - There are new endpoints for service and database health: /ready, /live, and /health.
  - There are new endpoints for simulating an order.
  - The API now supports HTTPS.
- UTC timestamp format is now used when storing to the database and when returning API calls. (This improvement also fixed issues related to daylight saving time changes.)
- When searching in the Protocol List pane, **Search** box no longer clears and the search results are preserved when you click away from the search.
- The Create Parameter Lookup and Remove Parameter Lookup permissions are now combined under a single permission Manage Parameter Lookups.
- You can now control load/unload sequences using order parameters.
- Operations with load/unload sequences assigned now have an indicator on the step icon.
- When creating an order, you can now select the stacker/location for Buffer operations.
- When creating an order, you can now select the stacker/location for Hold operations.

**Fixed**

- An error no longer occurs in **Storage Configuration** when you right-click a stacker label and select **Move**.
- The change to UTC timestamp and an update to the Audit Trail Viewer corrected issues where the viewer was displaying the incorrect time or time zone on transactions and incubations and was generating an error when creating a report using the UTC+1 time zone.
- The Run Analyzer now displays the correct number of critical timing bars when a critical segment is added to operations within a loop.

## 4.4.5: January 2026

**Improved**

- A **Simulate** button was added to screens in the Operator UI so that users can choose whether they want to simulate an order created through the Operator UI. Previously, orders were automatically simulated upon creation/start in the Operator UI, which caused unexpected dialog windows to appear.
- Scripts can now update sample locations by scanning plate barcodes through an API call.
- Dock information was added to the /resources and /resources/{resourceName} API endpoints to return docks' ID numbers, locations, and switch addresses.
- System status communications to the tower lights are now more efficient.

**Fixed**

- Copied orders created from templates with defined End-step locations no longer have plate segments reversed and plates in random stacker locations.
- Database errors no longer occur when starting the system after deleting an order while its resources were still initializing.
- In some environments, a failed CloseDoor operation would generate an error and record it in the driver log but not pass the error to CellarioScheduler.
- CellarioScheduler could enter the `OFF` state after completing an order despite another order being scheduled to run.
- When using the API to create transfer-list orders, CellarioScheduler would create virtual samples for source plates when the TransferPriority order property was set to `Source`.
- CellarioScheduler now correctly accounts for excluded liquid handler nest resources when a script modifies the exclusion parameter.
- Simulations in the Run Analyzer no longer restart several times when a deadlock occurs.

## 4.4.4: October 2025

**Improved**

- The algorithm for creating transfer-list orders via the API has been improved to acknowledge protocol structure and well-level dependencies. The algorithm can handle complex dose-response workflows with intermediate plates to have transfer sequences now execute based on Merge steps.
- On Transfer steps with tray pooling, where both trays' samples start on deck, the second tray no longer needs to wait for the first tray to move before its liquid transfer can begin.

**Fixed**

- Users whose roles do not have the permission "Update Sample Name" are now restricted from updating sample names.
- Users whose roles do not have the permission "Edit Storage Permissions" are now restricted from adding or removing labware from storage positions.
- The vertical scroll bar of the **Script Library** window no longer disappears when the window is enlarged.
- In certain specific workflows, the dynamic tray of the Prime would move while the robotic arm was still picking the plate from it. This issue no longer occurs.
- The display color in the API response for labware definitions now translates correctly to a HEX value for labware syncing with CellarioOS.
- Syncing to CellarioScheduler before and after changing labware parameters in CellarioOS no longer causes a "downstream call" error in CellarioOS.
- The API PUT Order Action endpoint (PUT /orders/{orderID}) no longer requires an ActionSet object for single actions. This returns the behavior of CellarioScheduler v4.2.
- In certain workflow scenarios, a counterbalance swap would not happen if a single plate from one thread is spun after two plates from another thread are spun together. This issue no longer occurs.

## 4.4.3: September 2025

**Improved**

- An improved, clearer error message displays when trying to delete a user that has logged events in the audit trail.
- An improved, clearer error message displays if a user tries to log in using Active Directory but does not yet have a CellarioScheduler user account.
- The User Accounts list in the **User Options** window is now in alphabetical order by user name.
- When editing a user account, the **Username** value is now read-only.
- The user permission related to changing samples and operations is now named **Apply Changes**. This was previously named "Add Operations."

  Four Active Plate Editor commands have been renamed for consistency:
  - "Delete Operation" is now **Remove Operation**.
  - "Delete Plate" is now **Delete Sample**.
  - "Cancel Remaining Operations" is now **Cancel Sample**.
  - "Remove All" is now **Remove From All Plates**.

**Fixed**

- The GET /events endpoint in the Cellario API no longer returns a null ProtocolName for protocol-related events.
- Inventory and Data events are no longer skipped following a change to the CellarioScheduler system configuration.
- The Critical Timing bar now appears in the Gantt chart of a completed run's analysis.
- Critical segment warnings are no longer skipped when the segment ends at the beginning of an operation.
- Audit trail events for creating or editing protocol groups now show the complete path of the group.
- Copying and pasting a protocol as a new revision now works correctly. Previously, the protocol would not paste.
- When a user copies or duplicates a protocol created by another user, or saves it as a new version, the **By** field in the protocol details now correctly shows the user who performed these actions.

## 4.4.2: July 2025

This release contained changes to the Audit Trail features. (These apply when specific Audit Trail system settings are enabled.)

**Improved**

- Annotations are now required when saving a group name and when moving a protocol to a new group.
- Audit records are now created when, in a resource's storage window, you add labware, change a sample name, or change a labware type.

**Fixed**

- Saving a protocol as a new version no longer generates a duplicate annotation request.
- The **Show Storage** command no longer creates a duplicate audit record.

## 4.4.1: July 2025

**Fixed**

- Changes to the operating system's time format and timezone are now reflected in CellarioScheduler and no longer cause errors.
- Orders started from the ready orders' details screen of the Operator UI no longer revert to the **Ready to Run** panel when returning to the **Order Dashboard**.
- Placing a Script step (set to "AfterMove") right before a move requiring an AutoPod or robot with storage no longer causes the move to execute indefinitely.
- Viewing the driver window for the ProcessExecutor resource now works as expected.
- `InvalidOperationExecption` errors no longer occur when a login session expires.
- The dialog window requesting Windows administrator privileges no longer interrupts CellarioScheduler silent install mode.

## 4.4: July 2025

**New**

- A new Operator UI has been introduced for simplified order creation and order management. This new, separate UI displays a "swimlane" view of Ready, Started, and Finished orders with their estimated duration and progress for order tracking. You can change between the Operator UI and full UI under **Settings**.
- CellarioScheduler licensing is integrated into the application and includes online and offline activation options.
- You can now save protocol images to the database in addition to offline PNG files. (Saving the image to the database automatically creates a protocol preview image in the Operator UI if the preview was blank.)

**Improved**

- Protocol parameters now support dynamic load/unload sequences.
- The Load/Unload sequence is exposed in the CellarioScheduler Scripting API.
- Audit trail coverage is now expanded in the following areas:
  - User management
  - Protocol management
  - Autorecovery
  - Resource/system state
  - Driver window
  - Maintenance actions
  - Order run simulation and scheduling
  - Script library
- Audit trail now logs simulations and tracks system state transitions.
- The Audit Trail Viewer hook has been updated to support CellarioScheduler v4.3.

**Fixed**

- A quantity calculation discrepancy between the template and the Order List pane when using custom ratios has been corrected.
- Unexpected application shutdowns no longer occur when simulating active orders.
- Liquid transfer events logged through the API are now done correctly. Events were incorrectly recorded in the `Started` instead of `Finished` event, and `OperationResource` was null instead of showing the correct resource.
