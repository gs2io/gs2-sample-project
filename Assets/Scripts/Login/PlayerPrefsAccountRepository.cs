using Gs2.Gs2Formation.Model;
using UnityEngine;

namespace Gs2.Sample.Login
{
    public class PlayerPrefsAccountRepository: MonoBehaviour, IAccountRepository
    {
        private const string AccountSaveName = "account";
        
        public bool IsExistsAccount()
        {
            return !string.IsNullOrEmpty(PlayerPrefs.GetString(AccountSaveName, null));
        }
        
        public bool IsExistsAccount(int slot)
        {
            if (slot == 0)
                return IsExistsAccount();
            else
                return !string.IsNullOrEmpty(PlayerPrefs.GetString(AccountSaveName + slot.ToString(), null));
        }

        public void SaveAccount(PersistAccount account)
        {
            PlayerPrefs.SetString(AccountSaveName, JsonUtility.ToJson(account));
            PlayerPrefs.Save();
        }
        
        public void SaveAccount(PersistAccount account, int slot)
        {
            if (slot == 0)
                SaveAccount(account);
            else
            {
                PlayerPrefs.SetString(AccountSaveName + slot.ToString(), JsonUtility.ToJson(account));
                PlayerPrefs.Save();
            }
        }

        public PersistAccount LoadAccount()
        {
            return JsonUtility.FromJson<PersistAccount>(PlayerPrefs.GetString(AccountSaveName, "{}"));
        }

        public PersistAccount LoadAccount(int slot)
        {
            if (slot == 0)
                return LoadAccount();
            else
                return JsonUtility.FromJson<PersistAccount>(PlayerPrefs.GetString(AccountSaveName+slot.ToString(), "{}"));
        }
        
        public void DeleteAccount()
        {
            PlayerPrefs.SetString(AccountSaveName, null);
            PlayerPrefs.Save();
        }
        
        public void DeleteAccount(int slot)
        {
            if (slot == 0)
                DeleteAccount();
            else
            {
                PlayerPrefs.SetString(AccountSaveName + slot.ToString(), null);
                PlayerPrefs.Save();
            }
        }
    }
}