# 계정 생성·로그인 해설

[GS2-Account](https://docs.gs2.io/ko/api_reference/account/) 를 사용하여 로그인하는 기능의 샘플입니다.

## GS2-Deploy 템플릿

- [initialize_core_template.yaml](../Templates/initialize_core_template.yaml)

## 로그인 설정 LoginSetting

![Login](LoginSetting.png)

| 설정 이름 | 설명 |
--------|-----
| accountNamespaceName | GS2-Account 의 네임스페이스 이름 |
| accountEncryptionKeyId | GS2-Account 에서 계정 정보의 암호화에 사용하는 GS2-Key 의 암호 키 GRN |
| gatewayNamespaceName | GS2-Gateway 의 네임스페이스 이름 |

| 이벤트 | 설명 |
---------|------
| OnLoadAccount(EzAccount account) | 계정이 로드되었을 때 호출됩니다. |
| OnSaveAccount(EzAccount account) | 계정이 저장되었을 때 호출됩니다. |
| OnCreateAccount(EzAccount account) | 계정이 생성되었을 때 호출됩니다. |
| OnLogin(EzAccount account, GameSession session) | 로그인에 성공했을 때 호출됩니다. |
| OnError(Gs2Exception error) | 오류가 발생했을 때 호출됩니다. |

## 로그인의 흐름

PlayerPrefs 에서 저장된 계정 정보를 읽어 들입니다.  
이미 저장된 계정 정보가 있으면 로그인을 실행합니다.  
최초 실행 시 등, 저장된 계정 정보가 없을 때에는 계정을 새로 생성합니다.

### 계정 생성

__GS2-Account__ 에 새 계정을 생성합니다.

UniTask 활성화 시
```c#
var domain = gs2.Account.Namespace(
    namespaceName: accountNamespaceName
);
var result = await domain.CreateAsync();
try
{
    _account = await result.ModelAsync();
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
    return;
}

onCreateAccount.Invoke(_account);
```
코루틴 사용 시
```c#
var domain = gs2.Account.Namespace(
    namespaceName: accountNamespaceName
);
var future = domain.CreateFuture();
yield return future;
if (future.Error != null)
{
    onError.Invoke(future.Error);
    yield break;
}

var future2 = future.Result.ModelFuture();
yield return future2;
if (future2.Error != null)
{
    onError.Invoke(future2.Error);
    yield break;
}

_account = future2.Result;

onCreateAccount.Invoke(_account);
```

PlayerPrefs 에 새로 생성한 계정 정보를 저장합니다.  

### 계정 삭제

계정 정보를 로컬 스토리지에서 삭제합니다.  
주로 디버그 목적의 기능입니다.  
클라이언트(PlayerPrefs)에 이미 저장된 계정 정보가 있는 상태에서, 그때까지와 다른 프로젝트나  
네임스페이스의 환경에 로그인을 실행한 경우, 또는 GS2-Deploy 에서 계정의 스택을 삭제하고 다시 생성한 경우,  
서버 쪽에 계정 정보가 존재하지 않기 때문에 로그인에 실패합니다.  
그 경우에는 계정 정보를 삭제하고 로그인을 실행하면 새 계정이 생성되어 로그인이 가능해집니다.

### 로그인 처리

UniTask 활성화 시
```c#
GameSession gameSession;
try
{
    gameSession = await _domain.LoginAsync(
        new Gs2AccountAuthenticator(
            accountSetting: new AccountSetting
            {
                accountNamespaceName = accountNamespaceName,
                keyId = accountEncryptionKeyId
            },
            // 서버로부터 앱 내 푸시 알림을 받기 위한 사용자 ID 를 설정
            gatewaySetting: new GatewaySetting
            {
                gatewayNamespaceName = gatewayNamespaceName,
                allowConcurrentAccess = false
            }
        ),
        userId,
        password
    );
}
catch (Gs2Exception e)
{
    onError.Invoke(e, null);
    return;
}

onLogin.Invoke(gameSession);
```
코루틴 사용
```c#
var future = _domain.LoginFuture(
    new Gs2AccountAuthenticator(
        accountSetting: new AccountSetting
        {
            accountNamespaceName = accountNamespaceName,
            keyId = accountEncryptionKeyId
        },
        // 서버로부터 앱 내 푸시 알림을 받기 위한 사용자 ID 를 설정
        gatewaySetting: new GatewaySetting
        {
            gatewayNamespaceName = gatewayNamespaceName,
            allowConcurrentAccess = false
        }
    ),
    userId,
    password
);
yield return future;
if (future.Error != null)
{
    onError.Invoke(future.Error, null);
    yield break;
}

gameSession = future.Result;
```

유틸리티 클래스 Profile 로 로그인을 수행합니다.  
Gs2AccountAuthenticator 라는 인증 클래스에 아래 인자를 전달하고, LoginFuture/LoginAsync 를 호출합니다.  
Profile 은 API 에 접근할 때 액세스 토큰의 기한이 만료되었다는 오류가 반환되면 Gs2AccountAuthenticator 를 사용하여 재인증을 자동으로 시도합니다.
액세스 토큰의 갱신에 성공하면 계속해서 API 에 접근할 수 있습니다.

| 인자 | 설명 |
------|--------------------------------------------------------
| AccountSetting accountSetting | GS2-Account 에서 인증을 수행하기 위한 정보 |
|  string accountNamespaceName |  GS2-Account 의 네임스페이스 이름 |
|  string keyId |  GS2-Account 에서 계정 정보의 암호화에 사용하는 GS2-Key 의 암호 키 GRN |
| GatewaySetting gatewaySetting | 로그인 후에 Gs2Gateway.SetUserId 를 호출하여, 서버로부터 푸시 알림을 받기 위한 사용자 ID 를 설정 |
|  string gatewayNamespaceName |  GS2-Gateway 의 네임스페이스 이름 |
|  string allowConcurrentAccess |  동일한 사용자 ID 로의 다중 로그인을 허용할지 |
| VersionSetting versionSetting | 로그인 후에 Gs2Version.CheckVersion 을 호출하여, 버전 체크를 실행 |
|  string versionNamespaceName | GS2-Version 의 네임스페이스 이름 |
|  EzTargetVersion targetVersions | 게임의 버전 정보 |
| string userId | EzAccount　계정 정보의 사용자 ID |
| string password | EzAccount　계정 정보의 비밀번호 |

액세스 토큰을 보유하는 GameSession 을 받습니다.  
[GS2-Gateway](https://docs.gs2.io/ko/api_reference/gateway/) 에 로그인한 자신의 사용자 ID 를 설정하여, 이 사용자 클라이언트에 대한 푸시 알림을 받을 수 있도록 하고 있습니다.  
채팅([GS2-Chat](Chat_ko.md))의 메시지 게시 알림, 친구 요청([GS2-Friend](Friend_ko.md)) 등의 알림, 매치메이킹([GS2-Matchmaking](Matchmaking_ko.md))의 전환 알림을 받기 위해 사용합니다.


