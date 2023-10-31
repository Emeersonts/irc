using System.Collections.Generic;

namespace BackOffice.Authorizer.Management.Domains
{
    public class ValidationHelper
    {
        public bool Valid { get; set; }
        public List<string> Errors { get; set; }

        public ValidationHelper()
        {
            this.Errors = new List<string>();
        }

        public string GetErrors()
        {
            if(this.Errors != null)
            {
                var errorList = string.Empty;
                
                foreach(var error in Errors)
                {
                    errorList += error + "\n";
                }

                return errorList;
            }

            return string.Empty;
        }
    }
}
