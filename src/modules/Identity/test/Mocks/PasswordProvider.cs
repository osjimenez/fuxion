using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    class PasswordProvider : IPasswordProvider
    {
        public void Generate(string password, out byte[] salt, out byte[] hash)
        {
            salt = new byte[] { 0x00 };
            hash = new byte[] { 0x00 };
        }
        public bool Verify(string password, byte[] hash, byte[] salt)
        {
            if (password == "root") return true;
            return false;
        }
    }
}
