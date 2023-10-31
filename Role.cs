using System;
using System.Collections.Generic;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Role
    {
       

        public RoleKey RoleKey { get; set; }
        public string Name { get; private set; }

        private Role(RoleKey roleKey)
        {
            RoleKey = roleKey;
        }

        public Role(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool Equals(object obj)
        {
            return obj is Role role &&
                   EqualityComparer<RoleKey>.Default.Equals(RoleKey, role.RoleKey) &&
                   Name == role.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = 1149865270;
            hashCode = hashCode * -1521134295 + EqualityComparer<RoleKey>.Default.GetHashCode(RoleKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
