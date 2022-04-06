# Create and login account Description

Sample function to login using [GS2-Account](https://app.gs2.io/docs/en/index.html#gs2-account).

## GS2-Deploy template

- [initialize_account_template.yaml](../Templates/initialize_account_template.yaml)

## LoginSetting LoginSetting

<img src="Gs2Settings.png" width="70%"/>

| Setting Name | Description |
--------|-----
| accountNamespaceName | GS2-Account's namespace name
| accountEncryptionKeyId | encryption key GRN of GS2-Key used to encrypt account information in GS2-Account
| gatewayNamespaceName | namespace name of GS2-Gateway

| Event | Description |
---------|------
| OnLoadAccount(EzAccount account) | Called when an account is loaded. | OnLoadAccount(EzAccount account)
| OnSaveAccount(EzAccount account) | Called when an account is saved. | OnSaveAccount(EzAccount account)
| OnCreateAccount(EzAccount account) | Called when an account is created. | OnCreateAccount(EzAccount account)
| OnLogin(EzAccount account, GameSession session) | Called when login succeeds. | OnLogin(EzAccount account, GameSession session)
| OnError(Gs2Exception error) | Called when an error occurs. | OnError(Gs2Exception error)

## Login Flow

Loads the saved account information from PlayerPrefs.  
If the account information has already been saved, login is performed.  
If there is no saved account information, such as at first startup, a new account is created.

### Create Account

Create an account with __GS2-Account__.

```c#
AsyncResult<EzCreateResult> result = null;
yield return client.Account.Create(
    r =>
    {
        result = r;
    },
    accountNamespaceName
);
```

Save the newly created account information in PlayerPrefs.

### delete account

Deletes account information from local storage.  
This function is mainly for debugging purposes.  
If a login is performed to an environment with a different project or namespace from the account information already stored on the client, the account information will be deleted from local storage.  
The login will fail because the account information does not exist on the server side. In such a case, delete the account information, and then click  
A new account will be created and login will be possible.

### Login Process

```c#
AsyncResult<EzAuthenticationResult> result = null;
yield return client.Account.Authentication(
    r =>
    {
        result = r;
    },
    accountNamespaceName,
    userId,
    accountEncryptionKeyId,
    password
);
```

```c#
AsyncResult<EzLoginResult> result2 = null;
yield return client.Auth.Login(
    r =>
    {
        result2 = r;
    },
    userId,
    accountEncryptionKeyId,
    Result.Body,
    Result.Signature
);

var session = new GameSession(
    new AccessToken()
        WithToken(result2.Result.Token)
        WithExpire(result2.Result.Expire)
        WithUserId(result2.Result.UserId)
);
```

The __GS2-Account__ authentication process is performed with the account information.  
The body and signature of the resulting EzAuthenticationResult are  
[GS2-Auth](https://app.gs2.io/docs/en/index.html#gs2-auth) to the login process to obtain an access token.

```c#
AsyncResult<EzSetUserIdResult> result3 = null;
yield return client.Gateway.SetUserId(
    r => { result3 = r; }
    session,
    gatewayNamespaceName,
    true
);
```

Set your own user ID logged in to [GS2-Gateway](https://app.gs2.io/docs/en/index.html#gs2-gateway) and
and receive push notifications for this user client.  
Notification of message posting in chat ([GS2-Chat](Chat_en.md)), notification of
Notification of [GS2-Friend](Friend_en.md), etc.
Used to receive notifications of matchmaking ([GS2-Matchmaking](Matchmaking_en.md)) transitions.


