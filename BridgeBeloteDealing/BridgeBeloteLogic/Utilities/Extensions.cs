namespace BridgeBeloteLogic.Utilities
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    public static class Extensions
    {
        public static string DescriptionAttr<T>(this T source)
        {
            var fi = source.GetType().GetField(source.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : source.ToString();
        }
        public static List<T> MoveFirstItemToEndOfList<T>(this List<T> list)
        {
            var firstItem = list[0];
            list.RemoveAt(0);
            list.Insert(list.Count, firstItem);
            return list;
        }
    }
}
