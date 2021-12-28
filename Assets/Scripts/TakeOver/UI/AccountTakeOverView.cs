using TMPro;
using UnityEngine;

namespace Gs2.Sample.AccountTakeOver
{
    public class AccountTakeOverView : MonoBehaviour
    {
        /// <summary>
        /// 連携/引継ぎ 選択
        /// </summary>
        public GameObject mainMenu;

        /// <summary>
        /// 引継ぎ方式選択
        /// </summary>
        public GameObject settingSelectType;

        /// <summary>
        /// Email連携
        /// </summary>
        public GameObject settingEmail;

        /// <summary>
        /// 引継ぎ方式選択
        /// </summary>
        public GameObject takeOverSelectType;
        
        /// <summary>
        /// Email引継ぎ
        /// </summary>
        public GameObject takeOverEmail;
        
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI emailTakeOverButtonLabel;

        [SerializeField]
        public TMP_InputField setEmailTakeOverSettingUserIdentifier;

        [SerializeField]
        public TMP_InputField setEmailTakeOverSettingPassword;

        [SerializeField]
        public TMP_InputField doEmailTakeOverSettingUserIdentifier;

        [SerializeField]
        public TMP_InputField doEmailTakeOverSettingPassword;

        [SerializeField]
        public TextMeshProUGUI platformTakeOverButtonLabel;
        
        private void Start()
        {
            OnCloseMainMenuEvent();
            OnCloseSettingTypeEvent();
            OnCloseSettingEmailEvent();
            OnCloseTakeOverTypeEvent();
            OnCloseTakeOverEmailEvent();
        }
        
        public void OnCloseEvent()
        {
            OnCloseMainMenuEvent();
            OnCloseSettingTypeEvent();
            OnCloseSettingEmailEvent();
            OnCloseTakeOverTypeEvent();
        }

        public void OnOpenMainMenuEvent()
        {
            mainMenu.SetActive(true);
        }
        
        public void OnCloseMainMenuEvent()
        {
            mainMenu.SetActive(false);
        }

        public void OnOpenSettingTypeEvent()
        {
            settingSelectType.SetActive(true);
        }
        
        public void OnCloseSettingTypeEvent()
        {
            settingSelectType.SetActive(false);
        }

        public void OnOpenSettingEmailEvent()
        {
            settingEmail.SetActive(true);
        }
        
        public void OnCloseSettingEmailEvent()
        {
            settingEmail.SetActive(false);
        }
        
        public void OnOpenTakeOverTypeEvent()
        {
            takeOverSelectType.SetActive(true);
        }
        
        public void OnCloseTakeOverTypeEvent()
        {
            takeOverSelectType.SetActive(false);
        }
        
        public void OnOpenTakeOverEmailEvent()
        {
            takeOverEmail.SetActive(true);
        }
        
        public void OnCloseTakeOverEmailEvent()
        {
            takeOverEmail.SetActive(false);
        }
    }
}