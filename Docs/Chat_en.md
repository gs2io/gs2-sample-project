# Chat Description

This is a sample of using [GS2-Chat](https://app.gs2.io/docs/en/index.html#gs2-chat) to chat, send and receive messages.

![Chat](Chat_en.png)

## GS2-Deploy template

- [initialize_chat_template.yaml - chat](../Templates/initialize_chat_template.yaml)

## ChatSetting ChatSetting

![Inspector Window](ChatSetting.png)

| Setting Name | Description |
|---|---|
| chatNamespaceName | GS2-Chat namespace name
| roomName | GS2-Chat room name

| Event | Description |
|---|---|
| onGetRoom(EzRoom) | Called when room information is obtained. | onGetRoom(EzRoom)
| onListSubscribeRooms(List<EzSubscribe>) | Called when a list of subscribed rooms is obtained. | onListSubscribeRooms(List<EzSubscribe>)
| onCreateRoom(EzRoom) | Called when a room is created. | onCreateRoom(EzRoom)
| onSubscribe(EzSubscribe) | Called when a room subscription is performed. | onSubscribe(EzSubscribe)
| onUnsubscribe(EzSubscribe) | Called when a subscription is canceled. | onUnsubscribe(EzSubscribe)
| onListMessages(List<EzMessage>) | Called when a list of messages in a room is retrieved. | onListMessages(List<EzMessage>)
| onPost(EzMessage) | Called when a message is posted. | onPost(EzMessage)
| OnError(Gs2Exception error) | Called when an error occurs. | OnError(Gs2Exception error)

## Send message
```c#
AsyncResult<EzPostResult> result = null;
yield return client.Chat.Post(
    r => { result = r; }
    session,
    chatNamespaceName,
    roomName,
    message,
    null,
    null
);
````

## Receive messages

You will receive a notification from [GS2-Gateway](https://app.gs2.io/docs/en/index.html#gs2-gateway) when a message is posted to the room to which you are subscribed.
```c#
Gs2WebSocketSession
    public delegate void NotificationHandler(NotificationMessage message);
```

Retrieve the message.

```c#
AsyncResult<EzListMessagesResult> result = null;
yield return client.Chat.ListMessages(
    r => { result = r; }
    session,
    chatNamespaceName,
    roomName,
    null,
    null,
    null
);
````

Tapping the balloon of a message received from another player allows you to follow, friend request, or blacklist the player.
This function is for the friend function using GS2-Friend.
