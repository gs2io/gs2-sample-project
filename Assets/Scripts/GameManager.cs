using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Auth.Model;
using Gs2.Sample.AccountTakeOver;
using Gs2.Sample.Core.Runtime;
using Gs2.Sample.Credential;
using Gs2.Sample.Gacha;
using Gs2.Sample.Gold;
using Gs2.Sample.Inventory;
using Gs2.Sample.Login;
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

        public bool skipCheckVersion = true;
        public bool skipCheckTerm = true;
        [SerializeField] private VersionModel _versionModel;
        [SerializeField] private TermModel _termModel;
        
        [SerializeField] public AccountTakeOverPresenter takeoverPresenter;
        
        [SerializeField] public StaminaPresenter staminaPresenter;
        [SerializeField] public MoneyPresenter moneyPresenter;
        [SerializeField] public GoldPresenter goldPresenter;
        [SerializeField] public InventoryPresenter inventoryPresenter;

        [SerializeField] public QuestPresenter questPresenter;
        [SerializeField] public GachaStorePresenter gachaPresenter;
        [SerializeField] public UnitPresenter unitPresenter;
        
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
        /// シーンの開始時に実行される。
        /// GS2 SDK の初期化を行う。
        ///
        /// 初期化は以下の流れで処理され、コールバックにより初期化の完了を受け取る。
        /// CredentialController::InitializeGs2
        ///  ↓
        /// CredentialSample::OnInitializeGs2
        ///  ↓
        /// this::OnCreateGs2Client
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
        /// </summary>
        public void OnStart()
        {
            UIManager.Instance.SetStateText(gameState.ToString());
            UIManager.Instance.AddLog("GameManager::OnStart");

            InitializeCredential();

            UIManager.Instance.SetStartButtonInteractable(false);
            UIManager.Instance.SetSelectAccountButtonInteractable(false);
            UIManager.Instance.SetRemoveAccountButtonInteractable(false);
            UIManager.Instance.SetActiveTitleProgress(true);
            
            UIManager.Instance.SetTapToStartInteractable(false);
            UIManager.Instance.SetTakeOverInteractable(false);
        }

        /// <summary>
        /// クレデンシャル　初期化
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
        /// クレデンシャル　初期化
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
        /// クレデンシャル　終期化
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
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="onFinalizeGs2"></param>
        /// <returns></returns>
        public static IEnumerator FinalizeGs2(
            Profile profile,
            FinalizeGs2AccountEvent onFinalizeGs2
        )
        {
            Assert.IsNotNull(profile, "profile != null");
            Assert.IsNotNull(onFinalizeGs2, "onFinalizeGs2 != null");
            
            yield return profile.Finalize();

            onFinalizeGs2.Invoke(profile);
        }

        /// <summary>
        /// クレデンシャル　初期化　完了
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
        /// </summary>
        /// <returns></returns>
        public void OnRemoveAccount()
        {
            UIManager.Instance.OpenDialog2("確認", "アカウント情報を削除します。よろしいですか？");
            UIManager.Instance.AddPositiveListner(() =>
            {
                accountRepository.DeleteAccount(_saveSlot);
            });
        }
        
        /// <summary>
        /// アカウント引継ぎ実行
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
        /// アカウントの新規作成・ログインは以下の流れで処理され、コールバックによりログイン結果を受け取る
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
            
            // ゲームプレイヤーアカウントを認証
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
            
            var go = GameObject.Find("GameSession");
            if (go != null)
                Destroy(go);
            
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
                UIManager.Instance.OpenDialog1("お知らせ", "最新のアプリがあります。");
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
                UIManager.Instance.OpenDialog2("利用規約", "「利用規約」への同意が必要です。", "同意する", "同意しない");
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
                Debug.Log("アカウントの認証に失敗したため、アカウントを削除します");
                accountRepository.DeleteAccount();
            }
            else if (e.Errors[0].message == "account.account.account.error.notFound")
            {
                Debug.Log("アカウントが存在しないため、アカウントを削除します");
                accountRepository.DeleteAccount();
            }
        }

        public void OnStartTitle()
        {
            takeoverPresenter.Initialize();
            
            UIManager.Instance.SetStateText(gameState.ToString());
            UIManager.Instance.AddLog("GameManager::OnStartTitle");
            UIManager.Instance.CloseDialog();
            
            UIManager.Instance.SetTapToStartInteractable(true);
            UIManager.Instance.SetTakeOverInteractable(true);
        }

        /// <summary>
        /// ログイン時のゲーム初期化
        /// </summary>
        public void OnTapToStart()
        {
            gameState = GameState.GAME_PLAY;
            UIManager.Instance.SetStateText(gameState.ToString());
            UIManager.Instance.AddLog("OnTapToStart");

            // ゲーム初期化
            UIManager.Instance.SetActiveGameProgress(true);

            StartCoroutine(staminaPresenter.Initialize());
            StartCoroutine(moneyPresenter.Initialize());
            StartCoroutine(goldPresenter.Initialize());

            StartCoroutine(inventoryPresenter.Initialize());

            StartCoroutine(questPresenter.Initialize());

            gachaPresenter.Initialize();
            StartCoroutine(unitPresenter.Initialize());
            
            UIManager.Instance.SetTapToStartInteractable(false);
            UIManager.Instance.SetTakeOverInteractable(false);
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
            UIManager.Instance.SetStateText(gameState.ToString());
            UIManager.Instance.AddLog("GameManager::OnLogout");

            UIManager.Instance.SetActiveGameProgress(false);
            UIManager.Instance.SetTapToStartInteractable(true);
            UIManager.Instance.SetTakeOverInteractable(true);
            UIManager.Instance.SetActiveGame(false);
            UIManager.Instance.CloseProcessing();

            questPresenter.Finish();
            gachaPresenter.Finish();
            
            _realtimeModel.Clear();
        }
        
        /// <summary>
        /// マッチメイキング完了 Realtimeへ
        /// </summary>
        /// <param name="gathering"></param>
        /// <param name="joinPlayerIds"></param>
        public void OnCompleteMatchmaking(EzGathering gathering, List<string> joinPlayerIds)
        {
            _realtimeModel.gatheringId = gathering.Name;
            
            realtimePresenter.Initialize();
        }
    }
}