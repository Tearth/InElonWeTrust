using System;
using System.Linq;
using System.Reflection;

namespace InElonWeTrust.Core.Helpers.Extensions
{
    public static class EnumExtensions
    {
        public static T GetEnumMemberAttribute<T>(this Enum enumObject, object enumValue)
        {
            var enumType = enumObject.GetType().GetTypeInfo();

            var memberInfo = enumType.GetDeclaredField(enumValue.ToString());
            var enumMemberAttribute = memberInfo.GetCustomAttributes(false).OfType<T>().FirstOrDefault();

            return enumMemberAttribute;
        }
    }
}
