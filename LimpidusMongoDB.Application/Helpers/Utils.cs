using LimpidusMongoDB.Application.CustomAttributes;
using System.ComponentModel;

namespace LimpidusMongoDB.Application.Helpers
{
    public static partial class Utils
    {
        public static string FirstCharToLowerCase(this string @string)
            => char.ToLowerInvariant(@string[0]) + @string[1..];

        public static string GetCollectionName<TEntity>() where TEntity : class
        {
            var attribute = (CollectionNameAttribute)Attribute.GetCustomAttribute(typeof(TEntity), typeof(CollectionNameAttribute))!;
            return attribute?.CollectionName ?? string.Empty;
        }

        public static string Description(this Enum value)
        {
            DescriptionAttribute[] array = (DescriptionAttribute[])value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
            return (array != null && array.Length != 0) ? array[0].Description : value.ToString();
        }
    }
}