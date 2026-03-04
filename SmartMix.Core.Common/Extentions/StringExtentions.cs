using System.Text.RegularExpressions;

namespace SmartMix.Core.Common.Extentions
{
    public static class StringExtentions
    {
        public static bool Match(this String s, string regex)
        {
            var re = new Regex(regex);
            return re.Match(s).Success;
        }
    }
}
