using Microsoft.Extensions.DependencyInjection;

namespace Fuxion.Telerik_.Wpf;

public interface IModule
{
	void PreRegister(IServiceCollection services) { }
	void Register(IServiceCollection services);
	void Initialize(IServiceProvider serviceProvider) { }
}