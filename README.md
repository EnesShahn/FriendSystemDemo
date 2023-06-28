# Friend System

### Live demo:

[https://friend-system-26d0f.web.app/](https://friend-system-26d0f.web.app/)

# Introduction

The Friend System is a software system designed to facilitate the creation of a friend list and private messaging between users. The system will be built using the Unity engine for the client-side and ASP.NET Core for the backend with Firebase Firestore Database as the primary data storage solution.

The system aims to provide a user-friendly interface for interaction, alongside robust and secure backend operations. The system is compatible with WebGL and Windows platforms.

# System Overview

## User Interface

The user interface is built using the Unity engine. It allows users to:

- Chat with friends.
- Send and receive friend requests.
- Accept or decline friend requests.
- Chat privately with friends.
- Search for friends by name identifier.
- View online status and last active time of friends.
- Create a group with other users
- Chat with group members
- Manage group memebers

## Backend System

The backend system is built using ASP.NET Core. This technology is suitable for creating secure and efficient server-side operations for the Friend System.

## Database

Firebase Firestore Database is used to store user data, friend lists, chat messages, online status, groups. Firebase Firestore provides an efficient and secure NoSQL database solution, which is perfect for the requirements of the Friend System. It will store:

## Hosting

The WebGL client is hosted on Firebase hosting and the ASP.Net App is hosted on Azure.

## Security

User data will is securely stored in the Firestore database with access controlled by Firebase Authentication. This provides automatic handling of user authentication i.e. token verification.

## Compatibility

The system is designed to be compatible with both WebGL and Windows platforms. Unity, being a versatile game development platform, provides the flexibility to deploy on these different platforms without significant alterations to the code base.

# Data Models

### User

```csharp
public class User
{
    public string UserID { get; set; }
    public string Email { get; set; }
    public DateTime LastOnlineTime { get; set; }
}
```

### Token

```csharp
public class Token
{
    public string? IdToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? ExpiresIn { get; set; }
}
```

### Message

```csharp
public class Message
{
    public string MessageID { get; set; }
    public string SenderID { get; set; }
    public string Content { get; set; }
    public DateTime CreateTime { get; set; }
}
```

### Invitation

```csharp
public class Invitation
{
    public string InvitationID { get; set; }
    public InvitationType InvitationType { get; set; }
    public string SenderID { get; set; }
    public string ReceiverID { get; set; }
    public InvitationStatus Status { get; set; }
}
```

### Friendship

```csharp
public class Friendship
{
    public string FriendshipID { get; set; }
    public string User1ID { get; set; }
    public string User2ID { get; set; }
}
```

### Group

```csharp
public class Group
{
    public string GroupID { get; set; }
    public string Name { get; set; }
    public string CreatorUID { get; set; }
    public List<string> Members { get; set; }
}
```

# REST API

### BaseUrl = [https://friendsystem.azurewebsites.net](https://friendsystem.azurewebsites.net/)

## /User

### (GET) /User/GetUser

Summary: Get user info

Authorized: True

Request Parameters: None

Response Data: (bool Success, string Message, User Data) 

### (GET) /User/GetUserLastOnlineTime

Summary: Get user last online time 

Authorized: True

Request Parameters: None

Response Data: (bool Success, string Message, DateTime Data) 

### (POST) /User/Register

Summary: Register user and get JWToken

Authorized: False

Request Parameters: (string Email, string Password)

Response Data: (bool Success, string Message, Token Data) 

### (POST) /User/Login

Summary: Login user and get JWToken

Authorized: False

Request Parameters: (string Email, string Password)

Response Data:  (bool Success, string Message, Token Data) 

## /Invitation

### (GET) /Invitation/GetPendingInvitations

Summary: Get pending invitations

Authorized: True

Request Parameters: None

Response Data:  (bool Success, string Message, List<Invitation> Data)

### (POST) /Invitation/AcceptInvitation

Summary: Accept invitation

Authorized: True

Request Parameters: (string InvitationID)

Response Data: (bool Success, string Message)

### (POST) /Invitation/RejectInvitation

Summary: Reject invitation

Authorized: True

Request Parameters: (string InvitationID)

Response Data: (bool Success, string Message)

## /Friendship

### (GET) /Friendship/GetFriends

Summary: Get user’s friends’ id

Authorized: True

Request Parameters: None

Response Data: (bool Success, string Message)

### (GET) /Friendship/GetFriendsLastOnlineTime

Summary: Get user’s friends’ last online time

Authorized: True

Request Parameters: None

Response Data: (bool Success, string Message, Dictionary<string, DateTime> Data)

### (POST) /Friendship/GetFriendshipMessages

Summary: Get messages between user and a friend

Authorized: True

Request Parameters: (string FriendID)

Response Data: (bool Success, string Message, List<Message>Data)

### (POST) /Friendship/RemoveFriend

Summary: Removes a friend then returns friend data.

Authorized: True

Request Parameters: (string FriendID)

Response Data: (bool Success, string Message, User Data)

### (POST) /Friendship/SendMessage

Summary: Send a message to a friend and return the message.

Authorized: True

Request Parameters: (string FriendID)

Response Data: (bool Success, string Message, Message Data)

### (POST) /Friendship/SendFriendInvitation

Summary: Send a friend request to a user.

Authorized: True

Request Parameters: (string FriendID)

Response Data: (bool Success, string Message)

## /Group

### (GET) /Group/GetGroups

Summary: Gets the groups in which the user is part of.

Authorized: True

Request Parameters: None

Response Data: (bool Success, string Message, List<Group> Data)

### (POST) /Group/CreateGroup

Summary: Create a group and return it.

Authorized: True

Request Parameters: (string GroupName)

Response Data: (bool Success, string Message, Group Data)

### (POST) /Group/GetGroupMembers

Summary: Gets a group’s members’ data

Authorized: True

Request Parameters: (string GroupID)

Response Data: (bool Success, string Message, List<User> Data)

### (POST) /Group/GetGroupMessages

Summary: Gets a group’s messages

Authorized: True

Request Parameters: (string GroupID)

Response Data: (bool Success, string Message, List<Message> Data)

### (POST) /Group/GetGroupMembersLastOnlineTime

Summary: Gets a group’s members’ last online time.

Authorized: True

Request Parameters: (string GroupID)

Response Data: (bool Success, string Message, Dictionary<string, DateTime> Data)

### (POST) /Group/LeaveGroup

Summary: Leave a group and return the group data.

Authorized: True

Request Parameters: (string GroupID)

Response Data: (bool Success, string Message, Group Data)

### (POST) /Group/KickMember

Summary: Kick a member from a group and return the member data.

Authorized: True

Request Parameters: (string GroupID, string MemberID)

Response Data: (bool Success, string Message, User Data)

### (POST) /Group/SendMessage

Summary: Send a message to a group and return the message.

Authorized: True

Request Parameters: (string GroupID, string Message)

Response Data: (bool Success, string Message, Message Data)

### (POST) /Group/SendGroupInvitation

Summary: Send a group invitation to a user to join the group.

Authorized: True

Request Parameters: (string GroupID, string MemberID)

Response Data: (bool Success, string Message)

# Utility

**Object Pool**: Provides a simple Object Pooling mechanism.

**Debugger**: Provides simple debuggin feature with abillity to turn all logs off with a single setting.

**GameConfig**: Simple global config service, easily accessable from anywhere.

**PanelSystem**: a system that makes creating game panels a lot easier. 

# SDK

### Client

Our entry point to all functionallity related to fetching and updating data from/to backend.

Client can be extended with modules by extending BaseModule class .

BaseModule provides the developer with server events that could be used in any way to implement a new feature.

Client mainly handles functionallity related to the logged in user.

### Sub Modules

**FriendshipModule**: Contains functionallities that allows the user to manage friends, including messaging.

**GroupModule**: Contains functionallities that allows the user to manage the groups in which the user is part of, including messaging.

**InvitationModule**: Contains functionallities that allows user to manage invitations received

# UI

All game panels implement BasePanel from PanelSystem, each panel has different functionallity, some in which communicate with the SDK to fetch new data and update UI

# Assets used

https://www.flaticon.com/free-icon/gamer_4333609?term=avatar&page=1&position=5&origin=search&related_id=4333609
https://www.flaticon.com/free-icon/group-chat_4389561?term=group+chat&page=1&position=25&origin=search&related_id=4389561
https://www.flaticon.com/free-icon/add-group_3815950?term=new+group&page=1&position=45&origin=search&related_id=3815950
https://www.flaticon.com/free-icon/social-care_921356?term=social&page=1&position=79&origin=search&related_id=921356
https://www.flaticon.com/free-icon/paper-plane_9131510?term=send&page=1&position=26&origin=search&related_id=9131510
https://www.flaticon.com/free-icon/add-friend_11246037
https://www.flaticon.com/free-icon/accept_4315445
https://www.flaticon.com/free-icon/decline_10621089?term=reject&page=1&position=3&origin=search&related_id=10621089
https://www.flaticon.com/free-icon/chat-bubbles_4856398?term=message&page=1&position=47&origin=search&related_id=4856398


