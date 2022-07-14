# Version Check Explanation

This sample uses [GS2-Version](https://app.gs2.io/docs/en/index.html#gs2-version) to check the version of an application when it is launched and to confirm user acceptance of the Terms of Use.

## GS2-Deploy template

- [initialize_version_template.yaml - app version check](../Templates/initialize_version_template.yaml)
- [initialize_term_template.yaml - Terms of Service Check](../Templates/initialize_term_template.yaml)

## VersionSetting VersionSetting

![Inspector Window](Version.png)

| Setting Name | Description |
---|---
| versionNamespaceName | Namespace name for GS2-Version app version check
| versionName | Version name of the app version check for GS2-Version
| currentVersionMajor | current version number of the app Major part of the app
| currentVersionMinor | current version number of the app Minor part | current version number of the app
| currentVersionMicro | current version number of the app Micro part

| Event | Description |
---|---
| onCheckVersion | Called when the result of performing a version check is retrieved. | onCheckVersion
| OnError(Gs2Exception error) | Called when an error occurs. | OnError(Gs2Exception error)

## Terms of Service Setting TermSetting

![Inspector Window](Term.png)

| Setting Name | Description |
---|---
| versionNamespaceName | Namespace Name of GS2-Version's Terms of Use Check
| versionName | version name of GS2-Version's Terms of Use check

| Event | Description |
---|---
| onCheckVersion | Called when the result of performing a version check of the Terms of Use is obtained. | onCheckVersion
| OnError(Gs2Exception error) | Called when an error occurs. | OnError(Gs2Exception error)

## Enable version checking functionality

In the project file at the time of acquisition from the repository, the version check process of the application after "application startup", the  
The Terms of Use confirmation process is disabled.  
To enable it, you need to change the  
Uncheck each of the following

![Inspector Window](VersionCheck.png)

## Version check flow

The current version of the app and the version set in the master data on the server side  
Compare each.  
If the version is older than the `warningVersion` that prompts the user to upgrade to the next version, it returns a Warning.  
`errorVersion` If the version is older than the version that requires  
Returns the result.

In the case of an Error, the application will prompt the user to upgrade the version of the application and  
Guidance to the distribution platform, etc.

```c#
EzVersion version = new EzVersion();
version.Major = 0;
Minor = 0;
Micro = 0;
Version = version;
Version = version; targetVersion.VersionName = versionName;
Add(targetVersion);

yield return client.Version.CheckVersion(
    r =>
    {
        result = r;
    },
    session, // Session object representing the GameSession login state
    versionNamespaceName, // namespace name
    targetVersions
);
```

## Flow of Terms of Use Confirmation Check

Compares the version of the agreement set in the master data on the server side with the approved version  
Unapproved versions are returned as results in Errors and Warnings.

```c#
yield return client.Version.CheckVersion(
    r =>
    {
        result = r;
    },
    session, // Session object representing the GameSession login state
    versionNamespaceName, // namespace name
    targetVersions
);
````

Display the terms of use to the user, obtain his/her consent, and send the approval to the server.  
It is stored on the server as the approved version for that user.

```c#
AsyncResult<EzAcceptResult> result = null;
var current = client.Version.Accept(
    r =>
    {
        result = r;
    },
    session,
    namespaceName: versionNamespaceName,
    versionName: versionName
);
```