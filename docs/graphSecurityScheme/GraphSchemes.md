#Delegated

| _Bookings.Read.All_ |  Allows an app to read Bookings appointments, businesses, customers, services, and staff on behalf of the signed-in user. | Intended for read-only applications. Typical target user is the customer of a booking business. | No | No |
| _Bookings.ReadWrite.Appointments_ | Allows an app to read and write Bookings appointments and customers, and additionally allows reading businesses, services, and staff on behalf of the signed-in user. | Intended for scheduling applications which need to manipulate appointments and customers. Cannot change fundamental information about the booking business, nor its services and staff members. Typical target user is the customer of a booking business.| No | No |
| _Bookings.ReadWrite.All_ | Allows an app to read and write Bookings appointments, businesses, customers, services, and staff on behalf of the signed-in user. Does not allow create, delete, or publish of Bookings businesses. | Intended for management applications that manipulate existing businesses, their services and staff members. Cannot create, delete, or change the publishing status of a booking business. Typical target user is the support staff of an organization.| No | No |
| _Bookings.Manage_ | Allows an app to read, write, and manage Bookings appointments, businesses, customers, services, and staff on behalf of the signed-in user.  | Allows the app to have full access. <br>Intended for a full management experience. Typical target user is the administrator of an organization.| No | No |


| _Calendars.Read_ |Read user calendars |Allows the app to read events in user calendars. |No | Yes |
| _Calendars.Read.Shared_ |Read user and shared calendars |Allows the app to read events in all calendars that the user can access, including delegate and shared calendars. |No | No |
| _Calendars.ReadWrite_ |Have full access to user calendars |Allows the app to create, read, update, and delete events in user calendars. |No | Yes |
| _Calendars.ReadWrite.Shared_ |Read and write user and shared calendars |Allows the app to create, read, update and delete events in all calendars the user has permissions to access. This includes delegate and shared calendars.|No | No |

|_Contacts.Read_ |Read user contacts  |Allows the app to read user contacts. |No | Yes |
|_Contacts.Read.Shared_ |Read user and shared contacts |Allows the app to read contacts that the user has permissions to access, including the user's own and shared contacts. |No |No|
|_Contacts.ReadWrite_ |Have full access to user contacts |Allows the app to create, read, update, and delete user contacts. |No |Yes|
|_Contacts.ReadWrite.Shared_ |Read and write user and shared contacts |Allows the app to create, read, update and delete contacts that the user has permissions to, including the user's own and shared contacts. |No |No|

|_Device.Read_ |Read user devices |Allows the app to read a user's list of devices on behalf of the signed-in user. |No | Yes |
|_Device.Command_ |Communicate with user devices |Allows the app to launch another app or communicate with another app on a user's device on behalf of the signed-in user. |No | Yes |

| _Directory.Read.All_ |Read directory data | Allows the app to read data in your organization's directory, such as users, groups and apps. **Note**: Users may consent to applications that require this permission if the application is registered in their own organization’s tenant.| Yes | No |
| _Directory.ReadWrite.All_ |Read and write directory data | Allows the app to read and write data in your organization's directory, such as users, and groups. It does not allow the app to delete users or groups, or reset user passwords. | Yes | No |
| _Directory.AccessAsUser.All_ |Access directory as the signed-in user  | Allows the app to have the same access to information in the directory as the signed-in user. | Yes | No |

|EduAdministration.Read | Read education app settings |  Allows the app to read education app settings on behalf of the user. | Yes | No |
|EduAdministration.ReadWrite | Manage education app settings | Allows the app to manage education app settings on behalf of the user. | Yes | No |
|EduAssignments.ReadBasic | Read users' class assignments without grades | Allows the app to read assignments without grades on behalf of the user | Yes | No |
|EduAssignments.ReadWriteBasic | Read and write users' class assignments without grades | Allows the app to read and write assignments without grades on behalf of the user | Yes | No |
|EduAssignments.Read | Read users' view of class assignments and their grades | Allows the app to read assignments and their grades on behalf of the user| Yes | No |
|EduAssignments.ReadWrite | Read and write users' view of class assignments and their grades | Allows the app to read and write assignments and their grades on behalf of the user|Yes | No |
|EduRostering.ReadBasic| Read a limited subset of users' view of the roster | Allows the app to read a limited subset of the data from the  structure of schools and classes in an organization's roster and  education-specific information about users to be read on behalf of the user.  | Yes  | No |

| _Files.Read_ | Read user files | Allows the app to read the signed-in user's files. | No | Yes |
| _Files.Read.All_ | Read all files that user can access | Allows the app to read all files the signed-in user can access. | No | Yes |
| _Files.ReadWrite_  | Have full access to user files | Allows the app to read, create, update, and delete the signed-in user's files. | No| Yes |
| _Files.ReadWrite.All_ | Have full access to all files user can access | Allows the app to read, create, update, and delete all files the signed-in user can access. | No | Yes |
| _Files.ReadWrite.AppFolder_ | Have full access to the application's folder (preview) | (Preview) Allows the app to read, create, update, and delete files in the application's folder. | No | No |
| _Files.Read.Selected_  | Read files that the user selects | **Limited support in Microsoft Graph; see Remarks** <br/> (Preview) Allows the app to read files that the user selects. The app has access for several hours after the user selects a file.  | No | No |
| _Files.ReadWrite.Selected_ | Read and write files that the user selects | **Limited support in Microsoft Graph; see Remarks** <br/> (Preview) Allows the app to read and write files that the user selects. The app has access for several hours after the user selects a file. | No | No |

| _Group.Read.All_ |    Read all groups | Allows the app to list groups, and to read their properties and all group memberships on behalf of the signed-in user.  Also allows the app to read calendar, conversations, files, and other group content for all groups the signed-in user can access. | Yes | No |
| _Group.ReadWrite.All_ |    Read and write all groups| Allows the app to create groups and read all group properties and memberships on behalf of the signed-in user.  Additionally allows group owners to manage their groups and allows group members to update group content. | Yes | No |

| _IdentityRiskEvent.Read.All_ |   Read identity risk event information  | Allows the app to read identity risk event information for all users in your organization on behalf of the signed-in user. | Yes | No |

| _IdentityProvider.Read.All_ |   Read identity provider information  | Allows the app to read identity providers configured in your Azure AD or Azure AD B2C tenant on behalf of the signed-in user. | Yes | No |
| _IdentityProvider.ReadWrite.All_ |   Read and write identity provider information  |  Allows the app to read or write identity providers configured in your Azure AD or Azure AD B2C tenant on behalf of the signed-in user. | Yes | No |

|_DeviceManagementApps.Read.All_ | Read Microsoft Intune apps | Allows the app to read the properties, group assignments and status of apps, app configurations and app protection policies managed by Microsoft Intune. | Yes | No |
|_DeviceManagementApps.ReadWrite.All_ | Read and write Microsoft Intune apps | Allows the app to read and write the properties, group assignments and status of apps, app configurations and app protection policies managed by Microsoft Intune. | Yes | No |
|_DeviceManagementConfiguration.Read.All_ | Read Microsoft Intune device configuration and policies | Allows the app to read properties of Microsoft Intune-managed device configuration and device compliance policies and their assignment to groups. | Yes | No |
|_DeviceManagementConfiguration.ReadWrite.All_ | Read and write Microsoft Intune device configuration and policies  | Allows the app to read and write properties of Microsoft Intune-managed device configuration and device compliance policies and their assignment to groups. | Yes | No |
|_DeviceManagementManagedDevices.PrivilegedOperations.All_ | Perform user-impacting remote actions on Microsoft Intune devices | Allows the app to perform remote high impact actions such as wiping the device or resetting the passcode on devices managed by Microsoft Intune. | Yes | No |
|_DeviceManagementManagedDevices.Read.All_ | Read Microsoft Intune devices | Allows the app to read the properties of devices managed by Microsoft Intune. | Yes | No |
|_DeviceManagementManagedDevices.ReadWrite.All_ | Read and write Microsoft Intune devices | Allows the app to read and write the properties of devices managed by Microsoft Intune. Does not allow high impact operations such as remote wipe and password reset on the device’s owner. | Yes | No |
|_DeviceManagementRBAC.Read.All_ | Read Microsoft Intune RBAC settings | Allows the app to read the properties relating to the Microsoft Intune Role-Based Access Control (RBAC) settings. | Yes | No |
|_DeviceManagementRBAC.ReadWrite.All_ | Read and write Microsoft Intune RBAC settings | Allows the app to read and write the properties relating to the Microsoft Intune Role-Based Access Control (RBAC) settings. | Yes | No |
|_DeviceManagementServiceConfig.Read.All_ | Read Microsoft Intune configuration | Allows the app to read Intune service properties including device enrollment and third party service connection configuration. | Yes | No |
|_DeviceManagementServiceConfig.ReadWrite.All_ | Read and write Microsoft Intune configuration | Allows the app to read and write Microsoft Intune service properties including device enrollment and third party service connection configuration. | Yes | No |

| _Mail.Read_ |    Read user mail | Allows the app to read email in user mailboxes. | No | Yes
| _Mail.ReadWrite_ |    Read and write access to user mail | Allows the app to create, read, update, and delete email in user mailboxes. Does not include permission to send mail.| No | Yes
| _Mail.Read.Shared_ |    Read user and shared mail | Allows the app to read mail that the user can access, including the user's own and shared mail. | No | No
| _Mail.ReadWrite.Shared_ |    Read and write user and shared mail | Allows the app to create, read, update, and delete mail that the user has permission to access, including the user's own and shared mail. Does not include permission to send mail. | No | No
| _Mail.Send_ |    Send mail as a user | Allows the app to send mail as users in the organization. | No | No
| _Mail.Send.Shared_ |    Send mail on behalf of others | Allows the app to send mail as the signed-in user, including sending on-behalf of others. | No | No
| _MailboxSettings.Read_ |  Read user mailbox settings | Allows the app to the read user's mailbox settings. Does not include permission to send mail. | No | No
| _MailboxSettings.ReadWrite_ |  Read and write user mailbox settings | Allows the app to create, read, update, and delete user's mailbox settings. Does not include permission to directly send mail, but allows the app to create rules that can forward or redirect messages. | No | No
| _Member.Read.Hidden_ | Read hidden memberships | Allows the app to read the memberships of hidden groups and administrative units on behalf of the signed-in user, for those hidden groups and administrative units that the signed-in user has access to. | Yes | No |

| _Notes.Read_ |    Read user OneNote notebooks | Allows the app to read the titles of OneNote notebooks and sections and to create new pages, notebooks, and sections on behalf of the signed-in user. | No | Yes
| _Notes.Create_ |    Create user OneNote notebooks | Allows the app to read the titles of OneNote notebooks and sections and to create new pages, notebooks, and sections on behalf of the signed-in user.| No | Yes
| _Notes.ReadWrite_ |    Read and write user OneNote notebooks | Allows the app to read, share, and modify OneNote notebooks on behalf of the signed-in user. | No | Yes
| _Notes.Read.All_ |    Read all OneNote notebooks that user can access | Allows the app to read OneNote notebooks that the signed-in user has access to in the organization. | No | Yes
| _Notes.ReadWrite.All_ |    Read and write all OneNote notebooks that user can access | Allows the app to read, share, and modify OneNote notebooks that the signed-in user has access to in the organization.| No | No
| _Notes.ReadWrite.CreatedByApp_ |    Limited notebook access (deprecated) | **Deprecated** <br/>Do not use. No privileges are granted by this permission. | No | No

| _email_ |    View users' email address | Allows the app to read your users' primary email address. | No | No |
| _offline_access_ |    Access user's data anytime | Allows the app to read and update user data, even when they are not currently using the app.| No | No |
| _openid_ |    Sign users in | Allows users to sign in to the app with their work or school accounts and allows the app to see basic user profile information.| No | No |
| _profile_ |    View users' basic profile | Allows the app to see your users' basic profile (name, picture, user name).| No | No |

| _People.Read_ |    Read users' relevant people lists | Allows the app to read a scored list of people relevant to the signed-in user. The list can include local contacts, contacts from social networking or your organization's directory, and people from recent communications (such as email and Skype). | No | No |
| _People.Read.All_ | Read all users' relevant people lists | Allows the app to read a scored list of people relevant to the signed-in user or other users in the signed-in user's organization. The list can include local contacts, contacts from social networking or your organization's directory, and people from recent communications (such as email and Skype). Also allows the app to search the entire directory of the signed-in user's organization. | Yes | No |

| _Reports.Read.All_ | Read all usage reports | Allows an app to read all service usage reports without a signed-in user. Services that provide usage reports include Office 365 and Azure Active Directory. | Yes | No |
| _SecurityEvents.Read.All_        |  Read your organization’s security events | Allows the app to read your organization’s security events on behalf of the signed-in user. | Yes  | No |
| _SecurityEvents.ReadWrite.All_   | Read and update your organization’s security events | Allows the app to read your organization’s security events on behalf of the signed-in user. Also allows the app to update editable properties in security events on behalf of the signed-in user. | Yes  | No |

| _Sites.Read.All_        | Read items in all site collections | Allows the app to read documents and list items in all site collections on behalf of the signed-in user. | No  | No |
| _Sites.ReadWrite.All_   | Read and write items in all site collections | Allows the app to edit or delete documents and list items in all site collections on behalf of the signed-in user. | No  | No |
| _Sites.Manage.All_      | Create, edit, and delete items and lists in all site collections | Allows the app to manage and create lists, documents, and list items in all site collections on behalf of the signed-in user. | No | No |
| _Sites.FullControl.All_ | Have full control of all site collections | Allows the app to have full control to SharePoint sites in all site collections on behalf of the signed-in user.  | Yes  | No |
| _Tasks.Read_ | Read user tasks | Allows the app to read user tasks. | No | Yes |
| _Tasks.Read.Shared_ | Read user and shared tasks | Allows the app to read tasks a user has permissions to access, including their own and shared tasks. | No | No |
| _Tasks.ReadWrite_ |    Create, read, update and delete user tasks and containers | Allows the app to create, read, update and delete tasks and containers (and tasks in them) that are assigned to or shared with the signed-in user.| No | Yes |
| _Tasks.ReadWrite.Shared_ | Read and write user and shared tasks | Allows the app to create, read, update, and delete tasks a user has permissions to, including their own and shared tasks. | No | No |
| _Agreement.Read.All_ | Read all terms of use agreements | Allows the app to read terms of use agreements on behalf of the signed-in user. | Yes | No |
| _Agreement.ReadWrite.All_ | Read and write all terms of use agreements | Allows the app to read and write terms of use agreements on behalf of the signed-in user. | Yes | No |
| _AgreementAcceptance.Read_ | Read user terms of use acceptance statuses | Allows the app to read terms of use acceptance statuses on behalf of the signed-in user. | Yes | No |
| _AgreementAcceptance.Read.All_ | Read terms of use acceptance statuses that user can access | Allows the app to read terms of use acceptance statuses on behalf of the signed-in user. | Yes | No |
| _User.Read_       |    Sign-in and read user profile | Allows users to sign-in to the app, and allows the app to read the profile of signed-in users. It also allows the app to read basic company information of signed-in users.| No | Yes |
| _User.ReadWrite_ |    Read and write access to user profile | Allows the app to read the signed-in user's full profile. It also allows the app to update the signed-in user's profile information on their behalf. | No | Yes |
| _User.ReadBasic.All_ |    Read all users' basic profiles | Allows the app to read a basic set of profile properties of other users in your organization on behalf of the signed-in user. This includes display name, first and last name, email address, open extensions and photo. Also allows the app to read the full profile of the signed-in user. | No | Yes |
| _User.Read.All_  |     Read all users' full profiles           | Allows the app to read the full set of profile properties, reports, and managers of other users in your organization, on behalf of the signed-in user. | Yes | Yes |
| _User.ReadWrite.All_ |     Read and write all users' full profiles | Allows the app to read and write the full set of profile properties, reports, and managers of other users in your organization, on behalf of the signed-in user. Also allows the app to create and delete users as well as reset user passwords on behalf of the signed-in user. | Yes | Yes |
| _User.Invite.All_  |     Invite guest users to the organization | Allows the app to invite guest users to your organization, on behalf of the signed-in user. | Yes | Yes |
| _User.Export.All_       |    Export users' data | Allows the app to export an organizational user's data, when performed by a Company Administrator.| Yes | Yes |
| _UserActivity.ReadWrite.CreatedByApp_ |Read and write app activity to users' activity feed |Allows the app to read and report the signed-in user's activity in the app. |No | Yes 






#Delegated Personal
| _Calendars.Read_ |Read user calendars |Allows the app to read events in user calendars. |No | Yes |
| _Calendars.ReadWrite_ |Have full access to user calendars |Allows the app to create, read, update, and delete events in user calendars. |No | Yes |
| _Contacts.Read_ |Read user contacts  |Allows the app to read user contacts. |No | Yes |
| _Contacts.ReadWrite_ |Have full access to user contacts |Allows the app to create, read, update, and delete user contacts. |No |Yes|

|_Device.Read_ |Read user devices |Allows the app to read a user's list of devices on behalf of the signed-in user. |No | Yes |
|_Device.Command_ |Communicate with user devices |Allows the app to launch another app or communicate with another app on a user's device on behalf of the signed-in user. |No | Yes |
| _Files.Read_ | Read user files | Allows the app to read the signed-in user's files. | No | Yes |
| _Files.Read.All_ | Read all files that user can access | Allows the app to read all files the signed-in user can access. | No | Yes |
| _Files.ReadWrite_  | Have full access to user files | Allows the app to read, create, update, and delete the signed-in user's files. | No| Yes |
| _Files.ReadWrite.All_ | Have full access to all files user can access | Allows the app to read, create, update, and delete all files the signed-in user can access. | No | Yes |

| _Mail.ReadWrite_ |    Read and write access to user mail | Allows the app to create, read, update, and delete email in user mailboxes. Does not include permission to send mail.| No | Yes
| _Member.Read.Hidden_ | Read all hidden memberships | Allows the app to read the memberships of hidden groups and administrative units without a signed-in user. | Yes |
| _Notes.Read_ |    Read user OneNote notebooks | Allows the app to read the titles of OneNote notebooks and sections and to create new pages, notebooks, and sections on behalf of the signed-in user. | No | Yes
| _Notes.Create_ |    Create user OneNote notebooks | Allows the app to read the titles of OneNote notebooks and sections and to create new pages, notebooks, and sections on behalf of the signed-in user.| No | Yes
| _Notes.ReadWrite_ |    Read and write user OneNote notebooks | Allows the app to read, share, and modify OneNote notebooks on behalf of the signed-in user. | No | Yes
| _Notes.Read.All_ |    Read all OneNote notebooks that user can access | Allows the app to read OneNote notebooks that the signed-in user has access to in the organization. | No | Yes
| _Tasks.Read_ | Read user tasks | Allows the app to read user tasks. | No | Yes |
| _Tasks.ReadWrite_ |    Create, read, update and delete user tasks and containers | Allows the app to create, read, update and delete tasks and containers (and tasks in them) that are assigned to or shared with the signed-in user.| No | Yes |
| _User.Read_       |    Sign-in and read user profile | Allows users to sign-in to the app, and allows the app to read the profile of signed-in users. It also allows the app to read basic company information of signed-in users.| No | Yes |
| _User.ReadWrite_ |    Read and write access to user profile | Allows the app to read the signed-in user's full profile. It also allows the app to update the signed-in user's profile information on their behalf. | No | Yes |
| _User.ReadBasic.All_ |    Read all users' basic profiles | Allows the app to read a basic set of profile properties of other users in your organization on behalf of the signed-in user. This includes display name, first and last name, email address, open extensions and photo. Also allows the app to read the full profile of the signed-in user. | No | Yes |
| _User.Read.All_  |     Read all users' full profiles           | Allows the app to read the full set of profile properties, reports, and managers of other users in your organization, on behalf of the signed-in user. | Yes | Yes |
| _User.ReadWrite.All_ |     Read and write all users' full profiles | Allows the app to read and write the full set of profile properties, reports, and managers of other users in your organization, on behalf of the signed-in user. Also allows the app to create and delete users as well as reset user passwords on behalf of the signed-in user. | Yes | Yes |
| _User.Invite.All_  |     Invite guest users to the organization | Allows the app to invite guest users to your organization, on behalf of the signed-in user. | Yes | Yes |
| _User.Export.All_       |    Export users' data | Allows the app to export an organizational user's data, when performed by a Company Administrator.| Yes | Yes |

| _UserActivity.ReadWrite.CreatedByApp_ |Read and write app activity to users' activity feed |Allows the app to read and report the signed-in user's activity in the app. |No | Yes 









#Application
| _Application.ReadWrite.All_ | Read and write all apps | Allows the calling app to create, and manage (read, update, update application secrets and delete) applications and service principals without a signed-in user.  Does not allow management of consent grants or application assignments to users or groups. | Yes |
| _Application.ReadWrite.OwnedBy_ | Manage apps that this app creates or owns | Allows the calling app to create other applications and service principals, and fully manage those applications and service principals (read, update, update application secrets and delete), without a signed-in user.  It cannot update any applications that it is not an owner of. Does not allow management of consent grants or application assignments to users or groups. | Yes |
|_Calendars.Read_ |Read calendars in all mailboxes |Allows the app to read events of all calendars without a signed-in user. |Yes |
|_Calendars.ReadWrite_ |Read and write calendars in all mailboxes |Allows the app to create, read, update, and delete events of all calendars without a signed-in user. |Yes |

|_Contacts.Read_ |Read contacts in all mailboxes |Allows the app to read all contacts in all mailboxes without a signed-in user. |Yes |
|_Contacts.ReadWrite_ |Read and write contacts in all mailboxes |Allows the app to create, read, update, and delete all contacts in all mailboxes without a signed-in user. |Yes |
|_Device.ReadWrite.All_ |Read and write devices |Allows the app to read and write all device properties without a signed in user. Does not allow device creation, device deletion, or update of device alternative security identifiers. |Yes |

| _Directory.Read.All_ | Read directory data | Allows the app to read data in your organization's directory, such as users, groups and apps, without a signed-in user. | Yes |
| _Directory.ReadWrite.All_ | Read and write directory data | Allows the app to read and write data in your organization's directory, such as users, and groups, without a signed-in user. Does not allow user or group deletion. | Yes |

|_EduAssignments.ReadBasic.All_| Read class assignments without grades|Allows the app to read assignments without grades for all users| Yes |
|_EduAssignments.ReadWriteBasic.All_ | Read and write class assignments without grades | Allows the app to read and write assignments without grades for all users| Yes |
|_EduAssignments.Read.All_| Read class assignments with grades | Allows the app to read assignments and their grades for all users | Yes |
|_EduAssignments.ReadWrite.All_ | Read and write class assignments with grades | Allows the app to read and write assignments and their grades for all users | Yes |
|_EduRostering.ReadBasic.All_ | Read a limited subset of the organization's roster. | Allows the app to read a limited subset of both the structure of schools and classes in an organization's roster and education-specific information about all users. | Yes |
|_EduRostering.Read.All_ | Read the organization's roster. | Allows the app to read the structure of schools and classes in the organization's roster and education-specific information about all users to be read. | Yes |
|_EduRostering.ReadWrite.All_| Read and write the organization's roster. | Allows the app to read and write the structure of schools and classes in the organization's roster and education-specific information about all users to be read and written.  | Yes |
| _Files.Read.All_ | Read files in all site collections | Allows the app to read all files in all site collections without a signed in user.  | Yes |
| _Files.ReadWrite.All_ | Read and write files in all site collections | Allows the app to read, create, update, and delete all files in all site collections without a signed in user. | Yes |

| _Group.Read.All_ | Read all groups | Allows the app to read memberships for all groups without a signed-in user. > **NOTE:** that not all group API supports access using app-only permissions. See [known issues](../concepts/known_issues.md) for examples. | Yes |
| _Group.ReadWrite.All_ | Read and write all groups | Allows the app to create groups, read and update group memberships, and delete groups. All of these operations can be performed by the app without a signed-in user. > **NOTE:** that not all group API supports access using app-only permissions. See [known issues](../concepts/known_issues.md) for examples.| Yes |
| _IdentityRiskEvent.Read.All_ |   Read identity risk event information | Allows the app to read identity risk event information for all users in your organization without a signed-in user. | Yes |
|   Permission    |  Display String   |  Description | Admin Consent Required |
|:-----------------------------|:-----------------------------------------|:-----------------|:-----------------|
| _Mail.Read_       |    Read mail in all mailboxes | Allows the app to read mail in all mailboxes without a signed-in user.| Yes |
| _Mail.ReadWrite_ |    Read and write mail in all mailboxes | Allows the app to create, read, update, and delete mail in all mailboxes without a signed-in user. Does not include permission to send mail. | Yes |
| _Mail.Send_ |    Send mail as any user | Allows the app to send mail as any user without a signed-in user. | Yes | 
| _MailboxSettings.Read_ |  Read all user mailbox settings | Allows the app to read user's mailbox settings without a signed-in user. Does not include permission to send mail. | No |
| _MailboxSettings.ReadWrite_ | Read and write all user mailbox settings  | Allows the app to create, read, update, and delete user's mailbox settings without a signed-in user. Does not include permission to send mail. | Yes |

| _Notes.Read.All_ |    Read all OneNote notebooks | Allows the app to read all the OneNote notebooks in your organization, without a signed-in user. | Yes |
| _Notes.ReadWrite.All_ |    Read and write all OneNote notebooks | Allows the app to read, share, and modify all the OneNote notebooks in your organization, without a signed-in user.| Yes |

| _People.Read.All_ | Read all users' relevant people lists | Allows the app to read a scored list of people relevant to the signed-in user or other users in the signed-in user's organization. <br/><br/>The list can include local contacts, contacts from social networking or your organization's directory, and people from recent communications (such as email and Skype). Also allows the app to search the entire directory of the signed-in user's organization. | Yes |

| _Reports.Read.All_ | Read all usage reports | Allows an app to read all service usage reports without a signed-in user. Services that provide usage reports include Office 365 and Azure Active Directory. | Yes |
| _SecurityEvents.Read.All_        |  Read your organization’s security events | Allows the app to read your organization’s security events. | Yes  |
| _SecurityEvents.ReadWrite.All_   | Read and update your organization’s security events | Allows the app to read your organization’s security events. Also allows the app to update editable properties in security events. | Yes  |
| _Sites.Read.All_        | Read items in all site collections | Allows the app to read documents and list items in all site collections without a signed in user. | Yes |
| _Sites.ReadWrite.All_   | Read and write items in all site collections | Allows the app to create, read, update, and delete documents and list items in all site collections without a signed in user. | Yes |
| _Sites.Manage.All_      | Have full control of all site collections | Allows the app to manage and create lists, documents, and list items in all site collections without a signed-in user.  | Yes  |
| _Sites.FullControl.All_ | Create, edit, and delete items and lists in all site collections | Allows the app to have full control to SharePoint sites in all site collections without a signed-in user.  | Yes  |

| _User.Read.All_ |    Read all users' full profiles | Allows the app to read the full set of profile properties, group membership, reports and managers of other users in your organization, without a signed-in user.| Yes |
| _User.ReadWrite.All_ |   Read and write all users' full profiles | Allows the app to read and write the full set of profile properties, group membership, reports and managers of other users in your organization, without a signed-in user.  Also allows the app to create and delete non-administrative users. Does not allow reset of user passwords. | Yes |
| _User.Invite.All_  |     Invite guest users to the organization | Allows the app to invite guest users to your organization, without a signed-in user. | Yes |
| _User.Export.All_       |    Export users' data | Allows the app to export organizational users' data, without a signed-in user.| Yes |