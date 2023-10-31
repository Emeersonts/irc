using MongoDB.Driver.Core.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Option
    {
        private readonly List<Property> properties = new List<Property>();
        private readonly List<Option> subOptions = new List<Option>();

        public OptionKey Key { get; private set; }
        public string Name { get; private set; }
        public OptionKey ParentId { get; private set;}
        public Property[] Properties { get { return properties.ToArray(); } }
        public Option[] SubOptions { get { return subOptions.ToArray(); } }

        public Option(OptionKey key, Property[] properties, Option[] subOptions = null, string name = null, OptionKey parentId = null)
        {
            this.Key = key;
            this.Name = name;
            this.ParentId = parentId;

            this.properties = properties.ToList();
            if (subOptions != null)
            {
                this.subOptions = subOptions.ToList();
            }
        }

        public void AddProperty(Property property)
        {
            this.properties.Add(property);
        }
        public void AddProperties(Property[] properties)
        {
            this.properties.AddRange(properties);
        }

        public void AddSubOption(Option option)
        {
            this.subOptions.Add(option);
        }

        public void AddSubOptions(Option[] subOptions)
        {
            if(subOptions != null)
            {
                this.subOptions.AddRange(subOptions);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Option option &&
                   EqualityComparer<OptionKey>.Default.Equals(Key, option.Key) &&
                   Name == option.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = 1256694258;
            hashCode = hashCode * -1521134295 + EqualityComparer<OptionKey>.Default.GetHashCode(Key);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
