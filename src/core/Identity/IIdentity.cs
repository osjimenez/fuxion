namespace Fuxion.Identity;

public interface IIdentity : IRol
{
	object Id           { get; }
	byte[] PasswordHash { get; }
	byte[] PasswordSalt { get; }
}

public interface IIdentity<TId> : IIdentity
{
	new TId Id { get; }
}