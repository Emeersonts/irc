using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Utilities
{
    public interface ICryptographyStrategy 
    { 
        string Encrypt(string clearText); 
        string Decrypt(string encryptedText); 
    }
}
