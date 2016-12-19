using Fuxion.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity
{
    public class IdentityKeyValueRepositoryValue : IKeyValueEntry<string, IIdentity>
    {
        public string Key { get; set; }
        public IIdentity Value { get; set; }
    }
    //public class IdentityManagerRepository
    //{
    //    public IdentityManagerRepository(
    //        IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity> identityRepository
    //        )
    //    {
    //        this.identityRepository = identityRepository;
    //    }
    //    IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity> identityRepository;
    //    public IIdentity Find(string username) { return identityRepository.Find(username); }
    //    public IEnumerable<IRol> GetRols() { return identityRepository.Find(username); }
    //}
    public class IdentityManager
    {
        public IdentityManager(IPasswordProvider passwordProvider, ICurrentUserNameProvider currentUserNameProvider, IKeyValueRepository<IdentityKeyValueRepositoryValue,string, IIdentity> repository)//, IPrincipalProvider principalProvider)
        {
            PasswordProvider = passwordProvider;
            CurrentUserNameProvider = currentUserNameProvider;
            Repository = repository;
        }
        public IIdentity GetCurrent() => Repository.Find(CurrentUserNameProvider.GetCurrentUserName());
        public event EventHandler<EventArgs<IIdentity>> CurrentChanged;
        public void RaiseCurrentChanged() { CurrentChanged?.Invoke(this, new EventArgs<IIdentity>(GetCurrent())); }
        public ICurrentUserNameProvider CurrentUserNameProvider { get; private set; }
        public IPasswordProvider PasswordProvider { get; private set; }
        public IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity> Repository { get; private set; }
        public bool Login(string username, string password)
        {
            Printer.Print($"Validando credenciales\r\n   Usuario: {username}\r\n   Contraseña: {password}\r\n");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Printer.Print($"Resultado: NO VALIDO - El nombre de usuario o la contraseña es NULL");
                return false;
            }
            var ide = Repository.Find(username);
            if (ide == null)
            {
                Printer.Print($"Resultado: NO VALIDO - No se ha encontrado una identidad con ese nombre de usuario");
                return false;
            }
            var res = PasswordProvider.Verify(password, ide.PasswordHash, ide.PasswordSalt);
            if (res)
            {
                Printer.Print($"Resultado: VALIDO");
            }
            return res;
        }
        //public void Logout()
        //{
        //    Current = null;
        //}
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
    public interface ICurrentUserNameProvider
    {
        string GetCurrentUserName();
    }
    public class GenericCurrentUserNameProvider : ICurrentUserNameProvider
    {
        public GenericCurrentUserNameProvider(Func<string> getCurrentUsernameFunction) { this.getCurrentUsernameFunction = getCurrentUsernameFunction; }
        Func<string> getCurrentUsernameFunction;
        public string GetCurrentUserName() { return getCurrentUsernameFunction(); }
    }
}
