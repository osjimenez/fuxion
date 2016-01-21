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
        public IdentityManager(IPasswordProvider passwordProvider, IAggregateRepository<IIdentity, string> repository)
        {
            PasswordProvider = passwordProvider;
            Repository = repository;
        }
        public IPasswordProvider PasswordProvider { get; private set; }
        public IAggregateRepository<IIdentity, string> Repository { get; private set; }
        public async Task<bool> ValidateCredentials(string username, string password)
        {
            if (username == null || password == null) return false;
            var ide = await Repository.FindAsync(username);
            if (ide == null) return false;
            return PasswordProvider.Verify(password, ide.PasswordHash, ide.PasswordSalt);
        }
        public async Task<bool> CheckFunctionAssigned(string username, IFunction function, params IDiscriminator[] discriminators)
        {
            if (username == null) return false;
            var ide = await Repository.FindAsync(username);
            if (ide == null) return false;
            ide.CheckFunctionAssigned(new FunctionGraph(), function, discriminators);
            return true;
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
