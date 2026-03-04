namespace SmartMix.Core.Common.Extentions
{
    public static class ReflectionExtensions
    {
        private const string Number = nameof(Number);

        public static int GetPopertyValue(this object mechanic, string propName)
        {
            var prop = mechanic.GetType().GetProperties().FirstOrDefault(p => p.Name == propName);
            if (prop != null)
            {
                object value = prop.GetValue(mechanic, null);
                if (value is int result)
                    return result;
                else if (value != null)
                    return value.GetPopertyValue(Number);
            }
            return 0;
        }
    }
}
