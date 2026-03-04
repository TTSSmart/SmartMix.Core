namespace SmartMix.Core.Common.Extentions
{
    public static class MimicObjectExtensions
    {
        public static T In<T, L>(this T obj, List<L> list)
        {
            list.Add((L)(object)obj);
            return obj;
        }

        public static T In<T, L, U>(this T obj, List<L> list1, List<U> list2)
        {
            list1.Add((L)(object)obj);
            list2.Add((U)(object)obj);
            return obj;
        }
    }
}
