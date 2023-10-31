using System;
using System.Collections.Generic;
using System.Linq;

namespace BackOffice.Authorizer.Management.Domains
{
    public class UserRole
    {
        private HashSet<UserPermission> permissions;

        public UserRoleKey UserRoleKey { get; private set; }
        public string Name { get; private set; }
        public UserRoleType UserRoleType { get; private set; }
        public UserPermission[] UserPermissions { get { return permissions.ToArray(); } }

        public UserRole(UserRoleKey userRoleKey, string name, UserRoleType userRoleType) {
            this.UserRoleKey = userRoleKey;
            this.Name = name;
            this.UserRoleType = userRoleType;
            this.permissions = new HashSet<UserPermission>();
        }

        public void AddPermission(UserPermission permission)
        {
            this.permissions.Add(permission);
        }

        public void AddPermissions(UserPermission[] userPermissions)
        {
            foreach (var userPermission in userPermissions)
            {
                permissions.Add(userPermission);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is UserRole role &&
                   EqualityComparer<UserRoleKey>.Default.Equals(UserRoleKey, role.UserRoleKey) &&
                   Name == role.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = 387724615;
            hashCode = hashCode * -1521134295 + EqualityComparer<UserRoleKey>.Default.GetHashCode(UserRoleKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
