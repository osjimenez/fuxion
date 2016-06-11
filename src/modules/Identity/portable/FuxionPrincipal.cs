using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity
{
    // TODO - Oscar - http://geeks.ms/blogs/etomas/archive/2014/12/19/securizando-tus-servicios-webapi-usando-owin.aspx
    [FactoryDefaultImplementation(typeof(StaticPrincipalProvider))]
    public interface IPrincipalProvider
    {
        void SetPrincipal(IPrincipal principal);
        IPrincipal GetPrincipal();
    }
    public class StaticPrincipalProvider : IPrincipalProvider
    {
        static IPrincipal staticPrincipal;
        public IPrincipal GetPrincipal()
        {
            return staticPrincipal;
        }
        public void SetPrincipal(IPrincipal principal)
        {
            staticPrincipal = principal;
        }
    }
    public class FuxionPrincipal : IPrincipal
    {
        public FuxionPrincipal(IIdentity identity)
        {

        }
        public System.Security.Principal.IIdentity Identity { get; private set; }

        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }
    }
    public class FuxionIdentity : System.Security.Principal.IIdentity, IIdentity
    {
        public string AuthenticationType { get { return "Fuxion"; } }

        public IEnumerable<IGroup> Groups
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte[] PasswordHash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte[] PasswordSalt
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IPermission> Permissions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
