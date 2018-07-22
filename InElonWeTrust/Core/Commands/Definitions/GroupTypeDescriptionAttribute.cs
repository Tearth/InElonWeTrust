using System;

namespace InElonWeTrust.Core.Commands.Definitions
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GroupTypeDescriptionAttribute : Attribute
    {
        public string Icon { get; }
        public string Group { get; }
        public string Description { get; }

        public GroupTypeDescriptionAttribute(string icon, string group, string description)
        {
            Icon = icon;
            Group = group;
            Description = description;
        }
    }
}
