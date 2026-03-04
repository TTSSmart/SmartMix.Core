using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SmartMix.Core.Common.ValidationRules
{
    /// <summary>
    /// Validates a text against a regular expression
    /// </summary>
    public class RegexValidationRule : ValidationRule
    {
        private string _pattern;
        private Regex _regex;

        public string ErrorMessage { get; set; }

        public string Pattern
        {
            get { return _pattern; }
            set
            {
                _pattern = value;
                _regex = new Regex(_pattern, RegexOptions.IgnoreCase);
            }
        }

        public RegexValidationRule()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || !_regex.Match(value.ToString()).Success)
            {
                return new ValidationResult(false, ErrorMessage);
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
