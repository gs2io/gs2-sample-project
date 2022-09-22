using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Sample.AccountTakeOver;
using Gs2.Sample.Chat;
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
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Assertions;
using InitializeGs2AccountEvent = Gs2.Sample.Credential.InitializeGs2AccountEvent;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

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
        /// Account information storage area
        /// </summary>
        [SerializeField] public PlayerPrefsAccountRepository accountRepository;

        private int _saveSlot = 0;
        
        private Profile _profile;
        public Profile Profile => _profile;
        
        private Gs2Domain _domain;
        public Gs2Domain Domain => _domain;

        private GameSession _session;
        public GameSession Session => _session;

        private EzAccount _account;

        private Coroutine _dispatchCoroutine;

        
        /// <summary>
        /// クレデンシャル 認証情報
        /// Credential
        /// </summary>
        [SerializeField]
        private CredentialSetting _credentialSetting;

        /// <summary>
        /// ログイン
        /// Login
        /// </summary>
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
            
#if GS2_ENABLE_UNITASK
            InitializeGs2Async(
                _credentialSetting.applicationClientId,
                _credentialSetting.applicationClientSecret,
                _credentialSetting.distributorNamespaceName,
                _credentialSetting.onInitializeGs2,
                _credentialSetting.onError
            ).Forget();
#else
            StartCoroutine(
                InitializeGs2(
                    _credentialSetting.applicationClientId,
                    _credentialSetting.applicationClientSecret,
                    _credentialSetting.distributorNamespaceName,
                    _credentialSetting.onInitializeGs2,
                    _credentialSetting.onError
                )
            );
#endif
        }
        
        /// <summary>
        /// GS2 SDK 初期化
        /// GS2 SDK Initialization
        /// </summary>
#if GS2_ENABLE_UNITASK
        private async UniTask InitializeGs2Async(
            string clientId,
            string clientSecret,
            string distributorNamespaceName,
            InitializeGs2AccountEvent onInitializeGs2,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            _profile = new Profile(
                clientId,
                clientSecret,
                new Gs2BasicReopener()
            );

            await _profile.InitializeAsync();
            _domain = new Gs2Domain(_profile, distributorNamespaceName);
            
            onInitializeGs2.Invoke(_domain);
        }
#endif
        private IEnumerator InitializeGs2(
            string clientId,
            string clientSecret,
            string distributorNamespaceName,
            InitializeGs2AccountEvent onInitializeGs2,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            Assert.IsFalse(string.IsNullOrEmpty(clientId), "string.IsNullOrEmpty(clientId)");
            Assert.IsFalse(string.IsNullOrEmpty(clientSecret), "string.IsNullOrEmpty(clientSecret)");
            Assert.IsNotNull(onInitializeGs2, "onInitializeGs2 != null");
            Assert.IsNotNull(onError, "onError != null");
            
            _profile = new Profile(
                clientId,
                clientSecret,
                new Gs2BasicReopener()
            );

            var future = _profile.Initialize();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(
                    future.Error,
                    null
                );
                yield break;
            }
            
            var domain = new Gs2Domain(_profile, distributorNamespaceName);

            onInitializeGs2.Invoke(domain);
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
        private void FinalizeCredential()
        {
            UIManager.Instance.AddLog("FinalizeCredential");
            
#if GS2_ENABLE_UNITASK
            FinalizeGs2Async(
                _profile
            ).Forget();
#else
            StartCoroutine(
                FinalizeGs2(
                    _profile
                )
            );
#endif
        }
        
        /// <summary>
        /// GS2 SDKの終了処理
        /// GS2 SDK Termination
        /// </summary>
#if GS2_ENABLE_UNITASK
        private async UniTask FinalizeGs2Async(
            Profile profile
        )
        {
            if (profile == null)
                return;

            await profile.FinalizeAsync();
        }
#endif
        private IEnumerator FinalizeGs2(
            Profile profile
        )
        {
            if (profile == null)
                yield break;

            yield return profile.Finalize();
        }
        
        /// <summary>
        /// クレデンシャル　初期化　完了
        /// Credential initialization complete
        /// </summary>
        public void OnInitializeGs2(
            Gs2Domain domain
        )
        {
            UIManager.Instance.AddLog("OnInitializeGs2");
            
            _domain = domain;
            
            OnCreateGs2Client();
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
#if GS2_ENABLE_UNITASK
            LoginAsync().Forget();
#else
            StartCoroutine(
                Login()
            );
#endif
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
        public void OnCreateGs2Client()
        {
            UIManager.Instance.AddLog("OnCreateGs2Client");

#if GS2_ENABLE_UNITASK
            LoginAsync().Forget();
#else
            StartCoroutine(
                Login()
            );
#endif
        }

#if GS2_ENABLE_UNITASK
        /// <summary>
        /// ログイン処理
        /// Login
        /// </summary>
        private async UniTask LoginAsync()
        {
            UIManager.Instance.AddLog("Login");

            _loginSetting.onLogin.AddListener(OnLogin);
            _loginSetting.onError.AddListener(OnError);
            
            await AutoLoginAsync(
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
#else
        /// <summary>
        /// ログイン処理
        /// Login
        /// </summary>
        private IEnumerator Login()
        {
            UIManager.Instance.AddLog("Login");

            _loginSetting.onLogin.AddListener(OnLogin);
            _loginSetting.onError.AddListener(OnError);
            
            yield return AutoLogin(
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
#endif
        
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// 自動ログイン処理
        /// 初回起動時等アカウント情報がアカウントリポジトリ（PlayerPrefs）にないときは
        /// GS2-Accountでアカウントを新規作成します。
        /// Automatic login process
        /// When account information is not in the account repository (PlayerPrefs),
        /// such as at first startup Create a new account with GS2-Account.
        /// </summary>
        private async UniTask AutoLoginAsync(
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

            void OnError(Gs2Exception e, Func<IEnumerator> retry)
            {
                error = true;
            }

            onCreateAccount.AddListener(OnCreateAccount);
            onError.AddListener(OnError);

            try
            {
                if (!repository.IsExistsAccount(_saveSlot))
                {
                    await CreateAccountAsync(
                        _domain,
                        accountNamespaceName,
                        onCreateAccount,
                        onError
                    );
                }

                if (error)
                {
                    return;
                }
            
                var account = repository.LoadAccount(_saveSlot);
                await AuthenticationAsync(
                    _domain,
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
        
        private async UniTask CreateAccountAsync(
            Gs2Domain gs2,
            string accountNamespaceName,
            CreateAccountEvent onCreateAccount,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
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
                onError.Invoke(e, null);
                return;
            }

            onCreateAccount.Invoke(_account);
        }
#else
        /// <summary>
        /// 自動ログイン処理
        /// 初回起動時等アカウント情報がアカウントリポジトリ（PlayerPrefs）にないときは
        /// GS2-Accountでアカウントを新規作成します。
        /// Automatic login process
        /// When account information is not in the account repository (PlayerPrefs),
        /// such as at first startup Create a new account with GS2-Account.
        /// </summary>
        private IEnumerator AutoLogin(
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

            void OnError(Gs2Exception e, Func<IEnumerator> retry)
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
                        _domain,
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
                    _domain,
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
        
        private IEnumerator CreateAccount(
            Gs2Domain gs2,
            string accountNamespaceName,
            CreateAccountEvent onCreateAccount,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            var domain = gs2.Account.Namespace(
                namespaceName: accountNamespaceName
            );
            var future = domain.Create();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                yield break;
            }

            var future2 = future.Result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error, null);
                yield break;
            }
            
            _account = future2.Result;

            onCreateAccount.Invoke(_account);
        }
#endif

        public void DeleteAccount(
            IAccountRepository repository
        )
        {
            repository.DeleteAccount(_saveSlot);
        }
        
        /// <summary>
        /// ユーザーの認証
        /// User Authentication
        /// </summary>
        public IEnumerator Authentication(
            Gs2Domain gs2,
            string userId,
            string password,
            string accountNamespaceName,
            string accountEncryptionKeyId,
            LoginEvent onLogin,
            Gs2.Unity.Util.ErrorEvent onError,
            string gatewayNamespaceName
        )
        {
            UIManager.Instance.AddLog("Profile.LoginFuture");

            GameSession gameSession;
            {
                var future = _profile.LoginFuture(
                    new Gs2AccountAuthenticator(
                        _profile.Gs2RestSession,
                        accountNamespaceName,
                        accountEncryptionKeyId,
                        userId,
                        password
                    )
                );
                yield return future;
                if (future.Error != null)
                {
                    onError.Invoke(future.Error, null);
                    yield break;
                }

                gameSession = future.Result;
            }

            UIManager.Instance.AddLog("Gateway.SetUserId");

            // サーバからプッシュ通知を受けるためのユーザーIDを設定
            // Set user ID to receive push notifications from the server
            {
                var domain = gs2.Gateway.Namespace(
                    namespaceName: gatewayNamespaceName
                ).Me(
                    gameSession: gameSession
                ).WebSocketSession();
                var future = domain.SetUserId(
                    allowConcurrentAccess: null
                );
                yield return future;
                if (future.Error != null)
                {
                    onError.Invoke(future.Error, null);
                    yield break;
                }
                var result = future.Result;
                var future2 = result.Model();
                yield return future2;
                if (future2.Error != null)
                {
                    onError.Invoke(future2.Error, null);
                    yield break;
                }
                var item = future2.Result;
                
                onLogin.Invoke(gameSession);
            }
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// ユーザーの認証
        /// User Authentication
        /// </summary>
        private async UniTask AuthenticationAsync(
            Gs2Domain gs2,
            string userId,
            string password,
            string accountNamespaceName,
            string accountEncryptionKeyId,
            LoginEvent onLogin,
            Gs2.Unity.Util.ErrorEvent onError,
            string gatewayNamespaceName
        )
        {
            UIManager.Instance.AddLog("Profile.LoginAsync");
            
            GameSession gameSession;
            try
            {
                gameSession = await _profile.LoginAsync(
                    new Gs2AccountAuthenticator(
                        _profile.Gs2RestSession,
                        accountNamespaceName,
                        accountEncryptionKeyId,
                        userId,
                        password
                    )
                );
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }

            UIManager.Instance.AddLog("Gateway.SetUserId");

            // サーバからプッシュ通知を受けるためのユーザーIDを設定
            // Set user ID to receive push notifications from the server
            
            var domain = gs2.Gateway.Namespace(
                namespaceName: gatewayNamespaceName
            ).Me(
                gameSession: gameSession
            ).WebSocketSession();
            var result = await domain.SetUserIdAsync(
                allowConcurrentAccess: null
            );
            var item = await result.ModelAsync();
            
            onLogin.Invoke(gameSession);
        }
#endif
        
        /// <summary>
        /// ログイン完了
        /// Login Completed
        /// </summary>
        private void OnLogin(GameSession session)
        {
            UIManager.Instance.AddLog("OnLogin");
            
            UIManager.Instance.AddLog("AccessToken.UserId : " + session.AccessToken.UserId);
            UIManager.Instance.AddLog("AccessToken.Token : " + session.AccessToken.Token);
            
            UIManager.Instance.SetAccountText(session.AccessToken.UserId);
            
            _session = session;

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
        
        /// <summary>
        /// バージョンチェック完了
        /// Version check completed
        /// </summary>
        public void OnCheckVersion(string projectToken, List<Gs2.Unity.Gs2Version.Model.EzStatus> warnings, List<Gs2.Unity.Gs2Version.Model.EzStatus> errors)
        {
            UIManager.Instance.AddLog("GameManager::OnCheckVersion");
            
            UIManager.Instance.AddLog("ProjectToken : " + projectToken);
            UIManager.Instance.AddLog("Warnings : " + warnings.Count);
            UIManager.Instance.AddLog("Errors : " + errors.Count);
            
            if (errors.Count > 0)
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

            if (!skipCheckTerm)
            {
                OnRequestTermCheck();
                return;
            }

            OnStartTitle();
        }

        /// <summary>
        /// バージョンチェック リクエスト
        /// Version Check Request
        /// </summary>
        private void OnRequestVersionCheck()
        {
#if GS2_ENABLE_UNITASK
            _versionModel.CheckVersionAsync(
                _domain,
                _session,
                _versionSetting.versionNamespaceName,
                _versionSetting.versionName,
                _versionSetting.currentVersionMajor,
                _versionSetting.currentVersionMinor,
                _versionSetting.currentVersionMicro,
                _versionSetting.onCheckVersion,
                _versionSetting.onError
            ).Forget();
#else
            StartCoroutine(
                _versionModel.CheckVersion(
                    _domain,
                    _session,
                    _versionSetting.versionNamespaceName,
                    _versionSetting.versionName,
                    _versionSetting.currentVersionMajor,
                    _versionSetting.currentVersionMinor,
                    _versionSetting.currentVersionMicro,
                    _versionSetting.onCheckVersion,
                    _versionSetting.onError
                )
            );
#endif
        }

        /// <summary>
        /// 規約確認 リクエスト
        /// Confirmation of Terms and Conditions Request
        /// </summary>
        private void OnRequestTermCheck()
        {
#if GS2_ENABLE_UNITASK
            _termModel.CheckTermAsync(
                _domain,
                _session,
                _termSetting.versionNamespaceName,
                _termSetting.versionName,
                _termSetting.onCheckVersion,
                _termSetting.onError
            ).Forget();
#else
            StartCoroutine(
                _termModel.CheckTerm(
                    _domain,
                    _session,
                    _termSetting.versionNamespaceName,
                    _termSetting.versionName,
                    _termSetting.onCheckVersion,
                    _termSetting.onError
                )
            );
#endif
        }

        /// <summary>
        /// 規約確認 実行
        /// Confirmation of Terms and Conditions Execution
        /// </summary>
        public void OnCheckTerm(string projectToken, List<Gs2.Unity.Gs2Version.Model.EzStatus> warnings, List<Gs2.Unity.Gs2Version.Model.EzStatus> errors)
        {
            UIManager.Instance.AddLog("GameManager::OnCheckTerm");
            
            UIManager.Instance.AddLog("ProjectToken : " + projectToken);
            UIManager.Instance.AddLog("Warnings : " + warnings.Count);
            UIManager.Instance.AddLog("Errors : " + errors.Count);

            var config = new Dictionary<string, string>();

            if (errors.Count > 0)
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
        
        /// <summary>
        /// 規約承認 リクエスト
        /// Agreement Approval Request
        /// </summary>
        private void OnRequestAcceptTerm()
        {
#if GS2_ENABLE_UNITASK
            _termModel.AcceptTermAsync(
                _domain,
                _session,
                _termSetting.versionNamespaceName,
                _termSetting.versionName,
                _termSetting.onAcceptTerm,
                _termSetting.onError
            ).Forget();
#else
            StartCoroutine(
                _termModel.AcceptTerm(
                    _domain,
                    _session,
                    _termSetting.versionNamespaceName,
                    _termSetting.versionName,
                    _termSetting.onAcceptTerm,
                    _termSetting.onError
                )
            );
#endif
        }
        
        /// <summary>
        /// ログイン エラー
        /// Login error
        /// </summary>
        private void OnLoginError(Gs2Exception e, Func<IEnumerator> retry)
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
        /// Game initialization at login
        /// </summary>
        public void OnTapToStart()
        {
            gameState = GameState.GAME_PLAY;
            UIManager.Instance.AddLog("GameState : " + gameState);
            UIManager.Instance.AddLog("OnTapToStart");

            UIManager.Instance.SetActiveGameProgress(true);

#if GS2_ENABLE_UNITASK
            staminaPresenter.InitializeAsync().Forget();
            moneyPresenter.InitializeAsync().Forget();
            goldPresenter.InitializeAsync().Forget();

            inventoryPresenter.InitializeAsync().Forget();
            experiencePresenter.InitializeAsync().Forget();
            
            questPresenter.InitializeAsync().Forget();
            unitPresenter.InitializeAsync().Forget();

            chatPresenter.InitializeAsync().Forget();
#else
            StartCoroutine(staminaPresenter.Initialize());
            StartCoroutine(moneyPresenter.Initialize());
            StartCoroutine(goldPresenter.Initialize());

            StartCoroutine(inventoryPresenter.Initialize());
            StartCoroutine(experiencePresenter.Initialize());

            StartCoroutine(questPresenter.Initialize());
            StartCoroutine(unitPresenter.Initialize());

            StartCoroutine(chatPresenter.Initialize());
#endif

            lotteryStorePresenter.Initialize();

            friendPresenter.Initialize();
            matchmakingPresenter.Initialize();
            realtimePresenter.Initialize();
            
            // JobQueueの自動実行コルーチン
            // JobQueue auto-execute coroutine
#if GS2_ENABLE_UNITASK
            async UniTask Impl()
            {
                while (true)
                {
                    await _domain.DispatchAsync(_session);

                    await UniTask.Yield();
                }
            }
            _dispatchCoroutine = StartCoroutine(Impl().ToCoroutine());
#else
            IEnumerator Impl()
            {
                while (true)
                {
                    var future = _domain.Dispatch(_session);
                    yield return future;
                    if (future != null)
                    {
                        yield break;
                    }
                    if (future.Result)
                    {
                        break;
                    }
                    yield return null;
                }
            }
            _dispatchCoroutine = StartCoroutine(Impl());
#endif
            
            UIManager.Instance.SetTapToStartInteractable(false);
            UIManager.Instance.SetNewsInteractable(false);
            UIManager.Instance.SetFinishButtonInteractable(false);
            UIManager.Instance.SetActiveGame(true);
        }

        /// <summary>
        /// エラー表示
        /// Error indication
        /// </summary>
        public void OnError(Gs2Exception e, Func<IEnumerator> retry)
        {
            string message = String.Empty;
            if (e.Errors.Length > 0)
            {
                foreach (var error in e.Errors)
                {
                    UIManager.Instance.AddLog("Error : " + error.message);
                    message += error.message + "\n";
                }
                UIManager.Instance.OpenDialog1("Error" ,message);
            }
            else
            {
                UIManager.Instance.AddLog("Error : " + e.Message);
                UIManager.Instance.OpenDialog1("Error" ,e.Message);
            }
            Debug.Log(e);
        }

        /// <summary>
        /// ログアウト
        /// Logout
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

            StopCoroutine(_dispatchCoroutine);
        }
        
        /// <summary>
        /// マッチメイキング完了 Realtimeへ
        /// Matchmaking Complete Go to Realtime
        /// </summary>
        public void OnCompleteMatchmaking(string gatheringName)
        {
            _realtimeModel.gatheringName = gatheringName;
            Debug.Log("Gathering Name:" + gatheringName);
            
            realtimePresenter.StartGetRoom();
        }
        
        public void Test()
        {
            _profile.Finalize();
        }
    }
}