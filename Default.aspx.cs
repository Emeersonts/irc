using System;
using System.Reflection;
using BackOffice.Authorizer.Management.API.App_Start;

namespace BackOffice.Authorizer.Management.API
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public string GetVersion()
        {
            return ApiInfo.GetVersion();
        }

        public string GetApplicationDescription()
        {
            return $"{ApiInfo.GetTitle()} v{GetVersion()} @ Grupo InterPlayers {DateTime.Now.Year}";
        }

        public string GetTitle()
        {
            return ApiInfo.GetTitle();
        }
    }
}