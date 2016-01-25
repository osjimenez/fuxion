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
        public Action<string, bool> Console { get; set; }
        public IPasswordProvider PasswordProvider { get; private set; }
        public IAggregateRepository<IIdentity, string> Repository { get; private set; }
        private void WriteConsole(string message, bool endOfMessage) { if (Console != null) Console(message, endOfMessage); }
        public async Task<bool> ValidateCredentials(string username, string password)
        {
            WriteConsole($"Validando credenciales\r\n   Usuario: {username}\r\n   Contraseña: {password}\r\n", false);
            if (username == null || password == null)
            {
                WriteConsole($"Resultado: NO VALIDO - El nombre de usuario o la contraseña es NULL", true);
                return false;
            }
            var ide = await Repository.FindAsync(username);
            if (ide == null)
            {
                WriteConsole($"Resultado: NO VALIDO - No se ha encontrado una identidad con ese nombre de usuario", true);
                return false;
            }
            var res = PasswordProvider.Verify(password, ide.PasswordHash, ide.PasswordSalt);
            WriteConsole($"Resultado: VALIDO", true);
            return res;
        }
        public async Task<bool> CheckFunctionAssignedAsync(string username, IFunctionGraph functionGraph, IFunction function, params IDiscriminator[] discriminators)
        {
            WriteConsole($"Comprobando asignación de funciones\r\n   Usuario: {username}\r\n   Funcion: {function.Name}\r\n   Discriminadores: {discriminators.Aggregate("", (a, c) => $"{a}      {c.Name}\r\n", a => a.Trim('\r', '\n'))}", true);
            if (username == null)
            {
                WriteConsole($"Resultado: NO VALIDO - El nombre de usuario es NULL", true);
                return false;
            }
            var ide = await Repository.FindAsync(username);
            if (ide == null)
            {
                WriteConsole($"Resultado: NO VALIDO - No se ha encontrado una identidad con ese nombre de usuario", true);
                return false;
            }
            ide.CheckFunctionAssigned(functionGraph, function, discriminators, Console);
            return true;
        }
        public bool IsFunctionAssigned(string username, IFunctionGraph functionGraph, IFunction function, params IDiscriminator[] discriminators)
        {
            WriteConsole($"Comprobando asignación de funciones\r\n   Usuario: {username}\r\n   Funcion: {function.Name}\r\n   Discriminadores:\r\n{discriminators.Aggregate("", (a, c) => $"{a}      {c.Name}\r\n", a => a.Trim('\r', '\n'))}", true);
            if (username == null)
            {
                WriteConsole($"Resultado: NO VALIDO - El nombre de usuario es NULL", true);
                return false;
            }
            var ide = Repository.Find(username);
            if (ide == null)
            {
                WriteConsole($"Resultado: NO VALIDO - No se ha encontrado una identidad con ese nombre de usuario", true);
                return false;
            }
            IPermission den;
            var res = ide.IsFunctionAssigned(functionGraph, function, discriminators, Console, out den);
            return res;
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
