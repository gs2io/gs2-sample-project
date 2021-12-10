
namespace Gs2.Sample.Login
{
    public interface IAccountRepository
    {
        bool IsExistsAccount();
        
        bool IsExistsAccount(int slot);
        
        void SaveAccount(PersistAccount account);

        void SaveAccount(PersistAccount account, int slot);
        
        PersistAccount LoadAccount();

        PersistAccount LoadAccount(int slot);
        
        void DeleteAccount();
        
        void DeleteAccount(int slot);

    }
}