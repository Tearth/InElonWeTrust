using System;
using InElonWeTrust.Core.Commands.Definitions;

namespace InElonWeTrust.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandsGroupAttribute : Attribute
    {
        public GroupType GroupType { get; }

        public CommandsGroupAttribute(GroupType groupType)
        {
            GroupType = groupType;
        }
    }
}
