namespace Fuxion.Identity;

using Fuxion.Repositories;
public class IdentityManager
{
	public IdentityManager(IPasswordProvider passwordProvider, ICurrentUserNameProvider currentUserNameProvider, IKeyValueRepository<string, IIdentity> repository)//, IPrincipalProvider principalProvider)
	{
		PasswordProvider = passwordProvider;
		CurrentUserNameProvider = currentUserNameProvider;
		Repository = repository;
	}
	public IIdentity? GetCurrent() => Repository.Find(CurrentUserNameProvider.GetCurrentUserName());
	public ICurrentUserNameProvider CurrentUserNameProvider { get; private set; }
	public IPasswordProvider PasswordProvider { get; private set; }
	public IKeyValueRepository<string, IIdentity> Repository { get; private set; }
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
}