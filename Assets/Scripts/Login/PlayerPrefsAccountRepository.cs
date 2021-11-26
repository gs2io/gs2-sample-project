using UnityEngine;

namespace Gs2.Sample.Login
{
    public class PlayerPrefsAccountRepository: MonoBehaviour, IAccountRepository
    {
        public bool IsExistsAccount()
        {
            return !string.IsNullOrEmpty(PlayerPrefs.GetString("account", null));
        }

        public void SaveAccount(PersistAccount account)
        {
            PlayerPrefs.SetString("account", JsonUtility.ToJson(account));
            PlayerPrefs.Save();
        }

        public PersistAccount LoadAccount()
        {
            return JsonUtility.FromJson<PersistAccount>(PlayerPrefs.GetString("account", "{}"));
        }

        public void DeleteAccount()
        {
            PlayerPrefs.SetString("account", null);
            PlayerPrefs.Save();
        }
    }
}