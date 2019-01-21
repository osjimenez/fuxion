using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks

{
    public class PasswordProviderMock : IPasswordProvider
    {
        public void Generate(string password, out byte[] salt, out byte[] hash)
        {
            salt = new byte[] { };
            hash = Encoding.Default.GetBytes(password);
        }
        public bool Verify(string password, byte[] hash, byte[] salt)
        {
            return Encoding.Default.GetString(hash) == password;
        }
    }
}
