using IDP.Authorizer;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Autorizer : IAuthorizer
    {
        public static Autorizer Logix()
        {
            return new Autorizer
            {
                logix = true
            };
        }

        public static Autorizer LogixProd()
        {
            return new Autorizer
            {
                logix = true
            };
        }

        public static Autorizer Vms()
        {
            return new Autorizer
            {
                vms = true
            };
        }

        public static Autorizer Btp()
        {
            return new Autorizer
            {
                btp = true
            };
        }

        private bool logix;
        private bool vms;
        private bool btp;

        private Autorizer()
        {
        }

        public bool IsBtp()
        {
            return btp;
        }

        public bool IsLogix()
        {
            return logix;
        }

        public bool IsVms()
        {
            return vms;
        }
    }
}
