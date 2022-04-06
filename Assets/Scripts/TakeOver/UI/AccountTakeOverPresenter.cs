using System.Collections;
using Gs2.Sample.Core.Runtime;
using Gs2.Sample.Login;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gs2.Sample.AccountTakeOver
{
    public class AccountTakeOverPresenter : MonoBehaviour
    {
        /// <summary>
        /// GS2-Account の設定値
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
            /// </summary>
            GetTakeOverSettingsProcessing,
            /// <summary>
            /// アカウント連携情報の取得に失敗
            /// </summary>
            GetTakeOverSettingsFailed,

            /// <summary>
            /// アカウント連携 方式メニュー
            /// </summary>
            Setting_SelectTypeMenu,
            
            /// <summary>
            /// アカウント連携 Email設定
            /// </summary>
            SettingEmail,
            /// <summary>
            /// Emailの設定をキャンセル
            /// </summary>
            CancelSettingEmail,
            
            /// <summary>
            /// Platformによるアカウント連携を保存/削除
            /// </summary>
            SettingPlatform,
            /// <summary>
            /// Platformによるアカウント連携を保存/削除に失敗
            /// </summary>
            SettingPlatformError,
            
            /// <summary>
            /// 引継ぎ設定の登録を実行中
            /// </summary>
            AddSettingProcessing,
            /// <summary>
            /// 引継ぎ設定の登録に成功
            /// </summary>
            AddSettingSucceed,
            /// <summary>
            /// 引継ぎ設定の登録に失敗
            /// </summary>
            AddSettingFailed,

            /// <summary>
            /// 引継ぎ設定の削除中
            /// </summary>
            DeleteSettingProcessing,
            /// <summary>
            /// 引継ぎ設定の削除に成功
            /// </summary>
            DeleteSettingSucceed,
            /// <summary>
            /// 引継ぎ設定の削除に失敗
            /// </summary>
            DeleteSettingFailed,
            
            /// <summary>
            /// 引継ぎ 方式メニュー
            /// </summary>
            TakeOver_SelectTypeMenu,
            
            /// <summary>
            /// 引継ぎ Email設定
            /// </summary>
            TakeOverEmail,
            /// <summary>
            /// Emailの設定をキャンセル
            /// </summary>
            CancelTakeOverEmail,
            
            /// <summary>
            /// 引継ぎ実行中
            /// </summary>
            TakeOver_Processing,
            /// <summary>
            /// 引継ぎの実行を完了
            /// </summary>
            TakeOverCompleted,
            /// <summary>
            /// 引継ぎの実行に失敗
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
                            _view.emailTakeOverButtonLabel.SetText("Email - 登録済み");
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
                            _view.platformTakeOverButtonLabel.SetText("Game Center - 登録済み");
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
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            if (!Social.localUser.authenticated)
            {
                Social.localUser.Authenticate(success => { Debug.Log("signed-in"); });
            }
        }

        /// <summary>
        /// 現在登録されている引継ぎ情報を取得
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetTakeOverSettings()
        {
            yield return _accountTakeOverModel.ListAccountTakeOverSettings(
                r =>
                {
                    SetState(r.Error == null
                        ? State.Setting_SelectTypeMenu
                        : State.GetTakeOverSettingsFailed);
                },
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onError
            );
        }
        
        /// <summary>
        /// 引継ぎ設定の登録を実行
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator AddTakeOverSetting()
        {
            yield return _accountTakeOverModel.AddAccountTakeOverSetting(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                r =>
                {
                    SetState(r.Error == null
                        ? State.AddSettingSucceed
                        : State.AddSettingFailed);
                },
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onSetTakeOver,
                _takeOverSetting.onError
            );
        }
        
        /// <summary>
        /// 引継ぎ設定を削除
        /// </summary>
        public void ExecTakeOver()
        {
            SetState(State.TakeOver_Processing);
            
            StartCoroutine(
                DoTakeOver()
            );
        }
        
        /// <summary>
        /// 引継ぎを実行
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoTakeOver()
        {
            yield return _accountTakeOverModel.DoAccountTakeOver(
                GameManager.Instance.Cllient.Client,
                r =>
                {
                    if (r.Error == null)
                    {
                        SetState(State.TakeOverCompleted);
                    }
                    else
                    {
                        SetState(State.TakeOverFailed);
                    }
                },
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onDoTakeOver,
                _takeOverSetting.onError
            );
        }
        
        /// <summary>
        /// 引継ぎ設定を削除
        /// </summary>
        public void DeleteSetting()
        {
            SetState(State.DeleteSettingProcessing);
            StartCoroutine(
                DeleteTakeOverSetting()
            );
        }
            
        /// <summary>
        /// 引継ぎ設定の削除を実行
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator DeleteTakeOverSetting()
        {
            yield return _accountTakeOverModel.DeleteAccountTakeOverSetting(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                r =>
                {
                    SetState(r.Error == null
                        ? State.DeleteSettingSucceed
                        : State.DeleteSettingFailed);
                },
                _loginSetting.accountNamespaceName,
                _takeOverSetting.onError
            );
        }

        /// <summary>
        /// メインメニューを開く
        /// </summary>
        public void OnClickMainMenu()
        {
            SetState(State.MainMenu);
        }
        
        /// <summary>
        /// アカウント連携を選択した
        /// </summary>
        public void OnClickSetting()
        {
            SetState(State.Setting_SelectTypeMenu);
        }
        
        /// <summary>
        /// アカウント連携を選択
        /// 連携方式を選択へ
        /// </summary>
        public void OnClickSettingSelectType()
        {
            SetState(State.GetTakeOverSettingsProcessing);
            StartCoroutine(
                GetTakeOverSettings()
            );
        }

        /// <summary>
        /// 引継ぎを選択
        /// 引継ぎ方式を選択へ
        /// </summary>
        public void OnClickSelectTakeOver()
        {
            SetState(State.TakeOver_SelectTypeMenu);
        }
        
        /// <summary>
        /// メールアドレスによるアカウント連携を保存/削除
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
        /// </summary>
        public void OnClickCancelSettingEmail()
        {
            SetState(State.CancelSettingEmail);
        }
        
        /// <summary>
        /// Platformによるアカウント連携を保存/削除
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
                    StartCoroutine(
                        AddTakeOverSetting()
                    );
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
        /// </summary>
        public void OnClickSubmitSettingEmail()
        {
            _accountTakeOverModel.userIdentifier = _view.setEmailTakeOverSettingUserIdentifier.text;
            _accountTakeOverModel.password = _view.setEmailTakeOverSettingPassword.text;
            
            SetState(State.AddSettingProcessing);
            
            StartCoroutine(
                AddTakeOverSetting()
            );
        }

        /// <summary>
        /// 引継ぎを選択した
        /// </summary>
        public void OnClickTakeOver()
        {
            SetState(State.TakeOver_SelectTypeMenu);
        }

        /// <summary>
        /// メールアドレスによる引継ぎ　設定画面へ
        /// </summary>
        public void OnClickTakeOverEmail()
        {
            _accountTakeOverModel.type = (int)TakeOverType.Email;
            SetState(State.TakeOverEmail);
        }

        /// <summary>
        /// Emailの設定をキャンセル
        /// </summary>
        public void OnClickCancelTakeOverEmail()
        {
            SetState(State.CancelTakeOverEmail);
        }
        
        /// <summary>
        /// メールアドレスによるアカウント連携を保存/削除
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
        /// </summary>
        public void OnClickExecTakeOverPlatform()
        {
            _accountTakeOverModel.type = (int)TakeOverType.Platform;

            SetState(State.TakeOver_Processing);
            
            ExecTakeOver();
        }

        /// <summary>
        /// 再ログイン
        /// </summary>
        public void OnClickConfirmTakeOver()
        {
            GameManager.Instance.ReLogin();
        }
        
        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void OnClickConfirmError()
        {
            SetState(State.MainMenu);
        }
        
        /// <summary>
        /// 閉じる
        /// </summary>
        public void OnClickClose()
        {
            SetState(State.Title);
        }
    }
}