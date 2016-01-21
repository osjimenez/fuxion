using Fuxion.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity
{
    public class IdentityManager
    {
        public IdentityManager(IPasswordProvider passwordProvider, IStorageProvider storageProvider) {
            PasswordProvider = passwordProvider;
            StorageProvider = storageProvider;
        }
        public IdentityManager(IPasswordProvider passwordProvider, IAggregateRepository<IIdentity> repository)
        {
            PasswordProvider = passwordProvider;
        }
        public IPasswordProvider PasswordProvider { get; private set; }
        public IStorageProvider StorageProvider { get; private set; }
        public bool ValidateCredentials(string username, string password) {
            if (username == null || password == null) return false;
            var ide = StorageProvider.Identities.FirstOrDefault(i => i.UserName == username);
            return PasswordProvider.Verify(password, ide.PasswordHash, ide.PasswordSalt);
        }
        public string GetAsCacheValue()
        {
            return null;
        }
    }
    public interface IPasswordProvider
    {
        bool Verify(string password, byte[] hash, byte[] salt);
        void Generate(string password, out byte[] salt, out byte[] hash);
    }
    public interface IStorageProvider
    {
        IQueryable<IIdentity> Identities { get; }
    }
}
