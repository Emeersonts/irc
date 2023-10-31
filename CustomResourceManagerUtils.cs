using System.Resources;

namespace BackOffice.Authorizer.Management.I18N
{
    public static class CustomResourceManagerUtils
    {
        public static ResourceManager GetResourceManager()
        {
            return new ResourceManager("BackOffice.Authorizer.Management.I18N.Messages", typeof(CustomResourceManagerUtils).Assembly);
        }
    }
}