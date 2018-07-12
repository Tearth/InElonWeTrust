using System;

namespace InElonWeTrust.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandsAttribute : Attribute
    {
        public string Icon { get; }
        public string Group { get; }
        public string Description { get; }

        public CommandsAttribute(string icon, string group, string description)
        {
            Icon = icon;
            Group = group;
            Description = description;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CommandsAttribute item))
            {
                return false;
            }

            return Icon == item.Icon &&
                   Group == item.Group &&
                   Description == item.Description;
        }

        public override int GetHashCode()
        {
            return Icon.GetHashCode() + Group.GetHashCode() + Description.GetHashCode();
        }
    }
}
