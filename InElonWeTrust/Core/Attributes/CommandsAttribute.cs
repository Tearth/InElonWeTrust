using System;

namespace InElonWeTrust.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandsAttribute : Attribute
    {
        public string Group { get; }

        public CommandsAttribute(string group)
        {
            Group = group;
        }
    }
}
