using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Auth.Model;
using Gs2.Sample.AccountTakeOver;
using Gs2.Sample.Chat;
using Gs2.Sample.Core.Runtime;
using Gs2.Sample.Credential;
using Gs2.Sample.Experience;
using Gs2.Sample.Friend;
using Gs2.Sample.Lottery;
using Gs2.Sample.Gold;
using Gs2.Sample.Inventory;
using Gs2.Sample.Login;
using Gs2.Sample.Matchmaking;
using Gs2.Sample.Money;
using Gs2.Sample.Quest;
using Gs2.Sample.Realtime;
using Gs2.Sample.Stamina;
using Gs2.Sample.Unit;
using Gs2.Sample.Version;
using Gs2.Unity;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Account.Result;
using Gs2.Unity.Gs2Auth.Result;
using Gs2.Unity.Gs2Gateway.Result;
using Gs2.Unity.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Version.Model;
using Gs2.Unity.Gs2Version.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Assertions;
using FinalizeGs2AccountEvent = Gs2.Sample.Credential.FinalizeGs2AccountEvent;
using InitializeGs2AccountEvent = Gs2.Sample.Credential.InitializeGs2AccountEvent;

namespace Gs2.Sample
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        enum GameState
        {
            START,
            TITLE,
            GAME_PLAY,
        }

        private GameState gameState = GameState.START;

        [Tooltip("アプリ起動時のアプリバージョンチェックをスキップします。")]
        public bool skipCheckVersion = true;
        [Tooltip("アプリ起動時の利用規約チェックをスキップします。")]
        public bool skipCheckTerm = true;
        
        [SerializeField] private VersionModel _versionModel;
        [SerializeField] private TermModel _termModel;

        [SerializeField] public AccountTakeOverPresenter takeoverPresenter;
        
        [SerializeField] public StaminaPresenter staminaPresenter;
        [SerializeField] public MoneyPresenter moneyPresenter;
        [SerializeField] public GoldPresenter goldPresenter;
        [SerializeField] public InventoryPresenter inventoryPresenter;

        [SerializeField] public QuestPresenter questPresenter;
        [SerializeField] public LotteryStorePresenter lotteryStorePresenter;
        [SerializeField] public UnitPresenter unitPresenter;
        [SerializeField] public ExperiencePresenter experiencePresenter;
        
        [SerializeField] public ChatPresenter chatPresenter;
        [SerializeField] public FriendPresenter friendPresenter;
        
        [SerializeField] public MatchmakingPresenter matchmakingPresenter;
        [SerializeField] public RealtimePresenter realtimePresenter;
        
        /// <summary>
        /// アカウント情報の保存領域
        /// </summary>
        [SerializeField] public PlayerPrefsAccountRepository accountRepository;

        private int _saveSlot = 0;
        
        private Gs2Client _client = new Gs2Client();
        public Gs2Client Cllient => _client;
        
        private Gs2GameSession _session = new Gs2GameSession();
        public Gs2GameSession Session => _session;
    
        // Credential
        [SerializeField]
        private CredentialSetting _credentialSetting;

        // Login
        [SerializeField]
        private LoginSetting _loginSetting;

        [SerializeField]
        private VersionSetting _versionSetting;
        
        [SerializeField]
        private TermSetting _termSetting;
        
        // Stamina
        [SerializeField]
        private StaminaSetting _staminaSetting;
        private StaminaModel _staminaModel;
        
        // Money
        [SerializeField]
        private MoneySetting _moneySetting;
        private MoneyModel _moneyModel;
        
        // Gold
        [SerializeField]
        private GoldSetting _goldSetting;
        private GoldModel _goldModel;

        // Realtime
        private RealtimeModel _realtimeModel;

        /// <summary>
        /// GS2 SDK の初期化
        /// Initialize GS2 SDK.
        /// </summary>
        public void Start()
        {
            Assert.IsNotNull(_credentialSetting);
            
            Assert.IsNotNull(_loginSetting);
            
            Assert.IsNotNull(_staminaSetting);
            _staminaModel = GetComponent<StaminaModel>();
            Assert.IsNotNull(_staminaModel);
            
            Assert.IsNotNull(_moneySetting);
            _moneyModel = GetComponent<MoneyModel>();
            Assert.IsNotNull(_moneyModel);
            
            Assert.IsNotNull(_goldSetting);
            _goldModel = GetComponent<GoldModel>();
            Assert.IsNotNull(_goldModel);

            _realtimeModel = GetComponent<RealtimeModel>();
            Assert.IsNotNull(_realtimeModel);
            
            UIManager.Instance.SetActiveGame(false);
        }

        /// <summary>
        /// アプリケーション起動
        /// Application Launch
        /// </summary>
        public void OnStart()
        {
            UIManager.Instance.AddLog("GameState : " + gameState);
            UIManager.Instance.AddLog("GameManager::OnStart");

            InitializeCredential();

            UIManager.Instance.SetStartButtonInteractable(false);
            UIManager.Instance.SetSelectAccountButtonInteractable(false);
            UIManager.Instance.SetRemoveAccountButtonInteractable(false);
            UIManager.Instance.SetActiveTitleProgress(true);
            
            UIManager.Instance.SetTapToStartInteractable(false);
            UIManager.Instance.SetTakeOverInteractable(false);
            UIManager.Instance.SetNewsInteractable(false);
            UIManager.Instance.SetFinishButtonInteractable(false);
        }

        /// <summary>
        /// クレデンシャルの初期化
        /// Initialize credentials
        /// </summary>
        public void InitializeCredential()
        {
            UIManager.Instance.AddLog("InitializeCredential");
            StartCoroutine(
                InitializeGs2(
                    _credentialSetting.applicationClientId,
                    _credentialSetting.applicationClientSecret,
                    _credentialSetting.onInitializeGs2,
                    _credentialSetting.onError
                )
            );
        }
        
        /// <summary>
        /// クレデンシャルの初期化
        /// Initialize credentials
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="onInitializeGs2"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static IEnumerator InitializeGs2(
            string clientId,
            string clientSecret,
            InitializeGs2AccountEvent onInitializeGs2,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            Assert.IsFalse(string.IsNullOrEmpty(clientId), "string.IsNullOrEmpty(clientId)");
            Assert.IsFalse(string.IsNullOrEmpty(clientSecret), "string.IsNullOrEmpty(clientSecret)");
            Assert.IsNotNull(onInitializeGs2, "onInitializeGs2 != null");
            Assert.IsNotNull(onError, "onError != null");
            
            var profile = new Profile(
                clientId,
                clientSecret,
                new Gs2BasicReopener()
            );
            
            AsyncResult<object> result = null;
            yield return profile.Initialize(
                r => { result = r; }
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            var client = new Client(profile);

            onInitializeGs2.Invoke(profile, client);
        }

        /// <summary>
        /// 想定上のアプリケーション終了
        /// Application termination on assumption
        /// </summary>
        public void OnFinish()
        {
            UIManager.Instance.AddLog("GameManager::OnFinish");

            FinalizeCredential();

            gameState = GameState.START;
            
            UIManager.Instance.SetStartButtonInteractable(true);
            UIManager.Instance.SetSelectAccountButtonInteractable(true);
            UIManager.Instance.SetRemoveAccountButtonInteractable(true);
            UIManager.Instance.SetActiveTitleProgress(false);
            
            UIManager.Instance.SetTapToStartInteractable(false);
            UIManager.Instance.SetTakeOverInteractable(false);
            UIManager.Instance.SetNewsInteractable(false);
            UIManager.Instance.SetFinishButtonInteractable(false);
        }
        
        /// <summary>
        /// クレデンシャル　終期化
        /// Credential Termination
        /// </summary>
        public void FinalizeCredential()
        {
            UIManager.Instance.AddLog("FinalizeCredential");
            StartCoroutine(
                FinalizeGs2(
                    _client.Profile,
                    _credentialSetting.onFinalizeGs2
                )
            );
        }
        
        /// <summary>
        /// クレデンシャル　終期化
        /// Credential Termination
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="onFinalizeGs2"></param>
        /// <returns></returns>
        public static IEnumerator FinalizeGs2(
            Profile profile,
            FinalizeGs2AccountEvent onFinalizeGs2
        )
        {
            if (profile == null)
                yield break;

            yield return profile.Finalize();

            if (onFinalizeGs2 != null)
                onFinalizeGs2.Invoke(profile);

            yield return null;
        }

        /// <summary>
        /// クレデンシャル　初期化　完了
        /// Credential initialization complete
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="client"></param>
        public void OnInitializeGs2(
            Profile profile,
            Client client
        )
        {
            UIManager.Instance.AddLog("OnInitializeGs2");
            
            _client.Client = client;
            _client.Profile = profile;
            
            OnCreateGs2Client(_client);
        }
        
        /// <summary>
        /// アカウント選択処理
        /// account selection process
        /// </summary>
        /// <returns></returns>
        public void OnSelectAccount(int slot)
        {
            switch (slot)
            {
                default:
                    UIManager.Instance.SetSaveSlotText("default");
                    _saveSlot = 0;
                    break;
                case 1:
                case 2:
                case 3:
                    UIManager.Instance.SetSaveSlotText("Slot " + slot.ToString());
                    _saveSlot = slot;
                    break;
            }
        }

        /// <summary>
        /// アカウント削除処理
        /// account deletion process
        /// </summary>
        /// <returns></returns>
        public void OnRemoveAccount()
        {
            switch (UIManager.Instance.Lang)
            {
                case  UIManager.Language.ja:
                    UIManager.Instance.OpenDialog2("確認", "アカウント情報を削除します。よろしいですか？");
                    break;
                case  UIManager.Language.en:
                    UIManager.Instance.OpenDialog2("Confirm", "Delete account information. Are you sure?");
                    break;
            }

            UIManager.Instance.AddPositiveListner(() =>
            {
                accountRepository.DeleteAccount(_saveSlot);
            });
        }
        
        /// <summary>
        /// アカウント引継ぎ実行
        /// Account takeover execution
        /// </summary>
        /// <returns></returns>
        public void OnDoTakeOverAccount(EzAccount account)
        {
            accountRepository.SaveAccount(
                new PersistAccount
                {
                    UserId = account.UserId,
                    Password = account.Password,
                },
                _saveSlot
            );
        }

        /// <summary>
        /// 再ログイン
        /// Re-login
        /// </summary>
        /// <returns></returns>
        public void ReLogin()
        {
            StartCoroutine(
                Login()
            );
        }
        
        /// <summary>
        /// GS2 SDK の初期化が完了し、GS2 Client の取得が終わったときに呼び出される。
        /// 受け取った GS2 Client を使用して、アカウントの新規作成・ログインを実行する。
        ///
        /// アカウントの新規作成・ログインは以下の流れで処理され、コールバックによりログイン結果を受け取る。
        ///
        /// Called when GS2 SDK initialization is complete and GS2 Client acquisition is finished.
        /// Create a new account and login using the received GS2 Client.
        ///
        /// The creation of a new account and login is processed as follows, and the login result is received by callback.
        /// 
        /// Login()
        ///  ↓
        /// AutoLogin()
        ///  ↓
        /// Authentication()
        /// </summary>
        /// <param name="client"></param>
        public void OnCreateGs2Client(Gs2Client client)
        {
            UIManager.Instance.AddLog("OnCreateGs2Client");

            _client = client;

            StartCoroutine(
                Login()
            );
        }

        /// <summary>
        /// ログイン処理
        /// Login
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="client"></param>
        public IEnumerator Login()
        {
            UIManager.Instance.AddLog("Login");

            _loginSetting.onLogin.AddListener(OnLogin);
            _loginSetting.onError.AddListener(OnError);
            
            yield return AutoLogin(
                _client.Client,
                accountRepository,
                _loginSetting.accountNamespaceName,
                _loginSetting.accountEncryptionKeyId,
                _loginSetting.onCreateAccount,
                _loginSetting.onLogin,
                _loginSetting.onError,
                _loginSetting.gatewayNamespaceName
            );
            
            _loginSetting.onLogin.RemoveListener(OnLogin);
            _loginSetting.onError.RemoveListener(OnLoginError);
        }
        
        /// <summary>
        /// 自動ログイン処理
        /// 初回起動時等アカウント情報がアカウントリポジトリ（PlayerPrefs）にないときは
        /// GS2-Accountでアカウントを新規作成します。
        /// Automatic login process
        /// When account information is not in the account repository (PlayerPrefs),
        /// such as at first startup Create a new account with GS2-Account.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="repository"></param>
        /// <param name="accountNamespaceName"></param>
        /// <param name="accountEncryptionKeyId"></param>
        /// <param name="onCreateAccount"></param>
        /// <param name="onLogin"></param>
        /// <param name="onError"></param>
        /// <param name="gatewayNamespaceName"></param>
        /// <returns></returns>
        public IEnumerator AutoLogin(
            Client client,
            IAccountRepository repository,
            string accountNamespaceName,
            string accountEncryptionKeyId,
            CreateAccountEvent onCreateAccount,
            LoginEvent onLogin,
            Gs2.Unity.Util.ErrorEvent onError,
            string gatewayNamespaceName
        )
        {
            var error = false;
            
            void OnCreateAccount(EzAccount account)
            {
                repository.SaveAccount(
                    new PersistAccount
                    {
                        UserId = account.UserId,
                        Password = account.Password,
                    },
                    _saveSlot
                );
            }

            void OnError(Gs2Exception e)
            {
                error = true;
            }

            onCreateAccount.AddListener(OnCreateAccount);
            onError.AddListener(OnError);

            try
            {
                if (!repository.IsExistsAccount(_saveSlot))
                {
                    yield return CreateAccount(
                        client,
                        accountNamespaceName,
                        onCreateAccount,
                        onError
                    );
                }

                if (error)
                {
                    yield break;
                }
            
                var account = repository.LoadAccount(_saveSlot);
                yield return Authentication(
                    client,
                    account?.UserId,
                    account?.Password,
                    accountNamespaceName,
                    accountEncryptionKeyId,
                    onLogin,
                    onError,
                    gatewayNamespaceName
                );
            }
            finally
            {
                onError.RemoveListener(OnError);
                onCreateAccount.RemoveListener(OnCreateAccount);
            }
        }
        
        public IEnumerator CreateAccount(
            Client client,
            string accountNamespaceName,
            CreateAccountEvent onCreateAccount,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            AsyncResult<EzCreateResult> result = null;
            yield return client.Account.Create(
                r =>
                {
                    result = r;
                },
                accountNamespaceName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            var account = result.Result.Item;

            onCreateAccount.Invoke(account);
        }
        
        public void DeleteAccount(
            IAccountRepository repository
        )
        {
            repository.DeleteAccount(_saveSlot);
        }
        
        public static IEnumerator Authentication(
            Client client,
            string userId,
            string password,
            string accountNamespaceName,
            string accountEncryptionKeyId,
            LoginEvent onLogin,
            Gs2.Unity.Util.ErrorEvent onError,
            string gatewayNamespaceName
        )
        {
            UIManager.Instance.AddLog("Authentication");
            
            // ゲームプレイヤーの認証
            // Game Player Authentication
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
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            var account = result.Result.Item;

            UIManager.Instance.AddLog("Auth.Login");
            
            // 指定したユーザーIDでGS2にログイン
            // Log in to GS2 with the specified user ID
            AsyncResult<EzLoginResult> result2 = null;
            yield return client.Auth.Login(
                r =>
                {
                    result2 = r;
                },
                userId,
                accountEncryptionKeyId,
                result.Result.Body,
                result.Result.Signature
            );

            var session = new GameSession(
                new AccessToken()
                    .WithToken(result2.Result.Token)
                    .WithExpire(result2.Result.Expire)
                    .WithUserId(result2.Result.UserId)
            );

            UIManager.Instance.AddLog("Gateway.SetUserId");

			// サーバからプッシュ通知を受けるためのユーザーIDを設定
            // Set user ID to receive push notifications from the server
            AsyncResult<EzSetUserIdResult> result3 = null;
            yield return client.Gateway.SetUserId(
                r => { result3 = r; },
                session,
                gatewayNamespaceName,
                true
            );
            
            if (result3.Error != null)
            {
                onError.Invoke(
                    result3.Error
                );
                yield break;
            }
            
            onLogin.Invoke(account, session);
        }

        public void OnLogin(EzAccount account, GameSession session)
        {
            UIManager.Instance.AddLog("OnLogin");
            UIManager.Instance.AddLog("session.userId : " + session.AccessToken.UserId);
            UIManager.Instance.AddLog("session.token : " + session.AccessToken.Token);
            UIManager.Instance.AddLog("session.AccessToken : " + session.AccessToken.Expire);
            
            UIManager.Instance.SetAccountText(session.AccessToken.UserId);
            
            _session.Session = session;

            if (!skipCheckVersion)
            {
                OnRequestVersionCheck();
                return;
            }
            else if (!skipCheckTerm)
            {
                OnRequestTermCheck();
                return;
            }

            OnStartTitle();
        }
        
        public void OnCheckVersion(List<EzTargetVersion> target, EzCheckVersionResult result)
        {
            UIManager.Instance.AddLog("GameManager::OnCheckVersion");
            UIManager.Instance.AddLog("Errors : " + result.Errors.Count);
            UIManager.Instance.AddLog("Warnings : " + result.Warnings.Count);
            UIManager.Instance.AddLog("ProjectToken : " + result.ProjectToken);

            if (result.Errors.Count > 0)
            {
                switch (UIManager.Instance.Lang)
                {
                    case UIManager.Language.ja:
                        UIManager.Instance.OpenDialog1("Notice", "最新のアプリがあります。");
                        break;
                    case  UIManager.Language.en:
                        UIManager.Instance.OpenDialog1("Notice", "New Update is Available");
                        break;
                }
                
                UIManager.Instance.AddAcceptListner(OnRequestVersionCheck);
                return;
            }

            var config = new Dictionary<string, string>();

            if (!skipCheckTerm)
            {
                OnRequestTermCheck();
                return;
            }

            OnStartTitle();
        }

        private void OnRequestVersionCheck()
        {
            StartCoroutine(
                _versionModel.CheckVersion(
                    _client.Client,
                    _session.Session,
                    _versionSetting.versionNamespaceName,
                    _versionSetting.versionName,
                    _versionSetting.currentVersionMajor,
                    _versionSetting.currentVersionMinor,
                    _versionSetting.currentVersionMicro,
                    _versionSetting.onCheckVersion,
                    _versionSetting.onError
                )
            );
        }

        private void OnRequestTermCheck()
        {
            StartCoroutine(
                _termModel.CheckTerm(
                    _client.Client,
                    _session.Session,
                    _termSetting.versionNamespaceName,
                    _termSetting.versionName,
                    _termSetting.onCheckVersion,
                    _termSetting.onError
                )
            );
        }
        
        public void OnCheckTerm(List<EzTargetVersion> target, EzCheckVersionResult result)
        {
            UIManager.Instance.AddLog("GameManager::OnCheckTerm");
            UIManager.Instance.AddLog("Errors : " + result.Errors.Count);
            UIManager.Instance.AddLog("Warnings : " + result.Warnings.Count);
            UIManager.Instance.AddLog("ProjectToken : " + result.ProjectToken);

            var config = new Dictionary<string, string>();

            if (result.Errors.Count > 0)
            {
                switch (UIManager.Instance.Lang)
                {
                    case  UIManager.Language.ja:
                        UIManager.Instance.OpenDialog2("利用規約", "「利用規約」への同意が必要です。", "同意する", "同意しない");
                        break;
                    case  UIManager.Language.en:
                        UIManager.Instance.OpenDialog2("Terms and Conditions", "You must agree to the Terms and Conditions.", "I Agree", "I Don't Agree ");
                        break;
                }
                UIManager.Instance.AddPositiveListner(OnRequestAcceptTerm);
                UIManager.Instance.AddNegativeListner(OnRequestVersionCheck);
            }
            else
            {
                OnStartTitle();
            }
        }
        
        private void OnRequestAcceptTerm()
        {
            StartCoroutine(
                _termModel.AcceptTerm(
                    _client.Client,
                    _session.Session,
                    _termSetting.versionNamespaceName,
                    _termSetting.versionName,
                    _termSetting.onAcceptTerm,
                    _termSetting.onError
                )
            );
        }
        
        public void OnLoginError(Gs2Exception e)
        {
            if (e.Errors[0].message == "account.account.account.error.notAuthorized")
            {
                switch (UIManager.Instance.Lang)
                {
                    case  UIManager.Language.ja:
                        Debug.Log("アカウントの認証に失敗したため、アカウントを削除します。");
                        break;
                    case  UIManager.Language.en:
                        Debug.Log("Delete account due to account authorization failure.");
                        break;
                }
                accountRepository.DeleteAccount();
            }
            else if (e.Errors[0].message == "account.account.account.error.notFound")
            {
                switch (UIManager.Instance.Lang)
                {
                    case UIManager.Language.ja:
                        Debug.Log("アカウントの認証に失敗したため、アカウントを削除します。");
                        break;
                    case UIManager.Language.en:
                        Debug.Log("Delete account due to account authorization failure.");
                        break;
                }
                accountRepository.DeleteAccount();
            }
        }

        public void OnStartTitle()
        {
            takeoverPresenter.Initialize();
            
            UIManager.Instance.AddLog("GameState : " + gameState);
            UIManager.Instance.AddLog("GameManager::OnStartTitle");
            UIManager.Instance.CloseDialog();
            
            UIManager.Instance.SetTapToStartInteractable(true);
            UIManager.Instance.SetTakeOverInteractable(true);
            UIManager.Instance.SetNewsInteractable(true);
            UIManager.Instance.SetFinishButtonInteractable(true);
        }

        /// <summary>
        /// ログイン時のゲーム初期化
        /// </summary>
        public void OnTapToStart()
        {
            gameState = GameState.GAME_PLAY;
            UIManager.Instance.AddLog("GameState : " + gameState);
            UIManager.Instance.AddLog("OnTapToStart");

            // ゲーム初期化
            UIManager.Instance.SetActiveGameProgress(true);

            StartCoroutine(staminaPresenter.Initialize());
            StartCoroutine(moneyPresenter.Initialize());
            StartCoroutine(goldPresenter.Initialize());

            StartCoroutine(inventoryPresenter.Initialize());
            StartCoroutine(experiencePresenter.Initialize());

            StartCoroutine(questPresenter.Initialize());
            lotteryStorePresenter.Initialize();
            StartCoroutine(unitPresenter.Initialize());
            
            StartCoroutine(chatPresenter.Initialize());
            friendPresenter.Initialize();
            
            matchmakingPresenter.Initialize();
            realtimePresenter.Initialize();
            
            UIManager.Instance.SetTapToStartInteractable(false);
            UIManager.Instance.SetNewsInteractable(false);
            UIManager.Instance.SetFinishButtonInteractable(false);
            UIManager.Instance.SetActiveGame(true);
        }

        /// <summary>
        /// エラー表示
        /// </summary>
        /// <param name="e"></param>
        public void OnError(Gs2Exception e)
        {
            UIManager.Instance.AddLog("Error : " + e.Message);
            UIManager.Instance.OpenDialog1("Error" ,e.Message);
            Debug.Log(e);
        }

        /// <summary>
        /// ログアウト
        /// </summary>
        public void OnLogout()
        {
            gameState = GameState.TITLE;
            
            UIManager.Instance.AddLog("GameState : " + gameState);
            UIManager.Instance.AddLog("GameManager::OnLogout");

            UIManager.Instance.SetActiveGameProgress(false);
            UIManager.Instance.SetTapToStartInteractable(true);
            UIManager.Instance.SetTakeOverInteractable(true);
            UIManager.Instance.SetNewsInteractable(true);
            UIManager.Instance.SetFinishButtonInteractable(true);
            UIManager.Instance.SetActiveGame(false);
            UIManager.Instance.CloseProcessing();

            questPresenter.Finish();
            lotteryStorePresenter.Finish();
            
            chatPresenter.Finish();
            friendPresenter.Finish();
            
            matchmakingPresenter.Finish();
            realtimePresenter.Finish();
            _realtimeModel.Clear();
        }
        
        /// <summary>
        /// マッチメイキング完了 Realtimeへ
        /// </summary>
        /// <param name="gathering"></param>
        /// <param name="joinPlayerIds"></param>
        public void OnCompleteMatchmaking(EzGathering gathering, List<string> joinPlayerIds)
        {
            _realtimeModel.gatheringName = gathering.Name;
            
            realtimePresenter.StartGetRoom();
        }
    }
}