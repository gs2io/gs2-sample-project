using System.Collections;
using Gs2.Sample.Login;
using UnityEngine;
using UnityEngine.Assertions;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.AccountTakeOver
{
    public class AccountTakeOverPresenter : MonoBehaviour
    {
        /// <summary>
        /// GS2-Account の設定値
        /// GS2-Account setting value
        /// </summary>
        [SerializeField] private LoginSetting _loginSetting;

        [SerializeField] private AccountTakeOverSetting _takeOverSetting;

        [SerializeField] private AccountTakeOverModel _accountTakeOverModel;
        [SerializeField] private AccountTakeOverView _view;

        public enum State
        {
            Initialize,

            Title,
            
            MainMenu,

            /// <summary>
            /// アカウント連携情報を取得中
            /// Account linkage information is being obtained.
            /// </summary>
            GetTakeOverSettingsProcessing,
            /// <summary>
            /// アカウント連携情報の取得に失敗
            /// Failure to obtain account linkage information
            /// </summary>
            GetTakeOverSettingsFailed,

            /// <summary>
            /// アカウント連携 方式メニュー
            /// Account Linking Method Menu
            /// </summary>
            Setting_SelectTypeMenu,
            
            /// <summary>
            /// アカウント連携 Email設定
            /// Account Linking Email Setting
            /// </summary>
            SettingEmail,
            /// <summary>
            /// Emailの設定をキャンセル
            /// Cancel Email setting
            /// </summary>
            CancelSettingEmail,
            
            /// <summary>
            /// Platformによるアカウント連携を保存/削除
            /// Save/delete account linkage by Platform
            /// </summary>
            SettingPlatform,
            /// <summary>
            /// Platformによるアカウント連携を保存/削除に失敗
            /// Failed to save/delete account linkage by Platform
            /// </summary>
            SettingPlatformError,
            
            /// <summary>
            /// 引継ぎ設定の登録を実行中
            /// Running registration of takeover setting
            /// </summary>
            AddSettingProcessing,
            /// <summary>
            /// 引継ぎ設定の登録に成功
            /// Successfully registering takeover setting
            /// </summary>
            AddSettingSucceed,
            /// <summary>
            /// 引継ぎ設定の登録に失敗
            /// Failed to register takeover setting
            /// </summary>
            AddSettingFailed,

            /// <summary>
            /// 引継ぎ設定の削除中
            /// Deleting takeover setting
            /// </summary>
            DeleteSettingProcessing,
            /// <summary>
            /// 引継ぎ設定の削除に成功
            /// Deletion of takeover setting succeeded
            /// </summary>
            DeleteSettingSucceed,
            /// <summary>
            /// 引継ぎ設定の削除に失敗
            /// Failed to delete takeover setting
            /// </summary>
            DeleteSettingFailed,
            
            /// <summary>
            /// 引継ぎ 方式メニュー
            /// Takeover method menu
            /// </summary>
            TakeOver_SelectTypeMenu,
            
            /// <summary>
            /// 引継ぎ Email設定
            /// Takeover Email setting
            /// </summary>
            TakeOverEmail,
            /// <summary>
            /// Emailの設定をキャンセル
            /// Cancel Email settings
            /// </summary>
            CancelTakeOverEmail,
            
            /// <summary>
            /// 引継ぎ実行中
            /// Takeover in progress
            /// </summary>
            TakeOver_Processing,
            /// <summary>
            /// 引継ぎの実行を完了
            /// Complete the transfer execution.
            /// </summary>
            TakeOverCompleted,
            /// <summary>
            /// 引継ぎの実行に失敗
            /// Failure to execute handover
            /// </summary>
            TakeOverFailed,

            Error,
        }

        private State takeOverState = State.Initialize;

        private void Start()
        {
            Assert.IsNotNull(_loginSetting);
            Assert.IsNotNull(_takeOverSetting);
            Assert.IsNotNull(_accountTakeOverModel);
        }

        private void SetState(State _state)
        {
            if (takeOverState != _state)
            {
                switch (_state)
                {
                    default:
                        UIManager.Instance.CloseProcessing();
                        _view.OnCloseEvent();
                        break;
                    
                    case State.Title:
                        UIManager.Instance.CloseProcessing();
                        _view.OnCloseEvent();
                        break;

                    case State.MainMenu:
                        UIManager.Instance.CloseProcessing();
                        _view.OnCloseSettingTypeEvent();
                        _view.OnCloseTakeOverTypeEvent();
                        _view.OnOpenMainMenuEvent();
                        break;
                        
                    case State.GetTakeOverSettingsProcessing:
                    case State.AddSettingProcessing:
                    case State.DeleteSettingProcessing:
                    case State.TakeOver_Processing:
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    case State.Setting_SelectTypeMenu:
                        UIManager.Instance.CloseProcessing();

                        if (_accountTakeOverModel.GetEmailSetting() != null)
                        {
                            _view.emailTakeOverButtonLabel.SetText("Email - Registered");
                        }
                        else
                        {
                            _view.emailTakeOverButtonLabel.SetText("Email");
                        }

                        if (_accountTakeOverModel.GetPlatformSetting() != null)
                        {
#if UNITY_ANDROID
                            _view.platformTakeOverButtonLabel.SetText("Google Play - Registered");
#else
                            _view.platformTakeOverButtonLabel.SetText("Game Center - Registered");
#endif
                        }
                        else
                        {
#if UNITY_ANDROID
                            _view.platformTakeOverButtonLabel.SetText("Google Play");
#else
                            _view.platformTakeOverButtonLabel.SetText("Game Center");

#endif
                        }

                        _view.OnOpenSettingTypeEvent();
                        break;

                    case State.SettingEmail:
                        _view.OnCloseSettingTypeEvent();;
                        _view.OnOpenSettingEmailEvent();
                        break;
                    case State.CancelSettingEmail:
                        _view.OnCloseSettingEmailEvent();
                        _view.OnOpenSettingTypeEvent();;
                        break;
                    
                    case State.SettingPlatform:
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    case State.AddSettingSucceed:
                        UIManager.Instance.CloseProcessing();
                        _view.OnCloseSettingTypeEvent();
                        _view.OnCloseSettingEmailEvent();
                        UIManager.Instance.OpenDialog1("Notice","LinkAdd");
                        UIManager.Instance.AddAcceptListner(OnClickSettingSelectType);
                        break;
                    case State.AddSettingFailed:
                        UIManager.Instance.CloseProcessing();
                        _view.OnCloseSettingEmailEvent();
                        _view.OnOpenSettingTypeEvent();
                        break;
                    
                    case State.DeleteSettingSucceed:
                        UIManager.Instance.CloseProcessing();
                        UIManager.Instance.OpenDialog1("Notice","LinkRemove");
                        UIManager.Instance.AddAcceptListner(OnClickSettingSelectType);
                        break;
                    case State.DeleteSettingFailed:
                        UIManager.Instance.CloseProcessing();
                        _view.OnOpenSettingTypeEvent();
                        break;
                    
                    case State.TakeOver_SelectTypeMenu:
                        _view.OnOpenTakeOverTypeEvent();
                        break;
                    
                    case State.TakeOverEmail:
                        _view.OnCloseTakeOverTypeEvent();
                        _view.OnOpenTakeOverEmailEvent();
                        break;
                    case State.CancelTakeOverEmail:
                        _view.OnCloseTakeOverEmailEvent();
                        _view.OnOpenTakeOverTypeEvent();
                        break;
                    
                    case State.TakeOverCompleted:
                        UIManager.Instance.CloseProcessing();
                        _view.OnCloseEvent();
                        _view.OnCloseSettingTypeEvent();
                        _view.OnCloseSettingEmailEvent();
                        _view.OnCloseTakeOverTypeEvent();
                        _view.OnCloseTakeOverEmailEvent();
                        UIManager.Instance.OpenDialog1("Notice","TakeOver");
                        UIManager.Instance.AddAcceptListner(OnClickConfirmTakeOver);
                        break;
                    case State.TakeOverFailed:
                        UIManager.Instance.CloseProcessing();
                        _view.OnOpenSettingTypeEvent();
                        break;
                }
            }
        }

        /// <summary>
        /// 初期化処理
        /// initialization process
        /// </summary>
        public void Initialize()
        {
            if (!Social.localUser.authenticated)
            {
                Social.localUser.Authenticate(success => { Debug.Log("signed-in"); });
            }
        }

        /// <summary>
        /// 現在登録されている引継ぎ情報を取得
        /// Retrieve currently registered transfer information
        /// </summary>
        private IEnumerator GetTakeOverSettings()
        {
            yield return _accountTakeOverModel.ListAccountTakeOverSettings(
                e =>
                {
                    SetState(e == null
                        ? State.Setting_SelectTypeMenu
                        : State.GetTakeOverSettingsFailed);
                },
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask GetTakeOverSettingsAsync()
        {
            var err = await _accountTakeOverModel.ListAccountTakeOverSettingsAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onError
            );
            
            if (err == null)
                SetState(State.Setting_SelectTypeMenu);
            else
                SetState(State.GetTakeOverSettingsFailed);
        }
#endif

        /// <summary>
        /// 引継ぎ設定の登録
        /// Registering takeover settings
        /// </summary>
        private IEnumerator AddTakeOverSetting()
        {
            yield return _accountTakeOverModel.AddAccountTakeOverSetting(
                e =>
                {
                    SetState(e == null
                        ? State.AddSettingSucceed
                        : State.AddSettingFailed);
                },
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onSetTakeOver,
                _takeOverSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask AddTakeOverSettingAsync()
        {
            var err = await _accountTakeOverModel.AddAccountTakeOverSettingAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onSetTakeOver,
                _takeOverSetting.onError
            );
            
            if (err == null)
                SetState(State.AddSettingSucceed);
            else
                SetState(State.AddSettingFailed);
        }
#endif
        
        /// <summary>
        /// 引継ぎを実行
        /// Execute takeover
        /// </summary>
        public void ExecTakeOver()
        {
            SetState(State.TakeOver_Processing);
#if GS2_ENABLE_UNITASK
            DoTakeOverAsync().Forget();
#else
            StartCoroutine(
                DoTakeOver()
            );
#endif
        }
        
        /// <summary>
        /// 引継ぎを実行
        /// Execute takeover
        /// </summary>
        private IEnumerator DoTakeOver()
        {
            yield return _accountTakeOverModel.DoAccountTakeOver(
                e =>
                {
                    if (e == null)
                    {
                        SetState(State.TakeOverCompleted);
                    }
                    else
                    {
                        SetState(State.TakeOverFailed);
                    }
                },
                GameManager.Instance.Domain,                
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onDoTakeOver,
                _takeOverSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask DoTakeOverAsync()
        {
            var err = await _accountTakeOverModel.DoAccountTakeOverAsync(
                GameManager.Instance.Domain,                
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onDoTakeOver,
                _takeOverSetting.onError
            );
            
            if (err == null)
                SetState(State.TakeOverCompleted);
            else
                SetState(State.TakeOverFailed);
        }
#endif

        /// <summary>
        /// 引継ぎ設定を削除
        /// Delete takeover setting
        /// </summary>
        public void DeleteSetting()
        {
            SetState(State.DeleteSettingProcessing);
#if GS2_ENABLE_UNITASK
            DeleteTakeOverSettingAsync().Forget();
#else
            StartCoroutine(
                DeleteTakeOverSetting()
            );
#endif
        }
            
        /// <summary>
        /// 引継ぎ設定の削除を実行
        /// Delete takeover settings
        /// </summary>
        private IEnumerator DeleteTakeOverSetting()
        {
            yield return _accountTakeOverModel.DeleteAccountTakeOverSetting(
                e =>
                {
                    SetState(e == null
                        ? State.DeleteSettingSucceed
                        : State.DeleteSettingFailed);
                },
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask DeleteTakeOverSettingAsync()
        {
            var err = await _accountTakeOverModel.DeleteAccountTakeOverSettingAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onError
            );
            
            if (err == null)
                SetState(State.DeleteSettingSucceed);
            else
                SetState(State.DeleteSettingFailed);
        }
#endif
        
        /// <summary>
        /// メインメニューを開く
        /// Open Main Menu
        /// </summary>
        public void OnClickMainMenu()
        {
            SetState(State.MainMenu);
        }
        
        /// <summary>
        /// アカウント連携を選択した
        /// Selected account linkage
        /// </summary>
        public void OnClickSetting()
        {
            SetState(State.Setting_SelectTypeMenu);
        }
        
        /// <summary>
        /// アカウント連携を選択
        /// 連携方式を選択へ
        /// Select Account Linkage
        /// Go to Select Linkage Method
        /// </summary>
        public void OnClickSettingSelectType()
        {
            SetState(State.GetTakeOverSettingsProcessing);
            
#if GS2_ENABLE_UNITASK
            GetTakeOverSettingsAsync().Forget();
#else
            StartCoroutine(
                GetTakeOverSettings()
            );
#endif
        }

        /// <summary>
        /// 引継ぎを選択
        /// 引継ぎ方式を選択へ
        /// Select takeover
        /// Go to Select takeover method
        /// </summary>
        public void OnClickSelectTakeOver()
        {
            SetState(State.TakeOver_SelectTypeMenu);
        }
        
        /// <summary>
        /// メールアドレスによるアカウント連携を保存/削除
        /// Save/delete account linkage by email address
        /// </summary>
        public void OnClickSettingEmail()
        {
            _accountTakeOverModel.type = (int) TakeOverType.Email;
            if (_accountTakeOverModel.GetEmailSetting() == null)
            {
                SetState(State.SettingEmail);
            }
            else
            {
                SetState(State.DeleteSettingProcessing);
                DeleteSetting();
            }
        }
        
        /// <summary>
        /// Emailの設定をキャンセル
        /// Cancel Email settings
        /// </summary>
        public void OnClickCancelSettingEmail()
        {
            SetState(State.CancelSettingEmail);
        }
        
        /// <summary>
        /// Platformによるアカウント連携を保存/削除
        /// Save/delete account linkage by Platform
        /// </summary>
        public void OnClickSettingPlatform()
        {
            _accountTakeOverModel.type = (int)TakeOverType.Platform;
            if (_accountTakeOverModel.GetPlatformSetting() == null)
            {

                if (Social.localUser.authenticated)
                {
                    _accountTakeOverModel.userIdentifier = Social.localUser.id;
                    _accountTakeOverModel.password = Social.localUser.id;
                    SetState(State.AddSettingProcessing);
#if GS2_ENABLE_UNITASK
                    AddTakeOverSettingAsync().Forget();
#else
                    StartCoroutine(
                        AddTakeOverSetting()
                    );
#endif
                }
                else
                {
#if UNITY_ANDROID
                    UIManager.Instance.OpenDialog1("Error", "Not logged in to Google Play Game Services");
#else
                    UIManager.Instance.OpenDialog1("Error", "Not logged in to Game Center");
#endif
                    SetState(State.SettingPlatformError);
                }
            }
            else
            {
                SetState(State.DeleteSettingProcessing);
                DeleteSetting();
            }
        }

        /// <summary>
        /// メールアドレスによるアカウント連携を保存
        /// Save account linkage by email address
        /// </summary>
        public void OnClickSubmitSettingEmail()
        {
            _accountTakeOverModel.userIdentifier = _view.setEmailTakeOverSettingUserIdentifier.text;
            _accountTakeOverModel.password = _view.setEmailTakeOverSettingPassword.text;
            
            SetState(State.AddSettingProcessing);
            
#if GS2_ENABLE_UNITASK
            AddTakeOverSettingAsync().Forget();
#else
            StartCoroutine(
                AddTakeOverSetting()
            );
#endif
        }

        /// <summary>
        /// 引継ぎを選択
        /// selected take over.
        /// </summary>
        public void OnClickTakeOver()
        {
            SetState(State.TakeOver_SelectTypeMenu);
        }

        /// <summary>
        /// メールアドレスによる引継ぎ　設定画面へ
        /// 
        /// </summary>
        public void OnClickTakeOverEmail()
        {
            _accountTakeOverModel.type = (int)TakeOverType.Email;
            SetState(State.TakeOverEmail);
        }

        /// <summary>
        /// Emailの設定をキャンセル
        /// Takeover setting by e-mail address Go to setting screen
        /// </summary>
        public void OnClickCancelTakeOverEmail()
        {
            SetState(State.CancelTakeOverEmail);
        }
        
        /// <summary>
        /// メールアドレスによるアカウント連携を保存/削除
        /// Save/delete account linkage by email address
        /// </summary>
        public void OnClickSubmitTakeOverEmail()
        {
            _accountTakeOverModel.type = (int)TakeOverType.Email;
            _accountTakeOverModel.userIdentifier = _view.doEmailTakeOverSettingUserIdentifier.text;
            _accountTakeOverModel.password = _view.doEmailTakeOverSettingPassword.text;
            
            SetState(State.TakeOver_Processing);
            
            ExecTakeOver();
        }
        
        /// <summary>
        /// Platformによる引継ぎ実行
        /// Takeover Execution by Platform
        /// </summary>
        public void OnClickExecTakeOverPlatform()
        {
            _accountTakeOverModel.type = (int)TakeOverType.Platform;

            SetState(State.TakeOver_Processing);
            
            ExecTakeOver();
        }

        /// <summary>
        /// 再ログイン
        /// Re-login
        /// </summary>
        public void OnClickConfirmTakeOver()
        {
            GameManager.Instance.ReLogin();
        }
        
        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// Click the Confirm Error button.
        /// </summary>
        public void OnClickConfirmError()
        {
            SetState(State.MainMenu);
        }
        
        /// <summary>
        /// 閉じる
        /// Close
        /// </summary>
        public void OnClickClose()
        {
            SetState(State.Title);
        }
    }
}