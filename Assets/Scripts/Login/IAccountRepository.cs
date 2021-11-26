
namespace Gs2.Sample.Login
{
    public interface IAccountRepository
    {
        bool IsExistsAccount();

        void SaveAccount(PersistAccount account);

        PersistAccount LoadAccount();

        void DeleteAccount();
    }
}