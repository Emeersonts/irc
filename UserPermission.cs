using System;
using System.Collections.Generic;

namespace BackOffice.Authorizer.Management.Domains
{
    public class UserPermission
    {
        public UserPermissionKey UserPermissionKey { get; set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public RoleKey RoleKey { get; set; }

        public UserPermission(string code, string name)
        {
            this.Code = code;
            this.Name = name;
        }

        public override bool Equals(object obj)
        {
            return obj is UserPermission permission &&
                   EqualityComparer<UserPermissionKey>.Default.Equals(UserPermissionKey, permission.UserPermissionKey) &&
                   Code == permission.Code &&
                   Name == permission.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = -240396122;
            hashCode = hashCode * -1521134295 + EqualityComparer<UserPermissionKey>.Default.GetHashCode(UserPermissionKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Code);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
