using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Color
    {
        public string PrimaryColor { get; private set; }
        public string SecundaryColor { get; private set; }

        public Color(string primaryColor, string secundaryColor)
        {
            this.PrimaryColor = primaryColor;
            this.SecundaryColor = secundaryColor;
        }
    }
}
