namespace Fuxion.Shell;

using Microsoft.Extensions.DependencyInjection;

public interface IModule
{
	void PreRegister(IServiceCollection services) { }
	void Register(IServiceCollection services);
	void Initialize(IServiceProvider serviceProvider) { }
}