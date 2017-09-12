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
        //public event EventHandler<EventArgs<IIdentity>> CurrentChanged;
        //public void RaiseCurrentChanged() { CurrentChanged?.Invoke(this, new EventArgs<IIdentity>(GetCurrent())); }
        public ICurrentUserNameProvider CurrentUserNameProvider { get; private set; }
        public IPasswordProvider PasswordProvider { get; private set; }
        public IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity> Repository { get; private set; }
        public bool CheckCredentials(string username, string password)
        {
            Printer.WriteLine($"Validando credenciales\r\n   Usuario: {username}\r\n   Contraseña: {password}\r\n");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Printer.WriteLine($"Resultado: NO VALIDO - El nombre de usuario o la contraseña es NULL");
                return false;
            }
            var ide = Repository.Find(username);
            if (ide == null)
            {
                Printer.WriteLine($"Resultado: NO VALIDO - No se ha encontrado una identidad con ese nombre de usuario");
                return false;
            }
            var res = PasswordProvider.Verify(password, ide.PasswordHash, ide.PasswordSalt);
            if (res)
            {
                Printer.WriteLine($"Resultado: VALIDO");
            }
            return res;
        }
        //public void Logout()
        //{
        //    Current = null;
        //}
    }
}
