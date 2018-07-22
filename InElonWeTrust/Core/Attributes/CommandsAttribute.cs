using System;
using InElonWeTrust.Core.Commands;
using InElonWeTrust.Core.Commands.Definitions;

namespace InElonWeTrust.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandsAttribute : Attribute
    {
        public GroupType GroupType { get; }

        public CommandsAttribute(GroupType groupType)
        {
            GroupType = groupType;
        }
    }
}
