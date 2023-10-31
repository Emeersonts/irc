using System;
using System.Collections.Generic;
using System.Linq;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Module
    {
        private readonly HashSet<Option> moduleOptions = new HashSet<Option>();
        private readonly HashSet<UserPermission> permissions = new HashSet<UserPermission>();
        
        public Color Color { get; private set; }
        public ModuleKey Key { get; set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Uri ModuleImage { get; private set; }
        public UserPermission[] Permissions { get { return permissions.ToArray(); } }
        public Option[] Options
        {
            get { return moduleOptions.ToArray(); }
        }

        public Module(ModuleKey key, string name, string description, Color color, string image)
        {
            this.Color = color;
            this.Key = key;
            this.Name = name;
            this.Description = description;
            this.ModuleImage = new Uri(image);
        }
                
        public void AddOptions(Option[] options)
        {
            this.moduleOptions.UnionWith(options);
        }
        
        public void AddPermissions(UserPermission[] permissions)
        {
            this.permissions.UnionWith(permissions);
        }

        public override bool Equals(object obj)
        {
            return obj is Module module &&
                   EqualityComparer<ModuleKey>.Default.Equals(Key, module.Key) &&
                   Name == module.Name &&
                   Description == module.Description &&
                   EqualityComparer<Uri>.Default.Equals(ModuleImage, module.ModuleImage);
        }

        public override int GetHashCode()
        {
            int hashCode = 1525024005;
            hashCode = hashCode * -1521134295 + EqualityComparer<ModuleKey>.Default.GetHashCode(Key);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + EqualityComparer<Uri>.Default.GetHashCode(ModuleImage);
            return hashCode;
        }
    }
}
