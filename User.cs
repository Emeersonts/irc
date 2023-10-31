using System;
using System.Collections.Generic;
using System.Linq;

namespace BackOffice.Authorizer.Management.Domains
{
    public class User
    {
        private HashSet<Acquirer> acquirers = new HashSet<Acquirer>();
        private HashSet<UserRole> roles = new HashSet<UserRole>();

        public UserKey UserKey { get; set; }
        public string Name { get; private set; }
        public string Login { get; private set; }
        public Password Password { get; set; }
        public bool Active { get; set; }
        public Guid ExternalKey { get; set; }
        public Acquirer[] Acquirers { get { return acquirers.ToArray(); } }
        public UserRole[] Roles { get { return roles.ToArray(); } }

        public User(string login, string name, Password password)
        {
            Login = login;
            Name = name;
            roles = new HashSet<UserRole>();
            acquirers = new HashSet<Acquirer>();
            Password = password;
        }

        public User(string login, string name, Acquirer[] acquirers)
        {
            Login = login;
            Name = name;
            roles = new HashSet<UserRole>();
            if(acquirers != null)
            {
                this.acquirers = new HashSet<Acquirer>(acquirers);
            }
        }

        public void AddRole(UserRole role)
        {
            roles.Add(role);
        }

        public void AddRoles(UserRole[] roles)
        {
            foreach (var role in roles)
            {
                this.roles.Add(role);
            }
        }

        public void AddAcquirers(Acquirer[] acquirers)
        {
            foreach (var acquirer in acquirers)
            {
                this.acquirers.Add(acquirer);
            }
        }

        public void AddAcquirer(Acquirer acquirer)
        {
            acquirers.Add(acquirer);
        }
    }
}
