using Fuxion.Repositories;

namespace Fuxion.Identity;

public class IdentityManager
{
	public IdentityManager(IPasswordProvider                      passwordProvider, ICurrentUserNameProvider currentUserNameProvider,
								  IKeyValueRepository<string, IIdentity> repository) //, IPrincipalProvider principalProvider)
	{
		PasswordProvider        = passwordProvider;
		CurrentUserNameProvider = currentUserNameProvider;
		Repository              = repository;
	}
	public ICurrentUserNameProvider               CurrentUserNameProvider { get; }
	public IPasswordProvider                      PasswordProvider        { get; }
	public IKeyValueRepository<string, IIdentity> Repository              { get; }
	public IIdentity?                             GetCurrent()            => Repository.Find(CurrentUserNameProvider.GetCurrentUserName());
	public bool CheckCredentials(string username, string password)
	{
		Printer.WriteLine($"Validando credenciales\r\n   Usuario: {username}\r\n   Contraseña: {password}\r\n");
		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
		{
			Printer.WriteLine("Resultado: NO VALIDO - El nombre de usuario o la contraseña es NULL");
			return false;
		}
		var ide = Repository.Find(username);
		if (ide == null)
		{
			Printer.WriteLine("Resultado: NO VALIDO - No se ha encontrado una identidad con ese nombre de usuario");
			return false;
		}
		var res = PasswordProvider.Verify(password, ide.PasswordHash, ide.PasswordSalt);
		if (res) Printer.WriteLine("Resultado: VALIDO");
		return res;
	}
}