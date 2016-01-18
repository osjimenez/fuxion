using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace Fuxion.ServiceModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class MefServiceHostAttribute : MefServiceAttribute
	{
		internal protected MefServiceHost Host { get; internal set; }
		public Type ResourceType { get; set; }
		public string ServerCertificateResourceName { get; set; }
		public string ServerCertificatePassword { get; set; }
		public UserNamePasswordValidationMode UserNamePasswordValidationMode { get; set; }
		public string CustomUserNamePasswordValidatorMefContractName { get; set; }
		public InstanceContextMode InstanceContextMode { get; set; }
		public bool IncludeExceptionDetailInFaults { get; set; }

		protected internal virtual TimeSpan DefaultCloseTimeout { get; set; }
		protected internal virtual TimeSpan DefaultOpenTimeout { get; set; }

		protected internal virtual ReadOnlyCollection<ServiceEndpoint> OnAddDefaultEndpoints(Func<ReadOnlyCollection<ServiceEndpoint>> baseFunc) { return baseFunc.Invoke(); }
		protected internal virtual void OnAddServiceEndpoint(Action<ServiceEndpoint> baseAction, ServiceEndpoint endpoint) { baseAction.Invoke(endpoint); }
		protected internal virtual void OnInitializeRuntime(Action baseAction)
		{
			if (ResourceType == null && ServerCertificateResourceName != null) throw new ArgumentException("Si especifica el parámetro 'ServerCertificateResourceName' debe especificar también 'ResourceType'.", "ResourceType");
			if (ResourceType != null)
			{
				var resMan = new ResourceManager(ResourceType);
				if (!string.IsNullOrWhiteSpace(ServerCertificateResourceName))
				{
					var obj = resMan.GetObject(ServerCertificateResourceName);
					var cert = new X509Certificate2((byte[])obj, ServerCertificatePassword);
					Host.Credentials.ServiceCertificate.Certificate = cert;
				}
			}
			Host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode;
			if (!string.IsNullOrWhiteSpace(CustomUserNamePasswordValidatorMefContractName))
				Host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = Container.GetExportedValue<UserNamePasswordValidator>(CustomUserNamePasswordValidatorMefContractName);
			baseAction.Invoke();
		}
	}
}
